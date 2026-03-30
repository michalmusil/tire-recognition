using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenCvSharp;
using SkiaSharp;
using TireRecognition.Application.Options;
using TireRecognition.Application.Services;
using TireRecognition.Domain.Preprocessing;
using TireRecognition.Infrastructure.Extensions;
using YoloDotNet;
using YoloDotNet.Models;

namespace TireRecognition.Infrastructure.Services;

public class TireRimExtractionService : ITireRimExtractionService
{
    private const string RimClassName = "rim";

    private readonly Yolo _segmentationModel;
    private readonly ILogger<TireRimExtractionService> _logger;
    private readonly PreprocessingOptions _preprocessingOptions;

    public TireRimExtractionService(Yolo segmentationModel, ILogger<TireRimExtractionService> logger,
        IOptions<PreprocessingOptions> preprocessingOptions)
    {
        _segmentationModel = segmentationModel;
        _logger = logger;
        _preprocessingOptions = preprocessingOptions.Value;
    }

    public async Task<TireRimPosition?> DetectTireRimAsync(ImageDataHandle image)
    {
        _logger.LogInformation("Started detection of a tire rim");
        var imageByteData = image.Bytes;
        // Running through segmentation to get masks for the rim (wheel excl. the tire sidewall) and the entire wheel (incl. tire sidewall)
        using var imageToDetect = SKImage.FromBitmap(SKBitmap.Decode(imageByteData));
        var results = _segmentationModel.RunSegmentation(imageToDetect);
        var rimMaskResult = results.FirstOrDefault(res =>
            string.Equals(res.Label.Name, RimClassName, StringComparison.CurrentCultureIgnoreCase));
        if (rimMaskResult is null ||
            rimMaskResult.Confidence < _preprocessingOptions.TireRimDetectionConfidenceThreshold)
        {
            _logger.LogInformation("Tire rim detection failed: segmentation model found no rim class");
            return null;
        }

        // If rim is found, get a mask for it and get it's contour
        using var mask = ConvertSegmentationMaskToMat(
                detectedImage: imageToDetect,
                segmentationResult: rimMaskResult
            )
            .Threshold(1, 255, ThresholdTypes.Binary);
        using var invertedMask = new Mat();
        Cv2.BitwiseNot(mask, invertedMask);
        invertedMask.FindContours(out var contours, out _,
            RetrievalModes.External,
            ContourApproximationModes.ApproxSimple
        );
        var largestContour = contours.MaxBy(p => p.Length);
        if (largestContour is null)
        {
            _logger.LogInformation("Tire rim detection failed: no contour was found for the detected rim mask");
            return null;
        }

        // Once the contour is found, find an enclosing circle of the rim. The circle should be the border between rim and the tire sidewall
        Cv2.MinEnclosingCircle(largestContour, out var center, out var radius);
        var rimPosition = new TireRimPosition(CenterX: (int)center.X, CenterY: (int)center.Y, Radius: radius);
        if (!IsRimWithinImageBounds(rimPosition, image.Dimensions))
        {
            // If circle is not within original image bounds, the photo doesn't contain the entire wheel
            _logger.LogInformation("Tire rim detection failed: detected rim contour was not within image bounds");
            return null;
        }

        _logger.LogInformation("Tire rim detection succeeded");
        return rimPosition;
    }

    public ImageDataHandle ExtractSidewallRingAroundRimCircle(ImageDataHandle image, TireRimPosition rimCircle)
    {
        var inner = rimCircle.Radius * _preprocessingOptions.TireInnerRadiusRatio;
        var outer = rimCircle.Radius * _preprocessingOptions.TireOuterRadiusRatio;
        var croppedHeight = (int)(2 * Math.PI * inner);

        var cvHandle = image.ToCvDataHandle();
        using var fullPolar = new Mat();

        Cv2.WarpPolar(
            cvHandle.Data,
            fullPolar,
            new Size((int)outer, croppedHeight),
            new Point2f(rimCircle.CenterX, rimCircle.CenterY),
            outer,
            interpolationFlags: InterpolationFlags.Cubic,
            warpPolarMode: WarpPolarMode.Linear
        );

        var tireThickness = (int)(outer - inner);
        var croppedResult = new Mat(
            fullPolar,
            new Rect(
                location: new Point(X: (fullPolar.Width - tireThickness), Y: 0),
                size: new Size(width: tireThickness, height: croppedHeight)
            )
        );
        Cv2.Rotate(croppedResult, croppedResult, RotateFlags.Rotate90Counterclockwise);

        return new CvImageDataHandle(croppedResult);
    }

    private Mat ConvertSegmentationMaskToMat(SKImage detectedImage, Segmentation segmentationResult)
    {
        var height = detectedImage.Height;
        var width = detectedImage.Width;

        var rimMask = new Mat(height, width, MatType.CV_8UC1, Scalar.White);
        var bbox = segmentationResult.BoundingBox;
        var maskData = segmentationResult.BitPackedPixelMask;

        // Iterate through the bounding box only (where the mask exists)
        for (var y = 0; y < bbox.Height; y++)
        {
            for (var x = 0; x < bbox.Width; x++)
            {
                // Calculate the linear index for this pixel within the bounding box
                var pixelIndex = y * bbox.Width + x;

                // Find the specific bit in the byte array
                var byteIndex = pixelIndex >> 3; // Same as / 8
                var bitIndex = pixelIndex & 0b0111; // Same as % 8

                // Check if the bit is set to 1 (meaning it passed the confidence threshold)
                var isSegmented = (maskData[byteIndex] & (1 << bitIndex)) != 0;

                if (isSegmented)
                {
                    // Map local bounding box coordinates back to global image coordinates
                    // Set to 0 (black)
                    rimMask.Set(bbox.Top + y, bbox.Left + x, (byte)0);
                }
            }
        }

        return rimMask;
    }

    private bool IsRimWithinImageBounds(TireRimPosition rimPosition, ImageDimensions dimensions)
    {
        var circleMinX = rimPosition.CenterX - rimPosition.Radius;
        var circleMaxX = rimPosition.CenterX + rimPosition.Radius;

        var circleMinY = rimPosition.CenterY - rimPosition.Radius;
        var circleMaxY = rimPosition.CenterY + rimPosition.Radius;

        var isWithinHorizontalBounds = circleMinX > 0 && circleMaxX <= dimensions.Width;
        var isWithinVerticalBounds = circleMinY > 0 && circleMaxY <= dimensions.Height;
        return isWithinHorizontalBounds && isWithinVerticalBounds;
    }
}
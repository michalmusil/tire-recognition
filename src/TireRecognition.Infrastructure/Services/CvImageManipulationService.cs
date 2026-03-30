using OpenCvSharp;
using TireRecognition.Application.Services;
using TireRecognition.Domain.Preprocessing;
using TireRecognition.Infrastructure.Exceptions;
using TireRecognition.Infrastructure.Extensions;

namespace TireRecognition.Infrastructure.Services;

public class CvImageManipulationService : IImageManipulationService
{
    public ImageDataHandle GetImageHandleFromStream(Stream imageDataStream)
    {
        var data = Mat.FromStream(imageDataStream, ImreadModes.Color);
        return new CvImageDataHandle(data);
    }

    public void ScaleToMaxDimension(ImageDataHandle image, int maxDimension)
    {
        var cvHandle = image.ToCvDataHandle();
        var width = cvHandle.Dimensions.Width;
        var height = cvHandle.Dimensions.Height;

        var largerSide = Math.Max(width, height);
        if (largerSide <= maxDimension)
            return;

        var scaleFactor = (double)maxDimension / largerSide;
        var newWidth = (int)(width * scaleFactor);
        var newHeight = (int)(height * scaleFactor);

        Cv2.Resize(cvHandle.Data, cvHandle.Data, new Size(newWidth, newHeight));
    }


    public void ApplyClahe(ImageDataHandle image)
    {
        var cvHandle = image.ToCvDataHandle();
        Cv2.CvtColor(cvHandle.Data, cvHandle.Data, ColorConversionCodes.BGR2GRAY);
        using var clahe = Cv2.CreateCLAHE(40, new Size(5, 5));

        clahe.Apply(cvHandle.Data, cvHandle.Data);
    }

    public void ApplyBilateralFilter(ImageDataHandle image)
    {
        var cvHandle = image.ToCvDataHandle();
        Cv2.BilateralFilter(cvHandle.Data, cvHandle.Data, 5, 40, 40);
    }

    public void ApplyBitwiseNot(ImageDataHandle image)
    {
        var cvHandle = image.ToCvDataHandle();
        Cv2.BitwiseNot(cvHandle.Data, cvHandle.Data);
    }

    public void ProlongImageHorizontally(ImageDataHandle image, double prolongWidthRatio)
    {
        var cvHandle = image.ToCvDataHandle();
        var width = cvHandle.Dimensions.Width;
        var height = cvHandle.Dimensions.Height;
        var appendixWidth = (int)(width * prolongWidthRatio);

        if (appendixWidth <= 0)
            throw new InvalidImageSizeException(cvHandle.Dimensions.Width, cvHandle.Dimensions.Height);

        using var partToProlongWith = new Mat(cvHandle.Data, new Rect(0, 0, appendixWidth, height));
        var resultImage = new Mat(height, width + appendixWidth, cvHandle.Data.Type());

        using var resultLeftPart = new Mat(resultImage, new Rect(0, 0, width, height));
        using var resultRightPart = new Mat(resultImage, new Rect(width, 0, appendixWidth, height));

        Cv2.CopyTo(cvHandle.Data, resultLeftPart);
        Cv2.CopyTo(partToProlongWith, resultRightPart);

        cvHandle.Data.Dispose();
        cvHandle.Data = resultImage;
    }

    public ImageDataHandle StackImagesVertically(List<ImageDataHandle> images)
    {
        if (images.Count < 2)
            throw new ArgumentException($"{nameof(images)} must contain at least two images");

        var cv2Handles = images
            .Select(image => image.ToCvDataHandle())
            .ToList();

        var width = cv2Handles.Max(i => i.Dimensions.Width);
        var height = cv2Handles.Sum(i => i.Dimensions.Height);
        var type = cv2Handles.First().Data.Type();
        if (cv2Handles.Any(i => i.Data.Type() != type))
            throw new ArgumentException($"{nameof(images)} must share the same underlying OpenCv type");

        var resultImage = new Mat(height, width, type);
        var yOffset = 0;
        for (int i = 0; i < cv2Handles.Count; i++)
        {
            var currentImage = cv2Handles[i];
            using var destinationRoi = new Mat(resultImage,
                new Rect(0, yOffset, currentImage.Dimensions.Width, currentImage.Dimensions.Height));

            Cv2.CopyTo(currentImage.Data, destinationRoi);
            yOffset += currentImage.Dimensions.Height;
        }

        return new CvImageDataHandle(resultImage);
    }

    public List<ImageDataHandle> SliceImage(ImageDataHandle image, ImageDimensions sliceDimensions,
        double xOverlapRatio, double yOverlapRatio)
    {
        var inputHandle = image.ToCvDataHandle();
        var height = inputHandle.Dimensions.Height;
        var width = inputHandle.Dimensions.Width;

        var realSliceWidth = sliceDimensions.Width;
        var realSliceHeight = sliceDimensions.Height;

        var overlapWidth = xOverlapRatio * realSliceWidth;
        var overlapHeight = yOverlapRatio * realSliceHeight;

        var startingXs = GenerateRange(0, width, realSliceWidth);
        var startingYs = GenerateRange(0, height, realSliceHeight);

        var slices = new List<ImageDataHandle>();
        foreach (var y in startingYs)
        {
            foreach (var x in startingXs)
            {
                var xmin = (int)Math.Max(0, x - overlapWidth);
                var ymin = (int)Math.Max(0, y - overlapHeight);
                var xmax = (int)Math.Min(x + realSliceWidth + overlapWidth, width);
                var ymax = (int)Math.Min(y + realSliceHeight + overlapHeight, height);

                var rect = new Rect(xmin, ymin, xmax - xmin, ymax - ymin);
                slices.Add(new CvImageDataHandle(new Mat(inputHandle.Data, rect)));
            }
        }

        return slices;
    }

    private static List<double> GenerateRange(double start, double end, double step)
    {
        var range = new List<double>();
        for (double i = start; i < end; i += step)
        {
            range.Add(i);
        }

        return range;
    }
}
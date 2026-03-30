using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TireRecognition.Application.Exceptions;
using TireRecognition.Application.Options;
using TireRecognition.Application.Services;
using TireRecognition.Domain.Preprocessing;
using TireRecognition.Domain.Recognition;

namespace TireRecognition.Application.Facades;

public class RecognitionFacade : IRecognitionFacade
{
    private readonly IContentTypeResolverService _contentTypeResolverService;
    private readonly IImageManipulationService _imageManipulationService;
    private readonly ITireRimExtractionService _tireRimExtractionService;
    private readonly IRecognitionService _recognitionService;
    private readonly IPostprocessingService _postprocessingService;
    private readonly IDbMatchingService _dbMatchingService;
    private readonly PreprocessingOptions _preprocessingOptions;
    private readonly ILogger<RecognitionFacade> _logger;

    public RecognitionFacade(IContentTypeResolverService contentTypeResolverService,
        IImageManipulationService imageManipulationService, ITireRimExtractionService tireRimExtractionService,
        IRecognitionService recognitionService, IPostprocessingService postprocessingService,
        IDbMatchingService dbMatchingService, IOptions<PreprocessingOptions> preprocessingOptions,
        ILogger<RecognitionFacade> logger)
    {
        _contentTypeResolverService = contentTypeResolverService;
        _imageManipulationService = imageManipulationService;
        _tireRimExtractionService = tireRimExtractionService;
        _recognitionService = recognitionService;
        _postprocessingService = postprocessingService;
        _dbMatchingService = dbMatchingService;
        _preprocessingOptions = preprocessingOptions.Value;
        _logger = logger;
    }

    public async Task PerformRecognitionAsync(Stream imageDataStream, string filename, string contentType)
    {
        _logger.LogInformation($"Started recognition pipeline for image '{filename}'");
        var contentTypeSupported = _contentTypeResolverService.IsContentTypeSupported(contentType);
        if (!contentTypeSupported)
        {
            _logger.LogWarning(
                $"Attempted to perform recognition on file '{filename}' with unsupported content type '{contentType}'");
            throw new ContentTypeNotSupported(contentType);
        }

        using var preprocessedImage = await PerformPreprocessing(imageDataStream, filename);
        var recognitionResult = await PerformRecognitionAsync(preprocessedImage, filename, contentType);
        
    }

    private async Task<ImageDataHandle> PerformPreprocessing(Stream imageDataStream, string filename)
    {
        _logger.LogInformation($"[Preprocessing]: Started for image '{filename}'");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var inputImageHandle = _imageManipulationService.GetImageHandleFromStream(imageDataStream);
        await imageDataStream.DisposeAsync();
        _imageManipulationService.ScaleToMaxDimension(inputImageHandle, _preprocessingOptions.MaxInputImageSize);

        // Detect tire rim
        var detectedRimPosition = await _tireRimExtractionService.DetectTireRimAsync(inputImageHandle);
        if (detectedRimPosition is null)
        {
            ApplyFinalPreprocessingActions(inputImageHandle);
            _logger.LogInformation(
                $"[Preprocessing]: No rim detected in image '{filename}'. Returning backup. Time taken: {stopWatch.Elapsed.TotalMilliseconds}ms");
            return inputImageHandle;
        }

        // Extract sidewall strip
        var extractedTireStripHandle = _tireRimExtractionService.ExtractSidewallRingAroundRimCircle(
            inputImageHandle,
            detectedRimPosition);
        inputImageHandle.Dispose();
        _imageManipulationService.ProlongImageHorizontally(
            extractedTireStripHandle,
            _preprocessingOptions.TireStripProlongWidthRatio);

        // Slice sidewall to prevent width-heavy aspect ratios
        var sliceWidth = (decimal)extractedTireStripHandle.Dimensions.Width /
                         (decimal)_preprocessingOptions.NumberOfSlices;
        var sliceDimensions = new ImageDimensions(
            Height: extractedTireStripHandle.Dimensions.Height,
            Width: (int)Math.Ceiling(sliceWidth)
        );
        var sidewallSlices = _imageManipulationService.SliceImage(
            image: extractedTireStripHandle,
            sliceDimensions: sliceDimensions,
            xOverlapRatio: _preprocessingOptions.SliceOverlapRatio,
            yOverlapRatio: 0
        );
        extractedTireStripHandle.Dispose();

        // Stack slices into one final image
        var finalImage = _imageManipulationService.StackImagesVertically(sidewallSlices);
        sidewallSlices.ForEach(s => s.Dispose());
        ApplyFinalPreprocessingActions(finalImage);
        _logger.LogInformation(
            $"[Preprocessing]: Successfully finished for '{filename}'. Time taken: {stopWatch.Elapsed.TotalMilliseconds}ms");
        return finalImage;
    }

    private void ApplyFinalPreprocessingActions(ImageDataHandle image)
    {
        // Applying more processing to improve contrast
        _imageManipulationService.ApplyClahe(image);
        _imageManipulationService.ApplyBilateralFilter(image);
        _imageManipulationService.ApplyBitwiseNot(image);
        // Reduce output image size
        _imageManipulationService.ScaleToMaxDimension(image, _preprocessingOptions.MaxInputImageSize);
    }

    private async Task<RecognitionResult> PerformRecognitionAsync(ImageDataHandle preprocessedImage, string filename,
        string contentType)
    {
        _logger.LogInformation($"[Recognition]: Started for image '{filename}'");
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var result = await _recognitionService.RecognizeTireCodeAsync(preprocessedImage, contentType);
        _logger.LogInformation(
            $"[Recognition]: Recognition with VLM finished after {stopWatch.Elapsed.TotalMilliseconds}ms. Input token count: '{result.InputTokenCount}'. Output token count: '{result.OutputTokenCount}'.");
        if (result.RecognizedTireCode is null)
        {
            _logger.LogInformation($"[Recognition]: No tire code detected for image '{filename}'");
            throw new NoTireCodeDetectedDuringRecognitionException();
        }

        return result;
    }
}
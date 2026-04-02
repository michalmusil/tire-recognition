using System.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TireRecognition.Application.Exceptions;
using TireRecognition.Application.Options;
using TireRecognition.Application.Services;
using TireRecognition.Domain;
using TireRecognition.Domain.DbMatching;
using TireRecognition.Domain.Postprocessing;
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
    private readonly TireDbMatchingOptions _dbMatchingOptions;
    private readonly ICostEstimationService _costEstimationService;
    private readonly ILogger<RecognitionFacade> _logger;

    public RecognitionFacade(IContentTypeResolverService contentTypeResolverService,
        IImageManipulationService imageManipulationService, ITireRimExtractionService tireRimExtractionService,
        IRecognitionService recognitionService, IPostprocessingService postprocessingService,
        IDbMatchingService dbMatchingService, IOptions<PreprocessingOptions> preprocessingOptions,
        IOptions<TireDbMatchingOptions> dbMatchingOptions, ICostEstimationService costEstimationService,
        ILogger<RecognitionFacade> logger)
    {
        _contentTypeResolverService = contentTypeResolverService;
        _imageManipulationService = imageManipulationService;
        _tireRimExtractionService = tireRimExtractionService;
        _recognitionService = recognitionService;
        _postprocessingService = postprocessingService;
        _dbMatchingService = dbMatchingService;
        _preprocessingOptions = preprocessingOptions.Value;
        _dbMatchingOptions = dbMatchingOptions.Value;
        _costEstimationService = costEstimationService;
        _logger = logger;
    }

    public async Task<RecognitionPipelineResult> ExecuteRecognitionPipelineAsync(
        Stream imageDataStream,
        string filename,
        string contentType,
        int? maxTireCodeDbMatchingEntries = 30
    )
    {
        await using var inputImageStream = imageDataStream;
        _logger.LogInformation($"Started recognition pipeline for image '{filename}'");
        var contentTypeSupported = _contentTypeResolverService.IsContentTypeSupported(contentType);
        if (!contentTypeSupported)
        {
            _logger.LogWarning(
                $"Attempted to perform recognition on file '{filename}' with unsupported content type '{contentType}'");
            throw new ContentTypeNotSupported(contentType);
        }

        var preprocessingExecutionResult = await PerformPreprocessing(inputImageStream, filename);
        using var preprocessedImage = preprocessingExecutionResult.Result;

        var recognitionExecutionResult = await PerformRecognitionAsync(preprocessedImage, filename, contentType);
        var recognitionResult = recognitionExecutionResult.Result;
        var estimatedCosts = _costEstimationService.EstimatedRecognitionCosts(recognitionResult);

        var postprocessingExecutionResult = await PerformPostprocessingAsync(recognitionResult.RecognizedTireCode!);
        var postprocessedTireCode = postprocessingExecutionResult.Result;

        var tireEntryLimit = maxTireCodeDbMatchingEntries ?? _dbMatchingOptions.DefaultTireDbMatchingResultLimit;
        var dbMatchingExecutionResult = await PerformDbMatchingAsync(
            postprocessedTireCode,
            recognitionResult.RecognizedManufacturer,
            tireEntryLimit);
        var dbMatchingResult = dbMatchingExecutionResult.Result;

        return new RecognitionPipelineResult(
            RecognitionResult: recognitionResult,
            EstimatedCosts: estimatedCosts,
            PostprocessedTireCode: postprocessedTireCode,
            DbMatchingResult: dbMatchingResult,
            ExecutionDetails:
            [
                new PipelineStepExecutionDetail("Preprocessing", preprocessingExecutionResult.ExecutionTime),
                new PipelineStepExecutionDetail("Recognition", recognitionExecutionResult.ExecutionTime),
                new PipelineStepExecutionDetail("Postprocessing", postprocessingExecutionResult.ExecutionTime),
                new PipelineStepExecutionDetail("DbMatching", dbMatchingExecutionResult.ExecutionTime),
            ]
        );
    }

    private async Task<MeasuredExecutionTimeResult<ImageDataHandle>> PerformPreprocessing(Stream imageDataStream,
        string filename)
    {
        _logger.LogInformation($"[Preprocessing]: Started for image '{filename}'");
        var stopWatch = new Stopwatch();
        stopWatch.Start();

        var inputImageHandle = _imageManipulationService.GetImageHandleFromStream(imageDataStream);
        _imageManipulationService.ScaleToMaxDimension(inputImageHandle, _preprocessingOptions.MaxInputImageSize);

        // Detect tire rim
        var detectedRimPosition = await _tireRimExtractionService.DetectTireRimAsync(inputImageHandle);
        if (detectedRimPosition is null)
        {
            ApplyFinalPreprocessingActions(inputImageHandle);
            var timeTaken = stopWatch.Elapsed;
            _logger.LogInformation(
                $"[Preprocessing]: No rim detected in image '{filename}'. Returning backup. Time taken: {timeTaken.TotalMilliseconds}ms");
            return new MeasuredExecutionTimeResult<ImageDataHandle>
            {
                ExecutionTime = timeTaken,
                Result = inputImageHandle
            };
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
        var sliceDimensions = extractedTireStripHandle.Dimensions with
        {
            Width = (int)Math.Ceiling(sliceWidth)
        };
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
        var elapsed = stopWatch.Elapsed;
        _logger.LogInformation(
            $"[Preprocessing]: Successfully finished for '{filename}'. Time taken: {elapsed.TotalMilliseconds}ms");
        return new MeasuredExecutionTimeResult<ImageDataHandle>
        {
            ExecutionTime = elapsed,
            Result = finalImage
        };
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

    private async Task<MeasuredExecutionTimeResult<RecognitionResult>> PerformRecognitionAsync(
        ImageDataHandle preprocessedImage, string filename,
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
            throw new NoTireCodeDetectedDuringRecognitionException(filename);
        }

        var elapsed = stopWatch.Elapsed;
        _logger.LogInformation(
            $"[Recognition]: Finished successfully with '{result.RecognizedTireCode}'. Time taken: {elapsed.TotalMilliseconds}ms");
        return new MeasuredExecutionTimeResult<RecognitionResult>
        {
            ExecutionTime = elapsed,
            Result = result
        };
    }

    private async Task<MeasuredExecutionTimeResult<TireCode>> PerformPostprocessingAsync(string rawTireCode)
    {
        _logger.LogInformation($"[Postprocessing]: Started for raw tire code '{rawTireCode}'");
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var extractedTireCodes = (await _postprocessingService.ExtractStructuredTireCodesAsync(rawTireCode)).ToList();
        if (!extractedTireCodes.Any())
        {
            _logger.LogInformation($"[Postprocessing]: No valid tire code detected in raw tire code '{rawTireCode}'");
            throw new NoTireCodeDetectedDuringPostprocessingException(rawTireCode);
        }

        var bestMatch = _postprocessingService.PickBestTireCode(extractedTireCodes)!;
        var elapsed = stopWatch.Elapsed;
        _logger.LogInformation(
            $"[Postprocessing]: Finished with '{bestMatch.GetProcessedCode()}'. Time taken: {elapsed.TotalMilliseconds}ms");
        return new MeasuredExecutionTimeResult<TireCode>
        {
            ExecutionTime = elapsed,
            Result = bestMatch
        };
    }

    private async Task<MeasuredExecutionTimeResult<DbMatchingResult>> PerformDbMatchingAsync(
        TireCode recognizedTireCode, string? rawManufacturer, int maxTireCodeDbMatchingEntries)
    {
        var codeAsString = recognizedTireCode.GetProcessedCode();
        _logger.LogInformation(
            $"[DbMatching]: Started for tire code '{codeAsString}' with raw manufacturer '{rawManufacturer}'");
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var tireCodeMatches = await _dbMatchingService.GetOrderedDbMatchesForTireCodeAsync(
            recognizedTireCode,
            maxTireCodeDbMatchingEntries);
        var manufacturerMatch = rawManufacturer is null
            ? null
            : await _dbMatchingService.GetManufacturerNameDbMatch(rawManufacturer);

        if (tireCodeMatches.Count < 1)
            _logger.LogWarning(
                $"[DbMatching]: Detected tire code matches for code '{codeAsString}' were empty, indicating a problem with tire entry DB.");

        var elapsed = stopWatch.Elapsed;
        _logger.LogInformation(
            $"[DbMatching]: Finished for tire code '{codeAsString}'. Time taken: {elapsed.TotalMilliseconds}ms");
        return new MeasuredExecutionTimeResult<DbMatchingResult>
        {
            ExecutionTime = elapsed,
            Result = new DbMatchingResult(tireCodeMatches, manufacturerMatch)
        };
    }
}
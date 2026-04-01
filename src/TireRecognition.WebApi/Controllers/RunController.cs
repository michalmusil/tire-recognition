using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TireRecognition.Application.Facades;
using TireRecognition.WebApi.Contracts.Run;
using TireRecognition.WebApi.Contracts.Run.Dtos;

namespace TireRecognition.WebApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class RunController : ControllerBase
{
    private readonly IRecognitionFacade _recognitionFacade;
    private readonly ILogger<RunController> _logger;

    public RunController(IRecognitionFacade recognitionFacade, ILogger<RunController> logger)
    {
        _recognitionFacade = recognitionFacade;
        _logger = logger;
    }

    /// <summary>
    /// Processes a single tire image by sequentially performing preprocessing, recognition, postprocessing and db matching pipeline steps. 
    /// </summary>
    /// <remarks>
    /// This endpoint is designed to analyze an uploaded image of a tire photo, extract general parameters of the tire,
    /// match them against existing tire code variations stored in a database and return the results in a structured
    /// format. Ensuring that the whole tire is visible in the photo, the tire code is not obscured, the tire is well
    /// lit and the tire in the photo is not skewed (perspective of the photographer is roughly 90° in each
    /// direction) will ensure the best possible results.
    ///
    /// ### Expected inputs
    /// * [required] A tire photo with aforementioned qualities in one of following formats: [jpg, jpeg, png, webp]
    /// * [optional] A limit on how many matches should get compared with the extracted tire code during db matching. If not specified, default from configuration is used
    /// 
    /// ### Extracted parameters
    /// * Width - width of the tire in millimeters
    /// * Aspect ratio - aspect ratio of the sidewall height to the tire width
    /// * Construction - 1 letter indicating the construction type of the tire
    /// * Diameter - diameter of the tire in inches (rarely in millimeters)
    /// * Load range - a single letter indicating tire ply rating (only rarely present, only on light truck tires)
    /// * Load index - a whole number, indicates load index of a passenger car tire, or single-mounting load index for light truck tire
    /// * Load index 2 - a whole number, only present in light truck tires - indicates dual-mounting load index 
    /// * Speed rating - 1 letter (sometimes with an additional number) indicating the speed rating of the tire
    ///
    /// ### Request format
    /// A multipart/form-data request is expected with the following parts:
    /// * Input photo of the tire
    /// ```
    /// ------exampleBoundary123
    /// Content-Disposition: form-data; name="Image"; filename="example-image.jpg"
    /// Content-Type: image/jpeg
    ///
    /// **raw image bytes**
    /// ```
    /// * [optionally] Limit on db matching
    /// ```
    /// ------exampleBoundary456
    /// Content-Disposition: form-data; name="DbMatchingResultLimit"
    ///
    /// 10
    /// ```
    /// </remarks>
    /// <param name="pipelineRequest">The request object containing the image.</param>
    /// <returns>A response object with the recognition pipeline results containing decoded tire parameters, estimated costs of inference and details of each pipeline steps.</returns>
    /// <response code="200">Returns the processed results.</response>
    /// <response code="400">If the request payload is invalid.</response>
    /// <response code="404">If no tire code is detected in one of the pipeline steps and the pipeline thus can't correctly finish.</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(RunPipelineResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RunPipelineResponse>> RunRecognitionPipelineAsync(
        [FromForm] RunPipelineRequest pipelineRequest)
    {
        _logger.LogInformation(
            $"[{nameof(RunController)}]: Started processing pipeline for image '{pipelineRequest.Image.FileName}'");
        var stopWatch = new Stopwatch();
        stopWatch.Start();
        var imageFileName = pipelineRequest.Image.FileName;
        var imageContentType = pipelineRequest.Image.ContentType;
        var imageDataStream = pipelineRequest.Image.OpenReadStream();
        var result = await _recognitionFacade.PerformRecognitionAsync(
            imageDataStream: imageDataStream,
            filename: imageFileName,
            contentType: imageContentType,
            maxTireCodeDbMatchingEntries: pipelineRequest.DbMatchingResultLimit
        );

        var elapsed = stopWatch.Elapsed;
        _logger.LogInformation(
            $"[{nameof(RunController)}]: Successfully finished processing pipeline for image '{pipelineRequest.Image.FileName}'. Time taken: {elapsed.TotalMilliseconds}ms");

        var response = new RunPipelineResponse(
            Result: ResultDto.FromDomain(
                domain: result,
                imageFileName: imageFileName,
                totalDuration: elapsed
            )
        );
        return Ok(response);
    }
}
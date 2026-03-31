using System.ComponentModel.DataAnnotations;

namespace TireRecognition.WebApi.Contracts.Run;

public record RunPipelineRequest(
    [Required] IFormFile Image,
    int? DbMatchingResultLimit
);
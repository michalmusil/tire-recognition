using System.ComponentModel.DataAnnotations;

namespace TireRecognition.WebApi.Contracts.Run;

public record RunRequest(
    [Required] IFormFile Image
);
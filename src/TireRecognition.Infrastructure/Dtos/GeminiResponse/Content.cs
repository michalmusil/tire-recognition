using System.Text.Json.Serialization;

namespace TireRecognition.Infrastructure.Dtos.GeminiResponse;

public record Content(
    [property: JsonPropertyName("parts")] List<ContentPart> Parts,
    [property: JsonPropertyName("role")] string Role
);
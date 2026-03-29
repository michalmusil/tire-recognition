using System.Text.Json.Serialization;

namespace TireRecognition.Infrastructure.Dtos.GeminiResponse;

public record ContentCandidate(
    [property: JsonPropertyName("content")] Content Content
);

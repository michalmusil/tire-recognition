using System.Text.Json.Serialization;

namespace TireRecognition.Infrastructure.Dtos.GeminiResponse;

public record ContentPart(
    [property: JsonPropertyName("text")] string Text
);
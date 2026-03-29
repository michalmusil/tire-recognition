using System.Text.Json.Serialization;

namespace TireRecognition.Infrastructure.Dtos.GeminiResponse;

public record UsageMetadata(
    [property: JsonPropertyName("promptTokenCount")]
    int PromptTokenCount,
    [property: JsonPropertyName("candidatesTokenCount")]
    int CandidatesTokenCount,
    [property: JsonPropertyName("totalTokenCount")]
    int TotalTokenCount
);
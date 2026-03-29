using System.Text.Json.Serialization;

namespace TireRecognition.Infrastructure.Dtos.GeminiResponse;

public record GeminiResponseDto(
    [property: JsonPropertyName("candidates")]
    List<ContentCandidate> ContentCandidates,
    [property: JsonPropertyName("usageMetadata")]
    UsageMetadata UsageMetadata
);
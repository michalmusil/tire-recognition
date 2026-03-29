namespace TireRecognition.Domain.Recognition;

public record RecognitionResult(
    string? RecognizedTireCode,
    string? RecognizedManufacturer,
    int InputTokenCount,
    int OutputTokenCount
);
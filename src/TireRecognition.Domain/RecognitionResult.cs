namespace TireRecognition.Domain;

public record RecognitionResult(
    string? RecognizedTireCode,
    string? RecognizedManufacturer,
    int InputTokenCount,
    int OutputTokenCount
);
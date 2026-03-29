namespace TireRecognition.Domain.DbMatching;

public record DbMatchingResult(
    List<TireDbMatch> TireDbMatches,
    string? ManufacturerDbMatch
);
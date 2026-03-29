namespace TireRecognition.Domain.DbMatching;

public record TireDbMatch(
    ProcessedTireParamsDatabaseEntryDto TireEntry,
    int TotalRequiredCharEdits,
    int MatchedMainParameterCount,
    decimal EstimatedAccuracy,
    ParameterMatch WidthMatch,
    ParameterMatch DiameterMatch,
    ParameterMatch ProfileMatch,
    ParameterMatch? ConstructionMatch,
    ParameterMatch LoadIndexMatch,
    ParameterMatch? LoadIndex2Match,
    ParameterMatch SpeedIndexMatch
);
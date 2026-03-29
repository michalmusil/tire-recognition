namespace TireRecognition.Domain.DbMatching;

public record ParameterMatch(
    int RequiredCharEdits,
    decimal EstimatedAccuracy
)
{
    public bool MatchesExactly => RequiredCharEdits == 0;
}
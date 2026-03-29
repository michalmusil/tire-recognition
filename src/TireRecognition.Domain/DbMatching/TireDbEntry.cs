namespace TireRecognition.Domain.DbMatching;

public class ProcessedTireParamsDatabaseEntryDto
{
    public decimal Width { get; init; }
    public decimal Diameter { get; init; }
    public decimal Profile { get; init; }
    public string? Construction { get; init; }
    public int? LoadIndex { get; init; }
    public int? LoadIndex2 { get; init; }
    public string? SpeedIndex { get; init; }
    public string? LoadIndexSpeedIndex { get; init; }

    public string GetTireCodeString()
    {
        var diameter = decimal.Round(Diameter, 1);

        return $"{Width}/{Profile}{Construction}{diameter} {LoadIndexSpeedIndex}";
    }
}
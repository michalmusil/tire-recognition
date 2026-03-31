using TireRecognition.Domain.DbMatching;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record TireDbEntryDto(
    decimal Width,
    decimal Diameter,
    decimal Profile,
    string Construction,
    int? LoadIndex,
    int? LoadIndex2,
    string? SpeedIndex,
    string LoadIndexSpeedIndex
)
{
    public static TireDbEntryDto FromDomain(TireDbEntry domain) => new(
        Width: domain.Width,
        Diameter: domain.Diameter,
        Profile: domain.Profile,
        Construction: domain.Construction ?? "",
        LoadIndex: domain.LoadIndex,
        LoadIndex2: domain.LoadIndex2,
        SpeedIndex: domain.SpeedIndex,
        LoadIndexSpeedIndex: domain.LoadIndexSpeedIndex ?? ""
    );
}
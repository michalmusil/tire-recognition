using TireRecognition.Domain.DbMatching;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record TireDbMatchDto(
    TireDbEntryDto TireEntry,
    int TotalRequiredCharEdits,
    int MatchedMainParameterCount,
    decimal EstimatedAccuracy,
    ParameterMatchDto WidthMatch,
    ParameterMatchDto DiameterMatch,
    ParameterMatchDto ProfileMatch,
    ParameterMatchDto? ConstructionMatch,
    ParameterMatchDto LoadIndexMatch,
    ParameterMatchDto? LoadIndex2Match,
    ParameterMatchDto SpeedIndexMatch
)
{
    public static TireDbMatchDto FromDomain(TireDbMatch domain) => new(
        TireEntry: TireDbEntryDto.FromDomain(domain.TireEntry),
        TotalRequiredCharEdits: domain.TotalRequiredCharEdits,
        MatchedMainParameterCount: domain.MatchedMainParameterCount,
        EstimatedAccuracy: domain.EstimatedAccuracy,
        WidthMatch: ParameterMatchDto.FromDomain(domain.WidthMatch),
        DiameterMatch: ParameterMatchDto.FromDomain(domain.DiameterMatch),
        ProfileMatch: ParameterMatchDto.FromDomain(domain.ProfileMatch),
        ConstructionMatch: domain.ConstructionMatch is null
            ? null
            : ParameterMatchDto.FromDomain(domain.ConstructionMatch),
        LoadIndexMatch: ParameterMatchDto.FromDomain(domain.LoadIndexMatch),
        LoadIndex2Match: domain.LoadIndex2Match is null ? null : ParameterMatchDto.FromDomain(domain.LoadIndex2Match),
        SpeedIndexMatch: ParameterMatchDto.FromDomain(domain.SpeedIndexMatch)
    );
}
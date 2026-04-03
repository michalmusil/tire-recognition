using TireRecognition.Domain.DbMatching;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record DbMatchingResultDto(
    List<TireDbMatchDto> OrderedTireCodeDbMatches,
    string? ManufacturerDbMatch
)
{
    public static DbMatchingResultDto FromDomain(DbMatchingResult domain) => new(
        OrderedTireCodeDbMatches: domain.TireDbMatches.Select(TireDbMatchDto.FromDomain).ToList(),
        ManufacturerDbMatch: domain.ManufacturerDbMatch
    );
}
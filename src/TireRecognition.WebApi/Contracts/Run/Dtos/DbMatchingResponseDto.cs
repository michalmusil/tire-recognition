using TireRecognition.Domain.DbMatching;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record DbMatchingResponseDto(
    List<TireDbMatchDto> OrderedTireCodeDbMatches,
    string? ManufacturerDbMatch
)
{
    public static DbMatchingResponseDto FromDomain(DbMatchingResult domain) => new(
        OrderedTireCodeDbMatches: domain.TireDbMatches.Select(TireDbMatchDto.FromDomain).ToList(),
        ManufacturerDbMatch: domain.ManufacturerDbMatch
    );
}
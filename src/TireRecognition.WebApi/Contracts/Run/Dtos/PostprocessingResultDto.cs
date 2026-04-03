using TireRecognition.Domain;
using TireRecognition.Domain.Postprocessing;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record PostprocessingResultDto(
    string RawCode,
    string PostprocessedTireCode,
    decimal? Width,
    decimal? AspectRatio,
    string? Construction,
    decimal? Diameter,
    char? LoadRange,
    int? LoadIndex,
    int? LoadIndex2,
    string? SpeedRating
)
{
    public static PostprocessingResultDto FromDomain(TireCode domain) => new(
        RawCode: domain.RawCode,
        PostprocessedTireCode: domain.GetProcessedCode(),
        Width: domain.Width,
        AspectRatio: domain.AspectRatio,
        Construction: domain.Construction,
        Diameter: domain.Diameter,
        LoadRange: domain.LoadRange,
        LoadIndex: domain.LoadIndex,
        LoadIndex2: domain.LoadIndex2,
        SpeedRating: domain.SpeedRating
    );
}
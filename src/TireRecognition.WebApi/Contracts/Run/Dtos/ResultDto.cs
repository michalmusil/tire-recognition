using TireRecognition.Domain;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record ResultDto(
    string ImageFileName,
    RecognitionResultDto OcrResult,
    PostprocessingResultDto PostprocessingResult,
    DbMatchingResultDto TasyDbMatchesResult,
    double TotalDurationMs,
    List<RunStatDto> RunTrace
)
{
    public static ResultDto FromDomain(
        RecognitionPipelineResult domain,
        string imageFileName,
        TimeSpan totalDuration
    ) => new(
        ImageFileName: imageFileName,
        OcrResult: RecognitionResultDto.FromDomain(domain.RecognitionResult, domain.EstimatedCosts),
        PostprocessingResult: PostprocessingResultDto.FromDomain(domain.PostprocessedTireCode),
        TasyDbMatchesResult: DbMatchingResultDto.FromDomain(domain.DbMatchingResult),
        TotalDurationMs: totalDuration.TotalMilliseconds,
        RunTrace: domain.ExecutionDetails.Select(RunStatDto.FromDomain).ToList()
    );
}
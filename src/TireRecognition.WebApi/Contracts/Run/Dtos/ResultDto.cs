using TireRecognition.Domain;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record ResultDto(
    string ImageFileName,
    RecognitionResponseDto OcrResponse,
    PostprocessingResponseDto PostprocessingResult,
    DbMatchingResponseDto TasyDbMatchesResult,
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
        OcrResponse: RecognitionResponseDto.FromDomain(domain.RecognitionResult, domain.EstimatedCosts),
        PostprocessingResult: PostprocessingResponseDto.FromDomain(domain.PostprocessedTireCode),
        TasyDbMatchesResult: DbMatchingResponseDto.FromDomain(domain.DbMatchingResult),
        TotalDurationMs: totalDuration.TotalMilliseconds,
        RunTrace: domain.ExecutionDetails.Select(RunStatDto.FromDomain).ToList()
    );
}
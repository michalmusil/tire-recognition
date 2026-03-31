using TireRecognition.Domain;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record RunStatDto(
    string TaskName,
    double DurationMs
)
{
    public static RunStatDto FromDomain(PipelineStepExecutionDetail domain) => new(
        TaskName: domain.PipelineStepName,
        DurationMs: domain.ExecutionTime.TotalMilliseconds
    );
}
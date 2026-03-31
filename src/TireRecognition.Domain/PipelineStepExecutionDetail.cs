namespace TireRecognition.Domain;

public record PipelineStepExecutionDetail(
    string PipelineStepName,
    TimeSpan ExecutionTime
);
using TireRecognition.Domain.DbMatching;
using TireRecognition.Domain.Postprocessing;
using TireRecognition.Domain.Recognition;

namespace TireRecognition.Domain;

public record RecognitionPipelineResult(
    RecognitionResult RecognitionResult,
    EstimatedRecognitionCosts EstimatedCosts,
    TireCode PostprocessedTireCode,
    DbMatchingResult DbMatchingResult,
    List<PipelineStepExecutionDetail> ExecutionDetails
);
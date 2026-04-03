using TireRecognition.Domain.Recognition;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record RecognitionResultDto(
    string DetectedCode,
    string? DetectedManufacturer,
    EstimatedCostsDto? EstimatedCosts
)
{
    public static RecognitionResultDto FromDomain(
        RecognitionResult domainResult,
        EstimatedRecognitionCosts domainCostEstimation
    ) => new(
        DetectedCode: domainResult.RecognizedTireCode ?? "",
        DetectedManufacturer: domainResult.RecognizedManufacturer,
        EstimatedCosts: EstimatedCostsDto.FromDomain(domainCostEstimation)
    );
}
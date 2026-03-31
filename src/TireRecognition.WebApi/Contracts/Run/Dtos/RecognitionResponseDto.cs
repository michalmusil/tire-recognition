using TireRecognition.Domain.Recognition;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record RecognitionResponseDto(
    string DetectedCode,
    string? DetectedManufacturer,
    EstimatedCostsDto? EstimatedCosts
)
{
    public static RecognitionResponseDto FromDomain(
        RecognitionResult domainResult,
        EstimatedRecognitionCosts domainCostEstimation
    ) => new(
        DetectedCode: domainResult.RecognizedTireCode ?? "",
        DetectedManufacturer: domainResult.RecognizedManufacturer,
        EstimatedCosts: EstimatedCostsDto.FromDomain(domainCostEstimation)
    );
}
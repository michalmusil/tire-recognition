using TireRecognition.Domain.Recognition;

namespace TireRecognition.WebApi.Contracts.Run.Dtos;

public record EstimatedCostsDto(
    decimal InputUnitCount,
    decimal OutputUnitCount,
    string BillingUnit,
    decimal EstimatedCost,
    string EstimatedCostCurrency
)
{
    public static EstimatedCostsDto FromDomain(EstimatedRecognitionCosts domain) => new EstimatedCostsDto(
        InputUnitCount: domain.InputTokenCount,
        OutputUnitCount: domain.OutputTokenCount,
        BillingUnit: "Token",
        EstimatedCost: domain.EstimatedCost,
        EstimatedCostCurrency: domain.EstimatedCostCurrency
    );
}
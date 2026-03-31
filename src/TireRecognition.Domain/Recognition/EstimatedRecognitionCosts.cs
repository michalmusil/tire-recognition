namespace TireRecognition.Domain.Recognition;

public record EstimatedRecognitionCosts(
    decimal InputTokenCount,
    decimal OutputTokenCount,
    decimal EstimatedCost,
    string EstimatedCostCurrency
);
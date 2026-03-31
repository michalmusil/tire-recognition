using TireRecognition.Application.Options;
using TireRecognition.Application.Services;
using TireRecognition.Domain.Recognition;

namespace TireRecognition.Infrastructure.Services;

public class CostEstimationService : ICostEstimationService
{
    private const decimal Million = 1000000;
    private readonly RecognitionOptions _recognitionOptions;

    public CostEstimationService(RecognitionOptions recognitionOptions)
    {
        _recognitionOptions = recognitionOptions;
    }

    public EstimatedRecognitionCosts EstimatedRecognitionCosts(RecognitionResult recognitionResult)
    {
        var inputPriceUsd = recognitionResult.InputTokenCount *
                            (_recognitionOptions.InputTokenPricePerMillion / Million);
        var outputPriceUsd = recognitionResult.OutputTokenCount *
                             (_recognitionOptions.OutputTokenPricePerMillion / Million);

        return new EstimatedRecognitionCosts(
            InputTokenCount: recognitionResult.InputTokenCount,
            OutputTokenCount: recognitionResult.OutputTokenCount,
            EstimatedCost: inputPriceUsd + outputPriceUsd,
            EstimatedCostCurrency: "USD"
        );
    }
}
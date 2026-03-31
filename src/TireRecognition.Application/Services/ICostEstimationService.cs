using TireRecognition.Domain.Recognition;

namespace TireRecognition.Application.Services;

public interface ICostEstimationService
{
    public EstimatedRecognitionCosts EstimatedRecognitionCosts(RecognitionResult recognitionResult);
}
using TireRecognition.Domain;

namespace TireRecognition.Application.Services;

public interface IRecognitionService
{
    public Task<RecognitionResult> RecognizeTireCodeAsync(ImageDataHandle image);
}
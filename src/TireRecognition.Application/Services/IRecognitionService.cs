using TireRecognition.Domain.Preprocessing;
using TireRecognition.Domain.Recognition;

namespace TireRecognition.Application.Services;

public interface IRecognitionService
{
    public Task<RecognitionResult> RecognizeTireCodeAsync(ImageDataHandle image);
}
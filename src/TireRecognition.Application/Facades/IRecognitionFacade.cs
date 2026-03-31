using TireRecognition.Domain;

namespace TireRecognition.Application.Facades;

public interface IRecognitionFacade
{
    public Task<RecognitionPipelineResult> PerformRecognitionAsync(
        Stream imageDataStream,
        string filename,
        string contentType,
        int? maxTireCodeDbMatchingEntries
    );
}
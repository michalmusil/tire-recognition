using TireRecognition.Domain;

namespace TireRecognition.Application.Facades;

public interface IRecognitionFacade
{
    public Task<RecognitionPipelineResult> ExecuteRecognitionPipelineAsync(
        Stream imageDataStream,
        string filename,
        string contentType,
        int? maxTireCodeDbMatchingEntries
    );
}
namespace TireRecognition.Application.Facades;

public interface IRecognitionFacade
{
    public Task PerformRecognitionAsync(Stream imageDataStream, string filename, string contentType);
}
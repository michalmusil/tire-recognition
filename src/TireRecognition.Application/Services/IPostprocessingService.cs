using TireRecognition.Domain.Postprocessing;

namespace TireRecognition.Application.Services;

public interface IPostprocessingService
{
    public Task<PostprocessingResult> PostprocessTireCodeAsync(string rawTireCode);
}
using TireRecognition.Domain.Postprocessing;

namespace TireRecognition.Application.Services;

public interface IPostprocessingService
{
    public Task<IEnumerable<TireCode>> ExtractStructuredTireCodesAsync(string rawTireCode);
    public TireCode? PickBestTireCode(IEnumerable<TireCode> tireCodes);
}
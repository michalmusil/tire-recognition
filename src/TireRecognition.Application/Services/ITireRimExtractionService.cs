using TireRecognition.Domain;

namespace TireRecognition.Application.Services;

public interface ITireRimExtractionService
{
    public Task<TireRimPosition?> DetectTireRimAsync(ImageDataHandle image);
    public ImageDataHandle ExtractSidewallRingAroundRimCircle(ImageDataHandle image, TireRimPosition rimCircle);
}
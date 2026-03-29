using TireRecognition.Application.Services;

namespace TireRecognition.Infrastructure.Services;

public class ContentTypeResolverService : IContentTypeResolverService
{
    private static readonly List<string> SupportedContentTypes = ["image/jpeg", "image/png", "image/webp"];

    public bool IsContentTypeSupported(string contentType)
    {
        return SupportedContentTypes
            .Any(ct => string.Equals(ct, contentType, StringComparison.OrdinalIgnoreCase));
    }
}
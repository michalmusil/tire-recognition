using TireRecognition.Domain.Preprocessing;
using TireRecognition.Infrastructure.Exceptions;

namespace TireRecognition.Infrastructure.Extensions;

public static class ImageDataHandleExtensions
{
    public static CvImageDataHandle ToCvDataHandle(this ImageDataHandle abstractHandle)
    {
        var abstractHandleType = abstractHandle.GetType();
        if (abstractHandle.GetType() != typeof(CvImageDataHandle))
            throw new UnsupportedImageDataHandleException(abstractHandleType);
        return (CvImageDataHandle)abstractHandle;
    }
}
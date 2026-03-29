using TireRecognition.Domain.Preprocessing;

namespace TireRecognition.Infrastructure.Exceptions;

public class UnsupportedImageDataHandleException(Type unsupportedType)
    : Exception($"Provided unsupported type of {nameof(ImageDataHandle)}: '{unsupportedType.Name}'")
{
}
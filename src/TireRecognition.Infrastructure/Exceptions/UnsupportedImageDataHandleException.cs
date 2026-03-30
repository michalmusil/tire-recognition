using TireRecognition.Domain.Preprocessing;
using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Infrastructure.Exceptions;

public class UnsupportedImageDataHandleException(Type unsupportedType)
    : InternalErrorException($"Provided unsupported type of {nameof(ImageDataHandle)}: '{unsupportedType.Name}'")
{
}
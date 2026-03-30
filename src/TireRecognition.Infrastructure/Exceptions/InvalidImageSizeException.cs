using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Infrastructure.Exceptions;

public class InvalidImageSizeException(int width, int height)
    : InternalErrorException($"Image size '{width}x{height}' is not supported");
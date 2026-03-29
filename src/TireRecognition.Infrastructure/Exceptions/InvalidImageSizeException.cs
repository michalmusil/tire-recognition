namespace TireRecognition.Infrastructure.Exceptions;

public class InvalidImageSizeException(int width, int height)
    : Exception($"Image size '{width}x{height}' is not supported");
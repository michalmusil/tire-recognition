using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Application.Exceptions;

public class ContentTypeNotSupported(string contentType)
    : BadRequestException($"Content type '{contentType}' is not supported.");
namespace TireRecognition.Shared.Exceptions;

public class NotFoundException(string message) : HttpTranslatableException(404, message);
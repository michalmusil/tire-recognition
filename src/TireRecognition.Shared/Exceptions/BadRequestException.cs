namespace TireRecognition.Shared.Exceptions;

public class BadRequestException(string message) : HttpTranslatableException(400, message);
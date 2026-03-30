namespace TireRecognition.Shared.Exceptions;

public class InternalErrorException(string message) : HttpTranslatableException(500, message);
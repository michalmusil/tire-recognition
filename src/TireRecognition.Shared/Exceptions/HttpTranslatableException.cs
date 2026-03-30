namespace TireRecognition.Shared.Exceptions;

public abstract class HttpTranslatableException(int code, string message) : Exception(message)
{
    public int Code { get; init; } = code;
}
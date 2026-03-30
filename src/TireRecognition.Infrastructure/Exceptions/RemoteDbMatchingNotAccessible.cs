namespace TireRecognition.Infrastructure.Exceptions;

public class RemoteDbMatchingNotAccessible(string endpoint, int statusCode)
    : Exception($"Failed to access remote db matching (code: {statusCode}) on endpoint: '{endpoint}'");
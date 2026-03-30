using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Infrastructure.Exceptions;

public class RemoteDbMatchingUnreachableException(string endpoint, int statusCode)
    : InternalErrorException(
        $"Retrieving entries for remote db matching resulted in code: {statusCode} on endpoint: '{endpoint}'");
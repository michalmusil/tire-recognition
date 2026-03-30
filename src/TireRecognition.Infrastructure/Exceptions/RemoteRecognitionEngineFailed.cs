using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Infrastructure.Exceptions;

public class RemoteRecognitionEngineFailed(string engineName)
    : InternalErrorException($"Recognition step with remote engine '{engineName}' failed.");
using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Application.Exceptions;

public class NoTireCodeDetectedDuringPostprocessingException(string rawTireCode)
    : NotFoundException($"No valid tire code detected in recognition result '{rawTireCode}'");
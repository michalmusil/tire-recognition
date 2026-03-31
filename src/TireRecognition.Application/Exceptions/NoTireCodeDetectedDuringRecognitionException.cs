using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Application.Exceptions;

public class NoTireCodeDetectedDuringRecognitionException(string imageFileName)
    : NotFoundException($"No tire code detected via recognition engine for file '{imageFileName}");
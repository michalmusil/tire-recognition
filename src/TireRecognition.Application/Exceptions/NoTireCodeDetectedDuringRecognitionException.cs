using TireRecognition.Shared.Exceptions;

namespace TireRecognition.Application.Exceptions;

public class NoTireCodeDetectedDuringRecognitionException()
    : NotFoundException("No tire code detected via recognition engine");
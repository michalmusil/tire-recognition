namespace TireRecognition.Domain;

public class MeasuredExecutionTimeResult<T>
    where T : class
{
    public required TimeSpan ExecutionTime { get; init; }
    public required T Result { get; init; }
}
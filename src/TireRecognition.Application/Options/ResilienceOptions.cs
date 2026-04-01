namespace TireRecognition.Application.Options;

public class ResilienceOptions
{
    /// <summary>
    /// How many times should the http clients retry after failure
    /// </summary>
    public int MaxRetries { get; set; } = 2;

    /// <summary>
    /// How much time should the client wait before attempting a retry
    /// </summary>
    public int RetryDelayMs { get; set; } = 1000;

    /// <summary>
    /// The timeout for any given single request
    /// </summary>
    public int AttemptTimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// The timeout for the entire call via http client (including retries)
    /// </summary>
    public int TotalTimeoutSeconds { get; set; } = 60;

    /// <summary>
    /// Determines the sampling duration for the circuit breaker
    /// </summary>
    public int CircuitBreakerSamplingDurationSeconds { get; set; } = 80;
}
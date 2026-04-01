using Microsoft.Extensions.DependencyInjection;
using TireRecognition.Application.Options;

namespace TireRecognition.Infrastructure.Extensions;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddConfiguredResilience(this IHttpClientBuilder httpClientBuilder,
        ResilienceOptions resilienceOptions)
    {
        httpClientBuilder.AddStandardResilienceHandler(opts =>
        {
            opts.Retry.MaxRetryAttempts = resilienceOptions.MaxRetries;
            opts.Retry.Delay = TimeSpan.FromMilliseconds(resilienceOptions.RetryDelayMs);
            opts.Retry.BackoffType = Polly.DelayBackoffType.Constant;

            var attemptTimeout = TimeSpan.FromSeconds(resilienceOptions.AttemptTimeoutSeconds);
            opts.AttemptTimeout.Timeout = attemptTimeout;
            opts.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(resilienceOptions.TotalTimeoutSeconds);
            opts.CircuitBreaker.SamplingDuration =
                TimeSpan.FromSeconds(resilienceOptions.CircuitBreakerSamplingDurationSeconds);
        });
        return httpClientBuilder;
    }
}
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using SkiaSharp;
using TireRecognition.Application.Options;
using TireRecognition.Application.Repositories;
using TireRecognition.Application.Services;
using TireRecognition.Infrastructure.Extensions;
using TireRecognition.Infrastructure.Repositories;
using TireRecognition.Infrastructure.Services;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.ExecutionProvider.Cpu;
using YoloDotNet.Models;

namespace TireRecognition.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration,
        ILoggingBuilder loggingBuilder)
    {
        AddOptions(services, configuration);
        AddMlModels(services);
        AddRepositories(services);
        AddServices(services);
        AddOtel(services, configuration, loggingBuilder);
        return services;
    }

    private static void AddOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PreprocessingOptions>(configuration.GetSection(key: nameof(PreprocessingOptions)));
        services.Configure<RecognitionOptions>(configuration.GetSection(key: nameof(RecognitionOptions)));
        services.Configure<TireDbMatchingOptions>(configuration.GetSection(key: nameof(TireDbMatchingOptions)));
        services.Configure<ResilienceOptions>(configuration.GetSection(key: nameof(ResilienceOptions)));
    }

    private static void AddMlModels(IServiceCollection services)
    {
        services.AddSingleton<Yolo>(sp =>
        {
            var preprocessingOptions = sp.GetRequiredService<IOptions<PreprocessingOptions>>().Value;
            var modelAbsolutePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
                preprocessingOptions.TireRimDetectionModelRelativePath);
            return new Yolo(new YoloOptions
            {
                ExecutionProvider = new CpuExecutionProvider(
                    model: modelAbsolutePath
                ),
                ImageResize = ImageResize.Proportional,
                SamplingOptions = new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None)
            });
        });
    }

    private static void AddRepositories(IServiceCollection services)
    {
        using var sp = services.BuildServiceProvider();
        var resilienceOptions = sp.GetRequiredService<IOptions<ResilienceOptions>>().Value;
        services.AddMemoryCache();

        services.AddHttpClient<ISupportedTireEntryRepository, RemoteSupportedTireEntryRepository>()
            .AddConfiguredResilience(resilienceOptions);
        services.AddHttpClient<ISupportedManufacturerRepository, RemoteSupportedManufacturerRepository>()
            .AddConfiguredResilience(resilienceOptions);
        ;
    }

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IContentTypeResolverService, ContentTypeResolverService>();
        services.AddScoped<IImageManipulationService, CvImageManipulationService>();
        services.AddScoped<ITireRimExtractionService, TireRimExtractionService>();
        services.AddScoped<IPostprocessingService, PostprocessingService>();
        services.AddScoped<IDbMatchingService, DbMatchingService>();
        services.AddScoped<ICostEstimationService, CostEstimationService>();

        using var sp = services.BuildServiceProvider();
        var resilienceOptions = sp.GetRequiredService<IOptions<ResilienceOptions>>().Value;
        services.AddHttpClient<IRecognitionService, GeminiRecognitionService>()
            .AddConfiguredResilience(resilienceOptions);
        ;
    }

    private static void AddOtel(IServiceCollection services, IConfiguration configuration,
        ILoggingBuilder loggingBuilder)
    {
        var otelEnabled = configuration.GetValue<bool>("OTEL_ENABLED");
        if (!otelEnabled)
            return;

        services.AddOpenTelemetry()
            .WithTracing(tracing => tracing
                .AddSource(configuration.GetValue<string>("OTEL_SERVICE_NAME") ?? "TireRecognition")
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddRuntimeInstrumentation());

        loggingBuilder.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        services.AddOpenTelemetry().UseOtlpExporter();
    }
}
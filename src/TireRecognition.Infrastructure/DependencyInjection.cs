using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SkiaSharp;
using TireRecognition.Application.Services;
using TireRecognition.Infrastructure.Options;
using TireRecognition.Infrastructure.Services;
using YoloDotNet;
using YoloDotNet.Enums;
using YoloDotNet.ExecutionProvider.Cpu;
using YoloDotNet.Models;

namespace TireRecognition.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddOptions(services, configuration);
        AddMlModels(services);
        AddServices(services);
        AddFacades(services);
        return services;
    }

    private static void AddOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PreprocessingOptions>(configuration.GetSection(key: nameof(PreprocessingOptions)));
        services.Configure<RecognitionOptions>(configuration.GetSection(key: nameof(RecognitionOptions)));
        services.Configure<TireDbMatchingOptions>(configuration.GetSection(key: nameof(TireDbMatchingOptions)));
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

    private static void AddServices(IServiceCollection services)
    {
        services.AddScoped<IContentTypeResolverService, ContentTypeResolverService>();
        services.AddScoped<IImageManipulationService, CvImageManipulationService>();
        services.AddScoped<IRecognitionService, GeminiRecognitionService>();
        services.AddScoped<ITireRimExtractionService, TireRimExtractionService>();
        services.AddScoped<IPostprocessingService, PostprocessingService>();
    }

    private static void AddFacades(IServiceCollection services)
    {
    }
}
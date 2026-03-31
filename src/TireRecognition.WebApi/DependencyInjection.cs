using System.Reflection;
using TireRecognition.Application.Options;

namespace TireRecognition.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        AddOptions(services, configuration);
        ApplyApiConfiguration(services);
        return services;
    }

    private static void AddOptions(IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<PreprocessingOptions>(configuration.GetSection(key: nameof(PreprocessingOptions)));
        services.Configure<RecognitionOptions>(configuration.GetSection(key: nameof(RecognitionOptions)));
        services.Configure<TireDbMatchingOptions>(configuration.GetSection(key: nameof(TireDbMatchingOptions)));
    }

    private static void ApplyApiConfiguration(IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(opt =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));
        });
    }
}
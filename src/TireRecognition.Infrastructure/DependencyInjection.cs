using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TireRecognition.Infrastructure.Options;

namespace TireRecognition.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        AddOptions(services, configuration);
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

    private static void AddServices(IServiceCollection services)
    {
    }

    private static void AddFacades(IServiceCollection services)
    {
    }
}
using Microsoft.Extensions.DependencyInjection;
using TireRecognition.Application.Facades;

namespace TireRecognition.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        AddFacades(services);
        return services;
    }

    private static void AddFacades(IServiceCollection services)
    {
        services.AddScoped<IRecognitionFacade, RecognitionFacade>();
    }
}
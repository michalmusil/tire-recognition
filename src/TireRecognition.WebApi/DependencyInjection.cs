using System.Reflection;
using TireRecognition.WebApi.ExceptionHandlers;

namespace TireRecognition.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        ApplyApiConfiguration(services);
        AddExceptionHandlers(services);
        return services;
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

    private static void AddExceptionHandlers(IServiceCollection services)
    {
        services.AddExceptionHandler<HttpTranslatableExceptionHandler>();
        services.AddProblemDetails();
    }
}
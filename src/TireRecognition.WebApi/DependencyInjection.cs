using System.Reflection;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using TireRecognition.Application.Options;
using TireRecognition.WebApi.Middleware;

namespace TireRecognition.WebApi;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        ApplyApiConfiguration(services);
        AddExceptionHandlers(services);
        AddSwagger(services);
        return services;
    }

    private static void ApplyApiConfiguration(IServiceCollection services)
    {
        using var sp = services.BuildServiceProvider();
        var authOptions = sp.GetRequiredService<IOptions<AuthOptions>>().Value;

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        if (authOptions.Enabled)
            services.AddAuthentication(authOptions.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(authOptions.SchemeName, null);
        else
            services.AddAuthentication(authOptions.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, AllowAllAuthHandler>(authOptions.SchemeName, null);
    }

    private static void AddSwagger(IServiceCollection services)
    {
        using var sp = services.BuildServiceProvider();
        var authOptions = sp.GetRequiredService<IOptions<AuthOptions>>().Value;

        services.AddSwaggerGen(opt =>
        {
            var xmlFilename = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            opt.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFilename));

            if (authOptions.Enabled)
            {
                opt.AddSecurityDefinition(authOptions.SchemeName, new OpenApiSecurityScheme
                {
                    Description = "Input your API Key to access the endpoints",
                    In = ParameterLocation.Header,
                    Name = "X-API-KEY",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = authOptions.SchemeName
                });
                opt.AddSecurityRequirement(document => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(authOptions.SchemeName, document)] = []
                });
            }
        });
    }

    private static void AddExceptionHandlers(IServiceCollection services)
    {
        services.AddExceptionHandler<HttpTranslatableExceptionHandler>();
        services.AddProblemDetails();
    }
}
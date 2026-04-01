using Microsoft.Extensions.Options;
using TireRecognition.Application;
using TireRecognition.Application.Options;
using TireRecognition.Infrastructure;
using TireRecognition.WebApi;
using TireRecognition.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration, builder.Logging)
    .AddPresentation(builder.Configuration);

var app = builder.Build();
var authOptions = app.Services.GetRequiredService<IOptions<AuthOptions>>().Value;

// Custom middleware
app.UseExceptionHandler();
if (authOptions.Enabled)
{
    app.UseAuthentication();
    app.UseAuthorization();
}

app.AddSwagger();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
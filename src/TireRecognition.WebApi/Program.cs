using TireRecognition.Application;
using TireRecognition.Infrastructure;
using TireRecognition.WebApi;
using TireRecognition.WebApi.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddApplication()
    .AddInfrastructure(builder.Configuration)
    .AddPresentation(builder.Configuration);

var app = builder.Build();

app.UseExceptionHandler();
app.AddSwagger();
app.UseHttpsRedirection();
app.MapControllers();

app.Run();
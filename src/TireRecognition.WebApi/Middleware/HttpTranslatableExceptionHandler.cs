using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using TireRecognition.Shared.Exceptions;

namespace TireRecognition.WebApi.Middleware;

public class HttpTranslatableExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not HttpTranslatableException translatableException)
            return false;
        
        httpContext.Response.StatusCode = translatableException.Code;
        var problemDetails = new ProblemDetails
        {
            Status = translatableException.Code,
            Title = "An error occurred while processing your request",
            Detail = translatableException.Message,
            Instance = httpContext.Request.Path
        };

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
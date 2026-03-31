namespace TireRecognition.WebApi.Extensions;

public static class WebApplicationExtension
{
    public static WebApplication AddSwagger(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        AddRootSwaggerRedirect(app);
        return app;
    }

    private static void AddRootSwaggerRedirect(WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var isRoot = context.Request.Path == "/";
            if (isRoot)
            {
                context.Response.Redirect("/swagger");
                return;
            }

            await next();
        });
    }
}
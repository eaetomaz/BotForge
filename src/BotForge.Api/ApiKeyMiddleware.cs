namespace BotForge.Api;

// Desabilitado quando BotForge:ApiKey não está configurado (uso local/dev).
public class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string HeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        var expectedKey = configuration["BotForge:ApiKey"];
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue(HeaderName, out var providedKey) || providedKey != expectedKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key inválida ou ausente.");
            return;
        }

        await next(context);
    }
}

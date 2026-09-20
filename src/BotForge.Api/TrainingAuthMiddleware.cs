using System.Text;

namespace BotForge.Api;

public class TrainingAuthOptions
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Protege as rotas de treino (/knowledge e /profile) com usuário/senha via Basic Auth.
// O chat (/chat) e a página estática continuam livres — só a área de treino exige login.
public class TrainingAuthMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private static readonly string[] ProtectedPrefixes = ["/knowledge", "/profile"];

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        if (!ProtectedPrefixes.Any(prefix => path.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)))
        {
            await next(context);
            return;
        }

        var expectedUsername = configuration["BotForge:TrainingAuth:Username"];
        var expectedPassword = configuration["BotForge:TrainingAuth:Password"];
        if (string.IsNullOrEmpty(expectedUsername))
        {
            await next(context);
            return;
        }

        if (!TryGetBasicAuthCredentials(context.Request, out var username, out var password) ||
            username != expectedUsername || password != expectedPassword)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Login necessário para acessar a área de treino.");
            return;
        }

        await next(context);
    }

    private static bool TryGetBasicAuthCredentials(HttpRequest request, out string username, out string password)
    {
        username = string.Empty;
        password = string.Empty;

        if (!request.Headers.TryGetValue("Authorization", out var header))
            return false;

        var value = header.ToString();
        if (!value.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return false;

        try
        {
            var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(value["Basic ".Length..]));
            var separatorIndex = decoded.IndexOf(':');
            if (separatorIndex < 0)
                return false;

            username = decoded[..separatorIndex];
            password = decoded[(separatorIndex + 1)..];
            return true;
        }
        catch (FormatException)
        {
            return false;
        }
    }
}

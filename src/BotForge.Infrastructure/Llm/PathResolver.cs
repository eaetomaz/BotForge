namespace BotForge.Infrastructure.Llm;

// Um caminho relativo nos settings é resolvido a partir da pasta do próprio executável,
// não do diretório de trabalho atual — assim o app funciona de onde quer que a pasta seja copiada.
public static class PathResolver
{
    public static string Resolve(string path) =>
        Path.IsPathRooted(path) ? path : Path.Combine(AppContext.BaseDirectory, path);
}

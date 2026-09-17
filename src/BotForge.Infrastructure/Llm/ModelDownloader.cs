using Microsoft.Extensions.Logging;

namespace BotForge.Infrastructure.Llm;

public static class ModelDownloader
{
    public static async Task EnsureModelAsync(string modelPath, string? downloadUrl, ILogger logger, CancellationToken ct = default)
    {
        if (File.Exists(modelPath))
            return;

        if (string.IsNullOrWhiteSpace(downloadUrl))
            throw new InvalidOperationException(
                $"Modelo não encontrado em '{modelPath}' e nenhuma LlamaSharp:ModelDownloadUrl foi configurada.");

        var directory = Path.GetDirectoryName(modelPath);
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Baixa para um arquivo temporário e só promove ao nome final no sucesso — assim uma
        // queda no meio do download não deixa um .gguf corrompido que passe no check acima.
        var tempPath = modelPath + ".download";
        logger.LogInformation("Modelo não encontrado. Baixando de {Url}...", downloadUrl);

        using var http = new HttpClient { Timeout = Timeout.InfiniteTimeSpan };
        using var response = await http.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead, ct);
        response.EnsureSuccessStatusCode();

        var totalBytes = response.Content.Headers.ContentLength ?? -1;
        var lastLoggedPercent = -1;

        await using (var contentStream = await response.Content.ReadAsStreamAsync(ct))
        await using (var fileStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None, 81920, useAsync: true))
        {
            var buffer = new byte[81920];
            long totalRead = 0;
            int read;
            while ((read = await contentStream.ReadAsync(buffer, ct)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, read), ct);
                totalRead += read;

                if (totalBytes <= 0)
                    continue;

                var percent = (int)(totalRead * 100 / totalBytes);
                if (percent / 10 != lastLoggedPercent / 10)
                {
                    logger.LogInformation("Download do modelo: {Percent}% ({DoneMB} MB / {TotalMB} MB)",
                        percent, totalRead / 1024 / 1024, totalBytes / 1024 / 1024);
                    lastLoggedPercent = percent;
                }
            }
        }

        File.Move(tempPath, modelPath, overwrite: true);
        logger.LogInformation("Download do modelo concluído: {Path}", modelPath);
    }
}

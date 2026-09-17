namespace BotForge.Infrastructure.Llm;

public class LlamaSharpOptions
{
    public string ChatModelPath { get; set; } = string.Empty;

    // Se vazio, usa o mesmo arquivo do ChatModelPath.
    public string EmbeddingModelPath { get; set; } = string.Empty;

    // Baixado automaticamente para ChatModelPath (e EmbeddingModelPath, se distinto) quando o
    // arquivo não existe — é isso que torna o app autossuficiente em uma máquina nova.
    public string ModelDownloadUrl { get; set; } = string.Empty;

    public uint ContextSize { get; set; } = 2048;
    public int GpuLayerCount { get; set; } = 0;
}

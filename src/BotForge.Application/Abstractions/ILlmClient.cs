using BotForge.Domain.Entities;

namespace BotForge.Application.Abstractions;

public record ChatTurn(MessageRole Role, string Content);

public interface ILlmClient
{
    Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<ChatTurn> history,
        string userMessage,
        CancellationToken ct = default);

    Task<float[]> EmbedAsync(string text, CancellationToken ct = default);
}

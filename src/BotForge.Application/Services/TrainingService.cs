using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;

namespace BotForge.Application.Services;

public class TrainingService(ILlmClient llm, IKnowledgeRepository knowledge)
{
    public async Task<KnowledgeItem> AddAsync(string content, string? tags, CancellationToken ct = default)
    {
        var embedding = await llm.EmbedAsync(content, ct);
        var now = DateTime.UtcNow;
        var item = new KnowledgeItem
        {
            Content = content,
            Tags = tags,
            Embedding = embedding,
            CreatedAt = now,
            UpdatedAt = now
        };

        return await knowledge.AddAsync(item, ct);
    }

    public Task<IReadOnlyList<KnowledgeItem>> ListAsync(CancellationToken ct = default) =>
        knowledge.GetAllAsync(ct);

    public async Task<KnowledgeItem?> UpdateAsync(int id, string content, string? tags, CancellationToken ct = default)
    {
        var existing = await knowledge.GetByIdAsync(id, ct);
        if (existing is null)
            return null;

        existing.Content = content;
        existing.Tags = tags;
        existing.Embedding = await llm.EmbedAsync(content, ct);
        existing.UpdatedAt = DateTime.UtcNow;

        await knowledge.UpdateAsync(existing, ct);
        return existing;
    }

    public Task<bool> DeleteAsync(int id, CancellationToken ct = default) =>
        knowledge.DeleteAsync(id, ct);
}

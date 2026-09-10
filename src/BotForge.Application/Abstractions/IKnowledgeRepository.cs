using BotForge.Domain.Entities;

namespace BotForge.Application.Abstractions;

public interface IKnowledgeRepository
{
    Task<KnowledgeItem> AddAsync(KnowledgeItem item, CancellationToken ct = default);
    Task<IReadOnlyList<KnowledgeItem>> GetAllAsync(CancellationToken ct = default);
    Task<KnowledgeItem?> GetByIdAsync(int id, CancellationToken ct = default);
    Task UpdateAsync(KnowledgeItem item, CancellationToken ct = default);
    Task<bool> DeleteAsync(int id, CancellationToken ct = default);
}

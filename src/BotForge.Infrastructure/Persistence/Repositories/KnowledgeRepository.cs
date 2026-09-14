using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BotForge.Infrastructure.Persistence.Repositories;

public class KnowledgeRepository(BotForgeDbContext db) : IKnowledgeRepository
{
    public async Task<KnowledgeItem> AddAsync(KnowledgeItem item, CancellationToken ct = default)
    {
        db.KnowledgeItems.Add(item);
        await db.SaveChangesAsync(ct);
        return item;
    }

    public async Task<IReadOnlyList<KnowledgeItem>> GetAllAsync(CancellationToken ct = default) =>
        await db.KnowledgeItems.AsNoTracking().ToListAsync(ct);

    public Task<KnowledgeItem?> GetByIdAsync(int id, CancellationToken ct = default) =>
        db.KnowledgeItems.FirstOrDefaultAsync(k => k.Id == id, ct);

    public async Task UpdateAsync(KnowledgeItem item, CancellationToken ct = default)
    {
        db.KnowledgeItems.Update(item);
        await db.SaveChangesAsync(ct);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct = default)
    {
        var existing = await db.KnowledgeItems.FirstOrDefaultAsync(k => k.Id == id, ct);
        if (existing is null)
            return false;

        db.KnowledgeItems.Remove(existing);
        await db.SaveChangesAsync(ct);
        return true;
    }
}

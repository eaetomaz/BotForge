using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BotForge.Infrastructure.Persistence.Repositories;

public class ConversationRepository(BotForgeDbContext db) : IConversationRepository
{
    public async Task<Conversation> GetOrCreateAsync(string externalUserId, CancellationToken ct = default)
    {
        var existing = await db.Conversations.FirstOrDefaultAsync(c => c.ExternalUserId == externalUserId, ct);
        if (existing is not null)
            return existing;

        var conversation = new Conversation
        {
            Id = Guid.NewGuid(),
            ExternalUserId = externalUserId,
            CreatedAt = DateTime.UtcNow
        };

        db.Conversations.Add(conversation);
        await db.SaveChangesAsync(ct);
        return conversation;
    }

    public async Task<IReadOnlyList<Message>> GetRecentMessagesAsync(Guid conversationId, int count, CancellationToken ct = default) =>
        await db.Messages
            .Where(m => m.ConversationId == conversationId)
            .OrderByDescending(m => m.CreatedAt)
            .Take(count)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync(ct);

    public async Task AddMessageAsync(Guid conversationId, MessageRole role, string content, CancellationToken ct = default)
    {
        db.Messages.Add(new Message
        {
            Id = Guid.NewGuid(),
            ConversationId = conversationId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }

    public async Task ClearMessagesAsync(Guid conversationId, CancellationToken ct = default)
    {
        await db.Messages.Where(m => m.ConversationId == conversationId).ExecuteDeleteAsync(ct);
    }
}

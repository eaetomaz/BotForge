using BotForge.Domain.Entities;

namespace BotForge.Application.Abstractions;

public interface IConversationRepository
{
    Task<Conversation> GetOrCreateAsync(string externalUserId, CancellationToken ct = default);
    Task<IReadOnlyList<Message>> GetRecentMessagesAsync(Guid conversationId, int count, CancellationToken ct = default);
    Task AddMessageAsync(Guid conversationId, MessageRole role, string content, CancellationToken ct = default);
    Task ClearMessagesAsync(Guid conversationId, CancellationToken ct = default);
}

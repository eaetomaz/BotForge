using BotForge.Domain.Entities;

namespace BotForge.Application.Abstractions;

public interface IBotProfileRepository
{
    Task<BotProfile> GetActiveAsync(CancellationToken ct = default);
    Task<BotProfile> SaveAsync(BotProfile profile, CancellationToken ct = default);
}

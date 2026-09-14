using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BotForge.Infrastructure.Persistence.Repositories;

public class BotProfileRepository(BotForgeDbContext db) : IBotProfileRepository
{
    public async Task<BotProfile> GetActiveAsync(CancellationToken ct = default)
    {
        var profile = await db.BotProfiles.FirstOrDefaultAsync(p => p.Id == BotProfile.ActiveProfileId, ct);
        if (profile is not null)
            return profile;

        profile = new BotProfile();
        db.BotProfiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return profile;
    }

    public async Task<BotProfile> SaveAsync(BotProfile profile, CancellationToken ct = default)
    {
        var existing = await db.BotProfiles.FirstOrDefaultAsync(p => p.Id == BotProfile.ActiveProfileId, ct);
        if (existing is null)
        {
            db.BotProfiles.Add(profile);
        }
        else
        {
            existing.Name = profile.Name;
            existing.Purpose = profile.Purpose;
            existing.SystemPromptInstructions = profile.SystemPromptInstructions;
            existing.StyleExamples = profile.StyleExamples;
        }

        await db.SaveChangesAsync(ct);
        return existing ?? profile;
    }
}

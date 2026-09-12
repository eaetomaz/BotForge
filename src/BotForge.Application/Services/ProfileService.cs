using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;

namespace BotForge.Application.Services;

public class ProfileService(IBotProfileRepository profiles)
{
    public Task<BotProfile> GetAsync(CancellationToken ct = default) =>
        profiles.GetActiveAsync(ct);

    public async Task<BotProfile> SaveAsync(string name, string purpose, string systemPromptInstructions, string? styleExamples, CancellationToken ct = default)
    {
        var profile = new BotProfile
        {
            Id = BotProfile.ActiveProfileId,
            Name = name,
            Purpose = purpose,
            SystemPromptInstructions = systemPromptInstructions,
            StyleExamples = styleExamples
        };

        return await profiles.SaveAsync(profile, ct);
    }
}

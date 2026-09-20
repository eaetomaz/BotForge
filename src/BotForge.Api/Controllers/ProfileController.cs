using BotForge.Application.Services;
using BotForge.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BotForge.Api.Controllers;

public record BotProfileDto(string Name, string Purpose, string SystemPromptInstructions, string? StyleExamples)
{
    public static BotProfileDto From(BotProfile profile) =>
        new(profile.Name, profile.Purpose, profile.SystemPromptInstructions, profile.StyleExamples);
}

[ApiController]
[Route("profile")]
public class ProfileController(ProfileService profileService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<BotProfileDto>> Get(CancellationToken ct)
    {
        var profile = await profileService.GetAsync(ct);
        return Ok(BotProfileDto.From(profile));
    }

    [HttpPut]
    public async Task<ActionResult<BotProfileDto>> Save(BotProfileDto request, CancellationToken ct)
    {
        var profile = await profileService.SaveAsync(
            request.Name, request.Purpose, request.SystemPromptInstructions, request.StyleExamples, ct);
        return Ok(BotProfileDto.From(profile));
    }
}

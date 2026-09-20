using BotForge.Application.Services;
using BotForge.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BotForge.Api.Controllers;

public record KnowledgeItemDto(int Id, string Content, string? Tags, DateTime CreatedAt, DateTime UpdatedAt)
{
    public static KnowledgeItemDto From(KnowledgeItem item) =>
        new(item.Id, item.Content, item.Tags, item.CreatedAt, item.UpdatedAt);
}

public record UpsertKnowledgeItemDto(string Content, string? Tags);

[ApiController]
[Route("knowledge")]
public class KnowledgeController(TrainingService trainingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<KnowledgeItemDto>>> List(CancellationToken ct)
    {
        var items = await trainingService.ListAsync(ct);
        return Ok(items.Select(KnowledgeItemDto.From));
    }

    [HttpPost]
    public async Task<ActionResult<KnowledgeItemDto>> Add(UpsertKnowledgeItemDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content é obrigatório.");

        var item = await trainingService.AddAsync(request.Content, request.Tags, ct);
        return Ok(KnowledgeItemDto.From(item));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<KnowledgeItemDto>> Update(int id, UpsertKnowledgeItemDto request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Content))
            return BadRequest("Content é obrigatório.");

        var item = await trainingService.UpdateAsync(id, request.Content, request.Tags, ct);
        return item is null ? NotFound() : Ok(KnowledgeItemDto.From(item));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var deleted = await trainingService.DeleteAsync(id, ct);
        return deleted ? NoContent() : NotFound();
    }
}

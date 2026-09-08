namespace BotForge.Domain.Entities;

public class KnowledgeItem
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? Tags { get; set; }
    public float[] Embedding { get; set; } = [];
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

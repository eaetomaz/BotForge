namespace BotForge.Domain.Entities;

public class Conversation
{
    public Guid Id { get; set; }
    public string ExternalUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<Message> Messages { get; set; } = [];
}

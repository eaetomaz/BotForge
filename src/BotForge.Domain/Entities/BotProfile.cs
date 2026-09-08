namespace BotForge.Domain.Entities;

public class BotProfile
{
    public const int ActiveProfileId = 1;

    public int Id { get; set; } = ActiveProfileId;
    public string Name { get; set; } = "Bot";
    public string Purpose { get; set; } = "Assistente geral de respostas.";
    public string SystemPromptInstructions { get; set; } =
        "Responda apenas com base no conhecimento fornecido. Se não souber, diga que não tem essa informação.";
    public string? StyleExamples { get; set; }
}

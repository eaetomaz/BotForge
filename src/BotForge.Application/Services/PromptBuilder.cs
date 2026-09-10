using System.Text;
using BotForge.Domain.Entities;

namespace BotForge.Application.Services;

public static class PromptBuilder
{
    public static string Build(BotProfile profile, IReadOnlyList<KnowledgeItem> relevantKnowledge, bool hasStyleExampleTurns)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"Você é {profile.Name}. Propósito: {profile.Purpose}");
        sb.AppendLine(profile.SystemPromptInstructions);

        if (relevantKnowledge.Count > 0)
        {
            sb.AppendLine("Conhecimento relevante para esta pergunta:");
            foreach (var item in relevantKnowledge)
                sb.AppendLine($"- {item.Content}");
        }

        if (!string.IsNullOrWhiteSpace(profile.StyleExamples))
        {
            // Colocado por último (mais próximo da geração) para pesar mais na resposta final.
            sb.AppendLine(hasStyleExampleTurns
                ? "Siga exatamente o tom demonstrado nos exemplos de conversa a seguir, em TODAS as respostas."
                : $"Tom de voz obrigatório em TODA resposta, inclusive quando não souber algo: {profile.StyleExamples}");
        }

        return sb.ToString();
    }
}

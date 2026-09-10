using System.Text.RegularExpressions;
using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;

namespace BotForge.Application.Services;

// Modelos pequenos seguem exemplos de diálogo (few-shot) muito melhor do que uma instrução
// abstrata de tom ("responda de forma brincalhona"). Se o usuário escrever pares de
// Pergunta/Resposta no campo de estilo, nós os transformamos em turnos reais de conversa.
public static class StyleExampleParser
{
    private static readonly Regex QuestionLine = new(
        @"^\s*(?:Pergunta|P|Usu[aá]rio|Cliente)\s*:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex AnswerLine = new(
        @"^\s*(?:Resposta|R|Bot|Atendente|Voc[eê])\s*:\s*(.+)$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public static IReadOnlyList<ChatTurn> Parse(string? styleExamples)
    {
        if (string.IsNullOrWhiteSpace(styleExamples))
            return [];

        var turns = new List<ChatTurn>();
        string? pendingQuestion = null;

        foreach (var rawLine in styleExamples.Split('\n'))
        {
            var line = rawLine.TrimEnd('\r');

            var questionMatch = QuestionLine.Match(line);
            if (questionMatch.Success)
            {
                pendingQuestion = questionMatch.Groups[1].Value.Trim();
                continue;
            }

            var answerMatch = AnswerLine.Match(line);
            if (answerMatch.Success && pendingQuestion is not null)
            {
                turns.Add(new ChatTurn(MessageRole.User, pendingQuestion));
                turns.Add(new ChatTurn(MessageRole.Assistant, answerMatch.Groups[1].Value.Trim()));
                pendingQuestion = null;
            }
        }

        return turns;
    }
}

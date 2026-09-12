using System.Text.RegularExpressions;
using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;

namespace BotForge.Application.Services;

public record ChatResult(string Reply, bool Cleared);

public class ChatService(
    ILlmClient llm,
    IKnowledgeRepository knowledge,
    IConversationRepository conversations,
    IBotProfileRepository profiles)
{
    private const int RelevantKnowledgeCount = 4;
    private const int RecentMessagesCount = 6;
    private const string ClearedReply = "Prontinho, esqueci nossa conversa anterior! Pode perguntar de novo.";

    // Detecta o pedido em texto livre (ex: "pode limpar o chat?", "esquece nossa conversa") em vez
    // de depender do LLM entender isso como um comando — mais rápido e sempre confiável.
    private static readonly Regex ClearChatIntent = new(
        @"\b(limp[ae]r?|apag[ae]r?|esque[cç][ae]r?|reinici[ae]r?|zer[ae]r?)\b[^\n]{0,25}\b(chat|conversa|hist[oó]rico|mem[oó]ria)\b|^\s*(novo\s+chat|nova\s+conversa)\s*[.!?]*\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    public async Task<ChatResult> AskAsync(string externalConversationId, string message, CancellationToken ct = default)
    {
        if (ClearChatIntent.IsMatch(message))
        {
            await ClearAsync(externalConversationId, ct);
            return new ChatResult(ClearedReply, Cleared: true);
        }

        var profile = await profiles.GetActiveAsync(ct);
        var conversation = await conversations.GetOrCreateAsync(externalConversationId, ct);
        var history = await conversations.GetRecentMessagesAsync(conversation.Id, RecentMessagesCount, ct);
        var relevantKnowledge = await RetrieveRelevantKnowledgeAsync(message, ct);

        var styleExampleTurns = StyleExampleParser.Parse(profile.StyleExamples);
        var systemPrompt = PromptBuilder.Build(profile, relevantKnowledge, styleExampleTurns.Count > 0);
        var chatHistory = styleExampleTurns
            .Concat(history.Select(m => new ChatTurn(m.Role, m.Content)))
            .ToList();

        var reply = await llm.CompleteAsync(systemPrompt, chatHistory, message, ct);

        await conversations.AddMessageAsync(conversation.Id, MessageRole.User, message, ct);
        await conversations.AddMessageAsync(conversation.Id, MessageRole.Assistant, reply, ct);

        return new ChatResult(reply, Cleared: false);
    }

    public async Task ClearAsync(string externalConversationId, CancellationToken ct = default)
    {
        var conversation = await conversations.GetOrCreateAsync(externalConversationId, ct);
        await conversations.ClearMessagesAsync(conversation.Id, ct);
    }

    private async Task<IReadOnlyList<KnowledgeItem>> RetrieveRelevantKnowledgeAsync(string message, CancellationToken ct)
    {
        var all = await knowledge.GetAllAsync(ct);
        if (all.Count == 0)
            return [];

        var queryEmbedding = await llm.EmbedAsync(message, ct);

        return all
            .Select(item => (Item: item, Score: VectorMath.CosineSimilarity(queryEmbedding, item.Embedding)))
            .OrderByDescending(x => x.Score)
            .Take(RelevantKnowledgeCount)
            .Where(x => x.Score > 0)
            .Select(x => x.Item)
            .ToList();
    }
}

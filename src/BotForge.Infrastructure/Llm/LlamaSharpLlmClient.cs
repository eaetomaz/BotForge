using System.Text;
using System.Text.RegularExpressions;
using BotForge.Application.Abstractions;
using BotForge.Domain.Entities;
using LLama;
using LLama.Common;
using LLama.Native;
using LLama.Sampling;
using LLama.Transformers;
using Microsoft.Extensions.Options;

namespace BotForge.Infrastructure.Llm;

public sealed class LlamaSharpLlmClient : ILlmClient, IDisposable
{
    // Modelos pequenos costumam abrir a resposta com um rótulo de papel (ex: "Resposta:", "Atendente:").
    private static readonly Regex LeadingRolePrefix = new(@"^(?:\s*[A-Za-zÀ-ÿ]{2,20}:\s*){1,2}", RegexOptions.Compiled);

    private readonly LLamaWeights _chatWeights;
    private readonly LLamaContext _chatContext;
    private readonly LLamaWeights _embedWeights;
    private readonly LLamaEmbedder _embedder;
    private readonly SemaphoreSlim _chatLock = new(1, 1);
    private readonly SemaphoreSlim _embedLock = new(1, 1);

    public LlamaSharpLlmClient(IOptions<LlamaSharpOptions> options)
    {
        var opts = options.Value;
        var chatModelPath = PathResolver.Resolve(opts.ChatModelPath);

        var chatParams = new ModelParams(chatModelPath)
        {
            ContextSize = opts.ContextSize,
            GpuLayerCount = opts.GpuLayerCount
        };
        _chatWeights = LLamaWeights.LoadFromFile(chatParams);
        _chatContext = _chatWeights.CreateContext(chatParams);

        // Modelo de embedding carregado separadamente (mesmo arquivo, por padrão) porque
        // PoolingType é uma configuração de carga do modelo, incompatível com o contexto de chat.
        var embeddingModelPath = string.IsNullOrWhiteSpace(opts.EmbeddingModelPath)
            ? chatModelPath
            : PathResolver.Resolve(opts.EmbeddingModelPath);
        var embedParams = new ModelParams(embeddingModelPath)
        {
            ContextSize = opts.ContextSize,
            GpuLayerCount = opts.GpuLayerCount,
            PoolingType = LLamaPoolingType.Mean
        };
        _embedWeights = LLamaWeights.LoadFromFile(embedParams);
        _embedder = new LLamaEmbedder(_embedWeights, embedParams);
    }

    public async Task<string> CompleteAsync(
        string systemPrompt,
        IReadOnlyList<ChatTurn> history,
        string userMessage,
        CancellationToken ct = default)
    {
        await _chatLock.WaitAsync(ct);
        try
        {
            // Modelos pequenos em CPU ocasionalmente encerram a geração quase de imediato,
            // produzindo uma resposta vazia; algumas tentativas com seed diferente costumam resolver.
            const int maxAttempts = 2;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                var reply = await GenerateOnceAsync(systemPrompt, history, userMessage, ct);
                if (!string.IsNullOrWhiteSpace(reply))
                    return reply;
            }

            return "Desculpe, pode reformular a pergunta?";
        }
        finally
        {
            _chatLock.Release();
        }
    }

    private async Task<string> GenerateOnceAsync(
        string systemPrompt,
        IReadOnlyList<ChatTurn> history,
        string userMessage,
        CancellationToken ct)
    {
        // O contexto é compartilhado entre requisições (protegido pelo semáforo), então o
        // cache de tokens de uma conversa anterior precisa ser limpo antes de cada chamada.
        _chatContext.NativeHandle.MemoryClear(true);
        var executor = new InteractiveExecutor(_chatContext);

        var chatHistory = new ChatHistory();
        chatHistory.AddMessage(AuthorRole.System, systemPrompt);
        foreach (var turn in history)
        {
            var role = turn.Role == MessageRole.User ? AuthorRole.User : AuthorRole.Assistant;
            chatHistory.AddMessage(role, turn.Content);
        }

        var session = new ChatSession(executor, chatHistory);
        // Aplica o template de chat nativo do modelo (ex.: tokens especiais do Llama 3) em vez de
        // texto puro "Role: conteúdo" — sem isso o modelo às vezes "confunde" e devolve "User:" cru.
        session.WithHistoryTransform(new PromptTemplateTransformer(_chatWeights, withAssistant: true));
        var antiPrompts = new List<string> { "User:" };
        session.WithOutputTransform(new LLamaTransforms.KeywordTextOutputStreamTransform(antiPrompts, redundancyLength: 5));
        var inferenceParams = new InferenceParams
        {
            MaxTokens = 300,
            AntiPrompts = antiPrompts,
            // DefaultSamplingPipeline usa uma seed fixa por padrão, o que tornaria a mesma
            // pergunta sempre determinística (inclusive gerando resposta vazia). Randomizamos aqui.
            SamplingPipeline = new DefaultSamplingPipeline
            {
                Temperature = 0.7f,
                RepeatPenalty = 1.2f,
                Seed = unchecked((uint)Random.Shared.Next())
            }
        };

        var sb = new StringBuilder();
        await foreach (var token in session.ChatAsync(
            new ChatHistory.Message(AuthorRole.User, userMessage), inferenceParams, ct))
        {
            sb.Append(token);
        }

        var reply = sb.ToString();
        foreach (var antiPrompt in antiPrompts)
        {
            var index = reply.IndexOf(antiPrompt, StringComparison.Ordinal);
            if (index >= 0)
                reply = reply[..index];
        }

        return LeadingRolePrefix.Replace(reply.Trim(), "").Trim();
    }

    public async Task<float[]> EmbedAsync(string text, CancellationToken ct = default)
    {
        await _embedLock.WaitAsync(ct);
        try
        {
            var embeddings = await _embedder.GetEmbeddings(text, ct);
            return embeddings.Count > 0 ? embeddings[0] : [];
        }
        finally
        {
            _embedLock.Release();
        }
    }

    public void Dispose()
    {
        _chatLock.Dispose();
        _embedLock.Dispose();
        _chatContext.Dispose();
        _chatWeights.Dispose();
        _embedWeights.Dispose();
    }
}

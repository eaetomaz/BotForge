using BotForge.Application.Abstractions;
using BotForge.Infrastructure.Llm;
using BotForge.Infrastructure.Persistence;
using BotForge.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BotForge.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBotForgeInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BotForge") ?? "Data Source=botforge.db";
        services.AddDbContext<BotForgeDbContext>(options => options.UseSqlite(connectionString));

        services.Configure<LlamaSharpOptions>(configuration.GetSection("LlamaSharp"));
        services.AddSingleton<ILlmClient, LlamaSharpLlmClient>();

        services.AddScoped<IKnowledgeRepository, KnowledgeRepository>();
        services.AddScoped<IConversationRepository, ConversationRepository>();
        services.AddScoped<IBotProfileRepository, BotProfileRepository>();

        return services;
    }
}

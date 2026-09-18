using BotForge.Api;
using BotForge.Application.Services;
using BotForge.Domain.Entities;
using BotForge.Infrastructure;
using BotForge.Infrastructure.Llm;
using BotForge.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddBotForgeInfrastructure(builder.Configuration);
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<TrainingService>();
builder.Services.AddScoped<ProfileService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BotForgeDbContext>();
    db.Database.EnsureCreated();
    if (!db.BotProfiles.Any(p => p.Id == BotProfile.ActiveProfileId))
        db.BotProfiles.Add(new BotProfile());
    db.SaveChanges();
}

// Torna o app autossuficiente: se o modelo não estiver na máquina, baixa antes de subir o servidor.
{
    var llamaOptions = app.Services.GetRequiredService<IOptions<LlamaSharpOptions>>().Value;
    var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();

    var chatModelPath = PathResolver.Resolve(llamaOptions.ChatModelPath);
    await ModelDownloader.EnsureModelAsync(chatModelPath, llamaOptions.ModelDownloadUrl, startupLogger);

    if (!string.IsNullOrWhiteSpace(llamaOptions.EmbeddingModelPath))
    {
        var embeddingModelPath = PathResolver.Resolve(llamaOptions.EmbeddingModelPath);
        if (embeddingModelPath != chatModelPath)
            await ModelDownloader.EnsureModelAsync(embeddingModelPath, llamaOptions.ModelDownloadUrl, startupLogger);
    }
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();
app.UseMiddleware<TrainingAuthMiddleware>();
app.UseAuthorization();

app.MapControllers();

app.Run();

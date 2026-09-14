using BotForge.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace BotForge.Infrastructure.Persistence;

public class BotForgeDbContext(DbContextOptions<BotForgeDbContext> options) : DbContext(options)
{
    public DbSet<KnowledgeItem> KnowledgeItems => Set<KnowledgeItem>();
    public DbSet<Conversation> Conversations => Set<Conversation>();
    public DbSet<Message> Messages => Set<Message>();
    public DbSet<BotProfile> BotProfiles => Set<BotProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var embeddingComparer = new ValueComparer<float[]>(
            (a, b) => (a ?? Array.Empty<float>()).SequenceEqual(b ?? Array.Empty<float>()),
            v => v.Aggregate(0, (hash, f) => HashCode.Combine(hash, f)),
            v => v.ToArray());

        modelBuilder.Entity<KnowledgeItem>(e =>
        {
            e.Property(k => k.Embedding)
                .HasConversion(
                    v => FloatsToBytes(v),
                    v => BytesToFloats(v))
                .Metadata.SetValueComparer(embeddingComparer);
        });

        modelBuilder.Entity<Message>(e =>
        {
            e.Property(m => m.Role).HasConversion<string>();
        });

        modelBuilder.Entity<Conversation>(e =>
        {
            e.HasMany(c => c.Messages).WithOne().HasForeignKey(m => m.ConversationId);
        });
    }

    private static byte[] FloatsToBytes(float[] floats)
    {
        var bytes = new byte[floats.Length * sizeof(float)];
        Buffer.BlockCopy(floats, 0, bytes, 0, bytes.Length);
        return bytes;
    }

    private static float[] BytesToFloats(byte[] bytes)
    {
        var floats = new float[bytes.Length / sizeof(float)];
        Buffer.BlockCopy(bytes, 0, floats, 0, bytes.Length);
        return floats;
    }
}

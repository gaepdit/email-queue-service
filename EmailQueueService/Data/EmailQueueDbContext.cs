using EmailQueueService.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EmailQueueService.Data;

public class EmailQueueDbContext(DbContextOptions<EmailQueueDbContext> options) : DbContext(options)
{
    public DbSet<EmailTask> EmailTasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailTask>().HasKey(e => e.Id);

        modelBuilder.Entity<EmailTask>()
            .Property(e => e.Recipients)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null!),
                v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null!) ?? new List<string>()
            )
            .HasMaxLength(7000);
    }
}

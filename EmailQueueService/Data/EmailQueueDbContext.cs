using EmailQueueService.Models;
using Microsoft.EntityFrameworkCore;

namespace EmailQueueService.Data;

public class EmailQueueDbContext(DbContextOptions<EmailQueueDbContext> options) : DbContext(options)
{
    public DbSet<EmailTask> EmailTasks { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<EmailTask>()
            .HasKey(e => e.Id);

        modelBuilder.Entity<EmailTask>()
            .Property(e => e.Status)
            .IsRequired();

        modelBuilder.Entity<EmailTask>()
            .Property(e => e.EmailAddress)
            .IsRequired();
    }
}

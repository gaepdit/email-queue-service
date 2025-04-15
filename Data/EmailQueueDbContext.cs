using Microsoft.EntityFrameworkCore;
using EmailQueueService.Models;

namespace EmailQueueService.Data;

public class EmailQueueDbContext : DbContext
{
    public EmailQueueDbContext(DbContextOptions<EmailQueueDbContext> options) : base(options)
    {
    }

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
using Microsoft.EntityFrameworkCore;
using TaskManager.Attachments.Domain.Entities;

namespace TaskManager.Attachments.Infrastructure.Data;

/// <summary>
/// Database context for attachments module (separate from legacy ApplicationDbContext).
/// </summary>
public class AttachmentsDbContext : DbContext
{
    public AttachmentsDbContext(DbContextOptions<AttachmentsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Attachment> Attachments => Set<Attachment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply entity configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AttachmentsDbContext).Assembly);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Attachments.Domain.Entities;

namespace TaskManager.Attachments.Infrastructure.Data.Configurations;

/// <summary>
/// EF Core entity configuration for Attachment.
/// </summary>
public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        // Table name
        builder.ToTable("Attachments");

        // Primary Key
        builder.HasKey(a => a.Id);

        // Properties
        builder.Property(a => a.Id)
            .IsRequired()
            .ValueGeneratedNever(); // Guid generated in application layer

        builder.Property(a => a.TaskId)
            .IsRequired();

        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(a => a.FileSize)
            .IsRequired();

        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.UploadedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(a => a.UploadedByUserId)
            .IsRequired();

        builder.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(a => a.TaskId)
            .HasDatabaseName("IX_Attachments_TaskId");

        builder.HasIndex(a => a.UploadedByUserId)
            .HasDatabaseName("IX_Attachments_UploadedByUserId");

        builder.HasIndex(a => a.UploadedAt)
            .HasDatabaseName("IX_Attachments_UploadedAt");

        builder.HasIndex(a => a.StoragePath)
            .IsUnique()
            .HasDatabaseName("IX_Attachments_StoragePath_Unique");

        // Check constraints
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Attachments_FileSize",
            "[FileSize] > 0 AND [FileSize] <= 10485760"));
    }
}

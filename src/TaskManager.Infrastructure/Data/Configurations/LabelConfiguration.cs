using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Data.Configurations
{
    /// <summary>
    /// Configuración de la entidad Label.
    /// </summary>
    public class LabelConfiguration : IEntityTypeConfiguration<Label>
    {
        public void Configure(EntityTypeBuilder<Label> builder)
        {
            builder.ToTable("Labels");

            builder.HasKey(l => l.Id);

            builder.Property(l => l.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(l => l.Color)
                .HasMaxLength(7)
                .HasDefaultValue("#6c757d");

            // Índices
            builder.HasIndex(l => l.Name);
            builder.HasIndex(l => l.IsActive);
        }
    }
}

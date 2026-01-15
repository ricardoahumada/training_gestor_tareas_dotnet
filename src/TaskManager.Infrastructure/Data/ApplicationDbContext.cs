using Microsoft.EntityFrameworkCore;
using TaskManager.Domain.Entities;
using TaskManager.Infrastructure.Data.Configurations;

namespace TaskManager.Infrastructure.Data
{
    /// <summary>
    /// Contexto de base de datos para Entity Framework Core.
    /// </summary>
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Domain.Entities.Task> Tasks => Set<Domain.Entities.Task>();
        public DbSet<Project> Projects => Set<Project>();
        public DbSet<Label> Labels => Set<Label>();
        public DbSet<User> Users => Set<User>();
        public DbSet<TaskLabel> TaskLabels => Set<TaskLabel>();
        public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar configuraciones de entidades
            modelBuilder.ApplyConfiguration(new TaskConfiguration());
            modelBuilder.ApplyConfiguration(new ProjectConfiguration());
            modelBuilder.ApplyConfiguration(new LabelConfiguration());
            modelBuilder.ApplyConfiguration(new UserConfiguration());

            // Configuración de la relación muchos a muchos Task-Label
            modelBuilder.Entity<TaskLabel>()
                .HasKey(tl => new { tl.TaskId, tl.LabelId });

            modelBuilder.Entity<TaskLabel>()
                .HasOne(tl => tl.Task)
                .WithMany(t => t.TaskLabels)
                .HasForeignKey(tl => tl.TaskId);

            modelBuilder.Entity<TaskLabel>()
                .HasOne(tl => tl.Label)
                .WithMany(l => l.TaskLabels)
                .HasForeignKey(tl => tl.LabelId);

            // Configuración de ProjectMember
            modelBuilder.Entity<ProjectMember>()
                .HasKey(pm => new { pm.ProjectId, pm.UserId });

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(pm => pm.ProjectId);

            modelBuilder.Entity<ProjectMember>()
                .HasOne(pm => pm.User)
                .WithMany(u => u.ProjectMemberships)
                .HasForeignKey(pm => pm.UserId);

            // Seed data para etiquetas
            modelBuilder.Entity<Label>().HasData(
                new Label { Id = Guid.Parse("11111111-1111-1111-1111-111111111111"), Name = "Urgente", Color = "#dc3545", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Label { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Name = "Importante", Color = "#ffc107", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Label { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Name = "Desarrollo", Color = "#0d6efd", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Label { Id = Guid.Parse("44444444-4444-4444-4444-444444444444"), Name = "Diseño", Color = "#6f42c1", IsActive = true, CreatedAt = DateTime.UtcNow },
                new Label { Id = Guid.Parse("55555555-5555-5555-5555-555555555555"), Name = "Documentación", Color = "#20c997", IsActive = true, CreatedAt = DateTime.UtcNow }
            );

            // Seed data para usuario admin
            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                    Username = "admin",
                    Email = "admin@taskmanager.com",
                    PasswordHash = "admin123", // Se hasheará en producción
                    Role = Domain.Enums.UserRole.Admin,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                }
            );
        }
    }
}

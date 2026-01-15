using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// Representa un usuario del sistema.
    /// </summary>
    public class User
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Nombre de usuario único.
        /// </summary>
        public string Username { get; set; } = string.Empty;
        
        /// <summary>
        /// Correo electrónico del usuario.
        /// </summary>
        public string Email { get; set; } = string.Empty;
        
        /// <summary>
        /// Contraseña hasheada.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;
        
        /// <summary>
        /// Rol del usuario en el sistema.
        /// </summary>
        public UserRole Role { get; set; } = UserRole.User;
        
        /// <summary>
        /// Indica si el usuario está activo.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Token de refresco para JWT.
        /// </summary>
        public string? RefreshToken { get; set; }
        
        /// <summary>
        /// Fecha de expiración del token de refresco.
        /// </summary>
        public DateTime? RefreshTokenExpiry { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Tareas asignadas al usuario.
        /// </summary>
        public ICollection<Task> AssignedTasks { get; set; } = new List<Task>();
        
        /// <summary>
        /// Tareas creadas por el usuario.
        /// </summary>
        public ICollection<Task> CreatedTasks { get; set; } = new List<Task>();
        
        /// <summary>
        /// Proyectos propiedad del usuario.
        /// </summary>
        public ICollection<Project> OwnedProjects { get; set; } = new List<Project>();
        
        /// <summary>
        /// Membresías de proyectos del usuario.
        /// </summary>
        public ICollection<ProjectMember> ProjectMemberships { get; set; } = new List<ProjectMember>();
    }
    
    /// <summary>
    /// Representa la membresía de un usuario en un proyecto.
    /// </summary>
    public class ProjectMember
    {
        public Guid ProjectId { get; set; }
        public Project Project { get; set; } = null!;
        
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        
        public string Role { get; set; } = "Member";
        
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}

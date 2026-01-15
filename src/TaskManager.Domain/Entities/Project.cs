using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// Representa un proyecto que agrupa tareas relacionadas.
    /// </summary>
    public class Project
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Nombre del proyecto.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Descripción del propósito y alcance del proyecto.
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Color identificador para visualización.
        /// </summary>
        public string Color { get; set; } = "#007bff";
        
        /// <summary>
        /// Identificador del propietario del proyecto.
        /// </summary>
        public Guid OwnerId { get; set; }
        
        /// <summary>
        /// Usuario propietario del proyecto.
        /// </summary>
        public User Owner { get; set; } = null!;
        
        /// <summary>
        /// Tareas pertenecientes al proyecto.
        /// </summary>
        public ICollection<Task> Tasks { get; set; } = new List<Task>();
        
        /// <summary>
        /// Miembros con acceso al proyecto.
        /// </summary>
        public ICollection<ProjectMember> Members { get; set; } = new List<ProjectMember>();
        
        /// <summary>
        /// Indica si el proyecto está activo.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}

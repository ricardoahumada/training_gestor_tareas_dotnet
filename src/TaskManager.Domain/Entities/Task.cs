using System.Collections.Generic;
using TaskManager.Domain.Enums;

namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// Representa una tarea individual en el sistema.
    /// </summary>
    public class Task
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Título breve que identifica la tarea.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        
        /// <summary>
        /// Descripción detallada de los requisitos de la tarea.
        /// </summary>
        public string? Description { get; set; }
        
        /// <summary>
        /// Estado actual de la tarea en el workflow.
        /// </summary>
        public Enums.TaskStatus Status { get; set; } = Enums.TaskStatus.Pending;
        
        /// <summary>
        /// Nivel de prioridad para ordenamiento y atención.
        /// </summary>
        public Enums.TaskPriority Priority { get; set; } = Enums.TaskPriority.Medium;
        
        /// <summary>
        /// Fecha límite para completitud de la tarea.
        /// </summary>
        public DateTime? DueDate { get; set; }
        
        /// <summary>
        /// Fecha de completitud real de la tarea.
        /// </summary>
        public DateTime? CompletedAt { get; set; }
        
        /// <summary>
        /// Identificador del usuario responsable de la tarea.
        /// </summary>
        public Guid? AssignedUserId { get; set; }
        
        /// <summary>
        /// Usuario asignado a la tarea.
        /// </summary>
        public User? AssignedUser { get; set; }
        
        /// <summary>
        /// Identificador del usuario que creó la tarea.
        /// </summary>
        public Guid CreatedByUserId { get; set; }
        
        /// <summary>
        /// Usuario que creó la tarea.
        /// </summary>
        public User CreatedByUser { get; set; } = null!;
        
        /// <summary>
        /// Identificador del proyecto al que pertenece la tarea.
        /// </summary>
        public Guid? ProjectId { get; set; }
        
        /// <summary>
        /// Proyecto contenedor de la tarea.
        /// </summary>
        public Project? Project { get; set; }
        
        /// <summary>
        /// Etiquetas asociadas a la tarea para categorización.
        /// </summary>
        public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
        
        /// <summary>
        /// Indica si la tarea está activa o eliminada lógicamente.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        /// <summary>
        /// Fecha de creación del registro.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        /// <summary>
        /// Fecha de última modificación del registro.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Versión optimista para manejo de concurrencia.
        /// </summary>
        public byte[]? RowVersion { get; set; }
    }
}

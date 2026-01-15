namespace TaskManager.Domain.Entities
{
    /// <summary>
    /// Representa una etiqueta para categorización de tareas.
    /// </summary>
    public class Label
    {
        public Guid Id { get; set; }
        
        /// <summary>
        /// Nombre de la etiqueta.
        /// </summary>
        public string Name { get; set; } = string.Empty;
        
        /// <summary>
        /// Color de la etiqueta para visualización.
        /// </summary>
        public string Color { get; set; } = "#6c757d";
        
        /// <summary>
        /// Indica si la etiqueta está activa.
        /// </summary>
        public bool IsActive { get; set; } = true;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        
        /// <summary>
        /// Tareas asociadas a esta etiqueta.
        /// </summary>
        public ICollection<TaskLabel> TaskLabels { get; set; } = new List<TaskLabel>();
    }
    
    /// <summary>
    /// Representa la relación muchos a muchos entre tareas y etiquetas.
    /// </summary>
    public class TaskLabel
    {
        public Guid TaskId { get; set; }
        public Task Task { get; set; } = null!;
        
        public Guid LabelId { get; set; }
        public Label Label { get; set; } = null!;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}

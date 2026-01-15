namespace TaskManager.Application.DTOs
{
    using TaskManager.Domain.Enums;

    /// <summary>
    /// DTO para respuesta de tarea.
    /// </summary>
    public class TaskResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskStatus Status { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? CompletedAt { get; set; }
        public Guid? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public Guid CreatedByUserId { get; set; }
        public string CreatedByUserName { get; set; } = string.Empty;
        public Guid? ProjectId { get; set; }
        public string? ProjectName { get; set; }
        public List<LabelDto> Labels { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    
    /// <summary>
    /// DTO para etiqueta.
    /// </summary>
    public class LabelDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
    }
    
    /// <summary>
    /// DTO para solicitud de creación de tarea.
    /// </summary>
    public class CreateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; } = TaskPriority.Medium;
        public DateTime? DueDate { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public IEnumerable<Guid>? LabelIds { get; set; }
    }
    
    /// <summary>
    /// DTO para solicitud de actualización de tarea.
    /// </summary>
    public class UpdateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TaskPriority Priority { get; set; }
        public DateTime? DueDate { get; set; }
        public Guid? ProjectId { get; set; }
    }
    
    /// <summary>
    /// DTO para solicitud de actualización de estado.
    /// </summary>
    public class UpdateTaskStatusRequest
    {
        public TaskStatus Status { get; set; }
    }
    
    /// <summary>
    /// DTO para lista paginada de tareas.
    /// </summary>
    public class TaskListResponse
    {
        public IEnumerable<TaskResponse> Tasks { get; set; } = new List<TaskResponse>();
        public int TotalCount { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
    }
    
    /// <summary>
    /// Filtros y opciones de paginación para consultar tareas.
    /// </summary>
    public class TaskFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 20;
        public TaskStatus? Status { get; set; }
        public TaskPriority? Priority { get; set; }
        public Guid? ProjectId { get; set; }
        public Guid? AssignedUserId { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public bool SortDescending { get; set; }
        public bool IncludeOverdue { get; set; }
    }
}

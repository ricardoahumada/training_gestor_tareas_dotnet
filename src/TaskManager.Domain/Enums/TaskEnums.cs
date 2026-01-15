namespace TaskManager.Domain.Enums
{
    /// <summary>
    /// Enumera los posibles estados de una tarea.
    /// </summary>
    public enum TaskStatus
    {
        /// <summary>
        /// Tarea pendiente de iniciar.
        /// </summary>
        Pending = 0,
        
        /// <summary>
        /// Tarea en progreso de ejecución.
        /// </summary>
        InProgress = 1,
        
        /// <summary>
        /// Tarea completada satisfactoriamente.
        /// </summary>
        Completed = 2,
        
        /// <summary>
        /// Tarea cancelada antes de completarse.
        /// </summary>
        Cancelled = 3,
        
        /// <summary>
        /// Tarea bloqueada esperando dependencias.
        /// </summary>
        Blocked = 4
    }
    
    /// <summary>
    /// Enumera los niveles de prioridad de una tarea.
    /// </summary>
    public enum TaskPriority
    {
        /// <summary>
        /// Prioridad baja, sin urgencia temporal.
        /// </summary>
        Low = 0,
        
        /// <summary>
        /// Prioridad media, atención normal.
        /// </summary>
        Medium = 1,
        
        /// <summary>
        /// Prioridad alta, atención pronto.
        /// </summary>
        High = 2,
        
        /// <summary>
        /// Prioridad crítica, atención inmediata.
        /// </summary>
        Critical = 3
    }
    
    /// <summary>
    /// Enumera los roles de usuario en el sistema.
    /// </summary>
    public enum UserRole
    {
        /// <summary>
        /// Usuario regular con permisos básicos.
        /// </summary>
        User = 0,
        
        /// <summary>
        /// Administrador con permisos completos.
        /// </summary>
        Admin = 1
    }
}

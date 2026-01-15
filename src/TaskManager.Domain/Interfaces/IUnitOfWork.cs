using System;
using System.Threading;
using System.Threading.Tasks;

namespace TaskManager.Domain.Interfaces
{
    /// <summary>
    /// Interfaz para el patrón Unit of Work.
    /// </summary>
    public interface IUnitOfWork
    {
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
    
    /// <summary>
    /// Interfaz para servicios de notificación de tareas.
    /// </summary>
    public interface ITaskNotifier
    {
        Task NotifyTaskAssignedAsync(Domain.Entities.Task task, Guid userId);
        Task NotifyStatusChangedAsync(Domain.Entities.Task task, Guid userId);
    }
}

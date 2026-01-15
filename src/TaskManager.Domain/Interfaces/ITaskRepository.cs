namespace TaskManager.Domain.Interfaces
{
    using TaskManager.Domain.Common;
    using TaskManager.Domain.Entities;
    using TaskManager.Domain.Enums;

    /// <summary>
    /// Interfaz para el repositorio de tareas.
    /// </summary>
    public interface ITaskRepository
    {
        System.Threading.Tasks.Task<Domain.Entities.Task?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<PagedResult<Domain.Entities.Task>> GetAllAsync(object filter);
        System.Threading.Tasks.Task<PagedResult<Domain.Entities.Task>> GetByAssignedUserIdAsync(Guid userId, int page, int pageSize);
        System.Threading.Tasks.Task<PagedResult<Domain.Entities.Task>> GetByProjectIdAsync(Guid projectId, int page, int pageSize);
        System.Threading.Tasks.Task<Domain.Entities.Task> AddAsync(Domain.Entities.Task task);
        System.Threading.Tasks.Task<Domain.Entities.Task> UpdateAsync(Domain.Entities.Task task);
        System.Threading.Tasks.Task<bool> ExistsAsync(Guid id);
        System.Threading.Tasks.Task DeleteAsync(Guid id);
    }
}

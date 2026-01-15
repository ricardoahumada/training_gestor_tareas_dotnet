using TaskManager.Application.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Interfaces
{
    /// <summary>
    /// Interfaz para el servicio de tareas.
    /// </summary>
    public interface ITaskService
    {
        System.Threading.Tasks.Task<TaskResponse?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<PagedResult<TaskResponse>> GetAllAsync(TaskFilterRequest filter);
        System.Threading.Tasks.Task<PagedResult<TaskResponse>> GetByAssignedUserIdAsync(Guid userId, int page, int pageSize);
        System.Threading.Tasks.Task<PagedResult<TaskResponse>> GetByProjectIdAsync(Guid projectId, int page, int pageSize);
        System.Threading.Tasks.Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid createdByUserId);
        System.Threading.Tasks.Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request);
        System.Threading.Tasks.Task<TaskResponse> UpdateStatusAsync(Guid id, Domain.Enums.TaskStatus newStatus);
        System.Threading.Tasks.Task<TaskResponse> AssignUserAsync(Guid id, Guid userId);
        System.Threading.Tasks.Task DeleteAsync(Guid id);
    }
    
    /// <summary>
    /// Interfaz para el servicio de proyectos.
    /// </summary>
    public interface IProjectService
    {
        System.Threading.Tasks.Task<ProjectResponse?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<PagedResult<ProjectResponse>> GetAllAsync(int page, int pageSize);
        System.Threading.Tasks.Task<PagedResult<ProjectResponse>> GetByOwnerIdAsync(Guid ownerId, int page, int pageSize);
        System.Threading.Tasks.Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid ownerId);
        System.Threading.Tasks.Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request);
        System.Threading.Tasks.Task DeleteAsync(Guid id);
    }
    
    /// <summary>
    /// Interfaz para el servicio de etiquetas.
    /// </summary>
    public interface ILabelService
    {
        System.Threading.Tasks.Task<LabelDto?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<IEnumerable<LabelDto>> GetAllAsync();
        System.Threading.Tasks.Task<LabelDto> CreateAsync(LabelDto request);
        System.Threading.Tasks.Task<LabelDto> UpdateAsync(Guid id, LabelDto request);
        System.Threading.Tasks.Task DeleteAsync(Guid id);
    }
    
    /// <summary>
    /// Interfaz para el servicio de usuarios.
    /// </summary>
    public interface IUserService
    {
        System.Threading.Tasks.Task<UserResponse?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<UserResponse?> GetByUsernameAsync(string username);
        System.Threading.Tasks.Task<PagedResult<UserResponse>> GetAllAsync(int page, int pageSize);
    }
}

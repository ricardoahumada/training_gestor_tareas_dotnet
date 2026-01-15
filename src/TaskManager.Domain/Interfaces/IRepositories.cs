namespace TaskManager.Domain.Interfaces
{
    using System.Collections.Generic;
    using TaskManager.Domain.Common;
    using TaskManager.Domain.Entities;

    /// <summary>
    /// Interfaz para el repositorio de proyectos.
    /// </summary>
    public interface IProjectRepository
    {
        System.Threading.Tasks.Task<Project?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<PagedResult<Project>> GetAllAsync(int page, int pageSize);
        System.Threading.Tasks.Task<PagedResult<Project>> GetByOwnerIdAsync(Guid ownerId, int page, int pageSize);
        System.Threading.Tasks.Task<Project> AddAsync(Project project);
        System.Threading.Tasks.Task<Project> UpdateAsync(Project project);
        System.Threading.Tasks.Task<bool> ExistsAsync(Guid id);
        System.Threading.Tasks.Task DeleteAsync(Guid id);
    }
    
    /// <summary>
    /// Interfaz para el repositorio de etiquetas.
    /// </summary>
    public interface ILabelRepository
    {
        System.Threading.Tasks.Task<Label?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<IEnumerable<Label>> GetAllAsync();
        System.Threading.Tasks.Task<IEnumerable<Label>> GetByTaskIdAsync(Guid taskId);
        System.Threading.Tasks.Task<Label> AddAsync(Label label);
        System.Threading.Tasks.Task<Label> UpdateAsync(Label label);
        System.Threading.Tasks.Task<bool> ExistsAsync(Guid id);
    }
    
    /// <summary>
    /// Interfaz para el repositorio de usuarios.
    /// </summary>
    public interface IUserRepository
    {
        System.Threading.Tasks.Task<User?> GetByIdAsync(Guid id);
        System.Threading.Tasks.Task<User?> GetByUsernameAsync(string username);
        System.Threading.Tasks.Task<User?> GetByEmailAsync(string email);
        System.Threading.Tasks.Task<PagedResult<User>> GetAllAsync(int page, int pageSize);
        System.Threading.Tasks.Task<User> AddAsync(User user);
        System.Threading.Tasks.Task<User> UpdateAsync(User user);
        System.Threading.Tasks.Task<bool> ExistsAsync(Guid id);
        System.Threading.Tasks.Task<bool> ExistsByUsernameAsync(string username);
        System.Threading.Tasks.Task<bool> ExistsByEmailAsync(string email);
    }
}

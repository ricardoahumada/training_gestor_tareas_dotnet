using TaskManager.Application.DTOs;
using TaskManager.Application.Extensions;
using TaskManager.Application.Interfaces;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;

namespace TaskManager.Application.Services
{
    /// <summary>
    /// Servicio de aplicación para la gestión de proyectos.
    /// </summary>
    public class ProjectService : IProjectService
    {
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly ITaskRepository _taskRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ProjectService(
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            ITaskRepository taskRepository,
            IUnitOfWork unitOfWork)
        {
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _taskRepository = taskRepository;
            _unitOfWork = unitOfWork;
        }

        /// <inheritdoc/>
        public async Task<ProjectResponse?> GetByIdAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            return project?.MapToResponse();
        }

        /// <inheritdoc/>
        public async Task<PagedResult<ProjectResponse>> GetAllAsync(int page, int pageSize)
        {
            var pagedResult = await _projectRepository.GetAllAsync(page, pageSize);
            return pagedResult.MapToResponse();
        }

        /// <inheritdoc/>
        public async Task<PagedResult<ProjectResponse>> GetByOwnerIdAsync(Guid ownerId, int page, int pageSize)
        {
            var pagedResult = await _projectRepository.GetByOwnerIdAsync(ownerId, page, pageSize);
            return pagedResult.MapToResponse();
        }

        /// <inheritdoc/>
        public async Task<ProjectResponse> CreateAsync(CreateProjectRequest request, Guid ownerId)
        {
            // Validar que el usuario existe
            var userExists = await _userRepository.ExistsAsync(ownerId);
            if (!userExists)
            {
                throw new InvalidOperationException($"No se encontró un usuario con el identificador {ownerId}");
            }

            var project = new Project
            {
                Name = request.Name,
                Description = request.Description,
                Color = request.Color,
                OwnerId = ownerId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            await _projectRepository.AddAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return project.MapToResponse();
        }

        /// <inheritdoc/>
        public async Task<ProjectResponse> UpdateAsync(Guid id, UpdateProjectRequest request)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new KeyNotFoundException($"No se encontró un proyecto con el identificador {id}");
            }

            project.Name = request.Name;
            project.Description = request.Description;
            project.Color = request.Color;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();

            return project.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var project = await _projectRepository.GetByIdAsync(id);
            if (project == null)
            {
                throw new KeyNotFoundException($"No se encontró un proyecto con el identificador {id}");
            }

            project.IsActive = false;
            project.UpdatedAt = DateTime.UtcNow;

            await _projectRepository.UpdateAsync(project);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Servicio de aplicación para la gestión de tareas.
    /// Implementa operaciones CRUD con validación de reglas de negocio.
    /// </summary>
    public class TaskService : ITaskService
    {
        private readonly ITaskRepository _taskRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILabelRepository _labelRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ITaskNotifier _taskNotifier;

        public TaskService(
            ITaskRepository taskRepository,
            IProjectRepository projectRepository,
            IUserRepository userRepository,
            ILabelRepository labelRepository,
            IUnitOfWork unitOfWork,
            ITaskNotifier taskNotifier)
        {
            _taskRepository = taskRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _labelRepository = labelRepository;
            _unitOfWork = unitOfWork;
            _taskNotifier = taskNotifier;
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<TaskResponse?> GetByIdAsync(Guid id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            return task?.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<PagedResult<TaskResponse>> GetAllAsync(TaskFilterRequest filter)
        {
            var pagedResult = await _taskRepository.GetAllAsync(filter);
            return pagedResult.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<PagedResult<TaskResponse>> GetByAssignedUserIdAsync(Guid userId, int page, int pageSize)
        {
            var pagedResult = await _taskRepository.GetByAssignedUserIdAsync(userId, page, pageSize);
            return pagedResult.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<PagedResult<TaskResponse>> GetByProjectIdAsync(Guid projectId, int page, int pageSize)
        {
            var pagedResult = await _taskRepository.GetByProjectIdAsync(projectId, page, pageSize);
            return pagedResult.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<TaskResponse> CreateAsync(CreateTaskRequest request, Guid createdByUserId)
        {
            // Validar que el proyecto existe si se especifica
            if (request.ProjectId.HasValue)
            {
                var projectExists = await _projectRepository.ExistsAsync(request.ProjectId.Value);
                if (!projectExists)
                {
                    throw new InvalidOperationException($"No se encontró un proyecto con el identificador {request.ProjectId}");
                }
            }

            // Validar que el usuario asignado existe si se especifica
            if (request.AssignedUserId.HasValue)
            {
                var userExists = await _userRepository.ExistsAsync(request.AssignedUserId.Value);
                if (!userExists)
                {
                    throw new InvalidOperationException($"No se encontró un usuario con el identificador {request.AssignedUserId}");
                }
            }

            // Validar fecha límite
            if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("La fecha límite no puede ser anterior a la fecha actual");
            }

            // Crear la entidad de dominio
            var task = new Domain.Entities.Task
            {
                Title = request.Title,
                Description = request.Description,
                Status = Domain.Enums.TaskStatus.Pending,
                Priority = request.Priority,
                DueDate = request.DueDate,
                ProjectId = request.ProjectId,
                AssignedUserId = request.AssignedUserId,
                CreatedByUserId = createdByUserId,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Agregar etiquetas si se especifican
            if (request.LabelIds != null && request.LabelIds.Any())
            {
                foreach (var labelId in request.LabelIds)
                {
                    var labelExists = await _labelRepository.ExistsAsync(labelId);
                    if (labelExists)
                    {
                        task.TaskLabels.Add(new TaskLabel
                        {
                            TaskId = task.Id,
                            LabelId = labelId,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }

            // Persistir y confirmar cambios
            await _taskRepository.AddAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // Notificar asignación si corresponde
            if (request.AssignedUserId.HasValue)
            {
                await _taskNotifier.NotifyTaskAssignedAsync(task, request.AssignedUserId.Value);
            }

            return task.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<TaskResponse> UpdateAsync(Guid id, UpdateTaskRequest request)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                throw new KeyNotFoundException($"No se encontró una tarea con el identificador {id}");
            }

            // Validar fecha límite
            if (request.DueDate.HasValue && request.DueDate.Value < DateTime.UtcNow.Date)
            {
                throw new InvalidOperationException("La fecha límite no puede ser anterior a la fecha actual");
            }

            // Actualizar campos permitidos
            task.Title = request.Title;
            task.Description = request.Description;
            task.Priority = request.Priority;
            task.DueDate = request.DueDate;
            task.ProjectId = request.ProjectId;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            return task.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<TaskResponse> UpdateStatusAsync(Guid id, Domain.Enums.TaskStatus newStatus)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                throw new KeyNotFoundException($"No se encontró una tarea con el identificador {id}");
            }

            // Validar transición de estado
            if (task.Status == Domain.Enums.TaskStatus.Completed && newStatus != Domain.Enums.TaskStatus.Completed)
            {
                throw new InvalidOperationException("No se puede cambiar el estado de una tarea ya completada");
            }

            task.Status = newStatus;
            task.UpdatedAt = DateTime.UtcNow;

            if (newStatus == Domain.Enums.TaskStatus.Completed)
            {
                task.CompletedAt = DateTime.UtcNow;
            }

            await _taskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // Notificar cambio de estado
            if (task.AssignedUserId.HasValue)
            {
                await _taskNotifier.NotifyStatusChangedAsync(task, task.AssignedUserId.Value);
            }

            return task.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<TaskResponse> AssignUserAsync(Guid id, Guid userId)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                throw new KeyNotFoundException($"No se encontró una tarea con el identificador {id}");
            }

            var userExists = await _userRepository.ExistsAsync(userId);
            if (!userExists)
            {
                throw new InvalidOperationException($"No se encontró un usuario con el identificador {userId}");
            }

            var previousAssignee = task.AssignedUserId;
            task.AssignedUserId = userId;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();

            // Notificar nueva asignación
            await _taskNotifier.NotifyTaskAssignedAsync(task, userId);

            return task.MapToResponse();
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var task = await _taskRepository.GetByIdAsync(id);
            if (task == null)
            {
                throw new KeyNotFoundException($"No se encontró una tarea con el identificador {id}");
            }

            // Soft delete: marcar como inactivo
            task.IsActive = false;
            task.UpdatedAt = DateTime.UtcNow;

            await _taskRepository.UpdateAsync(task);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Common;

namespace TaskManager.Application.Extensions
{
    /// <summary>
    /// Extensiones de mapeo para entidades de dominio.
    /// </summary>
    public static class MappingExtensions
    {
        /// <summary>
        /// Mapea una tarea a TaskResponse.
        /// </summary>
        public static TaskResponse MapToResponse(this Domain.Entities.Task task)
        {
            return new TaskResponse
            {
                Id = task.Id,
                Title = task.Title,
                Description = task.Description,
                Status = task.Status,
                Priority = task.Priority,
                DueDate = task.DueDate,
                CompletedAt = task.CompletedAt,
                AssignedUserId = task.AssignedUserId,
                AssignedUserName = task.AssignedUser?.Username,
                CreatedByUserId = task.CreatedByUserId,
                CreatedByUserName = task.CreatedByUser?.Username ?? "Unknown",
                ProjectId = task.ProjectId,
                ProjectName = task.Project?.Name,
                Labels = task.TaskLabels.Select(tl => new LabelDto
                {
                    Id = tl.LabelId,
                    Name = tl.Label?.Name ?? "Unknown",
                    Color = tl.Label?.Color ?? "#6c757d"
                }).ToList(),
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt
            };
        }

        /// <summary>
        /// Mapea un PagedResult de tareas a PagedResult de TaskResponse.
        /// </summary>
        public static PagedResult<TaskResponse> MapToResponse(this PagedResult<Domain.Entities.Task> pagedResult)
        {
            return new PagedResult<TaskResponse>(
                pagedResult.Items.Select(t => t.MapToResponse()).ToList(),
                pagedResult.TotalCount,
                pagedResult.Page,
                pagedResult.PageSize);
        }

        /// <summary>
        /// Mapea un proyecto a ProjectResponse.
        /// </summary>
        public static ProjectResponse MapToResponse(this Project project)
        {
            return new ProjectResponse
            {
                Id = project.Id,
                Name = project.Name,
                Description = project.Description,
                Color = project.Color,
                OwnerId = project.OwnerId,
                OwnerName = project.Owner?.Username ?? "Unknown",
                TaskCount = project.Tasks?.Count(t => t.IsActive) ?? 0,
                CompletedTaskCount = project.Tasks?.Count(t => t.IsActive && t.Status == Domain.Enums.TaskStatus.Completed) ?? 0,
                CreatedAt = project.CreatedAt,
                UpdatedAt = project.UpdatedAt
            };
        }

        /// <summary>
        /// Mapea un PagedResult de proyectos a PagedResult de ProjectResponse.
        /// </summary>
        public static PagedResult<ProjectResponse> MapToResponse(this PagedResult<Project> pagedResult)
        {
            return new PagedResult<ProjectResponse>(
                pagedResult.Items.Select(p => p.MapToResponse()).ToList(),
                pagedResult.TotalCount,
                pagedResult.Page,
                pagedResult.PageSize);
        }

        /// <summary>
        /// Mapea un usuario a UserResponse.
        /// </summary>
        public static UserResponse MapToResponse(this User user)
        {
            return new UserResponse
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Application.DTOs;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Enums;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de tareas.
    /// Utiliza Entity Framework Core para operaciones de persistencia.
    /// </summary>
    public class TaskRepository : ITaskRepository
    {
        private readonly ApplicationDbContext _context;

        public TaskRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <inheritdoc/>
        public async Task<Domain.Entities.Task?> GetByIdAsync(Guid id)
        {
            return await _context.Tasks
                .AsNoTracking()
                .Include(t => t.AssignedUser)
                .Include(t => t.Project)
                .Include(t => t.TaskLabels)
                    .ThenInclude(tl => tl.Label)
                .FirstOrDefaultAsync(t => t.Id == id && t.IsActive);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task<PagedResult<Domain.Entities.Task>> GetAllAsync(object filterObj)
        {
            var filter = filterObj as TaskFilterRequest ?? new TaskFilterRequest();
            
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.AssignedUser)
                .Include(t => t.Project)
                .Where(t => t.IsActive);

            // Aplicar filtros
            if (filter.Status.HasValue)
            {
                query = query.Where(t => t.Status == filter.Status.Value);
            }

            if (filter.Priority.HasValue)
            {
                query = query.Where(t => t.Priority == filter.Priority.Value);
            }

            if (filter.ProjectId.HasValue)
            {
                query = query.Where(t => t.ProjectId == filter.ProjectId.Value);
            }

            if (filter.AssignedUserId.HasValue)
            {
                query = query.Where(t => t.AssignedUserId == filter.AssignedUserId.Value);
            }

            if (filter.IncludeOverdue)
            {
                query = query.Where(t => t.DueDate.HasValue && 
                    t.DueDate < DateTime.UtcNow && 
                    t.Status != Domain.Enums.TaskStatus.Completed);
            }

            // Búsqueda por texto
            if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
            {
                var searchTerm = filter.SearchTerm.ToLower();
                query = query.Where(t => 
                    t.Title.ToLower().Contains(searchTerm) ||
                    (t.Description != null && t.Description.ToLower().Contains(searchTerm)));
            }

            // Ordenamiento
            query = filter.SortBy?.ToLower() switch
            {
                "duedate" => filter.SortDescending 
                    ? query.OrderByDescending(t => t.DueDate) 
                    : query.OrderBy(t => t.DueDate),
                "priority" => filter.SortDescending 
                    ? query.OrderByDescending(t => t.Priority) 
                    : query.OrderBy(t => t.Priority),
                "status" => filter.SortDescending 
                    ? query.OrderByDescending(t => t.Status) 
                    : query.OrderBy(t => t.Status),
                "createdat" => filter.SortDescending 
                    ? query.OrderByDescending(t => t.CreatedAt) 
                    : query.OrderBy(t => t.CreatedAt),
                _ => query.OrderByDescending(t => t.CreatedAt) // Por defecto ordenar por fecha de creación
            };

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToListAsync();

            return new PagedResult<Domain.Entities.Task>(items, totalCount, filter.Page, filter.PageSize);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<Domain.Entities.Task>> GetByAssignedUserIdAsync(Guid userId, int page, int pageSize)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.Project)
                .Where(t => t.AssignedUserId == userId && t.IsActive);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Domain.Entities.Task>(items, totalCount, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<PagedResult<Domain.Entities.Task>> GetByProjectIdAsync(Guid projectId, int page, int pageSize)
        {
            var query = _context.Tasks
                .AsNoTracking()
                .Include(t => t.AssignedUser)
                .Where(t => t.ProjectId == projectId && t.IsActive);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => t.DueDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Domain.Entities.Task>(items, totalCount, page, pageSize);
        }

        /// <inheritdoc/>
        public async Task<Domain.Entities.Task> AddAsync(Domain.Entities.Task task)
        {
            await _context.Tasks.AddAsync(task);
            return task;
        }

        /// <inheritdoc/>
        public async Task<Domain.Entities.Task> UpdateAsync(Domain.Entities.Task task)
        {
            _context.Tasks.Update(task);
            return task;
        }

        /// <inheritdoc/>
        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Tasks.AnyAsync(t => t.Id == id && t.IsActive);
        }

        /// <inheritdoc/>
        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var task = await _context.Tasks.FindAsync(id);
            if (task != null)
            {
                task.IsActive = false;
                _context.Tasks.Update(task);
            }
        }
    }
}

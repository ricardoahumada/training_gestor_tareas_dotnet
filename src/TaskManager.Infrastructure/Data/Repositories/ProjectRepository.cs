using System;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Domain.Common;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de proyectos.
    /// </summary>
    public class ProjectRepository : IProjectRepository
    {
        private readonly ApplicationDbContext _context;

        public ProjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _context.Projects
                .AsNoTracking()
                .Include(p => p.Owner)
                .Include(p => p.Tasks)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive);
        }

        public async Task<PagedResult<Project>> GetAllAsync(int page, int pageSize)
        {
            var query = _context.Projects
                .AsNoTracking()
                .Include(p => p.Owner)
                .Where(p => p.IsActive);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Project>(items, totalCount, page, pageSize);
        }

        public async Task<PagedResult<Project>> GetByOwnerIdAsync(Guid ownerId, int page, int pageSize)
        {
            var query = _context.Projects
                .AsNoTracking()
                .Include(p => p.Tasks)
                .Where(p => p.OwnerId == ownerId && p.IsActive);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(p => p.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<Project>(items, totalCount, page, pageSize);
        }

        public async Task<Project> AddAsync(Project project)
        {
            await _context.Projects.AddAsync(project);
            return project;
        }

        public async Task<Project> UpdateAsync(Project project)
        {
            _context.Projects.Update(project);
            return project;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Projects.AnyAsync(p => p.Id == id && p.IsActive);
        }

        public async System.Threading.Tasks.Task DeleteAsync(Guid id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.IsActive = false;
                _context.Projects.Update(project);
            }
        }
    }
}

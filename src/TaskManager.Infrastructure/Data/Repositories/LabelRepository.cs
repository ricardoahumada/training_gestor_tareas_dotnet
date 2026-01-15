using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TaskManager.Domain.Entities;
using TaskManager.Domain.Interfaces;
using TaskManager.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace TaskManager.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de etiquetas.
    /// </summary>
    public class LabelRepository : ILabelRepository
    {
        private readonly ApplicationDbContext _context;

        public LabelRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Label?> GetByIdAsync(Guid id)
        {
            return await _context.Labels
                .AsNoTracking()
                .FirstOrDefaultAsync(l => l.Id == id && l.IsActive);
        }

        public async Task<IEnumerable<Label>> GetAllAsync()
        {
            return await _context.Labels
                .AsNoTracking()
                .Where(l => l.IsActive)
                .OrderBy(l => l.Name)
                .ToListAsync();
        }

        public async Task<IEnumerable<Label>> GetByTaskIdAsync(Guid taskId)
        {
            return await _context.Labels
                .AsNoTracking()
                .Where(l => l.IsActive)
                .Join(_context.TaskLabels.Where(tl => tl.TaskId == taskId),
                      l => l.Id,
                      tl => tl.LabelId,
                      (l, tl) => l)
                .ToListAsync();
        }

        public async Task<Label> AddAsync(Label label)
        {
            await _context.Labels.AddAsync(label);
            return label;
        }

        public async Task<Label> UpdateAsync(Label label)
        {
            _context.Labels.Update(label);
            return label;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            return await _context.Labels.AnyAsync(l => l.Id == id && l.IsActive);
        }
    }
}

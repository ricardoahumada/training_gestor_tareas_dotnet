using Microsoft.EntityFrameworkCore;
using TaskManager.Attachments.Domain.Interfaces;
using TaskManager.Infrastructure.Data;

namespace TaskManager.Attachments.Infrastructure.Data.Repositories;

/// <summary>
/// Read-only repository for accessing legacy Task entities.
/// Provides minimal coupling to legacy system.
/// </summary>
public class TaskReadOnlyRepository : ITaskReadOnlyRepository
{
    private readonly ApplicationDbContext _legacyContext;

    public TaskReadOnlyRepository(ApplicationDbContext legacyContext)
    {
        _legacyContext = legacyContext;
    }

    public async Task<TaskInfo?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var task = await _legacyContext.Tasks
            .AsNoTracking()
            .Where(t => t.Id == taskId)
            .Select(t => new TaskInfo
            {
                Id = t.Id,
                IsActive = t.IsActive,
                CreatedByUserId = t.CreatedByUserId
            })
            .FirstOrDefaultAsync(cancellationToken);

        return task;
    }

    public async Task<bool> IsTaskActiveAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        return await _legacyContext.Tasks
            .AsNoTracking()
            .Where(t => t.Id == taskId && t.IsActive)
            .AnyAsync(cancellationToken);
    }
}

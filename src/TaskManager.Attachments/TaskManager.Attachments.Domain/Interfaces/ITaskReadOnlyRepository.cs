namespace TaskManager.Attachments.Domain.Interfaces;

/// <summary>
/// Read-only repository for accessing legacy Task entities without modifying them.
/// </summary>
public interface ITaskReadOnlyRepository
{
    /// <summary>
    /// Retrieves a task by ID (read-only).
    /// </summary>
    Task<TaskInfo?> GetTaskByIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a task exists and is active.
    /// </summary>
    Task<bool> IsTaskActiveAsync(Guid taskId, CancellationToken cancellationToken = default);
}

/// <summary>
/// Minimal task information needed by attachments module (avoids coupling to legacy Task entity).
/// </summary>
public class TaskInfo
{
    public Guid Id { get; set; }
    public bool IsActive { get; set; }
    public Guid CreatedByUserId { get; set; }
}

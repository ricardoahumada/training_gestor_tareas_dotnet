using TaskManager.Attachments.Domain.Entities;

namespace TaskManager.Attachments.Domain.Interfaces;

/// <summary>
/// Repository interface for Attachment entity persistence.
/// </summary>
public interface IAttachmentRepository
{
    /// <summary>
    /// Adds a new attachment and saves changes.
    /// </summary>
    Task<Attachment> AddAsync(Attachment attachment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves an attachment by its ID.
    /// </summary>
    Task<Attachment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves all attachments for a specific task, ordered by UploadedAt DESC.
    /// </summary>
    Task<IEnumerable<Attachment>> GetByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes an attachment.
    /// </summary>
    Task DeleteAsync(Attachment attachment, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts attachments for a specific task.
    /// </summary>
    Task<int> CountByTaskIdAsync(Guid taskId, CancellationToken cancellationToken = default);
}

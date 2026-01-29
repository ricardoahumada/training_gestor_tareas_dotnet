namespace TaskManager.Attachments.Domain.Entities;

/// <summary>
/// Represents a file attachment associated with a task.
/// Contains metadata without storing binary content.
/// </summary>
public class Attachment
{
    /// <summary>
    /// Unique identifier for the attachment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Reference to the associated task (legacy Task entity).
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Original filename including extension (max 255 characters).
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes (min: 1, max: 10,485,760 = 10MB).
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type of the file (e.g., "image/jpeg", "application/pdf").
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Upload timestamp in UTC.
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// User who uploaded the file (legacy User entity reference).
    /// </summary>
    public Guid UploadedByUserId { get; set; }

    /// <summary>
    /// Relative path in storage: {taskId}/{attachmentId}.{ext}
    /// </summary>
    public string StoragePath { get; set; } = string.Empty;
}

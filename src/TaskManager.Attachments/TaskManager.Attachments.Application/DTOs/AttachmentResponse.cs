namespace TaskManager.Attachments.Application.DTOs;

/// <summary>
/// Response DTO containing attachment metadata.
/// </summary>
public class AttachmentResponse
{
    /// <summary>
    /// Unique identifier for the attachment.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// ID of the associated task.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// Original filename with extension.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// File size in bytes.
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// MIME type (e.g., "image/jpeg", "application/pdf").
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// Upload timestamp (UTC).
    /// </summary>
    public DateTime UploadedAt { get; set; }

    /// <summary>
    /// User who uploaded the file.
    /// </summary>
    public Guid UploadedByUserId { get; set; }
}

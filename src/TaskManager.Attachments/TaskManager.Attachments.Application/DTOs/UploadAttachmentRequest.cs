using Microsoft.AspNetCore.Http;

namespace TaskManager.Attachments.Application.DTOs;

/// <summary>
/// Request DTO for uploading a file attachment to a task.
/// </summary>
public class UploadAttachmentRequest
{
    /// <summary>
    /// ID of the task to attach the file to.
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// File to upload (multipart/form-data).
    /// </summary>
    public IFormFile File { get; set; } = null!;
}

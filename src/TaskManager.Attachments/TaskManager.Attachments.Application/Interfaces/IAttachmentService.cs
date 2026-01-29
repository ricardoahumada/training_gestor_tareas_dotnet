using TaskManager.Attachments.Application.DTOs;

namespace TaskManager.Attachments.Application.Interfaces;

/// <summary>
/// Service interface for attachment operations.
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Uploads a file attachment to a task.
    /// </summary>
    /// <param name="request">Upload request with taskId and file</param>
    /// <param name="userId">User performing the upload (from JWT claims)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Attachment metadata</returns>
    /// <exception cref="NotFoundException">Task not found</exception>
    /// <exception cref="BadRequestException">Task inactive or attachment limit reached</exception>
    Task<AttachmentResponse> UploadAttachmentAsync(
        UploadAttachmentRequest request,
        Guid userId,
        CancellationToken cancellationToken = default);
}

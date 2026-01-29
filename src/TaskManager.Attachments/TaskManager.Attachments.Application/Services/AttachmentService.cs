using Microsoft.Extensions.Logging;
using TaskManager.Attachments.Application.DTOs;
using TaskManager.Attachments.Application.Exceptions;
using TaskManager.Attachments.Application.Interfaces;
using TaskManager.Attachments.Domain.Entities;
using TaskManager.Attachments.Domain.Helpers;
using TaskManager.Attachments.Domain.Interfaces;

namespace TaskManager.Attachments.Application.Services;

/// <summary>
/// Service for managing file attachments.
/// </summary>
public class AttachmentService : IAttachmentService
{
    private readonly IAttachmentRepository _attachmentRepository;
    private readonly IFileStorageService _fileStorageService;
    private readonly ITaskReadOnlyRepository _taskRepository;
    private readonly ILogger<AttachmentService> _logger;

    private const int MaxAttachmentsPerTask = 5;

    public AttachmentService(
        IAttachmentRepository attachmentRepository,
        IFileStorageService fileStorageService,
        ITaskReadOnlyRepository taskRepository,
        ILogger<AttachmentService> logger)
    {
        _attachmentRepository = attachmentRepository;
        _fileStorageService = fileStorageService;
        _taskRepository = taskRepository;
        _logger = logger;
    }

    public async Task<AttachmentResponse> UploadAttachmentAsync(
        UploadAttachmentRequest request,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Starting upload for TaskId={TaskId}, FileName={FileName}, UserId={UserId}",
            request.TaskId, request.File.FileName, userId);

        // 1. Validate task exists and is active
        var task = await _taskRepository.GetTaskByIdAsync(request.TaskId, cancellationToken);
        if (task == null)
        {
            _logger.LogWarning("Task not found: {TaskId}", request.TaskId);
            throw new NotFoundException($"No se encontró una tarea con el identificador {request.TaskId}");
        }

        if (!task.IsActive)
        {
            _logger.LogWarning("Task is inactive: {TaskId}", request.TaskId);
            throw new BadRequestException("No se pueden adjuntar archivos a tareas inactivas");
        }

        // 2. Check attachment count limit
        var attachmentCount = await _attachmentRepository.CountByTaskIdAsync(request.TaskId, cancellationToken);
        if (attachmentCount >= MaxAttachmentsPerTask)
        {
            _logger.LogWarning(
                "Attachment limit reached for TaskId={TaskId}, Count={Count}",
                request.TaskId, attachmentCount);
            throw new BadRequestException($"La tarea ya tiene el máximo de {MaxAttachmentsPerTask} adjuntos permitidos");
        }

        // 3. Generate attachment ID and storage path
        var attachmentId = Guid.NewGuid();
        var fileExtension = Path.GetExtension(request.File.FileName).ToLowerInvariant();
        var storagePath = $"{request.TaskId}/{attachmentId}{fileExtension}";

        // 4. Sanitize filename (prevent path traversal)
        var sanitizedFileName = SanitizeFileName(request.File.FileName);

        // 5. Read file content
        byte[] fileContent;
        using (var memoryStream = new MemoryStream())
        {
            await request.File.CopyToAsync(memoryStream, cancellationToken);
            fileContent = memoryStream.ToArray();
        }

        // 6. Save file to storage
        try
        {
            await _fileStorageService.SaveFileAsync(storagePath, fileContent, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save file to storage: {StoragePath}", storagePath);
            throw new InvalidOperationException("Error al guardar el archivo en el almacenamiento", ex);
        }

        // 7. Create attachment entity
        var attachment = new Attachment
        {
            Id = attachmentId,
            TaskId = request.TaskId,
            FileName = sanitizedFileName,
            FileSize = request.File.Length,
            ContentType = request.File.ContentType,
            UploadedAt = DateTime.UtcNow,
            UploadedByUserId = userId,
            StoragePath = storagePath
        };

        // 8. Save to database
        try
        {
            await _attachmentRepository.AddAsync(attachment, cancellationToken);
        }
        catch (Exception ex)
        {
            // Rollback: delete file from storage
            _logger.LogError(ex, "Failed to save attachment to database, rolling back file: {StoragePath}", storagePath);
            try
            {
                await _fileStorageService.DeleteFileAsync(storagePath, cancellationToken);
            }
            catch (Exception deleteEx)
            {
                _logger.LogError(deleteEx, "Failed to rollback file deletion: {StoragePath}", storagePath);
            }
            throw new InvalidOperationException("Error al guardar los metadatos del adjunto en la base de datos", ex);
        }

        _logger.LogInformation(
            "Upload completed: AttachmentId={AttachmentId}, TaskId={TaskId}, FileSize={FileSize}",
            attachmentId, request.TaskId, request.File.Length);

        // 9. Return response
        return new AttachmentResponse
        {
            Id = attachment.Id,
            TaskId = attachment.TaskId,
            FileName = attachment.FileName,
            FileSize = attachment.FileSize,
            ContentType = attachment.ContentType,
            UploadedAt = attachment.UploadedAt,
            UploadedByUserId = attachment.UploadedByUserId
        };
    }

    /// <summary>
    /// Sanitizes filename to prevent path traversal attacks.
    /// </summary>
    private string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            return "file";

        // Remove path components
        fileName = Path.GetFileName(fileName);

        // Remove dangerous characters
        var invalidChars = Path.GetInvalidFileNameChars();
        fileName = string.Join("_", fileName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));

        // Remove leading/trailing dots and spaces
        fileName = fileName.Trim('.', ' ');

        // Ensure not empty after sanitization
        if (string.IsNullOrWhiteSpace(fileName))
            fileName = "file";

        return fileName;
    }
}

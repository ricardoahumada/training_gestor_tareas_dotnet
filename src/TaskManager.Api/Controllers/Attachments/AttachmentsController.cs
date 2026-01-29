using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Api.Controllers.Base;
using TaskManager.Attachments.Application.DTOs;
using TaskManager.Attachments.Application.Exceptions;
using TaskManager.Attachments.Application.Interfaces;

namespace TaskManager.Api.Controllers.Attachments;

/// <summary>
/// Controller for managing task file attachments (Strangler Pattern).
/// </summary>
[Authorize]
[Route("api/v1/attachments")]
public class AttachmentsController : BaseController
{
    private readonly IAttachmentService _attachmentService;
    private readonly IValidator<UploadAttachmentRequest> _uploadValidator;

    public AttachmentsController(
        IAttachmentService attachmentService,
        IValidator<UploadAttachmentRequest> uploadValidator,
        ILogger<BaseController> logger) : base(logger)
    {
        _attachmentService = attachmentService;
        _uploadValidator = uploadValidator;
    }

    /// <summary>
    /// Upload a file attachment to a task.
    /// </summary>
    /// <param name="request">Upload request with taskId and file</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created attachment metadata</returns>
    /// <response code="201">Attachment uploaded successfully</response>
    /// <response code="400">Invalid request (task inactive, limit reached, invalid file)</response>
    /// <response code="401">Unauthorized (missing or invalid JWT)</response>
    /// <response code="404">Task not found</response>
    /// <response code="413">File too large (>10MB)</response>
    /// <response code="415">Unsupported file type</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(string), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(string), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(string), StatusCodes.Status413PayloadTooLarge)]
    [ProducesResponseType(typeof(string), StatusCodes.Status415UnsupportedMediaType)]
    [ProducesResponseType(typeof(string), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UploadAttachment(
        [FromForm] UploadAttachmentRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Validate request
            var validationResult = await _uploadValidator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = string.Join(", ", validationResult.Errors.Select(e => e.ErrorMessage));
                _logger.LogWarning("Validation failed for upload: {Errors}", errors);
                
                // Check if it's a MIME type or size issue
                if (errors.Contains("MIME") || errors.Contains("Tipo MIME"))
                {
                    return StatusCode(StatusCodes.Status415UnsupportedMediaType, 
                        "Tipo de archivo no permitido. Permitidos: jpg, jpeg, png, gif, pdf, doc, docx, xls, xlsx");
                }
                if (errors.Contains("MB") || errors.Contains("tamaño"))
                {
                    return StatusCode(StatusCodes.Status413PayloadTooLarge, 
                        "El archivo excede el tamaño máximo permitido de 10 MB");
                }
                
                return BadRequest(errors);
            }

            // Get authenticated user ID
            var userId = CurrentUserId;
            if (userId == null)
            {
                _logger.LogWarning("User ID not found in JWT claims");
                return Unauthorized("No se pudo identificar el usuario autenticado");
            }

            // Upload attachment
            var response = await _attachmentService.UploadAttachmentAsync(
                request,
                userId.Value,
                cancellationToken);

            _logger.LogInformation(
                "Attachment uploaded: Id={AttachmentId}, TaskId={TaskId}, UserId={UserId}",
                response.Id, response.TaskId, userId);

            return CreatedAtAction(
                nameof(GetAttachment),
                new { id = response.Id },
                response);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning(ex, "Task not found: {Message}", ex.Message);
            return NotFound(ex.Message);
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning(ex, "Bad request: {Message}", ex.Message);
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading attachment");
            return StatusCode(StatusCodes.Status500InternalServerError, 
                "Error interno al procesar el adjunto");
        }
    }

    /// <summary>
    /// Get attachment metadata by ID (placeholder for Phase 5).
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(AttachmentResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetAttachment(Guid id)
    {
        // TODO: Implement in Phase 5 (User Story 3)
        return NotFound("Endpoint no implementado aún");
    }
}

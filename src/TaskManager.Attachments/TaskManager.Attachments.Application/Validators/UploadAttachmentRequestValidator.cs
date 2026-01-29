using FluentValidation;
using TaskManager.Attachments.Application.DTOs;
using TaskManager.Attachments.Domain.Helpers;

namespace TaskManager.Attachments.Application.Validators;

/// <summary>
/// Validator for file upload requests.
/// Validates file size, extension, and MIME type.
/// </summary>
public class UploadAttachmentRequestValidator : AbstractValidator<UploadAttachmentRequest>
{
    private const long MaxFileSizeBytes = 10_485_760; // 10 MB

    public UploadAttachmentRequestValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty()
            .WithMessage("TaskId es requerido")
            .Must(BeValidGuid)
            .WithMessage("TaskId debe ser un GUID válido");

        RuleFor(x => x.File)
            .NotNull()
            .WithMessage("File es requerido")
            .Must(HaveValidSize)
            .WithMessage($"El archivo debe ser entre 1 byte y {MaxFileSizeBytes / 1_048_576} MB")
            .Must(HaveAllowedExtension)
            .WithMessage("Extensión de archivo no permitida. Permitidos: jpg, jpeg, png, gif, pdf, doc, docx, xls, xlsx")
            .Must(HaveAllowedMimeType)
            .WithMessage("Tipo MIME no permitido. Permitidos: jpg, jpeg, png, gif, pdf, doc, docx, xls, xlsx");
    }

    private bool BeValidGuid(Guid taskId)
    {
        return taskId != Guid.Empty;
    }

    private bool HaveValidSize(UploadAttachmentRequest request, Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null) return false;
        return file.Length > 0 && file.Length <= MaxFileSizeBytes;
    }

    private bool HaveAllowedExtension(UploadAttachmentRequest request, Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.FileName)) return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        return MimeTypeMapping.IsAllowedExtension(extension);
    }

    private bool HaveAllowedMimeType(UploadAttachmentRequest request, Microsoft.AspNetCore.Http.IFormFile? file)
    {
        if (file == null || string.IsNullOrWhiteSpace(file.ContentType)) return false;

        return MimeTypeMapping.IsAllowedMimeType(file.ContentType);
    }
}

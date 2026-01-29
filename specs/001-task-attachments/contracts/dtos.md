# DTOs: Módulo de Archivos Adjuntos

**Feature**: Módulo de Archivos Adjuntos
**Date**: 2026-01-29
**Purpose**: Definir contratos de entrada/salida de API

## Request DTOs

### UploadAttachmentRequest

**Purpose**: Upload file to task (multipart/form-data)

**Properties**:

| Property | Type | Required | Validation | Description |
|----------|------|----------|------------|-------------|
| `TaskId` | `Guid` | Yes | Must be valid GUID | ID de la tarea destino |
| `File` | `IFormFile` | Yes | See validations below | Archivo a subir |

**File Validations** (FluentValidation):

```csharp
public class UploadAttachmentRequestValidator : AbstractValidator<UploadAttachmentRequest>
{
    public UploadAttachmentRequestValidator()
    {
        RuleFor(x => x.TaskId)
            .NotEmpty().WithMessage("TaskId es requerido")
            .Must(BeValidGuid).WithMessage("TaskId debe ser un GUID válido");
        
        RuleFor(x => x.File)
            .NotNull().WithMessage("File es requerido")
            .Must(HaveValidSize).WithMessage("El archivo debe ser entre 1 byte y 10 MB")
            .Must(HaveAllowedExtension).WithMessage("Extensión de archivo no permitida. Permitidos: jpg, jpeg, png, gif, pdf, doc, docx, xls, xlsx")
            .Must(HaveAllowedMimeType).WithMessage("Tipo MIME no permitido")
            .MustAsync(HaveValidMagicNumbers).WithMessage("El contenido del archivo no coincide con su extensión (posible archivo malicioso)");
    }
    
    private bool BeValidGuid(Guid taskId)
    {
        return taskId != Guid.Empty;
    }
    
    private bool HaveValidSize(IFormFile file)
    {
        return file != null && file.Length > 0 && file.Length <= 10_485_760; // 10 MB
    }
    
    private bool HaveAllowedExtension(IFormFile file)
    {
        if (file == null) return false;
        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".pdf", ".doc", ".docx", ".xls", ".xlsx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        return allowedExtensions.Contains(extension);
    }
    
    private bool HaveAllowedMimeType(IFormFile file)
    {
        if (file == null) return false;
        
        var allowedMimeTypes = new[]
        {
            "image/jpeg", "image/png", "image/gif",
            "application/pdf",
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        };
        
        return allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant());
    }
    
    private async Task<bool> HaveValidMagicNumbers(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null) return false;
        
        using var stream = file.OpenReadStream();
        var inspector = new MimeDetective.ContentInspector();
        var results = inspector.Inspect(stream);
        
        if (!results.Any()) return false;
        
        var detectedType = results.First();
        var declaredType = file.ContentType.ToLowerInvariant();
        
        // Validar que MIME type detectado coincida con declarado
        return detectedType.MimeType.Equals(declaredType, StringComparison.OrdinalIgnoreCase);
    }
}
```

**Example Request** (C# HttpClient):

```csharp
var request = new HttpRequestMessage(HttpMethod.Post, "/api/v1/attachments");
request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

var content = new MultipartFormDataContent();
content.Add(new StringContent(taskId.ToString()), "taskId");

using var fileStream = File.OpenRead("screenshot.jpg");
var fileContent = new StreamContent(fileStream);
fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
content.Add(fileContent, "file", "screenshot.jpg");

request.Content = content;

var response = await httpClient.SendAsync(request);
```

**Example Request** (cURL):

```bash
curl -X POST https://api.taskmanager.com/api/v1/attachments \
  -H "Authorization: Bearer {jwt_token}" \
  -F "taskId=550e8400-e29b-41d4-a716-446655440000" \
  -F "file=@screenshot.jpg"
```

---

## Response DTOs

### AttachmentResponse

**Purpose**: Single attachment metadata (used in POST, GET by ID)

**Properties**:

| Property | Type | Nullable | Description | Example |
|----------|------|----------|-------------|---------|
| `Id` | `Guid` | No | Unique identifier | `"38f7e9a2-c4d1-4f3a-9e7b-1234567890ab"` |
| `TaskId` | `Guid` | No | Associated task ID | `"550e8400-e29b-41d4-a716-446655440000"` |
| `FileName` | `string` | No | Original file name | `"screenshot.jpg"` |
| `FileSize` | `long` | No | File size in bytes | `2048000` (2 MB) |
| `ContentType` | `string` | No | MIME type | `"image/jpeg"` |
| `UploadedAt` | `DateTime` | No | Upload timestamp (UTC, ISO 8601) | `"2026-01-29T14:30:00Z"` |
| `UploadedByUserId` | `Guid` | No | User who uploaded | `"6ba7b810-9dad-11d1-80b4-00c04fd430c8"` |

**C# DTO**:

```csharp
public class AttachmentResponse
{
    public Guid Id { get; set; }
    public Guid TaskId { get; set; }
    public string FileName { get; set; } = string.Empty;
    public long FileSize { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public Guid UploadedByUserId { get; set; }
}
```

**Mapping from Entity**:

```csharp
public static AttachmentResponse ToResponse(this Attachment attachment)
{
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
```

**Example JSON**:

```json
{
  "id": "38f7e9a2-c4d1-4f3a-9e7b-1234567890ab",
  "taskId": "550e8400-e29b-41d4-a716-446655440000",
  "fileName": "screenshot.jpg",
  "fileSize": 2048000,
  "contentType": "image/jpeg",
  "uploadedAt": "2026-01-29T14:30:00Z",
  "uploadedByUserId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8"
}
```

---

### AttachmentListResponse

**Purpose**: List of attachments for a task (GET /task/{taskId})

**Type**: `List<AttachmentResponse>`

**Example JSON**:

```json
[
  {
    "id": "38f7e9a2-c4d1-4f3a-9e7b-1234567890ab",
    "taskId": "550e8400-e29b-41d4-a716-446655440000",
    "fileName": "screenshot.jpg",
    "fileSize": 2048000,
    "contentType": "image/jpeg",
    "uploadedAt": "2026-01-29T14:30:00Z",
    "uploadedByUserId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8"
  },
  {
    "id": "7c9e6679-7425-40de-944b-e07fc1f90ae7",
    "taskId": "550e8400-e29b-41d4-a716-446655440000",
    "fileName": "reporte.pdf",
    "fileSize": 5120000,
    "contentType": "application/pdf",
    "uploadedAt": "2026-01-28T10:15:00Z",
    "uploadedByUserId": "6ba7b810-9dad-11d1-80b4-00c04fd430c8"
  }
]
```

**Empty Result**:

```json
[]
```

---

## Error Response DTOs

### ErrorResponse

**Purpose**: Standard error format (inherited from legacy API via ExceptionsMiddleware)

**Properties**:

| Property | Type | Description |
|----------|------|-------------|
| `StatusCode` | `int` | HTTP status code |
| `Message` | `string` | Human-readable error message |
| `Details` | `string?` | Additional error details (optional, only in dev mode) |

**C# DTO**:

```csharp
public class ErrorResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? Details { get; set; }
}
```

**Examples**:

**400 Bad Request** (Validation Error):
```json
{
  "statusCode": 400,
  "message": "El archivo debe ser entre 1 byte y 10 MB",
  "details": "File size: 15728640 bytes (15 MB) exceeds maximum allowed: 10485760 bytes (10 MB)"
}
```

**400 Bad Request** (Business Rule):
```json
{
  "statusCode": 400,
  "message": "La tarea ya tiene el máximo de 5 adjuntos permitidos",
  "details": "TaskId: 550e8400-e29b-41d4-a716-446655440000, Current attachments: 5"
}
```

**401 Unauthorized**:
```json
{
  "statusCode": 401,
  "message": "Token JWT inválido o ausente",
  "details": null
}
```

**403 Forbidden**:
```json
{
  "statusCode": 403,
  "message": "Solo el propietario de la tarea puede eliminar adjuntos",
  "details": "TaskOwnerId: 6ba7b810-9dad-11d1-80b4-00c04fd430c8, RequestUserId: 7c9e6679-7425-40de-944b-e07fc1f90ae7"
}
```

**404 Not Found**:
```json
{
  "statusCode": 404,
  "message": "Adjunto no encontrado",
  "details": "AttachmentId: 38f7e9a2-c4d1-4f3a-9e7b-1234567890ab"
}
```

**413 Payload Too Large**:
```json
{
  "statusCode": 413,
  "message": "El archivo excede el tamaño máximo permitido de 10 MB",
  "details": "Received: 15 MB"
}
```

**415 Unsupported Media Type**:
```json
{
  "statusCode": 415,
  "message": "Tipo de archivo no permitido",
  "details": "ContentType: application/x-executable. Permitidos: image/jpeg, image/png, image/gif, application/pdf, application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
}
```

**500 Internal Server Error**:
```json
{
  "statusCode": 500,
  "message": "Error interno del servidor al procesar el archivo",
  "details": "IOException: Disk full" // Solo en development mode
}
```

---

## Content Negotiation

**Supported Formats**:
- `application/json` (default)
- `multipart/form-data` (upload only)

**Headers**:
- `Accept: application/json` (responses)
- `Content-Type: multipart/form-data` (upload requests)
- `Content-Type: application/json` (other requests)

---

## Future Enhancements

**Potential Additional DTOs** (not in MVP):

### PagedAttachmentListResponse
```csharp
public class PagedAttachmentListResponse
{
    public List<AttachmentResponse> Items { get; set; }
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
```

### AttachmentStatisticsResponse
```csharp
public class AttachmentStatisticsResponse
{
    public int TotalAttachments { get; set; }
    public long TotalSizeBytes { get; set; }
    public Dictionary<string, int> AttachmentsByType { get; set; } // "image/jpeg": 15, "application/pdf": 8
    public AttachmentResponse? MostRecent { get; set; }
}
```

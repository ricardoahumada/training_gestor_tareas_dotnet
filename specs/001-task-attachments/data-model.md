# Data Model: Módulo de Archivos Adjuntos para Tareas

**Feature**: Módulo de Archivos Adjuntos
**Date**: 2026-01-29
**Purpose**: Definir entidades, relaciones, validaciones y reglas de negocio del dominio

## Entity: Attachment

**Description**: Representa un archivo adjunto asociado a una tarea específica. Contiene metadata del archivo (nombre, tamaño, tipo MIME) y referencia al almacenamiento físico, sin almacenar el contenido binario en la base de datos.

### Properties

| Property | Type | Nullability | Description | Constraints |
|----------|------|-------------|-------------|-------------|
| `Id` | `Guid` | NOT NULL (PK) | Identificador único del adjunto | Primary Key, auto-generado |
| `TaskId` | `Guid` | NOT NULL (FK) | Referencia a la tarea asociada | Foreign Key (conceptual, no modifica Task legacy) |
| `FileName` | `string` | NOT NULL | Nombre original del archivo (incluye extensión) | Max length: 255, Regex: `^[a-zA-Z0-9._-]+$` |
| `FileSize` | `long` | NOT NULL | Tamaño del archivo en bytes | Min: 1, Max: 10,485,760 (10 MB) |
| `ContentType` | `string` | NOT NULL | Tipo MIME del archivo | Enum AllowedFileType (ver abajo) |
| `UploadedAt` | `DateTime` | NOT NULL | Fecha y hora de subida (UTC) | Default: `DateTime.UtcNow` |
| `UploadedByUserId` | `Guid` | NOT NULL (FK) | Usuario que subió el archivo | Foreign Key (conceptual, no modifica User legacy) |
| `StoragePath` | `string` | NOT NULL | Ruta relativa del archivo en storage | Max length: 500, Pattern: `{taskId}/{attachmentId}.{ext}` |

### Enum: AllowedFileType

**Description**: Tipos MIME permitidos para adjuntos (implementado como `enum` en Domain layer o constantes en configuración).

```csharp
public enum AllowedFileType
{
    Jpeg = 0,          // image/jpeg
    Png = 1,           // image/png
    Gif = 2,           // image/gif
    Pdf = 3,           // application/pdf
    Doc = 4,           // application/msword
    Docx = 5,          // application/vnd.openxmlformats-officedocument.wordprocessingml.document
    Xls = 6,           // application/vnd.ms-excel
    Xlsx = 7           // application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
}

public static class MimeTypeMapping
{
    public static readonly Dictionary<AllowedFileType, string> ToMimeType = new()
    {
        { AllowedFileType.Jpeg, "image/jpeg" },
        { AllowedFileType.Png, "image/png" },
        { AllowedFileType.Gif, "image/gif" },
        { AllowedFileType.Pdf, "application/pdf" },
        { AllowedFileType.Doc, "application/msword" },
        { AllowedFileType.Docx, "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
        { AllowedFileType.Xls, "application/vnd.ms-excel" },
        { AllowedFileType.Xlsx, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" }
    };
    
    public static readonly Dictionary<string, string[]> FileExtensions = new()
    {
        { "image/jpeg", new[] { ".jpg", ".jpeg" } },
        { "image/png", new[] { ".png" } },
        { "image/gif", new[] { ".gif" } },
        { "application/pdf", new[] { ".pdf" } },
        { "application/msword", new[] { ".doc" } },
        { "application/vnd.openxmlformats-officedocument.wordprocessingml.document", new[] { ".docx" } },
        { "application/vnd.ms-excel", new[] { ".xls" } },
        { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", new[] { ".xlsx" } }
    };
}
```

### Validation Rules

#### Business Rules (Domain Layer)

1. **BR-001**: Un adjunto DEBE pertenecer a una tarea activa (`Task.IsActive == true`) al momento de creación
2. **BR-002**: Una tarea NO PUEDE tener más de 5 adjuntos simultáneamente
3. **BR-003**: El tamaño del archivo DEBE ser mayor a 0 bytes y menor o igual a 10,485,760 bytes (10 MB)
4. **BR-004**: El tipo MIME DEBE estar en la lista de tipos permitidos (AllowedFileType enum)
5. **BR-005**: Solo el propietario de la tarea (`Task.CreatedByUserId`) PUEDE eliminar adjuntos
6. **BR-006**: El nombre del archivo NO DEBE contener caracteres especiales peligrosos (path traversal: `..`, `/`, `\`)
7. **BR-007**: `StoragePath` DEBE seguir patrón `{taskId}/{attachmentId}.{extension}` para prevenir colisiones
8. **BR-008**: `UploadedAt` DEBE almacenarse en UTC para consistencia temporal global

#### Database Constraints (Infrastructure Layer)

```sql
CREATE TABLE Attachments (
    Id uniqueidentifier PRIMARY KEY DEFAULT NEWID(),
    TaskId uniqueidentifier NOT NULL,
    FileName nvarchar(255) NOT NULL,
    FileSize bigint NOT NULL CHECK (FileSize > 0 AND FileSize <= 10485760),
    ContentType nvarchar(200) NOT NULL,
    UploadedAt datetime2 NOT NULL DEFAULT SYSUTCDATETIME(),
    UploadedByUserId uniqueidentifier NOT NULL,
    StoragePath nvarchar(500) NOT NULL UNIQUE,
    
    INDEX IX_Attachments_TaskId (TaskId),
    INDEX IX_Attachments_UploadedByUserId (UploadedByUserId),
    INDEX IX_Attachments_UploadedAt (UploadedAt DESC)
);
```

**Note**: NO se crea Foreign Key constraint hacia Task legacy para respetar Strangler Pattern (validación en application layer).

### Entity Framework Core Configuration

```csharp
public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        
        builder.HasKey(a => a.Id);
        
        builder.Property(a => a.Id)
            .IsRequired()
            .ValueGeneratedOnAdd();
        
        builder.Property(a => a.TaskId)
            .IsRequired();
        
        builder.Property(a => a.FileName)
            .IsRequired()
            .HasMaxLength(255);
        
        builder.Property(a => a.FileSize)
            .IsRequired();
        
        builder.Property(a => a.ContentType)
            .IsRequired()
            .HasMaxLength(200);
        
        builder.Property(a => a.UploadedAt)
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");
        
        builder.Property(a => a.UploadedByUserId)
            .IsRequired();
        
        builder.Property(a => a.StoragePath)
            .IsRequired()
            .HasMaxLength(500);
        
        // Indexes for query performance
        builder.HasIndex(a => a.TaskId)
            .HasDatabaseName("IX_Attachments_TaskId");
        
        builder.HasIndex(a => a.UploadedByUserId)
            .HasDatabaseName("IX_Attachments_UploadedByUserId");
        
        builder.HasIndex(a => a.UploadedAt)
            .IsDescending()
            .HasDatabaseName("IX_Attachments_UploadedAt");
        
        // Unique constraint on storage path to prevent duplicates
        builder.HasIndex(a => a.StoragePath)
            .IsUnique()
            .HasDatabaseName("IX_Attachments_StoragePath_Unique");
    }
}
```

## Relationships

### Attachment → Task (N:1) - Conceptual Only

**Cardinality**: Many Attachments to One Task

**Description**: Cada adjunto pertenece a exactamente una tarea. Múltiples adjuntos pueden asociarse a la misma tarea (límite: 5).

**Implementation**:
- **NO se modifica** la entidad `Task` legacy para agregar propiedad de navegación `ICollection<Attachment> Attachments`
- Strangler Pattern: relación establecida mediante `TaskId` foreign key conceptual
- Validación de existencia de tarea mediante `ITaskReadOnlyRepository` (ver research.md)
- Cascading delete: implementado mediante domain event `TaskDeletedEvent` (ver research.md)

**Query Pattern**:
```csharp
// Obtener adjuntos de una tarea
var attachments = await _dbContext.Attachments
    .Where(a => a.TaskId == taskId)
    .OrderByDescending(a => a.UploadedAt)
    .ToListAsync();

// Contar adjuntos de una tarea (para validar límite de 5)
var count = await _dbContext.Attachments
    .CountAsync(a => a.TaskId == taskId);
```

### Attachment → User (N:1) - Conceptual Only

**Cardinality**: Many Attachments to One User

**Description**: Cada adjunto fue subido por exactamente un usuario (auditoría). Un usuario puede subir múltiples adjuntos.

**Implementation**:
- **NO se modifica** la entidad `User` legacy
- Relación establecida mediante `UploadedByUserId` foreign key conceptual
- `UserId` obtenido de JWT claims en controller: `User.FindFirst("userId")?.Value`
- No se valida existencia de usuario (asumimos token JWT válido garantiza usuario existente)

**Query Pattern**:
```csharp
// Obtener adjuntos subidos por un usuario
var userAttachments = await _dbContext.Attachments
    .Where(a => a.UploadedByUserId == userId)
    .ToListAsync();
```

## State Transitions

**Attachment Lifecycle**:

```
[Uploaded] → (User uploads file) → [Active]
[Active] → (User deletes attachment) → [Deleted]
[Active] → (Task marked inactive) → [Deleted] (cascade)
[Active] → (Storage cleanup job) → [Deleted] (orphan cleanup)
```

**State**: Attachment NO tiene campo `Status` explícito. Estados son:
- **Active**: Registro existe en DB + archivo existe en storage
- **Deleted**: Registro eliminado de DB + archivo eliminado de storage

**Soft Delete**: NO implementado (eliminación física inmediata). Si se requiere soft delete en futuro:
```csharp
public class Attachment
{
    // ... propiedades existentes
    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public Guid? DeletedByUserId { get; set; }
}
```

## Invariants

**Domain Invariants** (siempre deben ser verdaderos):

1. **INV-001**: `FileSize > 0 && FileSize <= 10,485,760`
2. **INV-002**: `ContentType` ∈ {AllowedFileType enum values}
3. **INV-003**: `StoragePath` es único en toda la tabla
4. **INV-004**: `UploadedAt` <= DateTime.UtcNow (no fechas futuras)
5. **INV-005**: `FileName` NO contiene caracteres: `..`, `/`, `\`, `:`, `*`, `?`, `"`, `<`, `>`, `|`
6. **INV-006**: Para cualquier `TaskId`, COUNT(Attachments WHERE TaskId = X) <= 5
7. **INV-007**: Si existe Attachment con `TaskId`, entonces existe Task con ese Id en sistema legacy (validado en service layer)

**Enforcement**:
- Invariants 1-5: Validados en `UploadAttachmentRequestValidator` (FluentValidation)
- Invariant 6: Validado en `AttachmentService.UploadAsync()` antes de persistencia
- Invariant 7: Validado en `AttachmentService.UploadAsync()` mediante `ITaskReadOnlyRepository.ExistsAsync()`

## Domain Events

### AttachmentUploadedEvent

**Trigger**: Al completar upload exitoso de archivo

**Payload**:
```csharp
public class AttachmentUploadedEvent : IDomainEvent
{
    public Guid AttachmentId { get; init; }
    public Guid TaskId { get; init; }
    public Guid UploadedByUserId { get; init; }
    public string FileName { get; init; }
    public long FileSize { get; init; }
    public DateTime UploadedAt { get; init; }
}
```

**Subscribers**: 
- `NotificationService` (futuro): notificar a miembros del equipo de nuevo adjunto
- `AuditService` (futuro): registrar evento en audit log

### AttachmentDeletedEvent

**Trigger**: Al completar eliminación exitosa de archivo

**Payload**:
```csharp
public class AttachmentDeletedEvent : IDomainEvent
{
    public Guid AttachmentId { get; init; }
    public Guid TaskId { get; init; }
    public Guid DeletedByUserId { get; init; }
    public DateTime DeletedAt { get; init; }
}
```

**Subscribers**:
- `AuditService` (futuro): registrar evento en audit log

## Aggregate Roots

**Attachment** es su propio Aggregate Root:
- NO es parte del agregado `Task` (Strangler Pattern: no modificar Task legacy)
- Tiene identidad independiente (Guid)
- Se persiste y recupera de forma independiente
- Consistencia garantizada dentro de sus propios invariants

**Design Decision**: En arquitectura ideal, Attachment podría ser Entity dentro del agregado Task. Sin embargo, restricciones de Strangler Pattern requieren separación. Consecuencias:
- ✅ No modifica código legacy
- ⚠️ Posible inconsistencia temporal (Attachment sin Task válido durante race conditions) → mitigado con validación en service layer
- ⚠️ Transacciones no abarcan Task + Attachment → aceptable dado que Task.IsActive es soft delete (no se pierde información crítica)

## Performance Considerations

**Indexes**:
- `IX_Attachments_TaskId`: Acelerar queries de "adjuntos por tarea" (query más frecuente)
- `IX_Attachments_UploadedByUserId`: Acelerar queries de "adjuntos por usuario"
- `IX_Attachments_UploadedAt DESC`: Acelerar queries ordenadas por fecha reciente
- `IX_Attachments_StoragePath_Unique`: Garantizar unicidad + acelerar lookups por path

**Query Optimization**:
```csharp
// ❌ BAD: N+1 query problem
var tasks = await _dbContext.Tasks.ToListAsync();
foreach (var task in tasks)
{
    var attachments = await _dbContext.Attachments
        .Where(a => a.TaskId == task.Id)
        .ToListAsync();
}

// ✅ GOOD: Single query with grouping
var taskIds = tasks.Select(t => t.Id).ToList();
var attachments = await _dbContext.Attachments
    .Where(a => taskIds.Contains(a.TaskId))
    .ToListAsync();
var grouped = attachments.GroupBy(a => a.TaskId);
```

**Pagination**:
```csharp
// Para tareas con muchos adjuntos (>20), paginar listado
public async Task<PagedResult<Attachment>> GetAttachmentsPagedAsync(
    Guid taskId, int pageNumber, int pageSize)
{
    var query = _dbContext.Attachments
        .Where(a => a.TaskId == taskId)
        .OrderByDescending(a => a.UploadedAt);
    
    var totalCount = await query.CountAsync();
    var items = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();
    
    return new PagedResult<Attachment>(items, totalCount, pageNumber, pageSize);
}
```

## Migration Strategy

**Initial Migration** (crear tabla Attachments):
```csharp
public partial class AddAttachmentsTable : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "Attachments",
            columns: table => new
            {
                Id = table.Column<Guid>(nullable: false),
                TaskId = table.Column<Guid>(nullable: false),
                FileName = table.Column<string>(maxLength: 255, nullable: false),
                FileSize = table.Column<long>(nullable: false),
                ContentType = table.Column<string>(maxLength: 200, nullable: false),
                UploadedAt = table.Column<DateTime>(nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                UploadedByUserId = table.Column<Guid>(nullable: false),
                StoragePath = table.Column<string>(maxLength: 500, nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Attachments", x => x.Id);
                table.CheckConstraint("CK_Attachments_FileSize", "FileSize > 0 AND FileSize <= 10485760");
            });

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_TaskId",
            table: "Attachments",
            column: "TaskId");

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_UploadedByUserId",
            table: "Attachments",
            column: "UploadedByUserId");

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_UploadedAt",
            table: "Attachments",
            column: "UploadedAt",
            descending: true);

        migrationBuilder.CreateIndex(
            name: "IX_Attachments_StoragePath_Unique",
            table: "Attachments",
            column: "StoragePath",
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "Attachments");
    }
}
```

**Data Seeding** (opcional, para desarrollo/pruebas):
```csharp
public class AttachmentsDbContextSeed
{
    public static async Task SeedAsync(AttachmentsDbContext context)
    {
        if (!await context.Attachments.AnyAsync())
        {
            var sampleAttachments = new[]
            {
                new Attachment
                {
                    Id = Guid.NewGuid(),
                    TaskId = Guid.Parse("00000000-0000-0000-0000-000000000001"), // Task sample Id
                    FileName = "screenshot.jpg",
                    FileSize = 2048000, // 2 MB
                    ContentType = "image/jpeg",
                    UploadedAt = DateTime.UtcNow.AddDays(-7),
                    UploadedByUserId = Guid.Parse("00000000-0000-0000-0000-000000000001"), // User sample Id
                    StoragePath = "00000000-0000-0000-0000-000000000001/12345678-1234-1234-1234-123456789012.jpg"
                }
            };
            
            await context.Attachments.AddRangeAsync(sampleAttachments);
            await context.SaveChangesAsync();
        }
    }
}
```

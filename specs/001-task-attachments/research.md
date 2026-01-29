# Research: Módulo de Archivos Adjuntos para Tareas

**Feature**: Módulo de Archivos Adjuntos
**Date**: 2026-01-29
**Purpose**: Resolver ambigüedades técnicas y establecer decisiones arquitectónicas antes de implementación

## Research Areas

### 1. Almacenamiento de Archivos

**Context**: Spec requiere almacenar archivos binarios (hasta 10MB) de forma persistente y segura.

**Options Evaluated**:

| Option | Pros | Cons | Decision |
|--------|------|------|----------|
| **A. Disco local (filesystem)** | ✅ Simplicidad implementación<br>✅ Cero costos infraestructura<br>✅ Acceso rápido (I/O local)<br>✅ No requiere configuración externa | ❌ No escalable horizontalmente<br>❌ Backups manuales<br>❌ Sin redundancia integrada<br>❌ Dificulta balanceo de carga | ✅ **SELECTED for MVP** |
| **B. Azure Blob Storage** | ✅ Escalabilidad infinita<br>✅ Redundancia geográfica<br>✅ CDN integration<br>✅ Backup automático | ❌ Costos operativos ($0.02/GB/mes)<br>❌ Latencia de red<br>❌ Requiere cuenta Azure<br>❌ Complejidad configuración | 🔄 **Future migration** |
| **C. AWS S3** | ✅ Similares ventajas a Azure Blob<br>✅ Amplia adopción industria | ❌ Similares desventajas a Azure Blob<br>❌ Vendor lock-in diferente | ⏸️ **Alternative** |
| **D. Base de datos (VARBINARY)** | ✅ Transacciones ACID<br>✅ Backup unificado con metadata | ❌ Límites de tamaño (SQL Server: 2GB max)<br>❌ Performance degradation<br>❌ Costos almacenamiento DB alto | ❌ **Rejected** |

**Decision**: **Option A - Disco Local para MVP**

**Rationale**:
- MVP requiere simplicidad y velocidad de desarrollo
- Equipo pequeño (<100 usuarios) no justifica complejidad de blob storage
- Diseño usa abstracción `IFileStorageService` → migración futura transparente
- Path configurado en `appsettings.json` (`FileStorage:BasePath`)
- Estructura de carpetas: `uploads/{taskId}/{attachmentId}.{ext}`

**Implementation Pattern**:
```csharp
public interface IFileStorageService
{
    Task<string> SaveFileAsync(Stream fileStream, string fileName, string contentType);
    Task<Stream> GetFileAsync(string storagePath);
    Task DeleteFileAsync(string storagePath);
}
```

**Migration Path to Azure Blob** (future):
1. Implementar `AzureBlobStorageService : IFileStorageService`
2. Configurar connection string en appsettings
3. Cambiar registration en DI container
4. Ejecutar script de migración para mover archivos existentes
5. ¡Cero cambios en service layer o controllers!

---

### 2. Validación de Tipos MIME

**Context**: Spec requiere validar tipos de archivo permitidos para prevenir uploads maliciosos.

**Clarification from Spec**: [NEEDS CLARIFICATION: validación por extensión o por MIME type del contenido?]

**Options Evaluated**:

| Option | Security Level | Complexity | Performance | Decision |
|--------|---------------|------------|-------------|----------|
| **A. Extension-only** | ⚠️ LOW (bypassable renaming .exe → .jpg) | 🟢 LOW | 🟢 FAST | ❌ **Rejected** (inseguro) |
| **B. Content-Type header** | ⚠️ LOW (client-controlled header) | 🟢 LOW | 🟢 FAST | ❌ **Rejected** (inseguro) |
| **C. Magic numbers (file signature)** | ✅ HIGH (valida bytes iniciales del archivo) | 🟡 MEDIUM | 🟡 MEDIUM | ✅ **SELECTED** |
| **D. Hybrid (C + A)** | ✅ HIGH (doble validación) | 🔴 HIGH | 🟡 MEDIUM | 🔄 **Alternative** |

**Decision**: **Option C - Magic Numbers Validation**

**Rationale**:
- Previene ataques de archivos maliciosos con extensión renombrada
- Balance seguridad/complejidad adecuado para entorno corporativo
- Librería recomendada: **`MimeDetective`** (NuGet package)
- Validación en `UploadAttachmentRequestValidator` (FluentValidation custom rule)

**Allowed File Signatures (Magic Numbers)**:

| File Type | Magic Number (Hex) | Offset |
|-----------|-------------------|--------|
| JPEG | `FF D8 FF` | 0 |
| PNG | `89 50 4E 47` | 0 |
| GIF | `47 49 46 38` | 0 |
| PDF | `25 50 44 46` | 0 |
| DOC | `D0 CF 11 E0 A1 B1 1A E1` | 0 |
| DOCX/XLSX | `50 4B 03 04` (ZIP) + validar XML interno | 0 |

**Implementation Pattern**:
```csharp
public class FileTypeValidator
{
    public bool IsAllowedType(Stream fileStream, string fileName)
    {
        var detector = new MimeDetective.ContentInspector();
        var detectedType = detector.Inspect(fileStream);
        
        // Validar contra whitelist de tipos permitidos
        return AllowedMimeTypes.Contains(detectedType.MimeType);
    }
}
```

**Recommendation**: Option D (Hybrid) para producción con datos sensibles, pero Option C suficiente para MVP.

---

### 3. Manejo de Nombres de Archivo Duplicados

**Clarification from Spec**: [NEEDS CLARIFICATION: estrategia de naming de duplicados]

**Options Evaluated**:

| Option | UX Impact | Complexity | Data Integrity | Decision |
|--------|-----------|------------|----------------|----------|
| **A. Allow duplicates (diff by ID)** | ⚠️ Confusing UI (multiple "doc.pdf") | 🟢 LOW | ✅ SAFE | ✅ **SELECTED** |
| **B. Auto-rename with suffix** | ✅ Clear UI ("doc_1.pdf", "doc_2.pdf") | 🟡 MEDIUM | ✅ SAFE | 🔄 **Alternative** |
| **C. Reject duplicate (409 Conflict)** | ❌ User friction (manual rename required) | 🟢 LOW | ✅ SAFE | ❌ **Rejected** |

**Decision**: **Option A - Allow Duplicates**

**Rationale**:
- Simplicidad técnica (sin lógica de generación de nombres únicos)
- Casos de uso reales: usuarios suben múltiples versiones del mismo documento ("reporte.pdf" v1, v2, v3)
- UI debe mostrar metadata adicional para diferenciar: `fileName + uploadedAt + uploadedBy`
- Ejemplo UI: `reporte.pdf (2026-01-29 14:30, Juan Pérez)`
- Almacenamiento físico usa GUID: `uploads/taskId/38f7e9a2-xxx.pdf` (sin colisiones)

**Implementation**:
- `Attachment.FileName` almacena nombre original sin modificar
- `Attachment.Id` (GUID) garantiza unicidad
- Physical storage path: `{basePath}/{taskId}/{attachmentId}{extension}`
- Download response header: `Content-Disposition: attachment; filename="{originalFileName}"`

**Alternative Option B** (si Product Owner prefiere auto-rename):
```csharp
private string GenerateUniqueFileName(string taskId, string originalFileName)
{
    var baseName = Path.GetFileNameWithoutExtension(originalFileName);
    var extension = Path.GetExtension(originalFileName);
    var counter = 1;
    
    while (await _repository.ExistsAsync(taskId, $"{baseName}_{counter}{extension}"))
        counter++;
    
    return $"{baseName}_{counter}{extension}";
}
```

---

### 4. Integración con Entidad Task Legacy

**Context**: Módulo necesita validar existencia de tarea y propiedad (CreatedByUserId) sin modificar entidad `Task` legacy.

**Options Evaluated**:

| Option | Coupling Level | Complexity | Constitution Compliance | Decision |
|--------|---------------|------------|-------------------------|----------|
| **A. Direct query to ApplicationDbContext** | 🔴 HIGH (acoplamiento a DbContext legacy) | 🟢 LOW | ❌ Viola Strangler Pattern | ❌ **Rejected** |
| **B. Shared interface ITaskRepository** | 🟡 MEDIUM (dependencia a interfaz legacy) | 🟡 MEDIUM | ⚠️ Acoplamiento aceptable | 🔄 **Alternative** |
| **C. HTTP call to TasksController** | 🟢 LOW (comunicación por API) | 🔴 HIGH | ✅ Total desacoplamiento | ❌ **Rejected** (overhead) |
| **D. Read-only access via separate repo** | 🟢 LOW (solo lectura, sin modificar Task) | 🟡 MEDIUM | ✅ Compliant | ✅ **SELECTED** |

**Decision**: **Option D - Read-Only Repository Wrapper**

**Rationale**:
- Crea `ITaskReadOnlyRepository` en módulo Attachments
- Implementación accede a `ApplicationDbContext` legacy en modo **read-only**
- NO modifica entidad `Task`, solo consulta propiedades públicas: `Id`, `IsActive`, `CreatedByUserId`
- Respeta Strangler Pattern: integración sin modificación
- Si Task legacy cambia schema, solo se actualiza wrapper (cambio localizado)

**Implementation Pattern**:
```csharp
// En TaskManager.Attachments.Domain/Interfaces
public interface ITaskReadOnlyRepository
{
    Task<bool> ExistsAsync(Guid taskId);
    Task<bool> IsActiveAsync(Guid taskId);
    Task<Guid?> GetOwnerIdAsync(Guid taskId);
}

// En TaskManager.Attachments.Infrastructure/Data/Repositories
public class TaskReadOnlyRepository : ITaskReadOnlyRepository
{
    private readonly ApplicationDbContext _legacyContext; // Inyectado, no modificado
    
    public async Task<bool> ExistsAsync(Guid taskId)
    {
        return await _legacyContext.Tasks.AnyAsync(t => t.Id == taskId);
    }
    
    public async Task<bool> IsActiveAsync(Guid taskId)
    {
        return await _legacyContext.Tasks
            .Where(t => t.Id == taskId)
            .Select(t => t.IsActive)
            .FirstOrDefaultAsync();
    }
    
    public async Task<Guid?> GetOwnerIdAsync(Guid taskId)
    {
        return await _legacyContext.Tasks
            .Where(t => t.Id == taskId)
            .Select(t => t.CreatedByUserId)
            .FirstOrDefaultAsync();
    }
}
```

**Alternative Option B** (si existe `ITaskRepository` legacy):
- Reutilizar interfaz existente (menor código)
- Pero crea dependencia a assembly legacy Application layer
- Evaluar trade-off: código duplicado vs. acoplamiento

---

### 5. Rate Limiting para Prevención de Abuso

**Clarification from Spec**: [NEEDS CLARIFICATION: rate limiting en MVP o producción?]

**Options Evaluated**:

| Option | Security Level | MVP Impact | Complexity | Decision |
|--------|---------------|------------|------------|----------|
| **A. No rate limiting (MVP)** | ❌ Vulnerable a DoS | 🟢 Fast MVP delivery | 🟢 NONE | ✅ **SELECTED for MVP** |
| **B. Kestrel-level config** | ⚠️ Basic protection | 🟢 Minimal impact | 🟢 LOW (appsettings) | 🔄 **Recommended for MVP** |
| **C. Middleware custom** | ✅ Granular control | 🟡 Moderate impact | 🟡 MEDIUM | 🔄 **For production** |
| **D. AspNetCoreRateLimit lib** | ✅ Full-featured | 🔴 Delays MVP | 🟡 MEDIUM | 🔄 **For production** |

**Decision**: **Option A for MVP, migrate to Option B before production**

**Rationale**:
- MVP con usuarios controlados (<100) no requiere rate limiting sofisticado
- Validaciones existentes (tamaño 10MB, máx 5 archivos/tarea) ya limitan abuso
- ExceptionsMiddleware ya registra errores 500 para detectar patrones anómalos
- **Recomendación pre-producción**: Implementar Option B

**Option B Implementation** (simple, recomendado antes de producción):
```json
// appsettings.json
{
  "Kestrel": {
    "Limits": {
      "MaxConcurrentConnections": 100,
      "MaxConcurrentUpgradedConnections": 100,
      "MaxRequestBodySize": 10485760, // 10 MB
      "RequestHeadersTimeout": "00:00:30"
    }
  }
}
```

**Option C Implementation** (producción con alto tráfico):
```csharp
// Middleware custom con sliding window
public class UploadRateLimitMiddleware
{
    private readonly IMemoryCache _cache;
    
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api/v1/attachments") 
            && context.Request.Method == "POST")
        {
            var userId = context.User.FindFirst("userId")?.Value;
            var cacheKey = $"upload_rate_{userId}";
            
            var uploadCount = _cache.GetOrCreate(cacheKey, entry => 
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1);
                return 0;
            });
            
            if (uploadCount >= 10) // Max 10 uploads/min
            {
                context.Response.StatusCode = 429; // Too Many Requests
                await context.Response.WriteAsync("Rate limit exceeded: max 10 uploads per minute");
                return;
            }
            
            _cache.Set(cacheKey, uploadCount + 1);
        }
        
        await _next(context);
    }
}
```

---

### 6. Eliminación de Adjuntos al Eliminar Tarea

**Context**: Spec FR-008 requiere eliminar automáticamente adjuntos cuando tarea se marca como inactiva.

**Options Evaluated**:

| Option | Timing | Reliability | Complexity | Decision |
|--------|--------|-------------|------------|----------|
| **A. Synchronous delete in TaskService** | Inmediato | ⚠️ Puede fallar y bloquear delete de Task | 🟢 LOW | ❌ **Rejected** (acoplamiento) |
| **B. Database cascade delete** | Inmediato | ✅ Transaccional | 🟢 LOW | ⚠️ **Solo metadata, no archivos físicos** |
| **C. Background job (Hangfire/Quartz)** | Asíncrono (segundos/minutos) | ✅ Resiliente | 🔴 HIGH | 🔄 **For production** |
| **D. Domain event + handler** | Asíncrono (in-process) | ✅ Desacoplado | 🟡 MEDIUM | ✅ **SELECTED** |

**Decision**: **Option D - Domain Event Handler**

**Rationale**:
- Mantiene desacoplamiento: TaskService legacy NO conoce módulo Attachments
- Attachments module subscribe to `TaskDeletedEvent` (patrón Observer)
- Resiliente: si delete de archivos falla, se registra error y se puede reintentar
- No bloquea operación de eliminación de tarea
- Consistencia eventual aceptable (archivos huérfanos temporales pueden limpiarse con job nocturno)

**Implementation Pattern**:
```csharp
// En TaskManager.Domain/Events (si no existe, crear)
public class TaskDeletedEvent : IDomainEvent
{
    public Guid TaskId { get; init; }
    public DateTime DeletedAt { get; init; }
}

// En TaskManager.Attachments.Application/EventHandlers
public class TaskDeletedEventHandler : IEventHandler<TaskDeletedEvent>
{
    private readonly IAttachmentService _attachmentService;
    private readonly ILogger<TaskDeletedEventHandler> _logger;
    
    public async Task HandleAsync(TaskDeletedEvent @event)
    {
        try
        {
            var attachments = await _attachmentService.GetAttachmentsByTaskIdAsync(@event.TaskId);
            
            foreach (var attachment in attachments)
            {
                await _attachmentService.DeleteAttachmentAsync(attachment.Id);
                _logger.LogInformation("Deleted attachment {AttachmentId} from task {TaskId}", 
                    attachment.Id, @event.TaskId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete attachments for task {TaskId}. Will retry in cleanup job.", 
                @event.TaskId);
            // No throw - no bloquear eliminación de tarea
        }
    }
}
```

**Fallback**: Cleanup job nocturno para detectar y eliminar archivos huérfanos:
```csharp
// Ejecutar diario a las 2 AM
public class OrphanAttachmentsCleanupJob
{
    public async Task ExecuteAsync()
    {
        var orphanAttachments = await _repository
            .GetAttachmentsWithDeletedTasksAsync(); // LEFT JOIN Task WHERE Task.IsActive = false
        
        foreach (var orphan in orphanAttachments)
        {
            await _attachmentService.DeleteAttachmentAsync(orphan.Id);
            _logger.LogWarning("Cleaned orphan attachment {AttachmentId}", orphan.Id);
        }
    }
}
```

---

## Summary of Decisions

| Research Area | Decision | Rationale | Implementation Priority |
|---------------|----------|-----------|------------------------|
| **File Storage** | Disco local (MVP) → Azure Blob (future) | Simplicidad + diseño para migración | 🔴 P1 (MVP Core) |
| **MIME Validation** | Magic numbers (MimeDetective lib) | Seguridad alta sin complejidad excesiva | 🔴 P1 (Security) |
| **Duplicate Filenames** | Allow duplicates, diff by ID + metadata | Simplicidad técnica, casos de uso reales | 🟡 P2 (Enhancement) |
| **Task Integration** | Read-only repository wrapper | Desacoplamiento + Strangler Pattern | 🔴 P1 (MVP Core) |
| **Rate Limiting** | Skip MVP, add Kestrel config pre-prod | Balance velocidad/seguridad | 🟢 P3 (Pre-production) |
| **Cascade Delete** | Domain event handler + cleanup job | Resiliencia + desacoplamiento | 🟡 P2 (Operational) |

**All NEEDS CLARIFICATION from spec.md resolved** ✅

**Ready to proceed to Phase 1 (Data Model + Contracts)** 🚀

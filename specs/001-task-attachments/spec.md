# Feature Specification: Módulo de Archivos Adjuntos para Tareas

**Feature Branch**: `001-task-attachments`  
**Created**: 2026-01-29  
**Status**: Draft  
**Input**: User description: "Módulo de archivos adjuntos para tareas"

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Adjuntar archivo a tarea (Priority: P1)

Un usuario autenticado necesita adjuntar documentos de soporte (imágenes, PDFs, documentos Office) a una tarea activa para compartir información relevante con otros miembros del equipo.

**Why this priority**: Esta es la funcionalidad core del módulo. Sin la capacidad de subir archivos, ninguna otra funcionalidad tiene valor. Es el MVP mínimo viable.

**Independent Test**: Puede probarse completamente mediante: (1) autenticación de usuario, (2) selección de tarea activa existente, (3) upload de archivo válido (jpg ≤10MB), (4) verificación de respuesta exitosa con metadata del archivo adjunto. Entrega valor independiente: usuarios pueden documentar tareas con evidencia visual/documental.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado con token JWT válido y una tarea activa con Id=123, **When** el usuario sube un archivo imagen "screenshot.jpg" de 2MB, **Then** el sistema responde con código 201, metadata del adjunto (id, nombre, tamaño, tipo MIME, fechaSubida, taskId=123) y el archivo se almacena correctamente
2. **Given** un usuario autenticado y una tarea activa sin adjuntos previos, **When** sube un PDF "reporte.pdf" de 5MB, **Then** el sistema acepta el archivo, retorna metadata con tipo MIME "application/pdf" y el contador de adjuntos de la tarea es 1
3. **Given** un usuario autenticado y una tarea activa con 4 adjuntos existentes, **When** intenta subir un quinto archivo "doc.docx" de 3MB, **Then** el sistema acepta el archivo (límite es 5) y el contador de adjuntos es 5
4. **Given** un usuario sin autenticar (sin token JWT), **When** intenta subir un archivo a una tarea, **Then** el sistema responde con código 401 Unauthorized
5. **Given** un usuario autenticado, **When** intenta subir un archivo a una tarea inactiva (IsActive=false), **Then** el sistema responde con código 400 Bad Request con mensaje "No se pueden adjuntar archivos a tareas inactivas"

---

### User Story 2 - Listar adjuntos de tarea (Priority: P2)

Un usuario autenticado necesita visualizar todos los archivos adjuntos asociados a una tarea específica para revisar documentación previa o descargar archivos compartidos.

**Why this priority**: Una vez que los usuarios pueden subir archivos (P1), necesitan poder consultarlos. Sin esta funcionalidad, los archivos quedarían "ciegos" en el sistema. Es el complemento natural del upload.

**Independent Test**: Puede probarse independientemente mediante: (1) autenticación de usuario, (2) creación de tarea con 3 adjuntos previamente subidos, (3) consulta GET del listado de adjuntos, (4) verificación de respuesta con array de metadata (id, nombre, tamaño, tipo MIME, fechaSubida). Entrega valor: usuarios ven inventario de archivos sin necesidad de descargarlos.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado y una tarea con 3 adjuntos (1 jpg, 1 pdf, 1 docx), **When** consulta la lista de adjuntos de la tarea Id=123, **Then** el sistema responde con código 200 y un array de 3 elementos con metadata completa (id, nombre, tamaño, tipoMIME, fechaSubida, taskId)
2. **Given** un usuario autenticado y una tarea sin adjuntos, **When** consulta la lista de adjuntos, **Then** el sistema responde con código 200 y un array vacío []
3. **Given** un usuario sin autenticar, **When** intenta consultar adjuntos de una tarea, **Then** el sistema responde con código 401 Unauthorized
4. **Given** un usuario autenticado, **When** consulta adjuntos de una tarea inexistente (Id=999), **Then** el sistema responde con código 404 Not Found

---

### User Story 3 - Descargar archivo adjunto (Priority: P3)

Un usuario autenticado necesita descargar un archivo específico previamente adjuntado a una tarea para revisarlo, editarlo o compartirlo fuera del sistema.

**Why this priority**: Complementa P1 (upload) y P2 (listado). Permite consumir los archivos subidos. Sin embargo, el listado (P2) ya entrega valor informativo; la descarga es el paso final de uso intensivo.

**Independent Test**: Puede probarse independientemente mediante: (1) autenticación, (2) tarea con adjunto existente Id=456, (3) solicitud GET de descarga por attachmentId, (4) verificación de respuesta con archivo binario y headers correctos (Content-Type, Content-Disposition). Entrega valor: acceso completo al contenido de archivos.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado y un adjunto existente Id=456 de tipo jpg en tarea Id=123, **When** solicita descargar el adjunto Id=456, **Then** el sistema responde con código 200, el archivo binario completo, header "Content-Type: image/jpeg" y "Content-Disposition: attachment; filename=screenshot.jpg"
2. **Given** un usuario autenticado, **When** solicita descargar un adjunto inexistente Id=999, **Then** el sistema responde con código 404 Not Found
3. **Given** un usuario sin autenticar, **When** intenta descargar un adjunto, **Then** el sistema responde con código 401 Unauthorized
4. **Given** un usuario autenticado y un adjunto de 8MB, **When** solicita descargarlo, **Then** el sistema transmite el archivo completo sin corrupción (hash/checksum coincide con el original)

---

### User Story 4 - Eliminar archivo adjunto (Priority: P4)

Un usuario autenticado con permisos adecuados necesita eliminar un adjunto específico de una tarea (por error de upload, archivo incorrecto, o información obsoleta) sin afectar otros adjuntos de la misma tarea.

**Why this priority**: Funcionalidad de mantenimiento. Menos crítica que upload/listado/descarga, pero necesaria para corregir errores y gestionar ciclo de vida de archivos. Puede diferirse si el equipo requiere MVP más rápido.

**Independent Test**: Puede probarse independientemente mediante: (1) autenticación, (2) tarea con 3 adjuntos, (3) solicitud DELETE de un adjunto específico, (4) verificación de respuesta 204 No Content, (5) validación de que solo ese adjunto se eliminó (listado muestra 2 adjuntos restantes). Entrega valor: control granular de archivos adjuntos.

**Acceptance Scenarios**:

1. **Given** un usuario autenticado propietario de la tarea, una tarea con 3 adjuntos, **When** elimina el adjunto Id=456, **Then** el sistema responde con código 204 No Content, el archivo se elimina del almacenamiento, y el listado de adjuntos muestra solo 2 archivos restantes
2. **Given** un usuario autenticado NO propietario de la tarea, **When** intenta eliminar un adjunto, **Then** el sistema responde con código 403 Forbidden con mensaje "Solo el propietario de la tarea puede eliminar adjuntos"
3. **Given** un usuario autenticado propietario, **When** intenta eliminar un adjunto inexistente Id=999, **Then** el sistema responde con código 404 Not Found
4. **Given** un usuario sin autenticar, **When** intenta eliminar un adjunto, **Then** el sistema responde con código 401 Unauthorized
5. **Given** un usuario autenticado propietario y una tarea que se elimina (soft delete IsActive=false), **When** la tarea se marca como inactiva, **Then** todos los adjuntos asociados se eliminan automáticamente del almacenamiento (requisito funcional #6)

---

### Edge Cases

- **Límite de 5 adjuntos por tarea**: ¿Qué sucede cuando un usuario intenta subir un sexto archivo? Sistema debe responder con código 400 Bad Request y mensaje "La tarea ya tiene el máximo de 5 adjuntos permitidos"
- **Archivo de tamaño exacto 10 MB**: ¿Es aceptado o rechazado? Asumimos que 10MB es el límite INCLUSIVO (≤10MB es válido), >10MB es rechazado con código 413 Payload Too Large
- **Tipos MIME no permitidos**: ¿Qué sucede si se intenta subir .exe, .zip, .sh? Sistema valida extensión y tipo MIME, responde con código 415 Unsupported Media Type y mensaje "Tipo de archivo no permitido. Permitidos: jpg, png, gif, pdf, doc, docx, xls, xlsx"
- **Archivo con nombre duplicado en la misma tarea**: Sistema permite nombres duplicados y diferencia por attachmentId único, o renombra automáticamente agregando sufijo (screenshot.jpg → screenshot_1.jpg). [NEEDS CLARIFICATION: estrategia de naming de duplicados - permitir duplicados o renombrar automáticamente?]
- **Concurrencia: dos usuarios suben simultáneamente el quinto archivo a una tarea con 4 adjuntos**: Sistema debe manejar race condition con transacción atómica o lock optimista. Solo uno debe exitir, el otro debe recibir 400 Bad Request.
- **Eliminación de tarea mientras se descarga adjunto**: Sistema debe completar la descarga en curso o responder con 410 Gone si el archivo ya no existe.
- **Adjunto sin extensión o con múltiples extensiones** (archivo.tar.gz): Sistema valida por tipo MIME del contenido (magic numbers) además de extensión. [NEEDS CLARIFICATION: validación primaria por extensión o por MIME type del contenido?]
- **Usuario con token JWT expirado**: Sistema responde con código 401 Unauthorized con mensaje "Token expirado, renueve su sesión"
- **Almacenamiento lleno o error I/O al guardar archivo**: Sistema responde con código 500 Internal Server Error, registra error en logs, y NO crea registro de adjunto en base de datos (rollback de transacción)
- **Descarga interrumpida (cliente cierra conexión)**: Sistema debe liberar recursos (file handles) y registrar evento en logs para debugging

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: Sistema DEBE permitir a usuarios autenticados adjuntar archivos a tareas activas (IsActive=true)
- **FR-002**: Sistema DEBE validar tipos de archivo permitidos: imágenes (jpg, jpeg, png, gif), PDFs (pdf), documentos Office (doc, docx, xls, xlsx) mediante validación de extensión y tipo MIME
- **FR-003**: Sistema DEBE rechazar archivos con tamaño superior a 10 MB (10,485,760 bytes) con código HTTP 413 Payload Too Large
- **FR-004**: Sistema DEBE limitar a máximo 5 adjuntos por tarea; rechazar uploads adicionales con código 400 Bad Request
- **FR-005**: Sistema DEBE permitir a usuarios autenticados listar todos los adjuntos de una tarea específica, retornando metadata (id, nombre, tamaño, tipo MIME, fecha de subida)
- **FR-006**: Sistema DEBE permitir a usuarios autenticados descargar archivos adjuntos existentes, transmitiendo contenido binario con headers HTTP correctos (Content-Type, Content-Disposition)
- **FR-007**: Sistema DEBE permitir a usuarios autenticados propietarios de la tarea eliminar adjuntos específicos
- **FR-008**: Sistema DEBE eliminar automáticamente todos los adjuntos asociados cuando una tarea se marca como inactiva (IsActive=false) o se elimina
- **FR-009**: Sistema DEBE validar token JWT en todos los endpoints de adjuntos; rechazar solicitudes sin token con código 401 Unauthorized
- **FR-010**: Sistema DEBE verificar que la tarea existe y está activa antes de aceptar uploads; responder con 400 Bad Request si tarea inactiva, 404 Not Found si no existe
- **FR-011**: Sistema DEBE generar identificador único (GUID/UUID) para cada adjunto al momento de subida
- **FR-012**: Sistema DEBE preservar nombre original del archivo en metadata del adjunto
- **FR-013**: Sistema DEBE registrar fecha y hora de subida (timestamp UTC) en metadata del adjunto
- **FR-014**: Sistema NO DEBE modificar entidades legacy existentes (Task, User, Project) para cumplir con Strangler Pattern y Principio I de la Constitution

### Key Entities *(include if feature involves data)*

- **Attachment**: Representa un archivo adjunto asociado a una tarea. Atributos clave:
  - Id (GUID/UUID): Identificador único
  - TaskId (GUID/UUID): Referencia a la tarea asociada (foreign key, pero sin modificar entidad Task legacy)
  - FileName (string): Nombre original del archivo (incluye extensión)
  - FileSize (long): Tamaño en bytes
  - ContentType (string): Tipo MIME (image/jpeg, application/pdf, etc.)
  - UploadedAt (DateTime UTC): Fecha y hora de subida
  - UploadedByUserId (GUID/UUID): Referencia al usuario que subió el archivo
  - StoragePath (string): Ruta física o clave de almacenamiento (blob storage key) - NO exponer en API

Relación conceptual (sin modificar Task legacy):
- Attachment N:1 Task (múltiples adjuntos por tarea)
- Attachment N:1 User (usuario que subió)

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Usuarios autenticados pueden subir un archivo válido (jpg ≤10MB) a una tarea activa y recibir confirmación en menos de 5 segundos
- **SC-002**: Sistema maneja correctamente 100 uploads concurrentes sin pérdida de archivos ni duplicación de identificadores
- **SC-003**: Usuarios pueden descargar archivos de hasta 10MB en menos de 10 segundos en conexión estándar (10 Mbps)
- **SC-004**: Sistema rechaza el 100% de intentos de subida con tipos de archivo no permitidos (.exe, .zip, .sh, etc.) con mensaje de error claro
- **SC-005**: Sistema elimina todos los adjuntos asociados (archivos físicos + registros DB) cuando una tarea se marca como inactiva, en menos de 30 segundos
- **SC-006**: 95% de operaciones de upload/download/delete completan exitosamente en primer intento (sin errores 500)
- **SC-007**: Sistema mantiene integridad referencial: 0 adjuntos huérfanos (sin tarea asociada válida) en auditorías mensuales
- **SC-008**: Usuarios pueden listar adjuntos de una tarea con 100 archivos en menos de 2 segundos
- **SC-009**: Sistema integra con endpoints de tareas sin modificar código legacy (validado mediante diff de commits en carpetas src/TaskManager.*)
- **SC-010**: Cobertura de pruebas unitarias ≥80% en servicios y repositorios del módulo de adjuntos

## Assumptions *(optional)*

- Se asume almacenamiento en disco local del servidor para fase inicial (desarrollo/pruebas). Para producción, se requerirá migración a blob storage (Azure Blob, AWS S3) sin cambios en contratos de API.
- Se asume que el usuario autenticado (token JWT) tiene permisos implícitos de lectura sobre tareas del proyecto al que pertenece. Solo se valida propiedad de tarea (CreatedByUserId) para operaciones de eliminación de adjuntos.
- Se asume que los tipos MIME permitidos corresponden a extensiones según:
  - jpg/jpeg → image/jpeg
  - png → image/png
  - gif → image/gif
  - pdf → application/pdf
  - doc → application/msword
  - docx → application/vnd.openxmlformats-officedocument.wordprocessingml.document
  - xls → application/vnd.ms-excel
  - xlsx → application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
- Se asume que eliminación de tarea (IsActive=false) es eliminación lógica (soft delete), NO eliminación física. Los adjuntos SÍ se eliminan físicamente del almacenamiento.
- Se asume que no hay requisitos de cifrado en tránsito adicionales (HTTPS manejado por infraestructura) ni cifrado en reposo (filesystem estándar). Para producción con datos sensibles, requerirá clarificación de políticas de seguridad.

## Out of Scope *(optional)*

- **Versionado de archivos**: Si un usuario sube un archivo con el mismo nombre, NO se mantiene historial de versiones. Cada upload es un adjunto independiente.
- **Vista previa de archivos** (thumbnails, preview de PDFs en browser): Solo metadata y descarga binaria. UI de preview queda fuera del alcance del backend.
- **Compresión automática de imágenes**: Archivos se almacenan tal cual se suben, sin procesamiento de reducción de tamaño.
- **Escaneo antivirus/malware**: No se implementa en MVP. Para producción requerirá integración con servicio de escaneo (ClamAV, VirusTotal API, etc.).
- **Cuotas por usuario o por proyecto**: Solo se limita a 5 adjuntos por tarea. No hay límite global de almacenamiento por usuario/proyecto.
- **Notificaciones**: No se envían notificaciones (email, push) cuando se adjunta o elimina un archivo. Eso sería responsabilidad de otro módulo.
- **Auditoría detallada** (quién descargó, cuándo, desde qué IP): Solo se registra metadata de subida. Logs de acceso son responsabilidad de middleware de logging existente.
- **Compartir adjuntos públicamente** (URLs sin autenticación): Todos los endpoints requieren JWT. No hay generación de enlaces públicos temporales.

## Dependencies *(optional)*

- **Sistema de autenticación JWT existente**: El módulo depende de la infraestructura de autenticación legacy para validar tokens y extraer claims (userId, role). No se modifica esta infraestructura.
- **Entidad Task existente**: Se requiere acceso de solo lectura a la entidad Task (Id, IsActive, CreatedByUserId) para validaciones. NO se agrega propiedad de navegación Attachments a Task para respetar Strangler Pattern.
- **Middleware de manejo de errores y logging**: Se reutilizan ExceptionsMiddleware y LoggingMiddleware existentes para consistencia en respuestas de error y observabilidad.
- **Entity Framework Core**: Se utilizará para persistencia de metadata de Attachment (nueva entidad en DbContext, pero sin modificar entidades legacy).
- **Configuración de tamaño máximo de request**: El servidor web (Kestrel) debe estar configurado para aceptar requests de hasta 10MB (por defecto es 28.6MB en .NET 8, suficiente).

## Security Considerations *(optional)*

- **Validación de tipo MIME por contenido**: Además de extensión de archivo, se debe validar "magic numbers" (primeros bytes del archivo) para prevenir ataques de archivo con extensión renombrada (.exe renombrado a .jpg). [NEEDS CLARIFICATION: ¿validación estricta por magic numbers o solo extensión + header Content-Type?]
- **Path traversal prevention**: Nombres de archivo NO deben usarse directamente en paths de almacenamiento. Se debe generar nombre único (GUID) para evitar ataques de directorio traversal (../../etc/passwd).
- **Autorización granular**: Solo el propietario de la tarea (CreatedByUserId == JWT userId claim) puede eliminar adjuntos. Lectura/descarga permitida a todos los usuarios autenticados del proyecto (asumiendo acceso a nivel de proyecto ya validado por endpoints de tareas).
- **Límite de tasa (rate limiting)**: Considerar implementar throttling para endpoints de upload (ej. máximo 10 uploads por minuto por usuario) para prevenir abuso/DoS. [NEEDS CLARIFICATION: ¿se requiere rate limiting en MVP o solo para producción?]
- **Sanitización de nombres de archivo**: Al retornar nombres de archivo en responses o headers, se debe escapar caracteres especiales para prevenir inyección en logs o XSS si se renderizan en UI sin sanitización.

## Notes *(optional)*

- Este módulo es el primer caso de uso del Strangler Pattern en el proyecto TaskManager. Sirve como template para futuros módulos (Notificaciones, Comentarios, etc.) que deban integrarse sin tocar código legacy.
- La especificación prioriza user stories de forma incremental (P1→P4) para permitir entregas de valor tempranas. El equipo puede decidir implementar solo P1-P2 como MVP inicial y diferir P3-P4 según capacidad.
- Se identificaron 3 áreas que requieren clarificación con Product Owner Laura Martínez (marcadas con [NEEDS CLARIFICATION]). Estas NO bloquean inicio de Phase 1 (planificación técnica), pero deben resolverse antes de implementación.

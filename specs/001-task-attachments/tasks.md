---
description: "Implementation tasks for attachments module"
---

# Tasks: Módulo de Archivos Adjuntos para Tareas

**Input**: Design documents from `/specs/001-task-attachments/`
**Prerequisites**: [plan.md](plan.md), [spec.md](spec.md), [research.md](research.md), [data-model.md](data-model.md), [contracts/](contracts/)

**Tests**: Este proyecto NO incluye tareas de tests. Las pruebas se implementarán al final como tarea transversal.

**Organization**: Tasks are grouped by user story to enable independent implementation and testing of each story.

## Format: `[ID] [P?] [Story] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- **[Story]**: Which user story this task belongs to (e.g., US1, US2, US3, US4)
- Include exact file paths in descriptions

---

## Phase 1: Setup (Shared Infrastructure)

**Purpose**: Create project structure and basic configuration for attachments module

- [X] T001 Create solution structure: `src/TaskManager.Attachments/TaskManager.Attachments.Domain/` with Domain.csproj
- [X] T002 [P] Create application layer structure: `src/TaskManager.Attachments/TaskManager.Attachments.Application/` with Application.csproj
- [X] T003 [P] Create infrastructure layer structure: `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/` with Infrastructure.csproj
- [X] T004 [P] Create API layer structure: `src/TaskManager.Attachments/TaskManager.Attachments.Api/` with Api.csproj
- [X] T005 Add NuGet packages to Domain: no external dependencies (POCOs only)
- [X] T006 [P] Add NuGet packages to Application: FluentValidation 11.x, MimeDetective via Application.csproj
- [X] T007 [P] Add NuGet packages to Infrastructure: EntityFrameworkCore 8.0, EntityFrameworkCore.SqlServer via Infrastructure.csproj
- [X] T008 [P] Add NuGet packages to Api: Microsoft.AspNetCore.Mvc, Microsoft.AspNetCore.Authentication.JwtBearer via Api.csproj
- [X] T009 Add project references: Application → Domain, Infrastructure → Application, Api → Application + Infrastructure
- [X] T010 [P] Create test project structure: `tests/TaskManager.Attachments.Tests/` with subdirs Unit/, Integration/, Fakes/
- [X] T011 Add NuGet packages to Tests: xUnit 2.x, xUnit.runner.visualstudio, Microsoft.AspNetCore.Mvc.Testing via Tests.csproj
- [X] T012 Create uploads directory structure: `uploads/` at repository root with .gitkeep
- [X] T013 Configure appsettings section for attachments in `src/TaskManager.Api/appsettings.Development.json` (FileStorage.BasePath, FileStorage.MaxFileSizeBytes)
- [X] T014 [P] Configure appsettings section for attachments in `src/TaskManager.Api/appsettings.json` (production settings)

---

## Phase 2: Foundational (Blocking Prerequisites)

**Purpose**: Core infrastructure that MUST be complete before ANY user story can be implemented

**⚠️ CRITICAL**: No user story work can begin until this phase is complete

- [X] T015 Create Attachment entity in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Entities/Attachment.cs` (Id, TaskId, FileName, FileSize, ContentType, UploadedAt, UploadedByUserId, StoragePath)
- [X] T016 [P] Create AllowedFileType enum in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Enums/AllowedFileType.cs` (Jpeg, Png, Gif, Pdf, Doc, Docx, Xls, Xlsx)
- [X] T017 [P] Create MimeTypeMapping helper in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Helpers/MimeTypeMapping.cs` (ToMimeType dictionary, FileExtensions dictionary)
- [X] T018 Create IAttachmentRepository interface in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Interfaces/IAttachmentRepository.cs` (AddAsync, GetByIdAsync, GetByTaskIdAsync, DeleteAsync, CountByTaskIdAsync)
- [X] T019 [P] Create IFileStorageService interface in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Interfaces/IFileStorageService.cs` (SaveFileAsync, GetFileAsync, DeleteFileAsync, FileExistsAsync)
- [X] T020 [P] Create ITaskReadOnlyRepository interface in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Interfaces/ITaskReadOnlyRepository.cs` (GetTaskByIdAsync, IsTaskActiveAsync)
- [X] T021 Create AttachmentsDbContext in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Data/AttachmentsDbContext.cs` (DbSet<Attachment>, OnModelCreating)
- [X] T022 Create AttachmentConfiguration in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Data/Configurations/AttachmentConfiguration.cs` (fluent API: PK, indexes on TaskId, UploadedByUserId, UploadedAt, unique index on StoragePath, check constraints)
- [X] T023 Create AttachmentRepository in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Data/Repositories/AttachmentRepository.cs` implementing IAttachmentRepository (AddAsync with SaveChangesAsync, GetByIdAsync, GetByTaskIdAsync ordered by UploadedAt DESC, DeleteAsync, CountByTaskIdAsync)
- [X] T024 [P] Create LocalFileStorageService in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Storage/LocalFileStorageService.cs` implementing IFileStorageService (SaveFileAsync using File.WriteAllBytesAsync with directory creation, GetFileAsync using File.ReadAllBytesAsync, DeleteFileAsync using File.Delete, FileExistsAsync using File.Exists)
- [X] T025 [P] Create TaskReadOnlyRepository in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Data/Repositories/TaskReadOnlyRepository.cs` implementing ITaskReadOnlyRepository (read-only access to legacy ApplicationDbContext.Tasks, GetTaskByIdAsync, IsTaskActiveAsync)
- [X] T026 Create ServiceCollectionExtensions in `src/TaskManager.Attachments/TaskManager.Attachments.Api/Extensions/ServiceCollectionExtensions.cs` (AddAttachmentsModule method registering DbContext, repositories, services with DI)
- [X] T027 Register AttachmentsModule in `src/TaskManager.Api/Program.cs` calling builder.Services.AddAttachmentsModule(configuration)
- [X] T028 Create initial EF migration: run `dotnet ef migrations add InitialAttachmentsSchema --project src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure --startup-project src/TaskManager.Api`
- [X] T029 Verify migration SQL in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Migrations/[timestamp]_InitialAttachmentsSchema.cs` (check constraints, indexes, default values)

**Checkpoint**: Foundation ready - user story implementation can now begin in parallel

---

## Phase 3: User Story 1 - Adjuntar archivo a tarea (Priority: P1) 🎯 MVP

**Goal**: Usuarios autenticados pueden subir archivos (imágenes, PDFs, documentos Office) a tareas activas, validando tipo MIME, tamaño ≤10MB y límite de 5 adjuntos por tarea.

**Independent Test**: (1) Autenticar usuario con JWT, (2) crear tarea activa con Id conocido, (3) POST multipart/form-data a /api/v1/attachments con taskId + file (screenshot.jpg 2MB), (4) verificar respuesta 201 con metadata completa (id, fileName, fileSize, contentType, uploadedAt, uploadedByUserId).

### Implementation for User Story 1

- [X] T030 [P] [US1] Create UploadAttachmentRequest DTO in `src/TaskManager.Attachments/TaskManager.Attachments.Application/DTOs/UploadAttachmentRequest.cs` (TaskId property Guid, IFormFile property File)
- [X] T031 [P] [US1] Create AttachmentResponse DTO in `src/TaskManager.Attachments/TaskManager.Attachments.Application/DTOs/AttachmentResponse.cs` (Id, TaskId, FileName, FileSize, ContentType, UploadedAt, UploadedByUserId properties)
- [X] T032 [US1] Create UploadAttachmentRequestValidator in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Validators/UploadAttachmentRequestValidator.cs` using FluentValidation (TaskId NotEmpty, File NotNull, File.Length between 1 and 10485760, FileName extension validation, ContentType validation using MimeDetective magic numbers, custom rule to reject invalid MIME types)
- [X] T033 [US1] Create IAttachmentService interface in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Interfaces/IAttachmentService.cs` (UploadAttachmentAsync method signature returning Task<AttachmentResponse>)
- [X] T034 [US1] Create AttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Services/AttachmentService.cs` implementing IAttachmentService (UploadAttachmentAsync with: (1) validate task exists and IsActive via ITaskReadOnlyRepository, (2) check attachment count ≤5 via IAttachmentRepository.CountByTaskIdAsync, (3) generate Guid for attachment Id, (4) generate StoragePath as {taskId}/{attachmentId}.{ext}, (5) save file via IFileStorageService.SaveFileAsync, (6) create Attachment entity with metadata, (7) save to DB via IAttachmentRepository.AddAsync, (8) return AttachmentResponse)
- [X] T035 [US1] Create AttachmentsController in `src/TaskManager.Attachments/TaskManager.Attachments.Api/Controllers/AttachmentsController.cs` inheriting from BaseController legacy (add [Authorize] attribute, constructor injecting IAttachmentService and ILogger)
- [X] T036 [US1] Implement POST /api/v1/attachments endpoint in AttachmentsController.UploadAttachment method (accepts [FromForm] UploadAttachmentRequest, validates model state, extracts userId from JWT claims, calls IAttachmentService.UploadAttachmentAsync, returns CreatedAtAction 201 with AttachmentResponse, handles exceptions: 400 for inactive task or 5 attachment limit, 404 for task not found, 413 for file too large, 415 for invalid MIME type, 500 for storage errors)
- [X] T037 [US1] Add logging statements in AttachmentService.UploadAttachmentAsync (info: upload started with taskId and fileName, success: upload completed with attachmentId, error: upload failed with exception details)
- [X] T038 [US1] Handle error scenarios in AttachmentsController.UploadAttachment (return 400 BadRequest with message "No se pueden adjuntar archivos a tareas inactivas" if task inactive, return 400 BadRequest with message "La tarea ya tiene el máximo de 5 adjuntos permitidos" if limit reached, return 404 NotFound if task not exists, return 413 PayloadTooLarge if file > 10MB, return 415 UnsupportedMediaType with message "Tipo de archivo no permitido. Permitidos: jpg, png, gif, pdf, doc, docx, xls, xlsx" if MIME validation fails)

**Checkpoint**: At this point, User Story 1 should be fully functional and testable independently. Users can upload files to tasks.

---

## Phase 4: User Story 2 - Listar adjuntos de tarea (Priority: P2)

**Goal**: Usuarios autenticados pueden consultar la lista de todos los archivos adjuntos asociados a una tarea específica, obteniendo metadata sin descargar los archivos.

**Independent Test**: (1) Autenticar usuario con JWT, (2) tarea con 3 adjuntos existentes (1 jpg, 1 pdf, 1 docx), (3) GET /api/v1/attachments/task/{taskId}, (4) verificar respuesta 200 con array de 3 AttachmentResponse ordenados por uploadedAt DESC.

### Implementation for User Story 2

- [ ] T039 [P] [US2] Create AttachmentListResponse DTO in `src/TaskManager.Attachments/TaskManager.Attachments.Application/DTOs/AttachmentListResponse.cs` (wrapper class with List<AttachmentResponse> property Attachments)
- [ ] T040 [US2] Add GetAttachmentsByTaskIdAsync method to IAttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Interfaces/IAttachmentService.cs` (signature returning Task<IEnumerable<AttachmentResponse>>)
- [ ] T041 [US2] Implement GetAttachmentsByTaskIdAsync in AttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Services/AttachmentService.cs` ((1) validate task exists via ITaskReadOnlyRepository.GetTaskByIdAsync, (2) query attachments via IAttachmentRepository.GetByTaskIdAsync, (3) map Attachment entities to AttachmentResponse DTOs, (4) return ordered by UploadedAt DESC)
- [ ] T042 [US2] Implement GET /api/v1/attachments/task/{taskId} endpoint in AttachmentsController.GetTaskAttachments method ((1) validate taskId parameter, (2) call IAttachmentService.GetAttachmentsByTaskIdAsync, (3) return 200 OK with array of AttachmentResponse, (4) return empty array [] if no attachments, (5) handle exceptions: 401 for missing JWT, 404 for task not found)
- [ ] T043 [US2] Add logging statements in AttachmentService.GetAttachmentsByTaskIdAsync (info: list requested for taskId, success: returned N attachments)
- [ ] T044 [US2] Add index optimization: verify IX_Attachments_TaskId exists in AttachmentConfiguration in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Data/Configurations/AttachmentConfiguration.cs` (already created in Phase 2, no changes needed - just verification task)

**Checkpoint**: At this point, User Stories 1 AND 2 should both work independently. Users can upload files and list them.

---

## Phase 5: User Story 3 - Descargar archivo adjunto (Priority: P3)

**Goal**: Usuarios autenticados pueden descargar el contenido binario de un archivo adjunto específico, recibiendo el archivo completo con headers HTTP correctos (Content-Type, Content-Disposition).

**Independent Test**: (1) Autenticar usuario con JWT, (2) adjunto existente Id=456 tipo jpg en tarea Id=123, (3) GET /api/v1/attachments/456/download, (4) verificar respuesta 200 con archivo binario, header Content-Type: image/jpeg, header Content-Disposition: attachment; filename=screenshot.jpg, archivo completo sin corrupción.

### Implementation for User Story 3

- [ ] T045 [P] [US3] Add GetAttachmentByIdAsync method to IAttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Interfaces/IAttachmentService.cs` (signature returning Task<AttachmentResponse>)
- [ ] T046 [P] [US3] Add DownloadAttachmentAsync method to IAttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Interfaces/IAttachmentService.cs` (signature returning Task<(byte[] fileContent, string contentType, string fileName)>)
- [ ] T047 [US3] Implement GetAttachmentByIdAsync in AttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Services/AttachmentService.cs` ((1) query attachment via IAttachmentRepository.GetByIdAsync, (2) throw NotFoundException if null, (3) map to AttachmentResponse)
- [ ] T048 [US3] Implement DownloadAttachmentAsync in AttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Services/AttachmentService.cs` ((1) query attachment via IAttachmentRepository.GetByIdAsync, (2) throw NotFoundException if null, (3) read file bytes via IFileStorageService.GetFileAsync with StoragePath, (4) return tuple with fileContent, ContentType, FileName)
- [ ] T049 [US3] Implement GET /api/v1/attachments/{id} endpoint in AttachmentsController.GetAttachment method ((1) validate id parameter Guid, (2) call IAttachmentService.GetAttachmentByIdAsync, (3) return 200 OK with AttachmentResponse, (4) handle exceptions: 401 for missing JWT, 404 for attachment not found)
- [ ] T050 [US3] Implement GET /api/v1/attachments/{id}/download endpoint in AttachmentsController.DownloadAttachment method ((1) validate id parameter Guid, (2) call IAttachmentService.DownloadAttachmentAsync, (3) return File(fileContent, contentType, fileName) with Content-Disposition header as attachment, (4) handle exceptions: 401 for missing JWT, 404 for attachment not found, 500 for file read errors)
- [ ] T051 [US3] Add logging statements in AttachmentService.DownloadAttachmentAsync (info: download started for attachmentId, success: download completed with fileSize bytes, error: download failed with exception)
- [ ] T052 [US3] Handle file corruption detection in LocalFileStorageService.GetFileAsync in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Storage/LocalFileStorageService.cs` (verify file exists before read, throw FileNotFoundException with clear message if missing, log error if IOException occurs)

**Checkpoint**: All user stories 1, 2, and 3 should now be independently functional. Users can upload, list, and download files.

---

## Phase 6: User Story 4 - Eliminar archivo adjunto (Priority: P4)

**Goal**: Usuarios autenticados propietarios de la tarea pueden eliminar adjuntos específicos (por error de upload o información obsoleta). Además, implementar eliminación automática en cascada cuando la tarea se marca como inactiva.

**Independent Test**: (1) Autenticar usuario propietario de tarea con JWT, (2) tarea con 3 adjuntos, (3) DELETE /api/v1/attachments/456, (4) verificar respuesta 204 No Content, (5) verificar archivo eliminado del almacenamiento (File.Exists returns false), (6) verificar listado muestra solo 2 adjuntos restantes.

### Implementation for User Story 4

- [ ] T053 [P] [US4] Add DeleteAttachmentAsync method to IAttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Interfaces/IAttachmentService.cs` (signature returning Task accepting attachmentId Guid and requestingUserId Guid)
- [ ] T054 [US4] Implement DeleteAttachmentAsync in AttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Services/AttachmentService.cs` ((1) query attachment via IAttachmentRepository.GetByIdAsync, (2) throw NotFoundException if null, (3) query task via ITaskReadOnlyRepository.GetTaskByIdAsync to get CreatedByUserId, (4) verify requestingUserId matches task.CreatedByUserId, (5) throw ForbiddenException if not owner, (6) delete file via IFileStorageService.DeleteFileAsync with StoragePath, (7) delete DB record via IAttachmentRepository.DeleteAsync, (8) log deletion event)
- [ ] T055 [US4] Implement DELETE /api/v1/attachments/{id} endpoint in AttachmentsController.DeleteAttachment method ((1) validate id parameter Guid, (2) extract userId from JWT claims, (3) call IAttachmentService.DeleteAttachmentAsync with id and userId, (4) return 204 NoContent on success, (5) handle exceptions: 401 for missing JWT, 403 Forbidden with message "Solo el propietario de la tarea puede eliminar adjuntos" if not owner, 404 for attachment not found)
- [ ] T056 [US4] Add logging statements in AttachmentService.DeleteAttachmentAsync (info: delete requested for attachmentId by userId, success: attachment deleted from storage and DB, warning: delete failed for storage but succeeded in DB (orphan file), error: delete failed with exception)
- [ ] T057 [US4] Create TaskDeletedEvent domain event in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Events/TaskDeletedEvent.cs` (TaskId property Guid, DeletedAt property DateTime)
- [ ] T058 [US4] Create IEventHandler interface in `src/TaskManager.Attachments/TaskManager.Attachments.Domain/Interfaces/IEventHandler.cs` (HandleAsync method with TEvent generic parameter)
- [ ] T059 [US4] Create TaskDeletedEventHandler in `src/TaskManager.Attachments/TaskManager.Attachments.Application/EventHandlers/TaskDeletedEventHandler.cs` implementing IEventHandler<TaskDeletedEvent> ((1) query all attachments via IAttachmentRepository.GetByTaskIdAsync, (2) foreach attachment: delete file via IFileStorageService.DeleteFileAsync, (3) delete DB records via IAttachmentRepository.DeleteAsync, (4) log cascade deletion with count of deleted attachments)
- [ ] T060 [US4] Register TaskDeletedEventHandler in ServiceCollectionExtensions in `src/TaskManager.Attachments/TaskManager.Attachments.Api/Extensions/ServiceCollectionExtensions.cs` (AddScoped<IEventHandler<TaskDeletedEvent>, TaskDeletedEventHandler>)
- [ ] T061 [US4] Create integration point for TaskDeletedEvent: add middleware or background job to detect Task IsActive=false changes and publish TaskDeletedEvent (implementation in `src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure/Jobs/TaskCleanupJob.cs` - scheduled job querying AttachmentsDbContext joined with legacy Tasks where IsActive=false, triggering event handler for each orphan)
- [ ] T062 [US4] Add transactional integrity: wrap DeleteAttachmentAsync in try-catch to rollback DB delete if file delete fails in AttachmentService in `src/TaskManager.Attachments/TaskManager.Attachments.Application/Services/AttachmentService.cs` (use DbContext transaction or compensating action)

**Checkpoint**: All user stories 1, 2, 3, and 4 should now be independently functional. Complete CRUD for attachments.

---

## Phase 7: Polish & Cross-Cutting Concerns

**Purpose**: Improvements that affect multiple user stories

- [ ] T063 [P] Add exception handling middleware integration: verify ExceptionsMiddleware legacy handles custom exceptions (NotFoundException, ForbiddenException) in AttachmentsController (no code changes needed if middleware already handles base exceptions, just verification task)
- [ ] T064 [P] Add CORS configuration: verify CorsConfiguration legacy allows multipart/form-data requests for /api/v1/attachments endpoints (no code changes needed if CORS already configured for all /api/v1/*, just verification task)
- [ ] T065 [P] Add Swagger/OpenAPI documentation: copy contents from `specs/001-task-attachments/contracts/openapi.yaml` into Swagger configuration in `src/TaskManager.Api/Configuration/SwaggerConfiguration.cs` (add AttachmentsController endpoints to Swagger UI with request/response examples)
- [ ] T066 Code cleanup: remove unused using statements, apply .editorconfig formatting rules across all TaskManager.Attachments.* projects
- [ ] T067 Performance optimization: add EF Core AsNoTracking() to read-only queries in AttachmentRepository.GetByTaskIdAsync and AttachmentRepository.GetByIdAsync (in download/list scenarios where entities are not modified)
- [ ] T068 Security hardening: add filename sanitization in AttachmentService.UploadAttachmentAsync to prevent path traversal attacks (strip ../, ..\, leading slashes from FileName before saving to StoragePath)
- [ ] T069 [P] Create unit tests for UploadAttachmentRequestValidator in `tests/TaskManager.Attachments.Tests/Unit/Validators/UploadAttachmentRequestValidatorTests.cs` (test cases: valid file jpg 2MB passes, file >10MB fails, null file fails, empty taskId fails, invalid MIME type fails)
- [ ] T070 [P] Create unit tests for AttachmentService in `tests/TaskManager.Attachments.Tests/Unit/Services/AttachmentServiceTests.cs` (test cases: UploadAttachmentAsync success, UploadAttachmentAsync task not found throws NotFoundException, UploadAttachmentAsync inactive task throws BadRequestException, UploadAttachmentAsync 5 attachment limit throws BadRequestException, DeleteAttachmentAsync success, DeleteAttachmentAsync not owner throws ForbiddenException)
- [ ] T071 [P] Create integration tests for AttachmentsController in `tests/TaskManager.Attachments.Tests/Integration/AttachmentsControllerTests.cs` using WebApplicationFactory (test cases: POST /api/v1/attachments with valid file returns 201, POST without JWT returns 401, GET /api/v1/attachments/task/{taskId} returns 200 with array, DELETE /api/v1/attachments/{id} by owner returns 204, DELETE by non-owner returns 403)
- [ ] T072 [P] Create FakeFileStorageService in `tests/TaskManager.Attachments.Tests/Fakes/FakeFileStorageService.cs` implementing IFileStorageService (in-memory dictionary to simulate file storage without actual I/O for faster unit tests)
- [ ] T073 [P] Create TestDataSeeder fixture in `tests/TaskManager.Attachments.Tests/Integration/Fixtures/TestDataSeeder.cs` (helper methods to create test tasks, users, and attachments for integration tests)
- [ ] T074 Run quickstart.md validation: execute all commands in `specs/001-task-attachments/quickstart.md` (dotnet build, dotnet test, dotnet run, cURL upload/list/download/delete examples) to verify documentation accuracy
- [ ] T075 Update README.md at repository root to mention new attachments module in features section
- [ ] T076 Generate code coverage report: run `dotnet test --collect:"XPlat Code Coverage"` and verify ≥80% coverage for AttachmentService, AttachmentRepository, UploadAttachmentRequestValidator

---

## Dependencies & Execution Order

### Phase Dependencies

- **Setup (Phase 1)**: No dependencies - can start immediately
- **Foundational (Phase 2)**: Depends on Setup completion - BLOCKS all user stories
- **User Story 1 (Phase 3)**: Depends on Foundational (Phase 2) - Independent of US2/US3/US4
- **User Story 2 (Phase 4)**: Depends on Foundational (Phase 2) - Independent of US1/US3/US4 (but may want US1 for realistic testing)
- **User Story 3 (Phase 5)**: Depends on Foundational (Phase 2) - Independent of US1/US2/US4 (but requires US1 for data to download)
- **User Story 4 (Phase 6)**: Depends on Foundational (Phase 2) - Independent of US1/US2/US3 (but requires US1 for data to delete)
- **Polish (Phase 7)**: Depends on all desired user stories being complete - Can start when US1-US4 implemented

### User Story Dependencies

- **User Story 1 (P1)**: Can start after Foundational (Phase 2) - NO dependencies on other stories - Core MVP functionality
- **User Story 2 (P2)**: Can start after Foundational (Phase 2) - Complementary to US1 (list uploaded files) - Independently testable with pre-seeded data
- **User Story 3 (P3)**: Can start after Foundational (Phase 2) - Extends US1/US2 (download listed files) - Independently testable with pre-seeded data
- **User Story 4 (P4)**: Can start after Foundational (Phase 2) - Completes CRUD operations - Independently testable with pre-seeded data

**Recommended MVP Scope**: User Story 1 (Upload) + User Story 2 (List) provides core value. US3 (Download) and US4 (Delete) can be deferred if timeline is tight.

### Within Each User Story

**User Story 1 (Upload)**:
- T030, T031 (DTOs) in parallel → T032 (Validator) depends on both
- T033 (IAttachmentService interface) independent → T034 (AttachmentService) depends on T033 + T032
- T035 (Controller) in parallel with T034 → T036 (POST endpoint) depends on T034 + T035
- T037, T038 (logging, error handling) depends on T036

**User Story 2 (List)**:
- T039 (DTO) independent → T040 (interface) in parallel with T039
- T041 (service method) depends on T040 → T042 (GET endpoint) depends on T041
- T043, T044 (logging, verification) depends on T042

**User Story 3 (Download)**:
- T045, T046 (interface methods) in parallel → T047, T048 (service methods) depend on T045, T046
- T049, T050 (GET endpoints) depend on T047, T048 → T051, T052 (logging, error handling) depends on T050

**User Story 4 (Delete)**:
- T053 (interface) independent → T054 (service method) depends on T053
- T055 (DELETE endpoint) depends on T054 → T056 (logging) depends on T055
- T057, T058 (domain events) in parallel → T059 (event handler) depends on T057, T058
- T060, T061, T062 (registration, integration, transaction) can proceed in parallel after T059

**Polish Phase**:
- T063, T064, T065 (middleware verification, CORS, Swagger) can run in parallel (read-only verification tasks)
- T066, T067, T068 (cleanup, performance, security) can run in parallel (independent code improvements)
- T069, T070, T071, T072, T073 (all test tasks) can run in parallel (different test files)
- T074, T075, T076 (validation, docs, coverage) run sequentially after tests complete

### Parallel Opportunities

- **Phase 1 (Setup)**: T002, T003, T004 (layer structures) + T006, T007, T008 (NuGet packages) + T010, T011 (tests) + T014 (appsettings.json) = 9 parallel tasks
- **Phase 2 (Foundational)**: T016, T017 (enum, helpers) + T019, T020 (storage/task interfaces) + T024, T025 (storage/task repositories) = 6 parallel tasks
- **Phase 3 (US1)**: T030, T031 (DTOs) = 2 parallel tasks
- **Phase 4 (US2)**: T039 (DTO) + T040 (interface) = 2 parallel tasks (but faster to do sequentially due to simplicity)
- **Phase 5 (US3)**: T045, T046 (interface methods) = 2 parallel tasks
- **Phase 6 (US4)**: T053 (interface) + T057, T058 (events) early on = 3 parallel tasks
- **Phase 7 (Polish)**: T063, T064, T065 (verifications) + T069, T070, T071, T072, T073 (all tests) = 8 parallel tasks

**Maximum Parallelization**: With 3 developers, Phase 1 can complete in ~2 hours (9 tasks / 3 devs = 3 tasks each), Phase 2 in ~4 hours (29 tasks with some sequential dependencies), then each User Story in parallel: Dev1=US1 (9 tasks, ~6 hours), Dev2=US2 (6 tasks, ~3 hours), Dev3=US3 (8 tasks, ~5 hours). US4 after US1-US3 complete (~6 hours). Polish phase with all 3 devs (~4 hours for tests + validation).

**Total Timeline Estimate**: 
- **Single developer sequential**: ~10-12 days (76 tasks * 2-3 hours/task = 152-228 hours / 20 hours/week = 7.6-11.4 weeks)
- **Three developers parallel**: ~4-5 days (same work parallelized across phases and user stories)
- **MVP (US1+US2 only)**: ~5-6 days single dev, ~2-3 days with 2 devs

---

## Parallel Example: User Story 1 (Upload)

**Scenario**: Two developers working on US1 simultaneously after Phase 2 completes

```bash
# Developer 1: DTOs and Validator
git checkout -b feature/001-us1-dtos
# T030: Create UploadAttachmentRequest.cs
# T031: Create AttachmentResponse.cs  
# T032: Create UploadAttachmentRequestValidator.cs
git commit -m "US1: Add DTOs and validator"
git push

# Developer 2: Service Layer
git checkout -b feature/001-us1-service
# Wait for T030, T031, T032 to be merged (or work with stubs)
# T033: Create IAttachmentService.cs
# T034: Implement AttachmentService.cs
git commit -m "US1: Add attachment service"
git push

# Developer 1: Controller (after service ready)
git checkout -b feature/001-us1-controller
git pull origin main  # Get service code
# T035: Create AttachmentsController.cs
# T036: Implement POST endpoint
# T037: Add logging
# T038: Add error handling
git commit -m "US1: Add upload endpoint"
git push
```

**Merge Order**: T030+T031+T032 → T033+T034 → T035+T036+T037+T038

---

## Validation Checklist

Before marking feature complete, verify:

- [ ] All 76 tasks completed and checked off
- [ ] All 4 user stories (P1-P4) independently functional and tested
- [ ] Code coverage ≥80% for AttachmentService, AttachmentRepository, Validators (T076 verification)
- [ ] All quickstart.md commands execute successfully (T074 validation)
- [ ] No modifications to legacy folders: `src/TaskManager.Api/Controllers/`, `src/TaskManager.Application/`, `src/TaskManager.Domain/Entities/`, `src/TaskManager.Infrastructure/Data/`, `src/TaskManager.Tests/` (git diff verification)
- [ ] Constitution principles I-V remain satisfied (Strangler Pattern, Clean Architecture, SDD, Security, Quality)
- [ ] OpenAPI documentation visible in Swagger UI at `/swagger` (T065)
- [ ] Performance goals met: upload <5s for 10MB, download <10s, list <2s for 100 attachments
- [ ] Security validated: JWT required, MIME magic numbers checked, path traversal prevented, authorization enforced for delete

---

## Implementation Strategy

**MVP First Approach**: Prioritize User Stories by value delivery

1. **Sprint 1 (Week 1)**: Phase 1 (Setup) + Phase 2 (Foundational) → Establishes infrastructure
2. **Sprint 2 (Week 2)**: Phase 3 (US1 - Upload) → Core MVP: users can attach files
3. **Sprint 3 (Week 2-3)**: Phase 4 (US2 - List) → Complementary: users can see attached files
4. **Sprint 4 (Week 3)**: Phase 5 (US3 - Download) → Value extension: users can retrieve files
5. **Sprint 5 (Week 4)**: Phase 6 (US4 - Delete) → CRUD completion: users can manage lifecycle
6. **Sprint 6 (Week 4-5)**: Phase 7 (Polish) → Quality: tests, documentation, hardening

**Alternative Minimum MVP**: If timeline is critical, deliver ONLY US1 (Upload) + US2 (List) in 2 weeks (Phase 1 + 2 + 3 + 4), defer US3/US4 to next iteration. This provides core value: users can upload and see files, download can be added later without breaking changes.

**Incremental Deployment**: Each User Story phase ends with a "Checkpoint" indicating independent functionality. Deploy to staging after each checkpoint to gather early feedback before proceeding to next priority.

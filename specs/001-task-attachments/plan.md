# Implementation Plan: Módulo de Archivos Adjuntos para Tareas

**Branch**: `001-task-attachments` | **Date**: 2026-01-29 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/001-task-attachments/spec.md`

## Summary

Implementar módulo de gestión de archivos adjuntos para tareas del sistema TaskManager, permitiendo a usuarios autenticados subir, listar, descargar y eliminar archivos (imágenes, PDFs, documentos Office) asociados a tareas activas. El módulo se integra usando Strangler Pattern sin modificar entidades legacy existentes, respetando Clean Architecture y utilizando autenticación JWT existente. Almacenamiento inicial en disco local con diseño preparado para migración futura a blob storage.

## Technical Context

**Language/Version**: .NET 8 (C# 12)
**Primary Dependencies**: 
- ASP.NET Core Web API 8.0
- Entity Framework Core 8.0 (InMemory para desarrollo, SQL Server para producción)
- FluentValidation 11.x para validación de DTOs
- JWT authentication (infraestructura legacy existente)
- xUnit 2.x para pruebas unitarias e integración

**Storage**: 
- Metadata de adjuntos: Entity Framework Core (nueva entidad `Attachment` en DbContext sin modificar entidades legacy)
- Archivos binarios: Sistema de archivos local (carpeta `uploads/` en ambiente desarrollo, ruta configurable para producción)
- Diseño preparado para migración futura a Azure Blob Storage o AWS S3

**Testing**: 
- xUnit para pruebas unitarias (servicios, validadores, repositorios)
- xUnit + WebApplicationFactory para pruebas de integración (endpoints completos)
- Fixtures para datos de prueba (tareas, usuarios, archivos simulados)

**Target Platform**: 
- Servidor Linux/Windows con .NET 8 Runtime
- Kestrel web server (configuración default permite requests hasta 28.6MB, suficiente para límite de 10MB)

**Project Type**: Web API backend (monolito modular con separación por capas Clean Architecture)

**Performance Goals**: 
- Upload de archivos ≤10MB: completar en <5 segundos (incluyendo validación y persistencia)
- Download de archivos ≤10MB: transmitir en <10 segundos en conexión 10 Mbps
- Listado de adjuntos: <2 segundos para tareas con hasta 100 adjuntos
- Soportar 100 uploads concurrentes sin pérdida de datos ni duplicación de IDs

**Constraints**: 
- **NO MODIFICAR** código legacy en `src/TaskManager.Api/`, `src/TaskManager.Application/`, `src/TaskManager.Domain/`, `src/TaskManager.Infrastructure/`, `src/TaskManager.Tests/`
- Máximo 5 adjuntos por tarea (validación en capa Application)
- Tamaño máximo 10 MB por archivo (10,485,760 bytes)
- Tipos MIME permitidos: image/jpeg, image/png, image/gif, application/pdf, application/msword, application/vnd.openxmlformats-officedocument.wordprocessingml.document, application/vnd.ms-excel, application/vnd.openxmlformats-officedocument.spreadsheetml.sheet
- Cobertura de pruebas ≥80% en servicios y repositorios nuevos

**Scale/Scope**: 
- MVP para equipos pequeños/medianos (<100 usuarios concurrentes)
- Estimado 1000 tareas activas promedio, ~3000 adjuntos totales
- Crecimiento esperado: ~500 uploads/día, ~50GB almacenamiento mensual
- Sin límites globales de cuota por usuario/proyecto en fase inicial

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

### ✅ Principle I: Strangler Pattern Integration (NON-NEGOTIABLE)

- [x] **Nueva funcionalidad NO modifica código legacy**: Módulo reside fuera de carpetas `src/TaskManager.*`
- [x] **Integración mediante puntos de extensión**: Nuevo controlador `AttachmentsController` hereda de `BaseController` legacy sin modificar `BaseController`
- [x] **Estrategia de integración documentada**: Spec.md sección Dependencies documenta acceso read-only a entidad `Task` legacy

**Status**: ✅ PASS - Módulo se implementará en nueva estructura fuera de legacy

---

### ✅ Principle II: Clean Architecture Boundaries

- [x] **Separación de capas respetada**: Módulo sigue estructura Domain → Application → Infrastructure ← Api
- [x] **Patrones establecidos**: Implementará `IAttachmentRepository` → `AttachmentRepository`, `IAttachmentService` → `AttachmentService`
- [x] **Validación con FluentValidation**: DTOs `UploadAttachmentRequest` tendrán validadores `UploadAttachmentRequestValidator`
- [x] **Domain libre de dependencias**: Entidad `Attachment` no dependerá de EF Core ni ASP.NET (solo POCOs)

**Status**: ✅ PASS - Clean Architecture respetada en diseño

---

### ✅ Principle III: Spec-Driven Development (SDD)

- [x] **User stories priorizadas**: P1-P4 documentadas en spec.md (Upload, List, Download, Delete)
- [x] **Acceptance scenarios Given/When/Then**: 15 scenarios definidos across 4 user stories
- [x] **Edge cases identificados**: 10 edge cases documentados en spec.md
- [x] **Plan antes de tareas**: Este plan.md generado antes de tasks.md

**Status**: ✅ PASS - SDD workflow seguido correctamente

---

### ✅ Principle IV: Security & Authentication

- [x] **JWT validation obligatoria**: Todos los endpoints usarán `[Authorize]` attribute y BaseController legacy
- [x] **Claims verification**: Service layer verificará `userId` claim para autorización de eliminación
- [x] **Validación MIME y tamaño**: Validadores FluentValidation + lógica en service layer
- [x] **No secretos en código**: Rutas de almacenamiento en appsettings.json, no hardcoded

**Status**: ✅ PASS - Seguridad integrada desde diseño

---

### ✅ Principle V: Quality & Testing

- [x] **Pruebas unitarias xUnit**: Planificadas para servicios, repositorios, validadores
- [x] **Pruebas de integración**: Planificadas para endpoints completos (WebApplicationFactory)
- [x] **Cobertura ≥80%**: Meta establecida en spec.md (SC-010)
- [x] **Manejo de errores**: Reutilizará ExceptionsMiddleware y LoggingMiddleware legacy

**Status**: ✅ PASS - Calidad asegurada por plan de testing

---

**Overall Gate Status**: ✅ ALL GATES PASS - Proceed to Phase 0 (Research)

## Project Structure

### Documentation (this feature)

```text
specs/001-task-attachments/
├── plan.md              # This file (/speckit.plan command output)
├── research.md          # Phase 0 output (/speckit.plan command)
├── data-model.md        # Phase 1 output (/speckit.plan command)
├── quickstart.md        # Phase 1 output (/speckit.plan command)
├── contracts/           # Phase 1 output (/speckit.plan command)
│   ├── openapi.yaml     # OpenAPI 3.0 spec for attachments endpoints
│   ├── dtos.md          # Request/Response DTOs documentation
│   └── endpoints.md     # Endpoint details (routes, methods, auth)
├── checklists/
│   └── requirements.md  # Specification quality checklist (already created)
└── tasks.md             # Phase 2 output (/speckit.tasks command - NOT created by /speckit.plan)
```

### Source Code (repository root)

**Strangler Pattern Structure**: Nuevo módulo fuera de carpetas legacy

```text
src/
├── TaskManager.Attachments/              # NUEVO MÓDULO (Strangler Pattern)
│   ├── TaskManager.Attachments.Domain/
│   │   ├── Entities/
│   │   │   └── Attachment.cs            # Nueva entidad (NO modifica Task legacy)
│   │   ├── Enums/
│   │   │   └── AllowedFileType.cs       # Enum para tipos MIME permitidos
│   │   └── Interfaces/
│   │       ├── IAttachmentRepository.cs
│   │       └── IFileStorageService.cs   # Abstracción para disco/blob storage
│   │
│   ├── TaskManager.Attachments.Application/
│   │   ├── DTOs/
│   │   │   ├── UploadAttachmentRequest.cs
│   │   │   ├── AttachmentResponse.cs
│   │   │   └── AttachmentListResponse.cs
│   │   ├── Services/
│   │   │   ├── AttachmentService.cs     # Lógica de negocio
│   │   │   └── IAttachmentService.cs
│   │   └── Validators/
│   │       └── UploadAttachmentRequestValidator.cs  # FluentValidation
│   │
│   ├── TaskManager.Attachments.Infrastructure/
│   │   ├── Data/
│   │   │   ├── AttachmentsDbContext.cs  # DbContext SEPARADO (no modifica ApplicationDbContext legacy)
│   │   │   ├── Configurations/
│   │   │   │   └── AttachmentConfiguration.cs  # EF Core fluent config
│   │   │   └── Repositories/
│   │   │       └── AttachmentRepository.cs
│   │   └── Storage/
│   │       ├── LocalFileStorageService.cs      # Implementación disco local
│   │       └── AzureBlobStorageService.cs      # Implementación futura (placeholder)
│   │
│   └── TaskManager.Attachments.Api/
│       ├── Controllers/
│       │   └── AttachmentsController.cs  # Hereda de BaseController legacy (sin modificarlo)
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs  # DI registration
│       └── Middleware/
│           └── FileSizeValidationMiddleware.cs # Validación temprana de tamaño
│
├── TaskManager.Api/                      # LEGACY - NO MODIFICAR
├── TaskManager.Application/              # LEGACY - NO MODIFICAR  
├── TaskManager.Domain/                   # LEGACY - NO MODIFICAR
├── TaskManager.Infrastructure/           # LEGACY - NO MODIFICAR
└── TaskManager.Tests/                    # LEGACY - NO MODIFICAR

tests/
└── TaskManager.Attachments.Tests/        # NUEVO - Pruebas del módulo
    ├── Unit/
    │   ├── Services/
    │   │   └── AttachmentServiceTests.cs
    │   ├── Validators/
    │   │   └── UploadAttachmentRequestValidatorTests.cs
    │   └── Repositories/
    │       └── AttachmentRepositoryTests.cs
    ├── Integration/
    │   ├── AttachmentsControllerTests.cs
    │   └── Fixtures/
    │       ├── AttachmentsWebApplicationFactory.cs
    │       └── TestDataSeeder.cs
    └── Fakes/
        └── FakeFileStorageService.cs     # Mock para pruebas sin I/O real

uploads/                                  # NUEVO - Almacenamiento local archivos
└── [task-id]/                            # Subdirectorios por tarea
    └── [attachment-id].[ext]             # Archivos nombrados por GUID
```

**Structure Decision**: 
- **Strangler Pattern aplicado**: Módulo completamente separado en `src/TaskManager.Attachments/` con sus propias capas Clean Architecture
- **Integración sin modificación**: `AttachmentsController` hereda de `BaseController` legacy, reutiliza JWT middleware, pero NO modifica código legacy
- **DbContext separado**: `AttachmentsDbContext` independiente para evitar modificar `ApplicationDbContext` legacy (migración futura a DbContext unificado opcional)
- **Storage abstraction**: `IFileStorageService` permite cambiar de disco local a blob storage sin modificar lógica de negocio

## Complexity Tracking

> **No violations detected - section not applicable**

**All Constitution principles satisfied without exceptions**

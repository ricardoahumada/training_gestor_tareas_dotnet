<!--
SYNC IMPACT REPORT
==================
Version Change: 0.0.0 → 1.0.0
Rationale: Initial constitution establishment for TaskManager project with SDD + Strangler Pattern integration strategy

Modified Principles:
- NEW: I. Strangler Pattern Integration
- NEW: II. Clean Architecture Boundaries
- NEW: III. Spec-Driven Development (SDD)
- NEW: IV. Security & Authentication
- NEW: V. Quality & Testing

Added Sections:
- Technology Stack & Constraints
- Development Workflow

Templates Requiring Updates:
✅ spec-template.md - Already aligned with user scenarios and requirements structure
✅ plan-template.md - Constitution Check section references this file
✅ tasks-template.md - Should align with principle-driven task categorization (review pending)
⚠ commands/*.md - Verify no hardcoded agent names (CLAUDE) remain

Follow-up TODOs:
- RATIFICATION_DATE set to 2026-01-29 (today) as initial adoption
- Review tasks-template.md for task type alignment with new principles
- Validate all command templates use generic guidance language
-->

# TaskManager Constitution

## Core Principles

### I. Strangler Pattern Integration (NON-NEGOTIABLE)

**Nueva funcionalidad NUNCA modifica código legacy**. Todo módulo nuevo (p. ej., Adjuntos, Notificaciones) DEBE:
- Residir fuera de las carpetas legacy existentes en `src/` (TaskManager.Api, TaskManager.Application, TaskManager.Domain, TaskManager.Infrastructure, TaskManager.Tests)
- Integrarse mediante puntos de extensión (nuevos controladores, middleware, servicios) sin alterar implementaciones existentes
- Documentar la estrategia de integración en la especificación del módulo

**Rationale**: Preservar estabilidad del sistema legacy mientras se agrega valor incremental. Reducir riesgo de regresiones y facilitar despliegue independiente de nuevas capacidades.

### II. Clean Architecture Boundaries

**Separación estricta de capas** (`Domain`, `Application`, `Infrastructure`, `Api`). Todo código DEBE:
- Respetar dependencias unidireccionales: Api → Application → Domain ← Infrastructure
- Implementar patrones establecidos: Repository (`I*Repository` → `*Repository`), Service Layer (`I*Service` → `*Service`), DTOs para requests/responses
- Validar inputs con FluentValidation antes de procesamiento en capa Application
- Mantener Domain libre de dependencias externas (Entity Framework, JWT, etc.)

**Rationale**: Garantizar mantenibilidad, testabilidad y evolución independiente de cada capa. Facilitar cambio de proveedores (InMemory → SQL Server, disco → blob storage) sin impacto en lógica de negocio.

### III. Spec-Driven Development (SDD)

**Especificación antes de implementación**. Todo feature DEBE:
1. Documentar user stories priorizadas e independientemente testables (P1, P2, P3...)
2. Definir acceptance scenarios en formato Given/When/Then
3. Identificar edge cases y requisitos funcionales/no funcionales
4. Obtener aprobación del Product Owner antes de Phase 1 (diseño técnico)
5. Generar plan de implementación, research, data model, contracts y quickstart antes de tareas

**Rationale**: Alinear expectativas con stakeholders, reducir retrabajo, y garantizar que cada entregable tiene criterios de aceptación verificables desde el inicio.

### IV. Security & Authentication

**Autenticación y autorización obligatorias**. Todo endpoint DEBE:
- Validar JWT token existente antes de acceso a recursos protegidos
- Verificar claims de usuario (userId, role) para autorización granular
- Validar tipos MIME y tamaño de archivos (cuando aplique: max 10 MB, tipos permitidos según requisitos)
- Nunca almacenar secretos en repositorio o código fuente
- Usar configuración externa (appsettings, variables de entorno) para claves y conexiones

**Rationale**: Cumplir estándares de seguridad de la industria, proteger datos de usuarios y tareas, y prevenir vulnerabilidades comunes (injection, unauthorized access, file upload exploits).

### V. Quality & Testing

**Pruebas antes de merge**. Todo código DEBE:
- Incluir pruebas unitarias (xUnit) para lógica de servicios y validadores
- Incluir pruebas de integración cuando se modifiquen contratos entre capas
- Mantener cobertura mínima del 80% en servicios y repositorios nuevos
- Ejecutar `dotnet test` sin errores antes de commit
- Manejar errores con middlewares existentes (ExceptionsMiddleware, LoggingMiddleware)

**Rationale**: Detectar regresiones temprano, documentar comportamiento esperado, y mantener calidad constante a medida que el sistema crece.

## Technology Stack & Constraints

**Stack Base**:
- .NET 8 Web API
- Entity Framework Core (InMemory para desarrollo/pruebas, SQL Server para producción)
- xUnit para testing
- FluentValidation para validación de DTOs
- JWT para autenticación

**Convenciones de Naming**:
- Entidades: PascalCase singular (Task, User, Project, Attachment)
- Interfaces: I*Repository, I*Service
- DTOs: *Request, *Response, *Dto
- Controladores: *Controller heredando de BaseController

**Límites Operativos** (cuando aplique a módulos de archivos):
- Tamaño máximo por archivo: 10 MB
- Tipos MIME permitidos: según requisitos funcionales (p. ej., imágenes, PDFs, Office docs)
- Cuotas por recurso: definir en especificación (p. ej., max 5 adjuntos por tarea)

**Preguntar antes de implementar**:
- Almacenamiento de archivos: disco local vs. blob storage (Azure/AWS)
- Políticas de retención, cifrado en reposo/tránsito, escaneo antivirus
- Límites de concurrencia, tiempos de respuesta esperados (SLA)
- Estrategia de backup/disaster recovery

## Development Workflow

**Phase 0 - Especificación** (requiere `/speckit.spec`):
1. Documentar user stories priorizadas
2. Definir acceptance scenarios
3. Identificar edge cases y requisitos funcionales/no funcionales
4. Aprobar con Product Owner

**Phase 1 - Planificación** (requiere `/speckit.plan`):
1. Generar plan de implementación con contexto técnico
2. Documentar research (patrones, integraciones, riesgos)
3. Definir data model (nuevas entidades, sin modificar legacy)
4. Especificar contracts (DTOs, interfaces, endpoints)
5. Crear quickstart (comandos de build, test, run)

**Phase 2 - Tareas** (requiere `/speckit.tasks`):
1. Descomponer plan en tareas granulares (2-4 horas cada una)
2. Agrupar por tipo: infra, core, integration, testing, docs
3. Asignar prioridades y dependencias

**Phase 3 - Implementación**:
1. Crear feature branch `###-feature-name`
2. Implementar según tareas, respetando principios I-V
3. Ejecutar `dotnet restore && dotnet build && dotnet test` localmente
4. Commit y push incremental (no esperar a completar todo el módulo)

**Phase 4 - Validación**:
1. Revisar compliance con Constitution (checklist en plan.md)
2. Verificar pruebas (cobertura, integración, edge cases)
3. Validar integración sin modificar legacy
4. Merge a main tras aprobación

## Governance

**Esta Constitution tiene precedencia sobre prácticas previas no documentadas**. Todo cambio a estos principios DEBE:
- Documentarse con justificación técnica y de negocio
- Incrementar versión según semantic versioning:
  - MAJOR: cambio incompatible hacia atrás (remover/redefinir principio)
  - MINOR: agregar nuevo principio o sección
  - PATCH: clarificación, typo, refinamiento no semántico
- Incluir plan de migración si afecta código existente
- Actualizarse en templates relacionados (spec, plan, tasks)

**Compliance Review**:
- Todo PR debe verificar adherencia a principios I-V en checklist de plan.md
- Complejidad técnica (nueva dependencia, cambio arquitectónico) requiere justificación explícita
- Usar `AGENTS.md` para guía runtime de desarrollo y límites operativos

**Version**: 1.0.0 | **Ratified**: 2026-01-29 | **Last Amended**: 2026-01-29

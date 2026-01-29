# Quickstart: Módulo de Archivos Adjuntos para Tareas

**Feature**: Módulo de Archivos Adjuntos
**Date**: 2026-01-29
**Purpose**: Comandos rápidos para build, test y ejecución

## Prerequisites

- .NET 8 SDK instalado
- Visual Studio Code o Visual Studio 2022
- Git configurado

## Project Structure

```
src/TaskManager.Attachments/
├── TaskManager.Attachments.Domain/
├── TaskManager.Attachments.Application/
├── TaskManager.Attachments.Infrastructure/
└── TaskManager.Attachments.Api/

tests/TaskManager.Attachments.Tests/
```

## Build Commands

### Restore Dependencies

```powershell
# Restaurar paquetes NuGet para todo el módulo
dotnet restore src/TaskManager.Attachments/TaskManager.Attachments.sln
```

### Build Solution

```powershell
# Compilar todo el módulo Attachments
dotnet build src/TaskManager.Attachments/TaskManager.Attachments.sln

# Compilar proyecto específico
dotnet build src/TaskManager.Attachments/TaskManager.Attachments.Api/TaskManager.Attachments.Api.csproj

# Compilar en modo Release
dotnet build src/TaskManager.Attachments/TaskManager.Attachments.sln --configuration Release
```

### Clean Build

```powershell
# Limpiar y reconstruir
dotnet clean src/TaskManager.Attachments/TaskManager.Attachments.sln
dotnet build src/TaskManager.Attachments/TaskManager.Attachments.sln
```

## Test Commands

### Run All Tests

```powershell
# Ejecutar todas las pruebas del módulo
dotnet test tests/TaskManager.Attachments.Tests/TaskManager.Attachments.Tests.csproj

# Con verbose output
dotnet test tests/TaskManager.Attachments.Tests/TaskManager.Attachments.Tests.csproj --verbosity detailed
```

### Run Specific Test Categories

```powershell
# Solo pruebas unitarias
dotnet test tests/TaskManager.Attachments.Tests/TaskManager.Attachments.Tests.csproj --filter "Category=Unit"

# Solo pruebas de integración
dotnet test tests/TaskManager.Attachments.Tests/TaskManager.Attachments.Tests.csproj --filter "Category=Integration"

# Pruebas de un servicio específico
dotnet test tests/TaskManager.Attachments.Tests/TaskManager.Attachments.Tests.csproj --filter "FullyQualifiedName~AttachmentServiceTests"
```

### Code Coverage

```powershell
# Generar reporte de cobertura (requiere coverlet.collector)
dotnet test tests/TaskManager.Attachments.Tests/TaskManager.Attachments.Tests.csproj /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generar reporte HTML (requiere ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport -reporttypes:Html
```

## Run Commands

### Run API (Development Mode)

```powershell
# Desde directorio raíz
dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj

# Con hot reload habilitado
dotnet watch run --project src/TaskManager.Api/TaskManager.Api.csproj

# Con perfil específico
dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj --launch-profile "Development"
```

**Note**: El módulo Attachments se registra en el API principal mediante `ServiceCollectionExtensions`. No tiene API independiente.

### Access API

Una vez ejecutado, el API estará disponible en:

```
http://localhost:5000
```

**Swagger UI** (documentación interactiva):
```
http://localhost:5000/index.html
```

**Endpoints de Attachments**:
```
POST   http://localhost:5000/api/v1/attachments
GET    http://localhost:5000/api/v1/attachments/task/{taskId}
GET    http://localhost:5000/api/v1/attachments/{id}
GET    http://localhost:5000/api/v1/attachments/{id}/download
DELETE http://localhost:5000/api/v1/attachments/{id}
```

## Database Commands

### Run Migrations

```powershell
# Crear migración inicial (primera vez)
dotnet ef migrations add InitialAttachmentsSchema --project src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure --startup-project src/TaskManager.Api

# Aplicar migraciones a base de datos
dotnet ef database update --project src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure --startup-project src/TaskManager.Api

# Revertir última migración
dotnet ef database update <previous-migration-name> --project src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure --startup-project src/TaskManager.Api

# Ver migraciones pendientes
dotnet ef migrations list --project src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure --startup-project src/TaskManager.Api
```

### Drop Database (Development Only)

```powershell
# ⚠️ WARNING: Elimina TODA la base de datos
dotnet ef database drop --project src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure --startup-project src/TaskManager.Api --force
```

## VS Code Tasks

Si usas VS Code, agrega estas tareas en `.vscode/tasks.json`:

```json
{
  "version": "2.0.0",
  "tasks": [
    {
      "label": "Build Attachments Module",
      "type": "shell",
      "command": "dotnet build src/TaskManager.Attachments/TaskManager.Attachments.sln",
      "group": {
        "kind": "build",
        "isDefault": false
      },
      "presentation": {
        "reveal": "always"
      },
      "problemMatcher": "$msCompile"
    },
    {
      "label": "Test Attachments Module",
      "type": "shell",
      "command": "dotnet test tests/TaskManager.Attachments.Tests/TaskManager.Attachments.Tests.csproj",
      "group": {
        "kind": "test",
        "isDefault": false
      },
      "presentation": {
        "reveal": "always"
      },
      "problemMatcher": "$msCompile"
    },
    {
      "label": "Run API with Attachments",
      "type": "shell",
      "command": "dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj",
      "isBackground": true,
      "group": {
        "kind": "none",
        "isDefault": false
      },
      "presentation": {
        "reveal": "always"
      },
      "problemMatcher": "$msCompile"
    }
  ]
}
```

## Quick Test Examples

### Test Upload (cURL)

```bash
# 1. Autenticarse (obtener JWT token)
TOKEN=$(curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"password123"}' \
  | jq -r '.token')

# 2. Upload archivo
curl -X POST http://localhost:5000/api/v1/attachments \
  -H "Authorization: Bearer $TOKEN" \
  -F "taskId=550e8400-e29b-41d4-a716-446655440000" \
  -F "file=@screenshot.jpg"

# 3. Listar adjuntos de tarea
curl -X GET http://localhost:5000/api/v1/attachments/task/550e8400-e29b-41d4-a716-446655440000 \
  -H "Authorization: Bearer $TOKEN"

# 4. Descargar adjunto
curl -X GET http://localhost:5000/api/v1/attachments/{attachment-id}/download \
  -H "Authorization: Bearer $TOKEN" \
  -o downloaded-file.jpg

# 5. Eliminar adjunto
curl -X DELETE http://localhost:5000/api/v1/attachments/{attachment-id} \
  -H "Authorization: Bearer $TOKEN"
```

### Test Upload (PowerShell)

```powershell
# 1. Autenticarse
$loginResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/auth/login" `
  -Method Post `
  -ContentType "application/json" `
  -Body '{"email":"user@example.com","password":"password123"}'
$token = $loginResponse.token

# 2. Upload archivo
$headers = @{ Authorization = "Bearer $token" }
$form = @{
    taskId = "550e8400-e29b-41d4-a716-446655440000"
    file = Get-Item -Path "screenshot.jpg"
}
Invoke-RestMethod -Uri "http://localhost:5000/api/v1/attachments" `
  -Method Post `
  -Headers $headers `
  -Form $form

# 3. Listar adjuntos
$attachments = Invoke-RestMethod -Uri "http://localhost:5000/api/v1/attachments/task/550e8400-e29b-41d4-a716-446655440000" `
  -Headers $headers
$attachments | ConvertTo-Json
```

## Configuration Files

### appsettings.Development.json

```json
{
  "FileStorage": {
    "BasePath": "uploads/",
    "MaxFileSizeBytes": 10485760
  },
  "ConnectionStrings": {
    "AttachmentsDb": "Server=(localdb)\\mssqllocaldb;Database=TaskManagerAttachments;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "TaskManager.Attachments": "Debug"
    }
  }
}
```

### appsettings.Production.json

```json
{
  "FileStorage": {
    "BasePath": "/var/taskmanager/uploads/",
    "MaxFileSizeBytes": 10485760
  },
  "ConnectionStrings": {
    "AttachmentsDb": "${ATTACHMENTS_DB_CONNECTION_STRING}"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "TaskManager.Attachments": "Information"
    }
  }
}
```

## Troubleshooting

### Error: "Database not created"

```powershell
dotnet ef database update --project src/TaskManager.Attachments/TaskManager.Attachments.Infrastructure --startup-project src/TaskManager.Api
```

### Error: "File upload fails with 413 Payload Too Large"

Verificar configuración de Kestrel en `appsettings.json`:
```json
{
  "Kestrel": {
    "Limits": {
      "MaxRequestBodySize": 10485760
    }
  }
}
```

### Error: "File upload fails with permission denied"

Verificar que directorio `uploads/` existe y tiene permisos de escritura:
```powershell
# Windows
New-Item -ItemType Directory -Path "uploads" -Force
icacls uploads /grant Everyone:F

# Linux/Mac
mkdir -p uploads
chmod 777 uploads
```

### Error: "Tests fail with NullReferenceException"

Asegurar que fixtures y mocks están configurados correctamente. Ver `tests/TaskManager.Attachments.Tests/Fixtures/`.

## Next Steps

1. ✅ **Build**: `dotnet build`
2. ✅ **Test**: `dotnet test`
3. ✅ **Run**: `dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj`
4. 🔄 **Develop**: Implementar tareas según `tasks.md` (generado con `/speckit.tasks`)
5. 🔄 **Deploy**: Configurar CI/CD pipeline y desplegar a staging/producción

## Useful Links

- [Swagger UI](http://localhost:5000/index.html) - API documentation
- [Specification](spec.md) - Feature requirements
- [Data Model](data-model.md) - Entity design
- [API Contracts](contracts/) - DTOs and endpoints
- [Research](research.md) - Technical decisions

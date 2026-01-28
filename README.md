# Gestor Tareas API

## Comandos de desarrollo

### Restaurar paquetes
```bash
dotnet restore TaskManager.sln
```

### Compilar
```bash
dotnet build TaskManager.sln
```

### Ejecutar pruebas
```bash
dotnet test TaskManager.sln
```

### Ejecutar la API
```bash
dotnet run --project src/TaskManager.Api/TaskManager.Api.csproj
```

O desde el directorio del proyecto API:
```bash
cd src/TaskManager.Api
dotnet run
```

## Acceso a la API

### Documentación Swagger
Una vez que la API esté en ejecución, puedes acceder a la documentación interactiva de Swagger en:

```
http://localhost:5000/index.html
```

### Endpoints principales

La API está disponible en:
- **HTTP**: `http://localhost:5000`

Rutas base de los endpoints:
- `/api/v1/auth` - Autenticación (login, registro, refresh token)
- `/api/v1/tasks` - Gestión de tareas
- `/api/v1/projects` - Gestión de proyectos
- `/api/v1/labels` - Gestión de etiquetas
- `/api/v1/users` - Gestión de usuarios

### Autenticación
La mayoría de los endpoints requieren autenticación JWT. Para acceder:

1. Registra un usuario en `/api/v1/auth/register`
2. Inicia sesión en `/api/v1/auth/login` para obtener el token
3. Usa el token en el header `Authorization: Bearer {token}` en las peticiones

En Swagger, puedes usar el botón **Authorize** para configurar el token globalmente.  
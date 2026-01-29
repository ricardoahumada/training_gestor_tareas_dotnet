# API Endpoints: Módulo de Archivos Adjuntos

**Feature**: Módulo de Archivos Adjuntos
**Date**: 2026-01-29
**Base Path**: `/api/v1/attachments`

## Authentication

**Required**: Yes (JWT Bearer token)
**Header**: `Authorization: Bearer {token}`
**Claims**: `userId` (Guid), `role` (string)

## Endpoints

### 1. Upload Attachment

**Upload file to task**

```http
POST /api/v1/attachments
Content-Type: multipart/form-data
Authorization: Bearer {jwt_token}
```

**Request Body** (multipart/form-data):
```
taskId: {guid}           # Required, tarea destino
file: {binary}           # Required, archivo a subir
```

**Success Response** (201 Created):
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

**Error Responses**:
- `400 Bad Request`: Tarea inactiva, límite de 5 adjuntos alcanzado, archivo inválido
- `401 Unauthorized`: Token JWT inválido o ausente
- `404 Not Found`: Tarea no existe
- `413 Payload Too Large`: Archivo > 10 MB
- `415 Unsupported Media Type`: Tipo de archivo no permitido
- `500 Internal Server Error`: Error al guardar archivo

---

### 2. List Task Attachments

**Get all attachments for a task**

```http
GET /api/v1/attachments/task/{taskId}
Authorization: Bearer {jwt_token}
```

**Path Parameters**:
- `taskId` (Guid, required): ID de la tarea

**Success Response** (200 OK):
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

**Empty Result** (200 OK):
```json
[]
```

**Error Responses**:
- `401 Unauthorized`: Token JWT inválido o ausente
- `404 Not Found`: Tarea no existe

---

### 3. Download Attachment

**Download attachment file**

```http
GET /api/v1/attachments/{id}/download
Authorization: Bearer {jwt_token}
```

**Path Parameters**:
- `id` (Guid, required): ID del adjunto

**Success Response** (200 OK):
```http
HTTP/1.1 200 OK
Content-Type: image/jpeg
Content-Disposition: attachment; filename="screenshot.jpg"
Content-Length: 2048000

{binary file content}
```

**Error Responses**:
- `401 Unauthorized`: Token JWT inválido o ausente
- `404 Not Found`: Adjunto no existe o archivo físico no encontrado
- `410 Gone`: Archivo eliminado (tarea inactiva)
- `500 Internal Server Error`: Error al leer archivo

---

### 4. Get Attachment Metadata

**Get attachment details without downloading**

```http
GET /api/v1/attachments/{id}
Authorization: Bearer {jwt_token}
```

**Path Parameters**:
- `id` (Guid, required): ID del adjunto

**Success Response** (200 OK):
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

**Error Responses**:
- `401 Unauthorized`: Token JWT inválido o ausente
- `404 Not Found`: Adjunto no existe

---

### 5. Delete Attachment

**Delete attachment (requires ownership)**

```http
DELETE /api/v1/attachments/{id}
Authorization: Bearer {jwt_token}
```

**Path Parameters**:
- `id` (Guid, required): ID del adjunto

**Success Response** (204 No Content):
```http
HTTP/1.1 204 No Content
```

**Error Responses**:
- `401 Unauthorized`: Token JWT inválido o ausente
- `403 Forbidden`: Usuario no es propietario de la tarea
- `404 Not Found`: Adjunto no existe
- `500 Internal Server Error`: Error al eliminar archivo

---

## Rate Limiting

**Current**: No rate limiting in MVP (see research.md)

**Recommended for Production**:
```http
HTTP/1.1 429 Too Many Requests
Retry-After: 60

{
  "error": "Rate limit exceeded",
  "message": "Maximum 10 uploads per minute allowed",
  "retryAfter": 60
}
```

## CORS

**Allowed Origins**: Configured in `appsettings.json` (inherited from legacy API)
**Allowed Methods**: `GET`, `POST`, `DELETE`
**Allowed Headers**: `Authorization`, `Content-Type`

## Versioning

**Current Version**: v1
**URL Pattern**: `/api/v1/attachments`
**Future Versions**: `/api/v2/attachments` (breaking changes)

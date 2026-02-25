# AppSimple.WebApi — Structure & Documentation

## Overview

`AppSimple.WebApi` is the ASP.NET Core 10 REST API layer for AppSimple. It directly references
`AppSimple.Core` and `AppSimple.DataLib`, exposes JWT-secured endpoints, and is intended to be
consumed by `AppSimple.AdminCli` and `AppSimple.WebApp`.

---

## Project Structure

```
AppSimple.WebApi/
├── Controllers/
│   ├── PublicController.cs        # Unauthenticated endpoints (/, /public, /health)
│   ├── AuthController.cs          # Login + token validation
│   ├── ProtectedController.cs     # Authenticated user endpoints (/me, change-password)
│   └── AdminController.cs         # Admin-only user management
├── DTOs/
│   ├── LoginRequest.cs            # POST /api/auth/login request body
│   ├── LoginResponse.cs           # POST /api/auth/login response body
│   ├── CreateUserRequest.cs       # POST /api/admin/users request body
│   ├── UpdateUserRequest.cs       # PUT /api/admin/users/{uid} request body
│   ├── ChangePasswordRequest.cs   # POST /api/protected/me/change-password body
│   └── UserDto.cs                 # Safe user representation (no password hash)
├── Extensions/
│   └── WebApiServiceExtensions.cs # AddWebApiServices() DI setup
├── Middleware/
│   └── ExceptionMiddleware.cs     # Global domain-to-HTTP error mapping
├── Properties/
│   └── launchSettings.json
├── appsettings.json
├── appsettings.Development.json
└── Program.cs
```

---

## Endpoints

### Public (no auth required)

| Method | Path          | Description                          |
|--------|---------------|--------------------------------------|
| GET    | `/api`        | API status ping                      |
| GET    | `/api/public` | Public message                       |
| GET    | `/api/health` | Health check (status, timestamp, uptime) |

### Authentication

| Method | Path                  | Description                              |
|--------|-----------------------|------------------------------------------|
| POST   | `/api/auth/login`     | Login — returns JWT token + role         |
| GET    | `/api/auth/validate`  | Validates a token, returns username      |

**Login Request:**
```json
{ "username": "admin", "password": "Admin123!" }
```

**Login Response:**
```json
{ "token": "eyJ...", "username": "admin", "role": "Admin" }
```

### Protected (requires valid JWT bearer token)

| Method | Path                              | Description                          |
|--------|-----------------------------------|--------------------------------------|
| GET    | `/api/protected`                  | Returns authenticated username       |
| GET    | `/api/protected/me`               | Returns the current user's profile   |
| PUT    | `/api/protected/me`               | Updates own profile (non-privileged) |
| POST   | `/api/protected/me/change-password` | Changes own password               |

### Admin (requires `Admin` role)

| Method | Path                          | Description               |
|--------|-------------------------------|---------------------------|
| GET    | `/api/admin`                  | Admin access confirmation |
| GET    | `/api/admin/users`            | List all users            |
| GET    | `/api/admin/users/{uid}`      | Get user by UID           |
| POST   | `/api/admin/users`            | Create new user           |
| PUT    | `/api/admin/users/{uid}`      | Update user (all fields)  |
| DELETE | `/api/admin/users/{uid}`      | Delete user               |
| PATCH  | `/api/admin/users/{uid}/role` | Set user role             |

---

## Authentication

The API uses **JWT Bearer** tokens. Include the token in the `Authorization` header:

```
Authorization: Bearer <token>
```

---

## Configuration (`appsettings.json`)

```json
{
  "Database": { "ConnectionString": "Data Source=appsimple.db" },
  "Jwt": {
    "Secret": "change-this-secret-in-production-32chars!!",
    "Issuer": "AppSimple",
    "Audience": "AppSimple",
    "ExpirationMinutes": 480
  },
  "AppLogging": { "EnableFile": true, "LogDirectory": "logs" }
}
```

---

## Error Handling

All domain exceptions are mapped by `ExceptionMiddleware`:

| Exception                  | HTTP Status         |
|----------------------------|---------------------|
| `EntityNotFoundException`  | 404 Not Found       |
| `DuplicateEntityException` | 409 Conflict        |
| `UnauthorizedException`    | 401 Unauthorized    |
| `SystemEntityException`    | 403 Forbidden       |
| Any other exception        | 500 Internal Server |

Error responses use the format:
```json
{ "error": "Human-readable message" }
```

---

## Running Locally

```bash
cd src/AppSimple.WebApi
dotnet run
# API listens on http://localhost:5157 (development)
```

OpenAPI docs available at `/openapi/v1.json` in development mode.

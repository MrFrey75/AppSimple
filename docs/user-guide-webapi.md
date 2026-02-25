# WebApi — API Reference Guide

`AppSimple.WebApi` is the REST API backend for AppSimple. It is used by **WebApp** and **AdminCli**, and can also be called directly with any HTTP client (e.g. `curl`, Postman, or custom integrations).

## Running

```bash
cd src/AppSimple.WebApi
dotnet run
# Listening on http://localhost:5157
```

Swagger UI is available in Development mode at **http://localhost:5157/swagger**.

---

## Authentication

The API uses **JWT Bearer tokens**.

1. Obtain a token via `POST /api/auth/login`.
2. Pass the token in the `Authorization` header for all protected endpoints:

```http
Authorization: Bearer <your-token>
```

Tokens expire after **8 hours** by default.

---

## Endpoints

### Public Endpoints (no authentication required)

#### `GET /api`
Health ping. Returns a simple status message.

**Response 200:**
```json
{ "message": "AppSimple API is running." }
```

#### `GET /api/health`
Detailed health check.

**Response 200:**
```json
{
  "status": "healthy",
  "timestamp": "2025-06-01T10:00:00Z",
  "uptime": "00:05:22"
}
```

#### `POST /api/auth/login`
Authenticates a user and returns a JWT token.

**Request:**
```json
{
  "username": "admin",
  "password": "Admin123!"
}
```

**Response 200:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "username": "admin",
  "role": "Admin"
}
```

**Response 401:** Invalid credentials.

#### `GET /api/auth/validate?token={jwt}`
Validates a JWT token.

**Response 200:**
```json
{ "username": "admin", "valid": true }
```

**Response 400:** Token is invalid or expired.

---

### Protected Endpoints (requires valid JWT)

All endpoints below require the `Authorization: Bearer <token>` header.

#### `GET /api/protected`
Confirms the token is valid.

**Response 200:**
```json
{ "message": "Hello, admin! You are authenticated." }
```

#### `GET /api/protected/me`
Returns the current user's profile.

**Response 200:** [UserDto](#userdto-schema)

#### `PUT /api/protected/me`
Updates the current user's profile.

**Request:**
```json
{
  "firstName": "Alice",
  "lastName": "Smith",
  "phoneNumber": "+1-555-0100",
  "bio": "Developer",
  "dateOfBirth": "1990-01-15"
}
```

**Response 200:** Updated [UserDto](#userdto-schema)

#### `POST /api/protected/me/change-password`
Changes the current user's password.

**Request:**
```json
{
  "currentPassword": "Admin123!",
  "newPassword": "NewPass456!"
}
```

**Response 204:** Password changed successfully.
**Response 401:** `currentPassword` is incorrect.

---

### Admin Endpoints (requires JWT + Admin role)

#### `GET /api/admin`
Confirms admin access.

**Response 200:**
```json
{ "message": "Hello, admin! You have admin access." }
```

#### `GET /api/admin/users`
Returns all users.

**Response 200:** Array of [UserDto](#userdto-schema)

#### `GET /api/admin/users/{uid}`
Returns a single user by GUID.

**Response 200:** [UserDto](#userdto-schema)
**Response 404:** User not found.

#### `POST /api/admin/users`
Creates a new user.

**Request:**
```json
{
  "username": "alice",
  "email": "alice@example.com",
  "password": "Pass123!"
}
```

**Response 201:** Created [UserDto](#userdto-schema)
**Response 409:** Username or email already exists.

#### `PUT /api/admin/users/{uid}`
Updates an existing user's profile, role, or active status.

**Request:**
```json
{
  "firstName": "Alice",
  "lastName": "Smith",
  "phoneNumber": "+1-555-0101",
  "bio": "Senior Developer",
  "dateOfBirth": "1985-06-20",
  "role": 0,
  "isActive": true
}
```

Role values: `0` = User, `1` = Admin.

**Response 200:** Updated [UserDto](#userdto-schema)
**Response 404:** User not found.
**Response 403:** Cannot modify a system-reserved user.

#### `DELETE /api/admin/users/{uid}`
Deletes a user.

**Response 204:** User deleted.
**Response 404:** User not found.
**Response 403:** Cannot delete a system-reserved user.

#### `PATCH /api/admin/users/{uid}/role`
Sets a user's role.

**Request body:** `0` (User) or `1` (Admin)

**Response 204:** Role updated.
**Response 404:** User not found.

---

## UserDto Schema

Returned by most user-related endpoints:

```json
{
  "uid": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "username": "alice",
  "email": "alice@example.com",
  "firstName": "Alice",
  "lastName": "Smith",
  "fullName": "Alice Smith",
  "phoneNumber": "+1-555-0101",
  "bio": "Developer",
  "dateOfBirth": "1985-06-20T00:00:00",
  "role": 0,
  "isActive": true,
  "isSystem": false,
  "createdAt": "2025-01-01T10:00:00Z"
}
```

---

## Error Responses

| Status | Meaning |
|---|---|
| `400` | Bad request — invalid input |
| `401` | Unauthorized — missing or invalid token, or wrong password |
| `403` | Forbidden — insufficient role, or system-protected resource |
| `404` | Not found — resource does not exist |
| `409` | Conflict — duplicate username or email |

Error bodies include a machine-readable message:
```json
{ "error": "A user with this username already exists." }
```

---

## Configuration

| Setting | Key | Default |
|---|---|---|
| JWT secret | `Jwt:Secret` | `change-this-secret-in-production-32chars!!` |
| JWT issuer | `Jwt:Issuer` | `AppSimple` |
| JWT audience | `Jwt:Audience` | `AppSimple` |
| Token expiry (minutes) | `Jwt:ExpirationMinutes` | `480` |
| Database connection | `Database:ConnectionString` | OS default path |

> ⚠️ Always override `Jwt:Secret` with a strong random value in production.

---

## Quick Examples with curl

```bash
# Login and save token
TOKEN=$(curl -s -X POST http://localhost:5157/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}' | jq -r .token)

# Get own profile
curl -H "Authorization: Bearer $TOKEN" http://localhost:5157/api/protected/me

# List all users (admin)
curl -H "Authorization: Bearer $TOKEN" http://localhost:5157/api/admin/users

# Create a user (admin)
curl -X POST http://localhost:5157/api/admin/users \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"username":"bob","email":"bob@example.com","password":"Pass123!"}'
```

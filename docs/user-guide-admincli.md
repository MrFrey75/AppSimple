# AdminCli — User Guide

`AppSimple.AdminCli` is a terminal-based administration tool for **admin users only**. It communicates with the **AppSimple WebApi over HTTP** — the WebApi must be running before you launch AdminCli.

## Prerequisites

Start the WebApi first:

```bash
cd src/AppSimple.WebApi
dotnet run
# Listening on http://localhost:5157
```

Then run AdminCli:

```bash
cd src/AppSimple.AdminCli
dotnet run
```

## Login

```
══════════════════════════════════════════
         AppSimple AdminCLI
══════════════════════════════════════════
Username: admin
Password: ********
```

**Default credentials:** `admin` / `Admin123!`

Non-admin accounts are rejected at the login gate. Only users with the **Admin** role can proceed.

---

## Main Menu

```
══════════════════════════════════════════
  Logged in as: admin (Admin)
══════════════════════════════════════════
  [1]  User Management
  [2]  System & Health
  [0]  Logout
```

---

## User Management

### List All Users

Displays all users in a table with username, email, full name, role, and active status.

### Create New User

Prompted fields:
- **Username** — unique identifier
- **Email** — unique email address
- **Password** — must meet complexity requirements (8+ chars, upper, lower, digit)

The new user is created with **User** role and **active** status by default.

### View User Details

Select a user to see their full profile in a read-only detail view:

```
  ──────────────────────────────────────────
  Uid                 a1b2c3d4-...
  Username            alice
  Email               alice@example.com
  Full Name           Alice Smith
  Phone               +1-555-0101
  Date of Birth       1985-06-20
  Bio                 Developer
  Role                User
  Active              Yes
  System              No
  Created At          2025-01-01 10:00
  ──────────────────────────────────────────
```

### Edit User

Select a user and modify:
- Profile fields: first/last name, phone, bio, date of birth
- Role: User or Admin
- Active status: active or inactive

### Delete User

Select a user and type `yes` to confirm permanent deletion. System-reserved users cannot be deleted.

### Change User Role

Quickly toggle a user between **User** and **Admin** without opening the full edit form.

---

## System & Health

### Health Check

Queries `GET /api/health` and displays:

```
  Status:    healthy
  Timestamp: 2025-06-01T10:00:00Z
  Uptime:    00:05:22
```

### Smoke Test

Runs three automated checks:
1. ✅ API is reachable (`GET /api/health`)
2. ✅ Authentication works (login with admin credentials)
3. ✅ Admin endpoint is accessible (`GET /api/admin`)

Reports pass/fail for each step.

### Seed Test Users

Creates three test accounts if they do not already exist:

| Username | Email | Password |
|---|---|---|
| `testuser1` | `testuser1@example.com` | `TestPass1!` |
| `testuser2` | `testuser2@example.com` | `TestPass1!` |
| `testuser3` | `testuser3@example.com` | `TestPass1!` |

Useful for setting up a development or test environment quickly.

### Reset & Reseed Database

> ⚠️ **Destructive action.** This permanently wipes all users and restores the database to its default seeded state. You will be logged out.

Type `RESET` to confirm. After the reset:
- Default `admin` user is restored
- Three sample users are added: `alice`, `bob`, `carol` (password: `Sample123!`)

---

## Logging Out

Select **Logout** from the main menu. You will be returned to the login screen.
Press `Ctrl+C` at any time to exit.

---

## Configuration

AdminCli reads from `appsettings.json`:

```json
{
  "WebApi": {
    "BaseUrl": "http://localhost:5157"
  },
  "Database": {
    "ConnectionString": "Data Source=..."
  }
}
```

To connect to a different WebApi host, update `WebApi:BaseUrl`.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| "Connection refused" at login | The WebApi is not running. Start it first: `dotnet run` in `AppSimple.WebApi/`. |
| Login rejected even with correct credentials | Only Admin accounts can log into AdminCli. Verify the user has Admin role. |
| Health check fails | Confirm the WebApi URL in `appsettings.json` matches where the API is listening. |
| "System user" error on delete | System-reserved accounts (`⚙`) cannot be deleted. |

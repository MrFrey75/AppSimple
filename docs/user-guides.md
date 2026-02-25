# AppSimple — User Guides

This folder contains end-user and operator documentation for each component of the AppSimple solution.

## Components

| Component | What it is | Guide |
|---|---|---|
| **UserCLI** | Console app — end users manage their own profile and admin users manage all users | [user-guide-usercli.md](user-guide-usercli.md) |
| **AdminCli** | Admin-only console app — remote user management via WebApi | [user-guide-admincli.md](user-guide-admincli.md) |
| **WebApp** | Browser-based MVC front-end — full profile and admin management | [user-guide-webapp.md](user-guide-webapp.md) |
| **MvvmApp** | Cross-platform Avalonia desktop app — direct database access | [user-guide-mvvmapp.md](user-guide-mvvmapp.md) |
| **WebApi** | REST API — used by WebApp and AdminCli; also callable directly | [user-guide-webapi.md](user-guide-webapi.md) |

## Default Credentials

All components use the same database. On first run the database is seeded with:

| Username | Password | Role |
|---|---|---|
| `admin` | `Admin123!` | Admin |

> ⚠️ Change the admin password after first login in a production environment.

## Architecture Summary

```
UserCLI   ──┐
MvvmApp  ──┼──► Core + DataLib ──► SQLite (~/.local/share/AppSimple/)
             │
WebApp   ────┤
AdminCli ────┴──► WebApi (HTTP) ──► Core + DataLib ──► SQLite
```

## Quick Start

```bash
# Run only the database-direct apps (no WebApi needed):
cd src/AppSimple.UserCLI && dotnet run
cd src/AppSimple.MvvmApp && dotnet run

# Run the full HTTP stack:
cd src/AppSimple.WebApi  && dotnet run   # start first, listens on http://localhost:5157
cd src/AppSimple.WebApp  && dotnet run   # listens on http://localhost:5000
cd src/AppSimple.AdminCli && dotnet run  # requires WebApi running
```

See [running-locally.md](running-locally.md) for full setup instructions.

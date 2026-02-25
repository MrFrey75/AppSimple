# Running AppSimple Locally

Open a separate terminal window for each process. Start them in the order listed — WebApi must be running before WebApp or AdminCli.

---

## Terminal 1 — WebApi

```bash
pkill -f "AppSimple.WebApi" 2>/dev/null; sleep 1; cd ~/Projects/AppSimple/src && dotnet run --project AppSimple.WebApi/AppSimple.WebApi.csproj
```

Listens on `http://localhost:5000` by default.

---

## Terminal 2 — WebApp

```bash
pkill -f "AppSimple.WebApp" 2>/dev/null; sleep 1; cd ~/Projects/AppSimple/src && dotnet run --project AppSimple.WebApp/AppSimple.WebApp.csproj
```

Browse to `http://localhost:5001` once running.

---

## Terminal 3 — AdminCli

```bash
pkill -f "AppSimple.AdminCli" 2>/dev/null; sleep 1; cd ~/Projects/AppSimple/src && dotnet run --project AppSimple.AdminCli/AppSimple.AdminCli.csproj
```

Interactive console — requires WebApi to be reachable.

---

## Terminal 4 — MvvmApp (optional)

```bash
pkill -f "AppSimple.MvvmApp" 2>/dev/null; sleep 1; cd ~/Projects/AppSimple/src && dotnet run --project AppSimple.MvvmApp/AppSimple.MvvmApp.csproj
```

Desktop GUI — connects directly to the local SQLite database (no WebApi dependency).

---

## Terminal 5 — UserCLI (optional)

```bash
pkill -f "AppSimple.UserCLI" 2>/dev/null; sleep 1; cd ~/Projects/AppSimple/src && dotnet run --project AppSimple.UserCLI/AppSimple.UserCLI.csproj
```

Interactive console — connects directly to the local SQLite database (no WebApi dependency).

---

## Notes

- All data is stored in `~/.local/share/AppSimple/`
  - `appsimple.db` — SQLite database
  - `logs/` — Serilog structured log files
  - `config.json` — theme and app preferences
- The database schema is auto-initialized on first run.
- Default credentials: username `admin`, password `Admin123!`
- Default ports can be changed in each project's `appsettings.json`.

---

## Database Reset & Reseed

> ⚠ **Warning**: This operation erases **all** user data and cannot be undone.

The reset/reseed feature is available in **UserCLI** and **AdminCli** for admin users only:

- **UserCLI**: Login as admin → Main Menu → Admin → Option 5 "Reset & Reseed Database"
- **AdminCli**: Login as admin → Main Menu → System & Health → Option 4 "Reset & Reseed Database"

After reset:
- All existing users are deleted
- Default admin is re-created: `admin` / `Admin123!`
- Three sample users are seeded: `alice`, `bob`, `carol` (password: `Sample123!`)
- All active sessions are invalidated — you will be logged out automatically

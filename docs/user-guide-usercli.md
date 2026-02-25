# UserCLI — User Guide

`AppSimple.UserCLI` is a terminal-based console application for end users. It connects **directly to the local SQLite database** — no WebApi or network connection required.

## Running

```bash
cd src/AppSimple.UserCLI
dotnet run
```

The database is created automatically on first run at:
- **Linux/macOS:** `~/.local/share/AppSimple/appsimple.db`
- **Windows:** `%LOCALAPPDATA%\AppSimple\appsimple.db`

## Login

On startup you will see the login screen:

```
══════════════════════════════════════════
         AppSimple UserCLI
══════════════════════════════════════════
Username: admin
Password: ********
```

**Default credentials:** `admin` / `Admin123!`

- Passwords are masked as you type.
- Three failed attempts logs the event and returns to the login prompt.

---

## Main Menu

After a successful login you will see the main menu. Available options depend on your role.

### All Users

#### 1. My Profile

View your current profile:

```
  Username    admin
  Email       admin@example.com
  Full Name   —
  Role        Admin
  Member Since 2025-01-01
```

#### 2. Edit Profile

Update your personal details:

| Field | Example |
|---|---|
| First Name | `Alice` |
| Last Name | `Smith` |
| Phone Number | `+1-555-0100` |
| Date of Birth | `1990-01-15` (YYYY-MM-DD) |
| Bio | Free text |

Press **Enter** on an empty field to keep the existing value.

#### 3. Change Password

```
Current password: ********
New password:     ********
Confirm new:      ********
```

Rules for new password:
- Minimum 8 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- Must differ from the current password

---

### Admin Users Only

These options are only visible when logged in as an **Admin**.

#### 4. User Management

Opens the user management sub-menu:

| Option | Description |
|---|---|
| List All Users | Tabular view of all users with role and active status |
| Create New User | Username, email, password, role assignment |
| Edit User | Modify profile, role, and active status of any user |
| Delete User | Permanently delete a user (system users are protected) |
| Reset & Reseed Database | ⚠️ Wipe all data and restore defaults |

##### List All Users

Displays a table:

```
  #    Username             Email                        Role     Active
  ──   ───────────────────  ───────────────────────────  ───────  ──────
  1    admin ⚙             admin@example.com            Admin    Yes
  2    alice                alice@example.com            User     Yes
```

The `⚙` icon marks system-reserved accounts.

##### Create New User

Prompted fields:
- **Username** — must be unique, letters/numbers/underscores only
- **Email** — must be unique, valid email format
- **Password** — must meet password rules above
- **Role** — `1` for Admin, `2` for User

##### Edit User

Select a user by number from the list, then modify:
- Profile fields (first/last name, phone, bio, DOB)
- Role (User / Admin)
- Active status (active / inactive)

##### Delete User

- Select a user by number.
- Type `yes` to confirm permanent deletion.
- System users (`⚙`) cannot be deleted.

##### Reset & Reseed Database

> ⚠️ **Destructive action.** This permanently deletes all users and resets the database to its default seeded state. You will be logged out immediately.

- Type `RESET` to confirm.
- After reset: default `admin` user is restored plus three sample users (`alice`, `bob`, `carol`).

---

## Logging Out

Select **Logout** from the main menu. You will be returned to the login screen.
Press `Ctrl+C` at any time to exit the application.

---

## Keyboard Shortcuts

| Key | Action |
|---|---|
| `Enter` | Confirm input / select option |
| `Ctrl+C` | Exit application |
| `Backspace` | Delete last character in input |

---

## Troubleshooting

| Problem | Solution |
|---|---|
| "Database not found" on startup | The database is created automatically — check write permissions on `~/.local/share/AppSimple/` |
| Login fails with correct password | Verify the database hasn't been reset. Default password is `Admin123!`. |
| Password change fails | Ensure the new password meets all complexity requirements. |
| Cannot delete a user | Users marked with `⚙` are system-reserved and cannot be deleted. |

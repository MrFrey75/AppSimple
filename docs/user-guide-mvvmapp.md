# MvvmApp — User Guide

`AppSimple.MvvmApp` is a cross-platform Avalonia desktop application. Like UserCLI, it connects **directly to the local SQLite database** — no WebApi or network connection is required.

## Running

```bash
cd src/AppSimple.MvvmApp
dotnet run
```

The database is shared with UserCLI and created automatically on first run at:
- **Linux/macOS:** `~/.local/share/AppSimple/appsimple.db`
- **Windows:** `%LOCALAPPDATA%\AppSimple\appsimple.db`

---

## Logging In

The login bar is located in the **top-right corner** of the main window.

```
[ Username ] [ ••••••••• ] [ Login ]
```

**Default credentials:** `admin` / `Admin123!`

On success the login bar is replaced with:

```
Welcome, admin (Admin)    [ Logout ]
```

And the navigation sidebar expands to show role-appropriate options.

---

## Navigation

The left sidebar provides navigation between views:

| Item | Visible to | View |
|---|---|---|
| **Home** | Everyone | Welcome page |
| **Profile** | Logged-in users | View and edit your profile |
| **Users** | Admin users only | Full user management |

---

## Home

The home page displays a welcome message when you are logged in and a prompt to log in when you are not. No actions are available on this page.

---

## Profile

### Viewing Your Profile

The Profile view displays your current account information in read-only fields at the top:

| Field | Notes |
|---|---|
| Username | Read-only |
| Email | Read-only |
| Role | `User` or `Admin` |
| Member Since | Account creation date |

### Editing Your Profile

Editable fields below the read-only section:

| Field | Notes |
|---|---|
| First Name | Optional |
| Last Name | Optional |
| Phone Number | Optional |
| Bio | Optional, free text |
| Date of Birth | Optional, enter as `YYYY-MM-DD` |

Click **Save Profile** to save changes. A success or error message appears below the form.

### Changing Your Password

A separate section in the Profile view:

| Field | Notes |
|---|---|
| Current Password | Required for verification |
| New Password | Min 8 chars, upper + lower + digit |
| Confirm New Password | Must match new password |

Click **Change Password** to apply.

---

## Users (Admin Only)

The Users view is only accessible when logged in as an **Admin**. It contains a data grid listing all users and a form panel for create/edit operations.

### User List (DataGrid)

Columns: Username, Email, Full Name, Role, Active status.

Click a row to select a user — this enables the **Edit** and **Delete** buttons.

### Creating a User

1. Click **New User** to open the create form.
2. Fill in the required fields:

| Field | Required | Notes |
|---|---|---|
| Username | ✅ | Unique |
| Email | ✅ | Unique, valid format |
| Password | ✅ | Min 8 chars, upper + lower + digit |
| First Name | Optional | |
| Last Name | Optional | |
| Role | ✅ | User or Admin |
| Active | ✅ | Checked = active |

3. Click **Save** to create the user. The grid refreshes automatically.
4. Click **Cancel** to discard without saving.

### Editing a User

1. Select a user in the grid.
2. Click **Edit Selected**.
3. Modify the profile fields, role, or active status (password cannot be changed here).
4. Click **Save** to apply changes.
5. Click **Cancel** to discard.

### Deleting a User

1. Select a user in the grid.
2. Click **Delete Selected**.
3. Confirm the deletion in the dialog that appears.

System-reserved users (seeded admin account) cannot be deleted.

---

## Themes

A theme drop-down is available in the top-right area of the window (next to the login bar). Available themes:

| Theme | Description |
|---|---|
| Dark | Default dark theme |
| Light | Light theme |
| Solarized | Warm solarized palette |
| Dracula | Purple-tinted dark theme |
| Nord | Cool blue-grey theme |

The selected theme is saved to `config.json` and restored on the next launch.

---

## Keyboard Shortcuts

| Shortcut | Action |
|---|---|
| `Tab` | Move between form fields |
| `Enter` | Submit focused button / form |
| `Escape` | Cancel / close form panel |

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Window does not open | Ensure Avalonia runtime dependencies are installed. Run `dotnet run` and check for error output. |
| Login fails | Default credentials are `admin` / `Admin123!`. Verify the database hasn't been reset. |
| Users view not visible | You must be logged in as an Admin. |
| Changes not saving | Ensure the app has write permissions to the database file at `~/.local/share/AppSimple/`. |
| Theme not persisting | Check write permissions on `config.json` in the app directory. |

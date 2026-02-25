# WebApp — User Guide

`AppSimple.WebApp` is a browser-based front-end for AppSimple. It communicates with the **AppSimple WebApi over HTTP** — the WebApi must be running before you open the web app.

## Prerequisites

Start the WebApi first:

```bash
cd src/AppSimple.WebApi
dotnet run
# Listening on http://localhost:5157
```

Then start the WebApp:

```bash
cd src/AppSimple.WebApp
dotnet run
# Listening on http://localhost:5000
```

Open your browser at **http://localhost:5000**.

---

## Logging In

Navigate to **http://localhost:5000/login** or click **Login** in the navigation bar.

**Default credentials:** `admin` / `Admin123!`

On success you are redirected to the home page with your username displayed in the header.

---

## Navigation

The top navigation bar contains:

| Link | Visible to | Destination |
|---|---|---|
| **Home** | Everyone | `/` |
| **Profile** | Logged-in users | `/profile` |
| **Admin** | Admin users only | `/admin` |
| **Login** | Guests only | `/login` |
| **Logout** | Logged-in users | (form POST, clears session) |

---

## Profile

### View Profile — `/profile`

Displays your current account information:

| Field | Description |
|---|---|
| Username | Your login name (read-only) |
| Email | Your email address (read-only) |
| Full Name | First + last name |
| Phone | Phone number |
| Date of Birth | In `YYYY-MM-DD` format |
| Bio | Short biography |
| Role | User or Admin |
| Member Since | Account creation date |

### Edit Profile — `/profile/edit`

Update your personal details. Username and email are not editable here.

| Field | Notes |
|---|---|
| First Name | Optional |
| Last Name | Optional |
| Phone Number | Optional |
| Date of Birth | Optional, `YYYY-MM-DD` |
| Bio | Optional, free text |

Click **Save Changes** to apply. A success or error banner is shown at the top of the page.

### Change Password — `/profile/change-password`

| Field | Notes |
|---|---|
| Current Password | Required for verification |
| New Password | Min 8 chars, upper + lower + digit |
| Confirm New Password | Must match new password |

---

## Admin Panel (Admin Users Only)

Accessible via the **Admin** link in the navigation bar. Non-admin users receive a **403 Forbidden** response.

### User List — `/admin`

A table of all users in the system:

| Column | Description |
|---|---|
| Username | Login name |
| Email | Email address |
| Full Name | First + last name (if set) |
| Role | `Admin` or `User` |
| Active | ✅ active / ❌ inactive |
| Created | Account creation date |

Buttons per row: **Edit**, **Delete**. A **Create New User** button appears at the top.

### Create User — `/admin/create`

| Field | Required | Notes |
|---|---|---|
| Username | ✅ | Unique, letters/numbers/underscores |
| Email | ✅ | Unique, valid format |
| Password | ✅ | Min 8 chars, upper + lower + digit |

### Edit User — `/admin/edit/{uid}`

Modify any user's profile and account settings:

| Field | Notes |
|---|---|
| First / Last Name | Optional |
| Phone / Bio / DOB | Optional |
| Role | User (0) or Admin (1) |
| Active | Checked = active account |

### Delete User — (POST `/admin/delete/{uid}`)

Triggered by the **Delete** button on the user list. Prompts for confirmation via browser dialog. System-reserved users cannot be deleted.

---

## Themes

A theme selector is available in the page footer (or navigation bar). Available themes:

| Theme | Description |
|---|---|
| Dark | Default dark theme |
| Light | Light theme |
| Solarized | Warm solarized palette |
| Dracula | Purple-tinted dark theme |
| Nord | Cool blue-grey theme |

The selected theme is persisted to `config.json` and remembered between sessions.

---

## Session & Security

- Sessions use **cookies** — your JWT token is stored as a session claim.
- The session expires when the JWT token expires (default: **8 hours**).
- All state-changing forms include **CSRF protection** via antiforgery tokens.
- Log out via the **Logout** button to clear your session immediately.

---

## Troubleshooting

| Problem | Solution |
|---|---|
| Blank page or connection error | Ensure the WebApi is running on `http://localhost:5157`. |
| Login fails | Verify credentials. Default is `admin` / `Admin123!`. |
| Admin menu not visible | The logged-in account must have the Admin role. |
| Theme not saving | Check write permissions on the `config.json` file in the app directory. |
| 403 on admin pages | You are logged in as a regular user. Log in with an Admin account. |

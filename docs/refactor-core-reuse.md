# Core Reuse Refactoring Opportunities

Analysis of duplicated code across AppSimple projects that could be moved to
`AppSimple.Core` for better reuse and maintainability.

---

## 🔴 High Priority — Duplicated Files That Should Not Exist

### 1. `LogPath` — exists in Core but also copied in WebApp

| Location | Status |
|---|---|
| `AppSimple.Core/Logging/LogPath.cs` | ✅ Canonical |
| `AppSimple.WebApp/Extensions/LogPath.cs` | ❌ Duplicate — delete |

**Fix**: Delete `WebApp/Extensions/LogPath.cs` and add a Core project reference to WebApp.

---

### 2. `AppConfigPath` + `AppConfigService` — exists in Core but also copied in WebApp

| Location | Status |
|---|---|
| `AppSimple.Core/Config/AppConfigPath.cs` | ✅ Canonical |
| `AppSimple.Core/Config/Impl/AppConfigService.cs` | ✅ Canonical |
| `AppSimple.WebApp/Config/AppConfigPath.cs` | ❌ Duplicate — delete |
| `AppSimple.WebApp/Config/AppConfigService.cs` | ❌ Duplicate — delete |

**Fix**: Delete both WebApp copies and add a Core project reference to WebApp.

> **⚠️ Design Decision Required**: WebApp intentionally has **no Core reference** today (HTTP-only architecture).
> Items 1 and 2 require accepting that dependency. Adding it would also unlock items 7–9 below.

---

## 🟠 Medium Priority — Request/Response Model Conflicts

### 3. `UpdateUserRequest` — 4 copies with differing fields

| Location | Has `Role`/`IsActive`? | Has `AvatarUrl`? |
|---|---|---|
| `AppSimple.Core/Models/Requests/UpdateUserRequest.cs` | ❌ | ✅ |
| `AppSimple.WebApi/DTOs/UpdateUserRequest.cs` | ✅ | ❌ |
| `AppSimple.WebApp/Services/UpdateProfileRequest.cs` | ✅ | ❌ |
| `AppSimple.AdminCli/Services/UpdateUserRequest.cs` | ✅ | ❌ |

**Fix**: Merge all fields into the Core version. WebApi, WebApp, and AdminCli use
`AppSimple.Core.Models.Requests.UpdateUserRequest` directly and remove their local copies.

---

### 4. `CreateUserRequest` — 2 copies with differing fields

| Location | Has `FirstName`/`LastName`? |
|---|---|
| `AppSimple.Core/Models/Requests/CreateUserRequest.cs` | ✅ |
| `AppSimple.WebApi/DTOs/CreateUserRequest.cs` | ❌ (minimal) |

**Fix**: WebApi should use or extend the Core version and remove its local copy.

---

### 5. `ChangePasswordRequest` — 2 copies

| Location | Has `ConfirmNewPassword`? |
|---|---|
| `AppSimple.Core/Models/Requests/ChangePasswordRequest.cs` | ✅ |
| `AppSimple.WebApi/DTOs/ChangePasswordRequest.cs` | ❌ |

**Fix**: WebApi adopts the Core version and removes its local copy.

---

### 6. `LoginRequest` — only in WebApi, should be in Core

- **Current**: `AppSimple.WebApi/DTOs/LoginRequest.cs`
- **Fix**: Move to `AppSimple.Core/Models/Requests/LoginRequest.cs`; WebApi references Core version.

---

## 🟡 Lower Priority — HTTP Client & DTO Duplication

### 7. `UserDto` — 3 copies

| Location | Notes |
|---|---|
| `AppSimple.WebApi/DTOs/UserDto.cs` | Has `.From(User)` mapper (server-side only) |
| `AppSimple.WebApp/Services/UserDto.cs` | Plain DTO |
| `AppSimple.AdminCli/Services/UserDto.cs` | Plain DTO, includes `IsSystem` field |

**Fix**: Move a plain DTO (no `.From()` mapper) to `AppSimple.Core/Models/DTOs/UserDto.cs`.
Keep `.From()` as an extension or static method inside WebApi only.

---

### 8. `LoginResult` — 2 copies

| Location |
|---|
| `AppSimple.WebApp/Services/LoginResult.cs` |
| `AppSimple.AdminCli/Services/LoginResult.cs` |

**Fix**: Move to `AppSimple.Core/Models/DTOs/LoginResult.cs`.

---

### 9. `IApiClient` + `ApiClient` — in both WebApp and AdminCli (~80% overlap)

| Location | Extra Methods |
|---|---|
| `AppSimple.WebApp/Services/IApiClient.cs` + `Impl/ApiClient.cs` | `GetMe`, `UpdateMe`, `ChangePassword` |
| `AppSimple.AdminCli/Services/IApiClient.cs` + `Impl/ApiClient.cs` | `SetUserRole`, `GetHealth`, `PingProtected` |

**Fix**: Extract a shared `IApiClient` base interface and `ApiClient` base class into
`AppSimple.Core/Http/`. Each project extends with project-specific methods.

> **⚠️ Dependency Note**: This requires both WebApp and AdminCli to reference Core.
> AdminCli already does. WebApp does not (see design decision in items 1–2).

---

## Refactoring Summary

| Priority | Item | Files to Remove / Move |
|---|---|---|
| 🔴 | `LogPath` duplicate | Delete `WebApp/Extensions/LogPath.cs` |
| 🔴 | `AppConfigPath`/`AppConfigService` duplicates | Delete 2 WebApp files |
| 🟠 | `UpdateUserRequest` merge | Remove 3 non-Core copies, merge fields into Core |
| 🟠 | `CreateUserRequest` align | Remove `WebApi/DTOs` copy |
| 🟠 | `ChangePasswordRequest` align | Remove `WebApi/DTOs` copy |
| 🟠 | `LoginRequest` move | Move from `WebApi/DTOs` → Core |
| 🟡 | `UserDto` consolidate | Remove 2 non-Core copies |
| 🟡 | `LoginResult` consolidate | Remove 2 non-Core copies |
| 🟡 | `IApiClient`/`ApiClient` merge | Consolidate 2 sets into Core base |

---

## Key Decision

> **Should WebApp reference AppSimple.Core?**
>
> Currently, WebApp is designed as an HTTP-only frontend with no direct domain dependency.
> Adding a Core reference would allow eliminating ~5 duplicate files but introduces a coupling
> that may be undesirable if WebApp is ever deployed separately or replaced.
>
> **Options:**
> 1. **Accept the dependency** — simpler, less duplication, Core stays the single source of truth.
> 2. **Keep WebApp isolated** — maintain local copies, accept some duplication as the cost of separation.
> 3. **Extract a shared `AppSimple.Contracts` project** — DTOs and request models only, no business logic.
>    WebApp, WebApi, AdminCli all reference it. Core may optionally reference it too.
>    This is the cleanest long-term architecture.

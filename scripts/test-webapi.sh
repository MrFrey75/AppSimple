#!/usr/bin/env bash
# ============================================================
# AppSimple WebApi — curl smoke-test script
#
# USAGE
#   ./scripts/test-webapi.sh [base_url]
#
# ARGUMENTS
#   base_url   Optional. Base URL of the running WebApi.
#              Defaults to http://localhost:5157
#
# EXAMPLES
#   ./scripts/test-webapi.sh
#   ./scripts/test-webapi.sh https://localhost:7095
#   ./scripts/test-webapi.sh http://myserver:8080
#
# PREREQUISITES
#   • curl and dotnet must be installed
#   • Default admin credentials must be seeded (admin / Admin123!)
#
# SERVER LIFECYCLE
#   The script automatically builds and starts the WebApi before
#   running tests, then shuts it down when finished (or on error).
#   If the API is already running externally, it will be used as-is
#   and will NOT be shut down by this script.
#
# WHAT IT TESTS
#   Public     — /api, /api/public, /api/health
#   Auth       — login (good + bad creds), token validate (good + bad)
#   Protected  — authenticated access, GET/PUT /me, change-password
#   Admin      — list users, create, get by UID, update, set role, delete
#
# OUTPUT
#   ✓  green  — check passed
#   ✗  red    — check failed (shows expected vs actual)
#
# NOTES
#   • A temporary test user (testuser_curl) is created and deleted
#     automatically during the Admin tests.
#   • The script exits early with a non-zero code if login fails,
#     since all remaining tests depend on a valid JWT token.
# ============================================================

BASE="${1:-http://localhost:5157}"
ADMIN_USER="admin"
ADMIN_PASS="Admin123!"
TOKEN=""
NEW_UID=""
API_PID=""
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$SCRIPT_DIR/../src/AppSimple.WebApi/AppSimple.WebApi.csproj"

# ── helpers ─────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; CYAN='\033[0;36m'; YELLOW='\033[0;33m'; RESET='\033[0m'

pass() { echo -e "${GREEN}  ✓ $1${RESET}"; }
fail() { echo -e "${RED}  ✗ $1${RESET}"; }
section() { echo -e "\n${CYAN}── $1 ──${RESET}"; }
info() { echo -e "${YELLOW}  → $1${RESET}"; }

check() {
  local label="$1" expected="$2" actual="$3"
  if [[ "$actual" == *"$expected"* ]]; then
    pass "$label"
  else
    fail "$label  (expected to contain: '$expected', got: '$actual')"
  fi
}

# ── server lifecycle ─────────────────────────────────────────
shutdown_api() {
  if [[ -n "$API_PID" ]]; then
    info "Stopping WebApi (PID $API_PID)..."
    kill "$API_PID" 2>/dev/null
    wait "$API_PID" 2>/dev/null
    info "WebApi stopped."
  fi
}
trap shutdown_api EXIT

# Check if the API is already up; if not, build and start it
if curl -sf "$BASE/api/health" >/dev/null 2>&1; then
  info "WebApi already running at $BASE — using existing instance."
else
  info "Building WebApi..."
  # Remove stale MSBuild artifacts that cause spurious CoreGenerateAssemblyInfo failures
  rm -rf "$SCRIPT_DIR/../src/AppSimple.Core/obj/Debug"
  dotnet build "$PROJECT" --nologo -q || { echo -e "${RED}Build failed — aborting.${RESET}"; exit 1; }

  info "Starting WebApi at $BASE..."
  ASPNETCORE_URLS="$BASE" dotnet run --project "$PROJECT" --no-build --nologo \
    >/tmp/appsimple-webapi.log 2>&1 &
  API_PID=$!

  # Wait up to 20 s for the server to become ready
  for i in $(seq 1 20); do
    sleep 1
    if curl -sf "$BASE/api/health" >/dev/null 2>&1; then
      pass "WebApi ready (PID $API_PID)"
      break
    fi
    if ! kill -0 "$API_PID" 2>/dev/null; then
      echo -e "${RED}WebApi process exited unexpectedly. See /tmp/appsimple-webapi.log${RESET}"
      exit 1
    fi
    if [[ $i -eq 20 ]]; then
      echo -e "${RED}WebApi did not start within 20 s. See /tmp/appsimple-webapi.log${RESET}"
      exit 1
    fi
  done
fi

# ── Public endpoints ─────────────────────────────────────────
section "Public"

RES=$(curl -sf "$BASE/api" 2>&1); check "GET /api" "AppSimple" "$RES"
RES=$(curl -sf "$BASE/api/public" 2>&1); check "GET /api/public" "public" "$RES"
RES=$(curl -sf "$BASE/api/health" 2>&1); check "GET /api/health" "healthy" "$RES"

# ── Auth ─────────────────────────────────────────────────────
section "Auth"

# Bad credentials → 401
HTTP=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"WrongPassword!"}')
check "POST /api/auth/login  (bad creds → 401)" "401" "$HTTP"

# Good credentials → token
RES=$(curl -sf -X POST "$BASE/api/auth/login" \
  -H "Content-Type: application/json" \
  -d "{\"username\":\"$ADMIN_USER\",\"password\":\"$ADMIN_PASS\"}")
check "POST /api/auth/login  (good creds)" "token" "$RES"

TOKEN=$(echo "$RES" | grep -o '"token":"[^"]*"' | cut -d'"' -f4)
if [[ -z "$TOKEN" ]]; then
  fail "Could not extract JWT token — aborting remaining tests"
  exit 1
fi
pass "JWT token extracted"

# Validate token
RES=$(curl -sf "$BASE/api/auth/validate?token=$TOKEN" 2>&1)
check "GET /api/auth/validate" "valid" "$RES"

# Bad token
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/auth/validate?token=bad.token.here")
check "GET /api/auth/validate (bad token → 400)" "400" "$HTTP"

# ── Protected (any authenticated user) ──────────────────────
section "Protected"

RES=$(curl -sf "$BASE/api/protected" -H "Authorization: Bearer $TOKEN" 2>&1)
check "GET /api/protected" "authenticated" "$RES"

RES=$(curl -sf "$BASE/api/protected/me" -H "Authorization: Bearer $TOKEN" 2>&1)
check "GET /api/protected/me" "admin" "$RES"

# Unauthenticated → 401
HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/protected")
check "GET /api/protected (no token → 401)" "401" "$HTTP"

# Update own profile
RES=$(curl -sf -X PUT "$BASE/api/protected/me" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Super","lastName":"Admin","bio":"System administrator"}')
check "PUT /api/protected/me" "Super" "$RES"

# Change password (use wrong current password → should error)
HTTP=$(curl -s -o /dev/null -w "%{http_code}" -X POST "$BASE/api/protected/me/change-password" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"currentPassword":"WrongOldPassword!","newPassword":"NewPass123!"}')
check "POST /api/protected/me/change-password (wrong current → non-2xx)" "4" "$HTTP"

# ── Admin ────────────────────────────────────────────────────
section "Admin"

# GET /api/admin
RES=$(curl -sf "$BASE/api/admin" -H "Authorization: Bearer $TOKEN" 2>&1)
check "GET /api/admin" "admin access" "$RES"

# GET /api/admin/users
RES=$(curl -sf "$BASE/api/admin/users" -H "Authorization: Bearer $TOKEN" 2>&1)
check "GET /api/admin/users" "username" "$RES"

# POST /api/admin/users — create a test user
RES=$(curl -sf -X POST "$BASE/api/admin/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"username":"testuser_curl","email":"curl@test.local","password":"Sample123!"}')
check "POST /api/admin/users" "testuser_curl" "$RES"

NEW_UID=$(echo "$RES" | grep -o '"uid":"[^"]*"' | cut -d'"' -f4)
if [[ -z "$NEW_UID" ]]; then
  fail "Could not extract new user UID — skipping UID-based tests"
else
  pass "New user UID: $NEW_UID"

  # GET /api/admin/users/{uid}
  RES=$(curl -sf "$BASE/api/admin/users/$NEW_UID" -H "Authorization: Bearer $TOKEN" 2>&1)
  check "GET /api/admin/users/{uid}" "testuser_curl" "$RES"

  # PUT /api/admin/users/{uid}
  RES=$(curl -sf -X PUT "$BASE/api/admin/users/$NEW_UID" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '{"firstName":"Curl","lastName":"Tester","isActive":true}')
  check "PUT /api/admin/users/{uid}" "Curl" "$RES"

  # PATCH /api/admin/users/{uid}/role  (set to Admin=1)
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" -X PATCH "$BASE/api/admin/users/$NEW_UID/role" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -d '1')
  check "PATCH /api/admin/users/{uid}/role → 204" "204" "$HTTP"

  # DELETE /api/admin/users/{uid}
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" -X DELETE "$BASE/api/admin/users/$NEW_UID" \
    -H "Authorization: Bearer $TOKEN")
  check "DELETE /api/admin/users/{uid} → 204" "204" "$HTTP"

  # Confirm deletion → 404
  HTTP=$(curl -s -o /dev/null -w "%{http_code}" "$BASE/api/admin/users/$NEW_UID" \
    -H "Authorization: Bearer $TOKEN")
  check "GET deleted user → 404" "404" "$HTTP"
fi

echo ""
echo "Done. Server: $BASE"
echo "(Log: /tmp/appsimple-webapi.log)"

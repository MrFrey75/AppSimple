#!/usr/bin/env bash
# ============================================================
# AppSimple WebApi — curl smoke-test script
# Usage: ./scripts/test-webapi.sh [base_url]
# Default base URL: http://localhost:5157
# ============================================================

BASE="${1:-http://localhost:5157}"
ADMIN_USER="admin"
ADMIN_PASS="Admin123!"
TOKEN=""
NEW_UID=""

# ── helpers ─────────────────────────────────────────────────
RED='\033[0;31m'; GREEN='\033[0;32m'; CYAN='\033[0;36m'; RESET='\033[0m'

pass() { echo -e "${GREEN}  ✓ $1${RESET}"; }
fail() { echo -e "${RED}  ✗ $1${RESET}"; }
section() { echo -e "\n${CYAN}── $1 ──${RESET}"; }

check() {
  local label="$1" expected="$2" actual="$3"
  if [[ "$actual" == *"$expected"* ]]; then
    pass "$label"
  else
    fail "$label  (expected to contain: '$expected', got: '$actual')"
  fi
}

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

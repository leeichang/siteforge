#!/usr/bin/env zsh
set -euo pipefail
unsetopt BG_NICE 2>/dev/null || true

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
API_DIR="$ROOT_DIR/backend/SiteForge/SiteForge.Api"
UI_DIR="$ROOT_DIR/frontend/siteforge-ui"
RUN_DIR="$ROOT_DIR/artifacts/service"
BACKEND_SLN="$ROOT_DIR/backend/SiteForge/SiteForge.sln"
API_DLL="$API_DIR/bin/Debug/net8.0/SiteForge.Api.dll"
UI_DIST_INDEX="$UI_DIR/dist/index.html"

UI_PORT="${SITEFORGE_UI_PORT:-5010}"
API_PORT="${SITEFORGE_API_PORT:-8000}"
UI_URL="http://127.0.0.1:$UI_PORT"
API_URL="http://127.0.0.1:$API_PORT"
DOTNET_BIN="${DOTNET_BIN:-/opt/homebrew/opt/dotnet@8/bin/dotnet}"
DB_HOST="${SITEFORGE_DB_HOST:-localhost}"
DB_PORT="${SITEFORGE_DB_PORT:-5432}"
DB_USER="${SITEFORGE_DB_USER:-postgres}"

mkdir -p "$RUN_DIR"

if [[ ! -x "$DOTNET_BIN" ]]; then
  DOTNET_BIN="$(command -v dotnet || true)"
fi

if [[ -z "$DOTNET_BIN" ]]; then
  print "dotnet was not found. Set DOTNET_BIN or install .NET 8." >&2
  exit 1
fi

is_pid_alive() {
  local pid="${1:-}"
  [[ -n "$pid" ]] && kill -0 "$pid" >/dev/null 2>&1
}

ensure_port_free() {
  local port="$1"
  local name="$2"
  if lsof -ti tcp:"$port" >/dev/null 2>&1; then
    print "$name port $port is already in use. Run ./stop_service.sh first, or set a different port." >&2
    exit 1
  fi
}

check_postgres() {
  if ! command -v pg_isready >/dev/null 2>&1; then
    return
  fi

  if ! pg_isready -h "$DB_HOST" -p "$DB_PORT" -U "$DB_USER" >/dev/null 2>&1; then
    print "Warning: PostgreSQL did not report ready at $DB_HOST:$DB_PORT." >&2
    print "If API startup fails, start PostgreSQL first and run ./start_service.sh again." >&2
  fi
}

needs_backend_build() {
  [[ ! -f "$API_DLL" ]] && return 0

  find "$ROOT_DIR/backend/SiteForge" \
    \( -name '*.cs' -o -name '*.csproj' -o -name '*.sln' -o -name 'appsettings*.json' \) \
    -newer "$API_DLL" -print -quit | grep -q .
}

needs_frontend_build() {
  [[ ! -f "$UI_DIST_INDEX" ]] && return 0

  find "$UI_DIR/src" "$UI_DIR/public" \
    "$UI_DIR/index.html" "$UI_DIR/package.json" "$UI_DIR/package-lock.json" "$UI_DIR/vite.config.js" \
    -newer "$UI_DIST_INDEX" -print -quit 2>/dev/null | grep -q .
}

build_backend_if_needed() {
  local log_file="$1"

  if needs_backend_build; then
    print "Building SiteForge backend..."
    "$DOTNET_BIN" build "$BACKEND_SLN" > "$log_file" 2>&1
    print "Backend build complete."
  else
    : > "$log_file"
  fi
}

build_frontend_if_needed() {
  local log_file="$1"

  if needs_frontend_build; then
    print "Building SiteForge frontend..."
    (
      cd "$UI_DIR"
      npm run build
    ) >> "$log_file" 2>&1
    print "Frontend build complete."
  fi
}

start_api() {
  local pid_file="$RUN_DIR/api.pid"
  local log_file="$RUN_DIR/api.log"

  if [[ -f "$pid_file" ]] && is_pid_alive "$(<"$pid_file")"; then
    print "SiteForge API is already running at $API_URL (pid $(<"$pid_file"))."
    return
  fi

  ensure_port_free "$API_PORT" "API"

  build_backend_if_needed "$log_file"

  (
    cd "$API_DIR"
    ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Production}" \
      nohup "$DOTNET_BIN" "$API_DLL" --urls "$API_URL" \
      >> "$log_file" 2>&1 &
    print $! > "$pid_file"
  )
  wait_for_url "$API_URL/swagger/v1/swagger.json" "$log_file"
  print "Started SiteForge API at $API_URL (pid $(<"$pid_file"))."
  print "  log: $log_file"
}

start_ui() {
  local pid_file="$RUN_DIR/ui.pid"
  local log_file="$RUN_DIR/ui.log"

  if [[ -f "$pid_file" ]] && is_pid_alive "$(<"$pid_file")"; then
    print "SiteForge UI is already running at $UI_URL (pid $(<"$pid_file"))."
    return
  fi

  ensure_port_free "$UI_PORT" "UI"
  : > "$log_file"
  build_frontend_if_needed "$log_file"

  (
    cd "$UI_DIR"
    SITEFORGE_UI_PORT="$UI_PORT" VITE_API_PROXY_TARGET="$API_URL" \
      nohup npm run dev -- --host 127.0.0.1 --port "$UI_PORT" \
      > "$log_file" 2>&1 &
    print $! > "$pid_file"
  )
  wait_for_url "$UI_URL" "$log_file"
  print "Started SiteForge UI at $UI_URL (pid $(<"$pid_file"))."
  print "  log: $log_file"
}

wait_for_url() {
  local url="$1"
  local log_file="$2"

  for _ in {1..240}; do
    if /usr/bin/curl -fsS "$url" >/dev/null 2>&1; then
      return 0
    fi
    sleep 0.5
  done

  print "Timed out waiting for $url. Log tail:" >&2
  tail -n 80 "$log_file" >&2 || true
  exit 1
}

check_postgres
start_api
start_ui

print ""
print "SiteForge services are starting:"
print "  UI:  $UI_URL"
print "  API: $API_URL"

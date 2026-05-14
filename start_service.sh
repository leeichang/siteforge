#!/usr/bin/env zsh
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
API_DIR="$ROOT_DIR/backend/SiteForge/SiteForge.Api"
UI_DIR="$ROOT_DIR/frontend/siteforge-ui"
RUN_DIR="$ROOT_DIR/artifacts/service"

UI_PORT="${SITEFORGE_UI_PORT:-5010}"
API_PORT="${SITEFORGE_API_PORT:-8000}"
UI_URL="http://127.0.0.1:$UI_PORT"
API_URL="http://127.0.0.1:$API_PORT"
DOTNET_BIN="${DOTNET_BIN:-/opt/homebrew/opt/dotnet@8/bin/dotnet}"

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

start_api() {
  local pid_file="$RUN_DIR/api.pid"
  local log_file="$RUN_DIR/api.log"

  if [[ -f "$pid_file" ]] && is_pid_alive "$(<"$pid_file")"; then
    print "SiteForge API is already running at $API_URL (pid $(<"$pid_file"))."
    return
  fi

  ensure_port_free "$API_PORT" "API"

  (
    cd "$API_DIR"
    ASPNETCORE_ENVIRONMENT="${ASPNETCORE_ENVIRONMENT:-Development}" \
      "$DOTNET_BIN" run --urls "$API_URL"
  ) > "$log_file" 2>&1 &

  print $! > "$pid_file"
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

  (
    cd "$UI_DIR"
    SITEFORGE_UI_PORT="$UI_PORT" VITE_API_PROXY_TARGET="$API_URL" npm run dev -- --host 127.0.0.1 --port "$UI_PORT"
  ) > "$log_file" 2>&1 &

  print $! > "$pid_file"
  print "Started SiteForge UI at $UI_URL (pid $(<"$pid_file"))."
  print "  log: $log_file"
}

start_api
start_ui

print ""
print "SiteForge services are starting:"
print "  UI:  $UI_URL"
print "  API: $API_URL"

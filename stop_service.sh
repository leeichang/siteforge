#!/usr/bin/env zsh
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"
RUN_DIR="$ROOT_DIR/artifacts/service"
UI_PORT="${SITEFORGE_UI_PORT:-5010}"
API_PORT="${SITEFORGE_API_PORT:-8000}"
LEGACY_UI_PORT="${SITEFORGE_LEGACY_UI_PORT:-5173}"

stop_pid_file() {
  local label="$1"
  local pid_file="$2"

  if [[ ! -f "$pid_file" ]]; then
    print "$label pid file not found."
    return
  fi

  local pid
  pid="$(<"$pid_file")"

  if [[ -n "$pid" ]] && kill -0 "$pid" >/dev/null 2>&1; then
    kill "$pid" >/dev/null 2>&1 || true
    for _ in {1..30}; do
      if ! kill -0 "$pid" >/dev/null 2>&1; then
        break
      fi
      sleep 0.2
    done
    if kill -0 "$pid" >/dev/null 2>&1; then
      kill -9 "$pid" >/dev/null 2>&1 || true
    fi
    print "Stopped $label (pid $pid)."
  else
    print "$label was not running from pid file."
  fi

  rm -f "$pid_file"
}

stop_port() {
  local label="$1"
  local port="$2"
  local pids
  pids="$(lsof -ti tcp:"$port" 2>/dev/null || true)"

  if [[ -z "$pids" ]]; then
    return
  fi

  for pid in ${(f)pids}; do
    kill "$pid" >/dev/null 2>&1 || true
  done
  print "Stopped remaining $label process(es) on port $port."
}

stop_pid_file "Legacy SiteForge UI" "$RUN_DIR/ui.pid"
stop_pid_file "SiteForge API" "$RUN_DIR/api.pid"
stop_port "UI" "$UI_PORT"
stop_port "legacy UI" "$LEGACY_UI_PORT"
stop_port "API" "$API_PORT"

print "SiteForge services stopped."

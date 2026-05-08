#!/usr/bin/env zsh
set -euo pipefail
unsetopt BG_NICE 2>/dev/null || true

ROOT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
API_DIR="$ROOT_DIR/backend/SiteForge/SiteForge.Api"
DOTNET_BIN="${DOTNET_BIN:-/opt/homebrew/opt/dotnet@8/bin/dotnet}"
CURL_BIN="${CURL_BIN:-/usr/bin/curl}"
JQ_BIN="${JQ_BIN:-/usr/bin/jq}"
CAT_BIN="${CAT_BIN:-/bin/cat}"
DATE_BIN="${DATE_BIN:-/bin/date}"
GREP_BIN="${GREP_BIN:-/usr/bin/grep}"
TAIL_BIN="${TAIL_BIN:-/usr/bin/tail}"
PORT="${SITEFORGE_E2E_PORT:-5068}"
BASE_URL="http://127.0.0.1:$PORT"
ARTIFACT_DIR="$ROOT_DIR/artifacts/e2e"
API_LOG="$ARTIFACT_DIR/api.log"

mkdir -p "$ARTIFACT_DIR"

server_pid=""
cleanup() {
  if [[ -n "$server_pid" ]]; then
    kill "$server_pid" >/dev/null 2>&1 || true
    wait "$server_pid" >/dev/null 2>&1 || true
  fi
}
trap cleanup EXIT

pass() {
  print "PASS $1${2:+ - $2}"
}

json_request() {
  local method="$1"
  local path="$2"
  local token="${3:-}"
  local body="${4:-}"
  local response_file="$ARTIFACT_DIR/response.json"
  local status_file="$ARTIFACT_DIR/status.txt"

  local curl_args=(
    -sS
    -o "$response_file"
    -w "%{http_code}"
    -X "$method"
    "$BASE_URL$path"
    -H "Content-Type: application/json"
  )
  if [[ -n "$token" ]]; then
    curl_args+=(-H "Authorization: Bearer $token")
  fi
  if [[ -n "$body" ]]; then
    curl_args+=(--data "$body")
  fi

  "$CURL_BIN" "${curl_args[@]}" > "$status_file"

  local http_status
  http_status="$("$CAT_BIN" "$status_file")"
  if [[ "$http_status" -lt 200 || "$http_status" -ge 300 ]]; then
    print "Request failed: $method $path -> $http_status" >&2
    "$CAT_BIN" "$response_file" >&2
    exit 1
  fi

  "$CAT_BIN" "$response_file"
}

wait_for_api() {
  for _ in {1..120}; do
    if "$CURL_BIN" -fsS "$BASE_URL/swagger/v1/swagger.json" >/dev/null 2>&1; then
      return 0
    fi
    sleep 1
  done

  print "Timed out waiting for SiteForge API. Log tail:" >&2
  "$TAIL_BIN" -n 120 "$API_LOG" >&2 || true
  exit 1
}

if [[ ! -f "$API_DIR/bin/Debug/net8.0/SiteForge.Api.dll" ]]; then
  "$DOTNET_BIN" build "$ROOT_DIR/backend/SiteForge/SiteForge.sln"
fi
(
  cd "$API_DIR"
  "$DOTNET_BIN" bin/Debug/net8.0/SiteForge.Api.dll --urls "$BASE_URL"
) > "$API_LOG" 2>&1 &
server_pid="$!"

wait_for_api
pass "API started" "$BASE_URL"

stamp="$("$DATE_BIN" +%s)"
email="e2e-$stamp@siteforge.test"
password="E2ePassw0rd!"
site_name="E2E Site $stamp"
hero_text="E2E Hero $stamp"

auth_json="$(json_request POST /api/Auth/register "" "{\"email\":\"$email\",\"password\":\"$password\",\"displayName\":\"E2E Tester\"}")"
token="$(printf '%s' "$auth_json" | "$JQ_BIN" -r '.data.token // .token')"
[[ "$token" != "null" && -n "$token" ]]
pass "registered user" "$email"

profile_json="$(json_request GET /api/Users/profile "$token")"
profile_email="$(printf '%s' "$profile_json" | "$JQ_BIN" -r '.data.email // .email')"
[[ "$profile_email" == "$email" ]]
pass "loaded authenticated profile"

ai_site_json="$(json_request POST /api/AiConversations/generate-site "$token" "{\"siteName\":\"AI Studio $stamp\",\"prompt\":\"Create a polished AI generated website for a design studio with services, trust signals, and a contact path.\",\"style\":\"tech\",\"contentLength\":\"medium\",\"pageTypes\":[\"home\",\"about\",\"services\",\"contact\"]}")"
ai_site_id="$(printf '%s' "$ai_site_json" | "$JQ_BIN" -r '.data.siteId // .siteId')"
ai_page_count="$(printf '%s' "$ai_site_json" | "$JQ_BIN" '.data.pages // .pages | length')"
ai_home_html="$(printf '%s' "$ai_site_json" | "$JQ_BIN" -r '.data.pages[0].htmlContent // .pages[0].htmlContent')"
[[ "$ai_site_id" != "null" && -n "$ai_site_id" && "$ai_page_count" -ge 4 ]]
printf '%s' "$ai_home_html" | "$GREP_BIN" -q "sf-ai-page"
pass "generated AI website" "$ai_page_count pages"

ai_page_json="$(json_request POST /api/AiConversations/generate-page "$token" "{\"siteId\":\"$ai_site_id\",\"pageName\":\"AI Landing\",\"pageType\":\"home\",\"prompt\":\"Generate a conversion-focused landing page with hero, features, and CTA.\",\"style\":\"premium\",\"contentLength\":\"concise\"}")"
ai_generated_page_id="$(printf '%s' "$ai_page_json" | "$JQ_BIN" -r '.data.pageId // .pageId')"
ai_generated_html="$(printf '%s' "$ai_page_json" | "$JQ_BIN" -r '.data.htmlContent // .htmlContent')"
[[ "$ai_generated_page_id" != "null" && -n "$ai_generated_page_id" ]]
printf '%s' "$ai_generated_html" | "$GREP_BIN" -q "AI generated website"
pass "generated AI page" "$ai_generated_page_id"

templates_json="$(json_request GET /api/WidgetTemplates)"
template_count="$(printf '%s' "$templates_json" | "$JQ_BIN" '.data // . | length')"
hero_count="$(printf '%s' "$templates_json" | "$JQ_BIN" '[.data // . | .[] | select(.name == "Hero")] | length')"
[[ "$template_count" -gt 0 && "$hero_count" -gt 0 ]]
pass "loaded seeded widget templates" "$template_count templates"

site_json="$(json_request POST /api/Sites "$token" "{\"name\":\"$site_name\",\"description\":\"Created by SiteForge E2E smoke test.\"}")"
site_id="$(printf '%s' "$site_json" | "$JQ_BIN" -r '.data.id // .id')"
[[ "$site_id" != "null" && -n "$site_id" ]]
pass "created site" "$site_id"

pages_json="$(json_request GET "/api/Pages/site/$site_id" "$token")"
home_id="$(printf '%s' "$pages_json" | "$JQ_BIN" -r '.data // . | map(select(.isHome == true))[0].id')"
page_count="$(printf '%s' "$pages_json" | "$JQ_BIN" '.data // . | length')"
[[ "$home_id" != "null" && -n "$home_id" ]]
pass "loaded site pages" "$page_count page(s)"

components="$("$JQ_BIN" -nc --arg hero "$hero_text" '[{tagName:"main",components:[{tagName:"h1",content:$hero}]}]')"
styles="$("$JQ_BIN" -nc '[{selectors:[".e2e-hero"],style:{padding:"48px"}}]')"
page_body="$("$JQ_BIN" -nc \
  --arg hero "$hero_text" \
  --arg components "$components" \
  --arg styles "$styles" \
  '{
    htmlContent: ("<main><section class=\"e2e-hero\"><h1>" + $hero + "</h1><p>Published from E2E.</p></section></main>"),
    cssContent: ".e2e-hero{padding:48px;color:#111827;background:#f8fafc}",
    jsContent: "window.siteforgeE2E=true;",
    components: $components,
    styles: $styles
  }')"
updated_page_json="$(json_request PUT "/api/Pages/$home_id" "$token" "$page_body")"
updated_id="$(printf '%s' "$updated_page_json" | "$JQ_BIN" -r '.data.id // .id')"
[[ "$updated_id" == "$home_id" ]]
pass "saved GrapesJS page payload"

about_slug="about-e2e-$stamp"
about_json="$(json_request POST "/api/Pages/site/$site_id" "$token" "{\"title\":\"About E2E\",\"slug\":\"$about_slug\",\"pageType\":\"custom\",\"isHome\":false}")"
about_id="$(printf '%s' "$about_json" | "$JQ_BIN" -r '.data.id // .id')"
[[ "$about_id" != "null" && -n "$about_id" ]]
pass "created secondary page" "$about_slug"

publish_json="$(json_request POST "/api/Sites/$site_id/publish" "$token" '{"taskType":"full_publish","targetUrl":""}')"
publish_status="$(printf '%s' "$publish_json" | "$JQ_BIN" -r '.data.status // .status')"
published_pages="$(printf '%s' "$publish_json" | "$JQ_BIN" -r '.data.publishedPages // .publishedPages')"
[[ "$publish_status" == "done" && "$published_pages" -ge 2 ]]
pass "published site" "$published_pages pages"

published_site_json="$(json_request GET "/api/Sites/$site_id" "$token")"
site_status="$(printf '%s' "$published_site_json" | "$JQ_BIN" -r '.data.status // .status')"
published_url="$(printf '%s' "$published_site_json" | "$JQ_BIN" -r '.data.publishedUrl // .publishedUrl')"
[[ "$site_status" == "published" && "$published_url" != "null" && -n "$published_url" ]]
pass "verified published site status" "$published_url"

published_html="$("$CURL_BIN" -fsS "$BASE_URL${published_url}index.html")"
printf '%s' "$published_html" | "$GREP_BIN" -q "$hero_text"
pass "verified published home HTML"

published_file="$API_DIR/wwwroot$published_url/index.html"
"$GREP_BIN" -q "$hero_text" "$published_file"
pass "verified static file output" "$published_file"

print ""
print "E2E smoke passed: 12 checks"

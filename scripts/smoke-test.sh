#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
AUTH_SERVER_DIR="$PROJECT_ROOT/src/AuthServer"
API_DIR="$PROJECT_ROOT/src/Api"
WEB_CLIENT_DIR="$PROJECT_ROOT/src/WebClient"
COOKIE_JAR="$(mktemp)"

AUTH_URL="https://localhost:5001"
API_URL="https://localhost:5003"
WEB_URL="https://localhost:5002"

cleanup() {
    local exit_code=$?
    trap - EXIT INT TERM
    if [ -n "${AUTH_PID:-}" ]; then kill "$AUTH_PID" 2>/dev/null || true; fi
    if [ -n "${API_PID:-}" ]; then kill "$API_PID" 2>/dev/null || true; fi
    if [ -n "${WEB_PID:-}" ]; then kill "$WEB_PID" 2>/dev/null || true; fi
    rm -f "$COOKIE_JAR"
    exit "$exit_code"
}

trap cleanup EXIT INT TERM

wait_for() {
    local name=$1
    local url=$2

    for _ in {1..45}; do
        if curl -sk --fail "$url" > /dev/null; then
            echo "$name is ready."
            return 0
        fi
        sleep 1
    done

    echo "$name did not become ready: $url" >&2
    return 1
}

extract_location() {
    awk 'BEGIN{IGNORECASE=1} /^location:/ { sub(/\r$/, "", $2); print $2; exit }'
}

extract_token() {
    sed -n 's/.*name="__RequestVerificationToken" type="hidden" value="\([^"]*\)".*/\1/p' | head -n 1
}

extract_return_url() {
    sed -n 's/.*name="ReturnUrl" value="\([^"]*\)".*/\1/p' | head -n 1
}

SKIP_RESTORE=1 SKIP_HTTPS_TRUST=1 "$SCRIPT_DIR/setup-dev.sh"

echo "Starting services..."
dotnet run --project "$AUTH_SERVER_DIR" --no-build &
AUTH_PID=$!

wait_for "AuthServer" "$AUTH_URL/.well-known/openid-configuration"

dotnet run --project "$API_DIR" --no-build &
API_PID=$!

dotnet run --project "$WEB_CLIENT_DIR" --no-build &
WEB_PID=$!

wait_for "Api" "$API_URL/swagger/index.html"
wait_for "WebClient" "$WEB_URL"

echo "Checking discovery document..."
curl -sk --fail "$AUTH_URL/.well-known/openid-configuration" | grep -q '"issuer"'

echo "Checking protected API rejects anonymous requests..."
anonymous_status="$(curl -sk -o /dev/null -w '%{http_code}' "$API_URL/identity")"
if [ "$anonymous_status" != "401" ]; then
    echo "Expected /identity to return 401 without a bearer token, got $anonymous_status." >&2
    exit 1
fi

echo "Running browser-style OIDC sign-in flow..."
secure_headers="$(mktemp)"
curl -sk -D "$secure_headers" -o /dev/null -c "$COOKIE_JAR" -b "$COOKIE_JAR" "$WEB_URL/Secure"
authorize_url="$(extract_location < "$secure_headers")"
rm -f "$secure_headers"

login_headers="$(mktemp)"
curl -sk -D "$login_headers" -o /dev/null -c "$COOKIE_JAR" -b "$COOKIE_JAR" "$authorize_url"
login_url="$(extract_location < "$login_headers")"
rm -f "$login_headers"

login_page="$(curl -sk -c "$COOKIE_JAR" -b "$COOKIE_JAR" "$login_url")"
request_token="$(printf '%s' "$login_page" | extract_token)"
return_url="$(printf '%s' "$login_page" | extract_return_url)"

login_response_headers="$(mktemp)"
curl -sk -D "$login_response_headers" -o /dev/null -c "$COOKIE_JAR" -b "$COOKIE_JAR" \
    -X POST "$AUTH_URL/Account/Login" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    --data-urlencode "UserName=alice" \
    --data-urlencode 'Password=Pass123$' \
    --data-urlencode "ReturnUrl=$return_url" \
    --data-urlencode "__RequestVerificationToken=$request_token"
return_authorize_url="$(extract_location < "$login_response_headers")"
rm -f "$login_response_headers"

callback_headers="$(mktemp)"
curl -sk -D "$callback_headers" -o /dev/null -c "$COOKIE_JAR" -b "$COOKIE_JAR" "$AUTH_URL$return_authorize_url"
callback_url="$(extract_location < "$callback_headers")"
rm -f "$callback_headers"

post_callback_headers="$(mktemp)"
curl -sk -D "$post_callback_headers" -o /dev/null -c "$COOKIE_JAR" -b "$COOKIE_JAR" "$callback_url"
rm -f "$post_callback_headers"

secure_page="$(curl -sk -c "$COOKIE_JAR" -b "$COOKIE_JAR" "$WEB_URL/Secure")"
printf '%s' "$secure_page" | grep -q "Access Token"

call_api_token="$(printf '%s' "$secure_page" | extract_token)"
api_page="$(curl -sk -c "$COOKIE_JAR" -b "$COOKIE_JAR" \
    -X POST "$WEB_URL/Secure?handler=CallApi" \
    -H "Content-Type: application/x-www-form-urlencoded" \
    --data-urlencode "__RequestVerificationToken=$call_api_token")"
printf '%s' "$api_page" | grep -q "Hello from the protected API"

echo "Smoke test passed."

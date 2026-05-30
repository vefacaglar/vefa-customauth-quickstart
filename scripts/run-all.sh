#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

AUTH_SERVER_DIR="$PROJECT_ROOT/src/AuthServer"
API_DIR="$PROJECT_ROOT/src/Api"
WEB_CLIENT_DIR="$PROJECT_ROOT/src/WebClient"

LOG_DIR="$PROJECT_ROOT/.logs"
mkdir -p "$LOG_DIR"

PIDS=()

cleanup() {
    echo ""
    echo "Stopping all services..."
    for pid in "${PIDS[@]}"; do
        if kill -0 "$pid" 2>/dev/null; then
            kill "$pid" 2>/dev/null || true
            wait "$pid" 2>/dev/null || true
        fi
    done
    echo "All services stopped."
    exit 0
}

trap cleanup SIGINT SIGTERM

echo "Building all projects..."
dotnet build "$PROJECT_ROOT" --nologo -v q

echo ""
echo "Starting AuthServer (https://localhost:5001)..."
dotnet run --project "$AUTH_SERVER_DIR" --no-build > "$LOG_DIR/authserver.log" 2>&1 &
PIDS+=($!)

echo "Waiting for AuthServer to be ready..."
for i in {1..30}; do
    if curl -sk https://localhost:5001/.well-known/openid-configuration > /dev/null 2>&1; then
        echo "AuthServer is ready."
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo "AuthServer failed to start. Check $LOG_DIR/authserver.log"
        cleanup
    fi
    sleep 1
done

echo "Starting Api (https://localhost:5003)..."
dotnet run --project "$API_DIR" --no-build > "$LOG_DIR/api.log" 2>&1 &
PIDS+=($!)

echo "Starting WebClient (https://localhost:5002)..."
dotnet run --project "$WEB_CLIENT_DIR" --no-build > "$LOG_DIR/webclient.log" 2>&1 &
PIDS+=($!)

echo ""
echo "All services started:"
echo "  AuthServer: https://localhost:5001"
echo "  WebClient:  https://localhost:5002"
echo "  Api:        https://localhost:5003"
echo ""
echo "Logs:"
echo "  $LOG_DIR/authserver.log"
echo "  $LOG_DIR/webclient.log"
echo "  $LOG_DIR/api.log"
echo ""
echo "Press Ctrl+C to stop all services."

wait

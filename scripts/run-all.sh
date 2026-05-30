#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"

AUTH_SERVER_DIR="$PROJECT_ROOT/src/AuthServer"
API_DIR="$PROJECT_ROOT/src/Api"
WEB_CLIENT_DIR="$PROJECT_ROOT/src/WebClient"

cleanup() {
    echo ""
    echo -e "\033[1;30mStopping all services...\033[0m"
    trap - SIGINT SIGTERM # Avoid recursion
    kill 0 2>/dev/null || true
    echo -e "\033[1;30mAll services stopped.\033[0m"
    exit 0
}

trap cleanup SIGINT SIGTERM

kill_port() {
    local port=$1
    local pids
    pids=$(lsof -t -i :"$port" || true)
    if [ -n "$pids" ]; then
        echo -e "\033[1;30mPort $port is in use. Releasing process(es): ${pids//$'\n'/ }...\033[0m"
        for pid in $pids; do
            kill -9 "$pid" 2>/dev/null || true
        done
    fi
}

echo -e "\033[1;30mChecking active network ports...\033[0m"
for port in 5001 5002 5003; do
    kill_port "$port"
done

echo -e "\033[1;30mBuilding all projects...\033[0m"
dotnet build "$PROJECT_ROOT" --nologo -v q

echo ""
echo -e "\033[1;30mStarting AuthServer (https://localhost:5001)...\033[0m"
dotnet run --project "$AUTH_SERVER_DIR" --no-build 2>&1 | while read -r line; do
    echo -e "\033[1;30m[auth]\033[0m $line"
done &

echo -e "\033[1;30mWaiting for AuthServer to be ready...\033[0m"
for i in {1..30}; do
    if curl -sk https://localhost:5001/.well-known/openid-configuration > /dev/null 2>&1; then
        break
    fi
    if [ "$i" -eq 30 ]; then
        echo -e "\033[1;31mAuthServer failed to start.\033[0m"
        cleanup
    fi
    sleep 1
done
echo -e "\033[1;30mAuthServer is ready.\033[0m"

echo -e "\033[1;30mStarting Api (https://localhost:5003)...\033[0m"
dotnet run --project "$API_DIR" --no-build 2>&1 | while read -r line; do
    echo -e "\033[1;34m[api]\033[0m  $line"
done &

echo -e "\033[1;30mStarting WebClient (https://localhost:5002)...\033[0m"
dotnet run --project "$WEB_CLIENT_DIR" --no-build 2>&1 | while read -r line; do
    echo -e "\033[1;36m[web]\033[0m  $line"
done &

echo ""
echo -e "\033[1;32mAll services running. Press Ctrl+C to terminate all process instances.\033[0m"
echo ""

wait

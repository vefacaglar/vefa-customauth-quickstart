#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
AUTH_SERVER_DIR="$PROJECT_ROOT/src/AuthServer"
SIGNING_CERT="$AUTH_SERVER_DIR/signing.pfx"
SIGNING_CERT_PASSWORD="${SIGNING_CERT_PASSWORD:-dev-only}"

if [ "${SKIP_RESTORE:-}" = "1" ]; then
    echo "Skipping .NET package restore."
else
    echo "Restoring .NET packages..."
    dotnet restore "$PROJECT_ROOT/VefaCustomAuth.Quickstart.slnx"
fi

if [ ! -f "$SIGNING_CERT" ]; then
    echo "Creating local signing certificate at src/AuthServer/signing.pfx..."
    dotnet dev-certs https -ep "$SIGNING_CERT" -p "$SIGNING_CERT_PASSWORD"
else
    echo "Local signing certificate already exists."
fi

if [ "${SKIP_HTTPS_TRUST:-}" = "1" ]; then
    echo "Skipping HTTPS development certificate trust."
else
    echo "Trusting the ASP.NET Core HTTPS development certificate..."
    dotnet dev-certs https --trust
fi

echo "Development setup complete."

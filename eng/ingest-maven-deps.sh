#!/bin/bash
# Ingest Maven/Gradle dependencies into the dotnet-public-maven Azure Artifacts feed.
#
# Run this locally after adding or updating Maven/Gradle dependencies in
# src/Core/AndroidNative/ to ensure they are available in the feed for CI builds.
#
# Prerequisites:
#   - JDK 17+
#   - .NET Azure Artifacts credential provider installed
#     (https://github.com/microsoft/artifacts-credprovider#installation)
#
# Usage:
#   ./eng/ingest-maven-deps.sh

set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/.." && pwd)"
ANDROID_DIR="$REPO_ROOT/src/Core/AndroidNative"
FEED_URL="https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public-maven/maven/v1"
CRED_PROVIDER="$HOME/.nuget/plugins/netcore/CredentialProvider.Microsoft/CredentialProvider.Microsoft.dll"

echo "=== Maven Dependency Ingestion for dotnet-public-maven ==="
echo ""

# Step 1: Get auth token
echo "Acquiring auth token..."
if [ ! -f "$CRED_PROVIDER" ]; then
    echo "ERROR: Azure Artifacts credential provider not found at $CRED_PROVIDER"
    echo "Install it from: https://github.com/microsoft/artifacts-credprovider#installation"
    exit 1
fi

TOKEN=$(dotnet "$CRED_PROVIDER" -U "$FEED_URL" -F Json -N true -I true 2>/dev/null \
    | python3 -c "import json,sys; print(json.load(sys.stdin).get('Password',''))" 2>/dev/null)

if [ -z "$TOKEN" ]; then
    echo "ERROR: Failed to acquire auth token. Make sure you're signed in to Azure DevOps."
    exit 1
fi
echo "Token acquired."

# Step 2: Run Gradle build with refresh to ingest via credential provider
echo ""
echo "Step 1/2: Running Gradle build with --refresh-dependencies..."
cd "$ANDROID_DIR"
./gradlew build --no-daemon --refresh-dependencies \
    -Dazure.artifacts.credprovider.nonInteractive=true \
    -Dazure.artifacts.credprovider.isRetry=true 2>&1 | tail -3 || true

# Step 3: Loop — build, find missing packages, curl-ingest them
echo ""
echo "Step 2/2: Ingesting any remaining packages via REST API..."
for i in $(seq 1 30); do
    result=$(./gradlew build --no-daemon \
        -Dazure.artifacts.credprovider.nonInteractive=true 2>&1)

    if echo "$result" | grep -q "BUILD SUCCESSFUL"; then
        echo "All dependencies ingested successfully! ✅"
        exit 0
    fi

    # Extract failed URLs and curl them with auth
    urls=$(echo "$result" | grep "Could not GET\|Could not HEAD" \
        | sed "s/.*'\(https:[^']*\)'.*/\1/" | sort -u | grep "pkgs.dev.azure.com")
    count=$(echo "$urls" | grep -c "https" 2>/dev/null || echo "0")

    if [ "$count" = "0" ]; then
        # No feed URLs failing — might be a different error
        echo "Build failed but not due to feed issues. Check build output."
        echo "$result" | grep -i "error" | grep -v "warning" | head -5
        exit 1
    fi

    echo "  Run $i: ingesting $count packages..."
    echo "$urls" | while read url; do
        [ -z "$url" ] && continue
        code=$(curl -s -o /dev/null -w "%{http_code}" -H "Authorization: Bearer $TOKEN" "$url" 2>/dev/null)
        if [ "$code" = "200" ]; then
            echo "    ✅ $(basename "$url")"
        else
            echo "    ❌ ($code) $(basename "$url")"
        fi
    done
done

echo "WARNING: Reached max iterations. Some packages may still need ingestion."
exit 1

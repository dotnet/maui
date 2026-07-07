#!/bin/bash
# Ingest Maven/Gradle dependencies into the dotnet-public-maven Azure Artifacts feed.
#
# WHY THIS IS NEEDED:
#   CI builds run under CFSClean network isolation which blocks direct access to
#   Maven Central (repo.maven.apache.org). All Maven dependencies are resolved
#   through the dotnet-public-maven Azure Artifacts feed instead. However, this
#   feed requires an authenticated request the FIRST time a package is pulled
#   from upstream Maven Central — after that, anyone can read it anonymously.
#
#   The CI pipeline's credential provider plugin (com.microsoft.azure.artifacts.
#   credprovider) skips authentication in Azure Pipelines (TF_BUILD=True), so
#   new packages MUST be pre-ingested locally before CI can use them.
#
# WHEN TO RUN:
#   After adding or updating any Maven/Gradle dependency in
#   src/Core/AndroidNative/build.gradle or settings.gradle.
#
# HOW IT WORKS:
#   1. Acquires an auth token via the .NET Azure Artifacts credential provider
#   2. Pre-ingests platform-specific artifacts (e.g. aapt2) for all OS variants
#      (macOS/Linux/Windows) since Gradle only resolves the local OS classifier
#   3. Runs the Gradle build with --refresh-dependencies to bypass local cache
#      and force actual downloads through the feed (which triggers ingestion)
#   4. For packages that Gradle's credential provider can't reach (e.g. AGP's
#      internal detachedConfiguration scopes), falls back to curl with Bearer
#      token to force-ingest the specific package URLs
#
# COMMON PITFALL:
#   Running ./gradlew build without --refresh-dependencies may appear to succeed
#   but actually resolves from ~/.gradle/caches/ (local cache from prior builds
#   that used mavenCentral() directly). This does NOT ingest into the feed.
#
# Prerequisites:
#   - JDK 17+
#   - python3 (for JSON parsing)
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

# Force Gradle to use the Azure Artifacts feed (same as CI) so that
# dependency resolution goes through the feed and triggers ingestion.
# Required because settings.gradle gates repo selection on TF_BUILD.
export TF_BUILD=True

# Step 2: Ingest platform-specific artifacts for all OS variants
# Gradle only resolves the classifier for the current OS (e.g. aapt2-osx.jar on macOS).
# CI builds on Windows/Linux need their variants pre-ingested too.
echo ""
echo "Step 1/3: Ingesting cross-platform artifacts..."
AAPT2_VERSION="8.11.1-12782657"
for classifier in osx linux windows; do
    for ext in jar pom; do
        url="$FEED_URL/com/android/tools/build/aapt2/$AAPT2_VERSION/aapt2-$AAPT2_VERSION-$classifier.$ext"
        code=$(curl -s -o /dev/null -w "%{http_code}" --oauth2-bearer "$TOKEN" "$url" 2>/dev/null)
        echo "  aapt2-$AAPT2_VERSION-$classifier.$ext: $code"
    done
done

# Step 3: Run Gradle build with refresh to ingest via credential provider
echo ""
echo "Step 2/3: Running Gradle build with --refresh-dependencies..."
cd "$ANDROID_DIR"
if ! ./gradlew build --no-daemon --refresh-dependencies \
    -Dazure.artifacts.credprovider.nonInteractive=true \
    -Dazure.artifacts.credprovider.isRetry=true 2>&1 | tail -20; then
    echo "WARNING: Initial Gradle build failed (expected if packages need ingestion). Continuing..."
fi

# Step 4: Loop — build, find missing packages, curl-ingest them
echo ""
echo "Step 3/3: Ingesting any remaining packages via REST API..."
for i in $(seq 1 30); do
    result=$(./gradlew build --no-daemon \
        -Dazure.artifacts.credprovider.nonInteractive=true 2>&1 || true)

    if echo "$result" | grep -q "BUILD SUCCESSFUL"; then
        echo "All dependencies ingested successfully! ✅"
        exit 0
    fi

    # Extract failed URLs and curl them with auth
    urls=$(echo "$result" | grep "Could not GET\|Could not HEAD" \
        | sed "s/.*'\(https:[^']*\)'.*/\1/" | sort -u | grep "pkgs.dev.azure.com" || true)
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
        code=$(curl -s -o /dev/null -w "%{http_code}" --oauth2-bearer "$TOKEN" "$url" 2>/dev/null)
        if [ "$code" = "200" ]; then
            echo "    ✅ $(basename "$url")"
        else
            echo "    ❌ ($code) $(basename "$url")"
        fi
    done
done

echo "WARNING: Reached max iterations. Some packages may still need ingestion."
exit 1

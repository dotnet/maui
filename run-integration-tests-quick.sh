#!/bin/bash
# Quick version - assumes packages are already built
set -e

echo "========================================="
echo "Quick Integration Test Runner"
echo "(Skips build/pack - uses existing packages)"
echo "========================================="

echo ""
echo "Step 1: Move Packages to Artifacts Root"
echo "========================================="
mkdir -p artifacts
cp artifacts/packages/Release/Shipping/*.nupkg artifacts/ 2>/dev/null || true

echo ""
echo "Step 2: Install .NET Without Workloads"
echo "========================================="
./.dotnet/dotnet build ./src/DotNet/DotNet.csproj \
  -p:InstallWorkloadPacks=false \
  -c Release \
  -bl:./artifacts/log/install-dotnet.binlog

echo ""
echo "Step 3: Install Workloads from Packages"
echo "========================================="
./.dotnet/dotnet build ./src/DotNet/DotNet.csproj \
  -t:Install \
  -c Release \
  -bl:./artifacts/log/install-dotnet-workload.binlog

echo ""
echo "Step 4: Extract MAUI Version"
echo "========================================="
MAUI_VERSION=$(ls artifacts/Microsoft.Maui.Controls.*.nupkg 2>/dev/null | head -1 | grep -oE '[0-9]+\.[0-9]+\.[0-9]+-[a-z]+\.[a-z]+\.[0-9]+\.[0-9]+')
export MAUI_PACKAGE_VERSION="${MAUI_VERSION}"
echo "MAUI_PACKAGE_VERSION: ${MAUI_PACKAGE_VERSION}"

echo ""
echo "Step 5: Restore dotnet Tools"
echo "========================================="
./.dotnet/dotnet tool restore

echo ""
echo "Step 6: Run Integration Tests"
echo "========================================="
TEST_CATEGORY="${1:-Build}"
echo "Running tests for category: ${TEST_CATEGORY}"

mkdir -p ./artifacts/TestResults

# Set SKIP_XCODE_VERSION_CHECK if running iOS tests to bypass Xcode version validation
if [ "${TEST_CATEGORY}" == "RunOniOS" ]; then
  export SKIP_XCODE_VERSION_CHECK=true
  echo "Note: Skipping Xcode version validation for iOS tests"
fi

./.dotnet/dotnet test \
  ./src/TestUtils/src/Microsoft.Maui.IntegrationTests/Microsoft.Maui.IntegrationTests.csproj \
  -c Release \
  --filter "Category=${TEST_CATEGORY}" \
  --logger "trx;LogFileName=integration-tests-${TEST_CATEGORY}.trx" \
  --results-directory ./artifacts/TestResults \
  --logger "console;verbosity=normal"

echo ""
echo "========================================="
echo "Tests Complete!"
echo "========================================="
echo "Results: ./artifacts/TestResults/integration-tests-${TEST_CATEGORY}.trx"

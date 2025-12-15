# Running Integration Tests Locally (Arcade Pipeline Approach)

This guide shows how to run integration tests locally using the same approach as the `stage-integration-tests.yml` pipeline (without Cake scripts).

## Prerequisites

- Xcode installed (for iOS/macOS tests)
- Android SDK installed (for Android tests)
- .NET workload already provisioned in `.dotnet/`

## Step-by-Step Process

### Step 1: Build and Pack MAUI

This creates all the NuGet packages that tests will use:

```bash
# Build and pack MAUI (creates packages in artifacts/packages/Release/Shipping/)
dotnet cake --target=dotnet-pack --configuration=Release
```

**Expected Output**: `.nupkg` files in `artifacts/packages/Release/Shipping/`

### Step 2: Move Packages to Root Artifacts (Like CI Does)

The CI pipeline downloads packages to `artifacts/` root. Replicate this:

```bash
# Move packages from Shipping folder to artifacts root (like CI DownloadPipelineArtifact does)
cp artifacts/packages/Release/Shipping/*.nupkg artifacts/
```

### Step 3: Install .NET Without Workloads

This builds the local `.dotnet` SDK without workload packs initially:

```bash
# Build DotNet.csproj without workload packs
./.dotnet/dotnet build ./src/DotNet/DotNet.csproj \
  -p:InstallWorkloadPacks=false \
  -c Release \
  -bl:./artifacts/log/install-dotnet.binlog
```

**What this does**: Provisions the base .NET SDK in `.dotnet/` without installing workload packs yet.

### Step 4: Install Workloads from Local Packages

Now install the workloads using the packages we just built:

```bash
# Install workloads from the local packages
./.dotnet/dotnet build ./src/DotNet/DotNet.csproj \
  -t:Install \
  -c Release \
  -bl:./artifacts/log/install-dotnet-workload.binlog
```

**What this does**: Installs MAUI workloads from `artifacts/*.nupkg` into `.dotnet/packs/`

### Step 5: Extract MAUI Version from Packages

Get the version number from the built packages (like CI does):

```bash
# Extract version from Microsoft.Maui.Controls package
MAUI_VERSION=$(ls artifacts/Microsoft.Maui.Controls.*.nupkg 2>/dev/null | head -1 | grep -oE '[0-9]+\.[0-9]+\.[0-9]+-[a-z]+\.[a-z]+\.[0-9]+\.[0-9]+')

# Set environment variable
export MAUI_PACKAGE_VERSION="${MAUI_VERSION}"

echo "MAUI_PACKAGE_VERSION: ${MAUI_PACKAGE_VERSION}"
```

**Expected Output**: Something like `10.0.20-ci.main.25612.1`

### Step 6: Restore dotnet Tools

Restore tools like xharness:

```bash
# Restore tools (xharness, etc.)
./.dotnet/dotnet tool restore
```

### Step 7: Run Integration Tests

Now run the tests with the locally built SDK and workloads:

```bash
# Run specific test category
./.dotnet/dotnet test \
  ./src/TestUtils/src/Microsoft.Maui.IntegrationTests/Microsoft.Maui.IntegrationTests.csproj \
  -c Release \
  --filter "Category=Build" \
  --logger "trx;LogFileName=integration-tests-Build.trx" \
  --results-directory ./artifacts/TestResults \
  --logger "console;verbosity=normal"
```

**Available Test Categories**:
- `Build` - Basic template build tests
- `WindowsTemplates` - Windows-specific tests
- `macOSTemplates` - macOS-specific tests
- `Blazor` - Blazor hybrid tests
- `MultiProject` - Multi-project template tests
- `AOT` - Native AOT tests
- `RunOnAndroid` - Android emulator tests
- `RunOniOS` - iOS simulator tests
- `Samples` - Sample project tests

### Step 8 (Optional): Run iOS Tests with Specific Device

For iOS tests, you can specify the device:

```bash
# Set iOS device environment variable (optional)
export IOS_TEST_DEVICE="ios-simulator-64_18.5"

# Run iOS integration tests
./.dotnet/dotnet test \
  ./src/TestUtils/src/Microsoft.Maui.IntegrationTests/Microsoft.Maui.IntegrationTests.csproj \
  -c Release \
  --filter "Category=RunOniOS" \
  --logger "trx;LogFileName=integration-tests-iOS.trx" \
  --results-directory ./artifacts/TestResults \
  --logger "console;verbosity=normal"
```

## Complete Script (All-in-One)

Here's a complete bash script that does everything:

```bash
#!/bin/bash
set -e

echo "========================================="
echo "Step 1: Build and Pack MAUI"
echo "========================================="
dotnet cake --target=dotnet-pack --configuration=Release

echo ""
echo "========================================="
echo "Step 2: Move Packages to Artifacts Root"
echo "========================================="
mkdir -p artifacts
cp artifacts/packages/Release/Shipping/*.nupkg artifacts/ 2>/dev/null || true

echo ""
echo "========================================="
echo "Step 3: Install .NET Without Workloads"
echo "========================================="
./.dotnet/dotnet build ./src/DotNet/DotNet.csproj \
  -p:InstallWorkloadPacks=false \
  -c Release \
  -bl:./artifacts/log/install-dotnet.binlog

echo ""
echo "========================================="
echo "Step 4: Install Workloads from Packages"
echo "========================================="
./.dotnet/dotnet build ./src/DotNet/DotNet.csproj \
  -t:Install \
  -c Release \
  -bl:./artifacts/log/install-dotnet-workload.binlog

echo ""
echo "========================================="
echo "Step 5: Extract MAUI Version"
echo "========================================="
MAUI_VERSION=$(ls artifacts/Microsoft.Maui.Controls.*.nupkg 2>/dev/null | head -1 | grep -oE '[0-9]+\.[0-9]+\.[0-9]+-[a-z]+\.[a-z]+\.[0-9]+\.[0-9]+')
export MAUI_PACKAGE_VERSION="${MAUI_VERSION}"
echo "MAUI_PACKAGE_VERSION: ${MAUI_PACKAGE_VERSION}"

echo ""
echo "========================================="
echo "Step 6: Restore dotnet Tools"
echo "========================================="
./.dotnet/dotnet tool restore

echo ""
echo "========================================="
echo "Step 7: Run Integration Tests"
echo "========================================="
TEST_CATEGORY="${1:-Build}"
echo "Running tests for category: ${TEST_CATEGORY}"

mkdir -p ./artifacts/TestResults

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
```

**Usage**:

```bash
# Make script executable
chmod +x run-integration-tests.sh

# Run Build tests (default)
./run-integration-tests.sh

# Run iOS tests
./run-integration-tests.sh RunOniOS

# Run Android tests
./run-integration-tests.sh RunOnAndroid

# Run Blazor tests
./run-integration-tests.sh Blazor
```

## PowerShell Version (Windows)

```powershell
# run-integration-tests.ps1

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 1: Build and Pack MAUI" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
dotnet cake --target=dotnet-pack --configuration=Release

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 2: Move Packages to Artifacts Root" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
New-Item -ItemType Directory -Force -Path artifacts | Out-Null
Copy-Item artifacts\packages\Release\Shipping\*.nupkg artifacts\ -Force -ErrorAction SilentlyContinue

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 3: Install .NET Without Workloads" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
.\.dotnet\dotnet.exe build .\src\DotNet\DotNet.csproj `
  -p:InstallWorkloadPacks=false `
  -c Release `
  -bl:.\artifacts\log\install-dotnet.binlog

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 4: Install Workloads from Packages" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
.\.dotnet\dotnet.exe build .\src\DotNet\DotNet.csproj `
  -t:Install `
  -c Release `
  -bl:.\artifacts\log\install-dotnet-workload.binlog

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 5: Extract MAUI Version" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
$nupkg = Get-ChildItem artifacts\Microsoft.Maui.Controls.*.nupkg | Select-Object -First 1
$version = [regex]::Match($nupkg.Name, '(\d+\.\d+\.\d+-[a-z]+\.[a-z]+\.\d+\.\d+)').Groups[1].Value
$env:MAUI_PACKAGE_VERSION = $version
Write-Host "MAUI_PACKAGE_VERSION: $version"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 6: Restore dotnet Tools" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
.\.dotnet\dotnet.exe tool restore

Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Step 7: Run Integration Tests" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
$testCategory = if ($args.Count -gt 0) { $args[0] } else { "Build" }
Write-Host "Running tests for category: $testCategory"

New-Item -ItemType Directory -Force -Path .\artifacts\TestResults | Out-Null

.\.dotnet\dotnet.exe test `
  .\src\TestUtils\src\Microsoft.Maui.IntegrationTests\Microsoft.Maui.IntegrationTests.csproj `
  -c Release `
  --filter "Category=$testCategory" `
  --logger "trx;LogFileName=integration-tests-$testCategory.trx" `
  --results-directory .\artifacts\TestResults `
  --logger "console;verbosity=normal"

Write-Host ""
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Tests Complete!" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host "Results: .\artifacts\TestResults\integration-tests-$testCategory.trx"
```

**Usage**:

```powershell
# Run Build tests (default)
.\run-integration-tests.ps1

# Run specific category
.\run-integration-tests.ps1 WindowsTemplates
.\run-integration-tests.ps1 Blazor
```

## Troubleshooting

### Issue: "Unable to find package Microsoft.Maui.Core"

**Solution**: Ensure Step 2 copied packages to `artifacts/` root and Step 4 completed successfully.

```bash
# Verify packages exist
ls -la artifacts/*.nupkg

# Verify workloads installed
./.dotnet/dotnet workload list
```

### Issue: "MAUI_PACKAGE_VERSION was not set"

**Solution**: Manually set the environment variable:

```bash
# Find the version
ls artifacts/Microsoft.Maui.Controls.*.nupkg

# Set manually
export MAUI_PACKAGE_VERSION="10.0.20-ci.main.25612.1"
```

### Issue: Tests can't find simulators/emulators

**Solution**: For device tests, ensure simulators/emulators are available:

```bash
# iOS: List simulators
xcrun simctl list devices available

# Android: List emulators
emulator -list-avds
```

## Key Differences from Cake Approach

| Aspect | Arcade Approach (This Guide) | Cake Approach |
|--------|------------------------------|---------------|
| Build Step | `dotnet cake --target=dotnet-pack` | Same |
| Package Location | `artifacts/*.nupkg` (root) | `artifacts/**/*.nupkg` (nested) |
| SDK Install | `dotnet build DotNet.csproj -p:InstallWorkloadPacks=false` | `dotnet cake --target=dotnet-local-workloads` |
| Workload Install | `dotnet build DotNet.csproj -t:Install` | Included in `dotnet-local-workloads` |
| Run Tests | `dotnet test` directly | `dotnet cake --target=dotnet-integration-test` |
| Complexity | ✅ More explicit, fewer abstractions | ❌ Wrapped in Cake tasks |

## Summary

The Arcade pipeline approach is more explicit and easier to understand. Each step does exactly one thing, making it easier to debug when something goes wrong. This guide replicates that exact flow locally.

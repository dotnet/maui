---
name: run-device-tests
description: "Build and run .NET MAUI device tests locally on iOS simulators using xharness."
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires xharness CLI (global dotnet tool), Xcode with iOS simulators, and .NET SDK with iOS workloads.
---

# Run Device Tests Skill

Build and run .NET MAUI device tests locally on iOS simulators.

## Tools Required

This skill uses `bash` together with `pwsh` (PowerShell 7+) to run the PowerShell scripts. Requires:
- `xharness` - Global dotnet tool for running iOS tests
- `dotnet` - .NET SDK with iOS workloads installed
- Xcode with iOS simulators

## When to Use

- User wants to run device tests locally
- User wants to verify iOS compatibility
- User wants to test on a specific iOS version (e.g., iOS 26)
- User asks "run device tests for Controls/Core/Essentials/Graphics"
- User asks "test on iOS simulator"

## Available Test Projects

| Project | Path |
|---------|------|
| Controls | `src/Controls/tests/DeviceTests/Controls.DeviceTests.csproj` |
| Core | `src/Core/tests/DeviceTests/Core.DeviceTests.csproj` |
| Essentials | `src/Essentials/test/DeviceTests/Essentials.DeviceTests.csproj` |
| Graphics | `src/Graphics/tests/DeviceTests/Graphics.DeviceTests.csproj` |
| BlazorWebView | `src/BlazorWebView/tests/DeviceTests/MauiBlazorWebView.DeviceTests.csproj` |

## Scripts

All scripts are in `.github/skills/run-device-tests/scripts/`

### 1. Run Device Tests (Full Workflow)

```bash
# Run Controls device tests on default iOS simulator
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls

# Run Core device tests on iOS 26
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -iOSVersion 26

# Run with test filter
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -TestFilter "Category=Button"

# Run specific test projects
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Essentials
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Graphics
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project BlazorWebView
```

### 2. Build Only (No Test Run)

```bash
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -BuildOnly
```

### 3. List Available Simulators

```bash
pwsh .github/skills/run-device-tests/scripts/List-Simulators.ps1

# Filter by iOS version
pwsh .github/skills/run-device-tests/scripts/List-Simulators.ps1 -iOSVersion 26
```

## Workflow

1. **List simulators** (optional): `scripts/List-Simulators.ps1` to see available iOS versions
2. **Run tests**: `scripts/Run-DeviceTests.ps1 -Project <name>` builds and runs tests
3. **Check results**: Look at the log output or `artifacts/log/` directory

## Output

- Build artifacts: `artifacts/bin/<Project>.DeviceTests/Release/net10.0-ios/iossimulator-arm64/`
- Test logs: `artifacts/log/`
- Test results summary is printed to console

## Prerequisites

- `xharness` global tool: `dotnet tool install --global Microsoft.DotNet.XHarness.CLI`
- .NET SDK with iOS workloads
- Xcode with iOS simulators installed
- For iOS 26: macOS Tahoe (26) with Xcode 26

## Examples

```bash
# Quick test run for Controls on default simulator
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls

# Test on iOS 26 specifically
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -iOSVersion 26

# Run only Button category tests
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -TestFilter "Category=Button"

# Build without running tests
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -BuildOnly
```

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

## Dependencies

This skill uses shared infrastructure scripts:
- `.github/scripts/shared/Start-Emulator.ps1` - Detects and boots iOS simulators
- `.github/scripts/shared/shared-utils.ps1` - Common utility functions

These are automatically loaded by the Run-DeviceTests.ps1 script.

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

### Run Device Tests (Full Workflow)

```bash
# Run Controls device tests on default iOS simulator (iPhone Xs with iOS 18.5)
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

### Build Only (No Test Run)

```bash
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -BuildOnly
```

### List Available Simulators

```bash
# Use xcrun simctl directly to see available simulators
xcrun simctl list devices available

# Or use the shared Start-Emulator.ps1 which auto-detects best simulator
# (iPhone Xs with iOS 18.5 by default)
```

## Workflow

1. **Run tests**: `scripts/Run-DeviceTests.ps1 -Project <name>` automatically detects/boots simulator, builds, and runs tests
2. **Check results**: Look at the console output or `artifacts/log/` directory for detailed test results

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
# Quick test run for Controls on default simulator (iPhone Xs with iOS 18.5)
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls

# Test on iOS 26 specifically
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -iOSVersion 26

# Run only Button category tests
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -TestFilter "Category=Button"

# Build without running tests
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -BuildOnly
```

## Notes

- The script automatically detects and boots an iOS simulator using the shared Start-Emulator.ps1 infrastructure
- Default simulator is iPhone Xs with iOS 18.5 (same as UI tests)
- Simulator selection and boot logic is handled by `.github/scripts/shared/Start-Emulator.ps1`
- xharness manages test execution and reporting

## XHarness Device Detection

The script automatically handles XHarness device targeting:

1. **Simulator Boot**: Start-Emulator.ps1 detects and boots appropriate iOS simulator
2. **UDID Extraction**: Script extracts simulator UDID from Start-Emulator.ps1 output
3. **iOS Version Detection**: Script queries `xcrun simctl` to get iOS version from booted simulator
4. **XHarness Targeting**: Passes both `--target ios-simulator-64_VERSION` and `--device UDID` to xharness for explicit targeting

**Why both --target and --device?**
- XHarness requires `--target ios-simulator-64` (or `ios-simulator-64_VERSION`) to specify platform type
- Adding `--device UDID` explicitly tells xharness which simulator to use
- This combination ensures reliable device selection even on ARM64 Macs where automatic detection can fail

**Example xharness invocation:**
```bash
dotnet xharness apple test \
  --app path/to/app \
  --target ios-simulator-64_18.5 \
  --device 56AE278D-60F7-4892-9DE0-6341357CA068 \
  -o artifacts/log \
  --timeout 01:00:00 \
  -v \
  --set-env=TestFilter=Category=Label
```

This ensures tests run on the correct simulator with proper version targeting.

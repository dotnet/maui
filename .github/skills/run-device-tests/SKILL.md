---
name: run-device-tests
description: "Build and run .NET MAUI device tests locally. Supports iOS, MacCatalyst, Android on macOS; Android, Windows on Windows."
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires xharness CLI (for iOS/MacCatalyst/Android), Xcode (for Apple platforms), Android SDK (for Android), and .NET SDK with platform workloads.
---

# Run Device Tests Skill

Build and run .NET MAUI device tests locally on iOS simulators, MacCatalyst, Android emulators, or Windows.

## Platform Support

| Host OS | Supported Platforms |
|---------|---------------------|
| macOS   | ios, maccatalyst, android |
| Windows | android, windows |

## Tools Required

This skill uses `bash` together with `pwsh` (PowerShell 7+) to run the PowerShell scripts. Requires:
- `xharness` - Global dotnet tool for running tests on iOS/MacCatalyst/Android
- `dotnet` - .NET SDK with platform workloads installed
- **iOS/MacCatalyst**: Xcode with iOS simulators
- **Android**: Android SDK with emulator
- **Windows**: Windows SDK

## Dependencies

This skill uses shared infrastructure scripts:
- `.github/scripts/shared/Start-Emulator.ps1` - Detects and boots iOS simulators / Android emulators
- `.github/scripts/shared/shared-utils.ps1` - Common utility functions

These are automatically loaded by the Run-DeviceTests.ps1 script.

## When to Use

- User wants to run device tests locally
- User wants to verify iOS/MacCatalyst/Android/Windows compatibility
- User wants to test on a specific iOS version (e.g., iOS 26)
- User asks "run device tests for Controls/Core/Essentials/Graphics"
- User asks "test on iOS simulator" or "test on Android emulator"
- User asks "run device tests on MacCatalyst"

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
# Run Controls device tests on iOS simulator (default on macOS)
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios

# Run Core device tests on MacCatalyst
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform maccatalyst

# Run Controls device tests on Android emulator
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform android

# Run Controls device tests on Windows (default on Windows)
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform windows

# Run on specific iOS version
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform ios -iOSVersion 26

# Run with test filter
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios -TestFilter "Category=Button"

# Run other test projects
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Essentials -Platform android
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Graphics -Platform maccatalyst
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project BlazorWebView -Platform ios
```

### Build Only (No Test Run)

```bash
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios -BuildOnly
```

### List Available Simulators/Emulators

```bash
# iOS simulators
xcrun simctl list devices available

# Android emulators
emulator -list-avds
```

## Workflow

1. **Run tests**: `scripts/Run-DeviceTests.ps1 -Project <name> -Platform <platform>` automatically detects/boots device, builds, and runs tests
2. **Check results**: Look at the console output or `artifacts/log/` directory for detailed test results

## Output

- Build artifacts: `artifacts/bin/<Project>.DeviceTests/<Configuration>/<tfm>/<rid>/`
- Test logs: `artifacts/log/`
- Test results summary is printed to console

## Prerequisites

- `xharness` global tool: `dotnet tool install --global Microsoft.DotNet.XHarness.CLI`
- .NET SDK with platform workloads
- **iOS/MacCatalyst**: Xcode with simulators installed
- **Android**: Android SDK with emulator configured
- **Windows**: Windows SDK
- For iOS 26: macOS Tahoe (26) with Xcode 26

## Examples

```bash
# Quick test run for Controls on iOS (default on macOS)
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios

# Test on MacCatalyst
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform maccatalyst

# Test on Android emulator (works on both macOS and Windows)
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform android

# Test on Windows (default on Windows)
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform windows

# Test on iOS 26 specifically
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios -iOSVersion 26

# Run only Button category tests on iOS
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios -TestFilter "Category=Button"

# Build for Android without running tests
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform android -BuildOnly
```

## Notes

- The script automatically detects and boots an iOS simulator / Android emulator using the shared Start-Emulator.ps1 infrastructure
- Default iOS simulator is iPhone Xs with iOS 18.5 (same as UI tests)
- Default Android emulator priority: API 30 Nexus > API 30 > Nexus > First available
- MacCatalyst runs directly on the Mac (no simulator needed)
- Windows tests run directly on the local machine
- Simulator/emulator selection and boot logic is handled by `.github/scripts/shared/Start-Emulator.ps1`
- xharness manages test execution and reporting for iOS/MacCatalyst/Android
- Windows uses vstest for test execution

## XHarness Device Detection

The script automatically handles XHarness device targeting for iOS and Android:

### iOS
1. **Simulator Boot**: Start-Emulator.ps1 detects and boots appropriate iOS simulator
2. **UDID Extraction**: Script extracts simulator UDID from Start-Emulator.ps1 output
3. **iOS Version Detection**: Script queries `xcrun simctl` to get iOS version from booted simulator
4. **XHarness Targeting**: Passes both `--target ios-simulator-64_VERSION` and `--device UDID` to xharness for explicit targeting

### MacCatalyst
- Runs directly on the Mac, no device targeting needed
- XHarness uses `--target maccatalyst`

### Android
1. **Emulator Boot**: Start-Emulator.ps1 detects running device or boots emulator
2. **Device ID Extraction**: Script gets device ID (e.g., `emulator-5554`)
3. **XHarness Targeting**: Passes `--device-id` to xharness android command

### Windows
- No device/emulator needed
- Uses vstest (`dotnet test`) for test execution

**Why both --target and --device for iOS?**
- XHarness requires `--target ios-simulator-64` (or `ios-simulator-64_VERSION`) to specify platform type
- Adding `--device UDID` explicitly tells xharness which simulator to use
- This combination ensures reliable device selection even on ARM64 Macs where automatic detection can fail

**Example xharness invocations:**
```bash
# iOS
dotnet xharness apple test \
  --app path/to/app \
  --target ios-simulator-64_18.5 \
  --device 56AE278D-60F7-4892-9DE0-6341357CA068 \
  -o artifacts/log \
  --timeout 01:00:00 \
  -v \
  --set-env=TestFilter=Category=Label

# MacCatalyst
dotnet xharness apple test \
  --app path/to/app \
  --target maccatalyst \
  -o artifacts/log \
  --timeout 01:00:00 \
  -v

# Android
dotnet xharness android test \
  --app path/to/app.apk \
  --package-name com.example.app \
  --device-id emulator-5554 \
  -o artifacts/log \
  --timeout 01:00:00 \
  -v
```

This ensures tests run on the correct device with proper version targeting.

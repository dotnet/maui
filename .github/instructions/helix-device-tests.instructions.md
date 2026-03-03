---
applyTo: "eng/helix_xharness.proj,eng/pipelines/**/device-tests*.yml,eng/pipelines/**/stage-device-tests.yml,src/**/DeviceTests/**"
description: "Guidelines for running and configuring .NET MAUI device tests on Helix infrastructure"
---

# Helix Device Tests Guidelines

This document provides guidance for working with .NET MAUI device tests that run on Helix infrastructure using XHarness.

## Overview

.NET MAUI uses [.NET Engineering Services Helix](https://helix.dot.net) with XHarness to run device tests across multiple platforms in parallel. This provides cloud-based device testing infrastructure.

### Device Test Projects

- `Controls.DeviceTests` - UI control tests
- `Core.DeviceTests` - Core framework tests
- `Graphics.DeviceTests` - Graphics and drawing tests
- `Essentials.DeviceTests` - Platform API tests
- `MauiBlazorWebView.DeviceTests` - Blazor WebView tests

### Available Helix Queues

Current configuration uses:
- **iOS**: `osx.15.arm64.maui.open`
- **Mac Catalyst**: `osx.15.arm64.maui.open`
- **Android**: `ubuntu.2204.amd64.android.33.open`

Check available queues at [helix.dot.net](https://helix.dot.net).

## Key Configuration Files

| File | Purpose |
|------|---------|
| `eng/helix_xharness.proj` | Main Helix configuration - defines scenarios, queues, and work items |
| `eng/pipelines/common/stage-device-tests.yml` | Pipeline template for device tests |
| `eng/test-configuration.json` | Test retry configuration |

## iOS Category Splitting

For iOS, Controls.DeviceTests heavy categories are split into separate Helix work items. This mirrors the old cake-based approach and enables parallel execution for the slowest tests.

**How it works:**
1. Heavy categories are defined in `ControlsTestCategoriesToSkipForRestOfTests` property in `helix_xharness.proj`
2. The `ControlsTestCategoriesToRunIndividually` ItemGroup is populated from that property
3. Each heavy category becomes a separate Helix work item
4. All other Controls tests run together in a single "General" work item
5. XHarness passes `--set-env="TestFilter=Category=X"` for individual categories
6. XHarness passes `--set-env="TestFilter=SkipCategories=X;Y;Z"` for the "General" work item
7. Core.DeviceTests runs as a single work item (no splitting)

**Heavy categories that run separately:**
- CollectionView, Shell, HybridWebView

**Keep in sync:** If adding new heavy categories, update the `ControlsTestCategoriesToSkipForRestOfTests` property in `eng/helix_xharness.proj`.

## Running Device Tests Locally

### Prerequisites

From the repository root:

```bash
# 1. Restore dotnet tools
dotnet tool restore

# 2. Build MSBuild tasks (required)
./build.sh -restore -build -configuration Release -projects $(PWD)/Microsoft.Maui.BuildTasks.slnf /bl:BuildBuildTasks.binlog -warnAsError false

# 3. Build device tests
./build.sh -restore -build -configuration Release /p:BuildDeviceTests=true /bl:BuildDeviceTests.binlog -warnAsError false
```

### Using the run-device-tests Skill (Recommended)

The easiest way to run device tests locally is using the `run-device-tests` skill:

```bash
# Run Controls tests on iOS simulator
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios

# Run only Button category tests
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform ios -TestFilter "Category=Button"

# Run on Android emulator
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Controls -Platform android -TestFilter "Category=Button"

# Run on MacCatalyst
pwsh .github/skills/run-device-tests/scripts/Run-DeviceTests.ps1 -Project Core -Platform maccatalyst
```

See `.github/skills/run-device-tests/SKILL.md` for full documentation.

### Submit to Helix

Set required environment variables:

```bash
export BUILD_REASON=pr
export BUILD_REPOSITORY_NAME=maui
export BUILD_SOURCEBRANCH=main
export SYSTEM_TEAMPROJECT=dnceng
export SYSTEM_ACCESSTOKEN=''
```

Submit tests:

```bash
# Android
./eng/common/msbuild.sh ./eng/helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=android /bl:sendhelix_android.binlog

# iOS
./eng/common/msbuild.sh ./eng/helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=ios /bl:sendhelix_ios.binlog

# Mac Catalyst
./eng/common/msbuild.sh ./eng/helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=maccatalyst /bl:sendhelix_catalyst.binlog
```

### Windows Commands

```cmd
set BUILD_REASON=pr
set BUILD_REPOSITORY_NAME=maui
set BUILD_SOURCEBRANCH=main
set SYSTEM_TEAMPROJECT=dnceng
set SYSTEM_ACCESSTOKEN=

.\build.cmd -restore -build -configuration Release -projects ".\Microsoft.Maui.BuildTasks.slnf" /bl:BuildBuildTasks.binlog -warnAsError false
.\build.cmd -restore -build -configuration Release /p:BuildDeviceTests=true /bl:BuildDeviceTests.binlog -warnAsError false
.\eng\common\msbuild.cmd .\eng\helix_xharness.proj /restore /p:TreatWarningsAsErrors=false /t:Test /p:TargetOS=android /bl:sendhelix.binlog
```

### Validate MSBuild Logic Only

To validate the helix proj without submitting (requires built artifacts):

```bash
dotnet msbuild eng/helix_xharness.proj /t:DiscoverTestBundles /p:TargetOS=ios /p:_MauiDotNetTfm=net10.0 /p:RepoRoot=$(pwd)/ -v:n
```

## Test Filtering Implementation

Test category filtering is implemented in `src/Core/tests/DeviceTests.Shared/DeviceTestSharedHelpers.cs`. The `GetExcludedTestCategories()` method reads the `TestFilter` value and converts it to a list of categories to skip.

### Filter Syntax

| Format | Description | Example |
|--------|-------------|---------|
| `Category=X` | Run only category X (skip all others) | `Category=Button` |
| `SkipCategories=X,Y,Z` | Skip specific categories | `SkipCategories=Shell,CollectionView` |

### Platform-Specific Filter Passing

| Platform | XHarness Argument | How App Reads It |
|----------|-------------------|------------------|
| **iOS/MacCatalyst** | `--set-env=TestFilter=...` | `NSProcessInfo.ProcessInfo.Environment["TestFilter"]` |
| **Android** | `--arg TestFilter=...` | `MauiTestInstrumentation.Current.Arguments.GetString("TestFilter")` |
| **Windows** | `--filter "Category=..."` | Native vstest filter |

**Important**: iOS uses `--set-env` (environment variable), while Android uses `--arg` (instrumentation argument). These are NOT interchangeable.

### Example XHarness Commands with Filters

```bash
# iOS - uses --set-env
xharness apple test --target ios-simulator-64_18.5 --device UDID --set-env=TestFilter=Category=Button ...

# Android - uses --arg
xharness android test --package-name com.microsoft.maui.controls.devicetests --arg TestFilter=Category=Button ...
```

## Configuration Details

The `eng/helix_xharness.proj` configuration includes:

- **Timeouts**: 2-hour work item timeout, 1-hour 15-min test timeout for category splits
- **Test Discovery**: Automatically discovers test bundles for each scenario
- **Platform Targeting**: Uses `TargetOS` property (ios, maccatalyst, android)
- **Queue Selection**: Platform-appropriate Helix queues
- **XHarness Integration**: Uses XHarness CLI for device orchestration

### CustomCommands for Category Filtering

When using category splitting, `CustomCommands` metadata overrides the default xharness invocation:

```xml
<CustomCommands>xharness apple test --target "$target" --app "$app" --output-directory "$output_directory" --timeout "$timeout" --launch-timeout "$launch_timeout" --set-env="TestFilter=Category=CategoryName"</CustomCommands>
```

**Important**: Keep CustomCommands as a single line. Multi-line commands with `set -ex` can cause parse errors.

## Troubleshooting

### Common Issues

1. **Build failures**: Ensure MSBuild tasks are built first
2. **Missing devices**: Check queue availability at [helix.dot.net](https://helix.dot.net)
3. **Authentication**: For CI, ensure proper Azure DevOps access tokens
4. **Timeouts**: Adjust `TestTimeout` and `WorkItemTimeout` for complex scenarios
5. **CustomCommands parse errors**: Keep commands on single line, avoid shell constructs like `set -ex`

### Logging and Diagnostics

- Use `/bl:filename.binlog` for detailed MSBuild logs
- Add `-verbosity:diag` for maximum diagnostic output
- Check Helix job results at the URL provided after submission
- Individual work item logs available at `https://helix.dot.net/api/2019-06-17/jobs/{jobId}/workitems/{workItemName}/console`

## Additional Resources

- [XHarness on Helix Documentation](https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Helix/Sdk/tools/xharness-runner/Readme.md)
- [Helix SDK Documentation](https://github.com/dotnet/arcade/blob/main/src/Microsoft.DotNet.Helix/Sdk/Readme.md)
- [Example Helix Run](https://dev.azure.com/dnceng-public/public/_build/results?buildId=1115383&view=results)

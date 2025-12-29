---
description: "Guidance for GitHub Copilot when working with .NET MAUI integration tests"
applyTo: "src/TestUtils/src/Microsoft.Maui.IntegrationTests/**"
---

# .NET MAUI Integration Tests Guidelines

Integration tests validate end-to-end functionality by creating, building, and running .NET MAUI projects using templates and the local workload.

## Test Framework

These integration tests use **NUnit** as the test framework.

## CI Infrastructure

All integration tests run on **Azure DevOps agents** via the stage template `eng/pipelines/arcade/stage-integration-tests.yml`.

- **Windows tests**: Run on Windows 1ES pools
- **macOS tests**: Run on Azure Pipelines hosted images
  - General macOS tests: `macOS-15` (ARM64/Apple Silicon)
  - `RunOnAndroid`: `macOS-15` (via MacOSPool)
  - `RunOniOS_*`: Each iOS test runs in **two separate lanes**:
    - **ARM64 lane** (`MacOSPoolArm64`): `macOS-15` with Apple Silicon
    - **MacOSPool lane** (`MacOSPool`): For comparison testing

### Test Retry

Integration tests are configured with **automatic retry on failure** (`retryCountOnTaskFailure: 1`). If a test fails, Azure DevOps will automatically retry it once before reporting failure.

**Log Preservation**: Logs from each attempt are saved to separate folders (`attempt-1/`, `attempt-2/`, etc.) so that both the original run and retry logs are preserved for debugging. This uses the `SYSTEM_JOBATTEMPT` environment variable provided by Azure DevOps.

### Individual iOS Test Lanes

iOS tests are split into individual jobs for faster debugging and parallel execution. Each test runs on both ARM64 and MacOSPool for comparison:

| Test | Description | Timeout |
|------|-------------|---------|
| `RunOniOS_MauiDebug` | MAUI app, Debug config | 45 min |
| `RunOniOS_MauiRelease` | MAUI app, Release config | 45 min |
| `RunOniOS_MauiReleaseTrimFull` | MAUI app, Release, full trim | 45 min |
| `RunOniOS_BlazorDebug` | Blazor app, Debug config | 45 min |
| `RunOniOS_BlazorRelease` | Blazor app, Release config | 45 min |
| `RunOniOS_MauiNativeAOT` | MAUI app, NativeAOT | 45 min |

**Note**: `RunOniOS_BlazorReleaseTrimFull` is temporarily disabled due to [ASP.NET Core issue #63951](https://github.com/dotnet/aspnetcore/issues/63951).

## Test Categories

Tests are organized by categories (defined in `Utilities/Categories.cs`) that map to CI jobs:

| Category | Purpose | Platform |
|----------|---------|----------|
| `Build` | Basic template build tests | All |
| `WindowsTemplates` | Windows-specific scenarios | Windows |
| `macOSTemplates` | macOS-specific scenarios | macOS |
| `Blazor` | Blazor hybrid templates | All |
| `MultiProject` | Multi-project templates | All |
| `AOT` | Native AOT compilation | macOS |
| `RunOnAndroid` | Build, install, run on Android emulator | macOS |
| `RunOniOS` | iOS simulator tests (class-level category on `AppleTemplateTests`) | macOS |
| `Samples` | Sample project builds | All |

**Note**: The `RunOniOS` category is applied at the **class level** on `AppleTemplateTests`. Individual test methods don't need their own category attribute.

## Writing Integration Tests

### Basic Pattern

```csharp
[Test]
[Category(Categories.Build)]
[TestCase("maui", DotNetCurrent, "Debug")]
[TestCase("maui", DotNetCurrent, "Release")]
public void Build(string id, string framework, string config)
{
    var projectDir = TestDirectory;
    var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

    // Create from template
    Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
        $"Unable to create template {id}.");

    // Build
    Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps),
        $"Project failed to build.");
}
```

### Device Test Pattern (iOS)

```csharp
[Test]
[Category(Categories.RunOniOS)]
[TestCase("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, null)]
public void RunOniOS(string id, string config, string framework, RuntimeVariant rv, string? trimMode)
{
    // Create and build
    Assert.IsTrue(DotnetInternal.New(id, projectDir, framework));
    Assert.IsTrue(DotnetInternal.Build(projectFile, config, 
        framework: $"{framework}-ios",
        properties: buildProps,
        runtimeIdentifier: TestEnvironment.IOSSimulatorRuntimeIdentifier));

    // Run with XHarness
    Assert.IsTrue(XHarness.RunAppleForTimeout(appPath, resultDir, TestSimulator.XHarnessID, TestSimulator.GetUDID()));
}
```

### Platform Guards

```csharp
if (!TestEnvironment.IsMacOS)
    Assert.Ignore("Running Apple templates is only supported on macOS.");

if (!TestEnvironment.IsWindows)
    Assert.Ignore("Running Windows templates is only supported on Windows.");
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `BaseBuildTest` | Base class with `TestDirectory`, `BuildProps`, lifecycle methods |
| `BaseTemplateTests` | Extends BaseBuildTest with template-specific setup |
| `DotnetInternal` | Wraps `dotnet` CLI using local SDK (`.dotnet/dotnet`) |
| `XHarness` | Runs apps on devices/simulators |
| `Simulator` | iOS simulator management (boot, shutdown, UDID) |
| `TestEnvironment` | Platform detection, paths, `IOSSimulatorRuntimeIdentifier` |
| `FileUtilities` | File manipulation helpers |

## Template IDs

- `maui` - .NET MAUI App
- `maui-blazor` - .NET MAUI Blazor Hybrid App
- `maui-blazor-web` - .NET MAUI Blazor Web Solution
- `mauilib` - .NET MAUI Class Library
- `maui-multiproject` - .NET MAUI Multi-Project App

## Running Tests Locally

### Prerequisites

1. Verify `.dotnet/` folder exists (local SDK). If missing, stop and tell user to run `dotnet cake` to provision locally.
2. Set `MAUI_PACKAGE_VERSION` environment variable. If missing, tests will fail with "MAUI_PACKAGE_VERSION was not set."
   - Example: `export MAUI_PACKAGE_VERSION=$(ls .dotnet/packs/Microsoft.Maui.Sdk | head -1)`

### Environment Variables

| Variable | Required | Purpose |
|----------|----------|---------|
| `MAUI_PACKAGE_VERSION` | Yes | Version of MAUI packages being tested |
| `IOS_TEST_DEVICE` | No | iOS simulator target (e.g., `ios-simulator-64_18.5`) |
| `SKIP_XCODE_VERSION_CHECK` | No | Set to `true` to bypass Xcode version validation |

### Run Commands

```bash
# Run specific category
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests \
  --filter "Category=Build"

# Run specific test
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests \
  --filter "FullyQualifiedName~AppleTemplateTests.RunOniOS"

# With iOS device and Xcode skip
export IOS_TEST_DEVICE="ios-simulator-64_18.5"
export SKIP_XCODE_VERSION_CHECK=true
dotnet test ... --filter "Category=RunOniOS"
```

## Common Pitfalls

| Issue | Solution |
|-------|----------|
| Template not found | Verify workloads installed, `MAUI_PACKAGE_VERSION` set |
| Xcode version mismatch | Set `SKIP_XCODE_VERSION_CHECK=true` |
| Device/simulator not found | Verify `IOS_TEST_DEVICE` or emulator is running |
| XHarness timeout on iOS | Expected behavior - app runs until timeout |
| Architecture mismatch | Use `TestEnvironment.IOSSimulatorRuntimeIdentifier` |

## Best Practices

### DO
- Use `BuildProps` from base class for isolation
- Apply `[Category]` at the **class level** when all tests share the same category
- Check platform with `TestEnvironment.Is*` before platform-specific tests
- Use `TestEnvironment.IOSSimulatorRuntimeIdentifier` for iOS builds
- Include meaningful assertion messages

### DON'T
- Hardcode paths - use `TestDirectory`, `TestEnvironment` helpers
- Add `[Category]` to each test method when the class has a category
- Skip platform guards for platform-specific tests
- Hardcode iOS runtime identifiers (arm64 vs x64)

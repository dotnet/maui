---
description: "Guidance for GitHub Copilot when working with .NET MAUI integration tests"
applyTo: "src/TestUtils/src/Microsoft.Maui.IntegrationTests/**"
---

# .NET MAUI Integration Tests Guidelines

Integration tests validate end-to-end functionality by creating, building, and running .NET MAUI projects using templates and the local workload.

## Test Framework

These integration tests use **xUnit** as the test framework.

## CI Infrastructure

All integration tests run on **Azure DevOps agents** via the stage template `eng/pipelines/arcade/stage-integration-tests.yml`.

- **Windows tests**: Run on Windows 1ES pools
- **macOS tests**: Run on Azure Pipelines hosted macOS-15 images (ARM64)

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
| `RunOniOS` | Build, install, run on iOS simulator | macOS |
| `Samples` | Sample project builds | All |

**Critical**: Each test should have **exactly ONE** `[Trait("Category", Categories.X)]` attribute.

## Writing Integration Tests

### Basic Pattern

```csharp
[Trait("Category", Categories.Build)]
public class SimpleTemplateTest : BaseTemplateTests
{
    public SimpleTemplateTest(BuildTestFixture fixture, ITestOutputHelper output) 
        : base(fixture, output) { }

    [Theory]
    [InlineData("maui", DotNetCurrent, "Debug")]
    [InlineData("maui", DotNetCurrent, "Release")]
    public void Build(string id, string framework, string config)
    {
        var projectDir = TestDirectory;
        var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

        // Create from template
        Assert.True(DotnetInternal.New(id, projectDir, framework),
            $"Unable to create template {id}.");

        // Build
        Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps),
            $"Project failed to build.");
    }
}
```

### Device Test Pattern (iOS)

```csharp
[Trait("Category", Categories.RunOniOS)]
public class AppleTemplateTests : BaseBuildTest, IDisposable
{
    public AppleTemplateTests(BuildTestFixture fixture, ITestOutputHelper output) 
        : base(fixture, output) { }

    [Theory]
    [InlineData("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, null)]
    public void RunOniOS(string id, string config, string framework, RuntimeVariant rv, string? trimMode)
    {
        if (!TestEnvironment.IsMacOS)
            return; // Skip on non-macOS
            
        // Create and build
        Assert.True(DotnetInternal.New(id, projectDir, framework));
        Assert.True(DotnetInternal.Build(projectFile, config, 
            framework: $"{framework}-ios",
            properties: buildProps));

        // Run with XHarness
        Assert.True(XHarness.RunAppleForTimeout(appPath, resultDir, TestSimulator.XHarnessID, TestSimulator.GetUDID()));
    }
}
```

### Platform Guards

```csharp
// In xUnit, skip tests by returning early
if (!TestEnvironment.IsMacOS)
    return; // Skip: Running Apple templates is only supported on macOS.

if (!TestEnvironment.IsWindows)
    return; // Skip: Running Windows templates is only supported on Windows.
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `BaseBuildTest` | Base class with `TestDirectory`, `BuildProps`, lifecycle methods |
| `BaseTemplateTests` | Extends BaseBuildTest with template-specific setup |
| `BuildTestFixture` | Collection fixture for one-time setup (NuGet config, packages) |
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
- Use exactly ONE `[Trait("Category", Categories.X)]` per test class
- Check platform with `TestEnvironment.Is*` before platform-specific tests
- Use `TestEnvironment.IOSSimulatorRuntimeIdentifier` for iOS builds
- Include meaningful assertion messages
- Implement `IDisposable` for cleanup in device test classes

### DON'T
- Hardcode paths - use `TestDirectory`, `TestEnvironment` helpers
- Use multiple category traits per test
- Skip platform guards for platform-specific tests
- Hardcode iOS runtime identifiers (arm64 vs x64)

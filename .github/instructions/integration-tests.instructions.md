---
description: "Guidance for GitHub Copilot when working with .NET MAUI integration tests"
applyTo: "src/TestUtils/src/Microsoft.Maui.IntegrationTests/**"
---

# .NET MAUI Integration Tests Guidelines

This document provides comprehensive guidance for working with integration tests in the .NET MAUI repository located at `src/TestUtils/src/Microsoft.Maui.IntegrationTests/`.

## Overview

Integration tests validate end-to-end functionality by creating, building, publishing, and running .NET MAUI projects using templates and the local workload. These tests ensure that:
- Templates generate valid projects
- Projects build successfully across all configurations
- Platform-specific functionality works correctly
- NuGet packages are correctly consumed
- Various build scenarios and MSBuild properties work as expected

## Project Structure

```
src/TestUtils/src/Microsoft.Maui.IntegrationTests/
├── BaseBuildTest.cs              # Base test infrastructure
├── BaseTemplateTests.cs          # Base class for template tests
├── SimpleTemplateTest.cs         # Basic template build tests
├── AndroidTemplateTests.cs       # Android device tests
├── AppleTemplateTests.cs         # iOS/macOS device tests
├── WindowsTemplateTest.cs        # Windows-specific tests
├── MacTemplateTest.cs           # macOS-specific tests
├── BlazorTemplateTest.cs        # Blazor hybrid tests
├── AOTTemplateTest.cs           # Native AOT tests
├── MultiProjectTemplateTest.cs  # Multi-project template tests
├── ResizetizerTests.cs          # Resizetizer integration tests
├── SampleTests.cs               # Sample project tests
├── Utilities/                   # Helper utilities
│   ├── DotnetInternal.cs        # dotnet CLI wrapper
│   ├── XHarness.cs              # XHarness wrapper
│   ├── FileUtilities.cs         # File manipulation helpers
│   ├── TestEnvironment.cs       # Environment detection
│   ├── Categories.cs            # Test categories
│   ├── BuildWarningsUtilities.cs # Build warning analysis
│   └── ToolRunner.cs            # Process execution helper
├── Android/                     # Android-specific utilities
│   ├── Adb.cs                   # Android Debug Bridge wrapper
│   └── Emulator.cs              # Emulator management
├── Apple/                       # Apple platform utilities
│   ├── Simulator.cs             # iOS/macOS simulator management
│   └── Codesign.cs              # Code signing utilities
└── TestResources/               # Embedded test resources
```

## Test Categories

Tests are organized by categories (defined in `Utilities/Categories.cs`) that map to CI jobs:

### Build-Only Categories
- **`Build`** - Basic template build tests (SimpleTemplateTest)
- **`WindowsTemplates`** - Windows-specific build scenarios
- **`macOSTemplates`** - macOS-specific build scenarios
- **`Blazor`** - Blazor hybrid template tests
- **`MultiProject`** - Multi-project template tests
- **`AOT`** - Native AOT compilation tests

### Device/Simulator Categories
- **`RunOnAndroid`** - Tests that build, install, and run on Android emulator
- **`RunOniOS`** - Tests that build, install, and run on iOS simulator
- **`Samples`** - Sample project build tests

**Critical**: Each test should have **exactly ONE** `[Category]` attribute matching one of the above categories.

## Base Test Classes

### BaseBuildTest

The foundation for all integration tests. Key features:

#### Properties
- **`TestName`** - Sanitized test name (max 20 chars, safe for file paths)
- **`TestDirectory`** - Isolated directory for test execution
- **`LogDirectory`** - Directory for test logs
- **`MauiPackageVersion`** - Version from `MAUI_PACKAGE_VERSION` environment variable
- **`BuildProps`** - Standard MSBuild properties for isolated builds

#### Standard MSBuild Properties
```csharp
protected List<string> BuildProps => new()
{
    "RestoreNoCache=true",                              // No cached packages
    $"RestorePackagesPath={TestDirectory}/packages",    // Isolated package cache
    $"RestoreConfigFile={TestNuGetConfig}",            // Custom NuGet.config
    "CustomBeforeMicrosoftCSharpTargets=...",          // Suppress iOS warnings on Windows
    "DisableTransitiveFrameworkReferenceDownloads=true", // No transitive downloads
    "TreatWarningsAsErrors=true",                       // Warnings as errors
    "TrimmerSingleWarn=false",                         // Detailed trimmer warnings
};
```

#### Lifecycle Methods
- **`[OneTimeSetUp] BuildTestFxtSetUp()`** - Copies NuGet packages to test directory, creates NuGet.config
- **`[SetUp] BuildTestSetUp()`** - Creates fresh `TestDirectory` for each test
- **`[TearDown] BuildTestTearDown()`** - Attaches logs as test artifacts
- **`[OneTimeTearDown] BuildTestFxtTearDown()`** - Cleanup (currently empty)

### BaseTemplateTests

Extends `BaseBuildTest` with template-specific functionality:

#### Setup
```csharp
[SetUp]
public void TemplateTestsSetUp()
{
    // Copies Directory.Build.props and Directory.Build.targets from Templates/tests
    // These provide template-specific build configurations
}
```

#### Helper Methods
- **`OnlyAndroid(projectFile)`** - Modifies project to target only Android
- **`AssertContains(expected, actual)`** - String containment assertion
- **`AssertDoesNotContain(expected, actual)`** - String non-containment assertion

## Framework Version Constants

Defined in `BaseBuildTest`:

```csharp
public const string DotNetCurrent = "net10.0";   // Current .NET version
public const string DotNetPrevious = "net9.0";   // Previous .NET version

// For <MauiVersion> property testing
public const string MauiVersionCurrent = "";     // Latest released current version
public const string MauiVersionPrevious = "9.0.82"; // Previous .NET MAUI version
```

**Important**: Update these constants when targeting a new .NET version.

## Common Test Patterns

### Pattern 1: Basic Template Build Test

```csharp
[Test]
[Category(Categories.Build)]
[TestCase("maui", DotNetCurrent, "Debug")]
[TestCase("maui", DotNetCurrent, "Release", "TrimMode=partial")]
public void Build(string id, string framework, string config, string additionalBuildParams = "")
{
    var projectDir = TestDirectory;
    var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

    // Create from template
    Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
        $"Unable to create template {id}. Check test output for errors.");

    // Prepare build properties
    var buildProps = BuildProps;
    if (!string.IsNullOrEmpty(additionalBuildParams))
        buildProps.Add(additionalBuildParams);

    // Build
    Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: buildProps),
        $"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
}
```

### Pattern 2: Device/Simulator Test (Android)

```csharp
[Test]
[Category(Categories.RunOnAndroid)]
[TestCase("maui", DotNetCurrent, "Debug")]
public void RunOnAndroid(string id, string framework, string config)
{
    var projectDir = TestDirectory;
    var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

    // Create template
    Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
        $"Unable to create template {id}. Check test output for errors.");

    // Add instrumentation for automated testing
    AddInstrumentation(projectDir);

    // Build and install on device
    Assert.IsTrue(DotnetInternal.Build(projectFile, config, 
        target: "Install", 
        framework: $"{framework}-android", 
        properties: BuildProps),
        $"Project failed to install. Check test output for errors.");

    // Run with XHarness
    testPackage = $"com.companyname.{Path.GetFileName(projectDir).ToLowerInvariant()}";
    Assert.IsTrue(XHarness.RunAndroid(testPackage, Path.Combine(projectDir, "xh-results"), -1),
        $"Project failed to run. Check test output for errors.");
}
```

### Pattern 3: Platform-Specific Test

```csharp
[Test]
[Category(Categories.WindowsTemplates)]
[TestCase("maui", DotNetCurrent, "Release")]
public void PublishUnpackaged(string id, string framework, string config)
{
    if (!TestEnvironment.IsWindows)
    {
        Assert.Ignore("Running Windows templates is only supported on Windows.");
    }

    // Test implementation...
}
```

### Pattern 4: Native AOT Test

```csharp
[Test]
[Category(Categories.AOT)]
[TestCase("maui", $"{DotNetCurrent}-ios", "iossimulator-x64")]
public void PublishNativeAOT(string id, string framework, string runtimeIdentifier)
{
    var buildProps = new List<string>(BuildProps)
    {
        "PublishAot=true",
        "PublishAotUsingRuntimePack=true",
        "_IsPublishing=true",
        "IlcTreatWarningsAsErrors=false",
        "_RequireCodeSigning=false"
    };

    string binLogPath = $"publish-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
    Assert.IsTrue(DotnetInternal.Build(projectFile, "Release", 
        framework: framework, 
        properties: buildProps, 
        runtimeIdentifier: runtimeIdentifier, 
        binlogPath: binLogPath),
        $"Project failed to build. Check test output for errors.");

    // Validate no AOT warnings
    var actualWarnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogPath);
    actualWarnings.AssertNoWarnings();
}
```

## Utility Classes

### DotnetInternal

Wraps `dotnet` CLI commands using the locally built .NET SDK (`.dotnet/dotnet`):

**Key Methods**:
- **`New(shortName, outputDirectory, framework, additionalParams)`** - Creates project from template
- **`Build(projectFile, config, target, framework, properties, binlogPath, runtimeIdentifier)`** - Builds project
- **`Publish(projectFile, config, target, framework, properties, binlogPath, runtimeIdentifier)`** - Publishes project
- **`Run(command, args, timeout)`** - Executes arbitrary dotnet command
- **`RunForOutput(command, args, out exitCode, timeout)`** - Returns command output

**Environment Variables**:
- `DOTNET_MULTILEVEL_LOOKUP=0` - Prevents SDK lookup outside local .dotnet
- `DOTNET_ROOT=.dotnet` - Points to locally built SDK

**Default Timeout**: 1800 seconds (30 minutes)

### XHarness

Wraps XHarness CLI for running apps on devices/simulators:

**Key Methods**:
- **`RunAndroid(packageName, resultDir, expectedExitCode, launchTimeout)`** - Runs Android app
- **`RunAppleForTimeout(appPath, resultDir, targetDevice, launchTimeout)`** - Runs iOS app (times out as expected behavior)
- **`InstallSimulator(targetDevice)`** - Installs iOS simulator
- **`GetSimulatorUDID(targetDevice)`** - Gets simulator UDID

**Android Example**:
```csharp
XHarness.RunAndroid("com.companyname.myapp", "./xh-results", expectedExitCode: -1)
```

**iOS Example**:
```csharp
XHarness.RunAppleForTimeout("./bin/Release/net10.0-ios/MyApp.app", "./xh-results", "ios-simulator-64")
```

### FileUtilities

File manipulation helpers:

**Methods**:
- **`ReplaceInFile(file, oldValue, newValue)`** - Simple string replacement
- **`ReplaceInFile(file, Dictionary<string, string> replacements)`** - Multiple replacements
- **`ShouldContainInFile(file, value)`** - Asserts file contains value
- **`ShouldNotContainInFile(file, value)`** - Asserts file doesn't contain value
- **`CreateFileFromResource(resourceName, destination)`** - Extracts embedded resource to file

**Usage**:
```csharp
FileUtilities.ReplaceInFile(projectFile,
    "<WindowsPackageType>None</WindowsPackageType>",
    "");

FileUtilities.ReplaceInFile(projectFile, new Dictionary<string, string>()
{
    { "UseMaui", "UseMauiCore" },
    { "SingleProject", "EnablePreviewMsixTooling" },
});
```

### TestEnvironment

Platform and environment detection:

**Properties**:
- **`IsWindows`** - Running on Windows
- **`IsMacOS`** - Running on macOS
- **`IsLinux`** - Running on Linux
- **`IsRunningOnCI`** - Running in Azure Pipelines (checks `AGENT_NAME`)

**Methods**:
- **`GetMauiDirectory()`** - Returns repository root directory
- **`GetTestDirectoryRoot()`** - Returns base test output directory
- **`GetLogDirectory()`** - Returns log output directory
- **`GetAndroidSdkPath()`** - Returns Android SDK path
- **`GetAndroidCommandLineToolsPath()`** - Returns Android SDK command-line tools path

### BuildWarningsUtilities

Analyzes build warnings from MSBuild binary logs:

**Methods**:
- **`ReadNativeAOTWarningsFromBinLog(binLogPath)`** - Extracts AOT warnings from binlog
- **`AssertNoWarnings(actualWarnings)`** - Asserts no warnings present
- **`AssertWarnings(actualWarnings, expectedWarnings)`** - Asserts expected warnings match actual

**Usage**:
```csharp
var actualWarnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogPath);
actualWarnings.AssertNoWarnings(); // Fail if any warnings

// OR compare against expected warnings
actualWarnings.AssertWarnings(BuildWarningsUtilities.ExpectedNativeAOTWarnings);
```

## Device/Simulator Management

### Android Emulator (Emulator.cs)

**Lifecycle**:
```csharp
Emulator TestAvd = new Emulator();

[OneTimeSetUp]
public void SetUp()
{
    // On Apple Silicon Macs, use ARM64 emulator
    if (TestEnvironment.IsMacOS && RuntimeInformation.OSArchitecture == Architecture.Arm64)
        TestAvd.Abi = "arm64-v8a";

    // Accept licenses and install AVD
    Assert.IsTrue(TestAvd.AcceptLicenses(out var licenseOutput));
    Assert.IsTrue(TestAvd.InstallAvd(out var installOutput));
}

[SetUp]
public void TestSetUp()
{
    var emulatorLog = Path.Combine(TestDirectory, $"emulator-launch-{DateTime.UtcNow.ToFileTimeUtc()}.log");
    Assert.IsTrue(TestAvd.LaunchAndWaitForAvd(600, emulatorLog));
}

[OneTimeTearDown]
public void TearDown()
{
    Adb.KillEmulator(TestAvd.Id);
}
```

### iOS Simulator (Simulator.cs)

**Lifecycle**:
```csharp
Simulator TestSimulator = new Simulator();

[SetUp]
public void SetUp()
{
    if (!TestEnvironment.IsMacOS)
        Assert.Ignore("Running Apple templates is only supported on macOS.");

    TestSimulator.Shutdown();
    Assert.IsTrue(TestSimulator.Launch(), $"Failed to boot simulator.");
    TestSimulator.ShowWindow();
}

[OneTimeTearDown]
public void TearDown()
{
    TestSimulator.Shutdown();
}
```

## Writing New Integration Tests

### Step 1: Choose Base Class

- **Build-only tests** → Inherit from `BaseTemplateTests`
- **Tests requiring custom setup** → Inherit from `BaseBuildTest`

### Step 2: Choose Category

Select the appropriate category from `Utilities/Categories.cs`:
- Use `[Category(Categories.Build)]` for basic template builds
- Use `[Category(Categories.RunOnAndroid)]` for Android device tests
- Use `[Category(Categories.RunOniOS)]` for iOS device tests
- Use `[Category(Categories.WindowsTemplates)]` for Windows-specific tests
- Use `[Category(Categories.macOSTemplates)]` for macOS-specific tests

### Step 3: Write Test Method

```csharp
[Test]
[Category(Categories.Build)]
[TestCase("maui", DotNetCurrent, "Debug")]
[TestCase("maui", DotNetCurrent, "Release")]
public void YourTestName(string templateId, string framework, string config)
{
    var projectDir = TestDirectory;
    var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

    // 1. Create project from template
    Assert.IsTrue(DotnetInternal.New(templateId, projectDir, framework),
        $"Unable to create template {templateId}. Check test output for errors.");

    // 2. Modify project if needed
    FileUtilities.ReplaceInFile(projectFile, "oldValue", "newValue");

    // 3. Build
    Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps),
        $"Project {Path.GetFileName(projectFile)} failed to build. Check test output for errors.");

    // 4. Assertions (file checks, etc.)
    Assert.IsTrue(File.Exists(Path.Combine(projectDir, "expected-file.txt")));
}
```

### Step 4: Platform-Specific Guards

Always check platform before executing platform-specific tests:

```csharp
if (!TestEnvironment.IsWindows)
{
    Assert.Ignore("Running Windows templates is only supported on Windows.");
}

if (!TestEnvironment.IsMacOS)
{
    Assert.Ignore("Running Apple templates is only supported on macOS.");
}
```

## Template IDs

Available template short names:

- **`maui`** - .NET MAUI App (standard MAUI application)
- **`maui-blazor`** - .NET MAUI Blazor App (Blazor hybrid)
- **`maui-blazor-web`** - .NET MAUI Blazor Web Solution (multi-project Blazor)
- **`mauilib`** - .NET MAUI Class Library
- **`maui-multiproject`** - .NET MAUI Multi-Project App
- **`maui-aspire-servicedefaults`** - .NET Aspire Service Defaults (for Aspire integration)
- **`classlib`** - .NET Class Library (for testing dependency scenarios)

## Common Template Parameters

When calling `DotnetInternal.New()`:

- **`--sample-content`** - Include sample content in template
- **`--empty`** - Create empty project (no sample content)
- **`--use-program-main`** - Use Program.Main instead of top-level statements
- **`--interactivity None|WebAssembly|Server|Auto`** - Blazor interactivity location
- **`--android`** / **`--ios`** / **`--windows`** / **`--macos`** - Include specific platforms only

**Example**:
```csharp
DotnetInternal.New("maui-blazor-web", projectDir, DotNetCurrent, 
    additionalDotNetNewParams: "--interactivity Server --empty");
```

## Runtime Variants

For Apple platform tests, specify runtime variant:

```csharp
public enum RuntimeVariant
{
    Mono,        // Standard Mono runtime
    NativeAOT    // Native AOT compilation
}
```

**Usage in test cases**:
```csharp
[TestCase("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
[TestCase("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.NativeAOT, null)]
```

## Build Properties for Special Scenarios

### Trimming
```csharp
buildProps.Add("TrimMode=partial");  // Faster, partial trimming
buildProps.Add("TrimMode=full");     // Aggressive, full trimming
buildProps.Add("TrimmerSingleWarn=false"); // Detailed trimmer warnings
```

### Native AOT (iOS/Mac/Windows)
```csharp
buildProps.Add("PublishAot=true");
buildProps.Add("PublishAotUsingRuntimePack=true");
buildProps.Add("_IsPublishing=true");
buildProps.Add("IlcTreatWarningsAsErrors=false");
buildProps.Add("_RequireCodeSigning=false"); // iOS without signing
```

### Windows Packaging
```csharp
buildProps.Add("WindowsPackageType=None");  // Unpackaged (loose files)
buildProps.Add("WindowsPackageType=MSIX");  // MSIX package
buildProps.Add("WindowsAppSDKSelfContained=true");
buildProps.Add("SelfContained=true");
```

### Android-Specific
```csharp
buildProps.Add("RuntimeIdentifier=android-arm64");
buildProps.Add("AndroidEnableProfiler=false");
```

### iOS-Specific
```csharp
buildProps.Add("RuntimeIdentifier=iossimulator-x64");
buildProps.Add("_RequireCodeSigning=false");
buildProps.Add("EnableCodeSigning=false");
```

## Binary Log Analysis

Always create binary logs for complex builds:

```csharp
string binLogPath = $"build-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
Assert.IsTrue(DotnetInternal.Build(projectFile, config, binlogPath: binLogPath, ...));

// Analyze warnings
var warnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogPath);
```

Binary logs are automatically attached as test artifacts by `BuildTestTearDown()`.

## Running Integration Tests Locally

### Basic Execution

```bash
# Run all integration tests
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests

# Run specific category
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests --filter "Category=Build"

# Run specific test
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests \
  --filter "FullyQualifiedName~SimpleTemplateTest.Build" \
  --logger "console;verbosity=diagnostic"

# Run with specific test case
dotnet test src/TestUtils/src/Microsoft.Maui.IntegrationTests \
  --filter "Name=Build(%22maui%22,%22net10.0%22,%22Debug%22,False)" \
  --logger "console;verbosity=diagnostic"
```

### Prerequisites

1. **Build MAUI locally**:
   ```bash
   ./build.sh --target=VS --workloads=global  # macOS/Linux
   # OR
   .\build.cmd --target=VS --workloads=global  # Windows
   ```

2. **Set environment variable**:
   ```bash
   export MAUI_PACKAGE_VERSION="10.0.200-preview.1.2345"  # Use version from artifacts
   ```

3. **Ensure device/simulator available** (for device tests):
   - Android: Emulator created and booted
   - iOS: Simulator available

### Environment Variables

- **`MAUI_PACKAGE_VERSION`** (Required) - Version of MAUI packages being tested
- **`AGENT_NAME`** (CI only) - Indicates running in Azure Pipelines
- **`BUILD_ARTIFACTSTAGINGDIRECTORY`** (CI only) - Log output directory
- **`AGENT_TEMPDIRECTORY`** (CI only) - Test execution directory
- **`IOS_TEST_DEVICE`** (iOS tests) - Specific iOS simulator to target

## CI Pipeline Integration

Integration tests run in Azure Pipelines via `eng/pipelines/arcade/stage-integration-tests.yml`.

### Pipeline Structure

```yaml
stages:
- stage: IntegrationTests
  jobs:
  - job: BuildTests_Windows
    pool: Windows
    steps:
    - template: /eng/pipelines/common/maui-templates-steps.yml  # Setup workloads
    - script: dotnet test ... --filter "Category=Build"

  - job: AndroidDeviceTests
    pool: macOS
    steps:
    - template: /eng/pipelines/common/maui-templates-steps.yml
    - script: dotnet test ... --filter "Category=RunOnAndroid"
```

### Category to Job Mapping

| Category | CI Job | Platform | Purpose |
|----------|--------|----------|---------|
| `Build` | Build Tests | Windows/macOS | Basic template builds |
| `WindowsTemplates` | Windows Templates | Windows | Windows-specific scenarios |
| `macOSTemplates` | macOS Templates | macOS | macOS-specific scenarios |
| `Blazor` | Blazor Tests | Windows/macOS | Blazor hybrid templates |
| `MultiProject` | Multi-Project Tests | Windows/macOS | Multi-project templates |
| `AOT` | AOT Tests | macOS | Native AOT compilation |
| `RunOnAndroid` | Android Device Tests | macOS | Android emulator execution |
| `RunOniOS` | iOS Device Tests | macOS | iOS simulator execution |
| `Samples` | Sample Tests | Windows/macOS | Sample project builds |

## Common Pitfalls and Solutions

### Issue: Test hangs during restore

**Solution**: Ensure `RestoreNoCache=true` and isolated `RestorePackagesPath` in `BuildProps`.

### Issue: Template not found

**Solution**: Verify workloads installed correctly and `MAUI_PACKAGE_VERSION` is set.

### Issue: Build succeeds locally but fails in CI

**Solution**: Check for absolute paths or environment-specific assumptions. Use `TestEnvironment` helpers.

### Issue: Test directory too long (Windows)

**Solution**: `TestName` automatically hashes long test names to ≤20 characters.

### Issue: Parallel test execution conflicts

**Solution**: Each test gets isolated `TestDirectory` and package cache. No conflicts should occur.

### Issue: Device/simulator tests fail

**Solution**: 
- Android: Verify emulator booted with `Emulator.LaunchAndWaitForAvd()`
- iOS: Verify simulator booted with `Simulator.Launch()`

### Issue: XHarness timeout

**Solution**: 
- Android: Check `expectedExitCode` matches app behavior
- iOS: Timeout is **expected behavior** (no reliable exit code detection on iOS 15+)

## Best Practices

### DO

✅ **Use base class lifecycle methods** - Don't reinvent setup/teardown
✅ **Use `BuildProps` from base class** - Ensures isolation
✅ **Create descriptive test case names** - Use `[TestCase]` parameters effectively
✅ **Check platform before executing** - Use `TestEnvironment.Is*` checks
✅ **Attach binary logs** - Automatic via `BuildTestTearDown()`
✅ **Use isolated test directories** - Automatic via `TestDirectory` property
✅ **Test multiple configurations** - Debug/Release, frameworks, platforms
✅ **Clean up device tests** - Uninstall packages, kill emulators
✅ **Use meaningful assertion messages** - Include context for failures

### DON'T

❌ **Don't share state between tests** - Each test is isolated
❌ **Don't hardcode paths** - Use `TestDirectory`, `TestEnvironment` helpers
❌ **Don't assume platform** - Always check with `TestEnvironment.Is*`
❌ **Don't ignore device setup failures** - Assert setup succeeds
❌ **Don't use multiple categories** - Exactly one `[Category]` per test
❌ **Don't skip teardown** - Clean up resources (emulators, simulators)
❌ **Don't rely on test execution order** - Tests must be independent
❌ **Don't commit absolute paths** - Everything must be relative to repo root

## Test Execution Flow

```
┌─────────────────────────────────────────────────────────────────┐
│ [OneTimeSetUp] BuildTestFxtSetUp()                              │
│   - Copy NuGet packages to test-dir/extra-packages              │
│   - Create test-dir/NuGet.config                                │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ For each test class with templates...                          │
│ [SetUp] TemplateTestsSetUp()                                   │
│   - Copy Directory.Build.props/targets from Templates/tests    │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ For each test method...                                         │
│ [SetUp] BuildTestSetUp()                                        │
│   - Delete and recreate TestDirectory                          │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ Execute Test Method                                             │
│   1. Create template with DotnetInternal.New()                 │
│   2. Modify project files if needed (FileUtilities)            │
│   3. Build with DotnetInternal.Build() or Publish()            │
│   4. Run on device (XHarness) if needed                        │
│   5. Assert results                                             │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ [TearDown] BuildTestTearDown()                                  │
│   - Attach all *.log files as test artifacts                   │
└─────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────┐
│ [OneTimeTearDown] BuildTestFxtTearDown()                        │
│   - (Currently no cleanup needed)                               │
└─────────────────────────────────────────────────────────────────┘
```

## Example: Complete Integration Test

Here's a complete example showing best practices:

```csharp
using Microsoft.Maui.IntegrationTests;

namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.Build)]
public class CustomFeatureTests : BaseTemplateTests
{
    [Test]
    [TestCase("maui", DotNetCurrent, "Debug", false)]
    [TestCase("maui", DotNetCurrent, "Release", true)]
    [TestCase("maui-blazor", DotNetCurrent, "Debug", false)]
    [TestCase("maui-blazor", DotNetCurrent, "Release", true)]
    public void BuildWithCustomFeature(string id, string framework, string config, bool enableTrimming)
    {
        var projectDir = TestDirectory;
        var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

        // Step 1: Create template
        Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
            $"Unable to create template {id}. Check test output for errors.");

        // Step 2: Modify project to enable custom feature
        FileUtilities.ReplaceInFile(projectFile,
            "</Project>",
            """
            <PropertyGroup>
                <EnableCustomFeature>true</EnableCustomFeature>
            </PropertyGroup>
            </Project>
            """);

        // Step 3: Prepare build properties
        var buildProps = BuildProps;
        if (enableTrimming)
        {
            buildProps.Add("TrimMode=partial");
        }

        // Step 4: Build
        string binLogPath = $"build-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
        Assert.IsTrue(DotnetInternal.Build(projectFile, config, 
            properties: buildProps, 
            binlogPath: binLogPath, 
            msbuildWarningsAsErrors: true),
            $"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

        // Step 5: Verify expected artifacts exist
        var expectedFile = Path.Combine(projectDir, "obj", config, framework, "CustomFeature.g.cs");
        Assert.IsTrue(File.Exists(expectedFile),
            $"Expected generated file '{expectedFile}' was not created.");

        // Step 6: Verify file contents
        var content = File.ReadAllText(expectedFile);
        StringAssert.Contains("CustomFeatureImplementation", content,
            "Generated file should contain CustomFeatureImplementation class.");
    }
}
```

## Related Documentation

- `.github/copilot-instructions.md` - General MAUI development guidelines
- `.github/instructions/templates.instructions.md` - Template development guidelines
- `docs/DevelopmentTips.md` - General development tips and workflows

## Summary

Integration tests are critical for validating the end-to-end MAUI experience. When writing integration tests:

1. **Inherit from the correct base class** (`BaseBuildTest` or `BaseTemplateTests`)
2. **Use exactly ONE test category** from `Categories.cs`
3. **Leverage provided utilities** (`DotnetInternal`, `XHarness`, `FileUtilities`, etc.)
4. **Ensure test isolation** using `TestDirectory` and `BuildProps`
5. **Include platform checks** for platform-specific tests
6. **Provide detailed assertion messages** for easier debugging
7. **Follow established patterns** from existing tests

Well-written integration tests ensure that MAUI templates work correctly for developers across all platforms and configurations.

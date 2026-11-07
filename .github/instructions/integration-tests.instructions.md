---
description: "Guidance for GitHub Copilot when working with .NET MAUI integration tests"
applyTo: "src/TestUtils/src/Microsoft.Maui.IntegrationTests/**"
---

# .NET MAUI Integration Tests Guidelines

Integration tests validate end-to-end functionality by creating, building, and running .NET MAUI projects using templates and the local workload.

## Test Framework

These integration tests use **xUnit** as the outer test framework. Installed
sample applications are ordinary apps, not native test runners.

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

### MacOSPool Lane Conditions

The **MacOSPool lanes** (non-ARM64 comparison tests) only run under specific conditions:

1. **Non-PR builds** on branches: `main`, `net*.0`, `release/*`, or `inflight/*`
2. **PR builds** where the target branch is: `net*.0`, `release/*`, or `inflight/*`

This ensures MacOSPool comparison tests don't run on regular PR builds targeting `main`, saving CI resources while still running them for release-related branches.

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
[Theory]
[Trait("Category", Categories.Build)]
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
```

### Device Test Pattern (iOS)

```csharp
[Theory]
[Trait("Category", Categories.RunOniOS)]
[InlineData("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, null)]
public void RunOniOS(string id, string config, string framework, RuntimeVariant rv, string? trimMode)
{
    // Create and build
    Assert.True(DotnetInternal.New(id, projectDir, framework));
    Assert.True(DotnetInternal.Build(projectFile, config,
        framework: $"{framework}-ios",
        properties: buildProps,
        runtimeIdentifier: TestEnvironment.IOSSimulatorRuntimeIdentifier));

    // Install and launch on the same dedicated simulator managed by the fixture.
    Assert.True(AppLaunch.RunApple(appPath, resultDir, TestSimulator.GetUDID(), completionMarker));
}
```

### Platform Guards

```csharp
Assert.SkipUnless(TestEnvironment.IsMacOS, "Running Apple templates is only supported on macOS.");

Assert.SkipUnless(TestEnvironment.IsWindows, "Running Windows templates is only supported on Windows.");
```

## Key Classes

| Class | Purpose |
|-------|---------|
| `BaseBuildTest` | Base class with `TestDirectory`, `BuildProps`, lifecycle methods |
| `BaseTemplateTests` | Extends BaseBuildTest with template-specific setup |
| `DotnetInternal` | Wraps `dotnet` CLI using local SDK (`.dotnet/dotnet`) |
| `AppLaunch` | Launches ordinary sample apps with simctl/adb and checks completion evidence |
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

### 🚨 ALWAYS Use the Skill

**When asked to run integration tests, ALWAYS use the `run-integration-tests` skill:**

```powershell
# macOS examples
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "macOSTemplates" -SkipBuild -SkipInstall -SkipXcodeVersionCheck
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "RunOniOS" -SkipBuild -SkipInstall -SkipXcodeVersionCheck
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "RunOnAndroid" -SkipBuild -SkipInstall

# Windows examples
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "WindowsTemplates" -SkipBuild -SkipInstall
pwsh .github/skills/run-integration-tests/scripts/Run-IntegrationTests.ps1 -Category "Build" -SkipBuild -SkipInstall
```

The skill handles:
- ✅ Environment variable setup (`MAUI_PACKAGE_VERSION`, `SKIP_XCODE_VERSION_CHECK`)
- ✅ Cross-platform support (Windows and macOS)
- ✅ Test results in TRX format
- ✅ Proper error reporting

See `.github/skills/run-integration-tests/SKILL.md` for full documentation.

---

### Prerequisites (Manual Setup)

If the skill reports missing prerequisites, provision the local SDK:

1. **Provision the local SDK and workloads** - The `.dotnet/` folder must contain a fully provisioned .NET SDK with MAUI workloads. Run:

   ```bash
   # Step 0: Restore repo-local tools (Cake, etc.) from .config/dotnet-tools.json
   dotnet tool restore

   # Step 1: Download the .NET SDK (creates .dotnet/dotnet binary)
   dotnet cake --target=dotnet

   # Step 2: Install MAUI workloads into the local SDK (takes ~5 minutes)
   dotnet cake --target=dotnet-local-workloads
   ```

   **Verification**: After provisioning, verify the setup:
   ```bash
   # Check dotnet binary exists
   ls .dotnet/dotnet
   
   # Check MAUI workloads are installed
   ls .dotnet/packs/Microsoft.Maui.Sdk
   ```

### Environment Variables (Reference)

The skill sets these automatically, but for manual runs:

| Variable | Required | Purpose |
|----------|----------|---------|
| `MAUI_PACKAGE_VERSION` | Yes | Version of MAUI packages being tested |
| `IOS_TEST_DEVICE` | No | iOS simulator target (e.g., `ios-simulator-64_18.5`) |
| `IOS_TEST_DEVICE_UDID` | No | UUID of a dedicated simulator for both fixture and app launcher; fixture boots/shuts it down, so never select a shared device |
| `SKIP_XCODE_VERSION_CHECK` | No | Set to `true` to bypass Xcode version validation |

### Manual Run Commands (Fallback Only)

**⚠️ Only use these if the skill is unavailable:**

```bash
# Set environment first
export MAUI_PACKAGE_VERSION=$(ls .dotnet/packs/Microsoft.Maui.Sdk | head -1)
export SKIP_XCODE_VERSION_CHECK=true

# Run specific category
dotnet test --project src/TestUtils/src/Microsoft.Maui.IntegrationTests \
  -- --filter-trait "Category=Build" --report-trx

# Run specific test
dotnet test --project src/TestUtils/src/Microsoft.Maui.IntegrationTests \
  -- --filter-method "Microsoft.Maui.IntegrationTests.AppleTemplateTests.RunOniOS*" --report-trx
```

## Common Pitfalls

| Issue | Solution |
|-------|----------|
| Template not found | Verify workloads installed, `MAUI_PACKAGE_VERSION` set |
| Xcode version mismatch | Set `SKIP_XCODE_VERSION_CHECK=true` |
| Device/simulator not found | Verify `IOS_TEST_DEVICE` or emulator is running |
| App launch timeout on iOS | Failure: install/launch/exit verification share a 300-second timeout; timeout alone never proves startup |
| Architecture mismatch | Use `TestEnvironment.IOSSimulatorRuntimeIdentifier` |

## Best Practices

The iOS template smoke tests inject `AppleTemplateLaunchProbe.cs` into the generated
app (not the shipped templates). Both application `OnActivated` and
`SceneOnActivated` start the same guarded main-run-loop timer, which waits
15 seconds, writes a unique per-test completion marker, and exits with code zero.
`AppLaunch.RunApple` requires that marker in `application.log`, successful native
commands, and a zero native application wait status for the launched PID from
runningboard's `termination reported by launchd` log. `simctl launch --console`
alone returns zero even for an application exiting nonzero, so its status is not
sufficient. Missing native exit evidence fails closed. Its 300-second command
budget covers installation, launch, execution and exit verification; it is not the
app's 15-second observation interval. Do not accept timeout codes or SIGKILL as
evidence that the app started. Logs are retained under `app-results/<test-name>/`.

The fixture uses `simctl list runtimes --json` to select the newest available iOS
runtime (or the `IOS_TEST_DEVICE` version), creates a dedicated compatible iPhone,
and deletes it after testing. An explicit `IOS_TEST_DEVICE_UDID` is shut down but
not deleted. App installation, launch, termination and uninstall use that same UUID.

Android builds install via the workload's `Install` target with an explicit
`AdbTarget`. `AppLaunch.RunAndroid` invokes `adb -s <id> shell am instrument -w`
and requires both `INSTRUMENTATION_CODE: -1` and the unique completion marker
returned only after the activity starts. A zero adb exit code alone is not success.
The instrumented app is force-stopped after execution; device logs are retained,
and teardown uninstalls the package on the same emulator. Both launch helpers
accept cancellation, terminate their child process on cancellation/timeout, and
perform bounded cleanup.

For local template validation, set `DOTNET_CLI_HOME` to a session-owned directory
before restoring tools and invoking the integration-test skill. This isolates
user-installed templates that can otherwise override the workload's templates.
Verify the selected template pack path/version in that home's
`.templateengine/dotnetcli/<sdk-version>/templatecache.json`, along with the
generated `Platforms/iOS/SceneDelegate.cs` and `UIApplicationSceneManifest`.
Correct NuGet library versions alone do not establish template provenance.
Exercise both scene-based templates and a test-only legacy lifecycle fixture;
application activation alone is not raised for scene-based apps.

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

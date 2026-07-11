# `dotnet test` for Mobile Platforms

<!-- cspell:words rmarinho testfx logcat Avalonia -->

## Overview

This document describes the design and implementation plan for enabling `dotnet test` to work seamlessly with mobile and device test projects targeting iOS, Android, and other platforms. The goal is to allow developers and AI agents to write and run tests on simulators, emulators, and physical devices using the familiar `dotnet test` command-line experience, powered by the Microsoft Testing Platform (MTP).

> **Scope:** This functionality is **not MAUI-specific**. It will be implemented in the **iOS SDK** and **Android SDK** as first-class platform features, available to all .NET workloads including native iOS/Android apps, MAUI, Uno Platform, Avalonia, and other frameworks.

## Status

| Stage | Status |
|-------|--------|
| Design | 🟡 In Progress |
| Implementation | 🟡 POC Available |
| Testing | 🔴 Not Started |
| Documentation | 🔴 Not Started |

### Proof of Concept

A working POC demonstrating Android device testing with MTP is available at:
- **Repository**: [rmarinho/testfx#2](https://github.com/rmarinho/testfx/pull/2)
- **Features demonstrated**:
  - Two execution modes: Activity Mode (via `dotnet run --device`) and Instrumentation Mode (via `adb instrument`)
  - MSTest with MTP (Microsoft.Testing.Platform) on Android
  - Custom `IDataConsumer` for logcat output
  - TRX file collection from device sandbox
  - Exit code propagation

## Authors

- .NET MAUI Team
- Microsoft Testing Platform Team

## Related Documents

- [dotnet run for MAUI](https://github.com/dotnet/sdk/blob/main/documentation/specs/dotnet-run-for-maui.md)
- [Microsoft Testing Platform Overview](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-intro)
- [MTP Server Mode](https://github.com/microsoft/testfx/tree/main/src/Platform/Microsoft.Testing.Platform/ServerMode)
- [Code Coverage for Native AOT](https://github.com/microsoft/codecoverage/blob/main/samples/Algorithms/scenarios/scenario06/README.md)
- [`dotnet test` CLI](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-test)
- [Unit testing with `dotnet test` (VSTest vs MTP modes)](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)
- [MTP integration with `dotnet test`](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-integration-dotnet-test)
- [MTP TRX extension](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-extensions-test-reports)
- [MTP code coverage extension](https://learn.microsoft.com/en-us/dotnet/core/testing/microsoft-testing-platform-extensions-code-coverage)
- [Microsoft Code Coverage Console (instrument + server mode)](https://learn.microsoft.com/en-us/visualstudio/test/microsoft-code-coverage-console-tool)
- [Push-only protocol](https://github.com/microsoft/testfx/blob/main/src/Platform/Microsoft.Testing.Platform/ServerMode/IPushOnlyProtocol.cs)
- [testfx repo](https://github.com/microsoft/testfx)

---

## 1. Goals and Motivation

### 1.1 Primary Goals

1. **Unified CLI Experience for iOS and Android Testing**: Enable `dotnet test` to work with **any test project** targeting iOS and Android platforms - including native iOS/Android apps, MAUI apps, Uno Platform, Avalonia, and other frameworks. This functionality will be implemented in the **iOS SDK** and **Android SDK** as first-class platform features. .NET MAUI will consume and leverage these SDK implementations for its device testing needs, but the testing infrastructure is **not MAUI-specific** - it's a platform capability available to all .NET workloads.

2. **AI Agent Enablement**: Make it easy for AI coding agents to write, run, and validate tests using only command-line tools. AI agents should be able to:
   - Run tests with a single command
   - Parse test results from standard output
   - Understand which tests passed/failed

3. **XHarness Replacement**: Eventually replace XHarness as the test execution tool, reducing the number of tools to maintain in the ecosystem.

### 1.2 SDK Architecture

This functionality will be implemented in the **Android SDK** and **iOS SDK** as first-class features. .NET MAUI will consume and leverage these SDK implementations rather than implementing its own device testing infrastructure. This ensures:

- Consistent testing experience across all .NET workloads (MAUI, native iOS/Android, etc.)
- Shared maintenance and improvements across the ecosystem
- Direct support for native iOS and Android test projects
- MAUI inherits all capabilities without duplication

### 1.3 Phase 2 Goals (Future Work)

**Code Coverage on Devices**: In a future phase, we plan to enable collection of code coverage data for platform-specific code running on iOS, Android, and Windows devices/simulators. This is **not a primary goal for Phase 1** but will be addressed later once the core testing infrastructure is established.

### 1.4 Non-Goals (Phase 1)

- Test Explorer integration in Visual Studio or VS Code (CLI-only focus for v1; IDE integration is a future phase)
- Hot reload during test execution
- Parallel test execution across multiple devices
- Support every test framework in v1 (start with MSTest runner / MTP-first; broaden later)
- Live debugging of tests in v1 (future work)

---

## 2. MTP Configuration

### 2.1 Enabling MTP Mode

`dotnet test` can run tests using **VSTest** or **Microsoft.Testing.Platform (MTP)**. For MAUI device tests, MTP mode is recommended. Enable it via `global.json`:

```json
{
  "test": {
    "runner": "Microsoft.Testing.Platform"
  }
}
```

### 2.2 Device Test Project Model

A device-test project is an **app that hosts MTP**. It produces an **app package** (apk/ipa/msix) that runs tests inside the real runtime.

**Key requirements:**
- Output must be an executable test app (MTP expectation)
- Include MTP runner + desired extensions via NuGet (TRX report, coverage)

**Example project file:**

```xml
<Project Sdk="MSTest.Sdk/4.3.0">

  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0</TargetFrameworks>
    <UseMaui>true</UseMaui>

    <!-- Exe is recommended for self-hosted MTP apps (MTP itself is host-agnostic) -->
    <OutputType>Exe</OutputType>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- Additional MTP extensions can be referenced explicitly when needed. -->
    <PackageReference Include="Microsoft.Testing.Extensions.TrxReport" Version="2.3.1" />
  </ItemGroup>

</Project>
```

> **Note:** The `IsTestProject` property is sufficient to identify test projects. No additional markers are required.

> **Note:** Using `MSTest.Sdk` as the project SDK (instead of a `PackageReference`) automatically brings in the MSTest framework, adapter, MTP runner, and common extensions. This is the recommended approach for MTP-based test projects. For other test frameworks like xUnit or NUnit, add the runner and extensions explicitly.

> **Versioning:** Samples must not rely on wildcard package versions. Real projects should pin SDK/package versions directly (as shown) or via central package management so device-test restores are reproducible in CI and Helix. Validate exact versions during implementation.

**MTP extensions provide:**
- TRX reporting via `Microsoft.Testing.Extensions.TrxReport` (`--report-trx`, `--report-trx-filename`)

> **Note:** Code coverage support via `Microsoft.Testing.Extensions.CodeCoverage` will be addressed in Phase 2.

---

## 3. Current State Analysis

### 3.1 Current Testing Infrastructure

Currently, .NET MAUI uses a combination of:

1. **XHarness**: Tool for deploying and running tests on iOS/Android devices/simulators
   - Handles device discovery, app deployment, test execution
   - Produces XML test results (xUnit/NUnit format)
   - Used in CI pipelines via Cake scripts

2. **Device Test Runners**: Custom test runners in `src/TestUtils/src/DeviceTests.Runners/`
   - `HeadlessRunner` for Android and iOS
   - Integrates with XHarness test runner infrastructure
   - Based on `Microsoft.DotNet.XHarness.TestRunners.*` packages

3. **Test Frameworks**:
   - xUnit for unit tests
   - NUnit for UI tests (via Appium)

### 3.2 Prior Art: UWP Test Host Manager (Visual Studio)

Visual Studio has an internal mechanism for running tests on UWP apps via `UwpTestHostManager`, which implements `ITestRuntimeProvider`. This is a **private VS-only implementation** that uses VSTest and Test Explorer—it does not work with `dotnet test` CLI.

While not directly reusable, a few patterns from this approach are worth considering:

- **Deployer Abstraction**: Platform-specific deployment via an `IDeployer` interface
- **Connection Info Model**: Encapsulating endpoint/role/transport in a structured object
- **Lifecycle Management**: Clear launch → execute → cleanup flow

The key limitations (VS-only, Windows-only, VSTest-coupled, no CLI support) mean we need a fresh implementation for MAUI's cross-platform `dotnet test` scenario.

### 3.3 Pain Points

1. **Complex Setup**: Running device tests requires understanding XHarness, Cake scripts, and platform-specific tooling
2. **No Code Coverage**: Platform-specific code has no coverage metrics
3. **AI-Unfriendly**: The toolchain is too complex for AI agents to effectively use
4. **Inconsistent Experience**: Different commands and workflows for different platforms

---

## 4. Architecture Design

### 4.1 High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                              Developer/AI Agent                              │
│                                                                              │
│    dotnet test --project MyMauiTests.csproj -f net10.0-ios --device "iPhone 15"       │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                           dotnet test (SDK)                                  │
│                                                                              │
│  • Parses arguments                                                          │
│  • Invokes MSBuild with test targets                                         │
│  • Communicates with test host (protocol TBD - see section 4.2)             │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
                                      ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│          MSBuild Test Targets (iOS SDK / Android SDK / Windows)             │
│                                                                              │
│  • iOS/Android: Implemented in iOS SDK and Android SDK                      │
│  • MAUI workload leverages these SDK implementations                         │
│                                                                              │
│  1. ComputeAvailableDevices (reuse from dotnet run)                         │
│  2. Build test application                                                   │
│  3. DeployToDevice (reuse from dotnet run)                                  │
│  4. ComputeTestRunArguments (new target)                                    │
│  5. Execute tests and establish communication channel                        │
└─────────────────────────────────────────────────────────────────────────────┘
                                      │
              ┌───────────────────────┼───────────────────────┐
              ▼                       ▼                       ▼
    ┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
    │ iOS Simulator   │     │ Android Emulator│     │ Windows/Mac     │
    │ / Device        │     │ / Device        │     │ (Local Exe)     │
    │                 │     │                 │     │                 │
    │ ┌─────────────┐ │     │ ┌─────────────┐ │     │ ┌─────────────┐ │
    │ │ Test App    │ │     │ │ Test App    │ │     │ │ Test App    │ │
    │ │ + MTP Host  │ │     │ │ + MTP Host  │ │     │ │ + MTP Host  │ │
    │ │ + Coverage  │ │     │ │ + Coverage  │ │     │ │ + Coverage  │ │
    │ └─────────────┘ │     │ └─────────────┘ │     │ └─────────────┘ │
    └────────┬────────┘     └────────┬────────┘     └────────┬────────┘
             │                       │                       │
             └───────────────────────┼───────────────────────┘
                                     │
                        Communication Protocol (TBD)
                      ┌──────────────────────────────────┐
                      │  Options under consideration:    │
                      │  • HTTP (cross-platform)         │
                      │  • Named Pipes/IPC (local only)  │
                      │  • TCP Sockets                   │
                      │  See section 4.2 for details     │
                      └──────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                   Microsoft Testing Platform (Host)                          │
│                                                                              │
│  • Receives test results via IPushOnlyProtocol                              │
│  • Aggregates results                                                        │
│  • Collects code coverage                                                    │
│  • Outputs to console / TRX / JSON                                          │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.2 Communication Protocol

The key challenge is streaming test results from the device/simulator back to the host machine. We propose using the **IPushOnlyProtocol** interface from MTP, which is already designed for this scenario.

#### 4.2.1 Communication Requirements

The communication protocol must support three distinct execution scenarios:

1. **Physical Devices** (iOS devices, Android devices): Test app runs on hardware connected via USB/network
2. **Simulators/Emulators** (iOS Simulator, Android Emulator): Test app runs in virtualized environment on same machine
3. **Local Executables** (Windows, macOS Catalyst): Test app runs as native executable on the same machine as `dotnet test`

Each scenario has different constraints for how the test host can communicate with the running test application.

#### 4.2.2 Protocol Options Analysis

> **Note:** The final protocol choice is not yet determined. The implementation should prioritize a unified approach that works across all three scenarios where possible, falling back to platform-specific solutions only when necessary.

| Protocol | Pros | Cons | Scenario Compatibility |
|----------|------|------|------------------------|
| **Named Pipes** | Low latency, already used by MTP for `dotnet test`, bidirectional, simple setup | Not available across device/host boundary (iOS/Android devices) | ✅ Local exe, ⚠️ Simulators (varies), ❌ Physical devices |
| **HTTP/HTTPS** | Works across all boundaries, well-supported on all platforms, firewall-friendly | Higher latency, requires server on host, more overhead. iOS devices show "allow connections to local network?" dialog. HTTP requires custom entitlement on iOS (HTTPS preferred but requires certificates) | ✅ All scenarios (with caveats) |
| **Unix Domain Sockets** | Low latency, works for local processes, cross-platform (macOS/Linux) | Not available for physical devices, Windows uses different mechanism, does not work for sandboxed macOS Catalyst apps | ✅ Local exe, ⚠️ Simulators (not Catalyst), ❌ Physical devices |
| **TCP Sockets** | Cross-platform, lower overhead than HTTP | Firewall issues, port management complexity. **Note:** iOS 18+ no longer supports TCP tunneling for physical devices | ⚠️ Simulators/emulators, ❌ iOS physical devices (18+), ✅ Android |
| **USB/ADB Forward** | Low latency for Android | Platform-specific, complex setup | ❌ iOS, ✅ Android only |

#### 4.2.3 Implementation Considerations

**Option A: Unified HTTP-based Protocol**
- Use HTTP for all scenarios (devices, simulators, local executables)
- Simplest to implement and maintain
- Trade-off: slightly higher overhead for local execution

**Option B: Hybrid Protocol (IPC + HTTP)**
- Use Named Pipes/IPC for local executables (Windows, macOS Catalyst running on same machine)
- Use HTTP for devices and simulators
- Trade-off: more complex implementation, but optimal performance for each scenario

**Option C: Abstracted Transport Layer**
- Define abstract transport interface
- Implement multiple backends (Named Pipes, HTTP, TCP)
- Auto-select based on target platform and execution scenario
- Trade-off: most flexible but highest implementation complexity

**Current Recommendation:** Start with **Option A (HTTP-based)** for V1 to ensure consistent behavior across all platforms. Evaluate adding IPC optimizations for local execution in V2 if performance becomes a concern.

#### 4.2.4 Recommended Interface

The protocol surface should use platform/test-generic names because this is an SDK capability for all mobile workloads, not a MAUI-only contract.

```csharp
public interface IDeviceTestProtocol : IPushOnlyProtocol
{
    // Connection establishment
    Task<bool> ConnectAsync(Uri endpoint, CancellationToken cancellationToken);
    
    // Test lifecycle events
    Task SendTestSessionStartingAsync(TestSessionInfo session);
    Task SendTestNodeUpdateAsync(TestNodeUpdate update);
    Task SendTestSessionFinishedAsync(TestSessionResult result);
    
    // Artifacts (logs, screenshots)
    Task SendFileArtifactAsync(FileArtifact artifact);
}
```

#### 4.2.5 Platform-Specific Considerations

> **Note:** The following are potential implementation patterns. Final implementation will depend on which protocol option is chosen.

**Physical Devices (iOS/Android):**
- Test app initiates connection to host (HTTP or TCP with port forwarding)
- Host machine runs server (integrated into MTP or standalone)
- App pushes results via HTTP POST or socket write
- Consider server-sent events (SSE) or WebSocket for real-time streaming

**Simulators/Emulators:**
- May use localhost networking (Android emulator, iOS Simulator)
- Could potentially use Named Pipes or Unix Domain Sockets if running on same machine
- HTTP remains simplest cross-platform option

**Local Executables (Windows/macOS):**
- Can use existing Named Pipe infrastructure from MTP (`--server` mode)
- Or use HTTP for consistency with device scenarios
- Trade-off between performance (IPC) and simplicity (unified HTTP)

### 4.3 Transport Phasing Strategy

We recommend a phased approach for device ↔ host communication:

#### V1: File-based artifacts (most robust) - **RECOMMENDED FOR MVP**

**Mechanism:** Test app writes TRX/coverage to its sandbox; tooling pulls files back after completion.

**POC Implementation (Android):**
```xml
<PropertyGroup>
  <!-- Deterministic file names avoid trusting device-controlled directory listings. -->
  <_DeviceTrxFileName>test-results.trx</_DeviceTrxFileName>
</PropertyGroup>

<ItemGroup>
  <TestRunArguments Include="--results-directory files/TestResults" />
  <TestRunArguments Include="--report-trx" />
  <TestRunArguments Include="--report-trx-filename $(_DeviceTrxFileName)" />
</ItemGroup>

<!-- Pull the expected TRX file using run-as + cat. The basename is fixed and host-controlled. -->
<Exec Command="adb $(_AdbDevice) exec-out run-as $(ApplicationId) cat &quot;files/TestResults/$(_DeviceTrxFileName)&quot; &gt; &quot;$(TestResultsDirectory)/$(_DeviceTrxFileName)&quot;" />

<!-- Capture logcat for debugging -->
<Exec Command="adb $(_AdbDevice) logcat -d &gt; &quot;$(TestResultsDirectory)/$(ProjectName)_logcat.txt&quot;" />
```

If a platform must discover artifact names dynamically, the SDK target must validate that each discovered value is a simple basename from an allowlist or fixed pattern before composing any host command. Device/app-controlled paths must not be interpolated into shell commands.

**Why `run-as` + `cat`:**
- Works with debuggable APKs (debug builds) without root
- `run-as <package>` accesses app's private storage
- `cat` outputs file content to stdout which can be redirected locally
- Alternative `adb pull` doesn't work with app-private directories

**Pros:**
- Works even when networking is constrained (especially iOS devices)
- Simple reliability model: "run → pull files → parse"
- Aligns naturally with MTP extensions that already write to `--results-directory` / `TestResults`
- Most reliable approach based on prior experience with XHarness

**Cons:**
- No live streaming of per-test progress to host (unless also mirrored to logs)
- Requires platform-specific "pull from sandbox" implementations

**Artifact Collection:**
- TRX test results
- Console/app logs
- Crash diagnostics (`.ips` files on iOS, tombstones on Android)
- Screenshots (if applicable)

#### V2: Streamed events to host (best UX for AI + humans)

**Mechanism:** `dotnet test` starts a local "results receiver" (HTTP/WebSocket/TCP). App connects and pushes events/results.

**Pros:**
- Live progress + early failure surfacing
- Lets AI agents react mid-run

**Cons:**
- Requires connectivity + (sometimes) port-forwarding
- Must harden security (auth token, localhost binding, ephemeral ports)

**Recommendation:** V1 ships with file-based for iOS/Android/Windows. Add streaming incrementally where easiest (Android emulator + iOS simulator first), then evaluate iOS device feasibility.

### 4.4 Code Coverage Architecture

Support two complementary paths for on-device code coverage.

#### Path 1 (preferred): MTP Code Coverage Extension

Use `Microsoft.Testing.Extensions.CodeCoverage` in the test app:

```bash
dotnet test --project MyMauiDeviceTests.csproj -f net10.0-android --device emulator-5554 \
  -- --coverage --coverage-output TestResults/coverage.cobertura.xml --coverage-output-format cobertura
```

This is simplest for MAUI device tests because coverage is collected during test execution and saved to artifacts. V1 extraction should either use host-known coverage filenames or write a manifest/archive inside the app sandbox so Android can retrieve files through `run-as` without relying on `adb pull` from private directories.

#### Path 2 (advanced): Instrument + Collect in Server Mode

For scenarios where coverage collection must be separated from execution, Microsoft tooling supports:
1. Instrument a binary
2. Start collector in `--server-mode`
3. Run app
4. Shutdown collector

This is a potential future enhancement (particularly for complex native coverage scenarios), not required for v1.

#### Coverage Flow Diagram

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Build Time (MSBuild)                                 │
│                                                                              │
│  1. Enable static instrumentation: /p:AotMsCodeCoverageInstrumentation=true │
│  2. Include coverage collector in test app                                   │
│  3. Configure coverage output path                                           │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                         Runtime (On Device)                                  │
│                                                                              │
│  1. Tests execute with instrumented code                                     │
│  2. Coverage data collected in memory                                        │
│  3. On test completion, coverage file written to device storage              │
│  4. Coverage file written with a host-known name or manifest entry          │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Post-Test (Host Machine)                               │
│                                                                              │
│  1. Extract coverage via platform sandbox tooling (run-as cat/archive on Android, xcrun simctl for simulators, or equivalent) │
│  2. Merge with any host-side coverage                                        │
│  3. Generate report (Cobertura XML, HTML, etc.)                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

### 4.5 Android Execution Modes

Based on the POC implementation, Android device tests support **two execution modes**:

#### 4.5.1 Activity Mode (Default) - via `dotnet run --device`

Uses the existing `dotnet run --device` infrastructure to deploy and launch the app's `MainActivity`.

```bash
dotnet test --project MyTests.csproj -f net10.0-android -p:Device=emulator-5554
```

**Flow:**
1. `dotnet run --device` builds and deploys the APK
2. Launches `MainActivity` via `adb shell am start`
3. `MainActivity.OnCreate` calls `MicrosoftTestingPlatformEntryPoint.Main()`
4. Tests execute, results stream to logcat
5. App exits via `Java.Lang.JavaSystem.Exit(exitCode)`

**Pros:**
- Simple, leverages existing `dotnet run` infrastructure
- No additional Android SDK changes required

**Cons:**
- App may not signal completion reliably in some scenarios
- Exit code propagation depends on app termination being detected

#### 4.5.2 Instrumentation Mode - via `adb instrument`

Uses Android Instrumentation for more reliable test execution with proper wait-for-completion semantics.

```bash
dotnet test --project MyTests.csproj -f net10.0-android -p:Device=emulator-5554 -p:UseInstrumentation=true
```

**Flow:**
1. Build and install APK via `dotnet build -t:Install`
2. Launch via `adb shell am instrument -w <package>/<instrumentation-class>`
3. `-w` flag waits for instrumentation to complete
4. `TestInstrumentation.OnStart` calls `MicrosoftTestingPlatformEntryPoint.Main()`
5. Tests execute, results stream to logcat
6. `Instrumentation.Finish(exitCode, results)` signals completion

**Pros:**
- More reliable completion detection
- Deterministic completion; the SDK must parse the instrumentation result bundle or TRX to propagate the MTP exit code
- Standard Android test pattern

**Cons:**
- Requires `TestInstrumentation` class in test project
- Requires AndroidManifest.xml instrumentation registration
- **Requires Android SDK enhancement** (see below)

#### 4.5.3 Android SDK Instrumentation Integration

Instrumentation Mode should build on existing Android SDK instrumentation plumbing where available (for example MSTest runner/instrumentation properties and `Microsoft.Android.Run --instrument`) instead of duplicating it. The remaining SDK work is to pass MTP arguments losslessly, wait for completion, collect artifacts, and propagate the MTP result.

**Illustrative target shape** - exact property names should align with the Android SDK's existing instrumentation support:
```xml
<PropertyGroup Condition="'$(UseInstrumentation)' == 'true'">
  <RunCommand>$(_AdbToolPath)</RunCommand>
  <RunArguments>$(AdbTarget) shell am instrument -w -e TestRunArgumentsBase64 "$(_EncodedTestRunArguments)" "$(_AndroidPackage)/$(AndroidInstrumentationName)"</RunArguments>
</PropertyGroup>
```

The SDK target must encode `@(TestRunArguments)` losslessly (for example JSON + Base64, a response file, or separate bundle entries) instead of joining on spaces, because filters, file names, and logger arguments can contain spaces or quotes. It must also treat the instrumentation result as test execution state, not just process completion. `adb shell am instrument -w` can complete successfully even when tests fail, so the target should parse the instrumentation result bundle and/or deterministic TRX artifact and fail the MSBuild target when the MTP exit code is non-zero.

**MSBuild Properties:**

| Property | Description | Default |
|----------|-------------|---------|
| existing Android instrumentation opt-in property | Use `adb instrument` instead of activity launch | SDK-defined |
| existing or new instrumentation class property | Full instrumentation class name (e.g., `myapp.TestInstrumentation`) | SDK-defined |
| `$(DeviceTrxFileName)` | Fixed host-controlled TRX basename passed to MTP and used for extraction | `test-results.trx` |

**Action Item:** Validate the current dotnet/android instrumentation properties and add only the missing MTP argument/result/artifact plumbing.

#### 4.5.4 Test Project Requirements (Instrumentation Mode)

**1. TestInstrumentation class:**
```csharp
[Instrumentation(Name = "mypackage.TestInstrumentation")]
public class TestInstrumentation : Instrumentation
{
    string[] _testArguments = [];

    public TestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
    }

    public override void OnCreate(Bundle? arguments)
    {
        base.OnCreate(arguments);
        _testArguments = ParseTestArguments(arguments?.GetString("TestRunArgumentsBase64"));
        Start();
    }

    public override void OnStart()
    {
        base.OnStart();
        int exitCode = 1;
        Bundle results = new Bundle();

        try
        {
            exitCode = RunTestsAsync(results).GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            results.PutString("error", ex.ToString());
        }
        finally
        {
            results.PutInt("exitCode", exitCode);
            results.PutString("status", exitCode == 0 ? "SUCCESS" : "FAILURE");
            Finish(exitCode == 0 ? Result.Ok : Result.Canceled, results);
        }
    }

    private async Task<int> RunTestsAsync(Bundle results)
    {
        var testResultsDir = Path.Combine(TargetContext.FilesDir.AbsolutePath, "TestResults");
        Directory.CreateDirectory(testResultsDir);

        var args = NormalizeDevicePaths(_testArguments, testResultsDir);
        return await MicrosoftTestingPlatformEntryPoint.Main(args);
    }

    private static string[] ParseTestArguments(string? encodedArgumentsBase64)
    {
        if (string.IsNullOrWhiteSpace(encodedArgumentsBase64))
            return ["--results-directory", "TestResults", "--report-trx", "--report-trx-filename", "test-results.trx"];

        var json = Encoding.UTF8.GetString(Convert.FromBase64String(encodedArgumentsBase64));
        return JsonSerializer.Deserialize<string[]>(json) ?? [];
    }

    private static string[] NormalizeDevicePaths(string[] args, string testResultsDir)
    {
        var normalized = args.ToArray();
        for (var i = 0; i < normalized.Length - 1; i++)
        {
            if (normalized[i] == "--results-directory" && !Path.IsPathRooted(normalized[i + 1]))
                normalized[i + 1] = testResultsDir;
        }

        return normalized;
    }
}
```

**2. Android manifest registration:**

The `[Instrumentation]` attribute should generate the manifest entry in .NET Android. Avoid adding a duplicate manual `<instrumentation>` element unless the SDK explicitly requires custom manifest metadata not covered by the attribute.

---

### 5.1 New MSBuild Targets

Building on the `dotnet run` infrastructure, we need these new targets. Note: Target names should align with existing SDK conventions and avoid framework-specific prefixes.

```xml
<!-- Implemented in iOS SDK / Android SDK -->

<!-- Compute V1 file-artifact test arguments. Streaming/server mode is future work. -->
<Target Name="ComputeTestRunArguments"
        DependsOnTargets="ComputeRunArguments"
        Returns="@(TestRunArguments)">

  <PropertyGroup>
    <_DeviceTrxFileName>test-results.trx</_DeviceTrxFileName>
  </PropertyGroup>

  <ItemGroup>
    <TestRunArguments Include="--results-directory $(DeviceTestResultsPath)" />
    <TestRunArguments Include="--report-trx" />
    <TestRunArguments Include="--report-trx-filename $(_DeviceTrxFileName)" />
  </ItemGroup>
</Target>

<!-- Execute tests on device, recording the result without preventing cleanup. -->
<Target Name="ExecuteDeviceTests"
        DependsOnTargets="DeployToDevice;ComputeTestRunArguments">

  <!-- Platform-specific execution logic should set _DeviceTestExitCode. -->
  <ExecuteDeviceTests
    Device="$(Device)"
    RuntimeIdentifier="$(RuntimeIdentifier)"
    TestArguments="@(TestRunArguments)"
    ResultsDirectory="$(TestResultsDirectory)"
    Timeout="$(TestTimeout)"
    ContinueOnError="WarnAndContinue">
    <Output TaskParameter="ExitCode" PropertyName="_DeviceTestExitCode" />
  </ExecuteDeviceTests>
</Target>

<!-- Pull test artifacts from device even when test execution failed. -->
<Target Name="CollectTestArtifacts"
        AfterTargets="ExecuteDeviceTests">

  <CollectDeviceArtifacts
    Device="$(Device)"
    SourcePath="$(DeviceTestResultsPath)"
    DestinationPath="$(TestResultsDirectory)" />
</Target>

<!-- Propagate test failure only after artifacts and diagnostics are collected. -->
<Target Name="FailDeviceTestsIfNeeded"
        AfterTargets="CollectTestArtifacts"
        Condition="'$(_DeviceTestExitCode)' != '' and '$(_DeviceTestExitCode)' != '0'">
  <Error Text="Device tests failed with exit code $(_DeviceTestExitCode). See $(TestResultsDirectory) for artifacts." />
</Target>
```

A future streaming target can add `--server` / `--server-mode` only when the selected platform has an explicit, tested transport (for example, named pipes for local executables, Android emulator reverse/forwarding, or simulator loopback). It should not be part of the default V1 target chain for physical devices.

### 5.2 Property Reference

| Property | Description | Default |
|----------|-------------|---------|
| `$(Device)` | Target device identifier (from `dotnet run` spec) | Interactive selection |
| `$(CollectCoverage)` | Enable code coverage collection | `false` |
| `$(TestResultsDirectory)` | Output directory for test results | `TestResults/` |
| `$(TestTimeout)` | Maximum test execution time | `00:30:00` |
| `$(TestFilter)` | Filter expression for tests to run | (none) |
| `$(MTPServerEndpoint)` | Host endpoint for test results | Auto-assigned |

> **Note:** Properties should use existing SDK conventions where possible. Avoid introducing new prefixed properties (e.g., `MauiTestDevice`) when existing properties like `$(Device)` serve the same purpose.

### 5.3 Target Chain

The test target chain (invoked from `dotnet test`):

| Target | Description |
|--------|-------------|
| `PrepareTestApp` | Build test app |
| `DeployTestApp` | Deploy to selected device |
| `RunTestApp` | Execute tests on device |
| `CollectTestArtifacts` | Pull TRX and diagnostics from device sandbox |

---

## 6. Command-Line Interface

> **Important:** In MTP mode, `dotnet test` aligns with `dotnet run` and does **not** support the `dotnet test x.csproj` positional syntax. Use `dotnet test` (in project directory), `dotnet test --project x.csproj`, or `dotnet test --solution a.sln` instead.

### 6.1 Basic Usage

> **Note:** When target framework is not specified, the tooling will prompt for selection if multiple frameworks are available. When device is not specified, the tooling will check for booted/running devices first, then prompt for device selection if needed (similar to `dotnet run` behavior).

```bash
# Run all tests on default device (interactive mode, from project directory)
dotnet test -f net10.0-ios

# Run tests with explicit project
dotnet test --project MyMauiTests.csproj -f net10.0-ios

# Run tests on specific device
dotnet test --project MyMauiTests.csproj -f net10.0-android --device "Pixel 7 - API 35"

# List available devices (reuses dotnet run --list-devices)
dotnet test --project MyMauiTests.csproj -f net10.0-android --list-devices

# Filter tests
dotnet test --project MyMauiTests.csproj -f net10.0-ios --filter "FullyQualifiedName~Button"

# Output formats (MTP report extensions)
dotnet test --project MyMauiTests.csproj -f net10.0-ios \
  -- --report-trx --report-trx-filename test-results.trx
```

### 6.2 Non-Interactive Mode (CI/AI)

For CI pipelines and AI agents, non-interactive mode is essential:

```bash
# Non-interactive with explicit device and deterministic TRX output
dotnet test --project MyMauiTests.csproj \
  -f net10.0-ios \
  --device "iossimulator-arm64:iPhone 15" \
  --no-restore \
  --results-directory ./TestResults \
  -- --report-trx --report-trx-filename test-results.trx

# With coverage for AI agent analysis, using MTP extension switches
dotnet test --project MyMauiTests.csproj \
  -f net10.0-android \
  --device "emulator-5554" \
  --results-directory ./TestResults \
  -- --report-trx --report-trx-filename test-results.trx \
     --coverage --coverage-output TestResults/android.cobertura.xml --coverage-output-format cobertura
```

### 6.3 Command-Line Switches (New)

`dotnet test --help` must remain useful even when a mobile app cannot be launched to query device-side extensions. The SDK/CLI should surface host-known options and clearly mark device/runtime extension options that are unavailable until build or launch time.


| Switch | Description |
|--------|-----------|
| `--device <id>` | Device/simulator identifier (bypasses interactive selection) |
| `--list-devices` | List available devices for the target framework |

> **Note:** Timeout switches should reuse existing `dotnet test` timeout mechanisms where available. New switches should only be introduced if existing options are insufficient.

### 6.4 MSBuild Property-Based Usage (AI-Friendly)

Device selection via MSBuild properties (no new CLI parsing required initially):

```bash
dotnet test --project MyDeviceTests.csproj -f net10.0-android -p:Device=emulator-5554
```

### 6.5 Full TRX + Coverage Example

```bash
dotnet test --project MyMauiDeviceTests.csproj -f net10.0-android --device emulator-5554 \
  --results-directory TestResults \
  -- --report-trx --report-trx-filename test-results.trx \
     --coverage --coverage-output TestResults/maui_android.cobertura.xml --coverage-output-format cobertura
```

**Notes:**
- In MTP mode, prefer MTP report-extension switches such as `--report-trx` instead of VSTest `--logger` samples unless the CLI has explicitly mapped the option for MTP.
- MTP args should not require the legacy `--` indirection that VSTest mode uses; keep `--` as an escape hatch for parity with existing CLI patterns.
- Keep naming and output paths stable/predictable for AI agents to parse reliably. For V1 device runs, the SDK target should own artifact file names (for example `test-results.trx`) or expose MSBuild properties for them so the same value is passed to MTP and used during extraction.

### 6.6 Expected Console Output Format

Based on the POC implementation, device test output follows this format:

```
╔══════════════════════════════════════════════════════════════╗
║           Microsoft.Testing.Platform - Device Tests          ║
╠══════════════════════════════════════════════════════════════╣
║  Started: 2026-01-13 14:30:00                                ║
╚══════════════════════════════════════════════════════════════╝

▶ Running: SimpleTest_ShouldPass
✓ Passed:  SimpleTest_ShouldPass
▶ Running: AndroidPlatformTest
✓ Passed:  AndroidPlatformTest
▶ Running: StringTest_ShouldPass
✓ Passed:  StringTest_ShouldPass
▶ Running: LongRunningTest_30Seconds
✓ Passed:  LongRunningTest_30Seconds

══════════════════════════════════════════════════════════════
  Test Run Completed
  Duration: 30.28s
══════════════════════════════════════════════════════════════

Test run summary: Passed!
  total: 4
  failed: 0
  succeeded: 4
  skipped: 0
  duration: 30s 282ms
```

This output is generated by custom MTP extensions (`IDataConsumer` and `ITestSessionLifetimeHandler`) and written to Android logcat, then streamed to the console via `dotnet run --device` or captured after `adb instrument` completes.

---

### Phase 1: Foundation (2-3 weeks)

**Goal:** Basic `dotnet test` working on Android emulator and iOS simulator

**Status:** 🟡 POC demonstrates Android emulator support

**Work Items:**
1. ✅ Create file-based artifact collection for Android (TRX pull from device sandbox) - **Demonstrated in POC**
2. ✅ Implement MSBuild targets for test execution (POC uses `Directory.Build.targets`) - **Demonstrated in POC**
3. ✅ Basic console output of test results via custom `IDataConsumer` - **Demonstrated in POC**
4. ✅ Two execution modes: Activity Mode and Instrumentation Mode - **Demonstrated in POC**
5. 🔴 Integration with `dotnet run` device selection - **Requires Android SDK changes**
6. 🔴 iOS simulator support - **Not started**

**Deliverables:**
- ✅ `dotnet test` runs and reports results for Android emulator (via POC workaround)
- 🔴 `dotnet test` runs and reports results for iOS simulator
- ✅ Basic pass/fail output in console
- ✅ TRX file retrieved from device

**Blocking Item:** Android SDK needs to add `UseInstrumentation` property support to enable `dotnet run --device` to launch via instrumentation.

### Phase 2: Cross-Platform & Physical Devices (2-3 weeks)

**Goal:** Full platform support including physical devices

**Work Items:**
1. Physical device support (iOS devices, Android devices)
2. Device artifact collection (pull logs, screenshots, crash diagnostics)
3. Windows support (leverage existing named pipe infrastructure)
4. Improved error messages and diagnostics

**Deliverables:**
- All platforms supported (iOS, Android, Windows, macOS Catalyst)
- Device selection via `--device` flag
- TRX and JSON result output
- Crash diagnostic collection (.ips files on iOS)

### Phase 3: Polish & Documentation (2-3 weeks)

**Goal:** Production-ready experience

**Work Items:**
1. Performance optimization
2. Comprehensive documentation and samples
3. Error handling improvements
4. Device-test project templates

**Deliverables:**
- Documentation for customers
- Project templates
- Stable artifact naming conventions
- Clear, actionable error messages

---

## 8. Deliverables Summary

### MVP (CLI)

- `dotnet test` runs device-test projects on:
  - Android emulator
  - iOS simulator
  - Windows (local)
- Produces:
  - TRX results
  - Console/app logs
- Pulls artifacts back into host `TestResults`

### Next

- Android physical device support
- iOS physical device support (file-based)
- Crash diagnostic collection

### Future (Optional)

- Live streaming mode for real-time results
- Test Explorer integration in VS/VS Code
- **Code coverage on devices**

---

## 9. Technical Requirements

### 9.1 Test Host on Device

The test application running on the device needs to:

1. **Host MTP**: Embed Microsoft.Testing.Platform as the test execution engine
2. **Implement Push Protocol**: Send results via HTTP to host machine (V2) or write to device storage (V1)
3. **Handle Lifecycle**: Properly handle app suspension/termination
4. **Custom Extensions**: Implement device-specific reporting extensions

#### 9.1.1 MTP Extensions for Device Testing

The POC demonstrates custom MTP extensions for device-specific output:

**DeviceTestReporter (IDataConsumer):**

The snippets below focus on the device-specific logic; real extensions must also implement the current required MTP extension metadata and enablement members.

```csharp
internal sealed class DeviceTestReporter : IDataConsumer, IOutputDeviceDataProducer
{
    const string TAG = "DeviceTests";

    public Type[] DataTypesConsumed => [typeof(TestNodeUpdateMessage)];

    public async Task ConsumeAsync(IDataProducer dataProducer, IData value, CancellationToken cancellationToken)
    {
        var testNodeUpdateMessage = (TestNodeUpdateMessage)value;
        TestNodeStateProperty? nodeState = testNodeUpdateMessage.TestNode.Properties.SingleOrDefault<TestNodeStateProperty>();
        if (nodeState is null)
            return;

        switch (nodeState)
        {
            case PassedTestNodeStateProperty:
                Log.Info(TAG, $"✓ Passed: {testNodeUpdateMessage.TestNode.DisplayName}");
                break;
            case FailedTestNodeStateProperty failedState:
                Log.Info(TAG, $"✗ Failed: {testNodeUpdateMessage.TestNode.DisplayName}");
                Log.Info(TAG, $"  Error: {failedState.Exception?.Message}");
                break;
            // ... other states
        }
    }
}
```

**DeviceTestSessionHandler (ITestSessionLifetimeHandler):**
```csharp
internal sealed class DeviceTestSessionHandler : ITestSessionLifetimeHandler, IOutputDeviceDataProducer
{
    const string TAG = "DeviceTests";

    public async Task OnTestSessionStartingAsync(ITestSessionContext testSessionContext)
    {
        Log.Info(TAG, "Test session starting...");
    }

    public async Task OnTestSessionFinishingAsync(ITestSessionContext testSessionContext)
    {
        Log.Info(TAG, "Test session completed.");
    }
}
```

**Registration via TestingPlatformBuilderHook:**
```csharp
public static class DeviceTestingPlatformBuilderHook
{
    public static void AddExtensions(ITestApplicationBuilder builder, string[] args)
    {
        builder.TestHost.AddDataConsumer(sp => new DeviceTestReporter());
        builder.TestHost.AddTestSessionLifetimeHandler(sp => new DeviceTestSessionHandler());
    }
}
```

**Project file registration:**
```xml
<ItemGroup>
  <TestingPlatformBuilderHook Include="DeviceTestingExtensions">
    <DisplayName>Device Testing Extensions</DisplayName>
    <TypeFullName>MyApp.DeviceTestingPlatformBuilderHook</TypeFullName>
  </TestingPlatformBuilderHook>
</ItemGroup>
```

> **Note:** MTP generates extension methods for NuGet packages that provide MTP extensions at MSBuild build time. Consider reusing this pattern for device-specific extensions.

#### 9.1.2 Entry Point Considerations

The entry point differs by platform and execution mode:

| Platform | Mode | Entry Point |
|----------|------|-------------|
| Android | Activity | `MainActivity.OnCreate` → `MicrosoftTestingPlatformEntryPoint.Main(args)` |
| Android | Instrumentation | `TestInstrumentation.OnStart` → `MicrosoftTestingPlatformEntryPoint.Main(args)` |
| iOS | App | `AppDelegate` or custom entry point → `MicrosoftTestingPlatformEntryPoint.Main(args)` |
| Windows | Exe | Standard `Main(string[] args)` → MTP |

> **Note:** On devices, command-line arguments may need to be passed via environment variables since `args` are not provided the same way as desktop apps.

### 9.2 Host Machine Requirements

The `dotnet test` process on the host needs to:

1. **Manage Device**: Deploy app and start test execution
2. **Aggregate Results**: Collect and format test results
3. **Pull Artifacts**: Retrieve test logs from device
4. **Start Server (V2 streaming only)**: Launch an authenticated receiver when a streaming transport is explicitly selected

### 9.3 Network Considerations

V1 does not require a device-to-host network connection: tests write deterministic files in the app sandbox and the SDK pulls artifacts after the run.

For future streaming scenarios, the endpoint is platform-specific:
- iOS Simulator: host loopback is generally reachable from the simulator.
- Android Emulator: use emulator host aliases such as `10.0.2.2`, or explicit `adb reverse` / `adb forward` setup depending on direction.
- Android physical device: use `adb reverse` where available, or a constrained host LAN endpoint with authentication.
- iOS physical device: do not assume TCP tunneling or `localhost`; local-network permission, ATS/HTTPS, certificates, provisioning, trust, and USB/network tunnel availability must be designed and tested explicitly.
- MacCatalyst sandboxed apps cannot rely on Unix domain sockets; use the local executable path or a sandbox-compatible transport.

Physical-device streaming should remain out of V1 until the per-platform transport and permission model is proven.

### 9.4 Security Considerations

> **⚠️ Important:** Security modeling for the communication channel should be prioritized early in the design process, as it may significantly affect the overall architecture.

Any new communication channel (TCP, HTTP, or WebSocket) from the device back to the host introduces security risks that must be addressed:

#### 9.4.1 Threat Model

| Threat | Description | Severity |
|--------|-------------|----------|
| **Unauthorized Connection** | Malicious process connects to the test results server | Medium |
| **Data Interception** | Test results/coverage data intercepted in transit | Low-Medium |
| **Injection Attacks** | Malicious data sent to host via test results channel | Medium |
| **Denial of Service** | Flooding the results server with fake connections | Low |
| **Information Disclosure** | Test output may contain sensitive data (connection strings, tokens) | Medium |

#### 9.4.2 Mitigation Strategies

**Authentication Token:**
- Generate a unique, cryptographically random token per test session
- Pass token to device app via command-line arguments or environment variable
- Require token in all requests from device to host
- Reject connections without valid token

**Network Binding:**
- Bind HTTP/TCP server to `localhost` only (127.0.0.1/::1)
- For simulators/emulators, use loopback interfaces
- For physical devices, consider USB tunneling over network exposure

**Ephemeral Ports:**
- Use dynamically assigned ports (port 0) rather than fixed ports
- Reduces predictability for attackers
- Pass actual port to device app at runtime

**Transport Security:**
- Consider HTTPS for physical device scenarios
- For localhost scenarios, encryption may be optional (defense in depth)
- Validate certificate chain if using HTTPS

**Input Validation:**
- Validate and sanitize all data received from device
- Limit payload sizes to prevent memory exhaustion
- Use structured formats (JSON) with schema validation

#### 9.4.3 Prior Art: .NET Aspire

.NET Aspire has implemented similar device-to-host communication patterns with security considerations. Key learnings to evaluate:

- **Resource tokens**: Aspire uses bearer tokens for service-to-service auth
- **mTLS**: Mutual TLS for service mesh communication
- **OTLP endpoints**: Secure telemetry collection patterns
- **Dashboard security**: Authentication for the Aspire dashboard

**Action Item:** Engage with Aspire team to understand their security model and evaluate reuse opportunities.

#### 9.4.4 Recommended Security Architecture

```
┌─────────────────────────────────────────────────────────────────────────────┐
│                            dotnet test (Host)                                │
│                                                                              │
│  1. Generate session token: SecureRandom(32 bytes) → Base64                │
│  2. Start receiver on a platform-specific endpoint (ephemeral port)        │
│  3. Pass token + endpoint to device app via launch args/env                │
│  4. Validate Authorization: Bearer <token> on all requests                 │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     │ Token + endpoint passed at app launch
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Device/Simulator App                                │
│                                                                              │
│  1. Read token from launch args/env: --test-session-token <token>          │
│  2. Read server endpoint mapped for this platform                          │
│  3. Include header: Authorization: Bearer <token>                           │
│  4. Send test results via HTTP POST                                         │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 10. Integration Points

### 10.1 With `dotnet run` for MAUI

Reuse these targets from the `dotnet run` spec:
- `ComputeAvailableDevices`
- `DeployToDevice`
- `ComputeRunArguments`

### 10.2 With Microsoft.Testing.Platform

Coordinate with MTP on a supported public transport/adapter surface before platform SDKs depend on streaming internals. Candidate concepts to align with include:
- `IPushOnlyProtocol` - Base protocol for result streaming
- `IPushOnlyProtocolConsumer` - Data consumer for results
- `DotnetTestConnection` - Existing implementation for named pipes

If those concepts remain internal implementation details, MTP should own the transport implementation or expose a stable public adapter API for the iOS/Android SDK targets to call.

### 10.3 With Code Coverage

Integrate with `Microsoft.CodeCoverage`:
- Static instrumentation at build time
- Coverage collector on device
- Cobertura output format

---

## 11. Risks & Mitigations

| Risk | Mitigation |
|------|------------|
| **iOS physical device connectivity** | File-based artifact pull is v1; streaming is optional later |
| **Mixed VSTest + MTP solutions can be fragile** | Recommend MTP mode; document constraints and supported combinations |
| **Device networking differences between iOS and Android** | Platform-specific transport implementations with abstraction layer |
| **Simulator reset between runs** | Document expected behavior; provide cleanup options |
| **Parallel test execution support** | Defer to future phase; document as non-goal for v1 |
| **Security vulnerabilities in communication channel** | Prioritize security modeling early; use auth tokens, localhost binding, ephemeral ports; engage with Aspire team for prior art |

---

## 12. Open Questions

### 12.1 For MTP Team

1. **HTTP Server Mode**: Does MTP need modifications to support HTTP as a transport for the push protocol, or can we implement this in the platform SDK layer?

2. **Protocol Versioning**: How should we handle protocol version mismatches between the device app and host?

3. **Large Result Sets**: What's the recommended approach for tests that produce many results (e.g., 10,000+ tests)?

### 12.2 For iOS/Android SDK Teams

1. **Test Framework Support**: Should we support all frameworks (xUnit, NUnit, MSTest) from day one, or start with one?

2. **Android Instrumentation**: ✅ **Resolved in POC** - See Section 4.5 for the two-mode approach (Activity Mode and Instrumentation Mode). The `Instrumentation` type is implemented in the test project (see `TestInstrumentation.cs` in POC). **Pending:** Android SDK needs to support `UseInstrumentation` property in `dotnet run` to launch via `adb shell am instrument` instead of activity start.

3. **Crash Diagnostic Collection**: What's the best approach for collecting .ips files (iOS) and tombstones (Android) after test failures?

4. **Android SDK `dotnet run` Enhancement**: The Android SDK's `_AndroidComputeRunArguments` target (in `Microsoft.Android.Sdk.Application.targets`) needs to support a new property `UseInstrumentation` to launch test apps via `adb shell am instrument -w` instead of `adb shell am start`. This enables proper wait-for-completion semantics for long-running tests.

### 12.3 Cross-Team

1. **XHarness Timeline**: When can we fully deprecate XHarness in favor of this solution?

2. **SDK Integration**: What SDK version will include this support?

3. **Canonical Device Selection Format**: One `--device` string, or separate `--device-id` / `--device-name`?

4. **Artifact Location Inside App**: Stable `TestResults` folder mapping vs per-run GUID folder?

5. **Test Filtering**: Should we support filtering (category/trait) on-device or host-side? MTP filters that execute inside the test app may not be fully pre-filterable on the host.

6. **Security Model Review**: When should we conduct a formal security review of the communication channel? Should this block streaming features?

7. **Aspire Reuse**: What components from .NET Aspire's security model can we directly reuse vs. adapt?

8. **Mixed Solutions**: Validate solutions that contain both mobile MTP app projects and normal desktop/non-MAUI test projects so `dotnet test --solution` can dispatch each project through the correct path.

### 12.4 .NET SDK / CLI Team

1. **CLI UX Alignment**: Should `dotnet test --device` become official or stay MSBuild property-driven?

2. **MTP Mode Guidance**: Documentation for device testing in MTP mode (selected via `global.json`)

---

## 13. Work Breakdown by Team

### iOS SDK Team

1. **MSBuild targets**: Implement test target chain (build/deploy/run/collect) for iOS
2. **Artifact extraction**: Pull from simulator/device container (TRX, logs, .ips crash files)
3. **Device selection plumbing**: Canonical device id formats + mapping to existing deploy tooling
4. **Reliability**: Timeouts, retries, crash diagnostics, log capture

### Android SDK Team

1. **MSBuild targets**: Implement test target chain (build/deploy/run/collect) for Android
2. **Artifact extraction**: Pull from app sandbox (TRX, logs, tombstones)
3. **Device selection plumbing**: ADB device formats + mapping to existing deploy tooling
4. **Android Instrumentation Support**: 
   - Add `UseInstrumentation` property to `_AndroidComputeRunArguments` target
   - Add `AndroidInstrumentationName` property for specifying instrumentation class
   - Modify `RunArguments` to use `adb shell am instrument -w` when `UseInstrumentation=true`
   - See [Microsoft.Android.Sdk.Application.targets](https://github.com/dotnet/android/blob/main/src/Xamarin.Android.Build.Tasks/Microsoft.Android.Sdk/targets/Microsoft.Android.Sdk.Application.targets#L52-L79)
5. **Reliability**: Timeouts, retries, crash diagnostics, log capture

### MTP Team

1. **Runner hosting guidance for app scenarios**:
   - How to host MTP inside an app UI process
   - Lifecycle integration (app start → run tests → exit/terminate)
2. **Extensions validation on mobile**:
   - TRX extension on iOS/Android file systems
   - ✅ POC validates TRX works on Android with `--report-trx` flag
3. **MTP extension compatibility for mobile**:
   - Some extensions (CodeCoverage, Fakes) may start subprocesses which isn't allowed on mobile
   - Need to adjust extensions/core to prevent subprocess launching in mobile scenarios
   - Fakes runtime should throw exception if unsupported APIs are called on mobile
4. **Console output forwarding**:
   - Determine if `PlatformOutputDeviceManager` needs updates to forward console output from device
   - Consider `IFileSystem` updates to ensure file writes to results directory are copied from device
5. **(Optional future) Streaming protocol surface**:
   - Best practice for push-only/event streaming (server mode / custom sink)
   - Stability/versioning expectations

### MacCatalyst / Desktop Platform Owners

1. **Execution mode**: Define whether MacCatalyst follows the local executable flow or an app-container launch flow.
2. **Sandbox artifacts**: Document and implement sandbox-compatible artifact extraction for TRX, logs, and crash diagnostics.
3. **Transport constraints**: Avoid Unix domain sockets for sandboxed MacCatalyst apps unless entitlement and sandbox behavior are explicitly validated.

### .NET SDK / CLI Team (if needed)

1. **CLI UX alignment**: Decide whether `dotnet test --device` becomes official or stays MSBuild property-driven
2. **MTP mode guidance**: Docs for device testing in MTP mode (selected via `global.json`)

---

## 14. Estimation Summary

| Phase | Effort | Duration |
|-------|--------|----------|
| Phase 1: Foundation | iOS SDK + Android SDK + MTP coordination | 2-3 weeks |
| Phase 2: Cross-Platform & Physical Devices | iOS SDK + Android SDK | 2-3 weeks |
| Phase 3: Polish & Documentation | All teams | 2-3 weeks |
| **Total** | | **6-9 weeks** |

**Note**: Phases can overlap. iOS and Android SDK work can proceed in parallel. MTP team work is mostly enabling/advisory. Code coverage will be planned separately after core infrastructure is complete.

---

## 15. Success Criteria

### 15.1 Functional

**V1 / MVP (file-based):**
- [ ] `dotnet test` successfully runs tests on iOS simulator
- [ ] `dotnet test` successfully runs tests on Android emulator
- [ ] `dotnet test` successfully runs tests on Windows
- [ ] TRX and JSON output formats are written with deterministic names
- [ ] Console/app logs and crash diagnostics are copied back to the host
- [ ] Device selection via `--device` flag or `-p:Device=` works

**V2 / Future streaming:**
- [ ] Test results appear in real-time in console
- [ ] Streaming endpoint selection is correct for each supported device type

### 15.2 Performance

**V1 / MVP:**
- [ ] Test startup time ≤ current XHarness approach
- [ ] Artifact collection time is acceptable for CI and local runs

**V2 / Future streaming:**
- [ ] Result streaming latency < 100ms
- [ ] Coverage overhead < 2x test execution time

### 15.3 Developer Experience

- [ ] AI agents can run tests with single command
- [ ] Error messages are clear and actionable
- [ ] Documentation is comprehensive
- [ ] Migration from XHarness is straightforward

---

## 16. Appendix

### A. Protocol Message Examples

#### Test Session Start
```json
{
  "type": "session/start",
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "startTime": "2024-12-10T10:30:00Z",
  "device": {
    "name": "iPhone 15",
    "os": "iOS 18.0",
    "runtime": "net10.0-ios"
  }
}
```

#### Test Node Update
```json
{
  "type": "test/update",
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "testId": "MyTests.ButtonTests.ClickTest",
  "displayName": "Button click should trigger event",
  "state": "passed",
  "duration": 1523,
  "output": "Test completed successfully"
}
```

#### Coverage Data
```json
{
  "type": "artifact/file",
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "artifactType": "log",
  "filePath": "/data/user/0/com.myapp/cache/TestLogs/test.log",
  "size": 524288
}
```

### B. References

1. [Microsoft Testing Platform Source](https://github.com/microsoft/testfx/tree/main/src/Platform/Microsoft.Testing.Platform)
2. [dotnet run for MAUI Spec](https://github.com/dotnet/sdk/blob/main/documentation/specs/dotnet-run-for-maui.md)
3. [XHarness Repository](https://github.com/dotnet/xharness)
4. [Code Coverage for Native AOT](https://github.com/microsoft/codecoverage)

### C. Glossary

| Term | Definition |
|------|------------|
| MTP | Microsoft Testing Platform - the new .NET test execution engine |
| XHarness | Cross-platform test execution tool currently used by MAUI |
| Push Protocol | One-way communication where device pushes results to host |
| Device Test | Test that runs on an actual device or simulator/emulator |
| Coverage Instrumentation | Process of modifying code to track execution |

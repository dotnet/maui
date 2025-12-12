# `dotnet test` for .NET MAUI

## Overview

This document describes the design and implementation plan for enabling `dotnet test` to work seamlessly with .NET MAUI applications. The goal is to allow developers and AI agents to write and run tests on simulators, emulators, and physical devices using the familiar `dotnet test` command-line experience, powered by the Microsoft Testing Platform (MTP).

## Status

| Stage | Status |
|-------|--------|
| Design | 🟡 In Progress |
| Implementation | 🔴 Not Started |
| Testing | 🔴 Not Started |
| Documentation | 🔴 Not Started |

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

1. **Unified CLI Experience**: Enable `dotnet test` to work with .NET MAUI test projects targeting iOS, Android, macOS (Catalyst), and Windows, similar to how `dotnet run` now supports device selection.

2. **AI Agent Enablement**: Make it easy for AI coding agents to write, run, and validate tests using only command-line tools. AI agents should be able to:
   - Run tests with a single command
   - Parse test results from standard output
   - Understand which tests passed/failed
   - Get code coverage information to guide further test creation

3. **Code Coverage on Devices**: Enable collection of code coverage data for platform-specific code running on iOS, Android, and Windows devices/simulators. This addresses a major gap where we currently have no visibility into test coverage of platform code.

4. **XHarness Replacement**: Eventually replace XHarness as the test execution tool, reducing the number of tools to maintain in the ecosystem.

### 1.2 Non-Goals (Phase 1)

- Test Explorer integration in Visual Studio or VS Code (CLI-only focus)
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

### 2.2 MAUI Device Test Project Model

A MAUI device-test project is a **MAUI app that hosts MTP**. It produces an **app package** (apk/ipa/msix) that runs tests inside the real runtime.

**Key requirements:**
- Output must be an executable test app (MTP expectation)
- Include MTP runner + desired extensions via NuGet (TRX report, coverage)

**Example project file:**

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFrameworks>net10.0-android;net10.0-ios;net10.0-windows10.0.19041.0</TargetFrameworks>
    <UseMaui>true</UseMaui>

    <!-- Exe is recommended for self-hosted MTP apps (MTP itself is host-agnostic) -->
    <OutputType>Exe</OutputType>
    <IsTestProject>true</IsTestProject>

    <!-- Device-test marker -->
    <IsMauiDeviceTestProject>true</IsMauiDeviceTestProject>
  </PropertyGroup>

  <ItemGroup>
    <!-- MTP-based runners (start with MSTest runner) -->
    <PackageReference Include="MSTest" Version="*" />
    <PackageReference Include="MSTest.Sdk" Version="*" />

    <!-- Extensions -->
    <PackageReference Include="Microsoft.Testing.Extensions.TrxReport" Version="*" />
    <PackageReference Include="Microsoft.Testing.Extensions.CodeCoverage" Version="*" />
  </ItemGroup>

</Project>
```

> **Note:** Using `MSTest.Sdk` as the project SDK (instead of `Microsoft.NET.Sdk`) automatically brings in the MSTest framework, adapter, MTP runner, and common extensions (TrxReport, CodeCoverage). This is the recommended approach for MTP-based test projects. For other test frameworks like xUnit or NUnit, you'll need to add the extensions explicitly.

**MTP extensions provide:**
- TRX reporting via `Microsoft.Testing.Extensions.TrxReport` (`--report-trx`, `--report-trx-filename`)
- Code coverage via `Microsoft.Testing.Extensions.CodeCoverage` (`--coverage`, `--coverage-output`, `--coverage-output-format`)

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
│    dotnet test MyMauiTests.csproj -f net10.0-ios --device "iPhone 15"       │
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
│                    MSBuild Test Targets (MAUI Workload)                      │
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
| **HTTP/HTTPS** | Works across all boundaries, well-supported on all platforms, firewall-friendly | Higher latency, requires server on host, more overhead | ✅ All scenarios |
| **Unix Domain Sockets** | Low latency, works for local processes, cross-platform (macOS/Linux) | Not available for physical devices, Windows uses different mechanism | ✅ Local exe, ✅ Simulators, ❌ Physical devices |
| **TCP Sockets** | Cross-platform, lower overhead than HTTP | Firewall issues, port management complexity | ✅ All scenarios (with port forwarding) |
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

```csharp
public interface IMauiTestProtocol : IPushOnlyProtocol
{
    // Connection establishment
    Task<bool> ConnectAsync(Uri endpoint, CancellationToken cancellationToken);
    
    // Test lifecycle events
    Task SendTestSessionStartingAsync(TestSessionInfo session);
    Task SendTestNodeUpdateAsync(TestNodeUpdate update);
    Task SendTestSessionFinishedAsync(TestSessionResult result);
    
    // Artifacts (coverage, logs)
    Task SendFileArtifactAsync(FileArtifact artifact);
    
    // Coverage data
    Task SendCoverageDataAsync(CoverageData coverage);
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

#### V1: File-based artifacts (most robust)

**Mechanism:** Test app writes TRX/coverage to its sandbox; MAUI tooling pulls files back after completion.

**Pros:**
- Works even when networking is constrained (especially iOS devices)
- Simple reliability model: "run → pull files → parse"
- Aligns naturally with MTP extensions that already write to `--results-directory` / `TestResults`

**Cons:**
- No live streaming of per-test progress to host (unless also mirrored to logs)
- Requires platform-specific "pull from sandbox" implementations

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
dotnet test MyMauiDeviceTests.csproj -f net10.0-android --device emulator-5554 \
  -- --coverage --coverage-output TestResults/coverage.cobertura.xml --coverage-output-format cobertura
```

This is simplest for MAUI device tests because coverage is collected during test execution and saved to artifacts.

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
│  4. Coverage file path sent via protocol                                     │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                       Post-Test (Host Machine)                               │
│                                                                              │
│  1. Pull coverage file from device (via adb pull / xcrun simctl)            │
│  2. Merge with any host-side coverage                                        │
│  3. Generate report (Cobertura XML, HTML, etc.)                             │
└─────────────────────────────────────────────────────────────────────────────┘
```

---

## 5. MSBuild Integration

### 5.1 New MSBuild Targets

Building on the `dotnet run` infrastructure, we need these new targets:

```xml
<!-- Microsoft.Maui.Sdk.Tests.targets -->

<!-- Compute test-specific run arguments -->
<Target Name="ComputeTestRunArguments" 
        DependsOnTargets="ComputeRunArguments"
        Returns="@(TestRunArguments)">
  
  <PropertyGroup>
    <!-- Enable MTP server mode for device communication -->
    <_MTPServerMode Condition="'$(TargetPlatformIdentifier)' == 'ios' or '$(TargetPlatformIdentifier)' == 'android'">http</_MTPServerMode>
    <_MTPServerMode Condition="'$(TargetPlatformIdentifier)' == 'windows'">pipe</_MTPServerMode>
    
    <!-- Coverage instrumentation -->
    <AotMsCodeCoverageInstrumentation Condition="'$(CollectCoverage)' == 'true'">true</AotMsCodeCoverageInstrumentation>
  </PropertyGroup>
  
  <ItemGroup>
    <TestRunArguments Include="--server" />
    <TestRunArguments Include="--server-mode $(_MTPServerMode)" />
    <TestRunArguments Include="--coverage" Condition="'$(CollectCoverage)' == 'true'" />
    <TestRunArguments Include="--results-directory $(TestResultsDirectory)" />
  </ItemGroup>
</Target>

<!-- Execute tests on device -->
<Target Name="ExecuteDeviceTests"
        DependsOnTargets="DeployToDevice;ComputeTestRunArguments">
  
  <!-- Platform-specific execution logic -->
  <ExecuteMauiTests
    Device="$(Device)"
    RuntimeIdentifier="$(RuntimeIdentifier)"
    TestArguments="@(TestRunArguments)"
    ResultsDirectory="$(TestResultsDirectory)"
    Timeout="$(TestTimeout)"
    ServerEndpoint="$(MTPServerEndpoint)" />
</Target>

<!-- Pull test artifacts from device -->
<Target Name="CollectTestArtifacts"
        AfterTargets="ExecuteDeviceTests">
  
  <CollectDeviceArtifacts
    Device="$(Device)"
    SourcePath="$(DeviceTestResultsPath)"
    DestinationPath="$(TestResultsDirectory)"
    IncludeCoverage="$(CollectCoverage)" />
</Target>
```

### 5.2 Property Reference

| Property | Description | Default |
|----------|-------------|---------|
| `$(Device)` | Target device identifier (from `dotnet run` spec) | Interactive selection |
| `$(CollectCoverage)` | Enable code coverage collection | `false` |
| `$(TestResultsDirectory)` | Output directory for test results | `TestResults/` |
| `$(TestTimeout)` | Maximum test execution time | `00:30:00` |
| `$(TestFilter)` | Filter expression for tests to run | (none) |
| `$(MTPServerEndpoint)` | Host endpoint for test results | Auto-assigned |
| `$(IsMauiDeviceTestProject)` | Marker for MAUI device test projects | `false` |
| `$(MauiTestDevice)` | Device identifier (name/UDID/emulator id) | (none) |
| `$(MauiTestDeviceType)` | Device type: emulator/device/simulator | (auto-detected) |
| `$(MauiTestTarget)` | Target platform: android/ios/windows | (from TFM) |

### 5.3 Target Chain

The MAUI test target chain (invoked from `dotnet test`):

| Target | Description |
|--------|-------------|
| `MauiPrepareTestApp` | Build test app + instrument assemblies |
| `MauiDeployTestApp` | Deploy to selected device |
| `MauiRunTestApp` | Execute tests on device |
| `MauiCollectTestArtifacts` | Pull TRX/coverage from device sandbox |

---

## 6. Command-Line Interface

> **Important:** In MTP mode, `dotnet test` aligns with `dotnet run` and does **not** support the `dotnet test x.csproj` positional syntax. Use `dotnet test` (in project directory), `dotnet test --project x.csproj`, or `dotnet test --solution a.sln` instead.

### 6.1 Basic Usage

```bash
# Run all tests on default device (interactive mode, from project directory)
dotnet test -f net10.0-ios

# Run tests with explicit project
dotnet test --project MyMauiTests.csproj -f net10.0-ios

# Run tests on specific device
dotnet test --project MyMauiTests.csproj -f net10.0-android --device "Pixel 7 - API 35"

# Run tests with coverage
dotnet test --project MyMauiTests.csproj -f net10.0-ios --collect "Code Coverage"

# List available devices (reuses dotnet run --list-devices)
dotnet test --project MyMauiTests.csproj -f net10.0-android --list-devices

# Filter tests
dotnet test --project MyMauiTests.csproj -f net10.0-ios --filter "FullyQualifiedName~Button"

# Output formats
dotnet test --project MyMauiTests.csproj -f net10.0-ios --logger trx --logger html
```

### 6.2 Non-Interactive Mode (CI/AI)

For CI pipelines and AI agents, non-interactive mode is essential:

```bash
# Non-interactive with explicit device
dotnet test --project MyMauiTests.csproj \
  -f net10.0-ios \
  --device "iossimulator-arm64:iPhone 15" \
  --no-restore \
  --logger "console;verbosity=detailed"

# With coverage for AI agent analysis
dotnet test --project MyMauiTests.csproj \
  -f net10.0-android \
  --device "emulator-5554" \
  --collect "Code Coverage" \
  --results-directory ./coverage \
  --logger "json;LogFileName=results.json"
```

### 6.3 Command-Line Switches (New)

| Switch | Description |
|--------|-----------|
| `--device <id>` | Device/simulator identifier (bypasses interactive selection) |
| `--list-devices` | List available devices for the target framework |
| `--deploy-timeout` | Timeout for app deployment (default: 2 minutes) |
| `--test-timeout` | Timeout for test execution (default: 30 minutes) |

### 6.4 MSBuild Property-Based Usage (AI-Friendly)

Add first-class MAUI test knobs as MSBuild properties (no new CLI parsing required initially):

```bash
dotnet test --project MyMauiDeviceTests.csproj -f net10.0-android \
  -p:MauiTestTarget=Android \
  -p:MauiTestDevice=emulator-5554
```

### 6.5 Full TRX + Coverage Example

```bash
dotnet test --project MyMauiDeviceTests.csproj -f net10.0-android --device emulator-5554 \
  --results-directory TestResults \
  -- --report-trx --report-trx-filename maui_android.trx \
     --coverage --coverage-output TestResults/maui_android.cobertura.xml --coverage-output-format cobertura
```

**Notes:**
- In MTP mode, MTP args should not require the legacy `--` indirection that VSTest mode uses; keep `--` as an escape hatch for parity with existing CLI patterns.
- Keep naming and output paths stable/predictable for AI agents to parse reliably.

---

## 7. Implementation Phases

### Phase 1: Foundation (2-3 weeks)

**MAUI Team:**
1. Create HTTP-based push protocol for iOS/Android
2. Implement `IMauiTestProtocol` interface
3. Create MSBuild targets for test execution
4. Basic console output of test results

**MTP Team:**
1. Expose HTTP server mode in MTP
2. Document push-only protocol requirements
3. Ensure coverage collector works with static instrumentation

**Deliverables:**
- `dotnet test` runs and reports results for iOS simulator
- Basic pass/fail output in console

### Phase 2: Cross-Platform (2-3 weeks)

**MAUI Team:**
1. Android emulator/device support
2. Windows support (leverage existing named pipe)
3. Device artifact collection (pull logs, screenshots)
4. Integration with `dotnet run` device selection

**MTP Team:**
1. Ensure consistent behavior across protocols
2. Add device-specific diagnostics

**Deliverables:**
- All platforms supported
- Device selection via `--device` flag
- TRX and JSON result output

### Phase 3: Code Coverage (2-3 weeks)

**MAUI Team:**
1. Integrate coverage instrumentation in build
2. Pull coverage files from devices
3. Merge coverage from multiple test runs

**MTP Team:**
1. Ensure coverage works with push-only protocol
2. Support for Cobertura output format

**Deliverables:**
- Coverage reports for platform code
- Cobertura XML output for CI integration
- Coverage summary in console output

### Phase 4: Polish & XHarness Migration (3-4 weeks)

**MAUI Team:**
1. Migrate existing tests from XHarness
2. Update CI pipelines
3. Performance optimization
4. Documentation and samples

**MTP Team:**
1. Performance tuning for device scenarios
2. Error handling improvements

**Deliverables:**
- Complete replacement of XHarness in MAUI repo
- Migration guide for external users
- Comprehensive documentation

### Phase 5: AI Enablement (2-3 weeks)

**MAUI Team:**
1. Device list capability (structured output for AI parsing)
2. Simplified error reporting with actionable messages
3. Stable artifact naming conventions

**MTP Team:**
1. Coverage-driven suggestions (identify uncovered code areas)
2. Structured event stream for AI consumption

**Deliverables:**
- AI agents can reliably discover devices, run tests, parse results
- Coverage data guides AI-assisted test generation
- Documentation tailored for AI agent integration

---

## 8. Deliverables Summary

### MVP (CLI)

- `dotnet test` runs MAUI device-test projects on:
  - Android emulator
  - Windows (local)
- Produces:
  - TRX
  - coverage (optional)
- Pulls artifacts back into host `TestResults`

### Next

- iOS simulator support
- Android physical device support
- iOS physical device support (likely file-based first)

### Long-term

- Optional live streaming mode
- Evaluate replacing xharness for selected scenarios
- Enhanced outputs for AI agents (structured event stream + stable artifact naming)

---

## 9. Technical Requirements

### 9.1 Test Host on Device

The test application running on the device needs to:

1. **Host MTP**: Embed Microsoft.Testing.Platform as the test execution engine
2. **Implement Push Protocol**: Send results via HTTP to host machine
3. **Collect Coverage**: When enabled, instrument and collect coverage data
4. **Handle Lifecycle**: Properly handle app suspension/termination

```csharp
// Example: MAUI Test Application Entry Point
public static class MauiTestProgram
{
    public static async Task<int> Main(string[] args)
    {
        var builder = await TestApplication.CreateBuilderAsync(args);
        
        // Register MAUI-specific extensions
        builder.AddMauiDeviceTestHost();
        
        // Register test framework (MSTest, xUnit, NUnit)
        builder.AddMSTest();
        
        // Add coverage if enabled
        if (args.Contains("--coverage"))
        {
            builder.AddCodeCoverage();
        }
        
        // Configure push protocol
        builder.AddMauiPushProtocol(options =>
        {
            options.ServerEndpoint = GetServerEndpoint(args);
        });
        
        using var app = await builder.BuildAsync();
        return await app.RunAsync();
    }
}
```

### 9.2 Host Machine Requirements

The `dotnet test` process on the host needs to:

1. **Start Server**: Launch HTTP server to receive test results
2. **Manage Device**: Deploy app and start test execution
3. **Aggregate Results**: Collect and format test results
4. **Pull Artifacts**: Retrieve coverage files and logs from device

### 9.3 Network Considerations

For iOS simulator and Android emulator:
- Use localhost with port forwarding
- iOS: Simulator shares host network
- Android: Use `adb forward` for emulator

For physical devices:
- Devices must be on same network as host
- Consider mDNS/Bonjour for discovery
- Fallback to USB tunneling

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
│  2. Start HTTP server on localhost:0 (ephemeral port)                       │
│  3. Pass token + port to device app via launch args                        │
│  4. Validate Authorization: Bearer <token> on all requests                 │
└─────────────────────────────────────────────────────────────────────────────┘
                                     │
                                     │ Token + Port passed at app launch
                                     ▼
┌─────────────────────────────────────────────────────────────────────────────┐
│                          Device/Simulator App                                │
│                                                                              │
│  1. Read token from launch args: --test-session-token <token>              │
│  2. Read server endpoint: --test-server http://localhost:<port>            │
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

Extend these MTP interfaces:
- `IPushOnlyProtocol` - Base protocol for result streaming
- `IPushOnlyProtocolConsumer` - Data consumer for results
- `DotnetTestConnection` - Existing implementation for named pipes

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
| **Coverage limitations on mobile** | Start with managed coverage where supported; document constraints; keep "instrument+collect server mode" as an advanced path |
| **Mixed VSTest + MTP solutions can be fragile** | Recommend MTP mode; document constraints and supported combinations |
| **Device networking differences between iOS and Android** | Platform-specific transport implementations with abstraction layer |
| **Simulator reset between runs** | Document expected behavior; provide cleanup options |
| **App sandboxing impacting coverage file export** | Test artifact extraction on all platforms early in development |
| **Parallel test execution support** | Defer to future phase; document as non-goal for v1 |
| **Security vulnerabilities in communication channel** | Prioritize security modeling early; use auth tokens, localhost binding, ephemeral ports; engage with Aspire team for prior art |

---

## 12. Open Questions

### 12.1 For MTP Team

1. **HTTP Server Mode**: Does MTP need modifications to support HTTP as a transport for the push protocol, or can we implement this in the MAUI layer?

2. **Protocol Versioning**: How should we handle protocol version mismatches between the device app and host?

3. **Large Result Sets**: What's the recommended approach for tests that produce many results (e.g., 10,000+ tests)?

4. **Coverage File Size**: For large apps, coverage files can be large. Should we support streaming coverage data?

### 12.2 For MAUI Team

1. **Test Framework Support**: Should we support all frameworks (xUnit, NUnit, MSTest) from day one, or start with one?

2. **Hot Reload**: Is there value in supporting hot reload during test development?

3. **Visual Testing**: Should screenshot comparison be integrated?

### 12.3 Cross-Team

1. **XHarness Timeline**: When can we fully deprecate XHarness in favor of this solution?

2. **SDK Integration**: What SDK version will include this support?

3. **Canonical Device Selection Format**: One `--device` string, or separate `--device-id` / `--device-name`?

4. **Artifact Location Inside App**: Stable `TestResults` folder mapping vs per-run GUID folder?

5. **Test Filtering**: Should we support filtering (category/trait) on-device or host-side?

6. **MAUI Test Runner Packaging**: Template or auto-generated?

7. **Security Model Review**: When should we conduct a formal security review of the communication channel? Should this block V1 streaming features?

8. **Aspire Reuse**: What components from .NET Aspire's security model can we directly reuse vs. adapt?

### 12.4 .NET SDK / CLI Team

1. **CLI UX Alignment**: Should `dotnet test --device` become official or stay MSBuild property-driven?

2. **MTP Mode Guidance**: Documentation for MAUI device testing in MTP mode (selected via `global.json`)

---

## 13. Work Breakdown by Team

### MAUI Team

1. **MSBuild targets**: Implement `Maui*Test*` target chain (build/deploy/run/collect)
2. **Artifact extraction**:
   - Android: pull from app sandbox
   - iOS simulator/device: pull from container
   - Windows: local file path
3. **Device selection plumbing**: Canonical device id formats + mapping to existing deploy tooling
4. **Reliability**: Timeouts, retries, crash diagnostics, log capture
5. **Templates/docs**: Device-test project template (recommended for adoption)

### MTP Team

1. **Runner hosting guidance for app scenarios**:
   - How to host MTP inside an app UI process
   - Lifecycle integration (app start → run tests → exit/terminate)
2. **Extensions validation on mobile**:
   - TRX extension on iOS/Android file systems
   - CodeCoverage extension feasibility/limits on iOS/Android
3. **(Optional v2) Streaming protocol surface**:
   - Best practice for push-only/event streaming (server mode / custom sink)
   - Stability/versioning expectations

### .NET SDK / CLI Team (if needed)

1. **CLI UX alignment**: Decide whether `dotnet test --device` becomes official or stays MSBuild property-driven
2. **MTP mode guidance**: Docs for MAUI device testing in MTP mode (selected via `global.json`)

---

## 14. Estimation Summary

| Phase | MAUI Team | MTP Team | Total |
|-------|-----------|----------|-------|
| Phase 1: Foundation | 2-3 weeks | 1-2 weeks | 2-3 weeks |
| Phase 2: Cross-Platform | 2-3 weeks | 0.5-1 weeks | 2-3 weeks |
| Phase 3: Code Coverage | 2-3 weeks | 1-2 weeks | 2-3 weeks |
| Phase 4: Polish & Migration | 3-4 weeks | 1-2 weeks | 3-4 weeks |
| Phase 5: AI Enablement | 2-3 weeks | 1-2 weeks | 2-3 weeks |
| **Total** | **11-16 weeks** | **4.5-9 weeks** | **11-16 weeks** |

**Note**: Phases can overlap. MTP team work is mostly enabling/advisory after Phase 1.

---

## 15. Success Criteria

### 15.1 Functional

- [ ] `dotnet test` successfully runs tests on iOS simulator
- [ ] `dotnet test` successfully runs tests on Android emulator
- [ ] `dotnet test` successfully runs tests on Windows
- [ ] Test results appear in real-time in console
- [ ] Code coverage data is collected and reported
- [ ] TRX and JSON output formats work correctly
- [ ] Device selection via `--device` flag works

### 15.2 Performance

- [ ] Test startup time ≤ current XHarness approach
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
  "type": "coverage/data",
  "sessionId": "550e8400-e29b-41d4-a716-446655440000",
  "format": "cobertura",
  "filePath": "/data/user/0/com.myapp/cache/coverage.xml",
  "size": 1048576
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

# MAUI Dev Tools Client — Product Specification

**Version**: 1.2-draft  
**Status**: Proposal  
**Last Updated**: 2026-02-03

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Problem Statement](#2-problem-statement)
3. [Goals / Non-Goals](#3-goals--non-goals)
4. [Personas & User Journeys](#4-personas--user-journeys)
5. [Functional Requirements](#5-functional-requirements)
6. [Non-Functional Requirements](#6-non-functional-requirements)
7. [Architecture](#7-architecture)
8. [Public API Surface](#8-public-api-surface)
9. [User Experience](#9-user-experience)
10. [Telemetry & Diagnostics](#10-telemetry--diagnostics)
11. [Security Model](#11-security-model)
12. [Extensibility](#12-extensibility)
13. [Testing Strategy](#13-testing-strategy)
14. [Rollout Plan](#14-rollout-plan)
15. [Open Questions & Risks](#15-open-questions--risks)
16. [Appendix](#16-appendix)

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 1.2-draft | 2026-02-03 | Added: MSBuild integration alignment with dotnet/sdk spec, `dotnet run` pipeline integration, `--list-devices` convention, deploy step, AI agent considerations |
| 1.1-draft | 2026-02-03 | Added: Exit code standardization, Windows management, offline/proxy support, container environments, migration strategy, concurrency model, permission storage, ARM64 considerations |
| 1.0-draft | 2026-02-03 | Initial draft |

---

## 1. Executive Summary

### What It Is

**MAUI Dev Tools Client** is a unified local tool and service API that detects, installs, repairs, and automates all native dependencies required for .NET MAUI development. It exposes device and simulator helpers that human users, IDEs, and AI agents can invoke safely and consistently.

### Why It Exists

Setting up a .NET MAUI development environment is one of the most significant friction points for new and experienced developers alike. The current experience requires:

- Manual installation of Android SDK, build-tools, emulators, and system images
- Manual installation and configuration of Xcode, simulators, and runtimes on macOS
- Troubleshooting cryptic errors when components are missing or misconfigured
- No unified way for IDEs or AI agents to query environment status or automate fixes

This tool eliminates that friction by providing a single, authoritative source for environment health and automated remediation.

### Who It's For

| Persona | Primary Value |
|---------|---------------|
| MAUI Developer (Windows) | One-command setup for Android development |
| MAUI Developer (macOS) | Unified setup for Android + iOS/Mac Catalyst |
| CI Engineer | Headless, scriptable environment provisioning |
| AI Agent | Structured APIs for querying and fixing environment issues |
| IDE (VS/VS Code) | Fast, reliable environment checks with actionable fixes |

---

## 2. Problem Statement

### Current Pain Points

1. **Fragmented Tooling**: Developers must use `sdkmanager`, `avdmanager`, `xcrun`, `simctl`, and `xcode-select` separately—each with different UX patterns, output formats, and error handling.

2. **Silent Failures**: Missing or misconfigured dependencies often surface as cryptic build errors deep in MSBuild logs, not as clear diagnostic messages.

3. **No Unified "Doctor"**: Unlike Flutter's `flutter doctor`, there's no single command that checks all MAUI prerequisites and offers fixes.

4. **IDE Integration Gap**: Visual Studio and VS Code must independently implement detection and installation logic, leading to inconsistent experiences.

5. **AI Agent Blindspot**: AI coding assistants cannot reliably query environment state or propose fixes because there's no structured API.

6. **CI Complexity**: Setting up MAUI builds in CI requires extensive scripting and platform-specific knowledge.

### Impact

- High abandonment rate for new MAUI developers during setup
- Increased support burden for environment-related issues
- Duplicated effort across IDE teams
- AI agents cannot effectively assist with environment problems

---

## 3. Goals / Non-Goals

### Goals

| ID | Goal | Success Metric |
|----|------|----------------|
| G1 | Reduce MAUI setup time to under 10 minutes | Time-to-first-build < 10 min for 90% of users |
| G2 | Provide a single "doctor" command that identifies all issues | 100% coverage of common setup issues |
| G3 | Enable one-click/one-command fixes for detected issues | >80% of issues auto-fixable |
| G4 | Expose structured APIs for IDE and AI agent consumption | JSON-RPC API with stable schema |
| G5 | Support headless operation for CI environments | All commands runnable non-interactively |
| G6 | Unify Android and Apple device/simulator management | Single CLI surface for both platforms |

### Non-Goals

| ID | Non-Goal | Rationale |
|----|----------|-----------|
| NG1 | Replace `dotnet` CLI | This tool complements, not replaces, the .NET CLI |
| NG2 | Full Apple signing/provisioning management | Complex domain; integrate with existing tools instead |
| NG3 | Linux host support (MVP) | MAUI mobile development requires Windows or macOS |
| NG4 | Physical iOS device provisioning | Requires Apple Developer account; out of scope for MVP |
| NG5 | Manage Visual Studio installation | VS has its own installer; we detect, not manage |

---

## 4. Personas & User Journeys

### 4.1 MAUI Developer on Windows

**Profile**: Sarah, a .NET developer building a cross-platform app. She has Visual Studio installed but has never done Android development.

**Journey**:
1. Sarah installs the .NET MAUI workload via `dotnet workload install maui`
2. She opens her project in VS Code and sees a notification: "MAUI environment issues detected"
3. She clicks "View Details" and sees a structured list:
   - ❌ Android SDK not found
   - ❌ Android emulator not installed
   - ❌ No Android system images available
4. She clicks "Fix All" and watches a progress panel:
   - Installing Android SDK... ✓
   - Installing build-tools 34.0.0... ✓
   - Installing emulator... ✓
   - Installing system-images;android-34;google_apis;x86_64... ✓
   - Creating default AVD "Pixel_5_API_34"... ✓
5. She presses F5 and her app launches in the emulator

**Key Requirements**:
- Clear, actionable error messages
- One-click installation of all dependencies
- Progress indication for long-running operations
- No prior Android knowledge required

### 4.2 MAUI Developer on macOS

**Profile**: Marcus, building an iOS app. He has Xcode installed but outdated simulators.

**Journey**:
1. Marcus runs `maui doctor` in terminal
2. Output shows:
   ```
   ✓ .NET SDK 9.0.100
   ✓ MAUI workload 9.0.0
   ✓ Xcode 16.0 (/Applications/Xcode.app)
   ✓ Android SDK (/Users/marcus/Library/Android/sdk)
   ⚠ iOS 18.0 runtime not installed (iOS 17.4 available)
   ⚠ No iPhone 16 simulator available
   ```
3. He runs `maui doctor --fix`
4. Tool prompts: "Install iOS 18.0 runtime? (requires 8GB download) [Y/n]"
5. After confirmation, runtime installs and simulator is created

**Key Requirements**:
- Detection of Xcode and simulator state
- Clear prompts for large downloads
- Respect for user's existing configuration

### 4.3 CI Engineer

**Profile**: DevOps engineer configuring GitHub Actions for a MAUI project.

**Journey**:
1. Engineer adds step to workflow:
   ```yaml
   - name: Setup MAUI Environment
     run: |
       dotnet tool install -g Microsoft.Maui.DevTools
       maui doctor --fix --non-interactive --json > setup-report.json
   ```
2. Tool runs silently, installs missing components, outputs JSON report
3. If any unfixable issues exist, tool exits with non-zero code
4. Subsequent build step succeeds

**Key Requirements**:
- `--non-interactive` flag for unattended operation
- Deterministic exit codes
- JSON output for pipeline integration
- Idempotent execution (safe to run multiple times)

### 4.4 AI Agent Invoked from IDE

**Profile**: Copilot agent assisting a developer with a build error.

**Journey**:
1. User asks: "My Android build is failing with 'SDK not found'"
2. Agent invokes: `maui doctor --json --target android`
3. Agent receives structured response:
   ```json
   {
     "status": "unhealthy",
     "issues": [
       {
         "id": "ANDROID_SDK_MISSING",
         "severity": "error",
         "message": "Android SDK not found",
         "fixable": true,
         "fix_command": "maui android sdk install --recommended"
       }
     ]
   }
   ```
4. Agent explains the issue and asks: "Would you like me to install the Android SDK?"
5. User confirms; agent invokes fix command with permission gate
6. Agent verifies fix by re-running doctor

**Key Requirements**:
- Structured JSON output with stable schema
- Fix commands included in diagnostic output
- Permission gates for destructive operations
- Correlation IDs for tracing

---

## 5. Functional Requirements

### 5.1 Doctor Capability

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-D1 | Detect .NET SDK version and MAUI workload installation status | P0 |
| FR-D2 | Detect Android SDK location and installed components | P0 |
| FR-D3 | Detect Android build-tools, platform-tools, and emulator presence | P0 |
| FR-D4 | Detect Xcode installation (full Xcode.app, not just CLI Tools), version, and selected developer directory | P0 |
| FR-D5 | Detect installed iOS/macOS runtimes and simulators | P0 |
| FR-D6 | Detect Windows SDK installation and Developer Mode status (on Windows) | P1 |
| FR-D7 | Produce human-readable output with color-coded status (with text fallback for accessibility) | P0 |
| FR-D8 | Produce machine-readable JSON output with stable schema | P0 |
| FR-D9 | Provide `--fix` flag to automatically remediate fixable issues | P0 |
| FR-D10 | Prompt for confirmation before downloads >100MB | P0 |
| FR-D11 | Support `--platform` filter (android, ios, windows, maccatalyst) — multiple allowed | P1 |
| FR-D12 | Verify available disk space before attempting large downloads | P0 |
| FR-D13 | Support `--fix <issue-id>` for targeted fixes | P1 |
| FR-D14 | Detect multiple SDK installations and report conflicts | P1 |

### 5.2 Android Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-A1 | List connected devices and running emulators | P0 |
| FR-A2 | List installed SDK packages with version info | P0 |
| FR-A3 | Install SDK packages by name or alias (e.g., `--recommended`) | P0 |
| FR-A4 | List available AVDs | P0 |
| FR-A5 | Create AVD with specified device profile and system image | P0 |
| FR-A6 | Start AVD and wait for boot completion | P0 |
| FR-A7 | Stop running emulator | P1 |
| FR-A8 | Cold boot emulator (wipe runtime state) | P1 |
| FR-A9 | Wipe AVD data | P2 |
| FR-A10 | Stream logcat output with filtering | P1 |
| FR-A11 | Install APK to device/emulator | P1 |
| FR-A12 | Uninstall package from device/emulator | P2 |
| FR-A13 | Capture screenshot from device/emulator | P0 |

### 5.3 Apple (Xcode) Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-X1 | List available simulators with runtime/device type info | P0 |
| FR-X2 | Filter simulators by runtime, device type, or state | P0 |
| FR-X3 | Boot simulator by UDID or name | P0 |
| FR-X4 | Shutdown simulator | P0 |
| FR-X5 | Create simulator with specified runtime and device type | P0 |
| FR-X6 | Delete simulator | P2 |
| FR-X7 | List available runtimes | P0 |
| FR-X8 | Install runtime (guide user if manual steps needed) | P1 |
| FR-X9 | Capture screenshot from simulator | P0 |
| FR-X10 | Stream simulator/device logs | P1 |
| FR-X11 | Validate basic signing prerequisites (team ID, certificate presence) | P2 |
| FR-X12 | Open Simulator.app with specific device | P1 |
| FR-X13 | Handle multiple Xcode versions (detect all, use `xcode-select` default) | P1 |
| FR-X14 | Detect Xcode beta installations at `/Applications/Xcode-beta.app` | P2 |

### 5.4 Windows Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-W1 | Detect Windows SDK installation and version | P1 |
| FR-W2 | Detect Developer Mode enabled status | P0 |
| FR-W3 | Guide user to enable Developer Mode if disabled | P0 |
| FR-W4 | Detect Visual Studio installation and MAUI workload | P1 |
| FR-W5 | Detect Hyper-V availability for Android emulation | P1 |
| FR-W6 | Detect Windows App SDK dependencies | P2 |

### 5.5 Cross-Platform Screenshot

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-S1 | Unified `screenshot` command across all platforms | P0 |
| FR-S2 | Auto-detect target device if only one is available | P0 |
| FR-S3 | Support `--device` flag for explicit device selection | P0 |
| FR-S4 | Support `--output` flag for file path (default: timestamped file) | P0 |
| FR-S5 | Support `--wait` flag to delay capture | P1 |
| FR-S6 | Support `--format` flag (png, jpg) | P2 |
| FR-S7 | Return file path in JSON output | P0 |

### 5.5 Device Listing (Unified)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-DL1 | `device list` shows all available devices across platforms | P0 |
| FR-DL2 | Include device type (physical/emulator/simulator), platform, state | P0 |
| FR-DL3 | Include unique identifier (serial/UDID) for targeting | P0 |
| FR-DL4 | Support `--platform` filter | P1 |
| FR-DL5 | Support `--json` output | P0 |

---

## 6. Non-Functional Requirements

### 6.1 Reliability

| ID | Requirement |
|----|-------------|
| NFR-R1 | All operations must be idempotent (safe to retry) |
| NFR-R2 | Network failures must produce clear error messages with retry guidance |
| NFR-R3 | Partial failures during multi-step operations must leave system in consistent state |
| NFR-R4 | Tool must gracefully handle missing permissions |

### 6.2 Observability

| ID | Requirement |
|----|-------------|
| NFR-O1 | All operations must support `--verbose` flag for detailed logging |
| NFR-O2 | JSON output must include `correlation_id` for tracing |
| NFR-O3 | Long-running operations must emit progress events |
| NFR-O4 | `diagnostic-bundle` command must collect all relevant logs and state |

### 6.3 Performance

| ID | Requirement |
|----|-------------|
| NFR-P1 | `doctor` command must complete in <5s when no network calls needed |
| NFR-P2 | Device list must complete in <2s |
| NFR-P3 | Daemon mode must respond to health checks in <100ms |
| NFR-P4 | Downloaded artifacts must be cached locally |
| NFR-P5 | Read operations must use direct file parsing (not CLI wrappers) for performance |

**Performance Implementation Notes**:
- Android SDK detection: Parse `package.xml` and `source.properties` directly instead of invoking `sdkmanager --list` (which has 3-10s JVM startup time)
- iOS simulator list: Use `xcrun simctl list -j` (fast native tool)
- Fall back to CLI tools only for write operations (install, create, etc.)

### 6.4 Security

| ID | Requirement |
|----|-------------|
| NFR-S1 | Downloads must verify checksums before installation |
| NFR-S2 | Tool must never store or transmit credentials |
| NFR-S3 | Elevation must be requested explicitly with clear justification |
| NFR-S4 | AI agent calls must respect permission gates (see Security Model) |

### 6.5 Privacy

| ID | Requirement |
|----|-------------|
| NFR-PR1 | No PII in logs or telemetry |
| NFR-PR2 | File paths must be redacted in telemetry (keep structure only) |
| NFR-PR3 | Telemetry must be opt-in with clear disclosure |

### 6.6 Accessibility

| ID | Requirement |
|----|-------------|
| NFR-A1 | CLI output must work with screen readers (avoid relying solely on color) |
| NFR-A2 | All status indicators must have text equivalents |
| NFR-A3 | IDE integration must follow platform accessibility guidelines |

### 6.7 Network & Offline Support

| ID | Requirement |
|----|-------------|
| NFR-N1 | Respect system proxy settings (`HTTP_PROXY`, `HTTPS_PROXY`, `NO_PROXY`) |
| NFR-N2 | Support custom CA certificates for corporate SSL inspection |
| NFR-N3 | Provide `--offline` mode for doctor to skip network checks |
| NFR-N4 | Support `--local-source <path>` for installing from pre-downloaded artifacts |
| NFR-N5 | Large downloads must support HTTP Range requests for resumability |
| NFR-N6 | Cache downloaded installers with configurable location and size limits |

### 6.8 Container & Headless Environments

| Environment | Android Emulator | iOS Simulator | Doctor | SDK Install |
|-------------|------------------|---------------|--------|-------------|
| macOS native | ✓ | ✓ | ✓ | ✓ |
| Windows native | ✓ | — | ✓ | ✓ |
| Docker (Linux) | ✓ (with KVM) | — | ✓ | ✓ |
| Docker (macOS) | ❌ No nested virt | ❌ No CoreSimulator | Partial | Partial |
| GitHub Actions macOS | ✓ | ✓ | ✓ | ✓ |
| GitHub Actions Windows | ✓ (with HAXM) | — | ✓ | ✓ |
| Azure DevOps Hosted | ✓ | ✓ (macOS only) | ✓ | ✓ |
| WSL2 | ✓ (experimental) | — | Partial | ✓ |

When emulator/simulator unavailable:
- `maui doctor` reports capability with reason (e.g., "emulator: unavailable (virtualization disabled)")
- `maui android avd start` fails fast with `E2010` "Hardware acceleration unavailable"

---

## 7. Architecture

### 7.1 High-Level Components

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Consumers                                   │
├─────────────┬─────────────┬─────────────┬─────────────┬─────────────────┤
│  Terminal   │  VS Code    │   Visual    │  AI Agent   │   CI/CD         │
│  (Human)    │  Extension  │   Studio    │  (Copilot)  │   Pipeline      │
└──────┬──────┴──────┬──────┴──────┬──────┴──────┬──────┴────────┬────────┘
       │             │             │             │               │
       │ CLI         │ JSON-RPC    │ JSON-RPC    │ JSON-RPC      │ CLI
       │             │ (stdio)     │ (named pipe)│ (stdio)       │
       ▼             ▼             ▼             ▼               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        MAUI Dev Tools Client                            │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────┐                      │
│  │  CLI Layer  │  │  JSON-RPC   │  │  Daemon     │                      │
│  │  (Parsing)  │  │  Server     │  │  (Optional) │                      │
│  └──────┬──────┘  └──────┬──────┘  └──────┬──────┘                      │
│         │                │                │                             │
│         ▼                ▼                ▼                             │
│  ┌──────────────────────────────────────────────────────────────┐       │
│  │                      Core Services                           │       │
│  ├──────────────┬──────────────┬──────────────┬─────────────────┤       │
│  │   Doctor     │   Device     │   Artifact   │   Logging       │       │
│  │   Service    │   Manager    │   Manager    │   Service       │       │
│  └──────┬───────┴──────┬───────┴──────┬───────┴────────┬────────┘       │
│         │              │              │                │                │
│         ▼              ▼              ▼                ▼                │
│  ┌──────────────────────────────────────────────────────────────┐       │
│  │                   Platform Providers                         │       │
│  ├────────────────┬────────────────┬────────────────────────────┤       │
│  │    Android     │     Apple      │     Windows                │       │
│  │    Provider    │     Provider   │     Provider               │       │
│  └────────────────┴────────────────┴────────────────────────────┘       │
└─────────────────────────────────────────────────────────────────────────┘
                              │
                              ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        Native Toolchains                                │
├───────────────────┬───────────────────┬─────────────────────────────────┤
│  Android SDK      │  Xcode/xcrun      │  Windows SDK                    │
│  (sdkmanager,     │  (simctl,         │  (vsdevcmd)                     │
│   adb, avdmanager)│   xcode-select)   │                                 │
└───────────────────┴───────────────────┴─────────────────────────────────┘
```

### 7.2 Component Descriptions

#### CLI Layer
- Parses command-line arguments using `System.CommandLine`
- Maps commands to core service calls
- Formats output for human consumption (colors, tables, progress bars)
- Entry point: `maui` command

#### JSON-RPC Server
- Implements JSON-RPC 2.0 over multiple transports:
  - **stdio**: For AI agents and simple integrations
  - **Named pipes**: For high-performance IDE integration (Windows)
  - **Unix domain sockets**: For high-performance IDE integration (macOS)
- Stateless request/response model
- Streaming support for logs and progress

#### Daemon (Optional)
- Long-running background process for faster IDE interactions
- Maintains cached state (device list, SDK components)
- Auto-starts on first IDE request; auto-terminates after idle timeout
- Not required for CLI usage

#### Core Services

| Service | Responsibility |
|---------|----------------|
| Doctor Service | Aggregates health checks from all providers; produces unified report |
| Device Manager | Unified device/emulator/simulator listing; dispatches to providers |
| Artifact Manager | Manages downloads, caching, and verification |
| Logging Service | Structured logging with correlation IDs; diagnostic bundle export |

#### Platform Providers

| Provider | Responsibility |
|----------|----------------|
| Android Provider | Wraps `sdkmanager`, `avdmanager`, `adb`, `emulator` |
| Apple Provider | Wraps `xcrun simctl`, `xcode-select`, `xcodebuild` |
| Windows Provider | Wraps Windows SDK detection and VS build tools |

### 7.3 Data Flow: Doctor Command

```
User: maui doctor --json
         │
         ▼
    ┌─────────┐
    │   CLI   │
    └────┬────┘
         │ Parse args, determine output format
         ▼
    ┌─────────────┐
    │   Doctor    │
    │   Service   │
    └──────┬──────┘
           │ Invoke each provider's health check
           ├────────────────────────┬────────────────────────┐
           ▼                        ▼                        ▼
    ┌─────────────┐          ┌─────────────┐          ┌─────────────┐
    │   Android   │          │    Apple    │          │   Windows   │
    │   Provider  │          │   Provider  │          │   Provider  │
    └──────┬──────┘          └──────┬──────┘          └──────┬──────┘
           │                        │                        │
           ▼                        ▼                        ▼
    ┌─────────────┐          ┌─────────────┐          ┌─────────────┐
    │ sdkmanager  │          │   simctl    │          │  VS Where   │
    │    adb      │          │xcode-select │          │             │
    └─────────────┘          └─────────────┘          └─────────────┘
           │                        │                        │
           └────────────────────────┴────────────────────────┘
                                    │
                                    ▼ Aggregate results
                             ┌─────────────┐
                             │   Doctor    │
                             │   Service   │
                             └──────┬──────┘
                                    │ Format as JSON
                                    ▼
                             ┌─────────────┐
                             │   stdout    │
                             └─────────────┘
```

### 7.4 IDE Extension Integration

#### VS Code Extension

```
┌─────────────────────────────────────────────────────────────┐
│                    VS Code                                   │
├─────────────────────────────────────────────────────────────┤
│  ┌──────────────────────────────────────────────────────┐   │
│  │           MAUI Extension                              │   │
│  ├──────────────────────────────────────────────────────┤   │
│  │  • Environment Status Bar Item                        │   │
│  │  • "MAUI: Setup Environment" command                  │   │
│  │  • Problems panel integration                         │   │
│  │  • Quick Fix code actions                             │   │
│  └─────────────────────┬────────────────────────────────┘   │
│                        │                                     │
│                        │ JSON-RPC (stdio)                    │
│                        ▼                                     │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           MAUI Dev Tools Client                       │   │
│  │           (spawned as child process)                  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

**Extension Responsibilities**:
1. Spawn `maui` process with `--rpc` flag on activation
2. Send `doctor.status` request on workspace open
3. Display issues in Problems panel
4. Register "MAUI: Setup Environment" command
5. Show progress notifications during fixes

#### Visual Studio Extension

- Uses named pipes for faster communication
- Integrates with Visual Studio's environment detection UI
- Surfaces issues in Error List window
- Provides menu items in Tools > MAUI submenu

### 7.5 Daemon Mode vs On-Demand

| Aspect | On-Demand (CLI) | Daemon Mode |
|--------|-----------------|-------------|
| Startup | New process per command | Single long-running process |
| Latency | ~200ms cold start | <50ms response |
| Use Case | CI, terminal, simple scripts | IDE integration |
| State | Stateless | Cached device list, SDK state |
| Resource Usage | None when idle | ~20MB memory when idle |

**Daemon Lifecycle**:
1. IDE sends request to well-known socket/pipe
2. If no daemon running, extension spawns `maui daemon --background`
3. Daemon responds to requests
4. After 5 minutes idle (configurable via `--idle-timeout`), daemon self-terminates
5. Next request restarts daemon

**Well-Known Paths**:
- macOS: `/tmp/maui-devtools.sock` (Unix domain socket)
- Windows: `\\.\pipe\MauiDevTools` (named pipe)

### 7.6 Concurrency Model

| Aspect | Behavior |
|--------|----------|
| Read Operations | Parallelized (multiple simultaneous queries allowed) |
| Write Operations | Serialized (single writer at a time) |
| Lock File | `$TMPDIR/maui-devtools.lock` prevents duplicate daemon instances |
| Request Timeout | Requests waiting >30s for lock rejected with `E5010` |
| Progress Notifications | Buffered with backpressure; slow consumers receive "progress.dropped" notification |

**Race Condition Prevention**:
- Multiple IDE instances requesting daemon start simultaneously are handled via lock file
- First acquirer starts daemon; others connect to existing instance
- Socket/pipe creation is atomic

### 7.7 SDK Conflict Resolution

When multiple SDK installations are detected:

**Android SDK Precedence** (first match wins):
1. `ANDROID_HOME` environment variable
2. `ANDROID_SDK_ROOT` environment variable  
3. Visual Studio configured path (Windows)
4. Android Studio configured path (`~/.android/sdk` or registry)
5. Default location (`~/Library/Android/sdk` on macOS, `%LOCALAPPDATA%\Android\Sdk` on Windows)

**Conflict Handling**:
- `maui doctor` reports all detected SDKs with recommendation
- `maui config set android-sdk-path <path>` to override
- `--sdk-path` flag available on all android commands for one-off override

**Android Studio Coexistence**:
- Detect Android Studio installation
- Warn if both AS and tool would manage same SDK
- Offer to adopt AS's SDK rather than installing duplicate

### 7.8 Migration from Existing Setups

When an existing SDK is detected:

1. **Validate** existing SDK health before offering modifications
2. **Adopt or Fresh**: Offer "adopt existing" vs "install fresh" options
3. **Non-Destructive**: Never delete or modify user's existing SDK without explicit consent
4. **Override Support**: `--sdk-path` for non-standard locations

---

## 8. Public API Surface

### 8.1 CLI Commands

#### Command Hierarchy

```
maui
├── doctor                    # Check environment health
│   ├── --fix [issue-id]      # Auto-fix all or specific issue
│   ├── --platform <p>        # Filter: android, ios, windows, maccatalyst (repeatable)
│   ├── --json                # Output as JSON
│   ├── --non-interactive     # No prompts (for CI)
│   └── --offline             # Skip network checks
│
├── device
│   ├── list                  # List all devices
│   │   ├── --platform        # Filter by platform
│   │   └── --json            # Output as JSON
│   ├── screenshot            # Capture screenshot
│   │   ├── --device <id>     # Target device
│   │   ├── --output <path>   # Output file
│   │   ├── --wait <ms>       # Delay before capture
│   │   └── --format <fmt>    # png, jpg
│   └── logs                  # Stream logs from device
│       ├── --device          # Device identifier
│       └── --filter          # Filter expression
│
├── android
│   ├── sdk
│   │   ├── list              # List installed packages
│   │   ├── list-available    # List available packages
│   │   ├── install <pkg>     # Install package
│   │   └── uninstall <pkg>   # Uninstall package
│   ├── avd
│   │   ├── list              # List AVDs
│   │   ├── create            # Create AVD
│   │   │   ├── --name        # AVD name
│   │   │   ├── --device      # Device profile
│   │   │   ├── --image       # System image (prefers arm64 on Apple Silicon)
│   │   │   └── --force       # Overwrite existing
│   │   ├── start             # Start AVD
│   │   │   ├── --name        # AVD name (or --avd)
│   │   │   ├── --cold-boot   # Fresh boot
│   │   │   └── --wait        # Wait for boot
│   │   ├── stop              # Stop emulator
│   │   │   └── --device      # Emulator serial
│   │   └── delete            # Delete AVD
│   │       └── --name        # AVD name
│   └── install               # Install APK
│       ├── --device          # Target device
│       └── <apk-path>        # APK file
│
├── apple
│   ├── simulator
│   │   ├── list              # List simulators
│   │   │   ├── --runtime     # Filter by runtime
│   │   │   ├── --device-type # Filter by device type
│   │   │   └── --state       # Filter: booted, shutdown
│   │   ├── create            # Create simulator
│   │   │   ├── --name        # Simulator name
│   │   │   ├── --runtime     # Runtime identifier
│   │   │   └── --device-type # Device type identifier
│   │   ├── boot              # Boot simulator
│   │   │   └── --udid        # Simulator UDID (or --name)
│   │   ├── shutdown          # Shutdown simulator
│   │   │   └── --udid        # Simulator UDID
│   │   └── delete            # Delete simulator
│   │       └── --udid        # Simulator UDID
│   └── runtime
│       ├── list              # List runtimes
│       └── install           # Install runtime (guidance)
│           └── --version     # Runtime version
│
├── deploy                    # Deploy app to device (without running)
│   ├── --device <id>         # Target device
│   ├── --project <path>      # Project file (default: current dir)
│   ├── --configuration       # Build configuration
│   └── --framework           # Target framework
│
├── config                    # Configuration management
│   ├── list                  # List all config values
│   ├── get <key>             # Get config value
│   └── set <key> <value>     # Set config value
│
├── diagnostic-bundle         # Export diagnostic info
│   └── --output <path>       # Output zip file
│
├── daemon                    # Daemon management
│   ├── start                 # Start daemon
│   ├── stop                  # Stop daemon
│   └── status                # Check daemon status
│
└── --version                 # Show version
```

#### Global Options (Available on All Commands)

| Option | Description |
|--------|-------------|
| `--json` | Output as JSON (machine-readable) |
| `--verbose` | Enable verbose logging |
| `--non-interactive` | Disable prompts; fail if input needed |
| `--correlation-id` | Set correlation ID for tracing |

#### Exit Code Standard

All commands follow a consistent exit code scheme:

| Code | Meaning | When Used |
|------|---------|-----------|
| 0 | Success / Healthy | Operation completed successfully |
| 1 | Partial success / Issues found | `doctor` found issues; operation completed with warnings |
| 2 | Operation failed | Command failed (network error, invalid input, etc.) |
| 3 | Permission denied | Elevation required but not granted |
| 4 | User canceled | User declined confirmation prompt |
| 5 | Resource not found | Requested device/AVD/simulator not found |
| 126 | Command not executable | Binary not found or not executable |
| 127 | Command not found | Unknown subcommand |

#### Command Table

| Command | Description | Inputs | Output | Exit Codes |
|---------|-------------|--------|--------|------------|
| `doctor` | Check environment health | `--fix`, `--platform`, `--json` | Status report | 0=healthy, 1=issues, 2=error |
| `device list` | List available devices | `--platform`, `--json` | Device list | 0=success, 2=error |
| `device screenshot` | Capture screenshot | `--device`, `--output`, `--wait` | File path | 0=success, 5=no device, 2=error |
| `android sdk list` | List SDK packages | `--json` | Package list | 0=success, 2=error |
| `android sdk install` | Install SDK package | `<package>`, `--accept-licenses` | Progress, result | 0=success, 5=not found, 2=error |
| `android avd create` | Create emulator | `--name`, `--device`, `--image` | AVD name | 0=success, 1=exists, 2=error |
| `android avd start` | Start emulator | `--name`, `--wait`, `--cold-boot` | Device serial | 0=success, 5=not found, 2=error |
| `apple simulator list` | List simulators | `--runtime`, `--device-type`, `--state` | Simulator list | 0=success, 2=error |
| `apple simulator boot` | Boot simulator | `--udid` or `--name` | UDID | 0=success, 5=not found, 2=error |
| `apple runtime list` | List iOS runtimes | `--json` | Runtime list | 0=success, 2=error |
| `config set` | Set configuration | `<key>`, `<value>` | Confirmation | 0=success, 2=error |

### 8.2 JSON-RPC API

**Transport**: JSON-RPC 2.0 over stdio (primary) or named pipes/Unix sockets (daemon mode)

**Transport Details**:
- **stdio mode**: Newline-delimited JSON (NDJSON) — one JSON object per line, requests on stdin, responses on stdout
- **Named pipes** (Windows): `\\.\pipe\MauiDevTools`
- **Unix sockets** (macOS): `/tmp/maui-devtools.sock`
- **Line protocol**: Each message is a complete JSON object followed by `\n`
- **Encoding**: UTF-8

**Why JSON-RPC**: 
- Language-agnostic (works with any IDE/agent)
- Simple request/response model
- Well-defined error handling
- Supports notifications for streaming (logs, progress)
- Familiar to IDE developers (same as LSP)

#### Method Reference

##### doctor.status

Check environment health.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "doctor.status",
  "params": {
    "platforms": ["android", "ios"]
  }
}
```

**Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "correlation_id": "abc-123",
    "status": "unhealthy",
    "checks": [
      {
        "category": "dotnet",
        "name": ".NET SDK",
        "status": "ok",
        "details": { "version": "9.0.100", "path": "/usr/local/share/dotnet" }
      },
      {
        "category": "dotnet",
        "name": "MAUI Workload",
        "status": "ok",
        "details": { "version": "9.0.0" }
      },
      {
        "category": "android",
        "name": "Android SDK",
        "status": "error",
        "message": "Android SDK not found",
        "issue": {
          "id": "ANDROID_SDK_MISSING",
          "severity": "error",
          "fixable": true,
          "fix": {
            "method": "doctor.fix",
            "params": { "issue_id": "ANDROID_SDK_MISSING" }
          }
        }
      }
    ],
    "summary": {
      "total": 8,
      "ok": 5,
      "warning": 1,
      "error": 2
    }
  }
}
```

##### doctor.fix

Fix a specific issue or all fixable issues.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "doctor.fix",
  "params": {
    "issue_id": "ANDROID_SDK_MISSING",
    "confirm": true
  }
}
```

**Response** (streaming progress via notifications):
```json
{
  "jsonrpc": "2.0",
  "method": "progress",
  "params": {
    "correlation_id": "abc-123",
    "operation": "android.sdk.install",
    "status": "running",
    "message": "Downloading Android SDK...",
    "percent": 45
  }
}
```

**Final Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "result": {
    "correlation_id": "abc-123",
    "status": "success",
    "fixed": ["ANDROID_SDK_MISSING"]
  }
}
```

##### device.list

List all available devices.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "device.list",
  "params": {
    "platform": "android"
  }
}
```

**Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "result": {
    "devices": [
      {
        "id": "emulator-5554",
        "name": "Pixel_5_API_34",
        "platform": "android",
        "type": "emulator",
        "state": "online",
        "os_version": "14",
        "details": {
          "avd_name": "Pixel_5_API_34",
          "api_level": 34
        }
      },
      {
        "id": "R58M32XXXXX",
        "name": "Galaxy S21",
        "platform": "android",
        "type": "physical",
        "state": "online",
        "os_version": "13"
      }
    ]
  }
}
```

##### device.screenshot

Capture screenshot from device.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "device.screenshot",
  "params": {
    "device_id": "emulator-5554",
    "output_path": "/tmp/screenshot.png",
    "wait_ms": 500
  }
}
```

**Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "result": {
    "path": "/tmp/screenshot.png",
    "width": 1080,
    "height": 2400,
    "format": "png"
  }
}
```

##### android.sdk.list

List installed Android SDK packages.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "method": "android.sdk.list",
  "params": {}
}
```

**Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 5,
  "result": {
    "sdk_path": "/Users/dev/Library/Android/sdk",
    "packages": [
      {
        "path": "build-tools;34.0.0",
        "version": "34.0.0",
        "description": "Android SDK Build-Tools 34",
        "location": "build-tools/34.0.0"
      },
      {
        "path": "platforms;android-34",
        "version": "2",
        "description": "Android SDK Platform 34",
        "location": "platforms/android-34"
      }
    ]
  }
}
```

##### android.sdk.install

Install Android SDK package.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 6,
  "method": "android.sdk.install",
  "params": {
    "packages": ["platforms;android-34", "build-tools;34.0.0"],
    "accept_licenses": true
  }
}
```

##### android.avd.create

Create Android Virtual Device.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 7,
  "method": "android.avd.create",
  "params": {
    "name": "Pixel_5_API_34",
    "device": "pixel_5",
    "system_image": "system-images;android-34;google_apis;x86_64",
    "force": false
  }
}
```

##### android.avd.start

Start Android emulator.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "method": "android.avd.start",
  "params": {
    "name": "Pixel_5_API_34",
    "wait_for_boot": true,
    "cold_boot": false,
    "timeout_seconds": 120
  }
}
```

**Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 8,
  "result": {
    "device_id": "emulator-5554",
    "state": "online",
    "boot_time_ms": 45000
  }
}
```

##### apple.simulator.list

List iOS simulators.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "method": "apple.simulator.list",
  "params": {
    "runtime": "iOS 18.0",
    "device_type": "iPhone",
    "state": "booted"
  }
}
```

**Response**:
```json
{
  "jsonrpc": "2.0",
  "id": 9,
  "result": {
    "simulators": [
      {
        "udid": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
        "name": "iPhone 16 Pro",
        "state": "booted",
        "runtime": {
          "identifier": "com.apple.CoreSimulator.SimRuntime.iOS-18-0",
          "name": "iOS 18.0",
          "version": "18.0"
        },
        "device_type": {
          "identifier": "com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro",
          "name": "iPhone 16 Pro"
        }
      }
    ]
  }
}
```

##### apple.simulator.boot

Boot iOS simulator.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 10,
  "method": "apple.simulator.boot",
  "params": {
    "udid": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890"
  }
}
```

##### apple.simulator.create

Create iOS simulator.

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 11,
  "method": "apple.simulator.create",
  "params": {
    "name": "Test iPhone 16",
    "device_type": "com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro",
    "runtime": "com.apple.CoreSimulator.SimRuntime.iOS-18-0"
  }
}
```

##### logs.stream

Stream logs from device (uses JSON-RPC notifications).

**Request**:
```json
{
  "jsonrpc": "2.0",
  "id": 12,
  "method": "logs.stream",
  "params": {
    "device_id": "emulator-5554",
    "filter": "Microsoft.Maui"
  }
}
```

**Notifications** (streamed):
```json
{
  "jsonrpc": "2.0",
  "method": "logs.entry",
  "params": {
    "stream_id": "log-123",
    "timestamp": "2026-02-03T17:45:00.123Z",
    "level": "info",
    "tag": "Microsoft.Maui.Controls",
    "message": "Application started"
  }
}
```

#### State Machine for Long-Running Operations

```
                    ┌─────────┐
                    │ Queued  │
                    └────┬────┘
                         │ start
                         ▼
    ┌────────────────────────────────────────┐
    │              Running                    │
    │  • Emits progress notifications        │
    │  • Can be canceled                     │
    └───────┬─────────────┬─────────────┬────┘
            │             │             │
         success       failure       cancel
            │             │             │
            ▼             ▼             ▼
    ┌───────────┐  ┌───────────┐  ┌───────────┐
    │ Succeeded │  │  Failed   │  │ Canceled  │
    └───────────┘  └───────────┘  └───────────┘
```

**Progress Notification Schema**:
```json
{
  "jsonrpc": "2.0",
  "method": "progress",
  "params": {
    "correlation_id": "string",
    "operation_id": "string",
    "status": "queued | running | succeeded | failed | canceled",
    "message": "string",
    "percent": 0-100,
    "bytes_downloaded": 123456,
    "bytes_total": 1000000,
    "eta_seconds": 30
  }
}
```

### 8.3 Capabilities Model

| Command | Windows | macOS | Requires Elevation |
|---------|---------|-------|-------------------|
| `doctor` | ✓ | ✓ | No |
| `doctor --fix` | ✓ | ✓ | Sometimes* |
| `device list` | ✓ | ✓ | No |
| `device screenshot` | ✓ | ✓ | No |
| `android sdk list` | ✓ | ✓ | No |
| `android sdk install` | ✓ | ✓ | No |
| `android avd create` | ✓ | ✓ | No |
| `android avd start` | ✓ | ✓ | No |
| `android logcat` | ✓ | ✓ | No |
| `apple simulator list` | — | ✓ | No |
| `apple simulator boot` | — | ✓ | No |
| `apple simulator create` | — | ✓ | No |
| `apple runtime list` | — | ✓ | No |
| `apple runtime install` | — | ✓ | Yes (admin) |
| `logs stream` | ✓ | ✓ | No |

*Elevation required for: installing Android SDK to system locations, installing Xcode runtimes

---

## 9. User Experience

### 9.1 IDE UI Flows

#### VS Code: Environment Status

**Status Bar Item** (always visible):
```
$(check-circle) MAUI Ready          ← Green when healthy
$(warning) MAUI: 2 issues          ← Yellow with issue count
$(error) MAUI: Setup Required      ← Red when critical
```

**Click Action**: Opens "MAUI Environment" panel

#### VS Code: Environment Panel

```
┌────────────────────────────────────────────────────────────┐
│  MAUI ENVIRONMENT                              [Refresh]   │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  .NET SDK                                                  │
│  ├── ✓ .NET SDK 9.0.100                                   │
│  └── ✓ MAUI Workload 9.0.0                                │
│                                                            │
│  Android                                                   │
│  ├── ✓ Android SDK (/Users/dev/Android/sdk)              │
│  ├── ✓ Build Tools 34.0.0                                 │
│  ├── ⚠ Emulator not running                    [Start]    │
│  └── ✓ AVD: Pixel_5_API_34                                │
│                                                            │
│  iOS / macOS (Xcode 16.0)                                 │
│  ├── ✓ iOS 18.0 Runtime                                   │
│  ├── ✓ iPhone 16 Pro Simulator                            │
│  └── ⚠ macOS 15.0 Runtime missing             [Install]   │
│                                                            │
│  ─────────────────────────────────────────────────────────│
│  [Fix All Issues]                                          │
└────────────────────────────────────────────────────────────┘
```

**States**:
- ✓ Green checkmark: Component healthy
- ⚠ Yellow warning: Non-critical issue, fixable
- ✖ Red X: Critical issue, blocks development
- [Action]: Inline fix button

#### VS Code: Fix Progress

```
┌────────────────────────────────────────────────────────────┐
│  Setting up MAUI Environment                               │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  ✓ Installing Android SDK                                  │
│  ✓ Installing build-tools;34.0.0                          │
│  ● Installing system-images;android-34;google_apis;x86_64 │
│    ████████████░░░░░░░░░░░░░░░░░░░░░░  45% (1.2 GB/2.6 GB)│
│  ○ Creating AVD Pixel_5_API_34                            │
│  ○ Verifying setup                                        │
│                                                            │
│  ─────────────────────────────────────────────────────────│
│  [Cancel]                                      ETA: 3 min  │
└────────────────────────────────────────────────────────────┘
```

#### Visual Studio: Tools Menu Integration

```
Tools
├── MAUI
│   ├── Check Environment...         Ctrl+Shift+M, E
│   ├── Fix Environment Issues...    Ctrl+Shift+M, F
│   ├── ─────────────────────────
│   ├── Android
│   │   ├── SDK Manager...
│   │   ├── AVD Manager...
│   │   └── Device Log...
│   ├── iOS Simulators...
│   └── ─────────────────────────
│   └── Diagnostic Bundle...
```

### 9.2 Interactive Prompting

When running in interactive mode (terminal), the tool prompts for missing information:

**Example: AVD Creation with Missing Parameters**:
```
$ maui android avd create

? AVD name: My_Pixel_5

? Select device profile:
  ❯ Pixel 5 (1080x2340, 440dpi)
    Pixel 6 (1080x2400, 411dpi)
    Pixel 7 Pro (1440x3120, 512dpi)
    (more...)

? Select system image:
  ❯ android-34 | Google APIs | x86_64 (recommended)
    android-34 | Google Play | x86_64
    android-33 | Google APIs | x86_64
    (more...)

Creating AVD 'My_Pixel_5'... done
```

**Non-Interactive Mode**:
```
$ maui android avd create --non-interactive
Error: --name is required in non-interactive mode
```

**Example: Large Download Confirmation**:
```
$ maui doctor --fix

The following will be installed:
  • system-images;android-34;google_apis;x86_64 (2.6 GB)
  • iOS 18.0 Runtime (8.1 GB)

Total download size: 10.7 GB

? Proceed? [Y/n]
```

---

## 10. Telemetry & Diagnostics

### 10.1 Telemetry Events

| Event | Data Collected | Purpose |
|-------|----------------|---------|
| `command.invoked` | Command name, flags used, duration | Understand usage patterns |
| `doctor.result` | Issue counts by category (no specifics) | Track environment health |
| `fix.attempted` | Issue type, success/failure | Measure fix effectiveness |
| `error.occurred` | Error code, category (no stack traces) | Identify common failures |

### 10.2 Opt-In / Opt-Out

```
# Check telemetry status
$ maui telemetry status
Telemetry is currently: DISABLED

# Enable telemetry
$ maui telemetry enable

# Disable telemetry
$ maui telemetry disable
```

**Default**: Telemetry is **OFF** by default. First run prompts:
```
? Help improve MAUI Dev Tools by sending anonymous usage data? [y/N]
```

### 10.3 Redaction Rules

All telemetry and logs follow these redaction rules:

| Data Type | Rule | Example |
|-----------|------|---------|
| File paths | Replace with `<path>` or keep structure | `/Users/name/code` → `<user-home>/code` |
| Device names | Replace with type | `John's iPhone` → `<iPhone>` |
| Serial numbers | Redact | `R58M32XXXXX` → `<android-serial>` |
| UDIDs | Redact | `A1B2C3...` → `<simulator-udid>` |
| Error messages | Keep, unless contains path | Preserved |

### 10.4 Diagnostic Bundle

```
$ maui diagnostic-bundle --output ~/Desktop/maui-diag.zip

Collecting diagnostics...
  • Environment info
  • Tool configuration
  • Recent command history (redacted)
  • Android SDK state
  • Xcode/simulator state
  • .NET SDK info

Saved to: /Users/dev/Desktop/maui-diag.zip (2.3 MB)
```

**Bundle Contents**:
```
maui-diag/
├── environment.json      # OS, tool version, paths
├── doctor-report.json    # Full doctor output
├── android/
│   ├── sdk-packages.json
│   └── avd-list.json
├── apple/
│   ├── simulators.json
│   └── runtimes.json
├── dotnet/
│   ├── sdk-info.json
│   └── workloads.json
└── logs/
    └── recent-commands.log  # Last 50 commands (redacted)
```

---

## 11. Security Model

### 11.1 Principle of Least Privilege

| Action | Privilege Level | Justification |
|--------|-----------------|---------------|
| Query status | User | Read-only operations |
| Install SDK to user directory | User | Default SDK location |
| Install SDK to system directory | Admin | System-wide installation |
| Create/start emulator | User | User-space operation |
| Install Xcode runtime | Admin | System component |
| Capture screenshot | User | Device already authorized |

### 11.2 Download Verification

All downloads are verified before installation:

1. **HTTPS only**: All downloads use HTTPS
2. **Checksum verification**: SHA-256 checksums verified against known-good values
3. **Signature verification**: Where available (Android SDK, Xcode components)
4. **Source allowlist**: Only download from:
   - `dl.google.com` (Android SDK)
   - `developer.apple.com` (Xcode components)
   - `aka.ms` / `dotnetcli.azureedge.net` (.NET components)

### 11.3 AI Agent Permission Gates

When invoked by an AI agent, certain operations require explicit user confirmation:

| Operation | Permission Required | Confirmation UI |
|-----------|---------------------|-----------------|
| `doctor.status` | None | — |
| `device.list` | None | — |
| `device.screenshot` | `device.capture` | IDE prompt |
| `doctor.fix` | `environment.modify` | IDE prompt with details |
| `android.sdk.install` | `environment.modify` | IDE prompt with package list |
| `android.avd.create` | `device.create` | IDE prompt |
| `logs.stream` | `device.logs` | IDE prompt |

**Permission Flow**:
```
┌─────────────────────────────────────────────────────────────┐
│  AI Agent Request                                           │
│  "Install Android SDK build-tools"                         │
├─────────────────────────────────────────────────────────────┤
│                                                             │
│  The AI assistant wants to install:                         │
│    • build-tools;34.0.0 (52 MB)                            │
│                                                             │
│  This will modify your Android SDK installation.            │
│                                                             │
│  [Allow]  [Allow Once]  [Deny]                             │
│                                                             │
│  □ Remember this choice for this session                   │
└─────────────────────────────────────────────────────────────┘
```

### 11.4 Sandbox Boundaries

AI agent calls are sandboxed:
- Cannot access arbitrary file system paths
- Cannot execute arbitrary commands
- Limited to defined API surface
- All actions logged with agent identifier

**Safe Paths** (agents may read/write only within):
- Project root directory
- Temp directory (`$TMPDIR` / `%TEMP%`)
- SDK directories (Android SDK, Xcode Developer directory)
- Tool cache directory (`~/.maui-devtools/`)

**Path Validation**:
- Reject paths containing `..`
- Reject absolute paths outside safe paths
- Validate in CLI Layer before dispatching to services

### 11.5 Permission Storage

| Context | Storage Location | Lifetime |
|---------|------------------|----------|
| IDE Session | In-memory | Until IDE closes |
| Terminal | Per-command | Single invocation |
| CI | Environment variable `MAUI_DEVTOOLS_ALLOW_MODIFY=1` | Pipeline run |
| Persistent | `~/.config/maui-devtools/permissions.json` | Until revoked |

**Persistent Permission Schema**:
```json
{
  "schema_version": 1,
  "agents": {
    "github-copilot": {
      "environment.modify": "allow",
      "device.create": "prompt",
      "device.capture": "allow"
    },
    "default": {
      "environment.modify": "prompt",
      "device.create": "prompt",
      "device.capture": "prompt"
    }
  }
}
```

**Permission Values**:
- `allow`: Permitted without prompt
- `prompt`: Requires user confirmation each time
- `deny`: Blocked (agent receives permission error)

### 11.6 Elevation Handling

When an operation requires elevation:

| Platform | Mechanism | User Experience |
|----------|-----------|-----------------|
| macOS | `sudo` prompt in terminal; Authorization Services in GUI | System password dialog |
| Windows | UAC prompt | Standard elevation dialog |

**For AI Agents**: Elevated operations cannot be auto-approved. Agent receives error `E5001` with message explaining elevation requirement. User must run command manually or through IDE's privileged execution path.

---

## 12. Extensibility

### 12.1 Alignment with `dotnet run` Pipeline

This tool is designed to complement and integrate with the `dotnet run` extensibility spec ([dotnet/sdk#51337](https://github.com/dotnet/sdk/pull/51337)). The SDK spec introduces MSBuild targets that workloads can implement for device discovery and deployment.

**Key MSBuild Integration Points**:

| MSBuild Target | SDK Responsibility | MAUI DevTools Role |
|----------------|-------------------|-------------------|
| `ComputeAvailableDevices` | Called by `dotnet run` to get device list | DevTools can invoke same target OR provide faster cached results |
| `DeployToDevice` | Called by `dotnet run` after build | DevTools wraps this for standalone deploy scenarios |
| `ComputeRunArguments` | Sets `$(RunCommand)` and `$(RunArguments)` | DevTools uses these for consistent launch behavior |

**Device Item Schema** (aligned with SDK spec):

```xml
<ItemGroup>
  <!-- Android examples -->
  <Devices Include="emulator-5554"  Description="Pixel 7 - API 35" Type="Emulator" Status="Online" />
  <Devices Include="0A041FDD400327" Description="Pixel 7 Pro"      Type="Device"   Status="Online" />
  <!-- iOS examples -->
  <Devices Include="FBF5DCE8-EE2B-4215-8118-3A2190DE1AD7" Description="iPhone 14 - iOS 18.0" Type="Simulator" Status="Booted" />
  <Devices Include="AF40CC64-2CDB-5F16-9651-86BCDF380881" Description="My iPhone 15"         Type="Device"    Status="Paired" />
</ItemGroup>
```

**Metadata Specification**:

| Metadata | Required | Values | Description |
|----------|----------|--------|-------------|
| `Description` | Yes | Free text | Human-readable device name |
| `Type` | Yes | `Device`, `Emulator`, `Simulator` | Device category |
| `Status` | Yes | `Online`, `Offline`, `Booted`, `Shutdown`, `Paired`, `Unavailable` | Current state |
| `Platform` | No | `android`, `ios`, `maccatalyst`, `windows` | Inferred from TFM if not specified |
| `OSVersion` | No | Version string | e.g., "14", "18.0" |

### 12.2 Interactive Prompting Alignment

Following the SDK spec pattern, MAUI DevTools supports interactive prompting with graceful non-interactive fallback:

**Interactive Mode** (terminal with TTY):
```
$ maui device list
? Select target framework:
  ❯ net10.0-android
    net10.0-ios
    net10.0-maccatalyst
    net10.0-windows10.0.19041.0

? Select device:
  ❯ emulator-5554 (Pixel 7 - API 35) [Online]
    0A041FDD400327 (Pixel 7 Pro) [Online]
```

**Non-Interactive Mode** (CI, piped, `--non-interactive`):
```
$ maui device list --non-interactive
Error: Multiple target frameworks available. Use --platform to specify:
  --platform android
  --platform ios
  --platform maccatalyst
  --platform windows

$ maui device screenshot --non-interactive
Error: Multiple devices available. Use --device to specify:
  --device emulator-5554  # Pixel 7 - API 35 [Online]
  --device 0A041FDD400327 # Pixel 7 Pro [Online]
```

### 12.3 `--list-devices` Convention

Aligned with `dotnet run --list-devices`, this tool provides:

```bash
# List devices for current project context
maui device list

# List devices for specific platform
maui device list --platform android

# Equivalent to dotnet run --list-devices (when in project directory)
dotnet run --list-devices
```

**Output Format** (aligned with SDK expectations):

```
Available devices for net10.0-android:

  ID                 DESCRIPTION           TYPE       STATUS
  emulator-5554      Pixel 7 - API 35      Emulator   Online
  0A041FDD400327     Pixel 7 Pro           Device     Online

Run with: dotnet run --device <ID>
      or: maui device screenshot --device <ID>
```

### 12.4 Deploy Step Support

The SDK spec introduces a `deploy` step in the `dotnet run` pipeline. MAUI DevTools provides standalone access:

```bash
# Deploy without running (useful for testing)
maui deploy --device emulator-5554

# This invokes the DeployToDevice MSBuild target
```

**JSON-RPC Method**:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "deploy",
  "params": {
    "project_path": "/path/to/MyApp.csproj",
    "device_id": "emulator-5554",
    "configuration": "Debug",
    "framework": "net10.0-android"
  }
}
```

### 12.5 AI Agent Considerations

The SDK spec explicitly mentions AI agents:

> "This has become more relevant in the AI era, as someone is going to expect AIs in 'agent mode' to build and run their app."

MAUI DevTools is designed with AI agents as first-class consumers:

| Requirement | How DevTools Addresses It |
|-------------|---------------------------|
| Structured output | All commands support `--json` with stable schema |
| Discoverability | `--list-devices` provides machine-parseable device list |
| Non-interactive | `--non-interactive` mode with helpful error messages |
| Deterministic | Same inputs produce same outputs; idempotent operations |
| Permission gates | Explicit confirmation for modifications (see §11.3) |

**AI Agent Workflow Example**:

```python
# 1. Discover available devices
devices = run("maui device list --platform android --json")

# 2. Select appropriate device (first online emulator)
device = next(d for d in devices if d["status"] == "Online" and d["type"] == "Emulator")

# 3. Build and deploy (with user confirmation via IDE)
run(f"maui deploy --device {device['id']} --project ./MyApp.csproj")

# 4. Capture screenshot for verification
run(f"maui device screenshot --device {device['id']} --output ./screenshot.png")
```

### 12.6 Plugin Model for Platform Providers

New platform providers can be added by implementing `IPlatformProvider`:

```csharp
public interface IPlatformProvider
{
    string PlatformId { get; }  // e.g., "android", "ios", "windows"
    
    Task<HealthCheckResult> CheckHealthAsync(CancellationToken ct);
    Task<IReadOnlyList<Issue>> GetIssuesAsync(CancellationToken ct);
    Task<FixResult> FixIssueAsync(string issueId, CancellationToken ct);
    Task<IReadOnlyList<Device>> GetDevicesAsync(CancellationToken ct);
    
    // New: MSBuild target integration
    bool HasMSBuildTarget(string targetName);
    Task<MSBuildResult> InvokeMSBuildTargetAsync(string targetName, IDictionary<string, string> properties, CancellationToken ct);
}
```

Providers are discovered via assembly scanning or explicit registration.

### 12.7 Adding New Subcommands

New commands are added by:
1. Creating a new `Command` class with `System.CommandLine`
2. Implementing the handler using core services
3. Registering the command in the root command builder
4. Adding corresponding JSON-RPC method if needed

### 12.8 Versioning Strategy

**CLI Versioning**:
- Follows SemVer: `MAJOR.MINOR.PATCH`
- MAJOR: Breaking changes to CLI arguments or behavior
- MINOR: New commands or options
- PATCH: Bug fixes

**API Versioning**:
- JSON-RPC methods include version in response
- Schema version included in all JSON output
- Breaking changes require new method names (e.g., `doctor.status` → `doctor.status.v2`)

**Capability Negotiation**:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {
    "client_version": "1.0.0",
    "capabilities": ["streaming", "progress"]
  }
}
```

Response includes server capabilities:
```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "result": {
    "server_version": "1.2.0",
    "schema_version": "1.1",
    "capabilities": ["streaming", "progress", "daemon"],
    "platforms": ["android", "ios", "windows", "maccatalyst"]
  }
}
```

---

## 13. Testing Strategy

### 13.1 Unit Tests

| Component | Coverage Target | Approach |
|-----------|-----------------|----------|
| CLI argument parsing | 100% | Test all commands, options, combinations |
| JSON-RPC serialization | 100% | Schema validation tests |
| Issue detection logic | 90% | Mock file system and tool outputs |
| Fix execution logic | 80% | Mock tool invocations |

### 13.2 Integration Tests

| Scenario | Environment | Frequency |
|----------|-------------|-----------|
| Doctor on clean Windows | Windows VM | Every PR |
| Doctor on clean macOS | macOS VM | Every PR |
| SDK installation | Isolated environment | Nightly |
| AVD creation/start | Android emulator | Nightly |
| Simulator management | macOS with Xcode | Nightly |

### 13.3 Device Farm / Emulator Tests

| Test | Infrastructure | Frequency |
|------|----------------|-----------|
| Screenshot capture | Helix (Android emulator) | Weekly |
| Screenshot capture | Helix (iOS simulator) | Weekly |
| Logcat streaming | Helix (Android emulator) | Weekly |
| Full doctor + fix flow | Dedicated VMs | Release |

### 13.4 Test Environments

```yaml
# CI Matrix
test-matrix:
  - os: windows-latest
    tests: [unit, integration-windows]
  - os: macos-14
    tests: [unit, integration-macos, integration-xcode]
  - os: ubuntu-latest
    tests: [unit]  # Limited to non-platform-specific
```

---

## 14. Rollout Plan

### 14.1 Release Channels

| Channel | Stability | Update Frequency | Audience |
|---------|-----------|------------------|----------|
| Preview | Experimental | Weekly | Early adopters, feedback |
| Release Candidate | Stable | Bi-weekly | Broader testing |
| Stable | Production | Monthly | General availability |

### 14.2 Packaging

**dotnet tool**:
```
Package ID: Microsoft.Maui.DevTools
Install: dotnet tool install -g Microsoft.Maui.DevTools [--prerelease]
Update: dotnet tool update -g Microsoft.Maui.DevTools
```

**IDE Bundling**:
- VS Code extension bundles specific version
- Visual Studio includes in MAUI workload
- Can override with global tool if newer version needed

### 14.3 Backwards Compatibility

| Aspect | Guarantee |
|--------|-----------|
| CLI arguments | No breaking changes within major version |
| JSON output schema | Additive changes only; new fields don't break clients |
| JSON-RPC methods | Methods never removed; deprecated methods return warnings |
| Exit codes | Stable within major version |

### 14.4 Update Strategy

**Auto-update notification**:
```
$ maui doctor
A new version of MAUI Dev Tools is available (1.2.0 → 1.3.0)
Run 'dotnet tool update -g Microsoft.Maui.DevTools' to update.

[Current output continues...]
```

**IDE handling**:
- Check for updates on extension activation
- Prompt to update if major version behind
- Auto-update patch versions silently

---

## 15. Open Questions & Risks

### 15.1 Open Questions

| ID | Question | Options | Recommendation |
|----|----------|---------|----------------|
| OQ1 | Should we support Linux for Android-only development? | Yes/No | Defer to vNext; validate demand first |
| OQ2 | How to handle multiple Xcode versions? | Detect all / Use xcode-select | Use xcode-select default; allow override |
| OQ3 | Should daemon auto-start on login? | Yes/No/Optional | No; start on first IDE request |
| OQ4 | How to handle Apple Silicon vs Intel for simulators? | Auto-detect / Require flag | Auto-detect from `uname -m` |
| OQ5 | Should we cache SDK installers? | Yes/No/Configurable | Yes, with configurable cache location |

### 15.2 Risks

| ID | Risk | Likelihood | Impact | Mitigation |
|----|------|------------|--------|------------|
| R1 | Android SDK license changes break auto-install | Low | High | Monitor Google announcements; have manual fallback |
| R2 | Xcode CLI tools break with updates | Medium | Medium | Version-specific handling; quick patch releases |
| R3 | IDE teams don't adopt | Medium | High | Early engagement; joint design reviews |
| R4 | Performance issues with large SDK installations | Low | Medium | Progress UI; resumable downloads |
| R5 | Security vulnerabilities in downloaded components | Low | Critical | Checksum verification; CVE monitoring |

---

## 16. Appendix

### 16.1 Command Examples

#### Doctor Commands
```bash
# Basic health check
maui doctor

# Health check with JSON output
maui doctor --json

# Check and fix automatically
maui doctor --fix

# Check only Android components
maui doctor --target android

# Non-interactive mode for CI
maui doctor --fix --non-interactive
```

#### Android Commands
```bash
# List connected devices
maui android device list

# Install recommended SDK packages
maui android sdk install --recommended

# Install specific packages
maui android sdk install "platforms;android-34" "build-tools;34.0.0"

# Create AVD
maui android avd create --name "Test_Pixel_5" --device pixel_5 --image "system-images;android-34;google_apis;x86_64"

# Start emulator and wait for boot
maui android avd start --name "Test_Pixel_5" --wait

# Stream logcat
maui android logcat --device emulator-5554 --filter "Microsoft.Maui:V *:S"

# Take screenshot
maui device screenshot --device emulator-5554 --output ./screenshot.png
```

#### Apple Commands
```bash
# List all simulators
maui apple simulator list

# List only booted iPhone simulators
maui apple simulator list --device-type iPhone --state booted

# Boot specific simulator
maui apple simulator boot --udid A1B2C3D4-E5F6-7890-ABCD-EF1234567890

# Create new simulator
maui apple simulator create --name "Test iPhone 16" --device-type "iPhone 16 Pro" --runtime "iOS 18.0"

# List available runtimes
maui apple runtime list

# Take screenshot from simulator
maui device screenshot --device A1B2C3D4-E5F6-7890-ABCD-EF1234567890 --output ./sim-screenshot.png
```

### 16.2 JSON Output Examples

#### Doctor Output (Healthy)
```json
{
  "schema_version": "1.0",
  "correlation_id": "diag-2026-02-03-174500-abc123",
  "timestamp": "2026-02-03T17:45:00.000Z",
  "status": "healthy",
  "checks": [
    {
      "category": "dotnet",
      "name": ".NET SDK",
      "status": "ok",
      "details": {
        "version": "9.0.100",
        "path": "/usr/local/share/dotnet",
        "architecture": "arm64"
      }
    },
    {
      "category": "dotnet",
      "name": "MAUI Workload",
      "status": "ok",
      "details": {
        "version": "9.0.0",
        "installed_packs": [
          "Microsoft.Maui.Sdk",
          "Microsoft.Maui.Controls",
          "Microsoft.Maui.Core"
        ]
      }
    },
    {
      "category": "android",
      "name": "Android SDK",
      "status": "ok",
      "details": {
        "path": "/Users/dev/Library/Android/sdk",
        "build_tools": ["34.0.0", "33.0.2"],
        "platforms": ["android-34", "android-33"],
        "emulator_version": "34.1.9"
      }
    },
    {
      "category": "apple",
      "name": "Xcode",
      "status": "ok",
      "details": {
        "version": "16.0",
        "path": "/Applications/Xcode.app",
        "developer_dir": "/Applications/Xcode.app/Contents/Developer"
      }
    }
  ],
  "summary": {
    "total": 8,
    "ok": 8,
    "warning": 0,
    "error": 0
  }
}
```

#### Doctor Output (Issues Found)
```json
{
  "schema_version": "1.0",
  "correlation_id": "diag-2026-02-03-174600-def456",
  "timestamp": "2026-02-03T17:46:00.000Z",
  "status": "unhealthy",
  "checks": [
    {
      "category": "android",
      "name": "Android SDK",
      "status": "error",
      "message": "Android SDK not found at expected locations",
      "issue": {
        "id": "ANDROID_SDK_MISSING",
        "severity": "error",
        "description": "Android SDK is required for Android development",
        "fixable": true,
        "fix": {
          "description": "Install Android SDK to default location",
          "command": "maui android sdk install --recommended",
          "rpc_method": "android.sdk.install",
          "rpc_params": { "packages": ["recommended"] },
          "estimated_size_bytes": 2800000000,
          "requires_confirmation": true
        }
      }
    }
  ],
  "summary": {
    "total": 8,
    "ok": 5,
    "warning": 1,
    "error": 2
  }
}
```

#### Device List Output
```json
{
  "schema_version": "1.0",
  "devices": [
    {
      "id": "emulator-5554",
      "name": "Pixel_5_API_34",
      "platform": "android",
      "type": "emulator",
      "state": "online",
      "os_version": "14",
      "architecture": "x86_64",
      "details": {
        "avd_name": "Pixel_5_API_34",
        "api_level": 34,
        "skin": "pixel_5",
        "snapshot": true
      }
    },
    {
      "id": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
      "name": "iPhone 16 Pro",
      "platform": "ios",
      "type": "simulator",
      "state": "booted",
      "os_version": "18.0",
      "architecture": "arm64",
      "details": {
        "udid": "A1B2C3D4-E5F6-7890-ABCD-EF1234567890",
        "runtime": "com.apple.CoreSimulator.SimRuntime.iOS-18-0",
        "device_type": "com.apple.CoreSimulator.SimDeviceType.iPhone-16-Pro"
      }
    }
  ]
}
```

### 16.3 Error Code Catalog

| Code | Category | Message | Resolution |
|------|----------|---------|------------|
| `E1001` | General | Unknown command | Check command spelling; run `maui --help` |
| `E1002` | General | Missing required argument | Provide required argument or use interactive mode |
| `E1003` | General | Invalid argument value | Check allowed values in help |
| `E2001` | Android | Android SDK not found | Run `maui android sdk install --recommended` |
| `E2002` | Android | ADB not found | Install platform-tools via SDK manager |
| `E2003` | Android | Emulator not found | Install emulator package |
| `E2004` | Android | AVD not found | Create AVD with `maui android avd create` |
| `E2005` | Android | System image not found | Install required system image |
| `E2006` | Android | Device offline | Reconnect device or restart emulator |
| `E2007` | Android | APK installation failed | Check device storage; verify APK is valid |
| `E3001` | Apple | Xcode not found | Install Xcode from App Store |
| `E3002` | Apple | Xcode CLI tools not found | Run `xcode-select --install` |
| `E3003` | Apple | Runtime not found | Install via Xcode > Settings > Platforms |
| `E3004` | Apple | Simulator not found | Create simulator with `maui apple simulator create` |
| `E3005` | Apple | Simulator failed to boot | Check runtime compatibility; try different device |
| `E3006` | Apple | Developer directory not set | Run `sudo xcode-select -s /Applications/Xcode.app` |
| `E4001` | Network | Download failed | Check internet connection; retry |
| `E4002` | Network | Checksum verification failed | Re-download; report if persistent |
| `E4003` | Network | Connection timeout | Retry; check firewall/proxy settings |
| `E5001` | Permission | Elevation required | Run with administrator/sudo |
| `E5002` | Permission | Access denied | Check file/directory permissions |
| `E5003` | Permission | Agent permission denied | Approve action in IDE prompt |

---

## 17. MVP vs vNext Features

### MVP (v1.0)

| Priority | Feature |
|----------|---------|
| P0 | `doctor` command with status reporting |
| P0 | `doctor --fix` for automated remediation |
| P0 | Android SDK detection and installation |
| P0 | Android AVD creation and management |
| P0 | iOS simulator listing and boot/shutdown |
| P0 | Unified `device list` command |
| P0 | `device screenshot` command |
| P0 | JSON output for all commands |
| P0 | JSON-RPC API for IDE integration |
| P0 | VS Code extension integration |
| P1 | Logcat streaming |
| P1 | iOS runtime listing |
| P1 | Interactive prompting |

### vNext (v1.x / v2.0)

| Priority | Feature |
|----------|---------|
| P1 | Visual Studio extension integration |
| P1 | Daemon mode for faster IDE interactions |
| P1 | iOS runtime installation guidance |
| P1 | APK install/uninstall |
| P2 | AVD snapshot management |
| P2 | Simulator log streaming |
| P2 | Windows SDK management |
| P2 | Linux host support (Android only) |
| P2 | Physical iOS device support (requires signing) |
| P2 | `tree` command for dependency visualization |
| P2 | `archive` command for app packaging |
| Future | MCP server integration for AI agents |
| Future | Cloud-hosted device support |

---

## 18. Acceptance Criteria

The following criteria must be met for v1.0 release:

1. **AC1**: `maui doctor` completes in <5 seconds on a healthy system and correctly identifies all missing MAUI prerequisites.

2. **AC2**: `maui doctor --fix` can install Android SDK, build-tools, emulator, and system images without manual intervention (license acceptance via `--accept-licenses`).

3. **AC3**: `maui android avd create` creates a functional emulator that can boot and run a MAUI app.

4. **AC4**: `maui apple simulator list` returns all available simulators with runtime and state information matching `xcrun simctl list`.

5. **AC5**: `maui device list` returns a unified list of all Android devices/emulators and iOS simulators with consistent schema.

6. **AC6**: `maui device screenshot` captures screenshots from both Android emulator and iOS simulator, saving to specified path.

7. **AC7**: All commands support `--json` output with stable schema documented in spec.

8. **AC8**: JSON-RPC server responds to all documented methods over stdio transport.

9. **AC9**: VS Code extension can invoke `doctor.status`, display results, and trigger fixes via `doctor.fix`.

10. **AC10**: Running `maui doctor --fix --non-interactive` in CI completes without prompts and returns appropriate exit codes.

11. **AC11**: All downloads verify SHA-256 checksums before installation.

12. **AC12**: AI agent permission gates prevent unauthorized modifications; all fix operations require explicit IDE confirmation.

13. **AC13**: `diagnostic-bundle` produces a zip file containing all relevant diagnostic information with PII redacted.

14. **AC14**: Tool runs correctly on Windows 10+ (x64/arm64) and macOS 13+ (arm64).

15. **AC15**: Full test suite passes with >80% code coverage on unit tests.

---

## 19. Future Subcommands (Roadmap)

Inspired by MAUI CLI TODOs and community requests:

| Command | Description | Inspiration |
|---------|-------------|-------------|
| `maui logs` | Stream unified logs from device/simulator | MAUI CLI TODO |
| `maui tree` | Display dependency tree for MAUI project | MAUI CLI TODO |
| `maui archive` | Build and package app for distribution | MAUI CLI TODO |
| `maui deploy` | Deploy to connected device | MAUI CLI TODO |
| `maui clean` | Clean MAUI build artifacts and caches | Common request |
| `maui template` | Scaffold new MAUI projects (delegate to dotnet new) | Convenience |
| `maui perf` | Performance profiling helpers | Future |
| `maui test` | Run device tests (delegate to dotnet test with device targeting) | Future |

---

*End of Specification*

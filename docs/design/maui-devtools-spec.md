# MAUI Dev Tools Client — Product Specification

**Version**: 2.9-draft  
**Status**: Proposal  
**Last Updated**: 2026-02-10

---

## Table of Contents

1. [Executive Summary](#1-executive-summary)
2. [Problem Statement](#2-problem-statement)
3. [Goals, Non-Goals & Personas](#3-goals-non-goals--personas)
4. [Functional Requirements](#4-functional-requirements)
5. [Non-Functional Requirements](#5-non-functional-requirements)
6. [Architecture](#6-architecture)
7. [Public API Surface](#7-public-api-surface)
8. [User Experience](#8-user-experience)
9. [Diagnostics](#9-diagnostics)
10. [MVP vs vNext Features](#10-mvp-vs-vnext-features)

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

## 3. Goals, Non-Goals & Personas

### Goals

| ID | Goal | Success Metric |
|----|------|----------------|
| G1 | Reduce MAUI setup time to under 10 minutes | Time-to-first-build < 10 min for 90% of users |
| G2 | Provide a single "doctor" command that identifies all issues | 100% coverage of common setup issues |
| G3 | Enable one-click/one-command fixes for detected issues | >80% of issues auto-fixable |
| G4 | Expose structured APIs for IDE and AI agent consumption | JSON output with stable schema |
| G5 | Support headless operation for CI environments | All commands runnable non-interactively |

### Non-Goals

| ID | Non-Goal | Rationale |
|----|----------|-----------|
| NG1 | Replace `dotnet` CLI | This tool complements, not replaces, the .NET CLI |
| NG2 | Full Apple signing/provisioning management | Complex domain; integrate with existing tools instead |
| NG3 | Linux host support (MVP) | MAUI mobile development requires Windows or macOS |
| NG4 | Physical iOS device provisioning | Requires Apple Developer account; out of scope for MVP |
| NG5 | Manage Visual Studio installation | VS has its own installer; we detect, not manage |

### Design Principles

**DP1: Delegate to Native Toolchains** — Do not reimplement. Use `sdkmanager`, `avdmanager`, `adb`, `emulator` for Android; `xcrun simctl`, `xcode-select` for Apple; Windows SDK installer for Windows. Native tools are authoritative, reduce maintenance burden, and ensure consistency.

**DP2: Consolidate Existing VS Repositories** — Replace `ClientTools.android-acquisition` and `android-platform-support` with unified `dotnet maui android` commands across VS, VS Code, CLI, and CI.

**DP3: Stateless Architecture** — Each command reads state, acts, and exits. Uses file-system caching (`~/.maui/cache/`) with TTLs for performance.

**DP4: Machine-First Output** — Every command supports `--json` with stable schema. Priority: AI agents > CI/CD > humans. Required flags: `--json`, `--dry-run`, `--ci`.

### Target Personas

| Persona | Profile | Key Need |
|---------|---------|----------|
| **Windows Developer** | .NET dev, new to Android | One-click install of all Android dependencies |
| **macOS Developer** | Building iOS apps, has Xcode | Detection of runtime/simulator state, guided fixes |
| **CI Engineer** | DevOps configuring pipelines | `--non-interactive`, JSON output, deterministic exit codes |
| **AI Agent** | GitHub Copilot, IDE assistants | Structured JSON for diagnosis, permission-gated fixes |

> **See [AI Agent Integration](./maui-devtools-ai-integration.md)** for detailed AI agent personas and permission model.

---

## 4. Functional Requirements

### 4.1 Doctor Capability

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

### 4.2 Android Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-A1 | List connected devices and running emulators | P0 |
| FR-A2 | List installed SDK packages with version info | P0 |
| FR-A3 | Install SDK packages by name or alias (e.g., `--recommended`) | P0 |
| FR-A4 | List available emulators | P0 |
| FR-A5 | Create emulator with specified device profile and system image | P0 |
| FR-A6 | Start emulator and wait for boot completion | P0 |
| FR-A7 | Stop running emulator | P1 |
| FR-A8 | Cold boot emulator (wipe runtime state) | P1 |
| FR-A9 | Wipe emulator data | P2 |
| FR-A10 | Stream logcat output with filtering | P1 |
| FR-A11 | Install APK to device/emulator | P1 |
| FR-A12 | Uninstall package from device/emulator | P2 |
| FR-A13 | Capture screenshot from device/emulator | P0 |
| FR-A14 | Install full Android environment (JDK + SDK) from scratch | P0 |
| FR-A15 | Detect JDK installation and version | P0 |
| FR-A16 | Install OpenJDK if missing (version 17 default, 21 supported) | P0 |
| FR-A17 | Use platform-appropriate default paths when env vars not set | P0 |

### 4.3 Apple (Xcode) Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-X1 | List available simulators with runtime/device type info | P0 |
| FR-X2 | Filter simulators by runtime, device type, or state | P0 |
| FR-X3 | Start simulator by UDID or name | P0 |
| FR-X4 | Stop simulator | P0 |
| FR-X5 | Create simulator with specified runtime and device type | P0 |
| FR-X6 | Delete simulator | P2 |
| FR-X7 | List available runtimes | P0 |
| FR-X8 | Install runtime (guide user if manual steps needed) | P1 |
| FR-X9 | Capture screenshot from simulator | P0 |
| FR-X10 | Stream simulator/device logs | P1 |
| FR-X11 | Validate basic signing prerequisites (team ID, certificate presence) | P2 |
| FR-X12 | Open Simulator.app with specific device | P1 |
| FR-X13 | List all Xcode installations with version, build number, and selected status (`xcode list`) | P0 |
| FR-X14 | Switch active Xcode installation (`xcode select <path>`) | P0 |
| FR-X15 | Detect Xcode beta installations at `/Applications/Xcode-beta.app` | P2 |

### 4.4 Windows Management

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-W1 | Detect Windows SDK installation and version | P1 |
| FR-W2 | Detect Developer Mode enabled status | P0 |
| FR-W3 | Guide user to enable Developer Mode if disabled | P0 |
| FR-W4 | Detect Visual Studio installation and MAUI workload | P1 |
| FR-W5 | Detect Hyper-V availability for Android emulation | P1 |
| FR-W6 | Detect Windows App SDK dependencies | P2 |

### 4.5 Cross-Platform Screenshot

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-S1 | Unified `screenshot` command across all platforms | P0 |
| FR-S2 | Auto-detect target device if only one is available | P0 |
| FR-S3 | Support `--device` flag for explicit device selection | P0 |
| FR-S4 | Support `--output` flag for file path (default: timestamped file) | P0 |
| FR-S5 | Support `--wait` flag to delay capture | P1 |
| FR-S6 | Support `--format` flag (png, jpg) | P2 |
| FR-S7 | Return file path in JSON output | P0 |

### 4.5 Device Listing (Unified)

| ID | Requirement | Priority |
|----|-------------|----------|
| FR-DL1 | `device list` shows all available devices across platforms | P0 |
| FR-DL2 | Include device type (physical/emulator/simulator), platform, state | P0 |
| FR-DL3 | Include unique identifier (serial/UDID) for targeting | P0 |
| FR-DL4 | Support `--platform` filter | P1 |
| FR-DL5 | Support `--json` output | P0 |

### 4.6 Install State Machine

**Critical**: The tool must handle the "install gap" — the chicken-and-egg problem where native tools (sdkmanager, xcrun) don't exist yet.

#### Platform Bootstrap States

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│   MISSING   │────▶│ DOWNLOADING │────▶│ INSTALLING  │────▶│    READY    │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
       │                   │                   │                   │
       │                   │                   │                   │
       ▼                   ▼                   ▼                   ▼
   User action         Progress            Progress           Operational
   required            reporting           reporting          (delegate to
   (or auto-fix)                                              native tools)
```

#### Android Install

The tool can fully install an Android development environment from scratch, including JDK and SDK installation.

**Dependency Order**: JDK must be installed before SDK (sdkmanager requires Java).

```
┌─────────────┐     ┌─────────────┐     ┌─────────────┐     ┌─────────────┐
│  JDK Check  │────▶│ JDK Install │────▶│ SDK Install │────▶│    READY    │
└─────────────┘     └─────────────┘     └─────────────┘     └─────────────┘
       │                   │                   │                   │
   Missing?            Download            Download           Operational
                      OpenJDK 17         cmdline-tools        (delegate)
```

**Default Installation Paths** (when env vars not set):

| Platform | JDK Path | SDK Path |
|----------|----------|----------|
| macOS | `~/Library/Developer/Android/jdk` | `~/Library/Developer/Android/sdk` |
| Windows | `%LOCALAPPDATA%\Android\jdk` | `%LOCALAPPDATA%\Android\sdk` |

**Install States**:

| State | Detection | Behavior |
|-------|-----------|----------|
| `JDK_MISSING` | No `java` in PATH, no JDK at default paths | Install OpenJDK 17 to default path |
| `SDK_MISSING` | `ANDROID_HOME` not set, no SDK at standard paths | Install command-line tools to default path |
| `PARTIAL` | SDK exists but `sdkmanager` missing/broken | Repair SDK or reinstall cmdline-tools |
| `READY` | `java -version` succeeds AND `sdkmanager --list` succeeds | Delegate all operations to native tools |

**Install Command**:
```bash
# Full install: JDK + SDK + recommended packages
dotnet maui android install --accept-licenses

# With custom paths
dotnet maui android install --jdk-path ~/my-jdk --sdk-path ~/my-sdk --accept-licenses

# With specific packages (comma-separated)
dotnet maui android install --packages "platform-tools,build-tools;35.0.0,platforms;android-35"
```

This command:
1. Checks for JDK; if missing, downloads and installs OpenJDK 17
2. Sets `JAVA_HOME` for the session (prints guidance for permanent setup)
3. Downloads Android command-line tools (if missing)
4. Accepts SDK licenses non-interactively (if `--accept-licenses`)
5. Installs specified packages (or default recommended set if `--packages` not provided)
6. Prints environment variable guidance (doesn't modify shell config)

**JDK Management Commands**:
```bash
# Check JDK status
dotnet maui android jdk check

# Install OpenJDK (default: version 17)
dotnet maui android jdk install
dotnet maui android jdk install --version 21

# List installed JDK versions
dotnet maui android jdk list
```

**Environment Variable Guidance Output**:
```
✓ JDK installed to ~/Library/Developer/Android/jdk
✓ SDK installed to ~/Library/Developer/Android/sdk

Add to your shell profile (~/.zshrc or ~/.bashrc):

  export JAVA_HOME="$HOME/Library/Developer/Android/jdk"
  export ANDROID_HOME="$HOME/Library/Developer/Android/sdk"
  export PATH="$JAVA_HOME/bin:$ANDROID_HOME/cmdline-tools/latest/bin:$ANDROID_HOME/platform-tools:$PATH"
```

**AVD Creation with Auto-Detection**:

When `--package` is not specified, the tool automatically detects the most recent installed system image:

```bash
# Auto-detect system image (uses highest API level installed)
dotnet maui android emulator create MyEmulator

# Explicitly specify system image
dotnet maui android emulator create MyEmulator --package "system-images;android-35;google_apis;arm64-v8a"
```

#### Apple Install

| State | Detection | Behavior |
|-------|-----------|----------|
| `MISSING` | No Xcode.app at `/Applications/Xcode*.app` | Error: "Install Xcode from App Store" (cannot auto-install) |
| `CLI_ONLY` | Only Command Line Tools installed | Error: "Full Xcode required for simulators" |
| `READY` | `xcrun simctl list` succeeds | Delegate all operations to native tools |

**Important**: Apple install is limited because:
- Xcode cannot be installed programmatically (App Store only)
- Runtime downloads require Xcode to be open at least once (license acceptance)

```bash
dotnet maui apple install
```

This command:
1. Verifies Xcode installation
2. Runs `xcode-select --install` if CLI tools missing
3. Prompts user to accept Xcode license if needed
4. Reports status and next steps

#### Windows Install

| State | Detection | Behavior |
|-------|-----------|----------|
| `MISSING` | No Windows SDK detected | Provide download link |
| `NO_DEV_MODE` | Developer Mode disabled | Guide user to enable (requires Settings app) |
| `READY` | SDK present, Developer Mode enabled | Operational |

---

## 5. Non-Functional Requirements

### 5.1 Reliability

| ID | Requirement |
|----|-------------|
| NFR-R1 | All operations must be idempotent (safe to retry) |
| NFR-R2 | Network failures must produce clear error messages with retry guidance |
| NFR-R3 | Partial failures during multi-step operations must leave system in consistent state |
| NFR-R4 | Tool must gracefully handle missing permissions |

### 5.2 Observability

| ID | Requirement |
|----|-------------|
| NFR-O1 | All operations must support `--verbose` flag for detailed logging |
| NFR-O2 | JSON output must include `correlation_id` for tracing |
| NFR-O3 | Long-running operations must emit progress events |
| NFR-O4 | (vNext) `diagnostic-bundle` command to collect all relevant logs and state |

**Progress Reporting**:

Long-running operations (install, SDK install, JDK install) emit step-by-step progress:

Console output:
```
[1/4] Checking JDK installation...
[2/4] Installing JDK 17...
[3/4] Checking SDK installation...
[4/4] Installing SDK packages: platform-tools, build-tools;35.0.0
✓ Bootstrap completed successfully
```

JSON output (`--json`):
```json
{
  "type": "progress",
  "step": 2,
  "total_steps": 4,
  "message": "Installing JDK 17...",
  "percentage": 50
}
```

IDE consumers can use the `type: "progress"` messages to update progress bars and status indicators.

### 5.3 Performance

| ID | Requirement |
|----|-------------|
| NFR-P1 | `doctor` command must complete in <5s when no network calls needed |
| NFR-P2 | Device list must complete in <2s |
| NFR-P3 | Downloaded artifacts must be cached locally |
| NFR-P4 | Read operations must use direct file parsing (not CLI wrappers) for performance |

**Performance Implementation Notes**:
- Android SDK detection: Parse `package.xml` and `source.properties` directly instead of invoking `sdkmanager --list` (which has 3-10s JVM startup time)
- iOS simulator list: Use `xcrun simctl list -j` (fast native tool)
- Fall back to CLI tools only for write operations (install, create, etc.)

**Shell Quoting Implementation Notes**:
- Package identifiers contain semicolons (e.g., `system-images;android-35;google_apis;arm64-v8a`)
- When calling `avdmanager` or `sdkmanager`, arguments with semicolons must be properly quoted
- Use single quotes with escaped inner quotes: `--package '{escapedImage}'`
- This prevents shell interpretation of semicolons as command separators

### 5.4 Security

| ID | Requirement |
|----|-------------|
| NFR-S1 | Downloads must verify checksums before installation |
| NFR-S2 | Tool must never store or transmit credentials |
| NFR-S3 | Elevation must be requested explicitly with clear justification |
| NFR-S4 | AI agent calls must respect permission gates (see Security Model) |

### 5.5 Privacy

| ID | Requirement |
|----|-------------|
| NFR-PR1 | No PII in logs or telemetry |
| NFR-PR2 | File paths must be redacted in telemetry (keep structure only) |
| NFR-PR3 | Telemetry must be opt-in with clear disclosure |

### 5.6 Accessibility

| ID | Requirement |
|----|-------------|
| NFR-A1 | CLI output must work with screen readers (avoid relying solely on color) |
| NFR-A2 | All status indicators must have text equivalents |
| NFR-A3 | IDE integration must follow platform accessibility guidelines |

### 5.7 Network & Offline Support

| ID | Requirement |
|----|-------------|
| NFR-N1 | Respect system proxy settings (`HTTP_PROXY`, `HTTPS_PROXY`, `NO_PROXY`) |
| NFR-N2 | Support custom CA certificates for corporate SSL inspection |
| NFR-N3 | Provide `--offline` mode for doctor to skip network checks |
| NFR-N4 | Support `--local-source <path>` for installing from pre-downloaded artifacts |
| NFR-N5 | Large downloads must support HTTP Range requests for resumability |
| NFR-N6 | Cache downloaded installers with configurable location and size limits |

### 5.8 Container & Headless Environments

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
- `dotnet maui doctor` reports capability with reason (e.g., "emulator: unavailable (virtualization disabled)")
- `dotnet maui android emulator start` fails fast with `E2010` "Hardware acceleration unavailable"

---

## 6. Architecture

### 6.1 High-Level Components

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              Consumers                                   │
├─────────────┬─────────────┬─────────────┬─────────────┬─────────────────┤
│  Terminal   │  VS Code    │   Visual    │  AI Agent   │   CI/CD         │
│  (Human)    │  Extension  │   Studio    │  (Copilot)  │   Pipeline      │
└──────┬──────┴──────┬──────┴──────┬──────┴──────┬──────┴────────┬────────┘
       │             │             │             │               │
       │ CLI         │ CLI         │ CLI         │ CLI           │ CLI
       │             │ (--json)    │ (--json)    │ (--json)      │
       ▼             ▼             ▼             ▼               ▼
┌─────────────────────────────────────────────────────────────────────────┐
│                        MAUI Dev Tools Client                            │
├─────────────────────────────────────────────────────────────────────────┤
│  ┌─────────────────────────────────────────────────────────────┐        │
│  │                       CLI Layer                              │        │
│  │  (Argument parsing, output formatting, --json support)       │        │
│  └──────────────────────────────┬──────────────────────────────┘        │
│                                 │                                       │
│                                 ▼                                       │
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

### 6.2 Component Descriptions

#### CLI Layer
- Parses command-line arguments using `System.CommandLine`
- Maps commands to core service calls
- Formats output for human consumption (colors, tables, progress bars)
- Supports `--json` for machine-readable output
- Entry point: `dotnet maui` command

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

### 6.3 Data Flow: Doctor Command

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

### 6.4 IDE Extension Integration

> **See [IDE Integration](./maui-devtools-ide-integration.md)** for detailed VS Code and Visual Studio UI flows, status panels, and menu integrations.

**Summary**: IDEs spawn `dotnet maui` as a child process, invoke `dotnet maui doctor --json` on workspace open, display issues in their problems/error list panels, and provide commands for environment setup with progress notifications.

### 6.5 Concurrency Model

| Aspect | Behavior |
|--------|----------|
| Read Operations | Parallelized (multiple simultaneous queries allowed) |
| Write Operations | Serialized (single writer at a time) |
| Lock File | `$TMPDIR/maui-devtools.lock` prevents concurrent writes |
| Request Timeout | Requests waiting >30s for lock rejected with `E5010` |

### 6.6 SDK Conflict Resolution

When multiple SDK installations are detected:

**Android SDK Precedence** (first match wins):
1. `ANDROID_HOME` environment variable
2. `ANDROID_SDK_ROOT` environment variable  
3. Visual Studio configured path (Windows)
4. Android Studio configured path (`~/.android/sdk` or registry)
5. Default location (`~/Library/Android/sdk` on macOS, `%LOCALAPPDATA%\Android\Sdk` on Windows)

**Conflict Handling**:
- `dotnet maui doctor` reports all detected SDKs with recommendation
- `--sdk-path` flag available on all android commands for one-off override

**Android Studio Coexistence**:
- Detect Android Studio installation
- Warn if both AS and tool would manage same SDK
- Offer to adopt AS's SDK rather than installing duplicate

### 6.7 Migration from Existing Setups

When an existing SDK is detected:

1. **Validate** existing SDK health before offering modifications
2. **Adopt or Fresh**: Offer "adopt existing" vs "install fresh" options
3. **Non-Destructive**: Never delete or modify user's existing SDK without explicit consent
4. **Override Support**: `--sdk-path` for non-standard locations

### 6.8 Error Contract Specification

**This is the highest-priority architectural element.** Every consumer (AI agents, CI pipelines, IDEs, humans) depends on predictable error handling.

#### Error Taxonomy

Errors are classified into three categories:

| Category | Prefix | Responsibility | Example |
|----------|--------|----------------|---------|
| **Tool** | `E1xxx` | Bug in this tool | E1001: Internal state corruption |
| **Platform** | `E2xxx` | Native tool or SDK issue | E2001: sdkmanager license not accepted |
| **User** | `E3xxx` | User action required | E3001: Xcode not installed |

#### Error Object Schema

**Every error MUST be expressible as this JSON structure:**

```json
{
  "code": "E2001",
  "category": "platform",
  "severity": "error",
  "message": "Android SDK licenses not accepted",
  "native_error": "Warning: License for package Android SDK Platform 34 not accepted.",
  "context": {
    "sdk_path": "/Users/dev/Library/Android/sdk",
    "package": "platforms;android-34"
  },
  "remediation": {
    "type": "auto_fixable",
    "command": "dotnet maui android sdk accept-licenses",
    "manual_steps": null
  },
  "docs_url": "https://learn.microsoft.com/dotnet/maui/troubleshoot/E2001",
  "correlation_id": "7f3d2a1b-..."
}
```

#### Remediation Types

| Type | Meaning | AI Agent Behavior |
|------|---------|-------------------|
| `auto_fixable` | Tool can fix this automatically | Execute `remediation.command` with user permission |
| `user_action` | User must take manual steps | Display `remediation.manual_steps`, cannot auto-fix |
| `terminal` | Cannot be fixed (e.g., unsupported OS) | Report error, suggest alternatives |
| `unknown` | Tool doesn't recognize this error | Escalate to Copilot Handoff |

#### Error Code Registry (Partial)

| Code | Category | Message | Remediation Type |
|------|----------|---------|------------------|
| `E1001` | tool | Internal error | terminal |
| `E2001` | platform | SDK licenses not accepted | auto_fixable |
| `E2002` | platform | sdkmanager not found | auto_fixable (install) |
| `E2003` | platform | xcrun failed | user_action |
| `E2004` | platform | Emulator acceleration unavailable | user_action |
| `E3001` | user | Xcode not installed | user_action |
| `E3002` | user | Developer Mode not enabled | user_action |
| `E3003` | user | Insufficient disk space | user_action |
| `E3004` | user | Network unavailable | user_action |

#### Unknown Error Handling

When the tool encounters an error it doesn't recognize:

```json
{
  "code": "E0000",
  "category": "unknown",
  "severity": "error",
  "message": "Unexpected error from native tool",
  "native_error": "<full stderr output>",
  "remediation": {
    "type": "unknown",
    "command": null,
    "manual_steps": null
  },
  "copilot_handoff": {
    "eligible": true,
    "context": {
      "doctor_report": { ... },
      "failed_command": "dotnet maui android emulator create ...",
      "environment": { ... },
      "native_tool_output": "..."
    }
  }
}
```

This is the **Copilot escalation trigger** — structured data that AI agents can consume to diagnose novel issues.

#### Exit Code Mapping

| Exit Code | Meaning | Contains |
|-----------|---------|----------|
| 0 | Success | Result data |
| 1 | Partial success | Result + warnings |
| 2 | Operation failed | Error objects |
| 3 | Permission denied | Error + elevation guidance |
| 4 | User cancelled | Cancellation reason |
| 5 | Resource not found | Error + suggestions |
| 126 | Command not executable | Error |
| 127 | Command not found | Error |

#### Structured Output Contract

**All commands MUST support these output modes:**

```bash
# Default: Human-readable (stderr for errors, stdout for results)
dotnet maui doctor

# Machine-readable: JSON to stdout (errors included in JSON, not stderr)
dotnet maui doctor --json

# CI mode: JSON output, no prompts, warnings become errors
dotnet maui doctor --ci
```

**JSON output envelope:**

```json
{
  "success": false,
  "correlation_id": "...",
  "duration_ms": 1234,
  "result": null,
  "errors": [ { ... } ],
  "warnings": [ { ... } ]
}
```

---

## 7. Public API Surface

### 7.1 CLI Commands

The tool is invoked as `dotnet maui <command>`, integrating naturally with the .NET CLI. Platform-specific commands use the target framework moniker pattern (`ios`, `android`, `maccatalyst`, `windows`).

#### Command Hierarchy

```
dotnet maui
├── doctor                    # Check environment health
│   ├── --fix                 # Auto-fix all detected issues
│   ├── --category <c>       # Filter: dotnet, android, apple, windows
│   ├── --json                # Output as JSON
│   └── --ci                  # CI mode (no prompts, fail-fast)
│
├── device
│   ├── list                  # List all devices across platforms
│   │   ├── --platform        # Filter by platform
│   │   └── --json            # Output as JSON
│   ├── screenshot            # Capture screenshot
│   │   ├── --device <id>     # Target device
│   │   ├── --output <path>   # Output file
│   │   ├── --wait <ms>       # Delay before capture
│   │   └── --format <fmt>    # png, jpg
│   └── logs                  # Stream logs from device (unified across platforms)
│       ├── --device <id>     # Device identifier (required)
│       ├── --filter <expr>   # Filter expression (platform-specific)
│       ├── --maui-only       # Filter to MAUI-related logs only
│       └── --since <time>    # Show logs since timestamp
│
├── android                   # Android-specific commands
│   ├── install               # Set up Android development environment
│   │   ├── --accept-licenses # Non-interactively accept licenses
│   │   ├── --packages <list> # Comma-separated packages to install
│   │   ├── --jdk-path <dir>  # JDK installation directory
│   │   ├── --jdk-version     # JDK version (default: 17)
│   │   └── --sdk-path <dir>  # SDK installation directory
│   ├── jdk                   # JDK management
│   │   ├── check             # Check JDK installation status
│   │   ├── install           # Install OpenJDK
│   │   │   ├── --version     # JDK version (default: 17)
│   │   │   └── --path <dir>  # Installation directory
│   │   └── list              # List installed JDK versions
│   ├── sdk
│   │   ├── list              # List SDK packages
│   │   │   ├── --available   # Show packages available for install
│   │   │   └── --all         # Show both installed and available
│   │   ├── install <pkg>     # Install package(s) - comma-separated
│   │   ├── accept-licenses   # Accept all SDK licenses
│   │   └── uninstall <pkg>   # Uninstall package
│   └── emulator
│       ├── list              # List emulators
│       ├── create            # Create emulator
│       │   ├── --name        # Emulator name
│       │   ├── --device      # Device profile
│       │   ├── --package     # System image (optional, auto-detects latest)
│       │   └── --force       # Overwrite existing
│       ├── start             # Start emulator
│       │   ├── --name        # Emulator name
│       │   ├── --cold-boot   # Cold boot
│       │   └── --wait        # Wait for boot
│       ├── stop              # Stop emulator
│       │   └── --serial      # Emulator serial (e.g., emulator-5554)
│       └── delete            # Delete emulator
│           └── --name        # Emulator name
│
├── apple                     # Apple platform commands (macOS only)
│   ├── simulator
│   │   ├── list              # List simulators
│   │   │   ├── --runtime     # Filter by runtime
│   │   │   ├── --device-type # Filter by device type
│   │   │   └── --state       # Filter: booted, shutdown
│   │   ├── create            # Create simulator
│   │   │   ├── --name        # Simulator name
│   │   │   ├── --runtime     # Runtime identifier
│   │   │   └── --device-type # Device type identifier
│   │   ├── start             # Start simulator
│   │   │   └── <udid>        # Simulator UDID
│   │   ├── stop              # Stop simulator
│   │   │   └── <udid>        # Simulator UDID
│   │   └── delete            # Delete simulator
│   │       └── <udid>        # Simulator UDID
│   ├── runtime
│   │   ├── list              # List installed iOS/macOS runtimes
│   │   └── install           # Install runtime (guidance)
│   │       └── --version     # Runtime version
│   └── xcode                 # Xcode installation management
│       ├── list              # List installed Xcode versions
│       └── select <path>     # Switch active Xcode installation
│
├── windows                   # Windows-specific commands (Windows only)
│   ├── sdk
│   │   └── list              # List Windows SDK installations
│   └── developer-mode
│       ├── status            # Check Developer Mode status
│       └── enable            # Guide to enable Developer Mode
│
└── --version                 # Show version
```

**Command Examples**:
```bash
# Cross-platform commands
dotnet maui doctor
dotnet maui doctor --fix
dotnet maui device list                              # List all devices (physical + emulators + simulators)
dotnet maui device list --platform android           # Android devices and emulators
dotnet maui device list --platform ios               # iOS simulators and physical devices
dotnet maui device screenshot --device emulator-5554

# Android-specific
dotnet maui android sdk install platforms;android-34
dotnet maui android emulator create --name Pixel_8 --device pixel_8
dotnet maui android emulator start --name Pixel_8 --wait
dotnet maui android logcat --device emulator-5554

# Apple-specific (macOS only)
dotnet maui apple simulator list
dotnet maui apple simulator start <udid>
dotnet maui apple runtime list
dotnet maui apple xcode list
dotnet maui apple xcode select /Applications/Xcode.app

# Windows-specific
dotnet maui windows developer-mode status
```

#### Global Options (Available on All Commands)

> **Design Principle DP4**: Every command MUST support `--json` output with a stable, versioned schema. The primary consumer is AI agents, not humans.

| Option | Description | Required |
|--------|-------------|----------|
| `--json` | Output as JSON (machine-readable) | **Mandatory on all commands** ✅ |
| `--dry-run` | Show what would be done without executing | **Mandatory on write commands** ✅ |
| `--ci` | Strict mode: no prompts, warnings become errors, JSON output forced | Recommended ✅ |
| `--verbose` / `-v` | Enable verbose logging | Optional ✅ |
| `--non-interactive` | Disable prompts; fail if input needed | Optional (vNext) |
| `--correlation-id` | Set correlation ID for tracing | Optional (vNext) |
| `--offline` | Skip network operations; use cached data only | Optional (vNext) |

**`--ci` Mode Behavior**:
- Forces `--json` output (human-readable disabled)
- Forces `--non-interactive` (no stdin prompts)
- Elevates warnings to errors (exit code 1 → 2)
- Includes full diagnostic context in error output
- Ideal for CI/CD pipelines and AI agent consumption

**`--dry-run` Mode Output**:
```json
{
  "dry_run": true,
  "planned_operations": [
    { "action": "install", "target": "platforms;android-34", "size_mb": 150 },
    { "action": "install", "target": "build-tools;34.0.0", "size_mb": 55 }
  ],
  "estimated_duration_seconds": 120,
  "requires_approval": true,
  "approval_reason": "Downloads exceed 100MB"
}
```

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
| `dotnet maui doctor` | Check environment health | `--fix`, `--platform`, `--json` | Status report | 0=healthy, 1=issues, 2=error |
| `dotnet maui device list` | List available devices | `--platform`, `--json` | Device list | 0=success, 2=error |
| `dotnet maui device screenshot` | Capture screenshot | `--device`, `--output`, `--wait` | File path | 0=success, 5=no device, 2=error |
| `dotnet maui android sdk list` | List SDK packages | `--available`, `--all`, `--json` | Package list | 0=success, 2=error |
| `dotnet maui android sdk install` | Install SDK package | `<package>`, `--accept-licenses` | Progress, result | 0=success, 5=not found, 2=error |
| `dotnet maui android emulator create` | Create emulator | `--name`, `--device`, `--package` | Emulator name | 0=success, 1=exists, 2=error |
| `dotnet maui android emulator start` | Start emulator | `--name`, `--wait`, `--cold-boot` | Device serial | 0=success, 5=not found, 2=error |
| `dotnet maui android emulator stop` | Stop emulator | `--serial` | Status | 0=success, 5=not found, 2=error |
| `dotnet maui android emulator delete` | Delete emulator | `--name` | Status | 0=success, 5=not found, 2=error |
| `dotnet maui apple simulator list` | List simulators | `--runtime`, `--device-type`, `--state` | Simulator list | 0=success, 2=error |
| `dotnet maui apple simulator start` | Start simulator | `<udid>` | UDID | 0=success, 5=not found, 2=error |
| `dotnet maui apple simulator stop` | Stop simulator | `<udid>` | Status | 0=success, 5=not found, 2=error |
| `dotnet maui apple simulator create` | Create simulator | `<name> <device-type> <runtime>` | UDID | 0=success, 2=error |
| `dotnet maui apple simulator delete` | Delete simulator | `<udid>` | Status | 0=success, 5=not found, 2=error |
| `dotnet maui apple runtime list` | List iOS runtimes | `--json` | Runtime list | 0=success, 2=error |
| `dotnet maui apple xcode list` | List Xcode installations | `--json` | Installation list | 0=success, 2=error |
| `dotnet maui apple xcode select` | Switch active Xcode | `<path>` | Confirmation | 0=success, 3=permission denied |
| `dotnet maui android sdk accept-licenses` | Accept SDK licenses | `--json` | Status | 0=success, 2=error |

### 7.2 JSON Output Schemas

#### MauiDevice Schema

The unified device model for all platforms (physical devices, emulators, simulators):

```json
{
  "name": "iPhone 15 Pro",
  "identifier": "12345678-1234-1234-1234-123456789ABC",
  "emulator_id": null,
  "platforms": ["ios", "maccatalyst"],
  "version": "18.5",
  "version_name": "18.5",
  "manufacturer": "Apple",
  "model": "iPhone 15 Pro",
  "sub_model": "Pro",
  "idiom": "phone",
  "platform_architecture": "arm64",
  "runtime_identifiers": ["ios-arm64", "maccatalyst-arm64"],
  "architecture": "arm64",
  "is_emulator": true,
  "is_running": true,
  "connection_type": "local",
  "type": "Simulator",
  "state": "Booted",
  "details": {}
}
```

| Field | Type | Description |
|-------|------|-------------|
| `name` | string | Display name (required) |
| `identifier` | string | Unique ID — AVD name (Android emulator), serial (Android physical), UDID (iOS) (required) |
| `emulator_id` | string? | AVD name for Android emulators |
| `platforms` | string[] | Supported platforms (required) |
| `version` | string? | API level for Android (e.g., `"35"`), OS version for iOS (e.g., `"18.5"`) |
| `version_name` | string? | OS version number without platform prefix (e.g., `"15.0"` not `"Android 15.0"`) |
| `manufacturer` | string? | Device manufacturer (`"Google"` for emulators, `ro.product.manufacturer` for physical) |
| `model` | string? | Device model (e.g., `"Pixel 8"`, `"iPhone 15 Pro"`) |
| `sub_model` | string? | System image variant for Android (e.g., `"Google API's"`, `"Google API's, Play Store"`) |
| `idiom` | string? | Form factor: `phone`, `tablet`, `watch`, `tv`, `desktop` |
| `platform_architecture` | string? | Raw platform ABI (e.g., `arm64-v8a`, `x86_64`, `arm64`) |
| `runtime_identifiers` | string[]? | .NET runtime identifiers |
| `architecture` | string? | Normalized CPU architecture (e.g., `arm64`, `x64`) |
| `is_emulator` | bool | True for emulator/simulator |
| `is_running` | bool | True if device is booted |
| `connection_type` | string? | `usb`, `wifi`, or `local` |
| `type` | string? | `Physical`, `Emulator`, or `Simulator` |
| `state` | string? | `Connected`, `Booted`, `Offline`, `Shutdown`, `Unknown` |
| `details` | object? | Additional platform-specific metadata (e.g., `tag_id`, `avd` for Android) |

**Android-Specific Field Semantics**:

| Field | Running Emulator | Shutdown Emulator | Physical Device |
|-------|-----------------|-------------------|-----------------|
| `identifier` | AVD name (e.g., `Pixel_8_API_35`) | AVD name | Serial (e.g., `R58M32XXXXX`) |
| `version` | API level from `ro.build.version.sdk` | API level from AVD config | API level from `ro.build.version.sdk` |
| `version_name` | `ro.build.version.release` | Mapped from API level | `ro.build.version.release` |
| `manufacturer` | `ro.product.manufacturer` | `"Google"` | `ro.product.manufacturer` |
| `platform_architecture` | Raw ABI (e.g., `arm64-v8a`) | From AVD config | Raw ABI from `ro.product.cpu.abi` |
| `sub_model` | Merged from AVD tag + `PlayStore.enabled` | From AVD tag + `PlayStore.enabled` | N/A |

#### SdkPackage Schema

```json
{
  "path": "platforms;android-35",
  "version": "3",
  "description": "Android SDK Platform 35",
  "location": "/Users/dev/Library/Android/sdk/platforms/android-35",
  "is_installed": true
}
```

| Field | Type | Description |
|-------|------|-------------|
| `path` | string | Package identifier (required) |
| `version` | string? | Installed/available version |
| `description` | string? | Human-readable description |
| `location` | string? | Install path (for installed packages) |
| `is_installed` | bool | True if currently installed |

#### XcodeInstallation Schema

```json
{
  "path": "/Applications/Xcode.app",
  "developer_dir": "/Applications/Xcode.app/Contents/Developer",
  "version": "16.2",
  "build": "16C5032a",
  "is_selected": true
}
```

| Field | Type | Description |
|-------|------|-------------|
| `path` | string | Path to Xcode.app bundle (required) |
| `developer_dir` | string | Path to Developer directory inside the bundle |
| `version` | string | Xcode version string |
| `build` | string | Xcode build number |
| `is_selected` | bool | True if this is the currently active Xcode (via `xcode-select`) |

### 7.3 Capabilities Model

| Command | Windows | macOS | Requires Elevation |
|---------|---------|-------|-------------------|
| `doctor` | ✓ | ✓ | No |
| `doctor --fix` | ✓ | ✓ | Sometimes* |
| `dotnet maui device list` | ✓ | ✓ | No |
| `dotnet maui device screenshot` | ✓ | ✓ | No |
| `dotnet maui android sdk list` | ✓ | ✓ | No |
| `dotnet maui android sdk install` | ✓ | ✓ | No |
| `dotnet maui android sdk accept-licenses` | ✓ | ✓ | No |
| `dotnet maui android emulator create` | ✓ | ✓ | No |
| `dotnet maui android emulator start` | ✓ | ✓ | No |
| `dotnet maui android emulator stop` | ✓ | ✓ | No |
| `dotnet maui android emulator delete` | ✓ | ✓ | No |
| `dotnet maui android install` | ✓ | ✓ | No |
| `dotnet maui android jdk check` | ✓ | ✓ | No |
| `dotnet maui android jdk install` | ✓ | ✓ | No |
| `dotnet maui android jdk list` | ✓ | ✓ | No |
| `dotnet maui apple simulator list` | — | ✓ | No |
| `dotnet maui apple simulator start` | — | ✓ | No |
| `dotnet maui apple simulator stop` | — | ✓ | No |
| `dotnet maui apple simulator create` | — | ✓ | No |
| `dotnet maui apple simulator delete` | — | ✓ | No |
| `dotnet maui apple runtime list` | — | ✓ | No |
| `dotnet maui apple runtime install` | — | ✓ | Yes (admin) |
| `dotnet maui apple xcode list` | — | ✓ | No |
| `dotnet maui apple xcode select` | — | ✓ | Yes (sudo) |
| `dotnet maui device logs` | ✓ | ✓ | No |

*Elevation required for: installing Android SDK to system locations, installing Xcode runtimes

---

## 8. User Experience

### 8.1 IDE UI Flows

> **See [IDE Integration](./maui-devtools-ide-integration.md)** for detailed VS Code status bars, environment panels, fix progress dialogs, and Visual Studio menu integration.

### 8.2 Interactive Prompting

When running in interactive mode (terminal), the tool prompts for missing information:

**Example: Emulator Creation with Missing Parameters**:
```
$ maui android emulator create

? Emulator name: My_Pixel_5

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

Creating emulator 'My_Pixel_5'... done
```

**Non-Interactive Mode**:
```
$ maui android emulator create --non-interactive
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

### 8.3 Copilot-Assisted Troubleshooting

> **See [AI Agent Integration](./maui-devtools-ai-integration.md)** for detailed Copilot escalation triggers, context handoff schema, MCP tool integration, and example conversations.

**Summary**: When automated fixes fail, the tool can escalate to GitHub Copilot with structured diagnostic context. The fallback hierarchy is: Automated Fix → Guided Manual Fix → Copilot-Assisted Troubleshooting → Community/Support Escalation.

---

## 9. Diagnostics

### 9.1 Redaction Rules

All telemetry and logs follow these redaction rules:

| Data Type | Rule | Example |
|-----------|------|---------|
| File paths | Replace with `<path>` or keep structure | `/Users/name/code` → `<user-home>/code` |
| Device names | Replace with type | `John's iPhone` → `<iPhone>` |
| Serial numbers | Redact | `R58M32XXXXX` → `<android-serial>` |
| UDIDs | Redact | `A1B2C3...` → `<simulator-udid>` |
| Error messages | Keep, unless contains path | Preserved |

---


## 10. MVP vs vNext Features

### MVP (v1.0)

| Priority | Feature | Status |
|----------|---------|--------|
| P0 | `doctor` command with status reporting | ✅ Implemented |
| P0 | `doctor --fix` for automated remediation | ✅ Implemented |
| P0 | Android SDK detection and installation | ✅ Implemented |
| P0 | Android emulator creation and management | ✅ Implemented |
| P0 | Android install (JDK + SDK from scratch) | ✅ Implemented |
| P0 | Android JDK management (check, install, list) | ✅ Implemented |
| P0 | Android SDK license acceptance | ✅ Implemented |
| P0 | iOS simulator listing and start/stop/create/delete | ✅ Implemented |
| P0 | iOS runtime listing | ✅ Implemented |
| P0 | Xcode installation listing and selection | ✅ Implemented |
| P0 | Unified `device list` command (emulators, simulators, physical devices) | ✅ Implemented |
| P0 | `device screenshot` command | ✅ Implemented |
| P0 | JSON output for all commands | ✅ Implemented |
| P0 | VS Code extension integration | ✅ In progress |
| P1 | Device log streaming | ✅ Implemented |
| P1 | Interactive prompting | Planned |

### vNext (v1.x / v2.0)

| Priority | Feature |
|----------|---------|
| P1 | Visual Studio extension integration |
| P1 | iOS runtime installation guidance |
| P1 | `--non-interactive`, `--correlation-id`, `--offline` global options |
| P2 | AVD snapshot management |
| P2 | Windows SDK management |
| P2 | Linux host support (Android only) |
| P2 | Physical iOS device support (requires signing) |
| P2 | Telemetry (opt-in) |
| Future | MCP server integration for AI agents |
| Future | Cloud-hosted device support |

---

## Related Documents

| Document | Description |
|----------|-------------|
| [AI Agent Integration](./maui-devtools-ai-integration.md) | Copilot integration, permission model, MCP tools |
| [IDE Integration](./maui-devtools-ide-integration.md) | VS Code and Visual Studio UI flows |

---

## Revision History

| Version | Date | Changes |
|---------|------|---------|
| 2.9-draft | 2026-02-10 | Unified command naming: renamed `avd` → `emulator` for Android, `simulator boot` → `simulator start`, `simulator shutdown` → `simulator stop` for Apple — consistent start/stop verbs across platforms |
| 2.8-draft | 2026-02-09 | Synced with implementation: renamed `bootstrap` → `install` per PR feedback; removed `deploy`, `config`, `diagnostic-bundle` commands (covered by `dotnet run`/`dotnet build`); removed telemetry section (vNext); added Xcode list/select, XcodeInstallation schema, Android device field semantics, `type`/`state`/`details` to MauiDevice |
| 2.6-draft | 2026-02-04 | Condensed §3 Goals and §4 Personas into single section; Now 10 sections |
| 2.5-draft | 2026-02-04 | Removed §11 Security, §12 Extensibility; Added physical device support to device list |
| 2.4-draft | 2026-02-04 | Removed §13 Testing Strategy, §14 Rollout Plan |
| 2.3-draft | 2026-02-04 | Removed §15-20 (Open Questions, Appendix, Acceptance Criteria, etc.) |
| 2.2-draft | 2026-02-04 | Removed daemon mode, §5.7 Deploy vs Run, G6, NG6 |
| 2.1-draft | 2026-02-04 | Extracted AI/IDE integration to separate documents; Removed JSON-RPC API |
| 1.0-draft | 2026-02-03 | Initial draft |

---

*End of Specification*

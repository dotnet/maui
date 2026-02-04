# MAUI Dev Tools Client — Product Specification

**Version**: 2.5-draft  
**Status**: Proposal  
**Last Updated**: 2026-02-04

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
11. [MVP vs vNext Features](#11-mvp-vs-vnext-features)

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

#### DP1: Delegate to Native Toolchains — Do Not Reimplement

**This tool MUST NOT implement custom logic to download, install, or configure platform components.** Instead, it delegates all such operations to the native platform tools:

| Platform | Native Tools Used | What They Handle |
|----------|-------------------|------------------|
| Android | `sdkmanager`, `avdmanager`, `adb`, `emulator` | SDK packages, AVDs, device communication, emulator lifecycle |
| Apple | `xcrun simctl`, `xcode-select`, `xcodebuild` | Simulators, runtimes, Xcode selection, build tools |
| Windows | Windows SDK installer, VS Build Tools | Windows SDK components |

**Rationale**:
- Native tools are **authoritative** and always up-to-date with platform changes
- Avoids **maintenance burden** of tracking repository URLs, package formats, and installation procedures
- Reduces **security risk** from implementing custom download/verification logic
- Ensures **consistency** between manual and automated installations
- Leverages **existing testing** of platform tools by their maintainers

**Example**:
```
❌ WRONG: Download android-sdk-*.zip from Google, extract, configure PATH
✅ RIGHT: Invoke `sdkmanager "platform-tools" "platforms;android-34"` and parse output
```

This tool's value is in **orchestration, detection, and unified UX**—not in reimplementing what the platform tools already do well.

#### DP2: Consolidate Existing Visual Studio Acquisition Repositories

This tool enables **consolidation of existing Visual Studio repositories** that currently implement Android acquisition separately:

| Repository | Current Function | Future State |
|------------|------------------|--------------|
| [ClientTools.android-acquisition](https://devdiv.visualstudio.com/DevDiv/_git/ClientTools.android-acquisition) | Android SDK acquisition for VS | **Deprecate**: Use `dotnet maui android sdk install` |
| [android-platform-support](https://devdiv.visualstudio.com/DevDiv/_git/android-platform-support) | Android platform support for VS | **Deprecate**: Use `dotnet maui android` commands |

**Benefits of Consolidation**:
- **Single codebase** for Android tooling across VS, VS Code, CLI, and CI
- **Reduced duplication** of detection and installation logic
- **Consistent behavior** across all Microsoft developer tools
- **Shared bug fixes** and improvements
- **Simplified maintenance** with one team owning the tooling

**Migration Path**:
1. Visual Studio installer integrates `dotnet maui` tool
2. VS Android features call `dotnet maui android` commands instead of internal libraries
3. Internal repositories enter maintenance mode
4. After VS release cycle, internal repositories are archived

#### DP3: Stateless Architecture

**The CLI operates statelessly.** Each command reads state, acts, and exits.

**Rationale**:
- Stateless commands are easier to debug, test, and reason about
- AI agents prefer deterministic request/response
- Performance bottleneck is native tool execution, not process spin-up

**State Caching**:
Stateless does not mean slow. The tool uses file-system caching:
```
~/.maui/cache/
├── devices.json          # TTL: 30 seconds
├── android-sdk-state.json # TTL: 5 minutes
├── apple-runtimes.json   # TTL: 5 minutes
└── doctor-report.json    # TTL: 1 minute
```

Commands read from cache if fresh, otherwise invoke native tools and update cache.

#### DP4: Machine-First Output — The User is the AI

**Every command MUST support `--json` output with a stable, versioned schema.**

This tool is designed for three consumers in priority order:
1. **AI Agents** (GitHub Copilot, IDE assistants) — need structured data to reason about
2. **CI/CD Pipelines** — need deterministic exit codes and parseable output
3. **Human Developers** — need readable summaries with color and formatting

**Implication**: If an error message is ambiguous plain text, AI agents will fail to use the tool reliably. Every failure must be expressed as structured data.

```bash
# Human-friendly (default)
dotnet maui doctor
# ✓ .NET SDK 9.0.100
# ✗ Android SDK not found

# Machine-friendly (for AI/CI)
dotnet maui doctor --json
# { "status": "unhealthy", "checks": [...], "errors": [...] }
```

**Required flags for all commands**:
| Flag | Purpose |
|------|---------|
| `--json` | Output structured JSON instead of human-readable text |
| `--dry-run` | Show what would be done without executing (enables "what will this do?" UX) |
| `--ci` | Strict mode: no interactive prompts, non-zero exit on warnings, machine-readable only |

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
1. Marcus runs `dotnet maui doctor` in terminal
2. Output shows:
   ```
   ✓ .NET SDK 9.0.100
   ✓ MAUI workload 9.0.0
   ✓ Xcode 16.0 (/Applications/Xcode.app)
   ✓ Android SDK (/Users/marcus/Library/Android/sdk)
   ⚠ iOS 18.0 runtime not installed (iOS 17.4 available)
   ⚠ No iPhone 16 simulator available
   ```
3. He runs `dotnet maui doctor --fix`
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
       dotnet maui doctor --fix --non-interactive --json > setup-report.json
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

> **See [AI Agent Integration](./maui-devtools-ai-integration.md)** for detailed AI agent personas, Copilot-assisted troubleshooting, and permission model.

**Summary**: AI agents (GitHub Copilot, IDE assistants) can invoke `dotnet maui doctor --json` to diagnose environment issues, receive structured responses with fix commands, and execute fixes with user permission gates.

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
| FR-A14 | Bootstrap full Android environment (JDK + SDK) from scratch | P0 |
| FR-A15 | Detect JDK installation and version | P0 |
| FR-A16 | Install OpenJDK if missing (version 17 default, 21 supported) | P0 |
| FR-A17 | Use platform-appropriate default paths when env vars not set | P0 |

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

### 5.6 Bootstrap State Machine

**Critical**: The tool must handle the "bootstrap gap" — the chicken-and-egg problem where native tools (sdkmanager, xcrun) don't exist yet.

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

#### Android Bootstrap

The tool can fully bootstrap an Android development environment from scratch, including JDK and SDK installation.

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

**Bootstrap States**:

| State | Detection | Behavior |
|-------|-----------|----------|
| `JDK_MISSING` | No `java` in PATH, no JDK at default paths | Install OpenJDK 17 to default path |
| `SDK_MISSING` | `ANDROID_HOME` not set, no SDK at standard paths | Install command-line tools to default path |
| `PARTIAL` | SDK exists but `sdkmanager` missing/broken | Repair SDK or reinstall cmdline-tools |
| `READY` | `java -version` succeeds AND `sdkmanager --list` succeeds | Delegate all operations to native tools |

**Bootstrap Command**:
```bash
# Full bootstrap: JDK + SDK + recommended packages
dotnet maui android bootstrap --accept-licenses

# With custom paths
dotnet maui android bootstrap --jdk-path ~/my-jdk --sdk-path ~/my-sdk --accept-licenses
```

This command:
1. Checks for JDK; if missing, downloads and installs OpenJDK 17
2. Sets `JAVA_HOME` for the session (prints guidance for permanent setup)
3. Downloads Android command-line tools (if missing)
4. Accepts SDK licenses non-interactively (if `--accept-licenses`)
5. Installs recommended packages (platform-tools, build-tools, emulator, system image)
6. Prints environment variable guidance (doesn't modify shell config)

**JDK Management Commands**:
```bash
# Check JDK status
dotnet maui android jdk status

# Install OpenJDK (default: version 17)
dotnet maui android jdk install
dotnet maui android jdk install --version 21

# List available JDK versions
dotnet maui android jdk list-available
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

#### Apple Bootstrap

| State | Detection | Behavior |
|-------|-----------|----------|
| `MISSING` | No Xcode.app at `/Applications/Xcode*.app` | Error: "Install Xcode from App Store" (cannot auto-install) |
| `CLI_ONLY` | Only Command Line Tools installed | Error: "Full Xcode required for simulators" |
| `READY` | `xcrun simctl list` succeeds | Delegate all operations to native tools |

**Important**: Apple bootstrap is limited because:
- Xcode cannot be installed programmatically (App Store only)
- Runtime downloads require Xcode to be open at least once (license acceptance)

```bash
dotnet maui apple bootstrap
```

This command:
1. Verifies Xcode installation
2. Runs `xcode-select --install` if CLI tools missing
3. Prompts user to accept Xcode license if needed
4. Reports status and next steps

#### Windows Bootstrap

| State | Detection | Behavior |
|-------|-----------|----------|
| `MISSING` | No Windows SDK detected | Provide download link |
| `NO_DEV_MODE` | Developer Mode disabled | Guide user to enable (requires Settings app) |
| `READY` | SDK present, Developer Mode enabled | Operational |

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
| NFR-P3 | Downloaded artifacts must be cached locally |
| NFR-P4 | Read operations must use direct file parsing (not CLI wrappers) for performance |

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
- `dotnet maui doctor` reports capability with reason (e.g., "emulator: unavailable (virtualization disabled)")
- `dotnet maui android avd start` fails fast with `E2010` "Hardware acceleration unavailable"

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

### 7.2 Component Descriptions

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

> **See [IDE Integration](./maui-devtools-ide-integration.md)** for detailed VS Code and Visual Studio UI flows, status panels, and menu integrations.

**Summary**: IDEs spawn `dotnet maui` as a child process, invoke `dotnet maui doctor --json` on workspace open, display issues in their problems/error list panels, and provide commands for environment setup with progress notifications.

### 7.5 Concurrency Model

| Aspect | Behavior |
|--------|----------|
| Read Operations | Parallelized (multiple simultaneous queries allowed) |
| Write Operations | Serialized (single writer at a time) |
| Lock File | `$TMPDIR/maui-devtools.lock` prevents concurrent writes |
| Request Timeout | Requests waiting >30s for lock rejected with `E5010` |

### 7.6 SDK Conflict Resolution

When multiple SDK installations are detected:

**Android SDK Precedence** (first match wins):
1. `ANDROID_HOME` environment variable
2. `ANDROID_SDK_ROOT` environment variable  
3. Visual Studio configured path (Windows)
4. Android Studio configured path (`~/.android/sdk` or registry)
5. Default location (`~/Library/Android/sdk` on macOS, `%LOCALAPPDATA%\Android\Sdk` on Windows)

**Conflict Handling**:
- `dotnet maui doctor` reports all detected SDKs with recommendation
- `dotnet maui config set android-sdk-path <path>` to override
- `--sdk-path` flag available on all android commands for one-off override

**Android Studio Coexistence**:
- Detect Android Studio installation
- Warn if both AS and tool would manage same SDK
- Offer to adopt AS's SDK rather than installing duplicate

### 7.7 Migration from Existing Setups

When an existing SDK is detected:

1. **Validate** existing SDK health before offering modifications
2. **Adopt or Fresh**: Offer "adopt existing" vs "install fresh" options
3. **Non-Destructive**: Never delete or modify user's existing SDK without explicit consent
4. **Override Support**: `--sdk-path` for non-standard locations

### 7.8 Error Contract Specification

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
| `E2002` | platform | sdkmanager not found | auto_fixable (bootstrap) |
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
      "failed_command": "dotnet maui android avd create ...",
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

## 8. Public API Surface

### 8.1 CLI Commands

The tool is invoked as `dotnet maui <command>`, integrating naturally with the .NET CLI. Platform-specific commands use the target framework moniker pattern (`ios`, `android`, `maccatalyst`, `windows`).

#### Command Hierarchy

```
dotnet maui
├── doctor                    # Check environment health
│   ├── --fix [issue-id]      # Auto-fix all or specific issue
│   ├── --platform <p>        # Filter: android, ios, windows, maccatalyst (repeatable)
│   ├── --json                # Output as JSON
│   ├── --non-interactive     # No prompts (for CI)
│   └── --offline             # Skip network checks
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
│   ├── bootstrap             # Bootstrap Android SDK from scratch
│   │   ├── --accept-licenses # Non-interactively accept licenses
│   │   ├── --recommended     # Install recommended components
│   │   ├── --jdk-path <dir>  # JDK installation directory
│   │   └── --sdk-path <dir>  # SDK installation directory
│   ├── jdk                   # JDK management
│   │   ├── status            # Check JDK installation status
│   │   ├── install           # Install OpenJDK
│   │   │   ├── --version     # JDK version (default: 17)
│   │   │   └── --path <dir>  # Installation directory
│   │   └── list-available    # List available JDK versions
│   ├── sdk
│   │   ├── list              # List installed packages
│   │   ├── list-available    # List available packages
│   │   ├── install <pkg>     # Install package
│   │   ├── accept-licenses   # Accept all SDK licenses
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
│   ├── install               # Install APK
│   │   ├── --device          # Target device
│   │   └── <apk-path>        # APK file
│   └── logcat                # Stream Android logs
│       ├── --device          # Target device
│       └── --filter          # Tag filter (e.g., "MainActivity:V *:S")
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
│   │   ├── boot              # Boot simulator
│   │   │   └── --udid        # Simulator UDID (or --name)
│   │   ├── shutdown          # Shutdown simulator
│   │   │   └── --udid        # Simulator UDID
│   │   └── delete            # Delete simulator
│   │       └── --udid        # Simulator UDID
│   └── runtime
│       ├── list              # List installed iOS/macOS runtimes
│       └── install           # Install runtime (guidance)
│           └── --version     # Runtime version
│
├── windows                   # Windows-specific commands (Windows only)
│   ├── sdk
│   │   └── list              # List Windows SDK installations
│   └── developer-mode
│       ├── status            # Check Developer Mode status
│       └── enable            # Guide to enable Developer Mode
│
├── deploy                    # Deploy app to device
│   ├── --device <id>         # Target device
│   ├── --project <path>      # Project file (default: current dir)
│   ├── --configuration       # Build configuration
│   ├── --framework           # Target framework
│   ├── --install-only        # Install without launching
│   ├── --wait                # Wait for app to exit (returns app exit code)
│   ├── --debug               # Launch with debugger attached
│   └── --timeout <seconds>   # Timeout for launch detection (default: 30)
│
├── config                    # Configuration management
│   ├── list                  # List all config values
│   ├── get <key>             # Get config value
│   └── set <key> <value>     # Set config value
│
├── diagnostic-bundle         # Export diagnostic info
│   └── --output <path>       # Output zip file
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
dotnet maui android avd create --name Pixel_8 --device pixel_8
dotnet maui android avd start --name Pixel_8 --wait
dotnet maui android logcat --device emulator-5554

# Apple-specific (macOS only)
dotnet maui apple simulator list
dotnet maui apple simulator boot --name "iPhone 16 Pro"
dotnet maui apple runtime list

# Windows-specific
dotnet maui windows developer-mode status
```

#### Global Options (Available on All Commands)

> **Design Principle DP4**: Every command MUST support `--json` output with a stable, versioned schema. The primary consumer is AI agents, not humans.

| Option | Description | Required |
|--------|-------------|----------|
| `--json` | Output as JSON (machine-readable) | **Mandatory on all commands** |
| `--dry-run` | Show what would be done without executing | **Mandatory on write commands** |
| `--ci` | Strict mode: no prompts, warnings become errors, JSON output forced | Recommended |
| `--verbose` | Enable verbose logging | Optional |
| `--non-interactive` | Disable prompts; fail if input needed | Optional |
| `--correlation-id` | Set correlation ID for tracing | Optional |
| `--offline` | Skip network operations; use cached data only | Optional |

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
| `dotnet maui android sdk list` | List SDK packages | `--json` | Package list | 0=success, 2=error |
| `dotnet maui android sdk install` | Install SDK package | `<package>`, `--accept-licenses` | Progress, result | 0=success, 5=not found, 2=error |
| `dotnet maui android avd create` | Create emulator | `--name`, `--device`, `--image` | AVD name | 0=success, 1=exists, 2=error |
| `dotnet maui android avd start` | Start emulator | `--name`, `--wait`, `--cold-boot` | Device serial | 0=success, 5=not found, 2=error |
| `dotnet maui apple simulator list` | List simulators | `--runtime`, `--device-type`, `--state` | Simulator list | 0=success, 2=error |
| `dotnet maui apple simulator boot` | Boot simulator | `--udid` or `--name` | UDID | 0=success, 5=not found, 2=error |
| `dotnet maui apple runtime list` | List iOS runtimes | `--json` | Runtime list | 0=success, 2=error |
| `dotnet maui config set` | Set configuration | `<key>`, `<value>` | Confirmation | 0=success, 2=error |

### 8.2 Capabilities Model

| Command | Windows | macOS | Requires Elevation |
|---------|---------|-------|-------------------|
| `doctor` | ✓ | ✓ | No |
| `doctor --fix` | ✓ | ✓ | Sometimes* |
| `dotnet maui device list` | ✓ | ✓ | No |
| `dotnet maui device screenshot` | ✓ | ✓ | No |
| `dotnet maui android sdk list` | ✓ | ✓ | No |
| `dotnet maui android sdk install` | ✓ | ✓ | No |
| `dotnet maui android avd create` | ✓ | ✓ | No |
| `dotnet maui android avd start` | ✓ | ✓ | No |
| `dotnet maui android logcat` | ✓ | ✓ | No |
| `dotnet maui apple simulator list` | — | ✓ | No |
| `dotnet maui apple simulator boot` | — | ✓ | No |
| `dotnet maui apple simulator create` | — | ✓ | No |
| `dotnet maui apple runtime list` | — | ✓ | No |
| `dotnet maui apple runtime install` | — | ✓ | Yes (admin) |
| `dotnet maui device logs` | ✓ | ✓ | No |

*Elevation required for: installing Android SDK to system locations, installing Xcode runtimes

---

## 9. User Experience

### 9.1 IDE UI Flows

> **See [IDE Integration](./maui-devtools-ide-integration.md)** for detailed VS Code status bars, environment panels, fix progress dialogs, and Visual Studio menu integration.

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

### 9.3 Copilot-Assisted Troubleshooting

> **See [AI Agent Integration](./maui-devtools-ai-integration.md)** for detailed Copilot escalation triggers, context handoff schema, MCP tool integration, and example conversations.

**Summary**: When automated fixes fail, the tool can escalate to GitHub Copilot with structured diagnostic context. The fallback hierarchy is: Automated Fix → Guided Manual Fix → Copilot-Assisted Troubleshooting → Community/Support Escalation.

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


## 11. MVP vs vNext Features

### MVP (v1.0)

| Priority | Feature |
|----------|---------|
| P0 | `doctor` command with status reporting |
| P0 | `doctor --fix` for automated remediation |
| P0 | Android SDK detection and installation |
| P0 | Android AVD creation and management |
| P0 | iOS simulator listing and boot/shutdown |
| P0 | Unified `device list` command (emulators, simulators, physical devices) |
| P0 | `device screenshot` command |
| P0 | JSON output for all commands |
| P0 | VS Code extension integration |
| P1 | Logcat streaming |
| P1 | iOS runtime listing |
| P1 | Interactive prompting |

### vNext (v1.x / v2.0)

| Priority | Feature |
|----------|---------|
| P1 | Visual Studio extension integration |
| P1 | iOS runtime installation guidance |
| P1 | APK install/uninstall |
| P2 | AVD snapshot management |
| P2 | Simulator log streaming |
| P2 | Windows SDK management |
| P2 | Linux host support (Android only) |
| P2 | Physical iOS device support (requires signing) |
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
| 2.5-draft | 2026-02-04 | Removed §11 Security, §12 Extensibility; Added physical device support to device list |
| 2.4-draft | 2026-02-04 | Removed §13 Testing Strategy, §14 Rollout Plan |
| 2.3-draft | 2026-02-04 | Removed §15-20 (Open Questions, Appendix, Acceptance Criteria, etc.) |
| 2.2-draft | 2026-02-04 | Removed daemon mode, §5.7 Deploy vs Run, G6, NG6 |
| 2.1-draft | 2026-02-04 | Extracted AI/IDE integration to separate documents; Removed JSON-RPC API |
| 1.0-draft | 2026-02-03 | Initial draft |

---

*End of Specification*

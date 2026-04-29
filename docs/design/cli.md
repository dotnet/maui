---
description: "Design document for the maui CLI tool"
date: 2026-01-07
updated: 2026-02-27
---

# `maui` CLI Design Document

## Overview

The `maui` CLI is a command-line tool for .NET MAUI development that provides two main capabilities:

1. **Environment setup** — manages Android SDK/JDK, Xcode runtimes, simulators, and emulators
2. **App inspection** — captures screenshots, streams logs, and inspects the visual tree of running apps

It is designed for three consumers: **AI agents**, **CI/CD pipelines**, and **humans**.

**Full specification**: [PR #33865](https://github.com/dotnet/maui/pull/33865) — covers architecture, error contracts, IDE integration, JSON schemas, and vNext roadmap.

## Motivation

The "vibe coding" experiment with WPF ([vibe-wpf]) showed that AI
agents can effectively develop applications when given the right
tools. .NET MAUI, however, spans multiple platforms with different
command-line interfaces: capturing a screenshot on iOS requires `xcrun
simctl io booted screenshot`, while Android uses `adb exec-out
screencap`. Similarly, log access, visual tree inspection, and device
management all have platform-specific implementations.

The `maui` CLI provides a unified interface across Android,
iOS, macOS, Windows, and Mac Catalyst, making these operations simple
and consistent for both developers and AI agents.

### Design Principles

1. **Delegate to native toolchains** — wraps `sdkmanager`, `adb`, `xcrun simctl`, etc.
2. **Reuse shared libraries** — leverages [`dotnet/android-tools`](https://github.com/dotnet/android-tools) (`Xamarin.Android.Tools.AndroidSdk`) for SDK/JDK discovery, and contributes new capabilities (JDK installation, SDK bootstrap, license acceptance) back to it.
3. **Machine-first output** — every command supports `--json`
4. **Stateless** — each command reads state, acts, and exits
5. **Complement `dotnet run`** — uses the same device identifiers and framework options as [`dotnet run` for .NET MAUI][dotnet-run-spec]

## Goals

1. **Environment setup**: Manage Android SDK/JDK, Xcode runtimes,
   simulators, and emulators from a single tool

2. **Screenshot capture**: Enable AI agents to capture screenshots of
   running .NET MAUI applications to validate visual changes

3. **Log access**: Provide unified access to platform-specific device
   logs (logcat, Console, etc.)

4. **Visual tree inspection**: Allow agents to inspect the runtime
   visual tree structure and properties (.NET MAUI visual tree)

5. **Developer experience**: Integrate seamlessly with existing
   `dotnet` CLI workflows, this should fit in with `dotnet run`,
   `dotnet watch`, etc.

## Installation and Invocation

The CLI is available through multiple invocation methods:

```bash
# Direct tool invocation (after install)
maui screenshot -o screenshot.png

# Via the .NET CLI
dotnet maui screenshot -o screenshot.png

# Inline install and invocation (no prior install needed)
dotnet tool exec -y Microsoft.Maui.Cli screenshot -o screenshot.png
```

### Installation

```bash
# As a global tool
dotnet tool install --global Microsoft.Maui.Cli

# As a local tool (recommended for projects)
dotnet tool install Microsoft.Maui.Cli

# Restore local tools
dotnet tool restore
```

The tool installs as `maui` on PATH. All commands in this document use the `maui` form.

The .NET workload specification includes support for automatically
installing tools from workloads via the `tools-packs` feature (see
[workload manifest specification][workload-spec]). However, this
feature is not yet implemented. Once available, `Microsoft.Maui.Cli`
could be automatically installed when the `maui` workload is
installed, eliminating the need for manual tool installation.

Until then, manual installation via `dotnet tool install` will be how
we prove out the `maui` CLI.

[workload-spec]: https://github.com/dotnet/designs/blob/566ad4cafcc578d6389c215c61924ee9e07dcb29/accepted/2020/workloads/workload-manifest.md#tools-packs

## Global Options

All commands support:

| Flag | Description |
|------|-------------|
| `--json` | Structured JSON output |
| `--verbose` | Detailed logging |
| `--interactive` | Control interactive prompts (default: `true` for terminals, `false` in CI or when output is redirected) |
| `--dry-run` | Preview actions without executing |
| `--platform <p>` | Filter by platform: `android`, `ios`, `maccatalyst`, `windows` |

**Interactivity detection** follows the same pattern as `dotnet` CLI — auto-detects CI environments (`TF_BUILD`, `GITHUB_ACTIONS`, `CI`, etc.) and checks `Console.IsOutputRedirected`.

## Environment Setup Commands

### Android

| Command | Description |
|---------|-------------|
| `maui android install` | Install JDK + SDK + recommended packages |
| `maui android install --accept-licenses` | Non-interactive install |
| `maui android install --packages <list>` | Install specific packages |
| `maui android jdk check` | Check JDK status |
| `maui android jdk install` | Install OpenJDK 21 |
| `maui android jdk list` | List installed JDKs |
| `maui android sdk list` | List installed packages |
| `maui android sdk list --available` | Show available packages |
| `maui android sdk install <packages>` | Install package(s) |
| `maui android sdk accept-licenses` | Accept all licenses |
| `maui android sdk uninstall <package>` | Uninstall a package |
| `maui android emulator list` | List emulators |
| `maui android emulator create <name>` | Create emulator (auto-detects system image) |
| `maui android emulator start <name>` | Start emulator |
| `maui android emulator stop <name>` | Stop emulator |
| `maui android emulator delete <name>` | Delete emulator |

Install paths and defaults are handled by [`dotnet/android-tools`](https://github.com/dotnet/android-tools).

### Apple (macOS only)

| Command | Description |
|---------|-------------|
| `maui apple install [--accept-license] [--runtime <version>]` | Optionally accepts Xcode license and installs simulator runtimes. Could prompt user to install Xcode in the future |
| `maui apple check` | Check Xcode, runtimes, and environment status |
| `maui apple xcode check` | Check Xcode installation and license |
| `maui apple xcode list` | List Xcode installations |
| `maui apple xcode select <path>` | Switch active Xcode |
| `maui apple xcode accept-license` | Accept Xcode license |
| `maui apple simulator list` | List simulators |
| `maui apple simulator create <name> <type> <runtime>` | Create simulator |
| `maui apple simulator start <id>` | Start simulator |
| `maui apple simulator stop <id>` | Stop simulator |
| `maui apple simulator delete <id>` | Delete simulator |
| `maui apple runtime check` | Check runtime status |
| `maui apple runtime list` | List installed runtimes |
| `maui apple runtime list --all` | List all runtimes (installed and downloadable) |
| `maui apple runtime install <version>` | Install an iOS runtime |

> **License flag naming**: Android uses `accept-licenses` (plural) because `sdkmanager` requires accepting multiple SDK component licenses. Apple uses `accept-license` (singular) because `xcodebuild -license accept` accepts one unified Xcode license agreement.

### Implementation References

The `maui` CLI delegates to shared libraries for platform operations:

**Android** — [`dotnet/android-tools`](https://github.com/dotnet/android-tools) (`Xamarin.Android.Tools.AndroidSdk`):

| Feature | Implementation |
|---------|---------------|
| SDK discovery, bootstrap & license acceptance | [`SdkManager`](https://github.com/dotnet/android-tools/pull/275) |
| JDK discovery & installation | [`JdkInstaller`](https://github.com/dotnet/android-tools/pull/274) |
| ADB device management | [`AdbRunner`](https://github.com/dotnet/android-tools/pull/282) |
| AVD / Emulator management | [`AvdManagerRunner`](https://github.com/dotnet/android-tools/pull/283), [`EmulatorRunner`](https://github.com/dotnet/android-tools/pull/284) |

**Apple** — wraps native toolchains directly:

| Feature | Native tool |
|---------|------------|
| Simulator management | `xcrun simctl` (list, create, boot, shutdown, delete) |
| Runtime management | `xcrun simctl runtime` (list, add) |
| Xcode management | `xcode-select`, `xcodebuild -license` |
| Device detection | `xcrun devicectl list devices` (physical), `xcrun simctl list` (simulators) |

Apple operations use [AppleDev.Tools][appledev-tools] for `simctl` and `devicectl` wrappers.

### Exit Codes

All commands use consistent exit codes:

| Code | Meaning |
|------|---------|
| 0 | Success |
| 1 | General error |
| 2 | Environment/configuration error |
| 3 | Permission denied (elevation required) |
| 4 | Network error (download failed) |
| 5 | Resource not found |

## Device Discovery

### `maui device list`

Lists connected devices, running emulators, and available simulators
across all platforms from a single command.

**Usage:**

```bash
maui device list [--platform <p>] [--json]
```

**Options:**

- `--platform <PLATFORM>`: Filter by platform (`android`, `ios`,
  `maccatalyst`). If omitted, lists all platforms.
- `--json`: Structured JSON output for machine consumption.

**Human-readable output:**

```
ID                                     Description              Type       Platform   Status
emulator-5554                          Pixel 7 - API 35         Emulator   android    Online
0A041FDD400327                         Pixel 7 Pro              Device     android    Online
94E71AE5-8040-4DB2-8A9C-6CD24EF4E7DE  iPhone 16 - iOS 26.0     Simulator  ios        Shutdown
FBF5DCE8-EE2B-4215-8118-3A2190DE1AD7  iPhone 14 - iOS 26.0     Simulator  ios        Booted
AF40CC64-2CDB-5F16-9651-86BCDF380881  My iPhone 15             Device     ios        Paired
```

**JSON output (`--json`):**

```json
{
  "devices": [
    {
      "id": "emulator-5554",
      "description": "Pixel 7 - API 35",
      "type": "Emulator",
      "platform": "android",
      "status": "Online"
    },
    {
      "id": "FBF5DCE8-EE2B-4215-8118-3A2190DE1AD7",
      "description": "iPhone 14 - iOS 26.0",
      "type": "Simulator",
      "platform": "ios",
      "status": "Booted"
    }
  ]
}
```

The `id` field is the same identifier accepted by `dotnet run --device
<id>`, so output from `maui device list` can be piped directly into a
run command.

### Two Approaches to Device Enumeration

There are two ways to enumerate devices, each suited to different
scenarios.

#### Approach A: Via `dotnet run --list-devices` (project-based)

The .NET SDK (≥ .NET 11) provides `dotnet run --list-devices`, which
calls the [`ComputeAvailableDevices`][compute-android] MSBuild target
defined by each platform workload ([spec][dotnet-run-spec]):

- **Android** ([dotnet/android]): calls `adb devices`, returns
  serial, description, type (Device/Emulator), status, model
- **Apple** ([dotnet/macios]): calls `simctl list` and `devicectl
  list`, returns UDID, description, type (Device/Simulator),
  OS version, RuntimeIdentifier

This approach **requires a project file** — MSBuild evaluates the
`.csproj` to locate the correct workload targets. It also operates
**per-framework**: you select a target framework first, then get
devices for that platform only.

[dotnet/android]: https://github.com/dotnet/android
[dotnet/macios]: https://github.com/dotnet/macios

#### Approach B: Direct native tool invocation (project-free)

The `maui` CLI calls the same native tools directly — `adb devices`,
`xcrun simctl list devices`, `xcrun devicectl list devices` — without
evaluating any MSBuild project. This returns a unified, cross-platform
device list in a single call.

#### Comparison

| | Approach A (MSBuild) | Approach B (Native CLI) |
|---|---|---|
| **Project required** | Yes — needs `.csproj` | No |
| **Cross-platform** | One platform per call (per TFM) | All platforms in one call |
| **Metadata** | Rich (RuntimeIdentifier, workload-specific fields) | Standard (id, description, type, status) |
| **Speed** | Slower (MSBuild evaluation + restore) | Fast (<2s, direct process calls) |
| **ID compatibility** | Source of truth for `dotnet run --device` | Same native IDs — compatible |
| **Requires workloads** | Yes (platform workload must be installed) | Only native tools (`adb`, `simctl`) |
| **Extensible** | Workloads add new device types automatically | Must add support per platform |

#### Scenarios Without a Project

Several real workflows need device enumeration **before** a project
exists or **outside** any project context:

1. **AI agent bootstrapping** — An agent starting a "vibe coding"
   session needs to discover available targets before scaffolding a
   project. It cannot call `dotnet run --list-devices` because there
   is no `.csproj` yet.

2. **IDE startup** — VS Code opens a workspace with no MAUI project
   loaded. The extension needs to populate its device picker to show
   the user what's available. A project-free query is the only option.

3. **Environment validation** — A developer runs `maui device list`
   to answer "can I see my phone?" without needing to be inside any
   project directory. This is a diagnostic step, not a build step.

4. **CI pipeline setup** — A CI script checks that the expected
   emulator or simulator is running before invoking `dotnet run`.
   The check should not depend on a specific project file.

5. **Multi-project solutions** — A solution contains both Android and
   iOS projects. The developer wants a single unified device list
   rather than running `--list-devices` per project.

6. **Cross-platform overview** — `dotnet run --list-devices` shows
   devices for one TFM at a time. A developer switching between
   Android and iOS wants to see everything at once.

#### Recommended Approach

`maui device list` uses **Approach B** (direct native tool invocation)
as its primary implementation:

- It works anywhere — no project, no workload targets, no MSBuild
  evaluation overhead.
- Device identifiers are the same native IDs used by
  `ComputeAvailableDevices`, so they are fully compatible with
  `dotnet run --device`.
- The `maui` CLI already wraps these native tools for other commands
  (environment setup, emulator management), so device listing is a
  natural extension.

When a project **is** available and the user wants framework-specific
device filtering, `dotnet run --list-devices` remains the right tool —
it provides richer metadata (RuntimeIdentifier) and benefits from
workload-specific logic. The two approaches are complementary:

```
maui device list          →  "What devices exist on this machine?"
dotnet run --list-devices →  "What devices can run this project?"
```

**Platform Implementation:**

| Platform | Native tool | What is enumerated |
|----------|------------|-------------------|
| Android | `adb devices -l` | Physical devices and running emulators |
| iOS (simulators) | `xcrun simctl list devices --json` | All simulators (booted + shutdown) |
| iOS (physical) | `xcrun devicectl list devices` | Connected physical devices |
| Mac Catalyst | (host machine) | The Mac itself |

## App Inspection Commands (Future)

> **Note**: App inspection commands are planned for a future release. The initial release focuses on environment setup and device management.

### Device Selection Options

App inspection commands will follow the conventions established by [`dotnet
run` for .NET MAUI][dotnet-run-spec], using the same device selection
and framework options:

```
-f|--framework <FRAMEWORK>     Target framework (e.g., net10.0-android, net10.0-ios)
-d|--device <DEVICE_ID>        Target device identifier (from --list-devices)
--list-devices                 List available devices/emulators/simulators
-p|--project <PATH>            Path to the .NET MAUI project (default: current directory)
-h|--help                      Show help information
--version                      Show version information
```

**Interactive Prompting Behavior:**

When `-f|--framework` is not specified:

- If a project file exists and has multiple target frameworks, the CLI
  prompts to select one

- If no project file is present, the CLI prompts from known .NET MAUI
  target frameworks (e.g., `net10.0-android`, `net10.0-ios`,
  `net10.0-maccatalyst`, `net10.0-windows`)

When `-d|--device` is not specified and a framework is selected:

- The CLI prompts to select from available
  devices/emulators/simulators for that platform

- Device list comes from the same `ComputeAvailableDevices` MSBuild
  target used by `dotnet run`

**Note**: The `-d|--device` option uses the same device identifiers
returned by `dotnet run --list-devices`.

[dotnet-run-spec]: https://github.com/dotnet/sdk/blob/a40a96aa565ee248121f8fdb0f729099d9e78efe/documentation/specs/dotnet-run-for-maui.md

### Commands

#### `screenshot`

Captures a screenshot of the currently running .NET MAUI application.

**Usage:**

```bash
maui screenshot [options]
```

**Options:**

- `-o|--output <PATH>`: Output file path (default: `screenshot_{timestamp}.png`)
- `-w|--wait <SECONDS>`: Wait before capturing (default: 0)

**Platform Implementation:**

Initial implementation targets Android and iOS/Mac Catalyst, with Windows and macOS support planned as described below.

- **Android**: Uses `adb exec-out screencap -p`
- **iOS/Mac Catalyst**: Uses `xcrun simctl io booted screenshot <file>` for simulator; physical device capture via Xcode tooling (future)
- **Windows** (planned): Uses Windows screen capture APIs to capture the active app window or full screen.
- **macOS** (planned): Uses macOS screen capture APIs or command-line tooling to capture the active app window or full screen.

### Future Commands

- `maui screenshot` for capturing screenshots of running apps
- `maui logs` for streaming device logs
- `maui tree` for inspecting the visual tree

## Integration with `dotnet run` and `dotnet watch`

The CLI is designed to work seamlessly with existing .NET workflows:

### Example Workflow

```bash
# Terminal 1: Run application with hot reload
dotnet watch run

# Terminal 2: Inspect application
maui screenshot --output iteration1.png
maui logs --follow --filter "MyApp"    # future
maui tree --json                       # future
```

### AI Agent Workflow

```bash
# 0. Discover available devices (no project needed)
maui device list --json

# 1. Make code changes
# ... agent modifies MainPage.xaml ...

# 2. Wait for hot reload to complete
sleep 2

# 3. Capture screenshot
maui screenshot -o current.png

# 4. Analyze visual tree (future)
maui tree --json

# 5. Check logs for errors (future)
maui logs --level error

# 6. Agent analyzes outputs and decides next steps
```

## Platform-Specific Considerations

### Android

- **Device Detection**: `adb devices`
- **Screenshots**: `adb exec-out screencap -p` or UI Automator
- **Logs**: `adb logcat` with package filtering

### iOS / Mac Catalyst

- **Device Detection**: `xcrun simctl list devices` (simulators),
  `xcrun devicectl list devices` (physical devices) — via [AppleDev.Tools][appledev-tools]

- **Screenshots**: `xcrun simctl io booted screenshot <file>`
  (simulators), iOS physical devices (future)

- **Logs**: `xcrun simctl spawn booted log stream` or Console.app
  (simulators), `mlaunch --logdev` (physical devices)

## Security and Privacy

The CLI is designed for development and debugging scenarios only:

1. **Debug builds only**: Features should be disabled in `Release`
   builds using trimmer feature flags or `#if DEBUG` conditionals

2. **Reuse existing infrastructure**: Leverage existing transport
   mechanisms (debugger, Hot Reload) rather than creating new
   communication channels

3. **No production exposure**: Except when using standard OS features
   (like screenshots and logs), the CLI should not be usable against
   production applications

## IDE Integration

The `maui` CLI and its underlying libraries are designed to be the shared backend for IDE extensions, eliminating duplicate environment detection and setup logic across tools.

### Architecture

```
┌──────────────────┐    ┌──────────────────┐    ┌──────────────────┐
│   VS Code ext    │    │  Visual Studio    │    │    AI Agent      │
│   (vscode-maui)  │    │   extension       │    │  (Copilot, etc.) │
└────────┬─────────┘    └────────┬──────────┘    └────────┬─────────┘
         │                       │                        │
    spawns CLI            references NuGet           spawns CLI
         │                  library directly              │
         │                       │                        │
         ▼                       ▼                        ▼
  ┌──────────────┐    ┌────────────────────┐    ┌──────────────┐
  │  maui CLI    │    │  android-tools     │    │  maui CLI    │
  │  (process)   │    │  (in-process)      │    │  (--json)    │
  └──────┬───────┘    └────────┬───────────┘    └──────┬───────┘
         │                     │                       │
         └─────────┬───────────┴───────────────────────┘
                   │ spawns native tools
       ┌───────────┼───────────┐
       ▼           ▼           ▼
 ┌───────────┐ ┌──────────┐ ┌──────────┐
 │ adb       │ │  xcrun   │ │ Windows  │
 │ sdkmanager│ │  simctl  │ │   SDK    │
 └───────────┘ └──────────┘ └──────────┘
```

### Integration Modes

| Consumer | Integration | Rationale |
|----------|------------|-----------|
| **Visual Studio** extension | References `android-tools` NuGet package directly (in-process) | .NET extension — no serialization overhead, direct API access |
| **VS Code** (`vscode-maui`) | Spawns `maui` CLI process, parses `--json` stdout | TypeScript extension — CLI is the natural process boundary |
| **AI agents / CI** | Invokes `maui` CLI with `--json` | Process-based, language-agnostic |
| **Terminal** (human) | Invokes `maui` CLI directly | Human-readable output by default, `--json` when needed |

Visual Studio consumes the `Xamarin.Android.Tools.AndroidSdk` NuGet package from [`dotnet/android-tools`](https://github.com/dotnet/android-tools) directly — the same library the CLI uses internally. This avoids process overhead and gives the VS extension full API access. Non-.NET consumers (VS Code, AI agents, CI) use the CLI as the canonical interface.

### How IDEs Use It

| Workflow | CLI command | IDE behavior |
|----------|------------|--------------|
| Workspace open | `maui apple check --json`, `maui android jdk check --json` | Show environment status in status bar / problems panel |
| Environment fix | `maui android install --json` | Display progress bar, stream `type: "progress"` messages |
| Device picker | `maui device list --json` | Populate device dropdown / selection UI |
| Emulator launch | `maui android emulator start <name> --json` | Show notification, update device list on completion |

### Benefits

- **Consistent behavior** — VS, VS Code, and CLI all use the same detection and setup logic (via shared libraries)
- **Single maintenance point** — bug fixes in `android-tools` propagate to all consumers
- **AI-ready** — agents use the same `--json` output that VS Code consumes
- **Flexible integration** — .NET consumers go in-process, others use the CLI

### Current Status

| Integration | Status |
|-------------|--------|
| VS Code extension (`vscode-maui`) | ✅ In progress |
| Visual Studio extension | Planned (vNext) |
| GitHub Copilot / AI agents | ✅ Supported via `--json` output |

## Future Goals

### MCP Server

An MCP (Model Context Protocol) server was considered. However, AI
agents can effectively work with CLI commands through examples in
`copilot-instructions.md` without requiring a custom MCP server.

If an MCP server is deemed useful after the CLI is completed, it could
be a thin wrapper that exposes the CLI operations through the MCP
protocol. Both Visual Studio and VS Code extensions provide options
for distributing MCP servers, so we would likely do this through .NET
MAUI tooling.

**Decision**: Build the CLI first. The MCP server can be added later
if there's demonstrated need.

### More Subcommands

Environment setup commands (Android SDK/JDK, Xcode, emulators,
simulators) are now included above. These were inspired by:

- .NET MAUI "Check" / "Doctor"
  - https://github.com/Redth/dotnet-maui-check
  - https://github.com/jfversluis/maui-cli
- Android SDK Management
  - https://github.com/Redth/AndroidSdk.Tools

Future commands:

- `maui logs` for viewing console output
- `maui tree` for displaying the visual tree
- `maui screenshot` for capturing screenshots

**Decision**: Environment setup and device listing ship first. App inspection commands
follow in a future release.

## References

- [vibe-wpf experiment][vibe-wpf]
- [dotnet run for .NET MAUI specification][dotnet-run-spec]
- [Workload manifest specification][workload-spec]
- [AppleDev.Tools][appledev-tools] - Wraps simctl and devicectl commands
- [ComputeAvailableDevices (Android)][compute-android] - Android workload MSBuild target
- [ComputeAvailableDevices (Apple)][compute-apple] - Apple workload MSBuild target
- [System.CommandLine documentation](https://learn.microsoft.com/dotnet/standard/commandline/)
- [Android Debug Bridge (ADB)](https://developer.android.com/studio/command-line/adb)
- [simctl command-line tool](https://nshipster.com/simctl/)

[vibe-wpf]: https://github.com/jonathanpeppers/vibe-wpf
[appledev-tools]: https://github.com/Redth/AppleDev.Tools
[compute-android]: https://github.com/dotnet/android/blob/main/Documentation/docs-mobile/building-apps/build-targets.md#computeavailabledevices
[compute-apple]: https://github.com/dotnet/macios/blob/main/docs/building-apps/build-targets.md#computeavailabledevices

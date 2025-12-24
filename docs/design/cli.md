---
description: "Design document for the dotnet-maui CLI tool for AI-assisted development"
date: 2025-11-19
---

# dotnet-maui CLI Design Document

## Overview

The `dotnet-maui` CLI is a command-line tool that provides simple
commands for capturing screenshots, viewing logs, and inspecting the
visual tree of running .NET MAUI applications. While designed to
enable AI agents to iteratively develop and validate applications,
these commands are equally useful for developers who want quick access
to debugging and inspection capabilities from the terminal.

## Motivation

The "vibe coding" experiment with WPF ([vibe-wpf]) showed that AI
agents can effectively develop applications when given the right
tools. .NET MAUI, however, spans multiple platforms with different
command-line interfaces: capturing a screenshot on iOS requires `xcrun
simctl io booted screenshot`, while Android uses `adb exec-out
screencap`. Similarly, log access, visual tree inspection, and device
management all have platform-specific implementations.

The `dotnet-maui` CLI provides a unified interface across Android,
iOS, macOS, Windows, and Mac Catalyst, making these operations simple
and consistent for both developers and AI agents.

## Goals

1. **Screenshot capture**: Enable AI agents to capture screenshots of
   running .NET MAUI applications to validate visual changes

2. **Log access**: Provide unified access to platform-specific device
   logs (logcat, Console, etc.)

3. **Visual tree inspection**: Allow agents to inspect the runtime
   visual tree structure and properties (.NET MAUI visual tree)

4. **Developer experience**: Integrate seamlessly with existing
   `dotnet` CLI workflows, this should fit in with `dotnet run`,
   `dotnet watch`, etc.

## Installation and Invocation

The CLI will be available through multiple invocation methods to
support different workflows:

### Method 1: Direct Tool Invocation

```bash
dotnet-maui screenshot -o screenshot.png
```

### Method 2: .NET CLI

```bash
dotnet maui screenshot -o screenshot.png
```

### Method 3: `dotnet tool exec` or `dnx`

```bash
dotnet tool exec -y Microsoft.Maui.Cli screenshot -o screenshot.png
dnx -y Microsoft.Maui.Cli screenshot -o screenshot.png
```

### Installation

```bash
# As a global tool
dotnet tool install --global Microsoft.Maui.Cli

# As a local tool (recommended for projects)
dotnet tool install Microsoft.Maui.Cli

# Restore local tools
dotnet tool restore

# Inline install and invocation
dotnet tool exec -y Microsoft.Maui.Cli screenshot -o screenshot.png
dnx -y Microsoft.Maui.Cli screenshot -o screenshot.png
```

The .NET workload specification includes support for automatically
installing tools from workloads via the `tools-packs` feature (see
[workload manifest specification][workload-spec]). However, this
feature is not yet implemented. Once available, `Microsoft.Maui.Cli`
could be automatically installed when the `maui` workload is
installed, eliminating the need for manual tool installation.

Until then, manual installation via `dotnet tool install` will be how
we prove out the `dotnet-maui` CLI.

[workload-spec]: https://github.com/dotnet/designs/blob/566ad4cafcc578d6389c215c61924ee9e07dcb29/accepted/2020/workloads/workload-manifest.md#tools-packs

## Command Structure

### Global Options

The `dotnet-maui` CLI follows the conventions established by [`dotnet
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
dotnet maui screenshot [options]
```

**Options:**

- `-o|--output <PATH>`: Output file path (default: `screenshot_{timestamp}.png`)
- `-w|--wait <SECONDS>`: Wait before capturing (default: 0)

**Platform Implementation:**

- **Android**: Uses `adb screencap`
- **iOS/Mac Catalyst**: Uses `simctl io screenshot` for simulator, iOS devices (future implementation)

#### `logs`

Streams or retrieves device logs from the running application.

**Usage:**

```bash
dotnet maui logs [options]
```

**Options:**

- `-f|--follow`: Stream logs continuously (like `tail -f`)
- `--level <LEVEL>`: Minimum log level (trace|debug|info|warn|error)
- `--clear`: Clear logs before starting

**Example:**

```bash
dotnet maui logs --follow
dotnet maui logs --filter "Exception"
dotnet maui logs --clear
```

**Platform Implementation:**

- **Android**: `adb logcat` with filtering
- **iOS/Mac Catalyst**: similar to existing `dotnet run` behavior

#### `tree`

Displays the visual tree structure of the running application.

**Usage:**

```bash
dotnet maui tree [options]
```

**Options:**

- `--format <FORMAT>`: Output format (text|json|xml, default: text)
- `--depth <NUMBER>`: Maximum tree depth (default: unlimited)
- `--element <AUTOMATION_ID>`: Start from specific element

**Example:**

```bash
dotnet maui tree
dotnet maui tree --format json
dotnet maui tree --element "MainCard"
```

**Output Format (text):**

```md
ContentPage (AutomationId: MainPage)
├─ VerticalStackLayout
│  ├─ Label (AutomationId: TitleLabel)
│  │  └─ Text: "Welcome to MAUI"
│  ├─ Entry (AutomationId: UsernameEntry)
│  │  └─ Placeholder: "Username"
│  └─ Button (AutomationId: LoginButton)
│     └─ Text: "Login"
```

**Output Format (json):**

```json
{
  "type": "ContentPage",
  "automationId": "MainPage",
  "properties": {
    "Title": "Login"
  },
  "children": [
    {
      "type": "VerticalStackLayout",
      "children": [
        {
          "type": "Label",
          "automationId": "TitleLabel",
          "properties": {
            "Text": "Welcome to MAUI",
            "FontSize": 24
          }
        }
      ]
    }
  ]
}
```

**Implementation:**

Since we want to see .NET MAUI's visual tree, and not the native one,
this can be implemented similarly to XAML Live Preview in Visual
Studio.

We should investigate existing connections like the debugger or Hot
Reload to implement this behavior and return this information from the
running application.

## Integration with `dotnet run` and `dotnet watch`

The CLI is designed to work seamlessly with existing .NET workflows:

### Example Workflow

```bash
# Terminal 1: Run application with hot reload
dotnet watch run

# Terminal 2: Monitor logs
dotnet maui logs --follow --filter "MyApp"

# Terminal 3: Inspect application
dotnet maui screenshot --output iteration1.png
dotnet maui tree --format json
```

### AI Agent Workflow

```bash
# 1. Make code changes
# ... agent modifies MainPage.xaml ...

# 2. Wait for hot reload to complete
sleep 2

# 3. Capture screenshot
dotnet maui screenshot -o current.png

# 4. Analyze visual tree
dotnet maui tree --format json

# 5. Check logs for errors
dotnet maui logs --level error

# 6. Agent analyzes outputs and decides next steps
```

## Platform-Specific Considerations

### Android

- **Device Detection**: `adb devices`
- **Screenshots**: `adb exec-out screencap -p` or UI Automator
- **Logs**: `adb logcat` with package filtering

### iOS / Mac Catalyst

- **Device Detection**: `xcrun simctl list devices` (simulators),
  `idevice_id -l` from [libimobiledevice] (physical devices)

- **Screenshots**: `xcrun simctl io booted screenshot <file>`
  (simulators), iOS devices (future implementation)

- **Logs**: `xcrun simctl spawn booted log stream` or Console.app
  (simulators), `idevicesyslog` (physical devices)

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

There are other .NET MAUI CLI tools such as:

- .NET MAUI "Check" / "Doctor"
  - https://github.com/Redth/dotnet-maui-check
  - https://github.com/jfversluis/maui-cli
- Android SDK Management
  - https://github.com/Redth/AndroidSdk.Tools

These could easily be added down the road.

**Decision**: Start with just a few subcommands and expand in the
future.

## References

- [vibe-wpf experiment][vibe-wpf]
- [dotnet run for .NET MAUI specification][dotnet-run-spec]
- [Workload manifest specification][workload-spec]
- [libimobiledevice for macOS][libimobiledevice]
- [System.CommandLine documentation](https://learn.microsoft.com/dotnet/standard/commandline/)
- [Android Debug Bridge (ADB)](https://developer.android.com/studio/command-line/adb)
- [simctl command-line tool](https://nshipster.com/simctl/)

[vibe-wpf]: https://github.com/jonathanpeppers/vibe-wpf
[libimobiledevice]: https://github.com/benvium/libimobiledevice-macosx

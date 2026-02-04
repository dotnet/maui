# MAUI Dev Tools — IDE Integration

**Parent Document**: [MAUI Dev Tools Specification](./maui-devtools-spec.md)

This document details how IDEs (Visual Studio, VS Code) integrate with MAUI Dev Tools.

---

## Table of Contents

1. [Extension Architecture](#1-extension-architecture)
2. [VS Code Integration](#2-vs-code-integration)
3. [Visual Studio Integration](#3-visual-studio-integration)
4. [Common UI Patterns](#4-common-ui-patterns)

---

## 1. Extension Architecture

### VS Code Extension

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
│                        │ CLI invocation (stdio)              │
│                        ▼                                     │
│  ┌──────────────────────────────────────────────────────┐   │
│  │           MAUI Dev Tools Client                       │   │
│  │           (spawned as child process)                  │   │
│  └──────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────┘
```

**Extension Responsibilities**:
1. Spawn `dotnet maui` process on activation
2. Send `dotnet maui doctor --json` request on workspace open
3. Display issues in Problems panel
4. Register "MAUI: Setup Environment" command
5. Show progress notifications during fixes

### Visual Studio Extension

- Uses CLI invocation for communication
- Integrates with Visual Studio's environment detection UI
- Surfaces issues in Error List window
- Provides menu items in Tools > MAUI submenu

---

## 2. VS Code Integration

### Status Bar Item

Always visible indicator of MAUI environment health:

```
$(check-circle) MAUI Ready          ← Green when healthy
$(warning) MAUI: 2 issues          ← Yellow with issue count
$(error) MAUI: Setup Required      ← Red when critical
```

**Click Action**: Opens "MAUI Environment" panel

### Environment Panel

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

### Fix Progress Dialog

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

### Fix Failed Dialog

When a fix operation fails:

```
┌────────────────────────────────────────────────────────────┐
│  ✖ Fix Failed                                              │
├────────────────────────────────────────────────────────────┤
│                                                            │
│  Unable to install Android SDK automatically.              │
│                                                            │
│  Error: Connection reset while downloading from            │
│  dl.google.com (E4001)                                     │
│                                                            │
│  Attempted:                                                │
│    • Retry download (3 times)                              │
│    • Clear download cache                                  │
│                                                            │
│  This may be caused by:                                    │
│    • Corporate proxy/firewall blocking Google domains      │
│    • Network connectivity issues                           │
│    • SSL inspection interfering with downloads             │
│                                                            │
│  ─────────────────────────────────────────────────────────│
│  [Ask Copilot for Help]  [View Logs]  [Retry]  [Cancel]   │
└────────────────────────────────────────────────────────────┘
```

**"Ask Copilot for Help" Flow**:

1. User clicks button or types `/maui-help` in Copilot Chat
2. Extension sends diagnostic context to Copilot
3. Copilot receives structured data + conversation prompt:

```
The MAUI Dev Tools detected an issue it couldn't fix automatically.

**Issue**: Android SDK installation failed (E4001 - Connection reset)
**Environment**: macOS 15.0, corporate network with proxy
**Attempted**: 3 download retries, cache clear

The diagnostic bundle is attached. Please help the user resolve this issue.
Common causes for this error include proxy configuration, firewall rules,
or SSL inspection. Ask clarifying questions if needed.
```

4. Copilot engages in conversational troubleshooting
5. Copilot can suggest manual steps or request tool actions with user approval

---

## 3. Visual Studio Integration

### Tools Menu

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

### Error List Integration

Issues detected by `dotnet maui doctor` appear in Visual Studio's Error List window:

| Severity | Code | Description | Project |
|----------|------|-------------|---------|
| ⚠️ Warning | MAUI001 | Android SDK build-tools outdated | Solution |
| ❌ Error | MAUI002 | iOS runtime 18.0 not installed | Solution |

Double-clicking an item opens the fix dialog.

### Output Window

Detailed logs available in Output window under "MAUI Dev Tools" pane:

```
[15:32:01] Running environment check...
[15:32:02] ✓ .NET SDK 9.0.100 found
[15:32:02] ✓ MAUI workload installed
[15:32:03] ✗ Android SDK not found at expected locations
[15:32:03] Checked: ANDROID_HOME, ANDROID_SDK_ROOT, ~/Library/Android/sdk
[15:32:03] Environment check complete: 1 error, 0 warnings
```

---

## 4. Common UI Patterns

### Interactive Prompting

When running in interactive mode (terminal), the tool prompts for missing information:

**Example: AVD Creation with Missing Parameters**:
```
$ dotnet maui android avd create

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
$ dotnet maui android avd create --non-interactive
Error: --name is required in non-interactive mode
```

**Example: Large Download Confirmation**:
```
$ dotnet maui doctor --fix

The following will be installed:
  • system-images;android-34;google_apis;x86_64 (2.6 GB)
  • iOS 18.0 Runtime (8.1 GB)

Total download size: 10.7 GB

? Proceed? [Y/n]
```

### Permission Prompt (AI Agent)

When an AI agent requests a modification:

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

### Progress Notification

For long-running operations, IDEs show progress:

**VS Code**: Notification toast with progress bar
**Visual Studio**: Status bar progress indicator + Output window details

Progress updates include:
- Current operation name
- Percentage complete (when determinable)
- Bytes downloaded / total (for downloads)
- ETA (estimated time remaining)
- Cancel button

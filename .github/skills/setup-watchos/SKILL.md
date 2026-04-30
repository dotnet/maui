---
name: setup-watchos
description: Sets up a watchOS companion app for a .NET MAUI iOS application. Configures a SwiftUI watchOS app with xcodegen, MSBuild auto-build integration, WatchConnectivity bidirectional sync, and deep link launch from watch.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires .NET MAUI with iOS target, Xcode 16+ with watchOS SDK 10+, WatchConnectivity.
---

# Setup watchOS Companion App Skill

Adds a watchOS companion app to a .NET MAUI iOS app with bidirectional communication via WatchConnectivity. The watch app auto-builds during `dotnet build` via MSBuild integration.

## Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                    .NET MAUI App (C#)                             │
│                                                                  │
│  Page ──► IWatchConnectivityService ──► WCSession                │
│    ▲            │                           │                    │
│    │            ▼                           │                    │
│    │   updateApplicationContext ─────────────┼──► watch receives  │
│    │   sendMessage (if reachable) ──────────┤      context/msg   │
│    │                                        │                    │
│    │   WCSessionDelegate ◄──────────────────┤                    │
│    └── DidReceiveApplicationContext ◄────────┘                   │
└──────────────────────────────────────────────────────────────────┘
                    │  WatchConnectivity (WCSession)  │
┌──────────────────────────────────────────────────────────────────┐
│                watchOS Companion App (SwiftUI)                    │
│                                                                  │
│  PhoneConnectivityProvider ──► WCSession                         │
│       ▲                            │                             │
│       │                            ▼                             │
│  ContentView ◄──── @Published ◄── didReceiveApplicationContext   │
│       │                                                          │
│       ▼                                                          │
│  Button tap ──► updateApplicationContext ──► iOS app receives     │
└──────────────────────────────────────────────────────────────────┘
```

### Communication Channels

| Direction | Method | When to Use |
|-----------|--------|-------------|
| **App → Watch** | `updateApplicationContext` | Latest state sync (most common, works in background) |
| **App → Watch** | `sendMessage` | Real-time when watch is reachable |
| **Watch → App** | `updateApplicationContext` | Latest state sync back to phone |
| **Watch → App** | `sendMessage` | Real-time when phone is reachable |
| **App → Watch** | `transferUserInfo` | Queued delivery (guaranteed, FIFO) |

### Key WatchConnectivity Concepts

- **Application Context**: Dictionary (`[String: Any]`) — only latest value kept. Best for state sync.
- **Messages**: Real-time, requires both apps active. `sendMessage` with reply handler.
- **User Info Transfer**: Queued, guaranteed delivery. Good for one-off events.
- **Complication User Info**: Priority transfer for complications (limited budget).

## When to Use This Skill

- User wants to add a watchOS companion app to their MAUI iOS app
- User asks about Apple Watch integration or WatchConnectivity
- User wants to sync data between MAUI app and a watchOS app
- User wants real-time communication between phone and watch

## Prerequisites

- macOS with **Xcode 16+**
- .NET 10+ SDK with MAUI workload
- watchOS 10+ simulator (paired with iPhone simulator)
- [xcodegen](https://github.com/yonaskolb/XcodeGen): `brew install xcodegen`

## Identifier Convention

Derive all identifiers from the app's bundle ID. For `com.example.myapp`:

| Identifier | Value | Where Used |
|-----------|-------|------------|
| App Bundle ID | `com.example.myapp` | `.csproj`, Info.plist |
| Watch App Bundle ID | `com.example.myapp.watchapp` | Xcode project (**must** be child of app ID) |

These must match **exactly** — the watch app bundle ID must be a child of the iOS app bundle ID.

## Step-by-Step Setup

### Step 1: Create the C# Service Layer

Create the WatchConnectivity service that wraps `WCSession` for the iOS side. See `references/csharp-templates.md` for complete code.

| File | Purpose |
|------|---------|
| `Services/WatchData.cs` | Shared dictionary data model |
| `Services/IWatchConnectivityService.cs` | Platform-agnostic interface |
| `Services/StubWatchConnectivityService.cs` | No-op for non-iOS platforms |
| `Platforms/iOS/WatchConnectivityService.cs` | iOS impl wrapping WCSession |

**Key design decisions:**
- WCSession uses `NSDictionary` (`[String: Any]`) — not JSON files like widgets
- The service fires a C# event when data arrives from the watch
- `updateApplicationContext` is the primary sync mechanism (latest-wins, works in background)
- `sendMessage` is used opportunistically when watch is reachable for instant sync

### Step 2: Wire Up App Integration

See `references/csharp-templates.md` — "App Integration" section.

1. **`MauiProgram.cs`** — Register `IWatchConnectivityService` as singleton (WCSession must be singleton)
2. **`App.xaml.cs`** — Activate the service on startup
3. **`MainPage.xaml.cs`** — Subscribe to `DataReceived` event, send data on user interaction

**⚠️ CRITICAL:** `WCSession` must be activated early in app lifecycle. Call `service.Activate()` in `CreateWindow`. The session must be a **singleton** — never create multiple instances.

### Step 3: Update the .csproj

Add iOS-specific configuration for watch app embedding and auto-build:

```xml
<!-- watchOS companion app embedding -->
<PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
    <CodesignEntitlements>Platforms/iOS/Entitlements.plist</CodesignEntitlements>
</PropertyGroup>

<!-- Auto-build watch app during dotnet build. Skip with: -p:SkipWatchBuild=true -->
<Target Name="BuildWatchApp"
        AfterTargets="ResolveReferences"
        Condition="$(TargetFramework.Contains('-ios')) AND '$(SkipWatchBuild)' != 'true'">
    <PropertyGroup>
        <_WatchIsSimulator>$(RuntimeIdentifier.Contains('simulator'))</_WatchIsSimulator>
        <_WatchSdk Condition="'$(_WatchIsSimulator)' == 'true'">watchsimulator</_WatchSdk>
        <_WatchSdk Condition="'$(_WatchIsSimulator)' != 'true'">watchos</_WatchSdk>
        <_WatchArch>arm64</_WatchArch>
        <_WatchOutputSubdir>Release-$(_WatchSdk)</_WatchOutputSubdir>
        <_XcodeProjectDir>$(MSBuildProjectDirectory)/WatchApp</_XcodeProjectDir>
        <_XcodeProjectPath>$(_XcodeProjectDir)/WatchApp.xcodeproj</_XcodeProjectPath>
        <_WatchBuildDir>$(_XcodeProjectDir)/build</_WatchBuildDir>
        <_WatchAppSource>$(_WatchBuildDir)/$(_WatchOutputSubdir)/MyWatchApp.app</_WatchAppSource>
        <_WatchDestDir>$(MSBuildProjectDirectory)/Platforms/iOS/WatchApps/$(_WatchOutputSubdir)</_WatchDestDir>
    </PropertyGroup>

    <Exec Command="xcodegen generate"
          WorkingDirectory="$(_XcodeProjectDir)"
          Condition="!Exists('$(_XcodeProjectPath)') AND Exists('$(_XcodeProjectDir)/project.yml')" />

    <Message Text="Building watch app for $(_WatchSdk)..." Importance="high" />
    <Exec Command="xcodebuild -quiet -project WatchApp.xcodeproj -target MyWatchApp -configuration Release -sdk $(_WatchSdk) -arch $(_WatchArch) CODE_SIGN_IDENTITY=&quot;-&quot; CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO BUILD_DIR=build build"
          WorkingDirectory="$(_XcodeProjectDir)" />

    <MakeDir Directories="$(_WatchDestDir)" />
    <Exec Command="rm -rf &quot;$(_WatchDestDir)/MyWatchApp.app&quot; &amp;&amp; cp -R &quot;$(_WatchAppSource)&quot; &quot;$(_WatchDestDir)/&quot;" />
</Target>
```

**What this does:** A single `dotnet build -f net10.0-ios` automatically:
1. Generates `.xcodeproj` from `project.yml` if needed
2. Builds the watch app with `xcodebuild`
3. Copies the `.app` to staging for embedding

### Step 4: Create the Swift watchOS App

Create `WatchApp/` at the project root with these files. See `references/swift-templates.md` for complete code.

**Directory structure:**
```
WatchApp/
├── project.yml                          # xcodegen spec
├── WatchApp/                            # Thin host iOS app (required by Xcode, not shipped)
│   ├── WatchAppApp.swift
│   └── ContentView.swift
├── MyWatchApp/                          # The actual watchOS app
│   ├── Info.plist
│   ├── MyWatchAppApp.swift              # @main SwiftUI entry point
│   ├── ContentView.swift                # Watch UI
│   └── PhoneConnectivityProvider.swift  # WCSession wrapper (ObservableObject)
└── build-release.sh                     # Manual build script (fallback)
```

**Key Swift files:**

- **`MyWatchAppApp.swift`** — `@main` entry, injects `PhoneConnectivityProvider` as environment object
- **`ContentView.swift`** — SwiftUI watch UI displaying synced counter, with ± buttons
- **`PhoneConnectivityProvider.swift`** — `WCSessionDelegate` wrapper, publishes received data as `@Published` properties

### Step 5: Create xcodegen project.yml

```yaml
name: WatchApp
options:
  bundleIdPrefix: com.example
  deploymentTarget:
    iOS: "17.0"
    watchOS: "10.0"

targets:
  WatchApp:
    type: application
    platform: iOS
    sources:
      - WatchApp
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: com.example.myapp

  MyWatchApp:
    type: application
    platform: watchOS
    sources:
      - MyWatchApp
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: com.example.myapp.watchapp
        INFOPLIST_FILE: MyWatchApp/Info.plist
        GENERATE_INFOPLIST_FILE: "NO"
```

Then: `cd WatchApp && xcodegen generate`

### Step 6: Build and Test

**Build (one command):**
```bash
dotnet build MyApp.csproj -f net10.0-ios -r iossimulator-arm64 \
  -p:CodesignRequireProvisioningProfile=false
```

**Post-build: embed watch app in iOS app bundle:**
```bash
APP_PATH="path/to/MyApp.app"
WATCH_APP="WatchApp/build/Release-watchsimulator/MyWatchApp.app"

# Create Watch directory and copy
mkdir -p "$APP_PATH/Watch"
cp -R "$WATCH_APP" "$APP_PATH/Watch/"

# Re-sign the iOS app
/usr/bin/codesign -v --force --timestamp=none --sign - "$APP_PATH"
```

**Pair simulators and test:**
```bash
# List available watch simulators
xcrun simctl list devices watchOS available

# Boot and pair (Xcode usually auto-pairs, but if needed):
PHONE_UDID="<iphone-simulator-udid>"
WATCH_UDID="<watch-simulator-udid>"

# Install on phone (watch app auto-installs on paired watch)
xcrun simctl install "$PHONE_UDID" "$APP_PATH"
xcrun simctl launch "$PHONE_UDID" com.example.myapp
```

**Skip watch build** (C#-only changes): add `-p:SkipWatchBuild=true`

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| **WCSession not a singleton** | Register `IWatchConnectivityService` as `AddSingleton`, never `AddTransient` |
| **Watch app not appearing on watch** | Bundle ID must be child of iOS app ID (e.g., `com.myapp.watchapp`) |
| **Session not activated** | Call `service.Activate()` early in `CreateWindow`, before any send calls |
| **sendMessage fails** | Watch may not be reachable. Always fall back to `updateApplicationContext` |
| **Data not syncing in background** | Use `updateApplicationContext` (works in background), not `sendMessage` (requires foreground) |
| **Watch simulator not paired** | Open Xcode → Window → Devices and Simulators → pair watch with phone |
| **xcodebuild fails for watchOS** | Use `type: application` with `platform: watchOS` (NOT `application.watchapp2` — causes `CopyAndPreserveArchs` conflict in Xcode 26+) |
| **DI null in CreateWindow** | Use `activationState?.Context?.Services ?? IPlatformApplication.Current?.Services` |
| **Multiple WCSession instances** | WCSession.default is a singleton — wrapping service must also be singleton |
| **Watch app won't install on simulator** | Embed `.app` in iOS app's `Watch/` directory, then install iOS app |

## Debugging

```bash
PHONE_UDID="<iphone-udid>"
WATCH_UDID="<watch-udid>"

# Check watch app is embedded
ls path/to/YourApp.app/Watch/

# Stream iOS app WCSession logs
xcrun simctl spawn "$PHONE_UDID" log stream \
  --predicate 'subsystem == "com.apple.WatchConnectivity" OR processImagePath contains "YourApp"' \
  --timeout 30

# Stream watch app logs
xcrun simctl spawn "$WATCH_UDID" log stream \
  --predicate 'processImagePath contains "MyWatchApp"' --timeout 30

# Check if devices are paired
xcrun simctl list pairs
```

## Reference Files

Complete code templates are in `references/`:
- **`references/csharp-templates.md`** — WatchData, IWatchConnectivityService, WatchConnectivityService (iOS), App.xaml.cs, MauiProgram.cs, MainPage
- **`references/swift-templates.md`** — PhoneConnectivityProvider, ContentView, MyWatchAppApp, Info.plist
- **`references/project-config.md`** — .csproj MSBuild targets, xcodegen project.yml, build script, directory structure

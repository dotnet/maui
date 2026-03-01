---
name: setup-widget
description: Sets up an iOS Home Screen Widget for a .NET MAUI application. Configures App Groups, entitlements, a SwiftUI WidgetKit extension with xcodegen, MSBuild auto-build integration, JSON file data sharing, deep links, and optional interactive AppIntents.
metadata:
  author: dotnet-maui
  version: "3.0"
compatibility: Requires .NET MAUI with iOS target, Xcode 16+ with iOS SDK 17+, WidgetKit.
---

# Setup iOS Home Screen Widget Skill

Adds an iOS Widget Extension to a .NET MAUI app with robust bidirectional communication. The widget auto-builds during `dotnet build` via MSBuild integration — no separate build step needed.

## Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│                    .NET MAUI App (C#)                             │
│                                                                  │
│  Page ──► IWidgetDataService ──► JSON file (App Group container) │
│    ▲            │                        │                       │
│    │            ▼                        │                       │
│    │   WidgetKit.ReloadTimelines ────────┼──► iOS refreshes      │
│    │                                     │      widget           │
│    │   AppDelegate.OpenUrl ◄─────────────┤                       │
│    └── HandleWidgetUrl (deep link) ◄─────┘                       │
└──────────────────────────────────────────────────────────────────┘
                    │  JSON files in App Group container  │
┌──────────────────────────────────────────────────────────────────┐
│                iOS Widget Extension (Swift/SwiftUI)               │
│                                                                  │
│  SharedStorage ──► File I/O (App Group) ──► Provider             │
│       ▲                                          │               │
│       │                                          ▼               │
│  AppIntents (buttons) ◄──── WidgetView ◄──── TimelineEntry       │
│       │                          │                               │
│       ▼                          ▼                               │
│  WidgetCenter.reloadTimelines   widgetURL (deep link to app)     │
└──────────────────────────────────────────────────────────────────┘
```

### Communication Channels

| Direction | Mechanism | Description |
|-----------|-----------|-------------|
| **App → Widget** | JSON file + WidgetKit reload | App writes JSON, calls `ReloadTimelines` |
| **Widget → App (tap)** | Deep link via `widgetURL()` | Opens app with custom URL scheme |
| **Widget → App (interactive)** | AppIntents + file I/O | Widget buttons modify shared JSON |

### ⚠️ Critical: Do NOT use UserDefaults for cross-process data

`UserDefaults(suiteName:)` can resolve to **different backing plist files** for the app vs. widget extension process, especially on the iOS Simulator or with ad-hoc signing. Use **file-based JSON I/O** via `NSFileManager.GetContainerUrl()` (C#) / `FileManager.containerURL(forSecurityApplicationGroupIdentifier:)` (Swift).

## When to Use This Skill

- User wants to add a Home Screen Widget to their MAUI iOS app
- User asks about widgets, WidgetKit, or Home Screen extensions
- User wants to share data between MAUI app and an iOS extension
- User wants interactive widget buttons (AppIntents)

## Prerequisites

- macOS with **Xcode 16+**
- .NET 10+ SDK with MAUI workload
- iOS 17+ simulator or device
- [xcodegen](https://github.com/yonaskolb/XcodeGen): `brew install xcodegen`

## Identifier Convention

Derive all identifiers from the app's bundle ID. For `com.example.myapp`:

| Identifier | Value | Where Used |
|-----------|-------|------------|
| App Bundle ID | `com.example.myapp` | `.csproj`, Xcode project |
| Widget Bundle ID | `com.example.myapp.widgetextension` | Xcode project (**must** be child of app ID) |
| App Group ID | `group.com.example.myapp` | Entitlements (×2), C# constants, Swift constants |
| URL Scheme | `myapp` | Info.plist, C# constants, Swift `widgetURL` |
| Widget Kind | `MyWidget` | Swift Widget struct, C# `RefreshWidget` call |

These must match **exactly** across all files — mismatches cause silent failures.

## Step-by-Step Setup

### Step 1: Configure Entitlements

Create **two** entitlements files — one for the app, one for the widget extension.

**`Platforms/iOS/Entitlements.plist`** (MAUI app):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.application-groups</key>
    <array>
        <string>group.com.example.myapp</string>
    </array>
</dict>
</plist>
```

**`Platforms/iOS/Entitlements.WidgetExtension.plist`** (widget):
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.application-groups</key>
    <array>
        <string>group.com.example.myapp</string>
    </array>
</dict>
</plist>
```

**⚠️ CRITICAL: Entitlements files MUST use LF line endings (Unix-style), NOT CRLF.** CRLF causes cryptic build failures. After creating:
```bash
sed -i '' 's/\r$//' Platforms/iOS/Entitlements.plist Platforms/iOS/Entitlements.WidgetExtension.plist
```

Add URL scheme to **`Platforms/iOS/Info.plist`** for deep linking:
```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLName</key>
        <string>com.example.myapp</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>myapp</string>
        </array>
    </dict>
</array>
```

### Step 2: Create the C# Service Layer

See `references/csharp-templates.md` for complete code. Create these files:

| File | Purpose |
|------|---------|
| `Services/WidgetData.cs` | Shared JSON data model (mirrors Swift side) |
| `Services/WidgetConstants.cs` | All identifiers in one place |
| `Services/IWidgetDataService.cs` | Platform-agnostic interface |
| `Services/StubWidgetDataService.cs` | No-op for non-iOS platforms |
| `Platforms/iOS/WidgetDataService.cs` | iOS implementation (file I/O + WidgetKit) |

The `WidgetDataService` writes JSON to the App Group container via `MauiWidgetCenter.GetSharedContainerUrl()` and triggers refreshes via `WidgetKit.WidgetCenterProxy` NuGet or `MauiWidgetCenter.ReloadTimelines()`.

**Two options for WidgetKit API access:**

| Option | Pros | Cons |
|--------|------|------|
| `WidgetKit.WidgetCenterProxy` NuGet | Clean API, no framework to embed | External dependency |
| `MauiWidgetCenter` (built into MAUI) | Zero dependencies, uses bundled `MauiWidgetHelper.framework` | Must embed framework in app |

Both work. The NuGet is simpler for new projects. `MauiWidgetCenter` is already part of MAUI Core.

### Step 3: Wire Up App Integration

See `references/csharp-templates.md` — "App Integration" section.

1. **`Platforms/iOS/AppDelegate.cs`** — Override `OpenUrl` to intercept deep links
2. **`App.xaml.cs`** — Add `HandleWidgetUrl` + resume handler
3. **`MauiProgram.cs`** — Register `IWidgetDataService` via DI

### Step 4: Update the .csproj

Add iOS-specific configuration for widget embedding and auto-build:

```xml
<!-- iOS Widget Extension embedding -->
<PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
    <CodesignEntitlements>Platforms/iOS/Entitlements.plist</CodesignEntitlements>
</PropertyGroup>

<ItemGroup Condition="$(TargetFramework.Contains('-ios'))">
    <!-- Option A: WidgetKit NuGet (check NuGet.org for latest version) -->
    <PackageReference Include="WidgetKit.WidgetCenterProxy" Version="10.0.0" />

    <!-- Copy widget .appex to output based on platform -->
    <Content Remove="Platforms\iOS\WidgetExtensions\**" />
    <Content Condition="'$(ComputedPlatform)' == 'iPhone'"
             Include=".\Platforms\iOS\WidgetExtensions\Release-iphoneos\MyWidgetExtension.appex\**"
             CopyToOutputDirectory="PreserveNewest" />
    <Content Condition="'$(ComputedPlatform)' == 'iPhoneSimulator'"
             Include=".\Platforms\iOS\WidgetExtensions\Release-iphonesimulator\MyWidgetExtension.appex\**"
             CopyToOutputDirectory="PreserveNewest" />

    <Content Include=".\Platforms\iOS\Entitlements.WidgetExtension.plist"
             CopyToOutputDirectory="PreserveNewest" />

    <!-- Embed widget in app bundle with correct entitlements -->
    <AdditionalAppExtensions Include="$(MSBuildProjectDirectory)/Platforms/iOS/WidgetExtensions">
        <Name>MyWidgetExtension</Name>
        <BuildOutput Condition="'$(ComputedPlatform)' == 'iPhone'">Release-iphoneos</BuildOutput>
        <BuildOutput Condition="'$(ComputedPlatform)' == 'iPhoneSimulator'">Release-iphonesimulator</BuildOutput>
        <CodesignEntitlements>Platforms/iOS/Entitlements.WidgetExtension.plist</CodesignEntitlements>
    </AdditionalAppExtensions>
</ItemGroup>

<!-- Auto-build widget during dotnet build. Skip with: -p:SkipWidgetBuild=true -->
<Target Name="BuildWidgetExtension"
        AfterTargets="ResolveReferences"
        Condition="$(TargetFramework.Contains('-ios')) AND '$(SkipWidgetBuild)' != 'true'">
    <PropertyGroup>
        <_WidgetIsSimulator>$(RuntimeIdentifier.Contains('simulator'))</_WidgetIsSimulator>
        <_WidgetSdk Condition="'$(_WidgetIsSimulator)' == 'true'">iphonesimulator</_WidgetSdk>
        <_WidgetSdk Condition="'$(_WidgetIsSimulator)' != 'true'">iphoneos</_WidgetSdk>
        <_WidgetArch Condition="$(RuntimeIdentifier.Contains('arm64'))">arm64</_WidgetArch>
        <_WidgetArch Condition="$(RuntimeIdentifier.Contains('x64'))">x86_64</_WidgetArch>
        <_WidgetArch Condition="'$(_WidgetArch)' == ''">arm64</_WidgetArch>
        <_WidgetOutputSubdir>Release-$(_WidgetSdk)</_WidgetOutputSubdir>
        <_XcodeProjectDir>$(MSBuildProjectDirectory)/XCodeWidget</_XcodeProjectDir>
        <_XcodeProjectPath>$(_XcodeProjectDir)/XCodeWidget.xcodeproj</_XcodeProjectPath>
        <_WidgetBuildDir>$(_XcodeProjectDir)/build</_WidgetBuildDir>
        <_WidgetAppexSource>$(_WidgetBuildDir)/$(_WidgetOutputSubdir)/MyWidgetExtension.appex</_WidgetAppexSource>
        <_WidgetDestDir>$(MSBuildProjectDirectory)/Platforms/iOS/WidgetExtensions/$(_WidgetOutputSubdir)</_WidgetDestDir>
    </PropertyGroup>

    <Exec Command="xcodegen generate"
          WorkingDirectory="$(_XcodeProjectDir)"
          Condition="!Exists('$(_XcodeProjectPath)') AND Exists('$(_XcodeProjectDir)/project.yml')" />

    <Message Text="Building widget extension for $(_WidgetSdk) $(_WidgetArch)..." Importance="high" />
    <Exec Command="xcodebuild -quiet -project XCodeWidget.xcodeproj -target MyWidgetExtension -configuration Release -sdk $(_WidgetSdk) -arch $(_WidgetArch) CODE_SIGN_IDENTITY=&quot;-&quot; CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO BUILD_DIR=build build"
          WorkingDirectory="$(_XcodeProjectDir)" />

    <MakeDir Directories="$(_WidgetDestDir)" />
    <Exec Command="rm -rf &quot;$(_WidgetDestDir)/MyWidgetExtension.appex&quot; &amp;&amp; cp -R &quot;$(_WidgetAppexSource)&quot; &quot;$(_WidgetDestDir)/&quot;" />
</Target>
```

**What this does:** A single `dotnet build -f net10.0-ios` automatically:
1. Generates `.xcodeproj` from `project.yml` if needed
2. Builds the widget with `xcodebuild`
3. Embeds the `.appex` in the app bundle with correct entitlements

### Step 5: Create the Swift Widget Extension

Create `XCodeWidget/` at the project root with these files. See `references/swift-templates.md` for complete code.

**Directory structure:**
```
XCodeWidget/
├── project.yml                          # xcodegen spec
├── XCodeWidget/                         # Thin host app (required by Xcode, not shipped)
│   ├── XCodeWidgetApp.swift
│   └── ContentView.swift
├── MyWidgetExtension/
│   ├── Info.plist                       # Full CFBundle keys required
│   ├── Settings.swift                   # Mirrors WidgetConstants.cs
│   ├── WidgetData.swift                 # Mirrors WidgetData.cs (Codable)
│   ├── SharedStorage.swift              # JSON file I/O via App Group container
│   ├── SimpleEntry.swift                # TimelineEntry
│   ├── Provider.swift                   # AppIntentTimelineProvider
│   ├── SimpleWidgetView.swift           # SwiftUI view with deep links
│   ├── SimpleWidget.swift               # Widget configuration
│   ├── SimpleWidgetBundle.swift         # @main entry point
│   └── Intents/                         # Optional: interactive buttons
│       ├── ConfigurationAppIntent.swift
│       └── IncrementCounterIntent.swift
├── MyWidgetExtension.entitlements       # App Group
└── build-release.sh                     # Manual build script (fallback)
```

**Key Swift files:**

**`Settings.swift`** — mirrors `WidgetConstants.cs` exactly:
```swift
enum Settings {
    static let groupId = "group.com.example.myapp"
    static let fromAppFile = "widget_data_fromapp.json"
    static let fromWidgetFile = "widget_data_fromwidget.json"
    static let urlScheme = "myapp"
    static let urlHost = "widget"
    static let widgetKind = "MyWidget"
}
```

**`SharedStorage.swift`** — JSON file I/O (NOT UserDefaults):
```swift
import Foundation

struct SharedStorage {
    static func read(filename: String) -> WidgetData? {
        guard let url = FileManager.default.containerURL(
            forSecurityApplicationGroupIdentifier: Settings.groupId)?
            .appendingPathComponent(filename),
              let data = try? Data(contentsOf: url) else { return nil }
        return try? JSONDecoder().decode(WidgetData.self, from: data)
    }

    static func write(_ widgetData: WidgetData, filename: String) {
        guard let url = FileManager.default.containerURL(
            forSecurityApplicationGroupIdentifier: Settings.groupId)?
            .appendingPathComponent(filename),
              let data = try? JSONEncoder().encode(widgetData) else { return }
        try? data.write(to: url)
    }
}
```

### Step 6: Create xcodegen project.yml

```yaml
name: XCodeWidget
options:
  bundleIdPrefix: com.example
  deploymentTarget:
    iOS: "17.0"

targets:
  XCodeWidget:
    type: application
    platform: iOS
    sources:
      - XCodeWidget
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: com.example.myapp

  MyWidgetExtension:
    type: app-extension
    platform: iOS
    sources:
      - MyWidgetExtension
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: com.example.myapp.widgetextension
        INFOPLIST_FILE: MyWidgetExtension/Info.plist
        GENERATE_INFOPLIST_FILE: "NO"
        CODE_SIGN_ENTITLEMENTS: MyWidgetExtension.entitlements
    entitlements:
      path: MyWidgetExtension.entitlements
      properties:
        com.apple.security.application-groups:
          - "group.com.example.myapp"
```

Then: `cd XCodeWidget && xcodegen generate`

### Step 7: Build and Test

**Simulator build (one command!):**
```bash
dotnet build MyApp.csproj -f net10.0-ios -r iossimulator-arm64 \
  -p:CodesignRequireProvisioningProfile=false
```

**⚠️ CRITICAL post-build step for simulator:** The MAUI build generates an empty `.xcent` when `CodesignRequireProvisioningProfile=false`, stripping the App Group entitlement. You MUST re-sign:
```bash
APP_PATH=$(find bin/Debug/net10.0-ios/iossimulator-arm64 -name "*.app" -maxdepth 1)
/usr/bin/codesign -v --force --timestamp=none --sign - \
  --entitlements Platforms/iOS/Entitlements.plist "$APP_PATH"
```

Without this, `NSFileManager.GetContainerUrl()` returns null and data sharing fails silently.

**Install and test:**
```bash
xcrun simctl install booted "$APP_PATH"
xcrun simctl launch booted com.example.myapp
# Long-press Home Screen → + → search for your widget → add it
```

**Device build (no re-signing needed):**
```bash
dotnet build -f net10.0-ios -t:Run
```

**Skip widget build** (C#-only changes): add `-p:SkipWidgetBuild=true`

## MauiWidgetCenter API Reference

Built into MAUI Core — no NuGet needed. Requires `MauiWidgetHelper.framework` in app bundle for reload methods.

| Method | Description |
|--------|-------------|
| `GetSharedContainerUrl(appGroupId)` | Returns `NSUrl?` for the App Group container directory (use for JSON file I/O) |
| `ReloadTimelines(kind)` | Reloads timelines for widgets of the specified kind |
| `ReloadAllTimelines()` | Reloads all widget timelines |
| `GetSharedDefaults(appGroupId)` | Returns `NSUserDefaults` for App Group (**⚠️ unreliable cross-process — prefer file I/O**) |

## Common Pitfalls

| Pitfall | Solution |
|---------|----------|
| **UserDefaults unreliable cross-process** | Use JSON files via `GetSharedContainerUrl()` / `containerURL(forSecurityApplicationGroupIdentifier:)` |
| **Empty `.xcent` strips entitlements** | Re-sign after simulator builds (see Step 7) |
| **CRLF in entitlements** | Always ensure LF line endings: `sed -i '' 's/\r$//'` |
| **Widget bundle ID mismatch** | Must be a child of app bundle ID (e.g., `com.myapp.widgetextension`) |
| **Widget not appearing** | Check `NSExtensionPointIdentifier` is `com.apple.widgetkit-extension`, check full CFBundle keys in Info.plist |
| **`GetContainerUrl` returns null** | App missing App Group entitlement — re-sign after build |
| **Widget shows stale data** | Call `ReloadTimelines` after writing JSON. WidgetKit throttles ~40-70/day |
| **AppIntents buttons override app data** | Use timestamp-based priority — compare `updatedAt` to pick most recent |
| **Xcode build fails** | Use `-target` not `-scheme` for xcodebuild. Ensure widget Info.plist has full CFBundle keys |

## Debugging

```bash
UDID="<simulator-udid>"

# Check if widget extension is embedded
ls path/to/YourApp.app/PlugIns/

# Check shared container for JSON files
find ~/Library/Developer/CoreSimulator/Devices/$UDID/data/ \
  -path "*group.com.example.myapp*" -name "*.json" -exec cat {} \;

# Stream widget extension logs
xcrun simctl spawn $UDID log stream \
  --predicate 'processImagePath contains "WidgetExtension"' --timeout 30

# Stream WidgetKit logs
xcrun simctl spawn $UDID log stream \
  --predicate 'subsystem == "com.apple.WidgetKit"' --timeout 30
```

## Reference Files

Complete code templates are in `references/`:
- **`references/csharp-templates.md`** — WidgetData, IWidgetDataService, WidgetConstants, WidgetDataService (iOS), AppDelegate, App.xaml.cs, MauiProgram.cs, MainPage
- **`references/swift-templates.md`** — Settings, WidgetData, SharedStorage, Provider, WidgetView, Widget config, AppIntents
- **`references/project-config.md`** — .csproj MSBuild targets, entitlements, Info.plist, xcodegen project.yml, build script

## Credits

Architecture based on [Maui.Apple.PlatformFeature.Samples](https://github.com/Redth/Maui.Apple.PlatformFeature.Samples) by Jon Dick (Redth), combined with MAUI Core `MauiWidgetCenter` and discoveries from end-to-end testing.

# Project Configuration for watchOS Companion App

Replace placeholders: `{AppBundleId}`, `{WatchBundleId}`, `{WatchAppName}`, `{ProjectName}`

## .csproj Additions

Add to the MAUI app's `.csproj`:

```xml
<!-- watchOS companion app auto-build -->
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
        <_WatchAppSource>$(_WatchBuildDir)/$(_WatchOutputSubdir)/{WatchAppName}.app</_WatchAppSource>
        <_WatchDestDir>$(MSBuildProjectDirectory)/Platforms/iOS/WatchApps/$(_WatchOutputSubdir)</_WatchDestDir>
    </PropertyGroup>

    <Exec Command="xcodegen generate"
          WorkingDirectory="$(_XcodeProjectDir)"
          Condition="!Exists('$(_XcodeProjectPath)') AND Exists('$(_XcodeProjectDir)/project.yml')" />

    <Message Text="Building watch app for $(_WatchSdk)..." Importance="high" />
    <Exec Command="xcodebuild -quiet -project WatchApp.xcodeproj -target {WatchAppName} -configuration Release -sdk $(_WatchSdk) -arch $(_WatchArch) CODE_SIGN_IDENTITY=&quot;-&quot; CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO BUILD_DIR=build build"
          WorkingDirectory="$(_XcodeProjectDir)" />

    <MakeDir Directories="$(_WatchDestDir)" />
    <Exec Command="rm -rf &quot;$(_WatchDestDir)/{WatchAppName}.app&quot; &amp;&amp; cp -R &quot;$(_WatchAppSource)&quot; &quot;$(_WatchDestDir)/&quot;" />
</Target>
```

## xcodegen project.yml

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
        PRODUCT_BUNDLE_IDENTIFIER: {AppBundleId}

  {WatchAppName}:
    type: application
    platform: watchOS
    sources:
      - {WatchAppName}
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: {WatchBundleId}
        INFOPLIST_FILE: {WatchAppName}/Info.plist
        GENERATE_INFOPLIST_FILE: "NO"
        SWIFT_VERSION: "5.0"
```

**Key xcodegen notes:**
- `type: application` with `platform: watchOS` — modern standalone watchOS app (watchOS 10+)
- **⚠️ Do NOT use `type: application.watchapp2`** — causes `CopyAndPreserveArchs` build conflict in Xcode 26+
- No `dependencies` block needed — the companion relationship is established via `WKCompanionAppBundleIdentifier` in Info.plist
- The iOS `WatchApp` target is a thin host app required by xcodegen but never shipped independently

## Host App Stubs

These are required by xcodegen but do nothing — the iOS MAUI app is the real host.

### WatchApp/WatchAppApp.swift

```swift
import SwiftUI

@main
struct WatchAppApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}
```

### WatchApp/ContentView.swift

```swift
import SwiftUI

struct ContentView: View {
    var body: some View {
        Text("Host App (not used)")
    }
}
```

## Directory Structure

```
{ProjectName}/
├── {ProjectName}.csproj               # MAUI project with BuildWatchApp target
├── Services/
│   ├── WatchData.cs                    # Data model (dictionary-based)
│   ├── IWatchConnectivityService.cs    # Platform-agnostic interface
│   └── StubWatchConnectivityService.cs # No-op for non-iOS
├── Platforms/
│   └── iOS/
│       ├── WatchConnectivityService.cs # WCSession wrapper
│       └── WatchApps/                  # Auto-populated by MSBuild target
│           ├── Release-watchos/
│           │   └── {WatchAppName}.app  # Device build
│           └── Release-watchsimulator/
│               └── {WatchAppName}.app  # Simulator build
├── WatchApp/                           # xcodegen project root
│   ├── project.yml                     # xcodegen spec
│   ├── WatchApp/                       # Thin iOS host app (required by Xcode)
│   │   ├── WatchAppApp.swift
│   │   └── ContentView.swift
│   ├── {WatchAppName}/                 # The actual watchOS app
│   │   ├── Info.plist
│   │   ├── {WatchAppName}App.swift     # @main entry point
│   │   ├── ContentView.swift           # Watch UI (SwiftUI)
│   │   └── PhoneConnectivityProvider.swift  # WCSession delegate
│   └── build-release.sh               # Manual build fallback
├── App.xaml.cs                         # Activates WCSession in CreateWindow
├── MauiProgram.cs                      # DI registration (singleton!)
└── MainPage.xaml[.cs]                  # UI with counter sync
```

## build-release.sh (Manual Build Fallback)

```bash
#!/bin/bash
set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

# Generate Xcode project if needed
if [ ! -d "WatchApp.xcodeproj" ] && [ -f "project.yml" ]; then
    echo "Generating Xcode project..."
    xcodegen generate
fi

# Determine SDK
SDK="${1:-watchsimulator}"
ARCH="arm64"
CONFIG="Release"

echo "Building watch app for $SDK ($ARCH)..."
xcodebuild -quiet \
    -project WatchApp.xcodeproj \
    -target {WatchAppName} \
    -configuration "$CONFIG" \
    -sdk "$SDK" \
    -arch "$ARCH" \
    CODE_SIGN_IDENTITY="-" \
    CODE_SIGNING_REQUIRED=NO \
    CODE_SIGNING_ALLOWED=NO \
    BUILD_DIR=build \
    build

echo "✅ Built: build/$CONFIG-$SDK/{WatchAppName}.app"
```

## Post-Build: Embed Watch App in iOS Bundle

After `dotnet build`, the watch app must be embedded in the iOS app's `Watch/` directory:

```bash
APP_PATH="path/to/YourApp.app"
WATCH_BUILD="WatchApp/build/Release-watchsimulator/{WatchAppName}.app"

# Create Watch directory and embed
mkdir -p "$APP_PATH/Watch"
rm -rf "$APP_PATH/Watch/{WatchAppName}.app"
cp -R "$WATCH_BUILD" "$APP_PATH/Watch/"

# Re-sign the iOS app (required after modifying bundle contents)
/usr/bin/codesign -v --force --timestamp=none --sign - "$APP_PATH"
```

**For device builds**, replace `watchsimulator` with `watchos` in the path.

## Simulator Pairing

watchOS simulator must be paired with iPhone simulator:

```bash
# List available watch simulators
xcrun simctl list devices watchOS available

# List existing pairs
xcrun simctl list pairs

# If not paired, create a pair
xcrun simctl pair <watch-udid> <iphone-udid>

# Boot both simulators
xcrun simctl boot <iphone-udid>
xcrun simctl boot <watch-udid>

# Install iOS app (watch app auto-installs via pairing)
xcrun simctl install <iphone-udid> path/to/YourApp.app
```

**Note:** Xcode usually auto-pairs simulators. Check Window → Devices and Simulators for existing pairs.

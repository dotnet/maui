---
name: setup-widget
description: Sets up an iOS Home Screen Widget for a .NET MAUI application. Configures App Groups, entitlements, a SwiftUI WidgetKit extension, and MAUI-side data sharing via MauiWidgetCenter.
metadata:
  author: dotnet-maui
  version: "2.0"
compatibility: Requires .NET MAUI with iOS/MacCatalyst target, Xcode with iOS SDK 17+, WidgetKit.
---

# Setup iOS Home Screen Widget Skill

Configures a .NET MAUI iOS app to include a Home Screen Widget using Apple's WidgetKit. Guides the user through App Group setup, entitlements, creating a SwiftUI widget extension, building and embedding it, and sharing data from the MAUI app.

## Overview

iOS Widgets use Apple's **WidgetKit** framework with **SwiftUI** for rendering. They run as a separate extension process — not inside the MAUI app. Communication between the MAUI app and widget happens through **App Groups** (shared `NSUserDefaults` + JSON file fallback).

**Architecture:**
```
┌─────────────────────────┐      App Group       ┌─────────────────────────┐
│     .NET MAUI App       │  ← NSUserDefaults →  │   Widget Extension      │
│                         │  ← JSON file →        │   (Swift/SwiftUI)       │
│  MauiWidgetCenter       │                       │   TimelineProvider      │
│   .GetSharedDefaults()  │  ── ReloadTimelines → │   reads shared data     │
│   .ReloadTimelines()    │                       │   renders SwiftUI view  │
└─────────────────────────┘                       └─────────────────────────┘
        │                                                    │
        └──── MauiWidgetHelper.framework ────────────────────┘
              (Swift bridge for WidgetKit API)
```

**Key components:**
- **`MauiWidgetCenter`** — C# helper in MAUI Core for writing shared data and triggering reloads
- **`MauiWidgetHelper.framework`** — Small Swift framework that bridges `WidgetCenter.shared.reloadTimelines()` to ObjC (since WidgetKit is Swift-only and has no stable ObjC class names)
- **Widget extension (.appex)** — SwiftUI-based WidgetKit extension

## Critical Build Requirements

### ⚠️ Extension binary MUST use `_NSExtensionMain` entry point

Widget extensions are XPC services, **not** standalone executables. Building with `swiftc -emit-executable` produces a binary with `_main` entry point that will **crash silently** when chronod tries to communicate with it.

**Correct approach: 2-step compile + link**
```bash
# Step 1: Compile to object file
xcrun swiftc -sdk "$SDK_PATH" -target "$TARGET" -O \
    -module-name MyWidgetExtension -parse-as-library \
    -application-extension -c \
    -o obj/MyWidget.o MyWidget.swift

# Step 2: Link with _NSExtensionMain entry point
xcrun clang -target "$TARGET" -isysroot "$SDK_PATH" \
    -fapplication-extension -e _NSExtensionMain \
    -dead_strip -fobjc-link-runtime \
    -Xlinker -rpath -Xlinker @executable_path/../../Frameworks \
    obj/MyWidget.o -o MyWidgetExtension.appex/MyWidgetExtension
```

**❌ WRONG** (produces broken binary):
```bash
xcrun swiftc ... -emit-executable -o MyWidgetExtension
```

### ⚠️ WidgetKit has NO stable ObjC class names

`WidgetCenter` is a Swift-only class. `Class.GetHandle("WGWidgetCenter")` returns null. You **cannot** call `WidgetCenter.shared.reloadTimelines()` via `objc_msgSend`.

**Solution: MauiWidgetHelper.framework** — a tiny Swift framework bundled in the app:

```swift
// WidgetHelper.swift
import Foundation
import WidgetKit

@objc(MauiWidgetHelper)
public class MauiWidgetHelper: NSObject {
    @objc public static func reloadTimelines(_ kind: String) {
        if #available(iOS 14.0, *) {
            WidgetCenter.shared.reloadTimelines(ofKind: kind)
        }
    }
    @objc public static func reloadAllTimelines() {
        if #available(iOS 14.0, *) {
            WidgetCenter.shared.reloadAllTimelines()
        }
    }
}
```

Build as a dynamic framework and embed in `YourApp.app/Frameworks/`.

### ⚠️ Info.plist must NOT use `$(PRODUCT_MODULE_NAME)`

Xcode resolves `$(PRODUCT_MODULE_NAME)` during builds, but manual `swiftc`/`clang` does not. Always use hardcoded values in widget Info.plist. Also, `NSExtensionPrincipalClass` is **not required** for WidgetKit extensions.

## When to Use This Skill

- User wants to add a Home Screen Widget to their MAUI iOS/MacCatalyst app
- User asks about widgets, WidgetKit, or Home Screen extensions
- User wants to display app data on the Home Screen

## Step-by-Step Setup

### Step 1: Configure App Groups

App Groups enable data sharing between the main app and the widget extension.

**Create or update `Platforms/iOS/Entitlements.plist`:**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.application-groups</key>
    <array>
        <string>group.com.yourcompany.yourapp</string>
    </array>
</dict>
</plist>
```

**Add to your `.csproj`:**

```xml
<PropertyGroup>
    <CodesignEntitlements Condition="$(TargetFramework.Contains('-ios'))">Platforms/iOS/Entitlements.plist</CodesignEntitlements>
</PropertyGroup>
```

**⚠️ Simulator signing note**: When re-signing the app to embed the widget, you **must** pass `--entitlements` to preserve app-groups:
```bash
codesign --force --sign - --entitlements Platforms/iOS/Entitlements.plist YourApp.app
```
Signing without `--entitlements` strips all entitlements, breaking App Group data sharing.

### Step 2: Share Data from MAUI App

Use `MauiWidgetCenter` to write shared data and trigger widget refreshes. Write data via **both** UserDefaults and a JSON file for reliable simulator support:

```csharp
#if IOS || MACCATALYST
#pragma warning disable CA1416
using Microsoft.Maui;

const string AppGroup = "group.com.yourcompany.yourapp";
const string WidgetKind = "MyWidgetKind";

void UpdateWidget(string title, string message)
{
    // UserDefaults (works on device with provisioning)
    var defaults = MauiWidgetCenter.GetSharedDefaults(AppGroup);
    if (defaults is not null)
    {
        defaults.SetString(title, "widget_title");
        defaults.SetString(message, "widget_message");
        defaults.Synchronize();
    }
    
    // JSON file fallback (reliable on simulator)
    var containerUrl = Foundation.NSFileManager.DefaultManager.GetContainerUrl(AppGroup);
    if (containerUrl is not null)
    {
        var json = $"{{\"title\":\"{title}\",\"message\":\"{message}\"}}";
        var filePath = System.IO.Path.Combine(containerUrl.Path!, "widget_data.json");
        System.IO.File.WriteAllText(filePath, json);
    }
    
    MauiWidgetCenter.ReloadTimelines(WidgetKind);
}
#pragma warning restore CA1416
#endif
```

**Common pattern — update widgets on app launch:**

```csharp
// MauiProgram.cs
.ConfigureLifecycleEvents(events =>
{
#if IOS || MACCATALYST
#pragma warning disable CA1416
    events.AddiOS(ios => ios
        .FinishedLaunching((app, options) =>
        {
            UpdateWidget("My App", "Updated at " + DateTime.Now.ToString("t"));
            return true;
        }));
#pragma warning restore CA1416
#endif
})
```

### Step 3: Create the MauiWidgetHelper Framework

This small Swift framework bridges WidgetKit's Swift-only API to ObjC for `MauiWidgetCenter` to call.

**`Platforms/iOS/Extensions/MyWidgetExtension/WidgetHelper.swift`:**

```swift
import Foundation
import WidgetKit

@objc(MauiWidgetHelper)
public class MauiWidgetHelper: NSObject {
    @objc public static func reloadTimelines(_ kind: String) {
        if #available(iOS 14.0, *) {
            WidgetCenter.shared.reloadTimelines(ofKind: kind)
        }
    }
    @objc public static func reloadAllTimelines() {
        if #available(iOS 14.0, *) {
            WidgetCenter.shared.reloadAllTimelines()
        }
    }
}
```

### Step 4: Create the Widget Extension (Swift)

Widget extensions **must** be written in Swift/SwiftUI — this is a WidgetKit requirement. Create the following files:

**Directory structure:**
```
Platforms/iOS/Extensions/MyWidgetExtension/
├── MyWidget.swift
├── WidgetHelper.swift
├── Info.plist
└── MyWidgetExtension.entitlements
```

**`MyWidget.swift`:**

```swift
import WidgetKit
import SwiftUI

struct MyWidgetEntry: TimelineEntry {
    let date: Date
    let title: String
    let message: String
}

struct MyWidgetProvider: TimelineProvider {
    let appGroupId = "group.com.yourcompany.yourapp"

    func placeholder(in context: Context) -> MyWidgetEntry {
        MyWidgetEntry(date: Date(), title: "My App", message: "Loading...")
    }

    func getSnapshot(in context: Context, completion: @escaping (MyWidgetEntry) -> Void) {
        completion(loadEntry())
    }

    func getTimeline(in context: Context, completion: @escaping (Timeline<MyWidgetEntry>) -> Void) {
        let entry = loadEntry()
        let nextUpdate = Calendar.current.date(byAdding: .minute, value: 15, to: Date())!
        let timeline = Timeline(entries: [entry], policy: .after(nextUpdate))
        completion(timeline)
    }

    private func loadEntry() -> MyWidgetEntry {
        // Try App Group UserDefaults first
        if let defaults = UserDefaults(suiteName: appGroupId),
           let title = defaults.string(forKey: "widget_title") {
            let message = defaults.string(forKey: "widget_message") ?? "Tap to open"
            return MyWidgetEntry(date: Date(), title: title, message: message)
        }

        // Fallback: shared JSON file (reliable on simulator)
        if let containerURL = FileManager.default.containerURL(
               forSecurityApplicationGroupIdentifier: appGroupId),
           let data = try? Data(contentsOf: containerURL.appendingPathComponent("widget_data.json")),
           let json = try? JSONSerialization.jsonObject(with: data) as? [String: String] {
            return MyWidgetEntry(
                date: Date(),
                title: json["title"] ?? "My App",
                message: json["message"] ?? "Tap to open")
        }

        return MyWidgetEntry(date: Date(), title: "My App", message: "Open app to see data")
    }
}

struct MyWidgetEntryView: View {
    var entry: MyWidgetProvider.Entry

    var body: some View {
        VStack(spacing: 8) {
            Text(entry.title)
                .font(.headline)
            Text(entry.message)
                .font(.subheadline)
                .foregroundColor(.secondary)
            Text(entry.date, style: .time)
                .font(.caption2)
                .foregroundColor(.gray)
        }
        .padding()
        .containerBackground(.fill.tertiary, for: .widget)
    }
}

@main
struct MyAppWidget: Widget {
    let kind: String = "MyWidgetKind"

    var body: some WidgetConfiguration {
        StaticConfiguration(kind: kind, provider: MyWidgetProvider()) { entry in
            MyWidgetEntryView(entry: entry)
        }
        .configurationDisplayName("My App Widget")
        .description("Shows data from the app.")
        .supportedFamilies([.systemSmall, .systemMedium])
    }
}
```

**Important**: The `kind` string in the Widget struct must match the `kind` you pass to `MauiWidgetCenter.ReloadTimelines()`.

**`Info.plist`:**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleDisplayName</key>
    <string>My Widget</string>
    <key>CFBundleExecutable</key>
    <string>MyWidgetExtension</string>
    <key>CFBundleIdentifier</key>
    <string>com.yourcompany.yourapp.MyWidgetExtension</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>MyWidgetExtension</string>
    <key>CFBundlePackageType</key>
    <string>XPC!</string>
    <key>CFBundleShortVersionString</key>
    <string>1.0</string>
    <key>CFBundleVersion</key>
    <string>1</string>
    <key>MinimumOSVersion</key>
    <string>17.0</string>
    <key>NSExtension</key>
    <dict>
        <key>NSExtensionPointIdentifier</key>
        <string>com.apple.widgetkit-extension</string>
    </dict>
</dict>
</plist>
```

**⚠️ Critical Info.plist rules:**
- `CFBundleIdentifier` must be a child of main app bundle ID (e.g., `com.yourcompany.yourapp.MyWidgetExtension`)
- Do NOT use `$(PRODUCT_MODULE_NAME)` or `$(MAIN_BUNDLE_ID)` — these are Xcode variables not resolved by swiftc/clang
- `NSExtensionPrincipalClass` is NOT needed for WidgetKit extensions
- `MinimumOSVersion` should be set to `17.0` or your minimum supported version

**`MyWidgetExtension.entitlements`:**

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.application-groups</key>
    <array>
        <string>group.com.yourcompany.yourapp</string>
    </array>
</dict>
</plist>
```

### Step 5: Build Script

Create a build script at the root of your project:

**`build-widget-extension.sh`:**

```bash
#!/bin/bash
# Builds WidgetKit extension + helper framework for iOS.
# Usage: ./build-widget-extension.sh [iossimulator-arm64|iossimulator-x64|ios-arm64]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
EXTENSION_DIR="$SCRIPT_DIR/Platforms/iOS/Extensions/MyWidgetExtension"
ARCH="${1:-iossimulator-arm64}"

case "$ARCH" in
    iossimulator-arm64) TARGET="arm64-apple-ios17.0-simulator" ;;
    iossimulator-x64)   TARGET="x86_64-apple-ios17.0-simulator" ;;
    ios-arm64)          TARGET="arm64-apple-ios17.0" ;;
    *)                  echo "Unknown arch: $ARCH"; exit 1 ;;
esac

SDK_NAME="iphonesimulator"
[[ "$ARCH" == "ios-arm64" ]] && SDK_NAME="iphoneos"
SDK_PATH="$(xcrun --sdk "$SDK_NAME" --show-sdk-path)"

OUTPUT_DIR="$SCRIPT_DIR/bin/widget/$ARCH"
APPEX_DIR="$OUTPUT_DIR/MyWidgetExtension.appex"
FRAMEWORK_DIR="$OUTPUT_DIR/MauiWidgetHelper.framework"
OBJ_DIR="$OUTPUT_DIR/obj"

echo "Building widget extension for $ARCH..."
rm -rf "$APPEX_DIR" "$FRAMEWORK_DIR" "$OBJ_DIR"
mkdir -p "$APPEX_DIR" "$FRAMEWORK_DIR" "$OBJ_DIR"

# --- Build MauiWidgetHelper.framework ---
xcrun swiftc \
    -sdk "$SDK_PATH" -target "$TARGET" -O \
    -module-name MauiWidgetHelper \
    -emit-library \
    -Xlinker -install_name -Xlinker @rpath/MauiWidgetHelper.framework/MauiWidgetHelper \
    -o "$FRAMEWORK_DIR/MauiWidgetHelper" \
    "$EXTENSION_DIR/WidgetHelper.swift"

cat > "$FRAMEWORK_DIR/Info.plist" << FWPLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key><string>MauiWidgetHelper</string>
    <key>CFBundleIdentifier</key><string>com.yourcompany.yourapp.MauiWidgetHelper</string>
    <key>CFBundleInfoDictionaryVersion</key><string>6.0</string>
    <key>CFBundleName</key><string>MauiWidgetHelper</string>
    <key>CFBundlePackageType</key><string>FMWK</string>
    <key>CFBundleShortVersionString</key><string>1.0</string>
    <key>CFBundleVersion</key><string>1</string>
    <key>MinimumOSVersion</key><string>17.0</string>
</dict>
</plist>
FWPLIST

# --- Build Widget Extension (.appex) ---
# Step 1: Compile to object file
xcrun swiftc \
    -sdk "$SDK_PATH" -target "$TARGET" -O \
    -module-name MyWidgetExtension \
    -parse-as-library -application-extension \
    -c -o "$OBJ_DIR/MyWidget.o" \
    "$EXTENSION_DIR/MyWidget.swift"

# Step 2: Link with _NSExtensionMain entry point (critical for XPC)
xcrun clang \
    -target "$TARGET" -isysroot "$SDK_PATH" \
    -fapplication-extension -e _NSExtensionMain \
    -dead_strip -fobjc-link-runtime \
    -Xlinker -rpath -Xlinker @executable_path/../../Frameworks \
    "$OBJ_DIR/MyWidget.o" \
    -o "$APPEX_DIR/MyWidgetExtension"

cp "$EXTENSION_DIR/Info.plist" "$APPEX_DIR/Info.plist"

# Sign with entitlements
ENTITLEMENTS="$EXTENSION_DIR/MyWidgetExtension.entitlements"
if [[ -f "$ENTITLEMENTS" ]]; then
    codesign --force --sign - --entitlements "$ENTITLEMENTS" "$APPEX_DIR"
else
    codesign --force --sign - "$APPEX_DIR"
fi

codesign --force --sign - "$FRAMEWORK_DIR"

rm -rf "$OBJ_DIR"
echo "✅ Built: $APPEX_DIR"
echo "✅ Built: $FRAMEWORK_DIR"
```

### Step 6: Embed in MAUI App Bundle

After building both the MAUI app and widget extension, embed the `.appex` and helper framework:

```bash
APP_BUNDLE="path/to/YourApp.app"
WIDGET="bin/widget/iossimulator-arm64/MyWidgetExtension.appex"
HELPER="bin/widget/iossimulator-arm64/MauiWidgetHelper.framework"

# Embed widget extension
mkdir -p "$APP_BUNDLE/PlugIns"
cp -R "$WIDGET" "$APP_BUNDLE/PlugIns/"

# Embed helper framework
mkdir -p "$APP_BUNDLE/Frameworks"
cp -R "$HELPER" "$APP_BUNDLE/Frameworks/"

# Sign everything (MUST include --entitlements to preserve app-groups)
codesign --force --sign - "$APP_BUNDLE/Frameworks/MauiWidgetHelper.framework"
codesign --force --sign - --entitlements Platforms/iOS/Entitlements.plist "$APP_BUNDLE"
```

### Step 7: Test on Simulator

1. Build the MAUI app: `dotnet build -f net11.0-ios -c Debug`
2. Build the widget extension: `bash build-widget-extension.sh`
3. Embed and install (see Step 6)
4. In the Simulator, **long-press the Home Screen → tap +** → search for your widget
5. Add the widget to the Home Screen
6. Tap the "Update Widget" button in the app — widget should refresh

## MauiWidgetCenter API Reference

| Method | Description |
|--------|-------------|
| `ReloadTimelines(string kind)` | Reloads timelines for widgets matching the specified kind |
| `ReloadAllTimelines()` | Reloads timelines for all widgets in the app |
| `GetSharedDefaults(string appGroupId)` | Gets `NSUserDefaults` for the App Group (shared with widget extension) |

**Platform support**: iOS 14.0+, MacCatalyst 14.0+

**Note**: `MauiWidgetCenter` uses a bundled `MauiWidgetHelper.framework` (Swift) to call WidgetKit APIs, because WidgetKit is a Swift-only framework with no stable ObjC class names accessible via `objc_msgSend`.

## Widget Families

| Family | Size | Best For |
|--------|------|----------|
| `.systemSmall` | 2×2 grid | Quick glance, single tap target |
| `.systemMedium` | 4×2 grid | More detail, multiple data points |
| `.systemLarge` | 4×4 grid | Rich content, lists |
| `.systemExtraLarge` | iPad only | Dashboard-style |
| `.accessoryCircular` | Lock Screen | Small circular widget |
| `.accessoryRectangular` | Lock Screen | Compact rectangular widget |
| `.accessoryInline` | Lock Screen | Single line of text |

## Data Sharing Patterns

### Dual-Write Pattern (Recommended)

Write data via both UserDefaults and a JSON file for maximum compatibility:

```csharp
// MAUI app writes (both methods)
var defaults = MauiWidgetCenter.GetSharedDefaults("group.com.myapp");
defaults?.SetString("Hello", "greeting");
defaults?.Synchronize();

var containerUrl = NSFileManager.DefaultManager.GetContainerUrl("group.com.myapp");
if (containerUrl is not null)
{
    var json = JsonSerializer.Serialize(new { greeting = "Hello" });
    File.WriteAllText(Path.Combine(containerUrl.Path!, "widget_data.json"), json);
}
```

```swift
// Widget reads (tries UserDefaults first, then JSON file)
if let defaults = UserDefaults(suiteName: "group.com.myapp"),
   let greeting = defaults.string(forKey: "greeting") {
    // Use UserDefaults value
} else if let url = FileManager.default.containerURL(
               forSecurityApplicationGroupIdentifier: "group.com.myapp"),
          let data = try? Data(contentsOf: url.appendingPathComponent("widget_data.json")),
          let json = try? JSONSerialization.jsonObject(with: data) as? [String: String] {
    // Use JSON file value
}
```

**Why dual-write?** On the iOS Simulator with ad-hoc signing, UserDefaults App Group sharing may not work reliably. The JSON file approach uses `containerURL(forSecurityApplicationGroupIdentifier:)` which is more reliable.

## Common Pitfalls

| Pitfall | Details |
|---------|---------|
| Using `swiftc -emit-executable` for extension | MUST use 2-step compile+link with `-e _NSExtensionMain`. See "Critical Build Requirements" |
| `$(PRODUCT_MODULE_NAME)` in Info.plist | Xcode resolves this, but manual builds don't. Hardcode all values |
| Signing without `--entitlements` | Strips app-groups entitlement, breaking data sharing |
| Widget extension bundle ID | Must be a child of the main app bundle ID |
| Missing `MauiWidgetHelper.framework` | `MauiWidgetCenter.ReloadTimelines()` silently no-ops without it |
| `kind` string mismatch | The `kind` in Swift `Widget` struct must match `MauiWidgetCenter.ReloadTimelines(kind)` |
| Only using UserDefaults on simulator | Add JSON file fallback for reliable simulator testing |
| Timeline refresh limits | WidgetKit budgets ~40-70 refreshes/day. Don't call `ReloadTimelines` excessively |

## Debugging

### Widget Not Appearing in Picker

```bash
# Check if extension is registered
xcrun simctl spawn $UDID pluginkit -m -p com.apple.widgetkit-extension | grep yourapp

# Check chronod logs for errors
xcrun simctl spawn $UDID log show \
  --predicate 'process == "chronod" AND message CONTAINS "yourwidget"' --last 5m
```

Common causes:
- Binary has `_main` instead of `_NSExtensionMain` entry point (rebuild with 2-step approach)
- Missing `MinimumOSVersion` in Info.plist
- Missing `MauiWidgetHelper.framework/Info.plist` (causes install failure)

### Widget Shows Default Data

```bash
# Check App Group plist contents
find ~/Library/Developer/CoreSimulator/Devices/$UDID/data/ \
  -name "group.com.yourcompany.yourapp.plist" -exec plutil -p {} \;

# Check JSON file
find ~/Library/Developer/CoreSimulator/Devices/$UDID/data/ \
  -name "widget_data.json" -exec cat {} \;
```

### Force Widget Refresh

From the MAUI app:
```csharp
MauiWidgetCenter.ReloadAllTimelines();
```

Or in the Simulator, remove and re-add the widget.

## Complete Working Example

See the Sandbox app for a working demo:
- **MAUI side**: `src/Controls/samples/Controls.Sample.Sandbox/MauiProgram.cs` — seeds widget data on launch, provides `UpdateWidgetData()` method
- **Widget extension**: `src/Controls/samples/Controls.Sample.Sandbox/Platforms/iOS/Extensions/MauiWidgetExtension/` — SwiftUI widget that reads from App Group
- **Helper framework**: `WidgetHelper.swift` in the same directory
- **Build script**: `src/Controls/samples/Controls.Sample.Sandbox/build-widget-extension.sh`

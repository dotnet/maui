---
name: setup-widget
description: Sets up an iOS Home Screen Widget for a .NET MAUI application. Configures App Groups, entitlements, a SwiftUI WidgetKit extension, and MAUI-side data sharing via MauiWidgetCenter.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires .NET MAUI with iOS/MacCatalyst target, Xcode with iOS SDK 17+, WidgetKit.
---

# Setup iOS Home Screen Widget Skill

Configures a .NET MAUI iOS app to include a Home Screen Widget using Apple's WidgetKit. Guides the user through App Group setup, entitlements, creating a SwiftUI widget extension, building and embedding it, and sharing data from the MAUI app.

## Overview

iOS Widgets use Apple's **WidgetKit** framework with **SwiftUI** for rendering. They run as a separate extension process — not inside the MAUI app. Communication between the MAUI app and widget happens through **App Groups** (shared `NSUserDefaults`).

**Architecture:**
```
┌─────────────────────────┐      App Group       ┌─────────────────────────┐
│     .NET MAUI App       │  ← NSUserDefaults →  │   Widget Extension      │
│                         │    (shared data)      │   (Swift/SwiftUI)       │
│  MauiWidgetCenter       │                       │   TimelineProvider      │
│   .GetSharedDefaults()  │  ── ReloadTimelines → │   reads shared data     │
│   .ReloadTimelines()    │                       │   renders SwiftUI view  │
└─────────────────────────┘                       └─────────────────────────┘
```

**MAUI provides `MauiWidgetCenter`** — a static helper that wraps WidgetKit APIs (which aren't in the .NET iOS bindings) via ObjC runtime interop.

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

### Step 2: Share Data from MAUI App

Use `MauiWidgetCenter` to write shared data and trigger widget refreshes:

```csharp
#if IOS || MACCATALYST
#pragma warning disable CA1416
using Microsoft.Maui;

// Write data to the shared App Group
var defaults = MauiWidgetCenter.GetSharedDefaults("group.com.yourcompany.yourapp");
if (defaults is not null)
{
    defaults.SetString("Hello from MAUI!", "widget_message");
    defaults.SetString("My App", "widget_title");
    defaults.Synchronize();
}

// Tell WidgetKit to refresh the widget
MauiWidgetCenter.ReloadTimelines("MyWidgetKind");

// Or reload all widgets
MauiWidgetCenter.ReloadAllTimelines();
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
            var defaults = MauiWidgetCenter.GetSharedDefaults("group.com.yourcompany.yourapp");
            defaults?.SetString("Updated at " + DateTime.Now.ToString("t"), "widget_message");
            defaults?.Synchronize();
            MauiWidgetCenter.ReloadTimelines("MyWidgetKind");
            return true;
        }));
#pragma warning restore CA1416
#endif
})
```

### Step 3: Create the Widget Extension (Swift)

Widget extensions **must** be written in Swift/SwiftUI — this is a WidgetKit requirement. Create the following files:

**Directory structure:**
```
Platforms/iOS/Extensions/MyWidgetExtension/
├── MyWidget.swift
└── Info.plist
```

**`MyWidget.swift`:**

```swift
import WidgetKit
import SwiftUI

// Timeline entry — the data model for each widget refresh
struct MyWidgetEntry: TimelineEntry {
    let date: Date
    let title: String
    let message: String
}

// Timeline provider — supplies data to the widget
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
        let defaults = UserDefaults(suiteName: appGroupId)
        let title = defaults?.string(forKey: "widget_title") ?? "My App"
        let message = defaults?.string(forKey: "widget_message") ?? "Open app to see data"
        return MyWidgetEntry(date: Date(), title: title, message: message)
    }
}

// Widget view — SwiftUI layout
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

// Widget configuration
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
    <string>$(MAIN_BUNDLE_ID).MyWidgetExtension</string>
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
    <key>NSExtension</key>
    <dict>
        <key>NSExtensionPointIdentifier</key>
        <string>com.apple.widgetkit-extension</string>
        <key>NSExtensionPrincipalClass</key>
        <string>$(PRODUCT_MODULE_NAME).MyAppWidget</string>
    </dict>
</dict>
</plist>
```

**Critical**: The `NSExtensionPrincipalClass` value must match `$(PRODUCT_MODULE_NAME).<YourWidgetStructName>`. The `CFBundleIdentifier` must be a child of your main app's bundle ID (e.g., `com.yourcompany.yourapp.MyWidgetExtension`).

### Step 4: Build the Widget Extension

Create a build script at the root of your project:

**`build-widget-extension.sh`:**

```bash
#!/bin/bash
set -euo pipefail

EXTENSION_DIR="Platforms/iOS/Extensions/MyWidgetExtension"
ARCH="${1:-iossimulator-arm64}"

case "$ARCH" in
    iossimulator-arm64) TARGET="arm64-apple-ios17.0-simulator"; SDK="iphonesimulator" ;;
    iossimulator-x64)   TARGET="x86_64-apple-ios17.0-simulator"; SDK="iphonesimulator" ;;
    ios-arm64)          TARGET="arm64-apple-ios17.0"; SDK="iphoneos" ;;
    *)                  echo "Unknown arch: $ARCH"; exit 1 ;;
esac

SDK_PATH="$(xcrun --sdk "$SDK" --show-sdk-path)"
APPEX_DIR="bin/widget/$ARCH/MyWidgetExtension.appex"

rm -rf "$APPEX_DIR"
mkdir -p "$APPEX_DIR"

xcrun swiftc \
    -sdk "$SDK_PATH" \
    -target "$TARGET" \
    -O \
    -module-name MyWidgetExtension \
    -parse-as-library \
    -emit-executable \
    -o "$APPEX_DIR/MyWidgetExtension" \
    "$EXTENSION_DIR/MyWidget.swift"

cp "$EXTENSION_DIR/Info.plist" "$APPEX_DIR/Info.plist"

# Replace bundle ID placeholder
sed -i '' "s/\$(MAIN_BUNDLE_ID)/com.yourcompany.yourapp/g" "$APPEX_DIR/Info.plist"
sed -i '' "s/\$(PRODUCT_MODULE_NAME)/MyWidgetExtension/g" "$APPEX_DIR/Info.plist"

codesign --force --sign - "$APPEX_DIR"
echo "✅ Built: $APPEX_DIR"
```

### Step 5: Embed in MAUI App Bundle

After building both the MAUI app and widget extension, embed the `.appex`:

```bash
APP_BUNDLE="path/to/YourApp.app"
WIDGET="bin/widget/iossimulator-arm64/MyWidgetExtension.appex"

mkdir -p "$APP_BUNDLE/PlugIns"
cp -R "$WIDGET" "$APP_BUNDLE/PlugIns/"
codesign --force --sign - "$APP_BUNDLE/PlugIns/MyWidgetExtension.appex"
codesign --force --sign - "$APP_BUNDLE"
```

**For automated builds**, add a custom MSBuild target to your `.csproj`:

```xml
<Target Name="_EmbedWidgetExtension" AfterTargets="_CodesignAppBundle"
        Condition="$(TargetFramework.Contains('-ios'))">
    <Exec Command="bash build-widget-extension.sh iossimulator-arm64" />
    <MakeDir Directories="$(AppBundleDir)/PlugIns" />
    <Exec Command="cp -R bin/widget/iossimulator-arm64/MyWidgetExtension.appex $(AppBundleDir)/PlugIns/" />
    <Exec Command="codesign --force --sign - $(AppBundleDir)/PlugIns/MyWidgetExtension.appex" />
    <Exec Command="codesign --force --sign - $(AppBundleDir)" />
</Target>
```

### Step 6: Test on Simulator

1. Build the MAUI app: `dotnet build -f net11.0-ios -c Debug`
2. Build the widget extension: `bash build-widget-extension.sh`
3. Embed and install (see Step 5)
4. In the Simulator, **long-press the Home Screen → tap +** → search for your widget
5. Add the widget to the Home Screen

**Verifying App Group data sharing:**

```bash
# Check if the widget extension is embedded
ls path/to/YourApp.app/PlugIns/

# Check shared defaults (after running the app)
UDID="<simulator-udid>"
xcrun simctl spawn $UDID defaults read group.com.yourcompany.yourapp
```

## MauiWidgetCenter API Reference

| Method | Description |
|--------|-------------|
| `ReloadTimelines(string kind)` | Reloads timelines for widgets matching the specified kind |
| `ReloadAllTimelines()` | Reloads timelines for all widgets in the app |
| `GetSharedDefaults(string appGroupId)` | Gets `NSUserDefaults` for the App Group (shared with widget extension) |

**Platform support**: iOS 14.0+, MacCatalyst 14.0+

**Note**: `MauiWidgetCenter` uses ObjC runtime interop because WidgetKit is not included in the .NET iOS bindings. The methods gracefully no-op if WidgetKit is unavailable.

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

### Simple Key-Value (NSUserDefaults)

Best for small amounts of data (strings, numbers, bools):

```csharp
// MAUI app writes
var defaults = MauiWidgetCenter.GetSharedDefaults("group.com.myapp");
defaults?.SetString("Hello", "greeting");
defaults?.SetInt(42, "count");
defaults?.Synchronize();
```

```swift
// Widget reads
let defaults = UserDefaults(suiteName: "group.com.myapp")
let greeting = defaults?.string(forKey: "greeting") ?? "Default"
let count = defaults?.integer(forKey: "count") ?? 0
```

### JSON for Complex Data

For structured data, serialize to JSON:

```csharp
// MAUI app writes
var data = new { Items = new[] { "Item 1", "Item 2" }, UpdatedAt = DateTime.UtcNow };
var json = System.Text.Json.JsonSerializer.Serialize(data);
var defaults = MauiWidgetCenter.GetSharedDefaults("group.com.myapp");
defaults?.SetString(json, "widget_data");
defaults?.Synchronize();
MauiWidgetCenter.ReloadTimelines("MyWidget");
```

```swift
// Widget reads
struct WidgetData: Codable {
    let items: [String]
    let updatedAt: Date
}

let defaults = UserDefaults(suiteName: "group.com.myapp")
if let json = defaults?.string(forKey: "widget_data"),
   let data = try? JSONDecoder().decode(WidgetData.self, from: Data(json.utf8)) {
    // Use data
}
```

## Common Pitfalls

| Pitfall | Details |
|---------|---------|
| Widget extension bundle ID | Must be a child of the main app bundle ID (e.g., `com.myapp.MyWidget`) |
| Forgetting `defaults.Synchronize()` | Data may not be written to disk before widget reads it |
| `kind` string mismatch | The `kind` in Swift `Widget` struct must match `MauiWidgetCenter.ReloadTimelines(kind)` |
| Widget not appearing in picker | Ensure `NSExtensionPointIdentifier` is `com.apple.widgetkit-extension` and `NSExtensionPrincipalClass` is correct |
| Using `#if IOS` for widget code | Use `#if IOS \|\| MACCATALYST` — widgets work on both platforms |
| Large data in NSUserDefaults | Keep shared data small; use shared file container for large payloads |
| Timeline refresh limits | WidgetKit budgets ~40-70 refreshes/day. Don't call `ReloadTimelines` excessively |

## Debugging

### Check Widget Extension Logs

```bash
# Stream widget extension logs
UDID="<simulator-udid>"
xcrun simctl spawn $UDID log stream \
  --predicate 'processImagePath contains "WidgetExtension"' --timeout 30

# Check if widget was loaded
xcrun simctl spawn $UDID log stream \
  --predicate 'subsystem == "com.apple.WidgetKit"' --timeout 30
```

### Verify Shared Data

```bash
# Read App Group defaults
xcrun simctl spawn $UDID defaults read group.com.yourcompany.yourapp
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
- **Build script**: `src/Controls/samples/Controls.Sample.Sandbox/build-widget-extension.sh`

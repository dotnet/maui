# Project Configuration Templates

Replace: `{AppBundleId}`, `{GroupId}`, `{UrlScheme}`, `{ExtensionName}`

## .csproj Additions

See SKILL.md Step 4 for the complete MSBuild configuration including:
- `CodesignEntitlements` property
- `WidgetKit.WidgetCenterProxy` NuGet reference (optional)
- `Content` items for `.appex` files
- `AdditionalAppExtensions` for widget embedding
- `BuildWidgetExtension` target for auto-build via xcodebuild

## Entitlements

### Platforms/iOS/Entitlements.plist (app)

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.application-groups</key>
    <array>
        <string>{GroupId}</string>
    </array>
</dict>
</plist>
```

### Platforms/iOS/Entitlements.WidgetExtension.plist (widget)

Same content as above — same App Group ID.

**⚠️ MUST use LF line endings.** After creating:
```bash
sed -i '' 's/\r$//' Platforms/iOS/Entitlements.plist Platforms/iOS/Entitlements.WidgetExtension.plist
```

### XCodeWidget/{ExtensionName}.entitlements

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.security.application-groups</key>
    <array>
        <string>{GroupId}</string>
    </array>
</dict>
</plist>
```

## Info.plist URL Scheme

Add to existing `Platforms/iOS/Info.plist` for deep link handling:

```xml
<key>CFBundleURLTypes</key>
<array>
    <dict>
        <key>CFBundleURLName</key>
        <string>{AppBundleId}</string>
        <key>CFBundleURLSchemes</key>
        <array>
            <string>{UrlScheme}</string>
        </array>
    </dict>
</array>
```

## xcodegen project.yml

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
        PRODUCT_BUNDLE_IDENTIFIER: {AppBundleId}

  {ExtensionName}:
    type: app-extension
    platform: iOS
    sources:
      - {ExtensionName}
    settings:
      base:
        PRODUCT_BUNDLE_IDENTIFIER: {AppBundleId}.{ExtensionName}
        INFOPLIST_FILE: {ExtensionName}/Info.plist
        GENERATE_INFOPLIST_FILE: "NO"
        CODE_SIGN_ENTITLEMENTS: {ExtensionName}.entitlements
    entitlements:
      path: {ExtensionName}.entitlements
      properties:
        com.apple.security.application-groups:
          - "{GroupId}"
```

**Notes:**
- Widget bundle ID **must** be a child of app bundle ID
- `GENERATE_INFOPLIST_FILE: "NO"` because we provide our own Info.plist
- After creating: `cd XCodeWidget && xcodegen generate`

## XCodeWidget Host App (required by Xcode, not shipped)

### XCodeWidget/XCodeWidget/XCodeWidgetApp.swift

```swift
import SwiftUI

@main
struct XCodeWidgetApp: App {
    var body: some Scene {
        WindowGroup {
            ContentView()
        }
    }
}
```

### XCodeWidget/XCodeWidget/ContentView.swift

```swift
import SwiftUI

struct ContentView: View {
    var body: some View {
        Text("Widget Host App (not shipped)")
    }
}
```

## build-release.sh

```bash
#!/bin/bash
set -e
SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cd "$SCRIPT_DIR"

XCODEPROJ="XCodeWidget.xcodeproj"
TARGET="{ExtensionName}"

echo "📱 Building for device..."
xcodebuild -quiet -project "$XCODEPROJ" -target "$TARGET" \
    -configuration Release -sdk iphoneos \
    CODE_SIGN_IDENTITY="-" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO \
    BUILD_DIR=build clean build

echo "🖥  Building for simulator..."
xcodebuild -quiet -project "$XCODEPROJ" -target "$TARGET" \
    -configuration Release -sdk iphonesimulator -arch arm64 \
    CODE_SIGN_IDENTITY="-" CODE_SIGNING_REQUIRED=NO CODE_SIGNING_ALLOWED=NO \
    BUILD_DIR=build clean build

echo "✅ Built: build/Release-iphoneos/{ExtensionName}.appex"
echo "✅ Built: build/Release-iphonesimulator/{ExtensionName}.appex"
echo ""
echo "Copy to MAUI project:"
echo "  cp -R build/Release-iphoneos ../Platforms/iOS/WidgetExtensions/"
echo "  cp -R build/Release-iphonesimulator ../Platforms/iOS/WidgetExtensions/"
```

## Directory Structure After Setup

```
MyApp/
├── MyApp.csproj
├── Services/
│   ├── WidgetData.cs
│   ├── WidgetConstants.cs
│   ├── IWidgetDataService.cs
│   └── StubWidgetDataService.cs
├── Platforms/iOS/
│   ├── AppDelegate.cs
│   ├── Info.plist                              # With CFBundleURLTypes
│   ├── Entitlements.plist                      # App Group
│   ├── Entitlements.WidgetExtension.plist      # App Group (widget)
│   ├── WidgetDataService.cs                    # iOS implementation
│   └── WidgetExtensions/                       # Auto-populated by BuildWidgetExtension
│       ├── Release-iphoneos/
│       │   └── {ExtensionName}.appex/
│       └── Release-iphonesimulator/
│           └── {ExtensionName}.appex/
└── XCodeWidget/
    ├── project.yml                             # xcodegen spec
    ├── XCodeWidget/                            # Host app (not shipped)
    ├── {ExtensionName}/                        # Widget extension Swift code
    │   ├── Info.plist
    │   ├── Settings.swift
    │   ├── WidgetData.swift
    │   ├── SharedStorage.swift
    │   ├── SimpleEntry.swift
    │   ├── Provider.swift
    │   ├── SimpleWidgetView.swift
    │   ├── SimpleWidget.swift
    │   ├── SimpleWidgetBundle.swift
    │   └── Intents/                            # Optional
    ├── {ExtensionName}.entitlements
    └── build-release.sh
```

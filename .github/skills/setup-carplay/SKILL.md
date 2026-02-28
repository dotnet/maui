---
name: setup-carplay
description: Sets up a CarPlay companion app for a .NET MAUI iOS application. Configures Info.plist, entitlements, scene delegate, and CarPlay UI using CPTemplate APIs. Supports audio, communication, EV charging, parking, quick food ordering, and navigation (maps) app categories. Navigation apps get a CPWindow for rendering custom UIKit/MAUI content.
metadata:
  author: dotnet-maui
  version: "1.0"
compatibility: Requires .NET MAUI with iOS target, Xcode with iOS SDK, CarPlay simulator support.
---

# Setup CarPlay Companion App Skill

Configures a .NET MAUI iOS app to include a CarPlay companion experience. Guides the user through entitlement selection, Info.plist configuration, scene delegate implementation, and CarPlay UI creation using the appropriate CPTemplate APIs.

## Overview

CarPlay uses iOS's **scene architecture** — the same multi-window system MAUI already supports. A CarPlay app is just another scene in your app with:
- A dedicated scene delegate (subclass of `CPTemplateApplicationSceneDelegate`)  
- A template-based UI (`CPListTemplate`, `CPGridTemplate`, `CPTabBarTemplate`, etc.)
- CarPlay-specific entitlements

**MAUI provides built-in infrastructure** via `MauiCarPlaySceneDelegate` and lifecycle events, so developers can wire up CarPlay with minimal code.

## When to Use This Skill

- User wants to add CarPlay support to their MAUI iOS app
- User asks about CarPlay or in-car experiences
- User wants to display content on a car's head unit display

## CarPlay App Categories

Apple requires each CarPlay app to declare a specific category via entitlements. The category determines which CPTemplate types are available and whether you get a drawable `CPWindow`.

| Category | Entitlement Key | Root Templates | CPWindow? |
|----------|----------------|----------------|-----------|
| **Audio** | `com.apple.developer.carplay-audio` | CPListTemplate, CPTabBarTemplate, CPGridTemplate, CPNowPlayingTemplate | ❌ No |
| **Communication** | `com.apple.developer.carplay-communication` | CPListTemplate, CPTabBarTemplate, CPGridTemplate | ❌ No |
| **Navigation** | `com.apple.developer.carplay-maps` | CPMapTemplate | ✅ Yes |
| **EV Charging** | `com.apple.developer.carplay-charging` | CPListTemplate, CPTabBarTemplate, CPGridTemplate, CPPointOfInterestTemplate | ❌ No |
| **Parking** | `com.apple.developer.carplay-parking` | CPListTemplate, CPTabBarTemplate, CPGridTemplate, CPPointOfInterestTemplate | ❌ No |
| **Quick Food Ordering** | `com.apple.developer.carplay-quick-ordering` | CPListTemplate, CPTabBarTemplate, CPGridTemplate | ❌ No |

**Important**: Only **Navigation** apps receive a `CPWindow` for custom rendering. All other categories are strictly template-based.

## Step-by-Step Setup

### Step 1: Choose Category and Set Entitlements

Create or update `Platforms/iOS/Entitlements.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <!-- Replace with the appropriate entitlement for your app category -->
    <key>com.apple.developer.carplay-audio</key>
    <true/>
</dict>
</plist>
```

Add to your `.csproj`:

```xml
<PropertyGroup>
    <CodesignEntitlements Condition="$(TargetFramework.Contains('-ios'))">Platforms/iOS/Entitlements.plist</CodesignEntitlements>
</PropertyGroup>
```

### Step 2: Configure Info.plist

Add a CarPlay scene configuration to `Platforms/iOS/Info.plist` inside the existing `<dict>`:

```xml
<key>UIApplicationSceneManifest</key>
<dict>
    <key>UIApplicationSupportsMultipleScenes</key>
    <true/>
    <key>UISceneConfigurations</key>
    <dict>
        <key>UIWindowSceneSessionRoleApplication</key>
        <array>
            <dict>
                <key>UISceneConfigurationName</key>
                <string>__MAUI_DEFAULT_SCENE_CONFIGURATION__</string>
                <key>UISceneDelegateClassName</key>
                <string>MauiUISceneDelegate</string>
            </dict>
        </array>
        <key>CPTemplateApplicationSceneSessionRoleApplication</key>
        <array>
            <dict>
                <key>UISceneConfigurationName</key>
                <string>__MAUI_CARPLAY_SCENE_CONFIGURATION__</string>
                <key>UISceneDelegateClassName</key>
                <string>MauiCarPlaySceneDelegate</string>
            </dict>
        </array>
    </dict>
</dict>
```

**Critical**: `UIApplicationSupportsMultipleScenes` MUST be `true`. The `UISceneDelegateClassName` MUST match the `[Register]` name of your scene delegate class.

### Step 3: Implement CarPlay UI

#### Option A: Using MAUI Lifecycle Events (Simplest)

Use the built-in `MauiCarPlaySceneDelegate` and lifecycle events. No custom delegate class needed:

```csharp
// MauiProgram.cs
using Microsoft.Maui.LifecycleEvents;

public static MauiApp CreateMauiApp() =>
    MauiApp
        .CreateBuilder()
        .UseMauiApp<App>()
#if IOS && !MACCATALYST
#pragma warning disable CA1416
        .ConfigureLifecycleEvents(events =>
        {
            events.AddiOS(ios => ios
                .CarPlayDidConnect((scene, interfaceController) =>
                {
                    // Create your CarPlay UI using CPTemplate APIs
                    var item1 = new CarPlay.CPListItem("Now Playing", "Artist - Song Title");
                    var item2 = new CarPlay.CPListItem("Browse", "Explore your library");

                    // Handle item taps via the Handler property
                    item1.Handler = (item, completion) =>
                    {
                        // Handle tap, then call completion() to dismiss the spinner
                        completion();
                    };

                    var section = new CarPlay.CPListSection(
                        new CarPlay.ICPListTemplateItem[] { item1, item2 }, "Music", "");
                    var listTemplate = new CarPlay.CPListTemplate(
                        "My Audio App", new CarPlay.CPListSection[] { section });

                    interfaceController.SetRootTemplate(listTemplate, true, null);
                })
                .CarPlayDidDisconnect((scene, interfaceController) =>
                {
                    // Clean up CarPlay resources
                }));
        })
#pragma warning restore CA1416
#endif
        .Build();
```

#### Option B: Custom Scene Delegate (More Control)

For more complex CarPlay apps, subclass `MauiCarPlaySceneDelegate` or `CPTemplateApplicationSceneDelegate`:

```csharp
// Platforms/iOS/MyCarPlaySceneDelegate.cs
#if IOS && !MACCATALYST
using CarPlay;
using Foundation;
using UIKit;

namespace MyApp;

[Register("MyCarPlaySceneDelegate")]
public class MyCarPlaySceneDelegate : CPTemplateApplicationSceneDelegate
{
#pragma warning disable CA1416
    public override void DidConnect(
        CPTemplateApplicationScene scene,
        CPInterfaceController interfaceController)
    {
        // Build your template UI
        var item1 = new CPListItem("Item 1", "Detail 1");
        var item2 = new CPListItem("Item 2", "Detail 2");

        // Handle taps via the Handler property (modern pattern)
        item1.Handler = (item, completion) =>
        {
            // Do something when tapped, then dismiss spinner
            completion();
        };

        var section = new CPListSection(
            new ICPListTemplateItem[] { item1, item2 }, "Section", "");
        var template = new CPListTemplate(
            "My App", new[] { section });

        interfaceController.SetRootTemplate(template, true, null);
    }

    public override void DidDisconnect(
        CPTemplateApplicationScene scene,
        CPInterfaceController interfaceController)
    {
        // Clean up
    }
#pragma warning restore CA1416
}
#endif
```

Update `Info.plist` to use your custom delegate name:
```xml
<key>UISceneDelegateClassName</key>
<string>MyCarPlaySceneDelegate</string>
```

If using `MauiUIApplicationDelegate.GetConfiguration()` routing, override it in your AppDelegate:
```csharp
[Export("application:configurationForConnectingSceneSession:options:")]
public override UISceneConfiguration GetConfiguration(
    UIApplication application,
    UISceneSession connectingSceneSession,
    UISceneConnectionOptions options)
{
    if (connectingSceneSession.Role.GetConstant() ==
        new NSString("CPTemplateApplicationSceneSessionRoleApplication"))
    {
        var config = new UISceneConfiguration(
            "__MAUI_CARPLAY_SCENE_CONFIGURATION__",
            connectingSceneSession.Role);
        config.DelegateType = typeof(MyCarPlaySceneDelegate);
        return config;
    }
    return base.GetConfiguration(application, connectingSceneSession, options);
}
```

### Step 4 (Navigation Apps Only): Render MAUI XAML in CPWindow

**Only for apps with `com.apple.developer.carplay-maps` entitlement.**

Navigation apps receive a `CPWindow` — a `UIWindow` subclass you can use to render custom content, including MAUI XAML pages. This is how Apple Maps renders its map view behind the template chrome.

```csharp
// Platforms/iOS/NavCarPlaySceneDelegate.cs
#if IOS && !MACCATALYST
using System.Runtime.InteropServices;
using CarPlay;
using Foundation;
using Microsoft.Maui;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;

namespace MyApp;

[Register("NavCarPlaySceneDelegate")]
public class NavCarPlaySceneDelegate : CPTemplateApplicationSceneDelegate
{
    [DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
    static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

#pragma warning disable CA1416
    public override void DidConnect(
        CPTemplateApplicationScene scene,
        CPInterfaceController interfaceController,
        CPWindow window)
    {
        // CPMapTemplate requires ObjC runtime init (no default .NET constructor)
        var cls = Class.GetHandle("CPMapTemplate");
        var ptr = objc_msgSend_IntPtr(
            objc_msgSend_IntPtr(cls, Selector.GetHandle("alloc")),
            Selector.GetHandle("init"));
        var mapTemplate = Runtime.GetNSObject<CPMapTemplate>(ptr)!;

        // Auto-hide chrome to maximize content area
        mapTemplate.AutomaticallyHidesNavigationBar = true;
        mapTemplate.HidesButtonsWithNavigationBar = true;

        // Render MAUI page in the CPWindow
        var mauiApp = IPlatformApplication.Current;
        if (mauiApp?.Services != null)
        {
            var mauiContext = new MauiContext(mauiApp.Services);
            var page = new MyCarPlayPage(); // Your XAML ContentPage
            var handler = page.ToHandler(mauiContext);
            var platformView = handler.PlatformView!;

            var bounds = window.Bounds;
            platformView.Frame = bounds;
            platformView.AutoresizingMask =
                UIViewAutoresizing.FlexibleWidth |
                UIViewAutoresizing.FlexibleHeight;

            var vc = new UIViewController();
            vc.View!.Frame = bounds;
            vc.View.AddSubview(platformView);
            window.RootViewController = vc;

            // Set template after window content
            interfaceController.SetRootTemplate(mapTemplate, false, null);

            // Force MAUI layout
            NSTimer.CreateScheduledTimer(0.1, _ =>
            {
                platformView.Frame = bounds;
                (page as IView).Arrange(
                    new Microsoft.Maui.Graphics.Rect(
                        0, 0, bounds.Width, bounds.Height));
                platformView.SetNeedsLayout();
                platformView.LayoutIfNeeded();
            });
        }
    }

    // Fallback for non-navigation entitlements
    public override void DidConnect(
        CPTemplateApplicationScene scene,
        CPInterfaceController interfaceController)
    {
        var item = new CPListItem("Navigation", "No CPWindow available");
        var section = new CPListSection(
            new ICPListTemplateItem[] { item }, "", "");
        interfaceController.SetRootTemplate(
            new CPListTemplate("My App", new[] { section }), true, null);
    }

    public override void DidDisconnect(
        CPTemplateApplicationScene scene,
        CPInterfaceController interfaceController,
        CPWindow window) { }

    public override void DidDisconnect(
        CPTemplateApplicationScene scene,
        CPInterfaceController interfaceController) { }
#pragma warning restore CA1416
}
#endif
```

Example XAML page for CarPlay (keep it simple — CarPlay display is ~800×480):

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.MyCarPlayPage"
             BackgroundColor="#1a1a2e">
    <VerticalStackLayout VerticalOptions="Center"
                         HorizontalOptions="Center" Spacing="10">
        <Label Text="Navigation"
               FontSize="48" TextColor="White"
               HorizontalOptions="Center" FontAttributes="Bold" />
        <Label Text="MAUI on CarPlay"
               FontSize="24" TextColor="#00d4ff"
               HorizontalOptions="Center" />
    </VerticalStackLayout>
</ContentPage>
```

**Important**: The MAUI view renders BEHIND the CPMapTemplate chrome. The template overlay auto-hides when the user taps the screen, revealing your full content. This is the same behavior as Apple Maps.

**⚠️ App Store Note**: Apple only allows navigation apps to use CPWindow for map rendering. Using it for arbitrary UI will be rejected in App Store review. This approach is valid for actual navigation/map content.

## Available CPTemplate Types

| Template | Description | Use Case |
|----------|-------------|----------|
| `CPListTemplate` | Scrollable list with sections | Playlists, contacts, stations |
| `CPGridTemplate` | Grid of buttons with icons | Quick actions, categories |
| `CPTabBarTemplate` | Tab bar with child templates | Multi-section apps |
| `CPNowPlayingTemplate` | Media playback controls | Audio apps |
| `CPMapTemplate` | Navigation map (needs CPWindow) | Navigation apps |
| `CPPointOfInterestTemplate` | POI map with annotations | EV charging, parking |
| `CPAlertTemplate` | Alert dialog | Confirmations |
| `CPActionSheetTemplate` | Action sheet | Option selection |
| `CPVoiceControlTemplate` | Voice input UI | Voice commands |

### Root Template Restrictions (iOS 18+)

Not all templates can be used as the **root** template. Allowed root templates:
- `CPListTemplate` ✅
- `CPGridTemplate` ✅  
- `CPTabBarTemplate` ✅
- `CPNowPlayingTemplate` ✅
- `CPMapTemplate` ✅ (navigation only)
- `CPAlertTemplate` ✅
- `CPActionSheetTemplate` ✅
- `CPVoiceControlTemplate` ✅
- `CPInformationTemplate` ❌ **NOT allowed as root** (crashes with NSInvalidArgumentException)

## Testing on Simulator

### Prerequisites

1. **Apple CarPlay Developer Program**: For real devices, you must enroll at [developer.apple.com/carplay](https://developer.apple.com/carplay)
2. **Simulator**: CarPlay works in the iOS Simulator without program enrollment

### Enabling CarPlay in Simulator

1. Boot an iOS simulator (iOS 18.5 recommended — iOS 26.1 has a known CarPlayApp crash)
2. In Simulator menu: **I/O → External Displays → CarPlay**
3. A CarPlay window (800×480) appears

### Simulator Entitlement Behavior

The iOS SDK embeds entitlements in the binary's `__TEXT,__entitlements` section for simulator builds. The CarPlay framework reads these embedded entitlements to determine your app's CarPlay category. Code signature entitlements are stripped for simulator builds (this is normal).

To verify entitlements are embedded:
```bash
# Extract embedded entitlements from built binary
segedit -extract __TEXT __entitlements /tmp/ent.xml \
  "artifacts/bin/MyApp/Debug/net10.0-ios/iossimulator-arm64/MyApp.app/MyApp"
cat /tmp/ent.xml
```

### Registering App in CarPlay (Simulator)

If your app icon doesn't appear on the CarPlay home screen:
```bash
UDID="<your-simulator-udid>"
BUNDLE_ID="com.yourcompany.yourapp"

# Register in CarPlay allowed apps
xcrun simctl spawn $UDID defaults write com.apple.CarPlayApp AllowedApps -array "$BUNDLE_ID"
xcrun simctl spawn $UDID defaults write com.apple.CarPlaySettings CARSupportedApps -array "$BUNDLE_ID"

# Toggle CarPlay off/on to rescan (via Simulator > I/O > External Displays > CarPlay)
```

### Taking CarPlay Screenshots

```bash
xcrun simctl io $UDID screenshot --display=external carplay_screenshot.png
```

### Known Simulator Issues

| iOS Version | Issue | Workaround |
|-------------|-------|------------|
| iOS 26.1 | CarPlayApp crashes (`UIScreen focus system`) | Use iOS 26.0 or 18.5 |
| iOS 26.0 | Some devices unstable | Use iPhone 17 Pro or iPhone 11 |
| iOS 18.5 | Stable ✅ | Recommended for testing |

## CPMapTemplate Constructor Issue

`CPMapTemplate` has **no default constructor** in the .NET iOS bindings (only `NSCoder`, `NSObjectFlag`, `NativeHandle`). You must use ObjC runtime directly:

```csharp
[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
static extern IntPtr objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector);

// Create CPMapTemplate via ObjC runtime
var cls = Class.GetHandle("CPMapTemplate");
var ptr = objc_msgSend_IntPtr(
    objc_msgSend_IntPtr(cls, Selector.GetHandle("alloc")),
    Selector.GetHandle("init"));
var mapTemplate = Runtime.GetNSObject<CPMapTemplate>(ptr)!;
```

## MAUI Framework CarPlay API Reference

### MauiCarPlaySceneDelegate

Base class for CarPlay scene delegates. Implements `ICPTemplateApplicationSceneDelegate`.

| Member | Description |
|--------|-------------|
| `InterfaceController` | The `CPInterfaceController` for managing template stack |
| `CarPlayWindow` | The `CPWindow` for custom rendering (navigation apps only) |
| `DidConnect(scene, controller)` | Called when CarPlay connects (template apps) |
| `DidConnect(scene, controller, window)` | Called when CarPlay connects (navigation apps — provides drawable window) |
| `DidDisconnect(scene, controller)` | Called when CarPlay disconnects |
| `DidSelectManeuver(scene, maneuver)` | Called when user selects a navigation maneuver |
| `DidSelectNavigationAlert(scene, alert)` | Called when user selects a navigation alert |

### Lifecycle Events

Register handlers via `ConfigureLifecycleEvents`:

| Event | Parameters | Description |
|-------|------------|-------------|
| `CarPlayDidConnect` | `(CPTemplateApplicationScene, CPInterfaceController)` | Scene connected |
| `CarPlayDidDisconnect` | `(CPTemplateApplicationScene, CPInterfaceController)` | Scene disconnected |
| `CarPlayDidSelectManeuver` | `(CPTemplateApplicationScene, CPManeuver)` | Maneuver selected |
| `CarPlayDidSelectNavigationAlert` | `(CPTemplateApplicationScene, CPNavigationAlert)` | Alert selected |

## Debugging

### NSLog for CarPlay (Console.WriteLine doesn't appear in os_log)

```csharp
[DllImport(ObjCRuntime.Constants.FoundationLibrary)]
static extern void NSLog(IntPtr format);

static void Log(string message)
{
    using var str = new NSString($"[CarPlay] {message}");
    NSLog(str.Handle);
}
```

### Reading CarPlay Logs

```bash
# Stream logs filtered to your app's CarPlay output
xcrun simctl spawn $UDID log stream \
  --predicate 'eventMessage contains "CarPlay"' --timeout 30

# Show recent logs
xcrun simctl spawn $UDID log show --last 30s \
  --predicate 'eventMessage contains "CarPlay"'
```

## Common Pitfalls

| Pitfall | Details |
|---------|---------|
| Using `#if IOS` instead of `#if IOS && !MACCATALYST` | CarPlay APIs don't exist on MacCatalyst. Using `#if IOS` alone will cause build failures on MacCatalyst since that condition is true for both iOS and MacCatalyst. |
| Forgetting to call `completion()` in `CPListItem.Handler` | The CarPlay UI shows a spinner when an item is tapped. You **must** call `completion()` to dismiss it, or the spinner stays visible indefinitely. |
| Using `Console.WriteLine` for CarPlay logging | `Console.WriteLine` doesn't appear in os_log for CarPlay scenes. Use the NSLog P/Invoke helper shown in the Debugging section above. |
| Setting `CPInformationTemplate` as root template | Crashes with `NSInvalidArgumentException` on iOS 18+. Only templates listed in the Root Template Restrictions table are allowed. |

## Complete Working Example (Audio App)

### Entitlements.plist
```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>com.apple.developer.carplay-audio</key>
    <true/>
</dict>
</plist>
```

### MauiProgram.cs
```csharp
using Microsoft.Maui.LifecycleEvents;

namespace MyAudioApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp() =>
        MauiApp
            .CreateBuilder()
            .UseMauiApp<App>()
#if IOS && !MACCATALYST
#pragma warning disable CA1416
            .ConfigureLifecycleEvents(events =>
            {
                events.AddiOS(ios => ios
                    .CarPlayDidConnect((scene, interfaceController) =>
                    {
                        var songs = new CarPlay.ICPListTemplateItem[]
                        {
                            CreateSong("Bohemian Rhapsody", "Queen"),
                            CreateSong("Hotel California", "Eagles"),
                            CreateSong("Stairway to Heaven", "Led Zeppelin"),
                        };

                        var section = new CarPlay.CPListSection(songs, "Favorites", "");

                        var template = new CarPlay.CPListTemplate(
                            "My Music", new[] { section });

                        interfaceController.SetRootTemplate(template, true, null);
                    })
                    .CarPlayDidDisconnect((scene, controller) =>
                    {
                        // Clean up CarPlay resources
                    }));
            })
#pragma warning restore CA1416
#endif
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            })
            .Build();

#if IOS && !MACCATALYST
#pragma warning disable CA1416
    static CarPlay.CPListItem CreateSong(string title, string artist)
    {
        var item = new CarPlay.CPListItem(title, artist);
        item.Handler = (listItem, completion) =>
        {
            System.Diagnostics.Debug.WriteLine($"[CarPlay] Now playing: {title} by {artist}");
            completion(); // Must call to dismiss the spinner
        };
        return item;
    }
#pragma warning restore CA1416
#endif
}
```

### .csproj addition
```xml
<CodesignEntitlements Condition="$(TargetFramework.Contains('-ios'))">Platforms/iOS/Entitlements.plist</CodesignEntitlements>
```

### Info.plist scene manifest
Add inside the root `<dict>`:
```xml
<key>UIApplicationSceneManifest</key>
<dict>
    <key>UIApplicationSupportsMultipleScenes</key>
    <true/>
    <key>UISceneConfigurations</key>
    <dict>
        <key>UIWindowSceneSessionRoleApplication</key>
        <array>
            <dict>
                <key>UISceneConfigurationName</key>
                <string>__MAUI_DEFAULT_SCENE_CONFIGURATION__</string>
                <key>UISceneDelegateClassName</key>
                <string>MauiUISceneDelegate</string>
            </dict>
        </array>
        <key>CPTemplateApplicationSceneSessionRoleApplication</key>
        <array>
            <dict>
                <key>UISceneConfigurationName</key>
                <string>__MAUI_CARPLAY_SCENE_CONFIGURATION__</string>
                <key>UISceneDelegateClassName</key>
                <string>MauiCarPlaySceneDelegate</string>
            </dict>
        </array>
    </dict>
</dict>
```

That's it — build, deploy to simulator, enable CarPlay from I/O menu, and your app appears on the CarPlay home screen.

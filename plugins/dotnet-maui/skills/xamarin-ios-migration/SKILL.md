---
name: xamarin-ios-migration
description: >
  **WORKFLOW SKILL** - Guide for migrating Xamarin.iOS, Xamarin.Mac, and Xamarin.tvOS native apps to
  .NET for iOS, .NET for macOS, and .NET for tvOS. Covers SDK-style project
  conversion, target framework monikers, MSBuild property changes, Info.plist
  updates, iOS binding library migration, Xamarin.Essentials in native apps,
  NuGet dependency compatibility, and code signing changes.
  USE FOR: "migrate Xamarin.iOS", "upgrade Xamarin.iOS to .NET",
  "Xamarin.iOS to .NET for iOS", "iOS project migration", "Xamarin.Mac migration",
  "tvOS migration", "iOS binding library migration", "MtouchArch to RuntimeIdentifier",
  "Apple project migration".
  DO NOT USE FOR: migrating Xamarin.Forms apps (use xamarin-forms-migration),
  migrating Xamarin.Android apps (use xamarin-android-migration),
  creating new MAUI apps from scratch (use feature-specific MAUI skills).
---

# Xamarin.iOS / Xamarin.Mac / Xamarin.tvOS → .NET Migration

Use this skill when migrating Xamarin.iOS, Xamarin.Mac, or Xamarin.tvOS native
apps (not Xamarin.Forms) to .NET for iOS, .NET for macOS, or .NET for tvOS.

## Migration Workflow Overview

1. Create a new .NET for iOS/macOS/tvOS project
2. Convert project file to SDK-style format
3. Update MSBuild properties (MtouchArch, HttpClientHandler, code signing)
4. Migrate Info.plist values to csproj
5. Copy code and resources
6. Update NuGet dependencies
7. Migrate binding libraries (if applicable)
8. Set up Xamarin.Essentials replacement (if applicable)
9. Handle configuration file removal
10. Build, verify code signing, test on device

---

## Migration Strategy

Create a new .NET project of the same type and name, then copy code into it.
This is simpler than converting the existing project file in place.

---

## Step 1 — SDK-Style Project File

### .NET for iOS

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-ios</TargetFramework>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <SupportedOSPlatformVersion>13.0</SupportedOSPlatformVersion>
  </PropertyGroup>
</Project>
```

### .NET for macOS (Mac Catalyst)

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-macos</TargetFramework>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <SupportedOSPlatformVersion>10.15</SupportedOSPlatformVersion>
  </PropertyGroup>
</Project>
```

### .NET for tvOS

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-tvos</TargetFramework>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>true</ImplicitUsings>
    <SupportedOSPlatformVersion>13.0</SupportedOSPlatformVersion>
  </PropertyGroup>
</Project>
```

For library projects, omit `<OutputType>` or set to `Library`.

> Replace `net8.0` with `net9.0` or `net10.0` as appropriate. Valid TFMs:
> `net8.0-ios`, `net8.0-macos`, `net8.0-tvos`.

---

## Step 2 — MSBuild Property Changes

| Xamarin Property | .NET Equivalent | Action |
|-----------------|-----------------|--------|
| `MtouchArch` / `XamMacArch` | `RuntimeIdentifier` / `RuntimeIdentifiers` | Convert (see table below) |
| `HttpClientHandler` / `MtouchHttpClientHandler` | `UseNativeHttpHandler` | Convert (see table below) |
| `MtouchExtraArgs` | *(some still apply)* | Copy and test |
| `EnableCodeSigning` | *(unchanged)* | Copy |
| `CodeSigningKey` | `CodesignKey` | Rename |
| `CodesignKey` | *(unchanged)* | Copy |
| `CodesignProvision` | *(unchanged)* | Copy |
| `CodesignEntitlements` | *(unchanged)* | Copy |
| `CodesignExtraArgs` | *(unchanged)* | Copy |
| `PackageSigningKey` | *(unchanged)* | Copy |
| `PackagingExtraArgs` | *(unchanged)* | Copy |
| `ProductDefinition` | *(unchanged)* | Copy |
| `MtouchEnableSGenConc` | `EnableSGenConc` | Rename |
| `LinkDescription` | *(unchanged)* | Copy |

### MtouchArch → RuntimeIdentifier (iOS)

| MtouchArch Value | RuntimeIdentifier | RuntimeIdentifiers |
|-----------------|-------------------|-------------------|
| `ARMv7` | `ios-arm` | |
| `ARMv7s` | `ios-arm` | |
| `ARMv7+ARMv7s` | `ios-arm` | |
| `ARM64` | `ios-arm64` | |
| `ARMv7+ARM64` | | `ios-arm;ios-arm64` |
| `ARMv7+ARMv7s+ARM64` | | `ios-arm;ios-arm64` |
| `x86_64` | `iossimulator-x64` | |
| `i386` | `iossimulator-x86` | |
| `x86_64+i386` | | `iossimulator-x86;iossimulator-x64` |

> Use `RuntimeIdentifiers` (plural) when targeting multiple architectures.

### MtouchArch → RuntimeIdentifier (macOS)

| Value | RuntimeIdentifier |
|-------|-------------------|
| `x86_64` | `osx-x64` |

### MtouchArch → RuntimeIdentifier (tvOS)

| Value | RuntimeIdentifier |
|-------|-------------------|
| `ARM64` | `tvos-arm64` |
| `x86_64` | `tvossimulator-x64` |

### HttpClientHandler Conversion

| Xamarin Value | `UseNativeHttpHandler` |
|--------------|----------------------|
| `HttpClientHandler` | `false` |
| `NSUrlSessionHandler` | *(don't set — default)* |
| `CFNetworkHandler` | *(don't set — default)* |

---

## Step 3 — Info.plist Changes

### MinimumOSVersion / LSMinimumSystemVersion

Move from Info.plist to the project file:

```xml
<!-- BEFORE (Info.plist) -->
<key>MinimumOSVersion</key>
<string>13.0</string>

<!-- AFTER (csproj) -->
<PropertyGroup>
    <SupportedOSPlatformVersion>13.0</SupportedOSPlatformVersion>
</PropertyGroup>
```

Other Info.plist entries (display name, bundle identifier, permissions, etc.)
remain in Info.plist.

---

## Step 4 — Copy Code and Resources

1. Copy source files, storyboards, XIBs, assets, and other resources from the
   Xamarin project to the new project.
2. Copy project properties (code signing, entitlements, linker settings) by
   comparing project files side-by-side.
3. Copy or merge any linker XML files (`LinkDescription` items).

---

## Step 5 — Update NuGet Dependencies

| Compatible Frameworks | Incompatible Frameworks |
|----------------------|------------------------|
| `net8.0-ios` | `monotouch`, `xamarinios`, `xamarinios10` |
| `net8.0-macos` | `monomac`, `xamarinmac`, `xamarinmac20` |
| `net8.0-tvos` | `xamarintvos` |

> Unlike Android, there is **no backward compatibility** with old Xamarin iOS/Mac
> TFMs. Packages must be recompiled for `net8.0-ios` etc.

.NET Standard libraries without incompatible dependencies remain compatible.

If no compatible version exists:
1. Recompile with .NET TFMs (if you own it)
2. Look for a preview .NET version
3. Replace with a .NET-compatible alternative

---

## Step 6 — iOS Binding Library Migration

For binding libraries wrapping Objective-C/Swift frameworks:

- Use SDK-style project format with `net8.0-ios` TFM
- The binding generator and API definitions work the same way
- Verify that native frameworks are updated for the target iOS version
- Test thoroughly — binding edge cases are common

---

## Step 7 — Xamarin.Essentials in Native Apps

If your Xamarin.iOS app used Xamarin.Essentials:

1. Remove the `Xamarin.Essentials` NuGet package
2. Add `<UseMauiEssentials>true</UseMauiEssentials>` to your project file
3. Initialize in your AppDelegate:

```csharp
using Microsoft.Maui.ApplicationModel;

[Register("AppDelegate")]
public class AppDelegate : UIApplicationDelegate
{
    public override UIWindow? Window { get; set; }

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        Window = new UIWindow(UIScreen.MainScreen.Bounds);
        var vc = new UIViewController();
        Window.RootViewController = vc;

        Platform.Init(() => vc);

        Window.MakeKeyAndVisible();
        return true;
    }
}
```

4. Update `using` directives:

| Xamarin.Essentials | .NET MAUI Namespace |
|-------------------|---------------------|
| App actions, permissions, version tracking | `Microsoft.Maui.ApplicationModel` |
| Contacts, email, networking | `Microsoft.Maui.ApplicationModel.Communication` |
| Battery, sensors, flashlight, haptics | `Microsoft.Maui.Devices` |
| Media picking, text-to-speech | `Microsoft.Maui.Media` |
| Clipboard, file sharing | `Microsoft.Maui.ApplicationModel.DataTransfer` |
| File picking, secure storage, preferences | `Microsoft.Maui.Storage` |

5. For app actions, override `PerformActionForShortcutItem`:

```csharp
public override void PerformActionForShortcutItem(
    UIApplication application,
    UIApplicationShortcutItem shortcutItem,
    UIOperationHandler completionHandler)
{
    Platform.PerformActionForShortcutItem(application, shortcutItem, completionHandler);
}
```

---

## Step 8 — Configuration Files

There is **no support** for `.dll.config` or `.exe.config` files in .NET for iOS/macOS.
`<dllmap>` elements are not supported in .NET Core. Migrate configuration to
`appsettings.json`, embedded resources, or platform preferences.

---

## Step 9 — watchOS

> **Xamarin.watchOS is not supported in .NET.** The recommendation is to bundle
> Swift extensions with .NET for iOS apps instead.

---

## Step 10 — Unsupported Projects

| Project Type | Status |
|-------------|--------|
| Xamarin.iOS | ✅ Supported |
| Xamarin.Mac | ✅ Supported |
| Xamarin.tvOS | ✅ Supported |
| iOS App Extensions | ✅ Supported |
| SpriteKit / SceneKit / Metal | ✅ Supported |
| Xamarin.watchOS | ❌ Not supported |
| OpenGL (iOS) | ❌ Not supported (OpenTK unavailable) |

---

## Build and Troubleshoot

1. Delete all `bin/` and `obj/` folders
2. Build and fix compiler errors iteratively
3. Verify code signing settings match your provisioning profiles
4. Test on physical devices (especially for App Store builds)

Common issues:
- **Namespace not found**: Replace `Xamarin.iOS`/`UIKit` imports — most UIKit
  namespaces remain the same, but verify against the .NET for iOS API surface.
- **Linker errors**: Update `LinkDescription` XML files if custom linker
  configuration was used. The linker behavior is similar but stricter in .NET.
- **Entitlements**: Ensure `CodesignEntitlements` points to the correct file.

---

## API Currency Note

If your migrated app will also adopt .NET MAUI controls (e.g., via `UseMaui`),
check the **maui-current-apis** skill for deprecated MAUI APIs to avoid
(ListView, Frame, Device.*, etc.).

---

## Quick Checklist

1. ☐ Created new .NET for iOS/macOS/tvOS project
2. ☐ Set `TargetFramework` to `net8.0-ios` (or `net8.0-macos`/`net8.0-tvos`)
3. ☐ Set `SupportedOSPlatformVersion` (moved from Info.plist)
4. ☐ Converted `MtouchArch` → `RuntimeIdentifier(s)`
5. ☐ Converted `HttpClientHandler` → `UseNativeHttpHandler`
6. ☐ Renamed `CodeSigningKey` → `CodesignKey` (if applicable)
7. ☐ Renamed `MtouchEnableSGenConc` → `EnableSGenConc` (if applicable)
8. ☐ Copied source, resources, storyboards, entitlements
9. ☐ Updated NuGet dependencies for `net8.0-ios` compatibility
10. ☐ Added `UseMauiEssentials` if using Essentials
11. ☐ Removed `.dll.config` files (not supported)
12. ☐ Deleted `bin/` and `obj/` folders
13. ☐ Verified code signing and provisioning
14. ☐ Tested on physical device

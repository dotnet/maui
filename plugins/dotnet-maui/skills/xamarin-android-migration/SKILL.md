---
name: xamarin-android-migration
description: >
  **WORKFLOW SKILL** - Guide for migrating Xamarin.Android native apps to .NET for Android. Covers
  SDK-style project conversion, target framework monikers, MSBuild property changes,
  AndroidManifest.xml updates, NuGet dependency compatibility, Android binding library
  migration, Xamarin.Essentials in native apps, .NET CLI support, and platform-specific
  gotchas.
  USE FOR: "migrate Xamarin.Android", "upgrade Xamarin.Android to .NET",
  "Xamarin.Android to .NET for Android", "Android project migration",
  "Android binding library migration", "convert Android project to SDK-style",
  "AndroidSupportedAbis to RuntimeIdentifiers".
  DO NOT USE FOR: migrating Xamarin.Forms apps (use xamarin-forms-migration),
  migrating Xamarin.iOS apps (use xamarin-ios-migration),
  creating new MAUI apps from scratch (use feature-specific MAUI skills).
---

# Xamarin.Android → .NET for Android Migration

Use this skill when migrating a Xamarin.Android native app (not Xamarin.Forms)
to .NET for Android.

> **Field-tested advice:** Android migration is significantly harder than iOS.
> Expect more UI bugs, OEM-specific rendering differences, and issues not
> reproducible on emulators. Test on physical devices.

## Migration Workflow Overview

1. Create a new .NET for Android project
2. Convert project file to SDK-style format
3. Update MSBuild properties (ABIs, AOT, etc.)
4. Update AndroidManifest.xml
5. Copy code and resources
6. Update NuGet dependencies
7. Migrate binding libraries (if applicable)
8. Set up Xamarin.Essentials replacement (if applicable)
9. Handle encoding changes
10. Build, test on physical devices

---

## Migration Strategy

Create a new .NET for Android project with the same name, then copy code into it.
This is simpler than editing the existing project file.

```shell
dotnet new android --output MyAndroidApp --packageName com.mycompany.myandroidapp
```

---

## Step 1 — SDK-Style Project File

A .NET for Android project uses the SDK-style format:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0-android</TargetFramework>
    <SupportedOSPlatformVersion>21</SupportedOSPlatformVersion>
    <OutputType>Exe</OutputType>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <ApplicationId>com.companyname.myapp</ApplicationId>
    <ApplicationVersion>1</ApplicationVersion>
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
  </PropertyGroup>
</Project>
```

For library projects, omit `<OutputType>` or set it to `Library`.

> Replace `net8.0-android` with `net9.0-android` or `net10.0-android` as needed.
> The TFM denotes the project as .NET (e.g., `net8.0-android` maps to Android API 34).

---

## Step 2 — MSBuild Property Changes

| Xamarin.Android Property | .NET for Android Equivalent | Notes |
|-------------------------|---------------------------|-------|
| `AndroidSupportedAbis` | `RuntimeIdentifiers` | See conversion table below |
| `AotAssemblies` | `RunAOTCompilation` | Deprecated in .NET 7 |
| `AndroidClassParser` | *(default: `class-parse`)* | `jar2xml` not supported |
| `AndroidDexTool` | *(default: `d8`)* | `dx` not supported |
| `AndroidCodegenTarget` | *(default: `XAJavaInterop1`)* | `XamarinAndroid` not supported |
| `AndroidManifest` | *(default: `AndroidManifest.xml` in root)* | No longer in `Properties/` |
| `DebugType` | *(default: `portable`)* | `full` and `pdbonly` not supported |
| `MonoSymbolArchive` | *(removed)* | `mono-symbolicate` not supported |
| `MAndroidI18n` | `System.Text.Encoding.CodePages` NuGet | See encoding section |
| `AndroidUseIntermediateDesignerFile` | *(default: `True`)* | |
| `AndroidBoundExceptionType` | *(default: `System`)* | Aligns with .NET semantics |

### ABI → RuntimeIdentifier Conversion

| `AndroidSupportedAbis` | `RuntimeIdentifiers` |
|------------------------|---------------------|
| `armeabi-v7a` | `android-arm` |
| `arm64-v8a` | `android-arm64` |
| `x86` | `android-x86` |
| `x86_64` | `android-x64` |

```xml
<!-- Xamarin.Android -->
<AndroidSupportedAbis>armeabi-v7a;arm64-v8a;x86;x86_64</AndroidSupportedAbis>

<!-- .NET for Android -->
<RuntimeIdentifiers>android-arm;android-arm64;android-x86;android-x64</RuntimeIdentifiers>
```

---

## Step 3 — AndroidManifest.xml Changes

Remove `<uses-sdk>` from AndroidManifest.xml. Use MSBuild properties instead:

```xml
<!-- BEFORE (Xamarin.Android AndroidManifest.xml) -->
<uses-sdk android:minSdkVersion="21" android:targetSdkVersion="33" />

<!-- AFTER (.NET for Android csproj) -->
<PropertyGroup>
    <TargetFramework>net8.0-android</TargetFramework>  <!-- targetSdkVersion -->
    <SupportedOSPlatformVersion>21</SupportedOSPlatformVersion>  <!-- minSdkVersion -->
</PropertyGroup>
```

`TargetFramework` maps to `android:targetSdkVersion` — it's set automatically
at build time from the TFM (e.g., `net8.0-android34.0` → API 34).

---

## Step 4 — Copy Code and Resources

1. Copy source files, layouts, drawables, and other resources from the Xamarin
   project to the new project (same folder structure).
2. Copy project properties (conditional compilation, code signing) by comparing
   project files side-by-side.
3. **Delete `Resource.designer.cs`** — it will be regenerated.
4. Delete all `bin/` and `obj/` folders.

---

## Step 5 — Update NuGet Dependencies

| Compatible Frameworks | Incompatible |
|----------------------|-------------|
| `net8.0-android` | |
| `monoandroid` | |
| `monoandroidXX.X` | |

> Android is unique: NuGet packages targeting `monoandroid` still work on .NET for Android.
> .NET Standard libraries without incompatible dependencies are also compatible.

If no compatible version exists:
1. Recompile with .NET TFMs (if you own it)
2. Look for a preview .NET version
3. Replace with a .NET-compatible alternative

---

## Step 6 — Android Binding Library Migration

For binding libraries, create a new project and copy bindings:

```shell
dotnet new android-bindinglib --output MyJavaBinding
```

Key changes:
- Use SDK-style project format
- `@(InputJar)`, `@(EmbeddedJar)`, or `@(LibraryProjectZip)` auto-enable
  `$(AllowUnsafeBlocks)`
- `AndroidClassParser` defaults to `class-parse` (no `jar2xml`)

---

## Step 7 — Xamarin.Essentials in Native Apps

If your Xamarin.Android app used Xamarin.Essentials, the functionality is now
part of .NET MAUI's platform integration layer:

1. Remove the `Xamarin.Essentials` NuGet package
2. Add `<UseMauiEssentials>true</UseMauiEssentials>` to your project file
3. Initialize in your Activity:

```csharp
using Microsoft.Maui.ApplicationModel;

[Activity(Label = "@string/app_name", MainLauncher = true)]
public class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        Platform.Init(this, savedInstanceState);
    }
}
```

4. Update `using` directives (see namespace table):

| Xamarin.Essentials | .NET MAUI Namespace |
|-------------------|---------------------|
| App actions, permissions, version tracking | `Microsoft.Maui.ApplicationModel` |
| Contacts, email, networking | `Microsoft.Maui.ApplicationModel.Communication` |
| Battery, sensors, flashlight, haptics | `Microsoft.Maui.Devices` |
| Media picking, text-to-speech | `Microsoft.Maui.Media` |
| Clipboard, file sharing | `Microsoft.Maui.ApplicationModel.DataTransfer` |
| File picking, secure storage, preferences | `Microsoft.Maui.Storage` |

5. Override `OnRequestPermissionsResult` in every Activity:

```csharp
public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
{
    Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
    base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
}
```

---

## Step 8 — Encoding Changes

Replace `MAndroidI18n` with the `System.Text.Encoding.CodePages` NuGet package:

```csharp
// At app startup
System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
```

---

## Step 9 — AOT Compilation

Release builds default to profiled AOT:

```xml
<PropertyGroup Condition="'$(Configuration)' == 'Release'">
    <RunAOTCompilation>true</RunAOTCompilation>
    <AndroidEnableProfiledAot>true</AndroidEnableProfiledAot>
</PropertyGroup>
```

To disable AOT, explicitly set both to `false`.

---

## Step 10 — Configuration Files

There is **no support** for `.dll.config` or `.exe.config` files in .NET for Android.
`<dllmap>` configuration elements are not supported in .NET Core.

If your app uses `System.Configuration.ConfigurationManager`, note it has never been
supported on Android. Migrate to `appsettings.json` or platform preferences.

---

## .NET CLI Support

| Command | Description |
|---------|-------------|
| `dotnet new android` | Create new app |
| `dotnet new androidlib` | Create class library |
| `dotnet new android-bindinglib` | Create binding library |
| `dotnet new android-activity --name LoginActivity` | Add activity |
| `dotnet new android-layout --name MyLayout --output Resources/layout` | Add layout |
| `dotnet build` | Build (produces `.apk`/`.aab`) |
| `dotnet run --project MyApp.csproj` | Deploy and run on device/emulator |
| `dotnet publish` | Publish for distribution |

> **Note:** `dotnet build` produces a runnable `.apk`/`.aab` directly (unlike desktop
> .NET where `publish` is typically needed). Inside IDEs, the `Install` target handles
> deployment instead.

---

## Build and Troubleshoot

1. Delete all `bin/` and `obj/` folders
2. Delete `Resource.designer.cs`
3. Build and fix compiler errors iteratively
4. Check `android:versionCode` and `android:versionName` — these can now live in
   the csproj as `ApplicationVersion` and `ApplicationDisplayVersion`

---

## Platform-Specific Gotchas

- **OEM rendering differences**: Android OEMs customize rendering in ways not
  reproducible on emulators. Always test on physical devices from multiple vendors.
- **Shadow rendering**: Varies across OEMs and API levels. Implement shadows in
  platform-specific handler code rather than relying on the cross-platform `Shadow`
  property alone.
- **Android Wear**: Referencing an Android Wear project from an Android app is
  not supported in .NET for Android.

---

## API Currency Note

If your migrated app will also adopt .NET MAUI controls (e.g., via `UseMaui`),
check the **maui-current-apis** skill for deprecated MAUI APIs to avoid
(ListView, Frame, Device.*, etc.).

---

## Quick Checklist

1. ☐ Created new .NET for Android project (`dotnet new android`)
2. ☐ Set `TargetFramework` to `net8.0-android` (or later)
3. ☐ Set `SupportedOSPlatformVersion` for minimum SDK
4. ☐ Converted `AndroidSupportedAbis` → `RuntimeIdentifiers`
5. ☐ Removed `<uses-sdk>` from AndroidManifest.xml
6. ☐ Copied source, resources, and project properties
7. ☐ Deleted `Resource.designer.cs`, `bin/`, `obj/`
8. ☐ Updated NuGet dependencies
9. ☐ Added `UseMauiEssentials` if using Essentials
10. ☐ Replaced `MAndroidI18n` with `System.Text.Encoding.CodePages`
11. ☐ Verified AOT settings for Release builds
12. ☐ Tested on physical Android device(s)

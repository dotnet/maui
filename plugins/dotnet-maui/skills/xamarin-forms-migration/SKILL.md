---
name: xamarin-forms-migration
description: >
  **WORKFLOW SKILL** - Guide for migrating Xamarin.Forms apps to .NET MAUI. Covers project structure
  decisions, SDK-style project conversion, namespace renames, layout behavior changes,
  renderer-to-handler migration, effects-to-behaviors redesign, Xamarin.Essentials
  namespace mapping, NuGet dependency compatibility, data migration, and common
  troubleshooting. Incorporates field-tested advice from production migrations.
  USE FOR: "migrate Xamarin.Forms", "upgrade to MAUI", "Xamarin to MAUI",
  "convert Xamarin.Forms project", "Forms migration", "namespace changes Xamarin",
  "renderer to handler", "effects to behaviors", "AndExpand replacement",
  "layout changes MAUI", "Xamarin.Essentials to MAUI".
  DO NOT USE FOR: migrating Xamarin.Android native apps (use xamarin-android-migration),
  migrating Xamarin.iOS native apps (use xamarin-ios-migration),
  creating new MAUI handlers from scratch (use maui-custom-handlers),
  performance optimization (use maui-performance).
---

# Xamarin.Forms → .NET MAUI Migration

Use this skill when migrating a Xamarin.Forms app to .NET MAUI. This covers the
full migration path from project file conversion through UI and API changes.

> **Field-tested advice:** Do not use the .NET Upgrade Assistant. Apply namespace
> renames, project file updates, and package replacements directly. Build after
> each batch of changes and use compiler errors to guide the next round of fixes.

## Migration Workflow Overview

1. Choose project structure (single-project recommended)
2. Create a new .NET MAUI project
3. Copy code and resources
4. Update namespaces (XAML + C# + Essentials)
5. Fix layout behavior changes
6. Migrate renderers → handlers
7. Migrate effects → behaviors
8. Remove Compatibility package
9. Update NuGet dependencies
10. Migrate app data stores
11. Bootstrap, build, and test iteratively
12. Verify against .NET 10 deprecated API list

---

## Step 1 — Choose Project Structure

| Option | Recommendation |
|--------|---------------|
| **Single-project** (recommended) | One `.csproj` with `<TargetFrameworks>net8.0-android;net8.0-ios;...</TargetFrameworks>`. Platform code lives in `Platforms/` folders. Avoids AOT/build errors that multi-project can cause. |
| **Multi-project** | Separate head projects per platform. Only use if you have a strong reason (shared solution with non-MAUI projects). |

> **Field advice:** Use single-project. Multi-project causes AOT/build errors and you'll lose git history.

---

## Step 2 — Create a New .NET MAUI Project

Create a new .NET MAUI project with the same name as your Xamarin.Forms app, then
copy code into it. This is simpler and less error-prone than editing the existing
project file in place.

```xml
<!-- .NET MAUI single-project csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>net8.0-android;net8.0-ios;net8.0-maccatalyst</TargetFrameworks>
    <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">
      $(TargetFrameworks);net8.0-windows10.0.19041.0
    </TargetFrameworks>
    <OutputType>Exe</OutputType>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>
</Project>
```

> Replace `net8.0` with `net9.0` or `net10.0` as appropriate for your target version.
> The migration steps are identical across .NET 8, 9, and 10.

---

## Step 3 — Copy Code and Resources

1. Copy all cross-platform code from your Xamarin.Forms library project into the
   new MAUI project (same folder structure).
2. Copy platform-specific code from each head project into `Platforms/<platform>/`.
3. Copy custom code from `MainActivity`/`MainApplication` (Android) and
   `AppDelegate` (iOS) into the MAUI equivalents.
4. Copy resources (images, fonts, raw assets) into `Resources/`.

---

## Step 4 — Update Namespaces

### XAML Namespace Changes

| Xamarin.Forms | .NET MAUI |
|---------------|-----------|
| `xmlns="http://xamarin.com/schemas/2014/forms"` | `xmlns="http://schemas.microsoft.com/dotnet/2021/maui"` |
| `xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"` | *(unchanged)* |

### C# Namespace Changes

| Xamarin.Forms Namespace | .NET MAUI Namespace |
|------------------------|---------------------|
| `Xamarin.Forms` | `Microsoft.Maui.Controls` |
| `Xamarin.Forms.Xaml` | `Microsoft.Maui.Controls.Xaml` |
| `Xamarin.Forms.PlatformConfiguration` | `Microsoft.Maui.Controls.PlatformConfiguration` |
| `Xamarin.Forms.PlatformConfiguration.iOSSpecific` | `Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific` |
| `Xamarin.Forms.PlatformConfiguration.AndroidSpecific` | `Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific` |
| `Xamarin.Forms.Shapes` | `Microsoft.Maui.Controls.Shapes` |
| `Xamarin.Forms.StyleSheets` | *(removed — use MAUI styles)* |

### Xamarin.Essentials → .NET MAUI Namespaces

In .NET MAUI, Xamarin.Essentials functionality is built in. Remove the `Xamarin.Essentials`
NuGet package and update `using` directives:

| Xamarin.Essentials | .NET MAUI Namespace |
|-------------------|---------------------|
| `Xamarin.Essentials` (general) | Split across multiple namespaces below |
| App actions, permissions, version tracking | `Microsoft.Maui.ApplicationModel` |
| Contacts, email, networking | `Microsoft.Maui.ApplicationModel.Communication` |
| Battery, sensors, flashlight, haptics | `Microsoft.Maui.Devices` |
| Media picking, text-to-speech | `Microsoft.Maui.Media` |
| Clipboard, file sharing | `Microsoft.Maui.ApplicationModel.DataTransfer` |
| File picking, secure storage, preferences | `Microsoft.Maui.Storage` |

---

## Step 5 — Fix Layout Behavior Changes

### Default Value Changes

.NET MAUI zeroes out default spacing/padding that Xamarin.Forms set to non-zero values.
Add explicit values or use implicit styles to preserve old behavior:

| Property | Xamarin.Forms Default | .NET MAUI Default |
|----------|----------------------|-------------------|
| `Grid.ColumnSpacing` | 6 | 0 |
| `Grid.RowSpacing` | 6 | 0 |
| `StackLayout.Spacing` | 6 | 0 |

```xml
<!-- Implicit styles to restore Xamarin.Forms defaults (add to App.xaml) -->
<Style TargetType="Grid">
    <Setter Property="ColumnSpacing" Value="6"/>
    <Setter Property="RowSpacing" Value="6"/>
</Style>
<Style TargetType="StackLayout">
    <Setter Property="Spacing" Value="6"/>
</Style>
```

> **Field advice:** Specify all layout values explicitly — don't rely on platform defaults. Define them in styles.

### Key Layout Behavior Differences

| Issue | Xamarin.Forms | .NET MAUI | Fix |
|-------|--------------|-----------|-----|
| Grid columns/rows | Inferred from XAML | Must be explicitly declared | Add `ColumnDefinitions` and `RowDefinitions` |
| `*AndExpand` options | Supported on StackLayout | Obsolete — no effect on `HorizontalStackLayout`/`VerticalStackLayout` | Convert to `Grid` with `*` row/column sizes |
| StackLayout fill | Children could fill stacking direction | Children stack beyond available space | Use `Grid` when children need to fill space |
| `RelativeLayout` | Built-in | Compatibility namespace only | Replace with `Grid` |
| `Frame` | Built-in | Replaced by `Border` (Frame still works but measures differently) | Migrate to `Border` |
| `ScrollView` in StackLayout | Compressed to fit | Expands to full content height (no scroll) | Place `ScrollView` in `Grid` with constrained row |
| `BoxView` default size | 40×40 | 0×0 | Set explicit `WidthRequest`/`HeightRequest` |

### Converting `*AndExpand` to Grid

```xml
<!-- BEFORE (Xamarin.Forms) -->
<StackLayout>
    <Label Text="Hello world!"/>
    <Image VerticalOptions="FillAndExpand" Source="dotnetbot.png"/>
</StackLayout>

<!-- AFTER (.NET MAUI) -->
<Grid RowDefinitions="Auto, *">
    <Label Text="Hello world!"/>
    <Image Grid.Row="1" Source="dotnetbot.png"/>
</Grid>
```

> **Field advice:** Flatten layout trees. Replace nested hierarchies with single Grid layouts. Avoid deep nesting.

---

## Step 6 — Migrate Renderers to Handlers

> **Field advice:** Migrate all renderers to handlers — don't use shimmed renderers. Extend existing mappers when possible.

### Option A — Customize Existing Handlers (Preferred)

Use mapper methods to modify existing control behavior without a full handler:

```csharp
// In MauiProgram.cs
Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, view) =>
{
#if ANDROID
    handler.PlatformView.Background = null;
#elif IOS || MACCATALYST
    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
});
```

| Mapper Method | When it runs |
|---------------|-------------|
| `PrependToMapping` | Before default mapper |
| `ModifyMapping` | Replaces default mapper |
| `AppendToMapping` | After default mapper |

### Option B — Full Handler Migration

For renderers that require a completely new native view, migrate to a full handler:

1. Create a cross-platform control class (extends `View`)
2. Create a `partial` handler class with `PropertyMapper`
3. Create platform-specific partial classes with `CreatePlatformView()`
4. Register with `ConfigureMauiHandlers` in `MauiProgram.cs`

> For detailed handler creation guidance, use the **maui-custom-handlers** skill.

### Shimmed Renderers (Fallback Only)

If you must keep a renderer temporarily, .NET MAUI provides shims for renderers
deriving from `FrameRenderer`, `ListViewRenderer`, `ShellRenderer`,
`TableViewRenderer`, and `VisualElementRenderer`:

1. Move code to `Platforms/<platform>/` folders
2. Change `Xamarin.Forms.*` namespaces to `Microsoft.Maui.*`
3. Remove `[assembly: ExportRenderer(...)]` attributes
4. Register with `ConfigureMauiHandlers` / `AddHandler`

> **Warning:** Shimmed renderers are a migration convenience, not a long-term solution.
> They create parent wrapper views that hurt performance.

---

## Step 7 — Migrate Effects to Behaviors

> **Field advice:** Effects are now Behaviors — this requires redesign, not just renaming.

Effects can be migrated to .NET MAUI but require changes:

1. Remove `ResolutionGroupNameAttribute` and `ExportEffectAttribute`
2. Remove `Xamarin.Forms` and `Xamarin.Forms.Platform.*` using directives
3. Combine `RoutingEffect` + `PlatformEffect` implementations into a single file
   with conditional compilation
4. Register with `ConfigureEffects` in `MauiProgram.cs`

```csharp
// MauiProgram.cs
builder.ConfigureEffects(effects =>
{
    effects.Add<FocusRoutingEffect, FocusPlatformEffect>();
});
```

For new development, prefer behaviors or handler mapper customizations over effects.

---

## Step 8 — Do NOT Use the Compatibility Package

> **Field advice:** `Microsoft.Maui.Controls.Compatibility` causes cascading incompatibilities. Remove it and rebuild layouts natively.

---

## Step 9 — Update NuGet Dependencies

| Compatible Frameworks | Incompatible Frameworks |
|----------------------|------------------------|
| `net8.0-android`, `monoandroid`, `monoandroidXX.X` | |
| `net8.0-ios` | `monotouch`, `xamarinios`, `xamarinios10` |
| `net8.0-macos` | `monomac`, `xamarinmac`, `xamarinmac20` |
| `net8.0-tvos` | `xamarintvos` |

.NET Standard libraries without incompatible framework dependencies remain compatible.

If no .NET-compatible version exists:
1. Recompile the package with .NET TFMs (if you own it)
2. Look for a preview .NET version
3. Replace with a .NET-compatible alternative

### Retired Dependencies

- **App Center** is retired → Replace with Sentry, Azure Monitor, or similar
- **Visual Studio for Mac** is retired → Use VS Code or Rider

---

## Step 10 — Migrate App Data

| Data Store | Migration Guide |
|-----------|----------------|
| `Application.Properties` | Migrate to `Preferences` (`Microsoft.Maui.Storage`) |
| Secure Storage | Migrate from `Xamarin.Essentials.SecureStorage` to `Microsoft.Maui.Storage.SecureStorage` |
| Version Tracking | Migrate from `Xamarin.Essentials.VersionTracking` to `Microsoft.Maui.ApplicationModel.VersionTracking` |
| SkiaSharp | Update to SkiaSharp 2.88+ with `SkiaSharp.Views.Maui` |

---

## Step 11 — Bootstrap and Test

### Entry Point Changes

Replace the Xamarin.Forms `App` class initialization with MAUI's `MauiProgram`:

```csharp
// MauiProgram.cs
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });
        return builder.Build();
    }
}
```

### Build and Fix

1. Delete all `bin/` and `obj/` folders
2. Delete `Resource.designer.cs` (Android)
3. Build and fix compiler errors iteratively
4. Profile early — do not assume MAUI is automatically faster

> **Field advice:** Build after each batch of changes and use compiler errors to guide fixes.

---

## .NET 10 API Currency Warning

When migrating to .NET MAUI targeting .NET 10, avoid these MAUI APIs that are
themselves deprecated or obsolete. Use the **maui-current-apis** skill for the
full list.

| Avoid in .NET 10 | Use Instead |
|-------------------|-------------|
| `ListView`, `TableView` | `CollectionView` |
| `Frame` | `Border` with `StrokeShape` |
| `Device.RuntimePlatform` | `DeviceInfo.Platform` |
| `Device.BeginInvokeOnMainThread()` | `MainThread.BeginInvokeOnMainThread()` |
| `Device.OpenUri()` | `Launcher.OpenAsync()` |
| `DependencyService` | Constructor injection via `builder.Services` |
| `MessagingCenter` | `WeakReferenceMessenger` (CommunityToolkit.Mvvm) |
| `DisplayAlert()` / `DisplayActionSheet()` | `DisplayAlertAsync()` / `DisplayActionSheetAsync()` |
| `FadeTo()`, `RotateTo()`, etc. | `FadeToAsync()`, `RotateToAsync()`, etc. |
| `Color.FromHex()` | `Color.FromArgb()` |
| `Page.IsBusy` | `ActivityIndicator` |

> Migration is the perfect time to skip deprecated APIs entirely. Don't migrate
> Xamarin.Forms code to a MAUI API that's already on its way out.

---

## Common Troubleshooting

| Issue | Fix |
|-------|-----|
| `Xamarin.*` namespace doesn't exist | Update to `Microsoft.Maui.*` equivalent |
| CollectionView doesn't scroll | Place in Grid (not StackLayout) to constrain size |
| Pop-up under page on iOS | Use `DisplayAlert`/`DisplayActionSheet` from the `ContentPage` |
| BoxView not visible | Set `WidthRequest` and `HeightRequest` (default is 0×0 in MAUI) |
| Missing padding/margin/spacing | Add explicit values or implicit styles for old defaults |
| Custom layout broken | Rewrite using MAUI layout APIs |
| Custom renderer broken | Migrate to handler (see Step 6) |
| Effect broken | Migrate to MAUI effect registration (see Step 7) |
| SkiaSharp broken | Update to `SkiaSharp.Views.Maui` package |
| Can't access App.Properties data | Migrate to Preferences |

---

## Android-Specific Warnings

- Android migration is **significantly harder** than iOS. Expect more UI bugs.
- Android has OEM-specific rendering differences not reproducible on emulators.
  **Test on physical devices.**
- Shadow rendering varies across Android OEMs and API levels. Implement shadows
  in platform-specific handler code.
- Handler-level property changes (e.g., colors) do not auto-update on theme switch.
  Manually handle theme changes inside custom handlers.

---

## Quick Checklist

1. ☐ Created new .NET MAUI project (single-project preferred)
2. ☐ Copied cross-platform and platform code
3. ☐ Updated XAML namespace to `http://schemas.microsoft.com/dotnet/2021/maui`
4. ☐ Replaced `Xamarin.Forms.*` → `Microsoft.Maui.*` namespaces
5. ☐ Replaced `Xamarin.Essentials` → split MAUI namespaces
6. ☐ Added explicit Grid `ColumnDefinitions`/`RowDefinitions`
7. ☐ Replaced `*AndExpand` with Grid layouts
8. ☐ Migrated renderers to handlers (not shimmed renderers)
9. ☐ Migrated effects to behaviors or MAUI effects
10. ☐ Removed `Microsoft.Maui.Controls.Compatibility` package
11. ☐ Updated NuGet dependencies for .NET compatibility
12. ☐ Migrated App.Properties, SecureStorage, VersionTracking data
13. ☐ Added explicit spacing/padding values (MAUI defaults to 0)
14. ☐ Deleted `bin/`, `obj/`, and `Resource.designer.cs`
15. ☐ Tested on physical Android device
16. ☐ Profiled performance
17. ☐ Verified no .NET 10 deprecated APIs (run **maui-current-apis** skill)

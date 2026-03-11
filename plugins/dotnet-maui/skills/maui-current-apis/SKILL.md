---
name: maui-current-apis
description: >
  Always-on guardrail for .NET MAUI API currency. Prevents AI coding agents from
  using deprecated, obsolete, or removed APIs across XAML/C#, Blazor Hybrid, and
  MauiReactor. Includes a reasoning framework for detecting project target framework
  and library versions, plus a curated table of the most common deprecated API traps
  in .NET MAUI 10.
  USE FOR: "deprecated API", "obsolete API", "API migration", "MAUI breaking changes",
  "check API currency", "review MAUI code", "generate MAUI code", "edit MAUI code".
  DO NOT USE FOR: learning new MAUI features (use feature-specific skills),
  performance optimization (use maui-performance), or testing guidance (use maui-unit-testing).
---

# .NET MAUI Current APIs — Guardrail Skill

This skill prevents you from generating code that uses deprecated, obsolete, or removed APIs.
**Read this before writing any .NET MAUI, Blazor Hybrid, or MauiReactor code.**

## Reasoning Framework

Follow these steps **before** generating any MAUI-related code:

### Step 1 — Detect the Target Framework

Read the project's `.csproj` file and find `<TargetFramework>` or `<TargetFrameworks>`:

```xml
<!-- Single target -->
<TargetFramework>net10.0-android</TargetFramework>

<!-- Multi-target (typical MAUI project) -->
<TargetFrameworks>net10.0-android;net10.0-ios;net10.0-maccatalyst;net10.0-windows10.0.19041.0</TargetFrameworks>
```

The version number (`net10.0`, `net9.0`, `net8.0`) determines which APIs are available.
**Always target the version in the project file — never assume a version.**

### Step 2 — Detect Library Versions

Scan `<PackageReference>` entries and `<ItemGroup>` for key packages:

| Package | What it tells you |
|---------|-------------------|
| `Microsoft.Maui.Controls` | MAUI version (if explicit) |
| `CommunityToolkit.Maui` | Toolkit version — APIs change between major versions |
| `CommunityToolkit.Mvvm` | MVVM Toolkit version — needed for `WeakReferenceMessenger` |
| `Reactor.Maui` | MauiReactor version — v3+ has different APIs than v2 |
| `Microsoft.AspNetCore.Components.WebView.Maui` | Blazor Hybrid version |

If no explicit MAUI package version is listed, the MAUI SDK version matches the `<TargetFramework>` .NET version.

### Step 3 — Verify API Currency

Before using any API, ask yourself:

1. **Does this API exist in the detected version?** If you're unsure, prefer the newer pattern.
2. **Is this a Xamarin.Forms API?** MAUI is a different API surface — never assume Xamarin.Forms compatibility.
3. **Is this in the deprecated table below?** If so, use the replacement.
4. **Am I using the Compatibility namespace?** `Microsoft.Maui.Controls.Compatibility.*` types are migration aids, not recommended for new code.

### Step 4 — Apply Decision Rules

- **Always use the newest form** of an API when both old and new exist.
- **Never generate `using Xamarin.Forms`** or `using Xamarin.Essentials` — these are not MAUI.
- **Never use the `Compatibility` namespace** in new projects (`Compatibility.RelativeLayout`, `Compatibility.StackLayout`, etc.).
- **Prefer `async` method names** when both sync-named and async-named versions exist (e.g., `DisplayAlertAsync` over `DisplayAlert`).
- **Check the project's NuGet versions** before using CommunityToolkit or third-party APIs.

---

## Deprecated API Table — .NET MAUI 10

### Controls

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `ListView` | `CollectionView` | `ListView`, `EntryCell`, `ImageCell`, `SwitchCell`, `TextCell`, `ViewCell`, and `Cell` are all deprecated in .NET 10 |
| `TableView` | `CollectionView` or custom layout | Deprecated in .NET 10 |
| `Frame` | `Border` | `Frame` is a Xamarin.Forms legacy control; `Border` is the MAUI replacement with `StrokeShape` support |
| `Compatibility.RelativeLayout` | `Grid` | Migration-only; removed from templates in .NET 10 |
| `Compatibility.StackLayout` | `VerticalStackLayout` / `HorizontalStackLayout` | The compatibility `StackLayout` wraps Xamarin layout logic. Use the native MAUI stack layouts for better performance |

### Gestures & Input

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `ClickGestureRecognizer` | `TapGestureRecognizer` | Removed in .NET 10 |
| `Accelerator` | `KeyboardAccelerator` | Removed from `Microsoft.Maui.Controls` in .NET 10 |

### Page & Navigation

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `Page.IsBusy` | `ActivityIndicator` | `IsBusy` is obsolete in .NET 10; use an explicit activity indicator |
| `DisplayAlert()` | `DisplayAlertAsync()` | Sync-named versions deprecated; use async versions |
| `DisplayActionSheet()` | `DisplayActionSheetAsync()` | Same as above |
| `MessagingCenter` | `WeakReferenceMessenger` (CommunityToolkit.Mvvm) | Made internal in .NET 10 |

### Animation

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `FadeTo()` | `FadeToAsync()` | All animation extension methods renamed to `*Async` in .NET 10 |
| `RotateTo()`, `ScaleTo()`, `TranslateTo()`, etc. | `RotateToAsync()`, `ScaleToAsync()`, `TranslateToAsync()`, etc. | Same pattern — `RelRotateTo` → `RelRotateToAsync`, `RelScaleTo` → `RelScaleToAsync`, `LayoutTo` → `LayoutToAsync` |

### Device & Platform APIs

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `Device.RuntimePlatform` | `DeviceInfo.Platform` | The entire `Device` class is deprecated |
| `Device.BeginInvokeOnMainThread()` | `MainThread.BeginInvokeOnMainThread()` | Use `Microsoft.Maui.ApplicationModel.MainThread` |
| `Device.InvokeOnMainThreadAsync()` | `MainThread.InvokeOnMainThreadAsync()` | Same |
| `Device.OpenUri()` | `Launcher.OpenAsync()` | Use `Microsoft.Maui.ApplicationModel.Launcher` |
| `Device.StartTimer()` | `Dispatcher.StartTimer()` or `PeriodicTimer` | Use the dispatcher or standard .NET timer |
| `DependencyService` | Constructor injection via `IServiceProvider` | Register services in `MauiProgram.cs` with `builder.Services` |

### XAML & Markup

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `FontImageExtension` (XAML markup extension) | `FontImageSource` (type) | Use `<FontImageSource>` element or property syntax |
| `Color.FromHex()` | `Color.FromArgb()` | `FromHex` is obsolete |

### Safe Area & Layout

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `Page.UseSafeArea` (iOS platform-specific) | `SafeAreaEdges` property | New in .NET 10; `ContentPage` defaults to `None` (edge-to-edge) on all platforms |
| `Layout.IgnoreSafeArea` | `SafeAreaEdges` property | Use the unified safe area API |

### Accessibility

| ❌ Deprecated / Removed | ✅ Use Instead | Notes |
|--------------------------|----------------|-------|
| `AutomationProperties.Name` | `SemanticProperties.Description` | `AutomationProperties` still works but `SemanticProperties` is the MAUI-native approach |
| `AutomationProperties.HelpText` | `SemanticProperties.Hint` | Same |
| iOS `SetAccessibilityHint` / `SetAccessibilityLabel` (Compatibility extensions) | `Microsoft.Maui.Platform.UpdateSemantics()` | Deprecated in .NET 10 |

### NuGet Packages

| ❌ Deprecated Package | ✅ Use Instead | Notes |
|------------------------|----------------|-------|
| `Xamarin.Forms` | `Microsoft.Maui.Controls` | Completely different API surface |
| `Xamarin.Essentials` | Built-in MAUI APIs (`Microsoft.Maui.Devices`, `Microsoft.Maui.ApplicationModel`, etc.) | Essentials is merged into MAUI |
| `Xamarin.CommunityToolkit` | `CommunityToolkit.Maui` | Different namespace and API surface |
| `Microsoft.Toolkit.Mvvm` | `CommunityToolkit.Mvvm` | Package was renamed |

---

## MauiReactor-Specific Guidance

MauiReactor v3+ (for .NET MAUI 9/10):

- **Hot reload**: v3+ uses a feature switch in `.csproj` via `<RuntimeHostConfigurationOption>`. Do NOT use the v2 `EnableMauiReactorHotReload()` call in `MauiProgram.cs`.
- **API wrappers**: MauiReactor auto-generates C# wrappers around MAUI controls. When a MAUI control is deprecated (e.g., `ListView`), the MauiReactor wrapper is also effectively deprecated. Use the MauiReactor wrapper for the replacement control (e.g., `CollectionView`).
- **State management**: Use `State<T>` and `Props<T>` — not older `RxComponent` patterns if they appear in outdated examples.
- **Navigation**: Use MauiReactor's built-in navigation (`Navigation.PushAsync<PageComponent>()`) — do NOT mix in MAUI Shell `GoToAsync` unless deliberately integrating Shell.

---

## Blazor Hybrid-Specific Guidance

- Use **`BlazorWebView`** — not the legacy `WebView` — for hosting Razor components.
- Use **.NET 10 Razor syntax**: `@rendermode` directives, `[SupplyParameterFromQuery]`, and the latest component model.
- **JS interop**: Use `IJSRuntime.InvokeAsync<T>()` — not the obsolete synchronous `IJSInProcessRuntime` patterns for mobile.
- **Safe areas**: In Blazor Hybrid, use CSS `env(safe-area-inset-*)` — do NOT combine XAML `SafeAreaEdges` and CSS safe area padding on the same element (causes double-padding).
- **Web resource interception**: .NET 10 adds `WebResourceRequested` event on `BlazorWebView` and `HybridWebView` for intercepting requests.

---

## Version Detection Cheat Sheet

### Reading the TFM

| TFM Pattern | .NET Version |
|-------------|-------------|
| `net10.0-*` | .NET 10 (latest, current) |
| `net9.0-*` | .NET 9 |
| `net8.0-*` | .NET 8 (LTS) |

### Reading MAUI SDK Version

If the `.csproj` uses `<UseMaui>true</UseMaui>`, the MAUI version matches the TFM:
- `net10.0` → MAUI 10
- `net9.0` → MAUI 9

### Reading MauiReactor Version

Check `<PackageReference Include="Reactor.Maui" Version="X.Y.Z" />`:
- v3.x → .NET MAUI 9/10 compatible, uses feature-switch hot reload
- v2.x → Legacy, uses `EnableMauiReactorHotReload()` builder pattern

### Reading CommunityToolkit Version

Check `<PackageReference Include="CommunityToolkit.Maui" Version="X.Y.Z" />`:
- v11+ → .NET 10 compatible
- v9-10 → .NET 9 compatible
- v5-7 → .NET 8 compatible

---

## Quick Rules

1. **Read the `.csproj` first.** Never generate code without knowing the target framework.
2. **When in doubt, use the newer API.** If two ways exist to do something, the newer way is correct.
3. **Never use `Xamarin.*` namespaces.** They do not exist in MAUI.
4. **Never use `Compatibility.*` types in new projects.** They are migration aids only.
5. **Check this table before using any `Device.*` API.** The `Device` class is fully deprecated.
6. **Use `*Async` method names** for animations, alerts, and action sheets in .NET 10+.
7. **Verify third-party package versions** before using their APIs — CommunityToolkit, MauiReactor, and others break between major versions.

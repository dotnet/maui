---
name: maui-theming
description: >
  Guide for theming .NET MAUI apps—light/dark mode support, AppThemeBinding,
  dynamic resources, ResourceDictionary theme switching, and system theme detection.
  USE FOR: "dark mode", "light mode", "theming", "AppThemeBinding", "theme switching",
  "ResourceDictionary theme", "dynamic resources", "system theme detection",
  "color scheme", "app theme".
  DO NOT USE FOR: localization or language switching (use maui-localization),
  accessibility visual adjustments (use maui-accessibility), or app icons (use maui-app-icons-splash).
---

# .NET MAUI Theming

Two main approaches exist for theming MAUI apps: **AppThemeBinding** (automatic OS theme response) and **ResourceDictionary theming** (custom themes with runtime switching). They can be combined.

## 1. AppThemeBinding (OS Theme Response)

`AppThemeBinding` is a markup extension that selects a value based on the current system theme.

### Properties

| Property | Description |
|----------|-------------|
| `Light`  | Value used when the device is in light mode |
| `Dark`   | Value used when the device is in dark mode |
| `Default`| Fallback value when the device theme is unknown |

### XAML Usage

```xml
<Label Text="Themed text"
       TextColor="{AppThemeBinding Light=Green, Dark=Red}"
       BackgroundColor="{AppThemeBinding Light=White, Dark=Black}" />

<!-- With StaticResource or DynamicResource values -->
<Label TextColor="{AppThemeBinding Light={StaticResource LightPrimary},
                                   Dark={StaticResource DarkPrimary}}" />
```

### C# Extension Methods

```csharp
var label = new Label();
// Color-specific helper
label.SetAppThemeColor(Label.TextColorProperty, Colors.Green, Colors.Red);

// Generic helper for any type
label.SetAppTheme<Color>(Label.TextColorProperty, Colors.Green, Colors.Red);
```

## 2. ResourceDictionary Theming (Custom Themes)

Use separate `ResourceDictionary` files with matching keys to define themes, then swap them at runtime.

### Define Theme Dictionaries

Each ResourceDictionary **must** have a code-behind file that calls `InitializeComponent()`.

**LightTheme.xaml**
```xml
<ResourceDictionary xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
                    xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
                    x:Class="MyApp.Themes.LightTheme">
    <Color x:Key="PageBackgroundColor">White</Color>
    <Color x:Key="PrimaryTextColor">#333333</Color>
    <Color x:Key="AccentColor">#2196F3</Color>
</ResourceDictionary>
```

**LightTheme.xaml.cs**
```csharp
namespace MyApp.Themes;

public partial class LightTheme : ResourceDictionary
{
    public LightTheme() => InitializeComponent();
}
```

**DarkTheme.xaml.cs** — same pattern, different values, same keys.

### Consume with DynamicResource

Use `DynamicResource` (not `StaticResource`) so values update at runtime when the dictionary changes:

```xml
<ContentPage BackgroundColor="{DynamicResource PageBackgroundColor}">
    <Label Text="Hello"
           TextColor="{DynamicResource PrimaryTextColor}" />
    <Button Text="Action"
            BackgroundColor="{DynamicResource AccentColor}" />
</ContentPage>
```

### Switch Themes at Runtime

```csharp
void ApplyTheme(ResourceDictionary theme)
{
    var mergedDictionaries = Application.Current!.Resources.MergedDictionaries;
    mergedDictionaries.Clear();
    mergedDictionaries.Add(theme);
}

// Usage
ApplyTheme(new LightTheme());
ApplyTheme(new DarkTheme());
```

## System Theme Detection

### Current Theme

```csharp
AppTheme currentTheme = Application.Current!.RequestedTheme;
// Returns AppTheme.Light, AppTheme.Dark, or AppTheme.Unspecified
```

### Override the System Theme

```csharp
// Force a specific theme regardless of OS setting
Application.Current!.UserAppTheme = AppTheme.Dark;

// Reset to follow the system theme
Application.Current!.UserAppTheme = AppTheme.Unspecified;
```

### React to Theme Changes

```csharp
Application.Current!.RequestedThemeChanged += (s, e) =>
{
    AppTheme newTheme = e.RequestedTheme;
    // Update UI or switch ResourceDictionaries
};
```

## Combining Both Approaches

Use `AppThemeBinding` with `DynamicResource` values for maximum flexibility:

```xml
<Label TextColor="{AppThemeBinding
    Light={DynamicResource LightPrimary},
    Dark={DynamicResource DarkPrimary}}" />
```

Or react to system changes and swap full ResourceDictionaries:

```csharp
Application.Current!.RequestedThemeChanged += (s, e) =>
{
    ApplyTheme(e.RequestedTheme == AppTheme.Dark
        ? new DarkTheme()
        : new LightTheme());
};
```

## Platform Support & Gotchas

| Platform       | Minimum Version |
|----------------|-----------------|
| iOS            | 13+             |
| Android        | 10+ (API 29)    |
| macOS Catalyst | 10.14+          |
| Windows        | 10+             |

### Android: ConfigChanges.UiMode

`MainActivity` **must** include `ConfigChanges.UiMode` or theme change events will not fire and the activity may restart instead:

```csharp
[Activity(Theme = "@style/Maui.SplashTheme",
          MainLauncher = true,
          ConfigurationChanges = ConfigChanges.ScreenSize
                               | ConfigChanges.Orientation
                               | ConfigChanges.UiMode  // Required for theme detection
                               | ConfigChanges.ScreenLayout
                               | ConfigChanges.SmallestScreenSize
                               | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity { }
```

### CSS Theming Limitation

MAUI supports CSS styling, but CSS-based themes **cannot be swapped dynamically** at runtime. Use ResourceDictionary theming instead for dynamic theme switching.

## Quick Reference

- **OS light/dark** → `AppThemeBinding` markup extension
- **Theme colors in C#** → `SetAppThemeColor()`, `SetAppTheme<T>()`
- **Read OS theme** → `Application.Current.RequestedTheme`
- **Force theme** → `Application.Current.UserAppTheme = AppTheme.Dark`
- **Theme changes** → `RequestedThemeChanged` event
- **Custom switching** → Swap `ResourceDictionary` in `MergedDictionaries`
- **Runtime bindings** → `DynamicResource` (not `StaticResource`)

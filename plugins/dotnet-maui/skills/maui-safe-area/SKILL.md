---
name: maui-safe-area
description: >
  .NET MAUI safe area and edge-to-edge layout guidance for .NET 10+. Covers
  SafeAreaEdges property, SafeAreaRegions enum, per-edge control, keyboard
  avoidance, Blazor Hybrid CSS safe areas, migration from legacy APIs, and
  platform-specific behavior for Android, iOS, and Mac Catalyst.
  USE FOR: "safe area", "edge-to-edge", "SafeAreaEdges", "SafeAreaRegions",
  "keyboard avoidance", "notch insets", "status bar overlap", "iOS safe area",
  "Android edge-to-edge", "content behind status bar".
  DO NOT USE FOR: general layout or grid design (use maui-collectionview),
  app lifecycle handling (use maui-app-lifecycle), or theming (use maui-theming).
---

# Safe Area & Edge-to-Edge Layout in .NET MAUI (.NET 10+)

## Overview

In .NET 10, MAUI introduces the `SafeAreaEdges` property for precise per-edge
control over how content interacts with system bars, notches, display cutouts,
and on-screen keyboards. This replaces the legacy iOS-only `Page.UseSafeArea`
and `Layout.IgnoreSafeArea` properties with a unified cross-platform API.

**Breaking change:** In .NET 10, `ContentPage` defaults to `None` (edge-to-edge)
on all platforms. In .NET 9, Android `ContentPage` behaved like `Container`.

## Core APIs

### SafeAreaRegions enum (flags)

```csharp
[Flags]
public enum SafeAreaRegions
{
    None      = 0,       // Edge-to-edge — no safe area padding
    SoftInput = 1 << 0,  // Pad to avoid keyboard
    Container = 1 << 1,  // Stay out of bars/notch, flow under keyboard
    Default   = -1,      // Platform default for the control type
    All       = 1 << 15  // Obey all safe area insets (most restrictive)
}
```

`SoftInput` and `Container` are flags and can be combined:
`SafeAreaRegions.Container | SafeAreaRegions.SoftInput` = respect bars AND keyboard.

### SafeAreaEdges struct

`SafeAreaEdges` is a struct with per-edge `SafeAreaRegions` values:

```csharp
public readonly struct SafeAreaEdges
{
    public SafeAreaRegions Left { get; }
    public SafeAreaRegions Top { get; }
    public SafeAreaRegions Right { get; }
    public SafeAreaRegions Bottom { get; }

    // Uniform — same value for all edges
    public SafeAreaEdges(SafeAreaRegions uniformValue)

    // Horizontal/Vertical
    public SafeAreaEdges(SafeAreaRegions horizontal, SafeAreaRegions vertical)

    // Per-edge
    public SafeAreaEdges(SafeAreaRegions left, SafeAreaRegions top,
                         SafeAreaRegions right, SafeAreaRegions bottom)
}
```

**Static presets:** `SafeAreaEdges.None`, `SafeAreaEdges.All`, `SafeAreaEdges.Default`

### XAML type converter

The XAML type converter follows Thickness-like syntax with comma-separated values:

```xaml
<!-- Uniform: all edges = Container -->
SafeAreaEdges="Container"

<!-- Horizontal, Vertical -->
SafeAreaEdges="Container, SoftInput"

<!-- Left, Top, Right, Bottom -->
SafeAreaEdges="Container, Container, Container, SoftInput"
```

### Controls that support SafeAreaEdges

| Control | Default value | Notes |
|---------|--------------|-------|
| `ContentPage` | `None` | Edge-to-edge. **Breaking change from .NET 9 Android.** |
| `Layout` (Grid, StackLayout, etc.) | `Container` | Respects bars/notch, flows under keyboard |
| `ScrollView` | `Default` | iOS: maps to `UIScrollViewContentInsetAdjustmentBehavior.Automatic`. Only `Container` and `None` have effect. |
| `ContentView` | `None` | Inherits parent behavior |
| `Border` | `None` | Inherits parent behavior |

## Usage Patterns

### 1. Edge-to-edge content (background images, immersive UIs)

```xaml
<ContentPage SafeAreaEdges="None">
    <Grid SafeAreaEdges="None">
        <Image Source="background.jpg" Aspect="AspectFill" />
        <VerticalStackLayout Padding="20"
                             VerticalOptions="End">
            <Label Text="Overlay text"
                   TextColor="White"
                   FontSize="24" />
        </VerticalStackLayout>
    </Grid>
</ContentPage>
```

**Important:** The `Grid` must be explicitly set to `SafeAreaEdges="None"` because
layouts default to `Container`. Without this, the Grid respects system bars and
prevents true edge-to-edge.

### 2. Respect all safe areas (forms, critical content)

```xaml
<ContentPage SafeAreaEdges="All">
    <VerticalStackLayout Padding="20">
        <Label Text="Safe content" FontSize="18" />
        <Entry Placeholder="Enter text" />
        <Button Text="Submit" />
    </VerticalStackLayout>
</ContentPage>
```

Content is never obscured by system bars, notches, or the keyboard.

### 3. Keyboard-aware chat/messaging layout

```xaml
<ContentPage>
    <Grid RowDefinitions="*,Auto"
          SafeAreaEdges="Container, Container, Container, SoftInput">
        <ScrollView Grid.Row="0">
            <VerticalStackLayout Padding="20" Spacing="10">
                <Label Text="Messages" FontSize="24" />
                <!-- message items -->
            </VerticalStackLayout>
        </ScrollView>

        <Border Grid.Row="1"
                BackgroundColor="LightGray"
                Padding="20">
            <HorizontalStackLayout Spacing="10">
                <Entry Placeholder="Type a message..."
                       HorizontalOptions="Fill" />
                <Button Text="Send" />
            </HorizontalStackLayout>
        </Border>
    </Grid>
</ContentPage>
```

Top and sides use `Container` (avoid bars/notch); bottom uses `SoftInput`
(avoid keyboard). The Grid handles safe area; ScrollView handles scrolling.

### 4. Mixed layout — edge-to-edge header, safe body

```xaml
<ContentPage SafeAreaEdges="None">
    <Grid RowDefinitions="Auto,*,Auto">
        <!-- Header: edge-to-edge behind status bar -->
        <Grid BackgroundColor="Primary">
            <Label Text="App Header"
                   TextColor="White"
                   Margin="20,40,20,20" />
        </Grid>

        <!-- Body: respect safe areas -->
        <ScrollView Grid.Row="1" SafeAreaEdges="All">
            <VerticalStackLayout Padding="20">
                <Label Text="Main content" />
            </VerticalStackLayout>
        </ScrollView>

        <!-- Footer: keyboard-aware -->
        <Grid Grid.Row="2"
              SafeAreaEdges="SoftInput"
              BackgroundColor="LightGray"
              Padding="20">
            <Entry Placeholder="Type a message..." />
        </Grid>
    </Grid>
</ContentPage>
```

### 5. Programmatic (C#)

```csharp
var page = new ContentPage
{
    SafeAreaEdges = SafeAreaEdges.All
};

var grid = new Grid
{
    // Per-edge: Container on top/left/right, SoftInput on bottom
    SafeAreaEdges = new SafeAreaEdges(
        left: SafeAreaRegions.Container,
        top: SafeAreaRegions.Container,
        right: SafeAreaRegions.Container,
        bottom: SafeAreaRegions.SoftInput)
};
```

## Blazor Hybrid Apps

Blazor Hybrid apps use `BlazorWebView` inside a MAUI `ContentPage`. Safe area
handling is split between XAML (the page) and CSS (the web content).

### Recommended approach

1. **Set the page to edge-to-edge** (default in .NET 10):

```xaml
<ContentPage SafeAreaEdges="None">
    <BlazorWebView HostPage="wwwroot/index.html">
        <BlazorWebView.RootComponents>
            <RootComponent Selector="#app" ComponentType="{x:Type local:Routes}" />
        </BlazorWebView.RootComponents>
    </BlazorWebView>
</ContentPage>
```

2. **Add `viewport-fit=cover`** in `index.html` to let CSS access safe area insets:

```html
<meta name="viewport" content="width=device-width, initial-scale=1.0,
      maximum-scale=1.0, user-scalable=no, viewport-fit=cover" />
```

3. **Use CSS `env()` functions** for safe area insets:

```css
/* Status bar spacer for iOS */
.status-bar-safe-area {
    display: none;
}

@supports (-webkit-touch-callout: none) {
    .status-bar-safe-area {
        display: flex;
        position: sticky;
        top: 0;
        height: env(safe-area-inset-top);
        background-color: var(--header-bg, #f7f7f7);
        width: 100%;
        z-index: 1;
    }
}

/* General safe area padding */
body {
    padding-top: env(safe-area-inset-top);
    padding-bottom: env(safe-area-inset-bottom);
    padding-left: env(safe-area-inset-left);
    padding-right: env(safe-area-inset-right);
}
```

Available CSS environment variables:
- `env(safe-area-inset-top)` — status bar, notch, Dynamic Island
- `env(safe-area-inset-bottom)` — home indicator, navigation bar
- `env(safe-area-inset-left)` — landscape left edge
- `env(safe-area-inset-right)` — landscape right edge

### Common Blazor Hybrid gotcha

**Do NOT combine XAML safe area padding with CSS safe area padding.** If you set
`SafeAreaEdges="Container"` on the ContentPage AND use `env(safe-area-inset-*)`
in CSS, you get double padding. Choose one approach:

- **XAML approach:** Set `SafeAreaEdges="Container"` or `"All"` on the page/layout
  and do NOT use `env()` in CSS.
- **CSS approach (recommended for Blazor):** Leave the page at `SafeAreaEdges="None"`
  (default) and handle all insets in CSS with `env()`. This gives finer control
  for web-based layouts.

## Platform-Specific Behavior

### iOS & Mac Catalyst

- Safe area insets include: status bar, navigation bar, tab bar, notch/Dynamic
  Island, home indicator
- `SoftInput` includes the keyboard when visible
- Insets update automatically on rotation and UI visibility changes
- `ScrollView` with `Default` maps to `UIScrollViewContentInsetAdjustmentBehavior.Automatic`

**Reading safe area insets at runtime (iOS only):**

```csharp
using Microsoft.Maui.Controls.PlatformConfiguration;
using Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;

Thickness insets = On<iOS>().SafeAreaInsets();
// insets.Top, insets.Bottom, insets.Left, insets.Right
```

**Transparent navigation bar for edge-to-edge under nav bar:**

```xaml
<!-- Shell -->
<Shell Shell.BackgroundColor="#80000000"
       Shell.NavBarHasShadow="False" />

<!-- NavigationPage -->
<NavigationPage BarBackgroundColor="#80000000"
    ios:NavigationPage.HideNavigationBarSeparator="True" />
```

### Android

- Safe area insets include: system bars (status/navigation) and display cutouts
- `SoftInput` includes the soft keyboard
- Behavior varies by Android version and edge-to-edge settings
- MAUI uses `WindowInsetsCompat` and `WindowInsetsAnimationCompat` internally

**Breaking change (.NET 9 → 10):** `ContentPage` default changed from
`Container` → `None`. Set `SafeAreaEdges="Container"` explicitly to restore
.NET 9 behavior.

**WindowSoftInputModeAdjust interaction:** If you were using
`WindowSoftInputModeAdjust.Resize` in .NET 9, you may need
`SafeAreaEdges="All"` on the ContentPage to maintain keyboard avoidance.

## Migration from Legacy APIs

### From ios:Page.UseSafeArea (iOS-only)

```xaml
<!-- .NET 9 (legacy) -->
<ContentPage xmlns:ios="clr-namespace:Microsoft.Maui.Controls.PlatformConfiguration.iOSSpecific;assembly=Microsoft.Maui.Controls"
             ios:Page.UseSafeArea="True">

<!-- .NET 10+ (cross-platform) -->
<ContentPage SafeAreaEdges="Container">
```

### From Layout.IgnoreSafeArea

```xaml
<!-- .NET 9 (legacy) -->
<Grid IgnoreSafeArea="True">

<!-- .NET 10+ -->
<Grid SafeAreaEdges="None">
```

The legacy properties still work but are obsolete. `IgnoreSafeArea="True"` maps
internally to `SafeAreaRegions.None`.

## Common Gotchas

1. **Layouts default to `Container`, not `None`.** For true edge-to-edge, you
   must set `SafeAreaEdges="None"` on BOTH the page AND any layout inside it.

2. **`SoftInput` on ScrollView has no effect.** ScrollView manages its own
   content insets. For keyboard avoidance with ScrollView, wrap it in a Grid or
   StackLayout and set `SafeAreaEdges` on the wrapper.

3. **`Default` is not `None`.** `Default` means "use platform defaults for this
   control type." `None` means "edge-to-edge, no padding." They differ on
   ScrollView especially.

4. **Blazor double-padding.** Don't combine XAML `SafeAreaEdges="Container"` with
   CSS `env(safe-area-inset-*)`. Use one or the other.

5. **Android .NET 9 → 10 regression.** If your Android app's content suddenly
   goes under the status bar after upgrading, add `SafeAreaEdges="Container"` to
   your ContentPage.

6. **Shell/NavigationPage on iOS.** Content won't extend behind the nav bar
   unless you set a transparent background color AND hide the separator line.

## Best Practices

1. **Choose the right value:**
   - `All` — forms, critical inputs that must always be visible
   - `None` — photo viewers, video players, games, background images
   - `Container` — scrollable content with fixed headers/footers
   - `SoftInput` — messaging/chat UIs with bottom input bars

2. **Test on multiple devices:**
   - Devices with notches (iPhone X+, Android cutouts)
   - Tablets in landscape
   - Different aspect ratios and screen sizes

3. **Combine SafeAreaEdges with Padding:** `SafeAreaEdges` controls automatic
   safe area insets. Add your own `Padding` for visual spacing on top:

   ```xaml
   <ContentPage SafeAreaEdges="All">
       <VerticalStackLayout Padding="20">
           <!-- Both safe area and visual padding applied -->
       </VerticalStackLayout>
   </ContentPage>
   ```

4. **Use per-control settings** to create layouts where different sections have
   different safe area behavior (edge-to-edge header + safe body + keyboard-aware footer).

5. **For Blazor Hybrid apps**, prefer the CSS `env()` approach and leave the
   ContentPage at `SafeAreaEdges="None"`.

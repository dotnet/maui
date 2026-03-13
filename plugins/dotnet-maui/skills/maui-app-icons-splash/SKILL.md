---
name: maui-app-icons-splash
description: >
  .NET MAUI app icon configuration, splash screen setup, SVG to PNG conversion
  at build time, composed/adaptive icons, and platform-specific icon and splash
  screen requirements for Android, iOS, Mac Catalyst, and Windows.
  USE FOR: "app icon", "splash screen", "MauiIcon", "MauiSplashScreen", "adaptive icon",
  "change app icon", "launch screen", "SVG icon", "foreground tint", "icon resize".
  DO NOT USE FOR: in-app images or image loading (use maui-performance),
  theming or colors (use maui-theming), or custom drawing (use maui-graphics-drawing).
---

# .NET MAUI App Icons & Splash Screens

Use this skill when configuring app icons, splash screens, or troubleshooting
image asset issues in .NET MAUI projects.

---

## App Icons

### Single-source icon

Add one `MauiIcon` item in the `.csproj`; the build resizes it for every platform:

```xml
<MauiIcon Include="Resources\AppIcon\appicon.svg"
          Color="#512BD4" />
```

- **Source**: SVG (recommended) or PNG. SVGs are converted to platform PNGs at build.
- **Color**: Background fill behind the icon (hex or named color).
- **BaseSize**: Logical size before platform scaling (default `456,456` for icons).
- **TintColor**: Recolors the foreground of a single-layer icon.

### Composed / layered icons

Use `Include` (background) + `ForegroundFile` (foreground) for a two-layer icon:

```xml
<MauiIcon Include="Resources\AppIcon\appicon.svg"
          ForegroundFile="Resources\AppIcon\appiconfg.svg"
          ForegroundScale="0.65"
          Color="#512BD4" />
```

- **ForegroundFile**: SVG/PNG overlaid on the background.
- **ForegroundScale**: Scale factor for the foreground layer (0.0–1.0).
- Composed icons automatically produce **Android adaptive icons** (API 26+).

### Platform-specific references

| Platform      | Where the icon is referenced                                   |
|---------------|----------------------------------------------------------------|
| Android       | `AndroidManifest.xml` → `android:icon="@mipmap/appicon"`      |
| iOS / Mac Cat | `Info.plist` → `XSAppIconAssets = "Assets.xcassets/appicon.appiconset"` |
| Windows       | Auto-configured via the build; no manual reference needed      |

### Sizing guidelines

- Provide the largest resolution you need; the tooling scales down.
- For SVG sources, `BaseSize` controls the logical canvas size.
- Android generates `mdpi` through `xxxhdpi` variants automatically.
- iOS generates all required `@1x`, `@2x`, `@3x` sizes.

---

## Splash Screens

### Basic splash screen

Add one `MauiSplashScreen` item in the `.csproj`:

```xml
<MauiSplashScreen Include="Resources\Splash\splash.svg"
                  Color="#512BD4"
                  BaseSize="128,128" />
```

- **Color**: Background color of the splash screen.
- **BaseSize**: Logical size of the splash image (default `128,128`).
- **TintColor**: Recolors the splash image foreground.

### Platform behavior

**Android (12+)**
- The splash icon is centered on a colored background.
- Uses `@style/Maui.SplashTheme` set in `AndroidManifest.xml`:
  ```xml
  <activity android:theme="@style/Maui.SplashTheme" ... />
  ```
- Pre-Android 12 uses the same theme with a legacy splash.

**iOS / Mac Catalyst**
- Build generates `MauiSplash.storyboard` in the app bundle.
- Referenced automatically in `Info.plist` (`UILaunchStoryboardName`).

**Windows**
- The splash image is embedded in the app package; no extra config needed.

### SVG to PNG conversion

- All SVG sources (`MauiIcon`, `MauiSplashScreen`, `MauiImage`) are rasterized
  to PNGs at build time by the `Microsoft.Maui.Resizetizer` MSBuild task.
- Conversion respects `BaseSize`, `TintColor`, and `Color` properties.
- To skip resizing for a pre-made PNG, set `Resize="false"`.
- If an SVG renders incorrectly, simplify paths or supply a PNG instead.

---

## Image sizing best practices

1. **Use SVG whenever possible** — one file, infinite scaling, smaller repo.
2. **Set `BaseSize` explicitly** when your SVG viewBox doesn't match the
   desired logical size.
3. **Keep splash images simple** — large or complex SVGs slow build and may
   render with artifacts.
4. **Test on real devices** — emulator densities can mask icon cropping,
   especially Android adaptive icon safe zones.
5. **Avoid transparency in splash `Color`** — some platforms ignore alpha and
   show black instead.

---

## Quick troubleshooting

| Symptom                        | Likely fix                                        |
|--------------------------------|---------------------------------------------------|
| Icon appears blank or white    | Check `Color` vs `TintColor`; ensure SVG paths exist |
| Splash shows default .NET logo | Verify `MauiSplashScreen` item is in the `.csproj`  |
| Android icon looks clipped     | Use composed icon with `ForegroundScale="0.65"`    |
| Build error on SVG             | Simplify SVG or switch to PNG with `Resize="false"` |

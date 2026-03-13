---
name: maui-localization
description: >-
  Guidance for localizing .NET MAUI apps: multi-language support via .resx resource files,
  culture resolution and runtime switching, RTL layout, platform language declarations
  (iOS/Mac Catalyst Info.plist, Windows Package.appxmanifest), and image localization strategies.
  USE FOR: "localization", "multi-language", "resx resource", "translate app", "RTL layout",
  "culture switching", "localize strings", "right-to-left", "language support MAUI",
  "Info.plist languages".
  DO NOT USE FOR: theming or visual styles (use maui-theming), accessibility labels
  (use maui-accessibility), or content from REST APIs (use maui-rest-api).
---

# .NET MAUI Localization

## Resource files (.resx)

.NET MAUI uses standard .NET resource files for localization. Each `.resx` file contains name/value string pairs.

### File naming convention

| File | Purpose |
|---|---|
| `AppResources.resx` | Default (fallback) language |
| `AppResources.es.resx` | Spanish (neutral) |
| `AppResources.fr-FR.resx` | French – France (specific) |
| `AppResources.zh-Hans.resx` | Chinese Simplified |

Place resource files in a `Resources/Strings` folder or project root. The naming pattern is `{BaseName}.{CultureCode}.resx`.

### Project configuration

Set the neutral language in `.csproj` so the `ResourceManager` resolves the default culture correctly:

```xml
<PropertyGroup>
  <NeutralLanguage>en-US</NeutralLanguage>
</PropertyGroup>
```

> **WARNING** — Without `NeutralLanguage` set, `ResourceManager` may return `null` for default-culture lookups at runtime. Always set it.

### Generated accessor class

The default `.resx` file auto-generates a strongly-typed class (typically `AppResources`) with static properties for each string key:

```csharp
// Auto-generated — do not edit manually
public static string WelcomeMessage => ResourceManager.GetString("WelcomeMessage", resourceCulture);
```

Access strings in C#: `string welcome = AppResources.WelcomeMessage;`

## Culture resolution order

The runtime resolves resources in this order:

1. **Specific culture** — e.g. `en-US` → `AppResources.en-US.resx`
2. **Neutral culture** — e.g. `en` → `AppResources.en.resx`
3. **Default (fallback)** — `AppResources.resx`

If no match is found at any level, the fallback file is used.

## XAML usage

### Using x:Static

```xml
<ContentPage xmlns:resx="clr-namespace:MyApp.Resources.Strings">

  <Label Text="{x:Static resx:AppResources.WelcomeMessage}" />

</ContentPage>
```

### Using a binding with a localization service

For runtime language switching without restarting, expose resource strings through a helper that raises `PropertyChanged`:

```csharp
public class LocalizationResourceManager : INotifyPropertyChanged
{
    public static LocalizationResourceManager Instance { get; } = new();

    public string this[string key] =>
        AppResources.ResourceManager.GetString(key, AppResources.Culture)!;

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SetCulture(CultureInfo culture)
    {
        AppResources.Culture = culture;
        CultureInfo.CurrentUICulture = culture;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
    }
}
```

```xml
<Label Text="{Binding [WelcomeMessage], Source={x:Static local:LocalizationResourceManager.Instance}}" />
```

## Runtime culture switching

Change the UI culture at runtime:

```csharp
var culture = new CultureInfo("es");
CultureInfo.CurrentUICulture = culture;
CultureInfo.CurrentCulture = culture; // for dates/numbers
AppResources.Culture = culture;
```

After setting the culture, UI elements bound via `x:Static` will **not** update automatically. Use the binding approach above or navigate to a new page to pick up the change.

## Platform declarations

### iOS and Mac Catalyst

Add supported localizations to `Platforms/iOS/Info.plist` (and `Platforms/MacCatalyst/Info.plist`):

```xml
<key>CFBundleLocalizations</key>
<array>
  <string>en</string>
  <string>es</string>
  <string>fr</string>
</array>
```

Without this, iOS will not offer the app's languages in system Settings and may ignore culture overrides.

### Windows

Declare supported languages in `Platforms/Windows/Package.appxmanifest`:

```xml
<Resources>
  <Resource Language="en-US" />
  <Resource Language="es" />
  <Resource Language="fr-FR" />
</Resources>
```

### Android

Android picks up `.resx`-based localization automatically. No additional manifest entries are required.

## RTL layout support

Set `FlowDirection` to support right-to-left languages (Arabic, Hebrew, etc.):
```xml
<!-- App-wide -->
<Application FlowDirection="RightToLeft" />

<!-- Per-page or per-element -->
<ContentPage FlowDirection="RightToLeft">
  <StackLayout FlowDirection="MatchParent">
    <Label Text="{x:Static resx:AppResources.Greeting}" />
  </StackLayout>
</ContentPage>
```

Detect and apply at runtime:

```csharp
bool isRtl = CultureInfo.CurrentUICulture.TextInfo.IsRightToLeft;
FlowDirection = isRtl ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
```

## Image localization

For culture-specific images, use a naming or folder convention and select at runtime:

```csharp
string cultureSuffix = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
string imageName = $"banner_{cultureSuffix}.png";
// Fall back to default if culture-specific image doesn't exist
bannerImage.Source = ImageSource.FromFile(
    FileSystem.AppPackageFileExistsAsync(imageName).Result ? imageName : "banner.png");
```

Alternatively, reference image paths in `.resx` files so each culture points to its own asset.

## VS Code setup

When using VS Code (not Visual Studio), the auto-generated `.Designer.cs` file for `.resx` may not regenerate on save. Ensure `DesignTimeBuild` is enabled:

```xml
<PropertyGroup>
  <CoreCompileDependsOn>PrepareResources;$(CoreCompileDependsOn)</CoreCompileDependsOn>
</PropertyGroup>
```

Run `dotnet build` after adding or modifying `.resx` entries to regenerate the accessor class.

## Quick checklist

- [ ] `NeutralLanguage` set in `.csproj`
- [ ] Default `AppResources.resx` contains all keys
- [ ] Each target language has its own `AppResources.{culture}.resx`
- [ ] iOS/Mac: `CFBundleLocalizations` lists all supported languages
- [ ] Windows: `Package.appxmanifest` declares `<Resource Language="..." />`
- [ ] RTL cultures set `FlowDirection` appropriately
- [ ] `dotnet build` regenerates `.Designer.cs` after `.resx` changes

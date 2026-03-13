---
name: maui-maps
description: >
  Guidance for adding map controls, pins, polygons, polylines, geocoding,
  Google Maps API key configuration, and platform setup in .NET MAUI apps
  using Microsoft.Maui.Controls.Maps.
  USE FOR: "add map", "map control", "map pins", "polygons on map", "polylines",
  "geocoding", "Google Maps API key", "Microsoft.Maui.Controls.Maps", "map setup",
  "reverse geocoding", "map region".
  DO NOT USE FOR: getting device GPS coordinates (use maui-geolocation),
  custom drawing overlays (use maui-graphics-drawing), or location permissions only (use maui-permissions).
---

# .NET MAUI Maps Skill

## NuGet Package

Install **Microsoft.Maui.Controls.Maps** (and on Windows, **CommunityToolkit.Maui.Maps**):

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Maui.Controls.Maps" Version="$(MauiVersion)" />
  <!-- Windows only – Bing Maps via Community Toolkit -->
  <PackageReference Include="CommunityToolkit.Maui.Maps"
                    Version="*"
                    Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'windows'" />
</ItemGroup>
```

## MauiProgram.cs Setup

```csharp
using Microsoft.Maui.Controls.Hosting;
using Microsoft.Maui.Hosting;

public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder
        .UseMauiApp<App>()
        .UseMauiMaps();           // <-- required
#if WINDOWS
        .UseMauiCommunityToolkitMaps("YOUR_BING_MAPS_KEY");
#endif

    return builder.Build();
}
```

## Platform Setup

### Android – Google Maps API Key

1. Obtain a key from the [Google Cloud Console](https://console.cloud.google.com/) with the **Maps SDK for Android** API enabled.
2. Add the key to `Platforms/Android/AndroidManifest.xml`:

```xml
<application ...>
  <meta-data android:name="com.google.android.geo.API_KEY"
             android:value="YOUR_GOOGLE_MAPS_KEY" />
</application>
```

3. Ensure Google Play Services version meta-data is present:

```xml
<meta-data android:name="com.google.android.gms.version"
           android:value="@integer/google_play_services_version" />
```

4. For Android 11+ (API 30), add package visibility queries so the app can launch external map intents:

```xml
<queries>
  <intent>
    <action android:name="android.intent.action.VIEW" />
    <data android:scheme="geo" />
  </intent>
</queries>
```

5. Required permissions (usually auto-merged): `ACCESS_FINE_LOCATION`, `ACCESS_COARSE_LOCATION`, `INTERNET`.

### iOS / Mac Catalyst

Add to `Platforms/iOS/Info.plist` (and `Platforms/MacCatalyst/Info.plist`):

```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>This app needs your location to show it on the map.</string>
```

### Windows

Maps are **not** natively supported by `Microsoft.Maui.Controls.Maps` on Windows.
Use **CommunityToolkit.Maui.Maps** which renders Bing Maps in a WebView2 control.

## Avoiding the `Map` Name Conflict

`Microsoft.Maui.Controls.Maps.Map` can conflict with `Microsoft.Maui.ApplicationModel.Map`.
Use a namespace alias in files that reference both:

```csharp
using Map = Microsoft.Maui.Controls.Maps.Map;
```

Or in XAML, add a namespace prefix:

```xml
xmlns:maps="clr-namespace:Microsoft.Maui.Controls.Maps;assembly=Microsoft.Maui.Controls.Maps"
<maps:Map ... />
```

## Core Map Properties

| Property | Type | Description |
|---|---|---|
| `MapType` | `MapType` | `Street`, `Satellite`, or `Hybrid` |
| `IsShowingUser` | `bool` | Show the user's current location |
| `IsScrollEnabled` | `bool` | Allow panning |
| `IsZoomEnabled` | `bool` | Allow zoom gestures |
| `IsTrafficEnabled` | `bool` | Show traffic overlay |

```xml
<maps:Map MapType="Hybrid"
          IsShowingUser="True"
          IsScrollEnabled="True"
          IsZoomEnabled="True"
          IsTrafficEnabled="False" />
```

## Pins

```csharp
var pin = new Pin
{
    Label = "Microsoft HQ",
    Address = "One Microsoft Way, Redmond, WA",
    Location = new Location(47.6423, -122.1391),
    Type = PinType.Place   // Place | Generic | SearchResult | SavedPin
};
pin.MarkerClicked += (s, e) => { e.HideInfoWindow = true; };
pin.InfoWindowClicked += (s, e) => { /* navigate or show details */ };
map.Pins.Add(pin);
```

## Map Elements – Polygon, Polyline, Circle

```csharp
// Polygon
var polygon = new Polygon
{
    StrokeColor = Colors.Blue,
    StrokeWidth = 2,
    FillColor = Color.FromRgba(0, 0, 255, 64)
};
polygon.Geopath.Add(new Location(47.64, -122.13));
polygon.Geopath.Add(new Location(47.65, -122.13));
polygon.Geopath.Add(new Location(47.65, -122.14));
map.MapElements.Add(polygon);

// Polyline
var polyline = new Polyline
{
    StrokeColor = Colors.Red,
    StrokeWidth = 5
};
polyline.Geopath.Add(new Location(47.64, -122.13));
polyline.Geopath.Add(new Location(47.65, -122.14));
map.MapElements.Add(polyline);

// Circle
var circle = new Circle
{
    Center = new Location(47.64, -122.13),
    Radius = new Distance(500),
    StrokeColor = Colors.Green,
    FillColor = Color.FromRgba(0, 128, 0, 64)
};
map.MapElements.Add(circle);
```

## Moving the Map Viewport

```csharp
var center = new Location(47.6423, -122.1391);
var span = MapSpan.FromCenterAndRadius(center, Distance.FromKilometers(2));
map.MoveToRegion(span);
```

## MapClicked Event

```csharp
map.MapClicked += (s, e) =>
{
    var location = e.Location;   // Location (lat/lon)
    System.Diagnostics.Debug.WriteLine(
        $"Map clicked at {location.Latitude}, {location.Longitude}");
};
```

## Data-Bound Pins (ItemsSource / ItemTemplate)

```xml
<maps:Map ItemsSource="{Binding Locations}">
    <maps:Map.ItemTemplate>
        <DataTemplate>
            <maps:Pin Label="{Binding Name}"
                      Address="{Binding Description}"
                      Location="{Binding Position}" />
        </DataTemplate>
    </maps:Map.ItemTemplate>
</maps:Map>
```

```csharp
public class LocationViewModel
{
    public string Name { get; set; }
    public string Description { get; set; }
    public Location Position { get; set; }
}
```

## Geocoding Integration

Use `Microsoft.Maui.Devices.Sensors.Geocoding` to convert addresses ↔ coordinates:

```csharp
var locations = await Geocoding.Default.GetLocationsAsync("Redmond, WA");
var location = locations?.FirstOrDefault();
if (location is not null)
{
    map.MoveToRegion(MapSpan.FromCenterAndRadius(
        new Location(location.Latitude, location.Longitude),
        Distance.FromKilometers(5)));
}
```

Reverse geocoding:

```csharp
var placemarks = await Geocoding.Default.GetPlacemarksAsync(47.64, -122.13);
var placemark = placemarks?.FirstOrDefault();
// placemark.Locality, placemark.AdminArea, placemark.CountryName, etc.
```

## Quick-Reference Checklist

- [ ] NuGet: `Microsoft.Maui.Controls.Maps` added
- [ ] `.UseMauiMaps()` called in `MauiProgram.cs`
- [ ] Android: Google Maps API key in `AndroidManifest.xml`
- [ ] iOS/Mac Catalyst: `NSLocationWhenInUseUsageDescription` in `Info.plist`
- [ ] Windows: `CommunityToolkit.Maui.Maps` + Bing Maps key (if needed)
- [ ] Namespace alias to avoid `Map` conflict

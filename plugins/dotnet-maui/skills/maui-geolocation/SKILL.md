---
name: maui-geolocation
description: >
  Add geolocation capabilities to .NET MAUI apps using Microsoft.Maui.Devices.Sensors.
  Covers one-shot and continuous location, platform permissions (Android, iOS, macOS, Windows),
  accuracy levels, CancellationToken usage, mock-location detection, and a DI-friendly service wrapper.
  USE FOR: "geolocation", "GPS location", "get current location", "location permission",
  "continuous location", "location tracking", "GeolocationRequest", "mock location",
  "latitude longitude MAUI".
  DO NOT USE FOR: displaying locations on a map (use maui-maps),
  general permission handling (use maui-permissions), or geocoding addresses (use maui-maps).
---

# .NET MAUI Geolocation

## Platform permissions

### Android

Add to `Platforms/Android/AndroidManifest.xml` inside `<manifest>`:

```xml
<uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
<uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
<!-- Android 10+ background location (only if needed) -->
<uses-permission android:name="android.permission.ACCESS_BACKGROUND_LOCATION" />
```

### iOS

Add to `Platforms/iOS/Info.plist`:

```xml
<key>NSLocationWhenInUseUsageDescription</key>
<string>This app needs your location to provide nearby results.</string>
```

For full-accuracy prompts on iOS 14+, also add:

```xml
<key>NSLocationTemporaryUsageDescriptionDictionary</key>
<dict>
  <key>FullAccuracyUsageKey</key>
  <string>This app needs precise location for turn-by-turn directions.</string>
</dict>
```

### macOS (Mac Catalyst)

Add to `Platforms/MacCatalyst/Entitlements.plist`:

```xml
<key>com.apple.security.personal-information.location</key>
<true/>
```

### Windows

No manifest changes required. Location capability is enabled by default.

## Core API — `Geolocation.Default`

| Method | Returns | Purpose |
|--------|---------|---------|
| `GetLastKnownLocationAsync()` | `Location?` | Cached device location (fast, may be stale) |
| `GetLocationAsync(GeolocationRequest, CancellationToken)` | `Location?` | Fresh GPS fix with desired accuracy |
| `StartListeningForegroundAsync(GeolocationListeningRequest)` | `bool` | Begin continuous location updates |
| `StopListeningForeground()` | `void` | Stop continuous updates |

### One-shot location

```csharp
try
{
    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
    var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));
    var location = await Geolocation.Default.GetLocationAsync(request, cts.Token);

    if (location is null)
    {
        // Location unavailable — GPS off, permissions denied, or timeout
        return;
    }

    Console.WriteLine($"{location.Latitude}, {location.Longitude} ±{location.Accuracy}m");
}
catch (FeatureNotSupportedException)
{
    // Device lacks GPS hardware
}
catch (PermissionException)
{
    // Location permission not granted
}
```

Always check for `null` — the method returns `null` when the device cannot obtain a fix.

### Continuous listening

```csharp
public partial class TrackingViewModel : ObservableObject
{
    [ObservableProperty] Location? currentLocation;

    public async Task StartTracking()
    {
        Geolocation.Default.LocationChanged += OnLocationChanged;
        var request = new GeolocationListeningRequest(GeolocationAccuracy.High, TimeSpan.FromSeconds(5));
        var success = await Geolocation.Default.StartListeningForegroundAsync(request);
        if (!success)
            Geolocation.Default.LocationChanged -= OnLocationChanged;
    }

    public void StopTracking()
    {
        Geolocation.Default.StopListeningForeground();
        Geolocation.Default.LocationChanged -= OnLocationChanged;
    }

    void OnLocationChanged(object? sender, GeolocationLocationChangedEventArgs e)
    {
        CurrentLocation = e.Location;
    }
}
```

## GeolocationAccuracy levels

| Enum value | Android (m) | iOS (m) | Windows (m) |
|------------|-------------|---------|-------------|
| `Lowest` | 500 | 3000 | 1000–5000 |
| `Low` | 500 | 1000 | 300–3000 |
| `Medium` | 100–500 | 100 | 30–500 |
| `High` | 0–100 | 10 | ≤30 |
| `Best` | 0–100 | ~0 | ≤10 |

Higher accuracy consumes more battery. Use the lowest level that satisfies your feature.

## CancellationToken pattern

Always pass a `CancellationToken` to `GetLocationAsync` to avoid indefinite hangs:

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
var location = await Geolocation.Default.GetLocationAsync(
    new GeolocationRequest(GeolocationAccuracy.High), cts.Token);
```

## DI-friendly service wrapper

Register `IGeolocation` in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<IGeolocation>(Geolocation.Default);
builder.Services.AddSingleton<LocationService>();
```

Consume via constructor injection:

```csharp
public class LocationService(IGeolocation geolocation)
{
    public async Task<Location?> GetCurrentAsync(CancellationToken ct = default)
    {
        var cached = await geolocation.GetLastKnownLocationAsync();
        if (cached is not null && cached.Timestamp > DateTimeOffset.UtcNow.AddMinutes(-5))
            return cached;

        return await geolocation.GetLocationAsync(
            new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10)), ct);
    }
}
```

## Platform gotchas

- **iOS 14 reduced accuracy**: Users can grant "approximate" location. Check `location.Accuracy` — values > 100 m likely indicate reduced precision. Use `GeolocationRequest.RequestFullAccuracy` with a matching key from `NSLocationTemporaryUsageDescriptionDictionary` to prompt for full accuracy.
- **Mock locations (`IsFromMockProvider`)**: On Android, `location.IsFromMockProvider` is `true` when a mock-location app is active. Always check this in security-sensitive flows.
- **Altitude 0.0 on Android**: Some Android devices return `0.0` for `Altitude` when GPS has no barometric sensor. Treat `0.0` as "unknown" rather than sea level.
- **Null returns**: `GetLastKnownLocationAsync` returns `null` on first boot or after a location-data reset. Always fall back to `GetLocationAsync`.
- **Permissions at runtime**: Call `Permissions.RequestAsync<Permissions.LocationWhenInUse>()` before any geolocation call, or handle `PermissionException`.
- **Background location on Android 10+**: `ACCESS_BACKGROUND_LOCATION` must be requested separately from foreground permissions and triggers a distinct system dialog.

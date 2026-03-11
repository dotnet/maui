---
name: maui-platform-invoke
description: >
  Guidance for calling platform-specific native APIs from .NET MAUI apps.
  Covers partial classes, conditional compilation, multi-targeting configuration,
  and dependency injection patterns for cross-platform code that needs
  Android, iOS, Mac Catalyst, or Windows functionality.
  USE FOR: "platform-specific code", "conditional compilation", "partial class",
  "#if ANDROID", "#if IOS", "multi-targeting", "native API call", "platform invoke",
  "platform-specific DI", "DeviceInfo.Platform".
  DO NOT USE FOR: custom control handlers (use maui-custom-handlers),
  permission requests (use maui-permissions), or dependency injection patterns (use maui-dependency-injection).
---

# Platform Invoke in .NET MAUI

Two approaches exist for invoking platform-specific APIs from shared code.

---

## 1. Conditional Compilation

Use preprocessor directives for small, inline platform code:

```csharp
public string GetDeviceName()
{
#if ANDROID
    return Android.OS.Build.Model;
#elif IOS || MACCATALYST
    return UIKit.UIDevice.CurrentDevice.Name;
#elif WINDOWS
    return Windows.Security.ExchangeActiveSyncProvisioning
        .EasClientDeviceInformation().FriendlyName;
#else
    return "Unknown";
#endif
}
```

Best for: one-off calls, quick checks, small blocks. Avoid for complex logic—use partial classes instead.

---

## 2. Partial Classes (Preferred)

Split a service into a cross-platform definition and per-platform implementations.

### Cross-platform definition

`Services/DeviceOrientationService.cs`:

```csharp
namespace MyApp.Services;

public partial class DeviceOrientationService
{
    public partial DeviceOrientation GetOrientation();
}

public enum DeviceOrientation
{
    Undefined, Portrait, Landscape
}
```

### Platform implementations

`Platforms/Android/Services/DeviceOrientationService.cs`:

```csharp
namespace MyApp.Services;

public partial class DeviceOrientationService
{
    public partial DeviceOrientation GetOrientation()
    {
        var activity = Platform.CurrentActivity
            ?? throw new InvalidOperationException("No current activity.");
        var rotation = activity.WindowManager?.DefaultDisplay?.Rotation;
        return rotation is SurfaceOrientation.Rotation90
                        or SurfaceOrientation.Rotation270
            ? DeviceOrientation.Landscape
            : DeviceOrientation.Portrait;
    }
}
```

`Platforms/iOS/Services/DeviceOrientationService.cs`:

```csharp
namespace MyApp.Services;

public partial class DeviceOrientationService
{
    public partial DeviceOrientation GetOrientation()
    {
        var orientation = UIKit.UIDevice.CurrentDevice.Orientation;
        return orientation is UIKit.UIDeviceOrientation.LandscapeLeft
                           or UIKit.UIDeviceOrientation.LandscapeRight
            ? DeviceOrientation.Landscape
            : DeviceOrientation.Portrait;
    }
}
```

### Key rules

- **Same namespace** across all partial files.
- The build system auto-includes only the file matching the target platform from `Platforms/{Platform}/`.
- No `#if` needed—MSBuild handles it.

---

## Multi-Targeting Configuration

The default `.csproj` already multi-targets. To add custom file-based patterns:

```xml
<!-- Include files matching *.android.cs only for Android -->
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">
  <Compile Include="**\*.android.cs" />
</ItemGroup>
```

You can also use folder-based conventions beyond `Platforms/`:

```xml
<ItemGroup Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">
  <Compile Include="iOS\**\*.cs" />
</ItemGroup>
```

---

## Dependency Injection Registration

Register platform-specific implementations in `MauiProgram.cs`:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder.UseMauiApp<App>();

    // Interface-based (recommended for testability)
    builder.Services.AddSingleton<IDeviceOrientationService, DeviceOrientationService>();

    // Platform-specific registrations when implementations differ by type
#if ANDROID
    builder.Services.AddSingleton<IPlatformNotifier, AndroidNotifier>();
#elif IOS || MACCATALYST
    builder.Services.AddSingleton<IPlatformNotifier, AppleNotifier>();
#elif WINDOWS
    builder.Services.AddSingleton<IPlatformNotifier, WindowsNotifier>();
#endif

    return builder.Build();
}
```

**Prefer interfaces** (`IDeviceOrientationService`) so shared code depends on abstractions, enabling unit testing with mocks.

---

## Android Java Interop Basics

Access Android APIs directly via C# bindings in the `Android.*` namespaces:

```csharp
// Get a system service
var connectivityManager = (Android.Net.ConnectivityManager)
    Platform.CurrentActivity!
        .GetSystemService(Android.Content.Context.ConnectivityService)!;

// Check network
var network = connectivityManager.ActiveNetwork;
var capabilities = connectivityManager.GetNetworkCapabilities(network);
bool hasWifi = capabilities?.HasTransport(
    Android.Net.TransportType.Wifi) ?? false;
```

For APIs without existing bindings, use Java Native Interface via `Java.Interop` or create an Android Binding Library.

---

## Quick Reference

| Approach | When to use |
|---|---|
| `#if ANDROID` | Small inline checks, 1–5 lines |
| Partial classes | Services, complex logic, testable code |
| Interface + DI | Swappable implementations, unit testing |
| Custom file patterns | Teams preferring `*.android.cs` naming |

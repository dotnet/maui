---
name: maui-permissions
description: >
  .NET MAUI runtime permissions guidance — checking and requesting permissions,
  PermissionStatus handling, custom permissions via BasePlatformPermission,
  platform-specific manifest/plist declarations, and DI-friendly service patterns.
  USE FOR: "request permission", "check permission", "PermissionStatus", "runtime permission",
  "BasePlatformPermission", "custom permission", "Android manifest permission",
  "Info.plist permission", "permission denied handling".
  DO NOT USE FOR: geolocation-specific permissions (use maui-geolocation),
  camera/photo permissions (use maui-media-picker), or notification permissions (use maui-local-notifications).
---

# .NET MAUI Permissions

## Core API

```csharp
using Microsoft.Maui.ApplicationModel;

// Check current status
PermissionStatus status = await Permissions.CheckStatusAsync<Permissions.Camera>();

// Request permission
status = await Permissions.RequestAsync<Permissions.Camera>();

// Android: check if rationale should be shown after prior denial
bool showRationale = Permissions.ShouldShowRationale<Permissions.Camera>();
```

## PermissionStatus Enum

| Value        | Meaning                                                |
|--------------|--------------------------------------------------------|
| `Unknown`    | Status unknown or not supported on platform            |
| `Denied`     | User denied the permission                             |
| `Disabled`   | Feature is disabled on the device                      |
| `Granted`    | User granted permission                                |
| `Restricted` | Permission restricted by policy (iOS parental, etc.)   |
| `Limited`    | Partial access granted (iOS limited photo access)      |

## Always-Check-Before-Request Pattern

```csharp
public async Task<PermissionStatus> CheckAndRequestPermissionAsync<T>() where T : Permissions.BasePermission, new()
{
    var status = await Permissions.CheckStatusAsync<T>();
    if (status == PermissionStatus.Granted)
        return status;

    if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
    {
        // iOS only allows one request; after denial user must go to Settings
        return status;
    }

    if (Permissions.ShouldShowRationale<T>())
    {
        // Show UI explaining why the permission is needed before re-requesting
        await Shell.Current.DisplayAlert("Permission needed",
            "This feature requires the requested permission.", "OK");
    }

    return await Permissions.RequestAsync<T>();
}
```

## Available Permissions

`Battery`, `Bluetooth`, `CalendarRead`, `CalendarWrite`, `Camera`,
`ContactsRead`, `ContactsWrite`, `Flashlight`, `LocationWhenInUse`,
`LocationAlways`, `Media`, `Microphone`, `NearbyWifiDevices`,
`NetworkState`, `Phone`, `Photos`, `PhotosAddOnly`, `PhotosReadWrite`,
`PostNotifications`, `Reminders`, `Sensors`, `Sms`, `Speech`,
`StorageRead`, `StorageWrite`, `Vibrate`

Access via `Permissions.<Name>`, e.g. `Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>()`.

## Custom Permissions

Extend `BasePlatformPermission` and override platform-specific required permissions:

```csharp
public class ReadExternalStoragePermission : Permissions.BasePlatformPermission
{
#if ANDROID
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions =>
        new (string, bool)[]
        {
            ("android.permission.READ_EXTERNAL_STORAGE", true)
        };
#endif
}

// Usage
var status = await Permissions.RequestAsync<ReadExternalStoragePermission>();
```

## Platform Notes

### Android
- Permission requests show a system dialog; `ShouldShowRationale` returns `true`
  after the user has previously denied (but not "Don't ask again").
- API 33+ (Android 13): `StorageRead` and `StorageWrite` always return `Granted`
  because scoped storage removes the need for broad storage permissions.
  Use `Media`, `Photos`, or `PhotosReadWrite` for media access instead.
- Declare permissions in `Platforms/Android/AndroidManifest.xml`:
  ```xml
  <uses-permission android:name="android.permission.CAMERA" />
  <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
  ```

### iOS
- The system permission dialog is shown **only once**. If the user denies,
  subsequent `RequestAsync` calls return `Denied` immediately.
  Guide users to **Settings → App → Permission** to re-enable.
- Declare usage descriptions in `Platforms/iOS/Info.plist`:
  ```xml
  <key>NSCameraUsageDescription</key>
  <string>This app needs camera access to take photos.</string>
  <key>NSLocationWhenInUseUsageDescription</key>
  <string>This app needs your location for nearby search.</string>
  ```

### Windows
- Most permissions return `Granted` or `Unknown`. Declare capabilities in
  `Platforms/Windows/Package.appxmanifest` under `<Capabilities>`.

### Mac Catalyst
- Follows iOS patterns. Add usage descriptions to `Info.plist` and
  entitlements to `Entitlements.plist` as needed.

## DI-Friendly Permission Service

```csharp
public interface IPermissionService
{
    Task<PermissionStatus> CheckAndRequestAsync<T>() where T : Permissions.BasePermission, new();
}

public class PermissionService : IPermissionService
{
    public async Task<PermissionStatus> CheckAndRequestAsync<T>() where T : Permissions.BasePermission, new()
    {
        var status = await Permissions.CheckStatusAsync<T>();
        if (status == PermissionStatus.Granted)
            return status;

        if (Permissions.ShouldShowRationale<T>())
        {
            await Shell.Current.DisplayAlert("Permission required",
                "Please grant the requested permission to use this feature.", "OK");
        }

        return await Permissions.RequestAsync<T>();
    }
}

// Registration
builder.Services.AddSingleton<IPermissionService, PermissionService>();
```

## Key Rules

1. **Always call `CheckStatusAsync` before `RequestAsync`** — avoids unnecessary prompts.
2. **Never call permission APIs from a constructor** — use `OnAppearing` or a command.
3. **Handle every `PermissionStatus` value** — especially `Denied` and `Restricted`.
4. **Android**: use `ShouldShowRationale` to show explanatory UI before re-requesting.
5. **iOS**: plan for one-shot request; provide Settings navigation for denied permissions.
6. **Scoped storage (API 33+)**: stop requesting `StorageRead`/`StorageWrite`; use media permissions.

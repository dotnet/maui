---
name: maui-push-notifications
description: >
  End-to-end guide for adding push notifications to .NET MAUI apps.
  Covers Firebase Cloud Messaging (Android), Apple Push Notification Service (iOS),
  Azure Notification Hubs as the cross-platform broker, an ASP.NET Core backend API,
  and the MAUI client wiring on every platform.
  USE FOR: "push notification", "FCM", "APNS", "Firebase Cloud Messaging",
  "Azure Notification Hubs", "device registration", "remote notification",
  "send push notification", "notification token".
  DO NOT USE FOR: local/scheduled notifications (use maui-local-notifications),
  general permission handling (use maui-permissions), or background tasks without notifications (use maui-app-lifecycle).
---

# Push Notifications for .NET MAUI

## Architecture

MAUI app → ASP.NET Core backend → Azure Notification Hub → FCM (Android) / APNS (iOS) → device.

## Step 1 — Create Azure Notification Hub

1. Azure Portal → create a **Notification Hub** inside a **Notification Hub Namespace**.
2. **Apple (APNS)** → upload `.p8` key or `.p12` cert; set mode to **Sandbox** for dev.
3. **Google (FCM V1)** → paste the **FCM V1 service-account JSON** from Firebase Console → Project Settings → Cloud Messaging.
4. Copy `DefaultFullSharedAccessSignature` and hub name for backend `appsettings.json`.

## Step 2 — ASP.NET Core backend API

### 2.1 NuGet package

```xml
<PackageReference Include="Microsoft.Azure.NotificationHubs" Version="4.*" />
```

### 2.2 appsettings.json

```json
{
  "NotificationHub": {
    "Name": "<hub-name>",
    "ConnectionString": "Endpoint=sb://...;SharedAccessKeyName=DefaultFullSharedAccessSignature;SharedAccessKey=..."
  },
  "Authentication": { "ApiKey": "<random-guid-or-secret>" }
}
```

### 2.3 Models

```csharp
namespace YOUR_NAMESPACE.Backend.Models;

public class DeviceInstallation
{
    public string InstallationId { get; set; } = "";
    public string Platform { get; set; } = "";    // "fcmv1" or "apns"
    public string PushChannel { get; set; } = "";  // device token
    public List<string> Tags { get; set; } = [];
}

public class NotificationRequest
{
    public string Title { get; set; } = "";
    public string Body { get; set; } = "";
    public List<string> Tags { get; set; } = [];
}
```

### 2.4 NotificationHubService

```csharp
namespace YOUR_NAMESPACE.Backend.Services;

using Microsoft.Azure.NotificationHubs;
using Microsoft.Extensions.Options;

public class NotificationHubOptions
{
    public string Name { get; set; } = "";
    public string ConnectionString { get; set; } = "";
}

public interface INotificationService
{
    Task<bool> CreateOrUpdateInstallationAsync(DeviceInstallation device, CancellationToken ct);
    Task<bool> DeleteInstallationByIdAsync(string installationId, CancellationToken ct);
    Task<bool> RequestNotificationAsync(NotificationRequest request, CancellationToken ct);
}

public class NotificationHubService : INotificationService
{
    readonly NotificationHubClient _hub;

    public NotificationHubService(IOptions<NotificationHubOptions> options)
    {
        _hub = NotificationHubClient.CreateClientFromConnectionString(
            options.Value.ConnectionString, options.Value.Name);
    }

    public async Task<bool> CreateOrUpdateInstallationAsync(DeviceInstallation device, CancellationToken ct)
    {
        var installation = new Installation
        {
            InstallationId = device.InstallationId,
            PushChannel = device.PushChannel,
            Tags = device.Tags,
            Platform = device.Platform switch
            {
                "fcmv1" => NotificationPlatform.FcmV1,
                "apns"  => NotificationPlatform.Apns,
                _ => throw new ArgumentException($"Unknown platform: {device.Platform}")
            }
        };
        await _hub.CreateOrUpdateInstallationAsync(installation, ct);
        return true;
    }

    public async Task<bool> DeleteInstallationByIdAsync(string installationId, CancellationToken ct)
    {
        await _hub.DeleteInstallationAsync(installationId, ct);
        return true;
    }

    public async Task<bool> RequestNotificationAsync(NotificationRequest request, CancellationToken ct)
    {
        // GOTCHA: Azure NH limits tag expressions to 20 tags — batch accordingly.
        var batches = request.Tags.Chunk(20);
        foreach (var batch in batches)
        {
            var tagExpr = string.Join(" || ", batch);
            var fcm = $$"""{"message":{"notification":{"title":"{{request.Title}}","body":"{{request.Body}}"}}}""";
            var apns = $$"""{"aps":{"alert":{"title":"{{request.Title}}","body":"{{request.Body}}"}}}""";
            await Task.WhenAll(
                _hub.SendFcmV1NativeNotificationAsync(fcm, tagExpr, ct),
                _hub.SendAppleNativeNotificationAsync(apns, tagExpr, ct));
        }
        return true;
    }
}
```

### 2.5 Minimal API endpoints (Program.cs)

```csharp
builder.Services.Configure<NotificationHubOptions>(
    builder.Configuration.GetSection("NotificationHub"));
builder.Services.AddSingleton<INotificationService, NotificationHubService>();

var app = builder.Build();
var apiKey = builder.Configuration["Authentication:ApiKey"]!;

var api = app.MapGroup("/api/notifications")
    .AddEndpointFilter(async (ctx, next) =>
    {
        if (!ctx.HttpContext.Request.Headers.TryGetValue("apikey", out var key) || key != apiKey)
            return Results.Unauthorized();
        return await next(ctx);
    });

api.MapPut("/installations", async (DeviceInstallation device,
    INotificationService svc, CancellationToken ct) =>
    await svc.CreateOrUpdateInstallationAsync(device, ct) ? Results.Ok() : Results.BadRequest());

api.MapDelete("/installations/{id}", async (string id,
    INotificationService svc, CancellationToken ct) =>
    await svc.DeleteInstallationByIdAsync(id, ct) ? Results.Ok() : Results.BadRequest());

api.MapPost("/requests", async (NotificationRequest req,
    INotificationService svc, CancellationToken ct) =>
    await svc.RequestNotificationAsync(req, ct) ? Results.Ok() : Results.BadRequest());

app.Run();
```

## Step 3 — MAUI client shared code

### 3.1 Config

```csharp
namespace YOUR_NAMESPACE;

public static class PushConfig
{
    // GOTCHA: trailing slash required — HttpClient.BaseAddress must end with "/".
    public const string BackendServiceEndpoint = "https://<your-backend>.azurewebsites.net/";
    public const string ApiKey = "<same-key-as-backend>";
}
```

### 3.2 IPushNotificationService

```csharp
namespace YOUR_NAMESPACE.Services;

public interface IPushNotificationService
{
    string Token { get; set; }
    Task RegisterAsync(CancellationToken ct = default);
    Task DeregisterAsync(CancellationToken ct = default);
}
```

### 3.3 PushNotificationService

```csharp
namespace YOUR_NAMESPACE.Services;

using System.Net.Http.Json;
using System.Text.Json;

public class PushNotificationService : IPushNotificationService
{
    static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web);
    readonly HttpClient _http;
    public string Token { get; set; } = "";

    public PushNotificationService()
    {
        _http = new HttpClient { BaseAddress = new Uri(PushConfig.BackendServiceEndpoint) };
        _http.DefaultRequestHeaders.Add("apikey", PushConfig.ApiKey);
    }

    public async Task RegisterAsync(CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(Token)) return;
        var installation = new
        {
            installationId = GetInstallationId(),
            platform = DeviceInfo.Platform == DevicePlatform.Android ? "fcmv1" : "apns",
            pushChannel = Token,
            tags = new[] { $"user:{GetUserId()}" }
        };
        await _http.PutAsJsonAsync("api/notifications/installations", installation, _json, ct);
    }

    public async Task DeregisterAsync(CancellationToken ct = default)
    {
        await _http.DeleteAsync($"api/notifications/installations/{GetInstallationId()}", ct);
    }

    string GetInstallationId()
    {
        var id = Preferences.Get("installation_id", string.Empty);
        if (string.IsNullOrEmpty(id))
        {
            id = Guid.NewGuid().ToString();
            Preferences.Set("installation_id", id);
        }
        return id;
    }

    string GetUserId() => "default-user"; // Replace with your auth identity.
}
```

### 3.4 DI registration (MauiProgram.cs)

```csharp
builder.Services.AddSingleton<IPushNotificationService, PushNotificationService>();
```

## Step 4 — Android setup

### 4.1 Firebase project

1. Firebase Console → Add **Android app** with your package name.
2. Download `google-services.json` → place in **Platforms/Android/**.
3. In `.csproj`: `<GoogleServicesJson Include="Platforms\Android\google-services.json" />`

### 4.2 NuGet packages (Android)

```xml
<ItemGroup Condition="'$(TargetFramework)' == 'net9.0-android'">
    <PackageReference Include="Xamarin.Firebase.Messaging" Version="124.*" />
    <PackageReference Include="Xamarin.Google.Dagger" Version="2.*" />
</ItemGroup>
```

### 4.3 AndroidManifest.xml additions

```xml
<uses-permission android:name="android.permission.INTERNET" />
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
<application>
    <service android:name=".PushNotificationFirebaseMessagingService" android:exported="false">
        <intent-filter>
            <action android:name="com.google.firebase.MESSAGING_EVENT" />
        </intent-filter>
    </service>
    <meta-data android:name="com.google.firebase.messaging.default_notification_channel_id"
               android:value="default_channel" />
</application>
```

### 4.4 MainActivity.cs

```csharp
namespace YOUR_NAMESPACE;

using Android;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Gms.Tasks;
using Android.OS;
using Firebase.Messaging;

[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true,
    LaunchMode = LaunchMode.SingleTop,
    ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
        | ConfigChanges.UiMode | ConfigChanges.ScreenLayout
        | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity, IOnSuccessListener
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        CreateNotificationChannel();
        if (Build.VERSION.SdkInt >= BuildVersionCodes.Tiramisu)
            RequestPermissions(new[] { Manifest.Permission.PostNotifications }, 0);
        FirebaseMessaging.Instance.GetToken().AddOnSuccessListener(this);
    }

    public void OnSuccess(Java.Lang.Object result)
    {
        var svc = IPlatformApplication.Current!.Services.GetRequiredService<IPushNotificationService>();
        svc.Token = result.ToString()!;
        _ = svc.RegisterAsync();
    }

    void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O) return;
        var channel = new NotificationChannel("default_channel", "General", NotificationImportance.Default);
        ((NotificationManager)GetSystemService(NotificationService)!).CreateNotificationChannel(channel);
    }
}
```

### 4.5 PushNotificationFirebaseMessagingService.cs

```csharp
namespace YOUR_NAMESPACE.Platforms.Android;

using global::Android.App;
using Firebase.Messaging;

[Service(Exported = false)]
[IntentFilter(new[] { "com.google.firebase.MESSAGING_EVENT" })]
public class PushNotificationFirebaseMessagingService : FirebaseMessagingService
{
    public override void OnNewToken(string token)
    {
        // GOTCHA: tokens regenerate frequently during debug builds — always re-register.
        var svc = IPlatformApplication.Current?.Services.GetService<IPushNotificationService>();
        if (svc is null) return;
        svc.Token = token;
        _ = svc.RegisterAsync();
    }

    public override void OnMessageReceived(RemoteMessage message)
    {
        var n = message.GetNotification();
        if (n is null) return;
        var intent = new global::Android.Content.Intent(this, typeof(MainActivity));
        intent.AddFlags(global::Android.Content.ActivityFlags.ClearTop);
        var pending = PendingIntent.GetActivity(this, 0, intent,
            PendingIntentFlags.OneShot | PendingIntentFlags.Immutable);
        var builder = new Notification.Builder(this, "default_channel")
            .SetContentTitle(n.Title ?? "")
            .SetContentText(n.Body ?? "")
            .SetSmallIcon(Resource.Drawable.appiconfg)
            .SetAutoCancel(true)
            .SetContentIntent(pending);
        ((NotificationManager)GetSystemService(NotificationService)!).Notify(0, builder.Build());
    }
}
```

## Step 5 — iOS setup

### 5.1 Apple Developer portal & Entitlements

1. Enable **Push Notifications** capability for your App ID.
2. Create an APNs Key (`.p8`) and upload to Azure Notification Hub.
3. Add `Platforms/iOS/Entitlements.plist`:

```xml
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
  "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>aps-environment</key>
    <string>development</string>
</dict>
</plist>
```

4. In `.csproj`:

```xml
<PropertyGroup Condition="$(TargetFramework.Contains('-ios'))">
    <CodesignEntitlements>Platforms\iOS\Entitlements.plist</CodesignEntitlements>
</PropertyGroup>
```

### 5.2 AppDelegate.cs

```csharp
namespace YOUR_NAMESPACE.Platforms.iOS;

using Foundation;
using UIKit;
using UserNotifications;

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool FinishedLaunching(UIApplication application, NSDictionary launchOptions)
    {
        var result = base.FinishedLaunching(application, launchOptions);
        UNUserNotificationCenter.Current.RequestAuthorization(
            UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
            (granted, _) =>
            {
                if (granted)
                    InvokeOnMainThread(UIApplication.SharedApplication.RegisterForRemoteNotifications);
            });
        return result;
    }

    [Export("application:didRegisterForRemoteNotificationsWithDeviceToken:")]
    public void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
    {
        var token = BitConverter.ToString(deviceToken.ToArray()).Replace("-", "").ToLowerInvariant();
        var svc = IPlatformApplication.Current?.Services.GetService<IPushNotificationService>();
        if (svc is null) return;
        svc.Token = token;
        _ = svc.RegisterAsync();
    }

    [Export("application:didReceiveRemoteNotification:fetchCompletionHandler:")]
    public void ReceivedRemoteNotification(UIApplication application,
        NSDictionary userInfo, Action<UIBackgroundFetchResult> completionHandler)
    {
        completionHandler(UIBackgroundFetchResult.NewData);
    }
}
```

## Step 6 — Test the pipeline

1. Run the backend locally or deploy to Azure App Service.
2. **Android**: deploy to device or emulator with Google Play Services.
3. **iOS**: deploy to a **physical device** — simulators cannot receive APNS push notifications.
4. Send a test notification:

```bash
curl -X POST https://<your-backend>/api/notifications/requests \
  -H "Content-Type: application/json" \
  -H "apikey: <your-api-key>" \
  -d '{"title":"Hello","body":"Push works!","tags":["user:default-user"]}'
```

## Gotchas and troubleshooting

| Issue | Cause | Fix |
|-------|-------|-----|
| Token changes every debug run (Android) | Debug builds regenerate Firebase tokens | Re-register on every `OnNewToken` — handled above |
| `HttpClient` requests fail silently | `BaseAddress` missing trailing `/` | Ensure endpoint ends with `/` |
| iOS push won't arrive on simulator | Simulators don't support APNS | Use a physical iOS device |
| No notifications on Android 13+ | `POST_NOTIFICATIONS` permission required (API 33+) | Call `RequestPermissions` in `OnCreate` |
| Notification channel missing (API 26+) | Android requires explicit channel creation | Create channel before sending |
| `SendNotificationAsync` throws for >20 tags | Azure NH tag expression limit | Batch tags in groups of 20 |
| `422` on registration | Platform string mismatch | Use `"fcmv1"` (not `"gcm"`) and `"apns"` |
| Token empty at `RegisterAsync` | Race condition on cold start | Guard with `IsNullOrWhiteSpace` check |

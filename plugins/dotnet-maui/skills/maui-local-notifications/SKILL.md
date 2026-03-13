---
name: maui-local-notifications
description: >
  Add local notifications to .NET MAUI apps on Android, iOS, and Mac Catalyst.
  Covers notification channels, permissions, scheduling, foreground/background handling,
  and DI registration. Works with XAML/MVVM, C# Markup, and MauiReactor.
  USE FOR: "local notification", "schedule notification", "notification channel",
  "notification permission", "reminder notification", "alert notification",
  "in-app notification", "foreground notification".
  DO NOT USE FOR: push notifications from a server (use maui-push-notifications),
  permission handling only (use maui-permissions), or app lifecycle background tasks (use maui-app-lifecycle).
---

# .NET MAUI Local Notifications

Add cross-platform local notifications to any .NET MAUI app with platform-specific implementations for Android, iOS, and Mac Catalyst.

## Overview

1. Define cross-platform interface and event args
2. Implement Android notification service (channel, AlarmManager, BroadcastReceiver)
3. Implement iOS/Mac Catalyst notification service (UNUserNotificationCenter)
4. Register platform implementations via DI
5. Configure platform permissions and MainActivity
6. Wire up UI to send/receive notifications

## Step 1: Cross-Platform Interface

Create in shared project:

```csharp
public class NotificationEventArgs : EventArgs
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public interface INotificationManagerService
{
    event EventHandler NotificationReceived;
    void SendNotification(string title, string message, DateTime? notifyTime = null);
    void ReceiveNotification(string title, string message);
}
```

## Step 2: Android Implementation

Place in `Platforms/Android/`:

### NotificationManagerService.cs

```csharp
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using AndroidX.Core.App;

namespace YOUR_NAMESPACE.Platforms.Android;

public class NotificationManagerService : INotificationManagerService
{
    const string channelId = "default";
    const string channelName = "Default";
    const string channelDescription = "The default channel for notifications.";

    public const string TitleKey = "title";
    public const string MessageKey = "message";

    bool channelInitialized = false;
    int messageId = 0;
    int pendingIntentId = 0;
    NotificationManagerCompat compatManager;

    public event EventHandler NotificationReceived;
    public static NotificationManagerService Instance { get; private set; }

    public NotificationManagerService()
    {
        if (Instance == null)
        {
            CreateNotificationChannel();
            compatManager = NotificationManagerCompat.From(Platform.AppContext);
            Instance = this;
        }
    }

    public void SendNotification(string title, string message, DateTime? notifyTime = null)
    {
        if (!channelInitialized)
            CreateNotificationChannel();

        if (notifyTime != null)
        {
            var intent = new Intent(Platform.AppContext, typeof(AlarmHandler));
            intent.PutExtra(TitleKey, title);
            intent.PutExtra(MessageKey, message);
            intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

            var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
                ? PendingIntentFlags.CancelCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.CancelCurrent;

            var pendingIntent = PendingIntent.GetBroadcast(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);
            long triggerTime = GetNotifyTime(notifyTime.Value);
            var alarmManager = Platform.AppContext.GetSystemService(Context.AlarmService) as AlarmManager;
            alarmManager.Set(AlarmType.RtcWakeup, triggerTime, pendingIntent);
        }
        else
        {
            Show(title, message);
        }
    }

    public void ReceiveNotification(string title, string message)
    {
        NotificationReceived?.Invoke(null, new NotificationEventArgs { Title = title, Message = message });
    }

    public void Show(string title, string message)
    {
        var intent = new Intent(Platform.AppContext, typeof(MainActivity));
        intent.PutExtra(TitleKey, title);
        intent.PutExtra(MessageKey, message);
        intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTop);

        var pendingIntentFlags = (Build.VERSION.SdkInt >= BuildVersionCodes.S)
            ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
            : PendingIntentFlags.UpdateCurrent;

        var pendingIntent = PendingIntent.GetActivity(Platform.AppContext, pendingIntentId++, intent, pendingIntentFlags);
        var builder = new NotificationCompat.Builder(Platform.AppContext, channelId)
            .SetContentIntent(pendingIntent)
            .SetContentTitle(title)
            .SetContentText(message)
            .SetSmallIcon(Resource.Drawable.dotnet_logo)
            .SetAutoCancel(true);

        compatManager.Notify(messageId++, builder.Build());
    }

    void CreateNotificationChannel()
    {
        if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        {
            var channel = new NotificationChannel(channelId, new Java.Lang.String(channelName), NotificationImportance.Default)
            {
                Description = channelDescription
            };
            var manager = (NotificationManager)Platform.AppContext.GetSystemService(Context.NotificationService);
            manager.CreateNotificationChannel(channel);
            channelInitialized = true;
        }
    }

    long GetNotifyTime(DateTime notifyTime)
    {
        DateTime utcTime = TimeZoneInfo.ConvertTimeToUtc(notifyTime);
        double epochDiff = (new DateTime(1970, 1, 1) - DateTime.MinValue).TotalSeconds;
        return utcTime.AddSeconds(-epochDiff).Ticks / 10000;
    }
}
```

### AlarmHandler.cs

```csharp
using Android.App;
using Android.Content;

namespace YOUR_NAMESPACE.Platforms.Android;

[BroadcastReceiver(Enabled = true, Label = "Local Notifications Broadcast Receiver")]
public class AlarmHandler : BroadcastReceiver
{
    public override void OnReceive(Context context, Intent intent)
    {
        if (intent?.Extras != null)
        {
            string title = intent.GetStringExtra(NotificationManagerService.TitleKey);
            string message = intent.GetStringExtra(NotificationManagerService.MessageKey);
            var manager = NotificationManagerService.Instance ?? new NotificationManagerService();
            manager.Show(title, message);
        }
    }
}
```

### NotificationPermission.cs

```csharp
using Android;

namespace YOUR_NAMESPACE.Platforms.Android;

public class NotificationPermission : Permissions.BasePlatformPermission
{
    public override (string androidPermission, bool isRuntime)[] RequiredPermissions
    {
        get
        {
            var result = new List<(string androidPermission, bool isRuntime)>();
            if (OperatingSystem.IsAndroidVersionAtLeast(33))
                result.Add((Manifest.Permission.PostNotifications, true));
            return result.ToArray();
        }
    }
}
```

### AndroidManifest.xml

Add inside `<manifest>`:

```xml
<uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
```

### MainActivity.cs modifications

Set `LaunchMode = LaunchMode.SingleTop` on the Activity attribute, then add:

```csharp
protected override void OnCreate(Bundle? savedInstanceState)
{
    base.OnCreate(savedInstanceState);
    CreateNotificationFromIntent(Intent);
}

protected override void OnNewIntent(Intent? intent)
{
    base.OnNewIntent(intent);
    CreateNotificationFromIntent(intent);
}

static void CreateNotificationFromIntent(Intent intent)
{
    if (intent?.Extras != null)
    {
        string title = intent.GetStringExtra(NotificationManagerService.TitleKey);
        string message = intent.GetStringExtra(NotificationManagerService.MessageKey);
        var service = IPlatformApplication.Current.Services.GetService<INotificationManagerService>();
        service.ReceiveNotification(title, message);
    }
}
```

## Step 3: iOS / Mac Catalyst Implementation

Place in `Platforms/iOS/` (and copy/share to `Platforms/MacCatalyst/`):

### NotificationManagerService.cs

```csharp
using Foundation;
using UserNotifications;

namespace YOUR_NAMESPACE.Platforms.iOS;

public class NotificationManagerService : INotificationManagerService
{
    int messageId = 0;
    bool hasNotificationsPermission;

    public event EventHandler? NotificationReceived;

    public NotificationManagerService()
    {
        UNUserNotificationCenter.Current.Delegate = new NotificationReceiver();
        UNUserNotificationCenter.Current.RequestAuthorization(UNAuthorizationOptions.Alert, (approved, err) =>
        {
            hasNotificationsPermission = approved;
        });
    }

    public void SendNotification(string title, string message, DateTime? notifyTime = null)
    {
        if (!hasNotificationsPermission) return;

        messageId++;
        var content = new UNMutableNotificationContent
        {
            Title = title, Subtitle = "", Body = message, Badge = 1
        };

        UNNotificationTrigger trigger = notifyTime != null
            ? UNCalendarNotificationTrigger.CreateTrigger(GetNSDateComponents(notifyTime.Value), false)
            : UNTimeIntervalNotificationTrigger.CreateTrigger(0.25, false);

        var request = UNNotificationRequest.FromIdentifier(messageId.ToString(), content, trigger);
        UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
        {
            if (err != null) throw new Exception($"Failed to schedule notification: {err}");
        });
    }

    public void ReceiveNotification(string title, string message)
    {
        NotificationReceived?.Invoke(null, new NotificationEventArgs { Title = title, Message = message });
    }

    NSDateComponents GetNSDateComponents(DateTime dateTime) => new()
    {
        Month = dateTime.Month, Day = dateTime.Day, Year = dateTime.Year,
        Hour = dateTime.Hour, Minute = dateTime.Minute, Second = dateTime.Second
    };
}
```

### NotificationReceiver.cs

```csharp
using UserNotifications;

namespace YOUR_NAMESPACE.Platforms.iOS;

public class NotificationReceiver : UNUserNotificationCenterDelegate
{
    public override void WillPresentNotification(UNUserNotificationCenter center, UNNotification notification, Action<UNNotificationPresentationOptions> completionHandler)
    {
        ProcessNotification(notification);
        completionHandler(OperatingSystem.IsIOSVersionAtLeast(14)
            ? UNNotificationPresentationOptions.Banner
            : UNNotificationPresentationOptions.Alert);
    }

    public override void DidReceiveNotificationResponse(UNUserNotificationCenter center, UNNotificationResponse response, Action completionHandler)
    {
        if (response.IsDefaultAction) ProcessNotification(response.Notification);
        completionHandler();
    }

    void ProcessNotification(UNNotification notification)
    {
        string title = notification.Request.Content.Title;
        string message = notification.Request.Content.Body;
        var service = IPlatformApplication.Current?.Services.GetService<INotificationManagerService>();
        service?.ReceiveNotification(title, message);
    }
}
```

## Step 4: DI Registration in MauiProgram.cs

```csharp
#if ANDROID
    builder.Services.AddTransient<INotificationManagerService,
        Platforms.Android.NotificationManagerService>();
#elif IOS
    builder.Services.AddTransient<INotificationManagerService,
        Platforms.iOS.NotificationManagerService>();
#elif MACCATALYST
    builder.Services.AddTransient<INotificationManagerService,
        Platforms.MacCatalyst.NotificationManagerService>();
#endif
```

## Step 5: Using Notifications

### Request Permission (Android 13+)

```csharp
#if ANDROID
PermissionStatus status = await Permissions.RequestAsync<Platforms.Android.NotificationPermission>();
#endif
```

### Send Notifications

```csharp
// Immediate
notificationManager.SendNotification("Title", "Message body");

// Scheduled (10 seconds from now)
notificationManager.SendNotification("Reminder", "Time to check in!", DateTime.Now.AddSeconds(10));
```

### Receive Notifications

```csharp
notificationManager.NotificationReceived += (sender, args) =>
{
    var data = (NotificationEventArgs)args;
    MainThread.BeginInvokeOnMainThread(() =>
    {
        // Update UI with data.Title and data.Message
    });
};
```

## Platform Notes

- **Android**: Scheduled notifications use `AlarmManager` and do NOT survive device restart. Notification channels required on API 26+. `POST_NOTIFICATIONS` runtime permission required on API 33+.
- **iOS/Mac Catalyst**: Uses `UNUserNotificationCenter`. Permission requested at construction. Foreground notifications shown via `UNUserNotificationCenterDelegate`.
- **Windows**: Windows App SDK supports toast notifications but scheduled notifications are not yet supported.

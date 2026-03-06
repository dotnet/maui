# C# Templates for watchOS Companion App Integration

Replace placeholders: `{Namespace}`, `{AppBundleId}`, `{WatchBundleId}`

## Service Layer

### Services/WatchData.cs

Simple dictionary wrapper for WCSession communication. WCSession uses `[String: Any]` dictionaries, not JSON files.

```csharp
namespace {Namespace}.Services;

/// <summary>
/// Data model for watch ↔ phone communication.
/// Keys must match between C# and Swift sides.
/// </summary>
public record WatchData
{
    public int Counter { get; init; }
    public string Title { get; init; } = "";
    public string Message { get; init; } = "";
    public string UpdatedAt { get; init; } = "";

    /// <summary>Convert to NSDictionary-compatible keys for WCSession.</summary>
    public Dictionary<string, object> ToDictionary() => new()
    {
        ["counter"] = Counter,
        ["title"] = Title,
        ["message"] = Message,
        ["updatedAt"] = UpdatedAt
    };

    /// <summary>Parse from WCSession dictionary.</summary>
    public static WatchData FromDictionary(IDictionary<string, object> dict) => new()
    {
        Counter = dict.TryGetValue("counter", out var c) && c is int i ? i : 0,
        Title = dict.TryGetValue("title", out var t) ? t?.ToString() ?? "" : "",
        Message = dict.TryGetValue("message", out var m) ? m?.ToString() ?? "" : "",
        UpdatedAt = dict.TryGetValue("updatedAt", out var u) ? u?.ToString() ?? "" : ""
    };
}
```

### Services/IWatchConnectivityService.cs

```csharp
namespace {Namespace}.Services;

public interface IWatchConnectivityService
{
    /// <summary>Activate the WCSession. Call once early in app lifecycle.</summary>
    void Activate();

    /// <summary>Send data to watch via applicationContext (latest-wins, works in background).</summary>
    void SendContext(WatchData data);

    /// <summary>Send real-time message (only works when watch is reachable).</summary>
    void SendMessage(WatchData data);

    /// <summary>True if paired watch is reachable for real-time messaging.</summary>
    bool IsReachable { get; }

    /// <summary>True if a paired watch exists.</summary>
    bool IsPaired { get; }

    /// <summary>Fired when data is received from the watch (context or message).</summary>
    event Action<WatchData>? DataReceived;
}
```

### Services/StubWatchConnectivityService.cs

```csharp
namespace {Namespace}.Services;

public class StubWatchConnectivityService : IWatchConnectivityService
{
    public void Activate() { }
    public void SendContext(WatchData data) { }
    public void SendMessage(WatchData data) { }
    public bool IsReachable => false;
    public bool IsPaired => false;
    public event Action<WatchData>? DataReceived { add { } remove { } }
}
```

### Platforms/iOS/WatchConnectivityService.cs

**⚠️ CRITICAL:** This must be registered as a **singleton**. `WCSession.DefaultSession` is a process-wide singleton — creating multiple delegate wrappers causes undefined behavior.

```csharp
using Foundation;
using WatchConnectivity;
using {Namespace}.Services;

namespace {Namespace}.Platforms.iOS;

public class WatchConnectivityService : NSObject, IWatchConnectivityService, IWCSessionDelegate
{
    WCSession? _session;

    public bool IsReachable => _session?.Reachable ?? false;
    public bool IsPaired => _session?.Paired ?? false;
    public event Action<WatchData>? DataReceived;

    public void Activate()
    {
        if (!WCSession.IsSupported)
            return;

        _session = WCSession.DefaultSession;
        _session.Delegate = this;
        _session.ActivateSession();
    }

    public void SendContext(WatchData data)
    {
        if (_session?.ActivationState != WCSessionActivationState.Activated)
            return;

        var dict = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
            data.ToDictionary().Values.Select(v => NSObject.FromObject(v)).ToArray(),
            data.ToDictionary().Keys.Select(k => new NSString(k)).ToArray()
        );

        _session.UpdateApplicationContext(dict, out var error);
        if (error is not null)
            System.Diagnostics.Debug.WriteLine($"[Watch] UpdateApplicationContext error: {error}");
    }

    public void SendMessage(WatchData data)
    {
        if (_session?.Reachable != true)
            return;

        var dict = NSDictionary<NSString, NSObject>.FromObjectsAndKeys(
            data.ToDictionary().Values.Select(v => NSObject.FromObject(v)).ToArray(),
            data.ToDictionary().Keys.Select(k => new NSString(k)).ToArray()
        );

        _session.SendMessage(dict, null, null);
    }

    // WCSessionDelegate required methods

    [Export("session:activationDidCompleteWithState:error:")]
    public void ActivationDidComplete(WCSession session, WCSessionActivationState activationState, NSError? error)
    {
        if (error is not null)
            System.Diagnostics.Debug.WriteLine($"[Watch] Activation error: {error}");
        else
            System.Diagnostics.Debug.WriteLine($"[Watch] Activated: {activationState}");
    }

    [Export("sessionDidBecomeInactive:")]
    public void DidBecomeInactive(WCSession session) { }

    [Export("sessionDidDeactivate:")]
    public void DidDeactivate(WCSession session)
    {
        // Reactivate for watch switching scenarios
        session.ActivateSession();
    }

    [Export("session:didReceiveApplicationContext:")]
    public void DidReceiveApplicationContext(WCSession session, NSDictionary<NSString, NSObject> applicationContext)
    {
        var dict = applicationContext.ToDictionary(
            kvp => kvp.Key.ToString(),
            kvp => (object)kvp.Value.ToString()!
        );

        // Parse counter as int
        if (applicationContext.TryGetValue(new NSString("counter"), out var counterObj) && counterObj is NSNumber num)
            dict["counter"] = num.Int32Value;

        var data = WatchData.FromDictionary(dict);
        MainThread.BeginInvokeOnMainThread(() => DataReceived?.Invoke(data));
    }

    [Export("session:didReceiveMessage:")]
    public void DidReceiveMessage(WCSession session, NSDictionary<NSString, NSObject> message)
    {
        DidReceiveApplicationContext(session, message);
    }
}
```

## App Integration

### MauiProgram.cs

**⚠️ CRITICAL:** Register as **singleton** — WCSession is a process-wide singleton.

```csharp
using {Namespace}.Services;

namespace {Namespace};

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

#if IOS || __IOS__
        builder.Services.AddSingleton<IWatchConnectivityService, Platforms.iOS.WatchConnectivityService>();
#else
        builder.Services.AddSingleton<IWatchConnectivityService, StubWatchConnectivityService>();
#endif
        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}
```

### App.xaml.cs

**⚠️ CRITICAL DI note:** `Handler?.MauiContext?.Services` is null during `CreateWindow`. Use `activationState?.Context?.Services` or `IPlatformApplication.Current?.Services`.

**⚠️ CRITICAL:** Activate WCSession early — before any UI is shown. If you activate too late, the watch app may not see the phone as reachable.

```csharp
using {Namespace}.Services;
using Microsoft.Extensions.DependencyInjection;

namespace {Namespace};

public partial class App : Application
{
    public App() => InitializeComponent();

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var services = activationState?.Context?.Services
            ?? IPlatformApplication.Current?.Services;

        MainPage mainPage;
        if (services is not null)
        {
            // Activate WCSession ASAP
            var watchService = services.GetService<IWatchConnectivityService>();
            watchService?.Activate();

            mainPage = new MainPage(
                watchService ?? new StubWatchConnectivityService()
            );
        }
        else
        {
            mainPage = new MainPage();
        }

        return new Window(new NavigationPage(mainPage));
    }
}
```

## MainPage Example

### MainPage.xaml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="{Namespace}.MainPage"
             Title="Watch Sync">
    <VerticalStackLayout Spacing="20" Padding="30" VerticalOptions="Center">

        <Label Text="Phone ↔ Watch Sync"
               FontSize="24" FontAttributes="Bold"
               HorizontalOptions="Center" />

        <Border StrokeShape="RoundRectangle 20" Padding="20"
                BackgroundColor="{AppThemeBinding Light=#f0f0f5, Dark=#2a2a2e}"
                HorizontalOptions="Center">
            <Label Text="{Binding Counter}"
                   FontSize="64" FontAttributes="Bold"
                   HorizontalOptions="Center" />
        </Border>

        <HorizontalStackLayout Spacing="20" HorizontalOptions="Center">
            <Button Text="−" FontSize="28" WidthRequest="70"
                    Clicked="OnSubtractClicked"
                    BackgroundColor="#FF3B30" TextColor="White" />
            <Button Text="+" FontSize="28" WidthRequest="70"
                    Clicked="OnAddClicked"
                    BackgroundColor="#34C759" TextColor="White" />
        </HorizontalStackLayout>

        <Label x:Name="StatusLabel" Text="Tap +/− to sync with watch"
               FontSize="14" TextColor="Gray"
               HorizontalOptions="Center" />

        <Label x:Name="WatchStatusLabel" Text=""
               FontSize="12" TextColor="Gray"
               HorizontalOptions="Center" />

    </VerticalStackLayout>
</ContentPage>
```

### MainPage.xaml.cs

```csharp
using {Namespace}.Services;

namespace {Namespace};

public partial class MainPage : ContentPage
{
    readonly IWatchConnectivityService _watchService;
    int _counter;

    public int Counter
    {
        get => _counter;
        set { if (_counter != value) { _counter = value; OnPropertyChanged(); } }
    }

    public MainPage(IWatchConnectivityService watchService)
    {
        InitializeComponent();
        _watchService = watchService;
        BindingContext = this;

        _watchService.DataReceived += OnWatchDataReceived;
    }

    // Parameterless ctor for non-DI paths
    public MainPage() : this(new StubWatchConnectivityService()) { }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        UpdateWatchStatus();
    }

    void OnAddClicked(object? s, EventArgs e) { Counter++; SyncToWatch(); }
    void OnSubtractClicked(object? s, EventArgs e) { Counter--; SyncToWatch(); }

    void SyncToWatch()
    {
        var data = new WatchData
        {
            Counter = Counter,
            Title = "{Namespace}",
            Message = $"Counter: {Counter}",
            UpdatedAt = DateTime.UtcNow.ToString("o")
        };

        _watchService.SendContext(data);

        // Also try real-time if watch is reachable
        if (_watchService.IsReachable)
            _watchService.SendMessage(data);

        StatusLabel.Text = $"Sent to watch — Counter: {Counter}";
        UpdateWatchStatus();
    }

    void OnWatchDataReceived(WatchData data)
    {
        Counter = data.Counter;
        StatusLabel.Text = $"Received from watch — Counter: {Counter}";
    }

    void UpdateWatchStatus()
    {
        WatchStatusLabel.Text = _watchService.IsPaired
            ? $"Watch paired • {(_watchService.IsReachable ? "Reachable ✓" : "Not reachable")}"
            : "No watch paired";
    }
}
```

# C# Templates for iOS Widget Integration

Replace placeholders: `{Namespace}`, `{GroupId}`, `{UrlScheme}`, `{UrlHost}`, `{WidgetKind}`, `{ExtensionName}`

## Service Layer

### Services/WidgetData.cs

```csharp
using System.Text.Json.Serialization;

namespace {Namespace}.Services;

public record WidgetData
{
    [JsonPropertyName("version")]
    public int Version { get; init; } = 1;

    [JsonPropertyName("title")]
    public string Title { get; init; } = "";

    [JsonPropertyName("message")]
    public string Message { get; init; } = "";

    [JsonPropertyName("counter")]
    public int Counter { get; init; }

    [JsonPropertyName("updatedAt")]
    public string UpdatedAt { get; init; } = DateTime.UtcNow.ToString("o");

    [JsonPropertyName("extras")]
    public Dictionary<string, string> Extras { get; init; } = new();
}
```

### Services/WidgetConstants.cs

```csharp
namespace {Namespace}.Services;

public static class WidgetConstants
{
    public const string GroupId = "{GroupId}";
    public const string FromAppFile = "widget_data_fromapp.json";
    public const string FromWidgetFile = "widget_data_fromwidget.json";
    public const string UrlScheme = "{UrlScheme}";
    public const string UrlHost = "{UrlHost}";
    public const string WidgetKind = "{WidgetKind}";
}
```

### Services/IWidgetDataService.cs

```csharp
namespace {Namespace}.Services;

public interface IWidgetDataService
{
    void SendDataToWidget(WidgetData data);
    WidgetData? ReadDataFromWidget();
    void ClearWidgetIncomingData();
    void RefreshWidget(string kind = WidgetConstants.WidgetKind);
}
```

### Services/StubWidgetDataService.cs

```csharp
namespace {Namespace}.Services;

public class StubWidgetDataService : IWidgetDataService
{
    public void SendDataToWidget(WidgetData data) { }
    public WidgetData? ReadDataFromWidget() => null;
    public void ClearWidgetIncomingData() { }
    public void RefreshWidget(string kind = WidgetConstants.WidgetKind) { }
}
```

### Platforms/iOS/WidgetDataService.cs

Uses **file-based I/O** to the App Group container. Two options for triggering WidgetKit reloads:

**Option A: Using WidgetKit.WidgetCenterProxy NuGet**

```csharp
using System.Text.Json;
using Foundation;
using {Namespace}.Services;

namespace {Namespace}.Platforms.iOS;

public class WidgetDataService : IWidgetDataService
{
    static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    string? GetFilePath(string filename)
    {
        var url = NSFileManager.DefaultManager.GetContainerUrl(WidgetConstants.GroupId);
        return url?.Path is { } path ? Path.Combine(path, filename) : null;
    }

    public void SendDataToWidget(WidgetData data)
    {
        var path = GetFilePath(WidgetConstants.FromAppFile);
        if (path is null) return;
        File.WriteAllText(path, JsonSerializer.Serialize(data, JsonOptions));
    }

    public WidgetData? ReadDataFromWidget()
    {
        var path = GetFilePath(WidgetConstants.FromWidgetFile);
        if (path is null || !File.Exists(path)) return null;
        try { return JsonSerializer.Deserialize<WidgetData>(File.ReadAllText(path), JsonOptions); }
        catch { return null; }
    }

    public void ClearWidgetIncomingData()
    {
        var path = GetFilePath(WidgetConstants.FromWidgetFile);
        if (path is not null && File.Exists(path)) File.Delete(path);
    }

    public void RefreshWidget(string kind = WidgetConstants.WidgetKind)
    {
        try { new WidgetKit.WidgetCenterProxy().ReloadTimeLinesOfKind(kind); }
        catch (Exception ex) { System.Diagnostics.Debug.WriteLine($"[Widget] Reload failed: {ex}"); }
    }
}
```

**Option B: Using MauiWidgetCenter (built into MAUI, no NuGet needed)**

Replace the `RefreshWidget` method:

```csharp
    public void RefreshWidget(string kind = WidgetConstants.WidgetKind)
    {
        Microsoft.Maui.MauiWidgetCenter.ReloadTimelines(kind);
    }
```

Note: This requires `MauiWidgetHelper.framework` to be embedded in the app bundle's `Frameworks/` directory.

## App Integration

### Platforms/iOS/AppDelegate.cs

```csharp
using Foundation;
using {Namespace}.Services;
using UIKit;

namespace {Namespace};

[Register("AppDelegate")]
public class AppDelegate : MauiUIApplicationDelegate
{
    protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

    public override bool OpenUrl(UIApplication application, NSUrl url, NSDictionary options)
    {
        if (url.Scheme == WidgetConstants.UrlScheme)
        {
            App.HandleWidgetUrl(new Uri(url.AbsoluteString!));
            return true;
        }
        return base.OpenUrl(application, url, options);
    }
}
```

### App.xaml.cs

```csharp
using {Namespace}.Services;

namespace {Namespace};

public partial class App : Application
{
    public App() => InitializeComponent();

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        window.Resumed += (s, e) =>
        {
            if (window.Page is AppShell { CurrentPage: MainPage mainPage })
                mainPage.OnResumed();
        };
        return window;
    }

    internal static void HandleWidgetUrl(Uri uri)
    {
        if (uri is not { Scheme: WidgetConstants.UrlScheme, Host: WidgetConstants.UrlHost })
            return;

        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        if (int.TryParse(query["counter"], out var count))
        {
            Current?.Dispatcher.Dispatch(() =>
            {
                if (Current?.Windows?.Count > 0 &&
                    Current.Windows[0].Page is AppShell { CurrentPage: MainPage page })
                    page.OnResumedByUrl(count);
            });
        }
    }
}
```

### MauiProgram.cs

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

#if IOS
        builder.Services.AddSingleton<IWidgetDataService, Platforms.iOS.WidgetDataService>();
#else
        builder.Services.AddSingleton<IWidgetDataService, StubWidgetDataService>();
#endif
        builder.Services.AddTransient<MainPage>();
        return builder.Build();
    }
}
```

## MainPage Example

### MainPage.xaml.cs

```csharp
using {Namespace}.Services;

namespace {Namespace};

public partial class MainPage : ContentPage
{
    readonly IWidgetDataService _widgetService;
    int _counter;

    public int Counter
    {
        get => _counter;
        set { if (_counter != value) { _counter = value; OnPropertyChanged(); } }
    }

    public MainPage(IWidgetDataService widgetService)
    {
        InitializeComponent();
        _widgetService = widgetService;
        BindingContext = this;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        LoadIncomingWidgetData();
    }

    public void OnResumed() => LoadIncomingWidgetData();

    public void OnResumedByUrl(int incomingCounter)
    {
        Counter = incomingCounter;
        SyncToWidget();
    }

    void LoadIncomingWidgetData()
    {
        var incoming = _widgetService.ReadDataFromWidget();
        if (incoming is null) return;
        Counter = incoming.Counter;
        _widgetService.ClearWidgetIncomingData();
        SyncToWidget();
    }

    void OnAddClicked(object? s, EventArgs e) { Counter++; SyncToWidget(); }
    void OnSubtractClicked(object? s, EventArgs e) { Counter--; SyncToWidget(); }

    void SyncToWidget()
    {
        _widgetService.SendDataToWidget(new WidgetData
        {
            Counter = Counter,
            Title = "{Namespace}",
            Message = $"Counter: {Counter}",
            UpdatedAt = DateTime.UtcNow.ToString("o")
        });
        _widgetService.RefreshWidget();
    }
}
```

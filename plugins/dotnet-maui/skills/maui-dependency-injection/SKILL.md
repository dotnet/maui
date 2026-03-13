---
name: maui-dependency-injection
description: >
  Guidance for dependency injection in .NET MAUI apps — service registration,
  lifetime selection (Singleton/Transient/Scoped), constructor injection,
  automatic resolution via Shell navigation, explicit resolution patterns,
  platform-specific registrations, and testability best practices.
  USE FOR: "dependency injection", "DI registration", "AddSingleton", "AddTransient",
  "AddScoped", "service registration", "constructor injection", "IServiceProvider",
  "MauiProgram DI", "register services".
  DO NOT USE FOR: data binding (use maui-data-binding), Shell route setup (use maui-shell-navigation),
  or unit test mocking patterns (use maui-unit-testing).
---

# Dependency Injection in .NET MAUI

## Service Registration in MauiProgram.cs
Register services on `builder.Services` inside `CreateMauiApp()`:

```csharp
public static MauiApp CreateMauiApp()
{
    var builder = MauiApp.CreateBuilder();
    builder.UseMauiApp<App>();

    // Services
    builder.Services.AddSingleton<IDataService, DataService>();
    builder.Services.AddTransient<IApiClient, ApiClient>();

    // ViewModels
    builder.Services.AddTransient<MainViewModel>();
    builder.Services.AddTransient<DetailViewModel>();

    // Pages
    builder.Services.AddTransient<MainPage>();
    builder.Services.AddTransient<DetailPage>();

    return builder.Build();
}
```

## Lifetime Guidance
| Lifetime | Use When | Examples |
|-----------|----------|----------|
| `AddSingleton<T>` | Shared state, expensive to create, or app-wide config | Database connection, settings service, HttpClient factory |
| `AddTransient<T>` | Stateless, lightweight, or per-request usage | ViewModels, pages, API call wrappers |
| `AddScoped<T>` | Per-scope lifetime (rarely used in MAUI — no built-in scope per page) | Scoped unit-of-work in manually created scopes |

> **Rule of thumb:** Use `AddTransient` for ViewModels and Pages. Use `AddSingleton` for
> services that hold shared state or are expensive to construct. Avoid `AddScoped` unless
> you create and manage `IServiceScope` instances yourself.

## Constructor Injection (Preferred)
Inject dependencies through the constructor. The DI container resolves them automatically
when the type is itself resolved from the container:

```csharp
public class MainViewModel
{
    private readonly IDataService _dataService;
    private readonly IApiClient _apiClient;

    public MainViewModel(IDataService dataService, IApiClient apiClient)
    {
        _dataService = dataService;
        _apiClient = apiClient;
    }
}
```

## ViewModel → Page Pattern
Register both the ViewModel and the Page. Inject the ViewModel into the Page constructor
and assign it as `BindingContext`:

```csharp
public partial class MainPage : ContentPage
{
    public MainPage(MainViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

## Automatic Resolution via Shell Navigation
When Pages are registered in the DI container **and** registered as Shell routes, Shell
resolves them (and their dependencies) automatically:

```csharp
// In MauiProgram.cs
builder.Services.AddTransient<DetailPage>();
builder.Services.AddTransient<DetailViewModel>();

// Route registration (AppShell.xaml.cs or startup)
Routing.RegisterRoute(nameof(DetailPage), typeof(DetailPage));

// Navigation — DI resolves DetailPage and its DetailViewModel
await Shell.Current.GoToAsync(nameof(DetailPage));
```

## Explicit Resolution
When constructor injection is not available, resolve services explicitly:

```csharp
// From any Element with a Handler
var service = this.Handler.MauiContext.Services.GetService<IDataService>();
```

### IServiceProvider Injection
Inject `IServiceProvider` when you need to resolve services dynamically:

```csharp
public class MyService
{
    private readonly IServiceProvider _serviceProvider;

    public MyService(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public void DoWork()
    {
        var api = _serviceProvider.GetRequiredService<IApiClient>();
    }
}
```

## Platform-Specific Service Registration
Use preprocessor directives to register platform-specific implementations:

```csharp
// In MauiProgram.cs
#if ANDROID
builder.Services.AddSingleton<INotificationService, AndroidNotificationService>();
#elif IOS || MACCATALYST
builder.Services.AddSingleton<INotificationService, AppleNotificationService>();
#elif WINDOWS
builder.Services.AddSingleton<INotificationService, WindowsNotificationService>();
#endif
```

## Interface-First Pattern for Testability
Define interfaces for services so implementations can be swapped in tests:

```csharp
public interface IDataService
{
    Task<List<Item>> GetItemsAsync();
}

public class DataService : IDataService
{
    public async Task<List<Item>> GetItemsAsync() { /* ... */ }
}

// Register the interface → implementation mapping
builder.Services.AddSingleton<IDataService, DataService>();
```

In tests, substitute a mock without touching production code:

```csharp
var services = new ServiceCollection();
services.AddSingleton<IDataService, FakeDataService>();
```

## Gotcha: XAML Resource Parsing vs. DI Timing
XAML resources (styles, resource dictionaries in `App.xaml`) are parsed **before** the
`App` class is fully resolved from the container. If a resource or converter needs a
service, inject `IServiceProvider` into the `App` constructor and resolve what you need
in `CreateWindow()`:

```csharp
public partial class App : Application
{
    private readonly IServiceProvider _services;

    public App(IServiceProvider services)
    {
        _services = services;
        InitializeComponent(); // XAML resources parse here
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        // Safe to resolve services here — container is fully built
        var mainPage = _services.GetRequiredService<MainPage>();
        return new Window(new AppShell());
    }
}
```

## Quick Checklist

- Register every Page and ViewModel you want injected in `MauiProgram.cs`.
- Prefer constructor injection over service-locator calls.
- Use `AddSingleton` only for truly shared or expensive services.
- Use interfaces for any service you want to mock in tests.
- Use `#if` directives for platform-specific implementations.
- Resolve late-bound services in `CreateWindow()`, not during XAML parse.

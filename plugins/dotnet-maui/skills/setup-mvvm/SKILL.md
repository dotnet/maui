---
name: setup-mvvm
description: >-
  Guide for setting up the MVVM pattern in .NET MAUI apps with ViewModels,
  commands, and dependency injection.
  USE FOR: "MVVM", "ViewModel", "INotifyPropertyChanged", "ObservableProperty",
  "RelayCommand", "AsyncRelayCommand", "CommunityToolkit.Mvvm", "MVVM Toolkit",
  "ObservableObject", "ICommand", "ObservableCollection", "ViewModel base class",
  "MVVM setup", "ViewModel registration", "ViewModel DI".
  DO NOT USE FOR: XAML binding syntax or value converters (use maui-data-binding),
  DI container lifetime details (use maui-dependency-injection),
  Shell route setup or URI navigation (use maui-shell-navigation),
  or unit test mocking patterns (use maui-unit-testing).
---

# Setting Up MVVM in .NET MAUI

## ViewModel Base Class Options

### Option 1: Manual INotifyPropertyChanged

Implement the interface directly when you need full control or cannot take a
dependency on the MVVM Toolkit:

```csharp
using System.ComponentModel;
using System.Runtime.CompilerServices;

public class BaseViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    protected bool SetProperty<T>(ref T field, T value,
        [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }
}

public class MainViewModel : BaseViewModel
{
    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }
}
```

**When to use:** library projects that must stay dependency-free, or when you need
custom change-tracking logic in `SetProperty`.

### Option 2: CommunityToolkit.Mvvm (Recommended)

The MVVM Toolkit uses source generators to eliminate boilerplate. Install the
`CommunityToolkit.Mvvm` NuGet package, then use attributes on a `partial` class:

```csharp
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task LoadDataAsync()
    {
        // The generator creates a LoadDataCommand property (IAsyncRelayCommand)
    }
}
```

The source generator produces:
- A public `Title` property with `OnPropertyChanged` calls.
- A public `IsBusy` property with `OnPropertyChanged` calls.
- A public `LoadDataCommand` property of type `IAsyncRelayCommand`.

**When to use:** all new projects. Less boilerplate, compile-time safe, and
trimmer/AOT-compatible.

> **Naming convention:** private field `_title` → generated property `Title`.
> Field `_isBusy` → property `IsBusy`. The generator strips the leading underscore
> and capitalizes the first letter.

## ViewModel Structure

### Properties with Change Notification

```csharp
public partial class ItemDetailViewModel : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveCommand))]
    private bool _isValid;
}
```

`[NotifyCanExecuteChangedFor]` re-evaluates the command's `CanExecute` whenever
`IsValid` changes — the button automatically enables/disables.

### Commands

| Type | Attribute | Generated Type | Use When |
|------|-----------|---------------|----------|
| Sync | `[RelayCommand]` | `RelayCommand` | Fast, non-blocking work |
| Async | `[RelayCommand]` on `Task` method | `AsyncRelayCommand` | I/O, network calls |
| With parameter | `[RelayCommand]` on method with param | `RelayCommand<T>` | Passing `CommandParameter` |
| With CanExecute | `[RelayCommand(CanExecute = ...)]` | Any of the above | Conditional enable/disable |

```csharp
public partial class ItemsViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private Item? _selectedItem;

    [RelayCommand]
    private void Add()
    {
        Items.Add(new Item { Name = "New item" });
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        var data = await _dataService.GetItemsAsync();
        Items = new ObservableCollection<Item>(data);
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    private void Delete(Item item)
    {
        Items.Remove(item);
    }

    private bool CanDelete(Item? item) => item is not null;
}
```

### ObservableCollection for List Data

Use `ObservableCollection<T>` so the UI updates automatically when items are
added or removed:

```csharp
public partial class ItemsViewModel : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        var data = await _dataService.GetItemsAsync();
        Items = new ObservableCollection<Item>(data);
    }
}
```

> **Tip:** Replace the entire collection rather than calling `Clear()` then `Add()`
> in a loop — it fires a single `CollectionChanged` reset event and avoids UI flicker.

### Constructor Injection of Services

ViewModels receive their dependencies through the constructor:

```csharp
public partial class ItemsViewModel : ObservableObject
{
    private readonly IDataService _dataService;
    private readonly INavigationService _navigationService;

    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    public ItemsViewModel(IDataService dataService,
                          INavigationService navigationService)
    {
        _dataService = dataService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    private async Task LoadItemsAsync()
    {
        var data = await _dataService.GetItemsAsync();
        Items = new ObservableCollection<Item>(data);
    }
}
```

## DI Registration

Register ViewModels, Pages, and services in `MauiProgram.cs`:

```csharp
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMauiApp<App>();

        // Services — singleton for shared/expensive, transient for stateless
        builder.Services.AddSingleton<IDataService, DataService>();
        builder.Services.AddSingleton<IConnectivityService, ConnectivityService>();
        builder.Services.AddTransient<IApiClient, ApiClient>();

        // ViewModels — typically transient so each navigation gets fresh state
        builder.Services.AddTransient<ItemsViewModel>();
        builder.Services.AddTransient<ItemDetailViewModel>();
        builder.Services.AddSingleton<SettingsViewModel>(); // singleton if state must persist

        // Pages — match the ViewModel lifetime
        builder.Services.AddTransient<ItemsPage>();
        builder.Services.AddTransient<ItemDetailPage>();
        builder.Services.AddSingleton<SettingsPage>();

        return builder.Build();
    }
}
```

| Registration | Use When |
|-------------|----------|
| `AddTransient` (ViewModel + Page) | Each navigation should get a fresh instance — most pages |
| `AddSingleton` (ViewModel + Page) | State must survive navigation (e.g., settings, dashboard) |

> **Important:** A Page's lifetime should match its ViewModel's lifetime. A transient
> Page with a singleton ViewModel is fine; a singleton Page with a transient ViewModel
> will silently keep the first ViewModel forever.

## View ↔ ViewModel Connection

### Constructor Injection (Recommended)

The Page receives its ViewModel through DI and sets `BindingContext`:

```csharp
public partial class ItemsPage : ContentPage
{
    public ItemsPage(ItemsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
```

### Compiled Bindings with x:DataType

Always declare `x:DataType` on the Page root to enable compiled bindings.
This catches binding errors at build time and is **8–20× faster** than
reflection-based resolution:

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:vm="clr-namespace:MyApp.ViewModels"
             x:Class="MyApp.Views.ItemsPage"
             x:DataType="vm:ItemsViewModel"
             Title="{Binding Title}">

    <CollectionView ItemsSource="{Binding Items}"
                    SelectionMode="Single"
                    SelectedItem="{Binding SelectedItem}">
        <CollectionView.ItemTemplate>
            <DataTemplate x:DataType="model:Item">
                <Label Text="{Binding Name}" Padding="10" />
            </DataTemplate>
        </CollectionView.ItemTemplate>
    </CollectionView>
</ContentPage>
```

> **Key rule:** Declare `x:DataType` only where `BindingContext` changes — the Page
> root and inside `DataTemplate`. Children inherit from their ancestor.
> See the **maui-data-binding** skill for full compiled binding guidance.

## Best Practices

### Keep ViewModels Testable

ViewModels should have **zero** references to MAUI UI types (`Page`, `Shell`,
`Application`, etc.). Inject abstractions instead:

```csharp
// ❌ Bad — hard dependency on Shell
public partial class ItemsViewModel : ObservableObject
{
    [RelayCommand]
    private async Task GoToDetail(Item item)
    {
        await Shell.Current.GoToAsync($"detail?id={item.Id}");
    }
}

// ✅ Good — inject an abstraction
public interface INavigationService
{
    Task GoToDetailAsync(int itemId);
}

public partial class ItemsViewModel : ObservableObject
{
    private readonly INavigationService _navigation;

    public ItemsViewModel(INavigationService navigation)
    {
        _navigation = navigation;
    }

    [RelayCommand]
    private async Task GoToDetail(Item item)
    {
        await _navigation.GoToDetailAsync(item.Id);
    }
}
```

### Async Commands with Loading State

Use `IsBusy` to show activity indicators and prevent double-taps:

```csharp
public partial class ItemsViewModel : ObservableObject
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<Item> _items = [];

    public ItemsViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        if (IsBusy) return;

        try
        {
            IsBusy = true;
            var data = await _dataService.GetItemsAsync();
            Items = new ObservableCollection<Item>(data);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
```

```xml
<ActivityIndicator IsRunning="{Binding IsBusy}"
                   IsVisible="{Binding IsBusy}" />
<Button Text="Refresh"
        Command="{Binding RefreshCommand}" />
```

> **Tip:** `AsyncRelayCommand` automatically sets its own `IsRunning` property.
> You can bind to `RefreshCommand.IsRunning` instead of managing `IsBusy` manually:
> `<ActivityIndicator IsRunning="{Binding RefreshCommand.IsRunning}" />`

### Navigation from ViewModels via Shell

Wrap Shell navigation behind an interface for testability.
Implementation uses `Shell.Current.GoToAsync`:

```csharp
public class ShellNavigationService : INavigationService
{
    public Task GoToDetailAsync(int itemId)
        => Shell.Current.GoToAsync($"detail?id={itemId}");

    public Task GoBackAsync()
        => Shell.Current.GoToAsync("..");
}
```

Register in DI:

```csharp
builder.Services.AddSingleton<INavigationService, ShellNavigationService>();
```

Receive navigation parameters with `IQueryAttributable`:

```csharp
public partial class ItemDetailViewModel : ObservableObject, IQueryAttributable
{
    private readonly IDataService _dataService;

    [ObservableProperty]
    private Item? _item;

    public ItemDetailViewModel(IDataService dataService)
    {
        _dataService = dataService;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var value) && value is string idStr
            && int.TryParse(idStr, out var id))
        {
            LoadItemCommand.Execute(id);
        }
    }

    [RelayCommand]
    private async Task LoadItem(int id)
    {
        Item = await _dataService.GetItemAsync(id);
    }
}
```

### Use Interfaces for Services

Every service the ViewModel depends on should be defined as an interface:

```csharp
public interface IDataService
{
    Task<List<Item>> GetItemsAsync();
    Task<Item?> GetItemAsync(int id);
    Task SaveItemAsync(Item item);
}
```

This allows swapping real implementations for fakes/mocks in tests without
changing ViewModel code.

## Quick Checklist

- [ ] Install `CommunityToolkit.Mvvm` NuGet package.
- [ ] Inherit ViewModels from `ObservableObject`.
- [ ] Mark ViewModel classes as `partial`.
- [ ] Use `[ObservableProperty]` for bindable properties.
- [ ] Use `[RelayCommand]` for commands.
- [ ] Register ViewModels and Pages in `MauiProgram.cs`.
- [ ] Inject ViewModel into Page constructor, assign `BindingContext`.
- [ ] Declare `x:DataType` on the Page root for compiled bindings.
- [ ] Keep ViewModels free of MAUI UI dependencies.
- [ ] Define service interfaces; inject them via constructor.

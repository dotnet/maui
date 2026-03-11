---
name: maui-shell-navigation
description: >
  .NET MAUI Shell navigation guidance — Shell visual hierarchy, AppShell setup,
  tab bars, flyout menus, URI-based navigation with GoToAsync, route registration,
  query parameters, back navigation, and navigation events.
  USE FOR: "Shell navigation", "GoToAsync", "AppShell", "tab bar", "flyout menu",
  "route registration", "query parameters navigation", "back navigation",
  "Shell tabs", "URI navigation", "navigation events".
  DO NOT USE FOR: deep linking from external URLs (use maui-deep-linking),
  data binding on pages (use maui-data-binding), or dependency injection setup (use maui-dependency-injection).
---

# .NET MAUI Shell Navigation

## Shell Visual Hierarchy

Shell uses a four-level hierarchy. Each level wraps the one below it:

```
Shell
 ├── FlyoutItem / TabBar          (top-level navigation grouping)
 │    ├── Tab                     (bottom-tab grouping)
 │    │    ├── ShellContent        (page slot; points to a ContentPage)
 │    │    └── ShellContent        (creates top tabs within a bottom tab)
 │    └── Tab
 └── FlyoutItem / TabBar
```

- **FlyoutItem** – appears in the flyout menu. Contains one or more `Tab` children.
- **TabBar** – bottom tab bar with no flyout entry. Use when the app has no flyout.
- **Tab** – groups `ShellContent` objects. Multiple `ShellContent` in one `Tab` produces top tabs.
- **ShellContent** – each represents a `ContentPage`.

### Implicit Conversion

You can omit intermediate wrappers. Shell auto-wraps:

| You write              | Shell creates                                |
|------------------------|----------------------------------------------|
| `ShellContent` only    | `FlyoutItem > Tab > ShellContent`            |
| `Tab` only             | `FlyoutItem > Tab`                           |
| `ShellContent` in `TabBar` | `TabBar > Tab > ShellContent`            |

This keeps simple apps concise while allowing full control when needed.

## AppShell.xaml Setup

```xml
<Shell xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
       xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
       xmlns:views="clr-namespace:MyApp.Views"
       x:Class="MyApp.AppShell"
       FlyoutBehavior="Flyout">

    <FlyoutItem Title="Animals" Icon="animals.png">
        <Tab Title="Cats">
            <ShellContent Title="Domestic"
                          ContentTemplate="{DataTemplate views:DomesticCatsPage}" />
            <ShellContent Title="Wild"
                          ContentTemplate="{DataTemplate views:WildCatsPage}" />
        </Tab>
        <Tab Title="Dogs" Icon="dogs.png">
            <ShellContent ContentTemplate="{DataTemplate views:DogsPage}" />
        </Tab>
    </FlyoutItem>

    <TabBar>
        <ShellContent Title="Home" Icon="home.png"
                      ContentTemplate="{DataTemplate views:HomePage}" />
        <ShellContent Title="Settings" Icon="settings.png"
                      ContentTemplate="{DataTemplate views:SettingsPage}" />
    </TabBar>
</Shell>
```

### ContentTemplate and Lazy Loading

Always use `ContentTemplate` with `DataTemplate` so pages are created on demand.
Using `Content` directly creates all pages during Shell init, hurting startup time.

## Tab Configuration

### Bottom Tabs

Multiple `ShellContent` (or `Tab`) children inside a `TabBar` or `FlyoutItem`
produce bottom tabs.

### Top Tabs

Multiple `ShellContent` children inside a single `Tab` produce top tabs within
that bottom tab:

```xml
<Tab Title="Photos">
    <ShellContent Title="Recent"  ContentTemplate="{DataTemplate views:RecentPage}" />
    <ShellContent Title="Favorites" ContentTemplate="{DataTemplate views:FavoritesPage}" />
</Tab>
```

### TabBar Appearance (Attached Properties)

Set these on any page or Shell element:

| Attached Property              | Type    | Purpose                          |
|--------------------------------|---------|----------------------------------|
| `Shell.TabBarBackgroundColor`  | `Color` | Tab bar background               |
| `Shell.TabBarForegroundColor`  | `Color` | Foreground / selected icon color |
| `Shell.TabBarTitleColor`       | `Color` | Selected tab title color         |
| `Shell.TabBarUnselectedColor`  | `Color` | Unselected tab icon/title color  |
| `Shell.TabBarDisabledColor`    | `Color` | Disabled tab color               |
| `Shell.TabBarIsVisible`        | `bool`  | Show/hide the tab bar            |

```xml
<ContentPage Shell.TabBarIsVisible="False" ... />
```

## Flyout Configuration

### FlyoutBehavior

Set on `Shell`:

```xml
<Shell FlyoutBehavior="Flyout"> ... </Shell>
```

Values: `Disabled`, `Flyout`, `Locked`.

### FlyoutDisplayOptions

Controls how a `FlyoutItem`'s children appear in the flyout:

```xml
<FlyoutItem Title="Animals" FlyoutDisplayOptions="AsMultipleItems">
    <Tab Title="Cats" ... />
    <Tab Title="Dogs" ... />
</FlyoutItem>
```

- `AsSingleItem` (default) – one flyout entry for the group.
- `AsMultipleItems` – each child `Tab` gets its own flyout entry.

### Flyout Item Template

Customize appearance with `Shell.ItemTemplate`. BindingContext exposes `Title`
and `FlyoutIcon` (FlyoutItem) or `Text` and `IconImageSource` (MenuItem):

```xml
<Shell.ItemTemplate>
    <DataTemplate>
        <Grid ColumnDefinitions="Auto,*" Padding="10">
            <Image Source="{Binding FlyoutIcon}" HeightRequest="24" />
            <Label Grid.Column="1" Text="{Binding Title}" VerticalTextAlignment="Center" />
        </Grid>
    </DataTemplate>
</Shell.ItemTemplate>
```

### Replacing Flyout Content

```xml
<Shell.FlyoutContent>
    <CollectionView BindingContext="{x:Reference shell}"
                    ItemsSource="{Binding FlyoutItems}" />
</Shell.FlyoutContent>
```

### MenuItem (non-navigation flyout entries)

```xml
<MenuItem Text="Log Out"
          Command="{Binding LogOutCommand}"
          IconImageSource="logout.png" />
```

## Route Registration

Shell visual hierarchy items have implicit routes derived from their `Route`
property (or type name). Detail pages not in the hierarchy must be registered:

```csharp
// In AppShell constructor or MauiProgram
Routing.RegisterRoute("animaldetails", typeof(AnimalDetailsPage));
Routing.RegisterRoute("editanimal", typeof(EditAnimalPage));
```

**Gotcha:** Duplicate route names throw `ArgumentException` at registration time.
Every route must be unique across the entire app.

## Navigation with GoToAsync

All programmatic navigation goes through `Shell.Current.GoToAsync`:

```csharp
// Absolute – navigate to a specific place in the hierarchy
await Shell.Current.GoToAsync("//animals/cats/domestic");

// Relative – push a registered page onto the navigation stack
await Shell.Current.GoToAsync("animaldetails");

// With query string
await Shell.Current.GoToAsync($"animaldetails?id={animal.Id}");
```

### Absolute vs Relative Routes

| Prefix   | Meaning                                        |
|----------|------------------------------------------------|
| `//`     | Absolute route from Shell root                 |
| (none)   | Relative; pushes onto the current nav stack    |
| `..`     | Go back one level in the navigation stack      |
| `../`    | Go back then navigate forward                  |

```csharp
// Go back one page
await Shell.Current.GoToAsync("..");

// Go back two pages
await Shell.Current.GoToAsync("../..");

// Go back one page, then navigate to edit
await Shell.Current.GoToAsync("../editanimal");
```

**Gotcha:** Relative routes work only for pages registered with
`Routing.RegisterRoute`. You cannot push visual-hierarchy pages as relative routes.

## Query Parameters

### QueryProperty Attribute

```csharp
[QueryProperty(nameof(AnimalId), "id")]
public partial class AnimalDetailsPage : ContentPage
{
    public string AnimalId { get; set; }
}

// Navigate with query string:
await Shell.Current.GoToAsync($"animaldetails?id={animal.Id}");
```

### IQueryAttributable Interface

Preferred for ViewModels — gives you all parameters in one call:

```csharp
public class AnimalDetailsViewModel : ObservableObject, IQueryAttributable
{
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("id", out var id))
            AnimalId = id.ToString();
    }
}
```

The interface works on the page itself or on any object set as the page's
`BindingContext`.

### Passing Complex Objects

Use `ShellNavigationQueryParameters` (dictionary of `string` → `object`) to pass
objects without serializing to strings:

```csharp
var parameters = new ShellNavigationQueryParameters
{
    { "animal", selectedAnimal }  // pass the object directly
};
await Shell.Current.GoToAsync("animaldetails", parameters);
```

Receive via `IQueryAttributable`:

```csharp
public void ApplyQueryAttributes(IDictionary<string, object> query)
{
    Animal = query["animal"] as Animal;
}
```

## Navigation Events

Override in your `AppShell`:

```csharp
protected override void OnNavigating(ShellNavigatingEventArgs args)
{
    base.OnNavigating(args);
    if (hasUnsavedChanges && args.Source == ShellNavigationSource.Pop)
        args.Cancel();  // prevent leaving
}

protected override void OnNavigated(ShellNavigatedEventArgs args)
{
    base.OnNavigated(args);
    // args.Current, args.Previous, args.Source
}
```

For async checks, use `args.GetDeferral()` → do work → `deferral.Complete()`.

`ShellNavigationSource` values: `Push`, `Pop`, `PopToRoot`, `Insert`, `Remove`,
`ShellItemChanged`, `ShellSectionChanged`, `ShellContentChanged`, `Unknown`.

## Inspecting Navigation State

```csharp
// Current URI location
ShellNavigationState state = Shell.Current.CurrentState;
string location = state.Location.ToString();  // e.g. "//animals/cats/domestic"

// Current page
Page page = Shell.Current.CurrentPage;

// Navigation stack of the current tab
IReadOnlyList<Page> stack = Shell.Current.Navigation.NavigationStack;
```

## Back Button Behavior

Customize the back button per page:

```xml
<Shell.BackButtonBehavior>
    <BackButtonBehavior Command="{Binding BackCommand}"
                       IconOverride="back_arrow.png"
                       TextOverride="Cancel" />
</Shell.BackButtonBehavior>
```

Properties: `Command`, `CommandParameter`, `IconOverride`, `TextOverride`,
`IsVisible`, `IsEnabled`.

## Common Gotchas

1. **Duplicate route names** – `Routing.RegisterRoute` throws `ArgumentException`
   if a route name is already registered or matches a visual hierarchy route.
2. **Relative routes require registration** – you cannot `GoToAsync("somepage")`
   unless `somepage` was registered with `Routing.RegisterRoute`. Visual hierarchy
   pages use absolute `//` routes.
3. **Pages are created on demand** – when using `ContentTemplate`, the page
   constructor runs only on first navigation. Don't assume pages exist at startup.
4. **Tab.Stack is read-only** – you cannot manipulate the navigation stack directly;
   use `GoToAsync` for all navigation changes.
5. **GoToAsync is async** – always `await` it. Fire-and-forget navigation causes
   race conditions and can silently fail.
6. **Route hierarchy matters** – absolute routes must match the full path through
   the visual hierarchy (`//FlyoutItem/Tab/ShellContent`).

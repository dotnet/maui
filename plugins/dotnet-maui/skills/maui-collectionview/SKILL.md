---
name: maui-collectionview
description: >
  Guidance for implementing CollectionView in .NET MAUI apps — data display,
  layouts (list & grid), selection, grouping, scrolling, empty views, templates,
  incremental loading, swipe actions, and pull-to-refresh.
  USE FOR: "CollectionView", "list view", "grid layout", "data template",
  "item template", "grouping", "pull to refresh", "incremental loading",
  "swipe actions", "empty view", "selection mode", "scroll to item".
  DO NOT USE FOR: simple static layouts (use maui-data-binding),
  map pin lists (use maui-maps), or table-based data entry forms.
---

# CollectionView – .NET MAUI

Use `CollectionView` for displaying scrollable lists and grids of data.
It replaces `ListView` and offers better performance, flexible layouts, and no `ViewCell` requirement.

## Basic setup

```xml
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:Item">
            <HorizontalStackLayout Padding="8" Spacing="8">
                <Image Source="{Binding Icon}" WidthRequest="40" HeightRequest="40" />
                <Label Text="{Binding Name}" VerticalOptions="Center" />
            </HorizontalStackLayout>
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

- Bind `ItemsSource` to an `ObservableCollection<T>` so the UI updates on add/remove.
- Each item template root must be a `Layout` or `View` — **never use `ViewCell`**.
- Always set `x:DataType` on `DataTemplate` for compiled bindings.

## DataTemplateSelector

Choose templates at runtime based on item properties:

```csharp
public class ItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate NormalTemplate { get; set; }
    public DataTemplate HighlightTemplate { get; set; }

    protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
    {
        return ((Item)item).IsHighlighted ? HighlightTemplate : NormalTemplate;
    }
}
```

```xml
<CollectionView ItemsSource="{Binding Items}"
                ItemTemplate="{StaticResource MyTemplateSelector}" />
```

## Layouts

Set `ItemsLayout` to control arrangement. Default is `VerticalList`.

| Layout | XAML value |
|---|---|
| Vertical list | `VerticalList` (default) |
| Horizontal list | `HorizontalList` |
| Vertical grid | `GridItemsLayout` with `Orientation="Vertical"` |
| Horizontal grid | `GridItemsLayout` with `Orientation="Horizontal"` |

### Grid layout

```xml
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.ItemsLayout>
        <GridItemsLayout Orientation="Vertical"
                         Span="2"
                         VerticalItemSpacing="8"
                         HorizontalItemSpacing="8" />
    </CollectionView.ItemsLayout>
</CollectionView>
```

### Horizontal list

```xml
<CollectionView ItemsSource="{Binding Items}"
                ItemsLayout="HorizontalList" />
```

## ItemSizingStrategy

Controls how items are measured. Set on `ItemsLayout`.

| Value | Behavior |
|---|---|
| `MeasureAllItems` | Measures every item individually (default). Accurate but slower for heterogeneous sizes. |
| `MeasureFirstItem` | Measures only the first item and applies that size to all. Much faster for uniform items. |

```xml
<CollectionView.ItemsLayout>
    <LinearItemsLayout Orientation="Vertical"
                       ItemSizingStrategy="MeasureFirstItem" />
</CollectionView.ItemsLayout>
```

## ItemSpacing

Use `ItemSpacing` on `LinearItemsLayout` or `VerticalItemSpacing` / `HorizontalItemSpacing` on `GridItemsLayout`:

```xml
<CollectionView.ItemsLayout>
    <LinearItemsLayout Orientation="Vertical" ItemSpacing="8" />
</CollectionView.ItemsLayout>
```

## Headers and footers

Supports string, view, or templated:

```xml
<!-- Simple string -->
<CollectionView Header="My Items" Footer="End of list" />

<!-- Custom view -->
<CollectionView ItemsSource="{Binding Items}">
    <CollectionView.Header>
        <Label Text="Header" FontAttributes="Bold" Padding="8" />
    </CollectionView.Header>
    <CollectionView.Footer>
        <Label Text="Footer" FontAttributes="Italic" Padding="8" />
    </CollectionView.Footer>
</CollectionView>
```

Use `HeaderTemplate` / `FooterTemplate` when headers/footers are data-bound.

## Selection

### Selection mode

| Mode | Property to bind | Binding mode |
|---|---|---|
| `None` | — | — |
| `Single` | `SelectedItem` | `TwoWay` |
| `Multiple` | `SelectedItems` | `OneWay` |

```xml
<CollectionView ItemsSource="{Binding Items}"
                SelectionMode="Single"
                SelectedItem="{Binding CurrentItem, Mode=TwoWay}"
                SelectionChangedCommand="{Binding ItemSelectedCommand}" />
```

For `Multiple` selection, bind `SelectedItems` (type `IList<object>`):

```xml
<CollectionView SelectionMode="Multiple"
                SelectedItems="{Binding ChosenItems, Mode=OneWay}" />
```

### Selected visual state

Highlight selected items using `VisualStateManager`:

```xml
<CollectionView.ItemTemplate>
    <DataTemplate x:DataType="models:Item">
        <Grid Padding="8">
            <VisualStateManager.VisualStateGroups>
                <VisualStateGroup Name="CommonStates">
                    <VisualState Name="Normal">
                        <VisualState.Setters>
                            <Setter Property="BackgroundColor" Value="Transparent" />
                        </VisualState.Setters>
                    </VisualState>
                    <VisualState Name="Selected">
                        <VisualState.Setters>
                            <Setter Property="BackgroundColor" Value="{AppThemeBinding Light={StaticResource Primary}, Dark={StaticResource PrimaryDark}}" />
                        </VisualState.Setters>
                    </VisualState>
                </VisualStateGroup>
            </VisualStateManager.VisualStateGroups>
            <Label Text="{Binding Name}" />
        </Grid>
    </DataTemplate>
</CollectionView.ItemTemplate>
```

## Grouping

1. Create a group class inheriting from `List<T>`:

```csharp
public class AnimalGroup : List<Animal>
{
    public string Name { get; }
    public AnimalGroup(string name, List<Animal> animals) : base(animals)
    {
        Name = name;
    }
}
```

2. Bind to `ObservableCollection<AnimalGroup>` and set `IsGrouped="True"`:

```xml
<CollectionView ItemsSource="{Binding AnimalGroups}"
                IsGrouped="True">
    <CollectionView.GroupHeaderTemplate>
        <DataTemplate x:DataType="models:AnimalGroup">
            <Label Text="{Binding Name}"
                   FontAttributes="Bold"
                   BackgroundColor="{StaticResource Gray100}"
                   Padding="8" />
        </DataTemplate>
    </CollectionView.GroupHeaderTemplate>
    <CollectionView.GroupFooterTemplate>
        <DataTemplate x:DataType="models:AnimalGroup">
            <Label Text="{Binding Count, StringFormat='{0} items'}"
                   FontAttributes="Italic"
                   Padding="4,0" />
        </DataTemplate>
    </CollectionView.GroupFooterTemplate>
    <CollectionView.ItemTemplate>
        <DataTemplate x:DataType="models:Animal">
            <Label Text="{Binding Name}" Padding="16,4" />
        </DataTemplate>
    </CollectionView.ItemTemplate>
</CollectionView>
```

## EmptyView

Shown when `ItemsSource` is empty or null.

```xml
<!-- Simple string -->
<CollectionView EmptyView="No items found." />

<!-- Custom view — wrap in a ContentView -->
<CollectionView ItemsSource="{Binding SearchResults}">
    <CollectionView.EmptyView>
        <ContentView>
            <VerticalStackLayout HorizontalOptions="Center" VerticalOptions="Center">
                <Image Source="empty_state.png" WidthRequest="120" />
                <Label Text="Nothing here yet" HorizontalTextAlignment="Center" />
            </VerticalStackLayout>
        </ContentView>
    </CollectionView.EmptyView>
</CollectionView>
```

You can also use `EmptyViewTemplate` with a `DataTemplateSelector` to swap empty views based on state.

> **Gotcha**: When using a custom view for `EmptyView`, wrap it in a `ContentView` to avoid layout issues.

## Scrolling

### ScrollTo

Programmatically scroll by index or item:

```csharp
// Scroll to index
collectionView.ScrollTo(index: 10, position: ScrollToPosition.Center, animate: true);

// Scroll to item
collectionView.ScrollTo(item: myItem, position: ScrollToPosition.MakeVisible, animate: true);
```

| ScrollToPosition | Behavior |
|---|---|
| `MakeVisible` | Scrolls just enough to make the item visible |
| `Start` | Scrolls item to the start of the viewport |
| `Center` | Scrolls item to the center of the viewport |
| `End` | Scrolls item to the end of the viewport |

### Snap points

Control snap behavior after scrolling:

```xml
<CollectionView.ItemsLayout>
    <LinearItemsLayout Orientation="Horizontal"
                       SnapPointsType="MandatorySingle"
                       SnapPointsAlignment="Center" />
</CollectionView.ItemsLayout>
```

- `SnapPointsType`: `None`, `Mandatory`, `MandatorySingle`
- `SnapPointsAlignment`: `Start`, `Center`, `End`

## Incremental loading (infinite scroll)

Load more data as the user scrolls near the end:

```xml
<CollectionView ItemsSource="{Binding Items}"
                RemainingItemsThreshold="5"
                RemainingItemsThresholdReachedCommand="{Binding LoadMoreCommand}" />
```

Or handle the `RemainingItemsThresholdReached` event in code-behind.

> **Gotcha**: Do not use incremental loading with a `StackLayout`-based `ItemsLayout`.
> A `StackLayout` has no concept of virtualization and will trigger infinite threshold-reached events.
> Use the default `LinearItemsLayout` or `GridItemsLayout`.

## SwipeView integration

Wrap item content in `SwipeView` for swipe actions:

```xml
<CollectionView.ItemTemplate>
    <DataTemplate x:DataType="models:Item">
        <SwipeView>
            <SwipeView.RightItems>
                <SwipeItems>
                    <SwipeItem Text="Delete"
                               BackgroundColor="Red"
                               Command="{Binding Source={RelativeSource AncestorType={x:Type viewmodels:MainViewModel}}, Path=DeleteCommand}"
                               CommandParameter="{Binding}" />
                </SwipeItems>
            </SwipeView.RightItems>
            <Grid Padding="12">
                <Label Text="{Binding Name}" />
            </Grid>
        </SwipeView>
    </DataTemplate>
</CollectionView.ItemTemplate>
```

## Pull-to-refresh with RefreshView

Wrap `CollectionView` in a `RefreshView`:

```xml
<RefreshView IsRefreshing="{Binding IsRefreshing}"
             Command="{Binding RefreshCommand}">
    <CollectionView ItemsSource="{Binding Items}">
        <!-- ItemTemplate -->
    </CollectionView>
</RefreshView>
```

Set `IsRefreshing` back to `false` in the view model when the refresh completes.

## Common gotchas

| Issue | Fix |
|---|---|
| UI doesn't update when items change | Use `ObservableCollection<T>`, not `List<T>`. |
| App crashes or blank items | Never use `ViewCell` — use `Grid`, `StackLayout`, or any `View` as template root. |
| Items disappear or layout breaks | Always update `ItemsSource` and the collection on the **UI thread** (`MainThread.BeginInvokeOnMainThread`). |
| Incremental loading fires endlessly | Don't use `StackLayout` as layout; use `LinearItemsLayout` or `GridItemsLayout`. |
| EmptyView doesn't render correctly | Wrap custom empty views in `ContentView`. |
| Poor scroll performance | Use `MeasureFirstItem` sizing strategy for uniform item sizes. |
| Selected state not visible | Add `VisualState Name="Selected"` to the item template root element. |
| Binding errors in SwipeView commands | Use `RelativeSource AncestorType` to reach the view model from inside the item template. |

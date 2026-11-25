---
applyTo:
  - "src/Controls/src/Core/Handlers/Items/*.Windows.cs"
  - "src/Controls/src/Core/Handlers/Items/*.windows.cs"
  - "src/Controls/src/Core/Handlers/Items2/*.Windows.cs"
  - "src/Controls/src/Core/Handlers/Items2/*.windows.cs"
  - "src/Controls/src/Core/Handlers/Items2/Windows/**"
---
# CollectionView — Windows

On .NET 11, Items2/ is the default Windows CollectionView implementation. Items/ remains the obsolete CollectionView fallback behind `RuntimeFeature.IsWindowsCollectionView2HandlerEnabled` and the only Windows CarouselView implementation.

## Which Handler to Change

- CollectionView work targets `Items2/`.
- CarouselView work targets `Items/`.
- Change the Items/ CollectionView path only for explicit legacy fallback maintenance.
- Keep `ItemsViewHandler<TItemsView>` active because Windows CarouselView still derives from it; the Structured/Selectable/Groupable/Reorderable Items handler chain is CollectionView-specific and obsolete.

## WinUI ListView/ItemsRepeater Patterns
- Preserve WinUI XAML styles applied via native theming — clearing a MAUI property must restore the style-applied value, not a hardcoded default
- `double.NaN` is the WinUI convention for unconstrained dimensions — do not confuse with MAUI's `double.PositiveInfinity`
- Use `DispatcherQueue.TryEnqueue` for deferred UI thread work — do not use `Dispatcher.BeginInvoke`

## Data Source and Change Notifications
- Handle all `ObservableCollection` change actions (Add, Remove, Replace, Move, Reset) with range-scoped updates
- Avoid full source refresh (`NotifyDataSetChanged` equivalent) — it kills selection state and scroll position
- Selection mode changes must propagate correctly to the native `SelectionMode` property

## Layout Configuration
- `ItemsLayout` changes require reconfiguration of the underlying panel (e.g., `ItemsWrapGrid`, `ItemsStackPanel`)
- Verify that `ItemsLayout.Span` for grid layouts maps correctly to WinUI's `MaximumRowsOrColumns`

## Cross-Platform Consistency
- Default values for control properties must produce the same visual result as Android and iOS
- Event firing order (selection changed, scrolled) should match other platforms for the same user interaction

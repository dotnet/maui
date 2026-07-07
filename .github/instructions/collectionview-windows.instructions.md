---
applyTo:
  - "src/Controls/src/Core/Handlers/Items/*.Windows.cs"
  - "src/Controls/src/Core/Handlers/Items/*.windows.cs"
---
# CollectionView — Windows (Items/ Handler)

Items/ is the **ONLY** Windows CollectionView implementation. Items2/ has NO Windows code.

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

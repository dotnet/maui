# RefreshView + CollectionView2 on Windows — Investigation & Workaround

## Issue

The PullToRefresh indicator (RefreshVisualizer) is always visible when using CollectionView2 (CV2) inside a RefreshView on Windows. With CV1 it works correctly — the indicator only appears on pull.

## Root Cause

WinUI3's `RefreshContainer` internally uses a `ScrollViewerIRefreshInfoProviderAdapter` that walks the visual tree (BFS) looking for a `ScrollViewer` to hook up pull-to-refresh interaction tracking via an `InteractionTracker`.

- **CV1** uses `FormsListView`/`FormsGridView` which extend `ListView`/`GridView` — these have `ScrollViewer` in their control template → adapter finds it → works.
- **CV2** uses `MauiItemsView` which extends WinUI3's `ItemsView` — this uses the newer `ScrollView` control (`PART_ScrollView`) in its template → adapter doesn't recognize it → `RefreshVisualizer` stays stuck at its default visible position.

## Why We Cannot Replace ScrollView with ScrollViewer in CV2

CV2's `MauiItemsView` extends WinUI3's `ItemsView`, which is designed around the newer `ScrollView` control. `ItemsView`'s internal APIs — `ScrollTo`, scroll events, `ScrollBar` visibility, `ItemsUpdatingScrollMode` — are all wired to `ScrollView`. Replacing it with `ScrollViewer` would break `ItemsView`'s core scrolling infrastructure since they are different controls with different API surfaces.

## Approaches Explored

### 1. MauiItemsView Wrapper Approach

Inject a `ScrollViewer` wrapper around `MauiItemsView` when it detects it's hosted within a `RefreshContainer`.

- **Pros**: Native pull gesture would work since the adapter would find the injected `ScrollViewer`.
- **Cons**: Nested scrolling (ScrollViewer wrapping an ItemsView with its own internal ScrollView), visual tree manipulation, lifecycle complexity, tight coupling between `MauiItemsView` and `RefreshContainer`.
- **Verdict**: Rejected — reviewer feedback stated the fix belongs in `RefreshViewHandler`, not `MauiItemsView`.

### 2. RefreshViewHandler Source-Level Fix

Detect at load time whether the content tree contains a `ScrollViewer`. If not, manually control the `RefreshVisualizer`'s `Visibility` (collapsed when not refreshing, visible during programmatic refresh).

- **Pros**: Universal, no coupling to specific content types, no lifecycle bugs.
- **Cons**: Physical pull-down gesture won't work for CV2 content — only programmatic refresh.
- **Verdict**: Accepted as the correct location per reviewer direction.

### 3. App-Level Workaround via AppendToMapping (MauiProgram.cs)

Use `RefreshViewHandler.Mapper.AppendToMapping` to control visualizer visibility from the app's `MauiProgram.cs`.

- **Problem**: `PlatformView` / `Visualizer` is null at the time custom-key mappings fire — timing issues.
- **Verdict**: Unreliable, abandoned.

### 4. App-Level Workaround via HandlerChanged (Code-Behind)

Use `HandlerChanged` event on the `RefreshView` in code-behind to get the `RefreshContainer`, hook `Loaded` + `DispatcherQueue.TryEnqueue`, then collapse the visualizer. Includes `HasScrollViewerInVisualTree` detection to only apply for CV2.

- **Pros**: Reliable timing, selective (doesn't affect CV1).
- **Cons**: Still cannot restore physical pull gesture.
- **Verdict**: Works as a sample-level workaround if source changes aren't possible.

## Why Physical Pull-to-Refresh Cannot Work with CV2

The pull gesture is driven by `RefreshContainer`'s internal `InteractionTracker`, wired exclusively through a `ScrollViewer`. It uses the `ScrollViewer`'s scroll position to detect when the user is at the top of the list.

Even wrapping `MauiItemsView` in a `ScrollViewer` wouldn't work correctly — the wrapper would have zero scroll range (since `ItemsView` fills the viewport), so pull-to-refresh would trigger even when the user is scrolled halfway down the list.

This is a **WinUI3 platform limitation** — no `ScrollViewIRefreshInfoProviderAdapter` exists. Microsoft would need to add one in a future WinUI update.

## Current Options

1. **Source-level fix in RefreshViewHandler** — hide stuck indicator, programmatic refresh works, pull gesture lost for CV2.
2. **Use CV1** — if physical pull is a must-have requirement.
3. **Raise with WinUI team** — request `ScrollView` adapter support in `RefreshContainer`.

## Files Investigated

- `src/Core/src/Handlers/RefreshView/RefreshViewHandler.Windows.cs` — Platform handler for RefreshView on Windows
- `src/Controls/src/Core/Handlers/Items2/Windows/MauiItemsView.cs` — CV2's custom ItemsView control
- `src/Controls/src/Core/Handlers/Items2/ItemsViewHandler2.Windows.cs` — CV2 base handler
- `src/Controls/src/Core/Platform/Windows/CollectionView/ItemsViewStyles.xaml` — XAML templates for CV1 and CV2

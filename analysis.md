# Issue #37427 Fix Analysis — BindingContext Order + Inline Arrange

## Root Cause

Two independent issues combine to produce blank color bars in recycled CV2 cells:

### 1. BindingContext set AFTER handler creation (new cells)

CV2's `BindVirtualView` calls `ToPlatform(mauiContext)` (line 268) **before** setting
`BindingContext` (line 283). CV1 does the opposite — `BindingContext` is set first
(TemplatedCell.cs line 200), then the handler is created (line 202).

When `BindingContext` is set after `ToPlatform`, property-changed callbacks like
`HandlePreviewColorsPropertyChanged` fire and dynamically add children via
`LayoutHandler.Add()`. Each `Add()` calls `InvalidateAncestorsMeasures()` which walks
up the superview chain. If the cell does not yet have a `Window` (common during initial
creation), the walk reaches a `MauiView` that implements
`IPlatformMeasureInvalidationController` and finds `child.Window is null`, so it calls
`InvalidateAncestorsMeasuresWhenMovedToWindow()` and **returns early**.

For `TemplatedCell2`, `InvalidateAncestorsMeasuresWhenMovedToWindow` is a **no-op**,
so the invalidation is silently swallowed. The `MauiCollectionView.NeedsCellLayout`
flag is never set, and the layout is never invalidated for these cells.

### 2. Missing arrange pass for recycled cells

When recycled cells (PlatformHandler already exists) get new children via a
BindingContext change, the measure invalidation propagates correctly (cell has a
Window). `PreferredLayoutAttributesFittingAttributes` runs and calls `Measure`, setting
`_needsArrange = true`. However, UIKit may skip `LayoutSubviews` if the cell's frame
doesn't change (same card size), so `Arrange` is never called and the new children
retain zero-width frames.

## Fix Description

### Change 1: BindingContext before ToPlatform

Move `virtualView.BindingContext = bindingContext` **before** `ToPlatform()` in the
new-cell path. This matches CV1's binding order and ensures property-changed callbacks
fire before the handler tree exists. When `ToPlatform` runs, the Grid already has all
its children, so they are included in the initial layout pass — no invalidation needed.

### Change 2: Inline arrange in PreferredLayoutAttributesFittingAttributes

Replace the PR's `SetNeedsLayout()` with a direct `Arrange()` call inside
`PreferredLayoutAttributesFittingAttributes`, guarded by `_needsArrange`. After
computing the preferred frame, immediately arrange the virtual view with the computed
size. This eliminates the need for a separate `LayoutSubviews` pass and avoids the
timing issue where `LayoutSubviews` runs before `_needsArrange` is set.

## Performance Implications

### PR's SetNeedsLayout approach (REMOVED)
- Calls `SetNeedsLayout()` on every measurement in `PreferredLayoutAttributesFittingAttributes`
- Forces UIKit to schedule an additional `LayoutSubviews` → `Arrange` cycle for every
  cell that enters the `_measureInvalidated || constraints changed` branch
- On the steady-state scrolling hot path, this adds a UIKit layout pass per cell

### This fix: Inline Arrange
- The `_needsArrange` check is inside the `_measureInvalidated || constraints changed`
  gate — this block is **only entered** when content actually changes
- In steady-state scrolling (no content changes), `_measureInvalidated` is `false` and
  `_cachedConstraints == constraints`, so the entire block is skipped
- The `Arrange` call uses the size already computed in the same method, avoiding
  redundant measurement
- `_needsArrange` is set to `false` immediately, so any subsequent `LayoutSubviews` is
  a no-op (returns early due to `!_needsArrange`)
- **Net zero** additional work on the scrolling hot path

### BindingContext reorder
- Zero runtime cost — simply changes when an assignment happens within `BindVirtualView`
- Actually **reduces** work for new cells by eliminating the dynamic child addition path
  (children exist at handler creation time instead of being added post-creation)

## Files Changed

- `src/Controls/src/Core/Handlers/Items2/iOS/TemplatedCell2.cs` only

## Test Result

- `DynamicallyAddedContentRendersAfterCellRealization`: **PASSED** (0% diff from baseline)

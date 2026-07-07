---
applyTo:
  - "src/Controls/src/Core/Handlers/Items/Android/**"
  - "src/Controls/src/Core/Handlers/Items/*.Android.cs"
  - "src/Controls/src/Core/Handlers/Items/*.android.cs"
---
# CollectionView — Android (Items/ Handler)

> Items/ is the sole Android handler — see `collectionview-handler-detection.instructions.md` for the full platform→handler mapping.

## RecyclerView Adapter Patterns
- Use range-specific notifications (`NotifyItemRangeInserted`, `NotifyItemRangeRemoved`, `NotifyItemRangeChanged`) when INCC semantics provide exact affected ranges — prefer these over `NotifyDataSetChanged` for preserving scroll position and animations
- Full refresh via `NotifyDataSetChanged` is valid for `Reset` actions, ambiguous index cases, and header/footer template changes
- Handle all `ObservableCollection` change actions: Add, Remove, Replace, Move, Reset
- On `Reset`, recalculate adapter state from scratch — do not assume incremental consistency

## ViewHolder Recycling
- `BindingContext` MUST be updated on every rebind — stale data from a previous holder is a common source of visual glitches
- Do not store item-specific state in the ViewHolder outside of `BindViewHolder` — recycled holders carry state from previous items
- Dispose and recreate platform views only when the template changes, not on every rebind

## Layout Manager
- Select layout manager (Linear, Grid, custom) based on `ItemsLayout` specification — do not hardcode
- `ItemsLayout` property changes require full layout manager replacement, not partial reconfiguration
- Account for Android pixel rounding in item measurement — fractional dp values cause 1px gaps

## Memory and Lifecycle
- Unsubscribe from `ScrollChange` and adapter observers in `DisconnectHandler` — do NOT call Dispose on platform objects
- Scroll position restoration after adapter data changes must handle empty-collection edge case

## Regression Patterns
- Test across empty collection, single item, many items, and with grouping — a fix for one layout scenario routinely breaks another

---
applyTo:
  - "src/Core/src/Layouts/**"
  - "src/Controls/src/Core/Layout/**"
  - "src/Core/src/Handlers/Layout/**"
---
# Layout System Rules

## Measure/Arrange Contract
- `Measure(widthConstraint, heightConstraint)` returns the desired size — it must not mutate layout state or trigger side effects
- `ArrangeChildren(bounds)` positions children within the given bounds — it must respect the size returned from `Measure`, not compute independently
- A child measured with `widthConstraint=200` must not be arranged with `width=300` — constraints must stay consistent across passes
- Subtract padding, margin, and border thickness from constraints BEFORE passing to children

## Constraint Propagation
- Stack layouts pass `double.PositiveInfinity` along the stacking axis and the parent constraint along the cross axis
- Grid cells compute per-cell constraints from column/row definitions — do not pass the full grid constraint to each child
- `MeasureContent` helpers on `IContentView` handle inset subtraction — use them instead of manual arithmetic
- On Windows, `double.NaN` represents unconstrained dimensions (WinUI convention) — do not confuse with `double.PositiveInfinity` (MAUI convention)

## Infinite Loop Avoidance
- Never create circular dependencies where child size depends on parent AND parent size depends on child
- Layout invalidation (`InvalidateMeasure`) must not re-trigger during an active measure pass
- ScrollView content must always be re-measured on layout trigger — do not aggressively cache child measurements in scrollable containers

## Performance on Hot Paths

> Layout measure/arrange methods are hot paths. All rules from `performance-hotpaths.instructions.md` apply — no LINQ, closures, or allocations. Additionally for layout:
- Cache expensive computations (e.g., `GridStructure`) and invalidate only when inputs change
- Bindable property change handlers should skip layout invalidation when the value has not actually changed

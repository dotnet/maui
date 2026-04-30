---
applyTo:
  - "src/Core/src/Layouts/**"
  - "src/Controls/src/Core/Handlers/**"
---
# Performance-Critical Path Rules

## Hot Paths in MAUI
Measure/arrange cycles, scrolling callbacks, binding propagation, and property change notifications are called at high frequency. All rules below apply to code on these paths.

## Allocation Avoidance
- No LINQ methods (`.Where`, `.Select`, `.FirstOrDefault`, `.ToList`) — use indexed `for` loops
- No closures or lambdas that capture variables — these allocate a compiler-generated class per invocation
- No string concatenation or interpolation — use `StringBuilder` or pre-allocated strings if logging is required
- Prefer `Count` + indexer over `IEnumerable` iteration to avoid enumerator allocation

## Caching and Invalidation
- Cache results of expensive computations called multiple times per layout pass
- Invalidate caches when inputs change — stale caches cause incorrect layout
- Skip redundant work: if a property's new value equals the old value, do not trigger layout invalidation or re-render

## Collection Iteration
- When the source implements `IList` or `IReadOnlyList`, use `for (int i = 0; i < list.Count; i++)` instead of `foreach`
- `ObservableCollection` change handlers should process only the affected range, not re-enumerate the entire collection

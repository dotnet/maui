# MAUI Layout Engine — Initial Performance Analysis

> **Config:** ShortRun (3 iterations, 1 warmup, 1 launch) · Apple M1 Max · .NET 10 · Release
> **Note:** Allocation deltas are deterministic (reliable). Mean time deltas have high variance (ShortRun CIs ±100–900%).

## Benchmark Results

### Stack Loops

| Benchmark | Baseline Mean | Post Mean | Δ Mean | Baseline Alloc | Post Alloc | Δ Alloc |
|---|---:|---:|---:|---:|---:|---:|
| Vertical Stack (12, spans) | 6,443.4 us | 4,070.0 us | 🟢 -36.8% | 2.21 MB | 1.77 MB | 🟢 -19.8% |
| Horizontal Stack (60) | 20,245.0 us | 18,960.1 us | 🟢 -6.3% | 10.28 MB | 8.34 MB | 🟢 -18.8% |
| Horizontal Stack (12) | 4,501.0 us | 5,381.7 us | 🔴 +19.6% | 2.16 MB | 1.77 MB | 🟢 -18.1% |
| Vertical Stack (12) | 4,492.4 us | 4,414.7 us |  -1.7% | 2.15 MB | 1.77 MB | 🟢 -17.4% |
| Horizontal Stack (12, spans) | 5,880.6 us | 3,874.3 us | 🟢 -34.1% | 2.10 MB | 1.77 MB | 🟢 -15.7% |
| Horizontal Stack (60, spans) | 24,976.3 us | 24,202.6 us | 🟢 -3.1% | 9.64 MB | 8.34 MB | 🟢 -13.4% |
| Vertical Stack (60, spans) | 18,523.7 us | 19,044.9 us |  +2.8% | 10.76 MB | 9.50 MB | 🟢 -11.7% |
| Vertical Stack (60) | 18,987.6 us | 21,800.0 us | 🔴 +14.8% | 10.76 MB | 9.50 MB | 🟢 -11.7% |

### Flex Loops

| Benchmark | Baseline Mean | Post Mean | Δ Mean | Baseline Alloc | Post Alloc | Δ Alloc |
|---|---:|---:|---:|---:|---:|---:|
| Flex No Wrap (12) | 279.0 us | 256.6 us | 🟢 -8.0% | 85.94 KB | 82.03 KB | 🟢 -4.5% |
| Flex No Wrap (12, spans) | 319.6 us | 277.7 us | 🟢 -13.1% | 85.94 KB | 82.03 KB | 🟢 -4.5% |
| Flex Wrap (12) | 267.8 us | 340.0 us | 🔴 +27.0% | 89.84 KB | 85.94 KB | 🟢 -4.3% |
| Flex Wrap (12, spans) | 285.6 us | 255.0 us | 🟢 -10.7% | 89.84 KB | 85.94 KB | 🟢 -4.3% |
| Flex No Wrap (60) | 2,010.6 us | 1,789.9 us | 🟢 -11.0% | 404.69 KB | 400.78 KB | 🟢 -1.0% |
| Flex No Wrap (60, spans) | 1,986.3 us | 1,770.2 us | 🟢 -10.9% | 404.69 KB | 400.78 KB | 🟢 -1.0% |
| Flex Wrap (60) | 2,331.7 us | 1,847.8 us | 🟢 -20.8% | 408.60 KB | 404.69 KB | 🟢 -1.0% |
| Flex Wrap (60, spans) | 2,004.8 us | 1,791.8 us | 🟢 -10.6% | 408.59 KB | 404.69 KB | 🟢 -1.0% |

### Grid Spans

| Benchmark | Baseline Mean | Post Mean | Δ Mean | Baseline Alloc | Post Alloc | Δ Alloc |
|---|---:|---:|---:|---:|---:|---:|
| Grid (12) | 6,127.9 us | 4,297.6 us | 🟢 -29.9% | 2.68 MB | 2.30 MB | 🟢 -14.2% |
| Grid (12, spans) | 4,142.5 us | 4,669.1 us | 🔴 +12.7% | 2.44 MB | 2.42 MB | 🟢 -0.7% |
| Grid (60, spans) | 20,147.0 us | 20,240.1 us |  +0.5% | 10.59 MB | 11.00 MB | 🔴 +3.8% |
| Grid (60) | 19,672.0 us | 22,695.8 us | 🔴 +15.4% | 10.27 MB | 10.77 MB | 🔴 +4.9% |

### Invalidation

| Benchmark | Baseline Mean | Post Mean | Δ Mean | Baseline Alloc | Post Alloc | Δ Alloc |
|---|---:|---:|---:|---:|---:|---:|
| Propagate Measure Invalidation Through Legacy Layout | — | 2.5 us |  — | — | — |  — |
| Propagate Measure Invalidation Through Page | — | 2.7 us |  — | — | — |  — |
| Raise Measure Invalidated With Subscriber | — | 4.9 us |  — | — | — |  — |

### Grid (pre-existing)

| Benchmark | Baseline Mean | Post Mean | Δ Mean | Baseline Alloc | Post Alloc | Δ Alloc |
|---|---:|---:|---:|---:|---:|---:|
| Grid AllStars (5×5) | 54.2 us | 90.5 us | 🔴 +67.1% | 17.74 KB | 17.71 KB |  -0.2% |
| Grid AllAbsolute (5×5) | 27.8 us | 62.8 us | 🔴 +126.0% | 15.99 KB | 17.38 KB | 🔴 +8.7% |
| Grid AllAuto (5×5) | 33.9 us | 56.0 us | 🔴 +65.3% | 15.99 KB | 17.38 KB | 🔴 +8.7% |

### Flex (pre-existing)

| Benchmark | Baseline Mean | Post Mean | Δ Mean | Baseline Alloc | Post Alloc | Δ Alloc |
|---|---:|---:|---:|---:|---:|---:|
| Flex 100 items Wrap | 2.8 ms | 3.1 ms | 🔴 +12.7% | 1.40 MB | 1.40 MB |  0.0% |
| Flex 100 items NoWrap | 2.5 ms | 2.6 ms | 🔴 +4.6% | 1.40 MB | 1.40 MB |  0.0% |

## Summary by Optimization Track

| Track | What Changed | Alloc Δ | Status |
|---|---|---|---|
| **Stack loops** | `foreach` → indexed `for`, cached `Count`/`Spacing` | **−12% to −20%** | ✅ Clean win |
| **Flex loops** | `foreach` → indexed `for`, cached `Count` | **−1% to −5%** | ✅ Modest improvement |
| **Grid spans** | `Dict<SpanKey,Span>` → `Dict<SpanKey,double>`, `IEquatable` | **−14%** (12ch) / **+4–5%** (60ch) | ⚠️ Regresses at scale |
| **InvalidationEventArgs** | Static cached singletons via `GetCached()` | **0 B** per dispatch | ✅ Complete elimination |

---

## Deep Dive: Remaining Allocation Sources

### Flex Layout Engine (`Flex.cs` + `FlexLayoutManager.cs`)

The `foreach` → `for` optimization only shaved 1–5% because the vast majority of Flex allocations
come from the internal `Flex.cs` C-port layout engine. Key hotspots:

| Source | Location | Frequency | Impact |
|---|---|---|---|
| `new float[4]` (Item.Frame) | `Flex.cs` Item ctor | Per flex item creation | Medium — 1 array per child |
| `float[] size = { w, h }` | `Flex.cs` layout_item ~L508 | Per child with SelfSizing | **High** — called during every measure |
| `Array.Resize(ref layout.lines)` | `Flex.cs` ~L863 | Per wrapped line | **High** — grows by 1 per line in wrap mode |
| `new int[item.Count]` (ordered_indices) | `Flex.cs` flex_layout.init ~L993 | Per layout pass when ordering | Medium — conditional |
| `new Rect(...)` in GetFlexFrame | `FlexExtensions.cs` ~L11 | Per child per Measure+Arrange | Medium — struct, but per-child |
| SelfSizing delegate/lambda | `FlexLayout.cs` | Per child | Medium — closure captures child ref |

**Opportunities:**

1. **`float[] size` in SelfSizing callback** — Replace 2-element array with a `ValueTuple<float,float>` or pass width/height as separate out params
2. **`Array.Resize` for lines** — Pre-allocate lines array to estimated count (e.g., `childCount / avgItemsPerLine`), or use `List<flex_layout_line>`
3. **`float[4]` Frame per Item** — Replace with 4 individual float fields (`FrameX`, `FrameY`, `FrameW`, `FrameH`)
4. **SelfSizing delegate** — Cache delegate per child or use static method with explicit child parameter

### Grid Layout Engine (`GridLayoutManager.cs`)

The span-tracking Dictionary optimization helped at 12 children but regressed at 60. Root causes and remaining hotspots:

| Source | Location | Frequency | Impact |
|---|---|---|---|
| `Dictionary<SpanKey,double> _spans` | GridStructure field L83 | 1 per layout | **High** — resizes at 3→7→17→37→73 capacity |
| `Cell[]` + `new Cell(...)` per child | GridStructure ctor ~L140, ~L228 | 1 array + N objects | **High** — Cell is a **class** (heap alloc per child) |
| `IView[] _childrenToLayOut` + `Array.Resize` | GridStructure ctor ~L123–136 | 1 + possible resize | Medium — hidden resize if collapsed children |
| `Definition[]` + `new Definition(...)` | InitializeRows/Columns ~L155–195 | 2 arrays + N objects | Low — typically 6–10 Definition objects |
| `new GridStructure(...)` in ArrangeChildren | ArrangeChildren L34 | 1 per arrange (if no prior measure) | High — full re-creation |

**Why the Dictionary regresses at 60 children:**

- Initial capacity = 0; resizes at 3, 7, 17, 37, 73 entries
- At 60 children with ~30% spanning → 18–24 TrackSpan calls → 3–4 resize operations
- Each resize allocates new bucket+entry arrays and rehashes all keys
- The per-entry savings (smaller `double` vs `Span` struct) are overwhelmed by resize overhead

**Opportunities:**

1. **Pre-size the Dictionary** — estimate span count from child count: `new Dictionary<SpanKey,double>(childCount / 2)`
2. **Cell class → struct** — Cell has ~48 bytes of fields; making it a struct eliminates 60 heap allocations at 60 children
3. **Definition class → struct** — Similar to Cell, avoids heap allocations for row/column definitions
4. **Avoid `Array.Resize` for `_childrenToLayOut`** — Count non-collapsed children first, allocate exact size
5. **Replace Dictionary with sorted list** — For small span counts (<30), `List<(SpanKey, double)>` with linear scan beats Dictionary overhead

---

## Next Steps

### Phase 2 — Deeper Flex Optimizations
- [x] Replace `float[] size` in SelfSizing with value tuple or out params → **−42% to −48% alloc**
- [x] Replace `float[4]` Frame with InlineArray(4) struct → eliminates per-item array
- [x] Pre-allocate `lines` array in flex_layout → doubling strategy
- [ ] Cache SelfSizing delegates (diminishing returns)

### Phase 2 — Deeper Grid Optimizations
- [ ] ~~Pre-size `_spans` Dictionary based on child count~~ → REVERTED (wasted memory)
- [x] Convert `Cell` from class to struct → eliminates N heap allocs per measure
- [x] Convert `Definition` from class to struct → eliminates heap allocs
- [x] Eliminate `Array.Resize` for `_childrenToLayOut` → track count instead
- [x] Lazy `_spans` Dictionary → −5% for no-span grids
- [x] `GridStructure` class → struct → saves ~200B per measure
- [x] ArrayPool for IView[], Cell[], Definition[] arrays → **massive** reduction
- [x] `foreach` → `for` in ArrangeChildren → eliminates enumerator boxing

### Phase 2 — Validation
- [x] Verify no behavioral regressions via existing unit tests (501 tests pass)
- [x] Multi-target build (net10.0, netstandard2.0) ✅

---

## Final Results — True Allocation Benchmarks (Fake Objects, No NSubstitute Noise)

> **Benchmark:** LayoutAllocBenchmarker — 50 Measure+Arrange loops per iteration
> All numbers from the same machine, same commit, ShortRun (3 iter).
> Apple M1 Max, .NET 10.0.1, Release build.

### Grid — Complete Allocation Elimination (ALL Scenarios)

| Scenario | Baseline Alloc | Final Alloc | Δ Alloc | Δ % |
|---|---:|---:|---:|---:|
| **12 children, no spans** | 87,110 B | **0 B** | −87,110 B | 🟢 **−100%** |
| **12 children, with spans** | 125,390 B | **0 B** | −125,390 B | 🟢 **−100%** |
| **60 children, no spans** | 307,420 B | **0 B** | −307,420 B | 🟢 **−100%** |
| **60 children, with spans** | 457,420 B | **0 B** | −457,420 B | 🟢 **−100%** |

### Grid — Timing (50 Measure+Arrange loops)

| Scenario | Mean | Notes |
|---|---:|---|
| 12 children, no spans | 43.2 μs | Zero GC pressure |
| 12 children, with spans | 47.7 μs | Zero GC pressure (Dictionary reused) |
| 60 children, no spans | 528.6 μs | Zero GC pressure |
| 60 children, with spans | 567.4 μs | Zero GC pressure (Dictionary reused) |

### Stack — Zero Allocations (All Scenarios)

| Scenario | Allocated |
|---|---:|
| VStack 12 children | 0 B |
| HStack 12 children | 0 B |
| VStack 60 children | 0 B |
| HStack 60 children | 0 B |

### Flex — Reduced via ArrayPool (Hotpath Benchmark, Real Controls Objects)

| Scenario | Phase 1 Alloc | Final Alloc | Δ Alloc | Δ % |
|---|---:|---:|---:|---:|
| Flex Wrap 12ch | 85.94 KB | **37.5 KB** | −48.44 KB | 🟢 **−56.4%** |
| Flex NoWrap 12ch | 85.94 KB | **37.5 KB** | −48.44 KB | 🟢 **−56.4%** |
| Flex Wrap 60ch | 408.60 KB | **187.5 KB** | −221.10 KB | 🟢 **−54.1%** |
| Flex NoWrap 60ch | 404.69 KB | **187.5 KB** | −217.19 KB | 🟢 **−53.7%** |
| HStack 12 children | 0 B |
| VStack 60 children | 0 B |
| HStack 60 children | 0 B |

---

## Optimization Summary

| # | Optimization | Files Changed | Alloc Impact | Status |
|---|---|---|---|---|
| 1 | Stack `foreach` → `for`, cached Count/Spacing | VerticalStackLM, HorizontalStackLM, StackLM | −12–20% (mock), 0→0 (real) | ✅ |
| 2 | Flex `foreach` → `for` | FlexLayoutManager | −1–5% | ✅ |
| 3 | Flex `float[] size` → locals | Flex.cs SelfSizing | −42–48% of flex alloc | ✅ |
| 4 | Flex `float[4] Frame` → InlineArray | Flex.cs Item | Eliminates per-item array | ✅ |
| 5 | Flex lines array doubling + ArrayPool | Flex.cs | Eliminates per-line resize, pools arrays | ✅ |
| 6 | Flex `ordered_indices` ArrayPool | Flex.cs | Pools sort indices array | ✅ |
| 7 | Grid `Cell` class → struct | GridLayoutManager | Eliminates N heap allocs | ✅ |
| 8 | Grid `Definition` class → struct | GridLayoutManager | Eliminates N heap allocs | ✅ |
| 9 | Grid `GridStructure` class → struct | GridLayoutManager | −200B per measure | ✅ |
| 10 | Grid ArrayPool for all arrays | GridLayoutManager | **−100%** for Grid | ✅ |
| 11 | Grid lazy `_spans` Dictionary + reuse | GridLayoutManager | Dictionary shared across passes | ✅ |
| 12 | Grid `foreach` → `for` in ArrangeChildren | GridLayoutManager | Eliminates enumerator boxing | ✅ |
| 13 | Grid SpanKey `IEquatable` + `HashCode.Combine` | GridLayoutManager | Better Dictionary perf | ✅ |
| 14 | InvalidationEventArgs cached singletons | InvalidationEventArgs, VisualElement, Page, Layout | 0 B per dispatch | ✅ |

---

## Final Benchmark Results (Core-Layer, Fake Objects)

All layout engines achieve **zero managed allocations** in the Core layer:

| Benchmark | ChildCount | Allocated |
|---|---:|---:|
| GridMeasureArrange (NoSpan) | 12 | **0 B** |
| GridMeasureArrange (Span) | 12 | **0 B** |
| GridMeasureArrange (NoSpan) | 60 | **0 B** |
| GridMeasureArrange (Span) | 60 | **0 B** |
| VerticalStackMeasureArrange | 12 | **0 B** |
| HorizontalStackMeasureArrange | 12 | **0 B** |
| FlexCoreMeasureArrange | 12 | **0 B** |
| VerticalStackMeasureArrange | 60 | **0 B** |
| HorizontalStackMeasureArrange | 60 | **0 B** |
| FlexCoreMeasureArrange | 60 | **0 B** |

## Flex Controls-Layer Allocation Analysis

Remaining Flex allocations (37.5 KB for 12ch, 187.5 KB for 60ch) trace to the Controls-layer
`VisualElement.ArrangeOverride` → `UpdateBoundsComponents` path:

- Each `child.Arrange(frame)` calls `UpdateBoundsComponents` which sets `X`, `Y`, `Width`, `Height` via `BindableObject.SetValue`
- `SetValue` on `double` properties boxes the value (24 bytes per double × 4 properties = 96 bytes)
- Confirmed by scaling test: **64 B per child per loop** (768 B for 12 children, 3840 B for 60 children)
- This is Controls-layer `BindableObject` infrastructure shared by ALL layout types
- Optimizing this would require changes to the `BindableProperty` system (typed property accessors to avoid boxing)

## Key Technical Decisions

1. **ArrayPool with count tracking** — Rented arrays are larger than requested; all loops use `_childCount`, `_rowCount`, `_columnCount` instead of `.Length`
2. **Dictionary reuse** — `_spansDictionary` field on `GridLayoutManager` is passed into `GridStructure` and `.Clear()`'d instead of reallocated
3. **Struct conversions** — `Cell`, `Definition`, `GridStructure` converted to structs with `ref` parameter patterns to avoid defensive copies
4. **InlineArray(4)** — Conditional on `NET8_0_OR_GREATER` with `float[]` fallback for netstandard
5. **NSubstitute benchmark noise** — Created `LayoutAllocBenchmarker` with lightweight fakes for TRUE allocation measurements; NSubstitute mocks add 40-200% allocation noise


# [Perf] Optimize memory usage of control elements

## Summary

Reduce per-instance memory usage and startup allocations by optimizing the **shared underlying data structures** in the control base chain:

`View` → `VisualElement` → `NavigableElement` → `StyleableElement` → `Element` → `BindableObject`

This issue focuses on structure-level costs (dictionaries, lists, sets, arrays, weak refs, lazy wrappers, delegate objects), not on control-specific feature work.

## Scope

**In scope:**
- Shared base-chain storage/layout costs that all controls pay
- Eager allocations and expansion patterns in core data structures
- Structure-level replacements and lazy-init strategies validated by benchmarks

**Out of scope:**
- Control-specific candidates (Shell, ListView, SwipeView, Menu*, Toolbar*, etc.)
- Handler-specific one-offs not part of base-chain shared costs
- API/behavior changes without dedicated compatibility review

## Goals

1. Exhaustively catalog structure-centric optimization opportunities
2. Prioritize low-risk lazy/deferred allocation changes first
3. Define a benchmark ladder from `new Label()` to app-like startup emulation
4. Use measurements to gate each optimization

## Guardrails

- No behavior changes in binding, layout, styling, navigation, or gesture semantics
- No pooling unless measured allocation wins clearly outweigh complexity
- High-risk core pathways (`_properties`, layout internals) require dedicated micro + integration benchmarks before code changes

---

## Data-structure opportunity list

The table below catalogs all identified optimization candidates in the base chain. Each candidate (DS01–DS34) represents one concrete storage/allocation site.

<details>
<summary><strong>DS01–DS34 opportunity table (click to expand)</strong></summary>

| ID | Structure family | Current usage in base chain | Opportunity | Expected allocation effect | Risk |
|---|---|---|---|---|---|
| DS01 | `Dictionary<BindableProperty, BindablePropertyContext>` | `BindableObject._properties` eager init (cap 4) | Evaluate lower initial capacity / compact storage strategy | Lower baseline dictionary + bucket footprint | High |
| DS02 | `Dictionary<TriggerBase, SetterSpecificity>` | `BindableObject._triggerSpecificity` eager init | Lazy-init on first trigger usage | Avoid dictionary alloc for trigger-free objects | Low |
| DS03 | `Dictionary<Size, SizeRequest>` | `VisualElement._measureCache` eager init | Lazy-init on first measure cache write | Avoid cache alloc for simple/one-shot elements | Medium |
| DS04 | `Dictionary<BindableProperty,(string,SetterSpecificity)>` | `Element._dynamicResources` lazy field but often forced early | Ensure true on-demand path everywhere | Reduce early dictionary creation | Low |
| DS05 | `Dictionary<BindableProperty,(string,SetterSpecificity)>` | `MergedStyle._defaultStyleProperties` used by style plumbing | Delay creation until first implicit/class style mutation | Reduce style dictionary overhead in unstyled cases | Medium |
| DS06 | `HashSet<string>` | `Element._pendingHandlerUpdatesFromBPSet` | Strict lazy-init path only when handler update batching starts | Avoid set alloc for elements without pending updates | Low |
| DS07 | `Int32[]` bucket arrays | Backing for dictionaries/hashsets | Reduce initial capacities where safe | Smaller initial bucket arrays | Medium |
| DS08 | `Entry<TKey,TValue>[]` arrays | Dictionary entries arrays | Delay growth; tune first-resize thresholds | Fewer entry-array allocations | Medium |
| DS09 | `SetterSpecificityList<object>` | BindablePropertyContext.Values | Lazy-init when non-default value path is used | Avoid list alloc on untouched/default properties | Low |
| DS10 | `SetterSpecificityList<BindingBase>` | BindablePropertyContext.Bindings | Lazy-init only when bindings are attached | Avoid bindings list alloc for non-bound properties | Low |
| DS11 | `Queue<SetValueArgs>`-style delayed setters | BindablePropertyContext delayed work | Lazy-init only during batched set scenarios | Avoid queue alloc for non-batched paths | Low |
| DS12 | `Object[]` backing arrays | `SetterSpecificityList<object>.Values` | Tighten initial capacity / delayed allocate backing | Fewer backing-array allocations | Low |
| DS13 | `BindableProperty[]` backing arrays | `SetterSpecificityList` keys | Delay keys array creation until second insertion | Avoid arrays for singleton/default cases | Low |
| DS14 | `SetterSpecificity[]` backing arrays | `SetterSpecificityList` specificities | Delay/init with smaller footprint | Reduce small-array overhead | Low |
| DS15 | `List<BindableProperty>` | `MergedStyle._implicitStyles` | Delay list creation until first implicit style registration | Avoid list alloc in style-light trees | Medium |
| DS16 | `IList<BindableProperty>` / `IList<Style>` | `MergedStyle` class-style tracking | Allocate only when `StyleClass` is actually set | Avoid class-style list allocations | Low |
| DS17 | `IList<Element>` / `List<Element>` | `Element._internalChildren` | Keep null or shared empty until first child add | Save child-list alloc for leaf elements | Low |
| DS18 | `List<Action<object,ResourcesChangedEventArgs>>` | `Element._changeHandlers` | Single-subscriber fast path + lazy list promotion | Avoid list alloc in 0/1-handler cases | Low |
| DS19 | `IList<BindableObject>` | `Element._bindableResources` | Strict lazy-init and avoid pre-creation | Save list alloc when no bindable resources | Low |
| DS20 | `TrackableCollection<Effect>` | `Element._effects` | Lazy-create only on first effects access | Save collection alloc for effect-free elements | Low |
| DS21 | `ObservableCollection<IGestureRecognizer>` | `View._gestureRecognizers` eager init | Lazy-init on first gesture registration | Remove always-paid collection cost on gesture-free views | Medium |
| DS22 | `List<IGestureRecognizer>` | Backing list inside `ObservableCollection` | Delay internal list creation via lazy outer collection | Avoid nested list alloc when unused | Medium |
| DS23 | `ObservableCollection<IGestureRecognizer>` | `View._compositeGestureRecognizers` | Maintain strict on-demand path | Prevent accidental eager creation | Low |
| DS24 | `Lazy<List<Page>>` | `NavigationProxy` push stack wrapper | Replace wrapper with nullable list + manual lazy init | Remove `Lazy` + `LazyHelper` overhead | Medium |
| DS25 | `Lazy<NavigatingStepRequestList>` | `NavigationProxy` modal stack wrapper | Replace wrapper with nullable field + manual lazy init | Remove `Lazy` + `LazyHelper` overhead | Medium |
| DS26 | `Lazy<T>` wrappers (general) | Base-path internals where thread-safe lazy is unnecessary | Convert to nullable field + `??=` in single-threaded UI paths | Remove wrapper/helper/delegate objects | Medium |
| DS27 | `LazyHelper` internal objects | Created by `Lazy<T>` | Eliminated indirectly by DS24–DS26 | Fewer helper allocations per instance | Medium |
| DS28 | `Func<T>` factory delegates | Factory delegates captured by `Lazy<T>` | Replace with direct initializer methods and null checks | Remove delegate allocations | Low |
| DS29 | Delegate objects (`EventHandler*`) | Constructor-time subscriptions and event fields | Defer subscriptions where safe; static delegate caching | Fewer per-instance delegate allocations | Medium |
| DS30 | `WeakReference` / `WeakReference<Element>` | `_inheritedContext`, `_parentOverride`, `_realParent` | Audit for nullable direct refs + explicit lifecycle points | Reduce weak-ref object churn | Medium |
| DS31 | Weak proxy wrappers | Background/clip/shadow proxy wrappers | Ensure strict on-demand creation and reuse patterns | Avoid proxy alloc on feature-unused elements | Low |
| DS32 | Boolean flag fields | Multiple bools in `VisualElement`/`Element`/`BindableObject` | Bit-pack to a compact flags field | Reduce object size + padding waste | Medium |
| DS33 | Small scalar state (`ushort`, nullable ids) | `_triggerCount`, `_id` and related state | Pack/co-locate infrequent state with flags or deferred generation | Minor but broad per-instance savings | Low |
| DS34 | Struct-heavy state slots | `_frame`, mock bounds and similar fields | Verify necessity of always-live slots vs deferred state blocks | Potential object-size reduction | High |

</details>

### Recommended evaluation order

1. **DS02** — lazy-init `_triggerSpecificity` dictionary
2. **DS04** — enforce true on-demand `_dynamicResources` dictionary creation
3. **DS17** — keep `_internalChildren` null/shared-empty until first child add
4. **DS20** — lazy-create `TrackableCollection<Effect>`
5. **DS18** — single-subscriber fast path for `_changeHandlers`
6. **DS21/DS22** — lazy gesture collection + backing list
7. **DS10** — lazy `BindablePropertyContext.Bindings` list
8. **DS24/DS25/DS26** — replace selected `Lazy<T>` wrappers with nullable fields where safe
9. **DS29** — defer/canonicalize delegate allocations
10. **DS03** — lazy `_measureCache` with perf guardrails

---

## Benchmarking plan

### Benchmark ladder

| Level | Scenario | What it measures |
|---|---|---|
| B0 | `new object()` / empty baseline | Harness + runtime floor |
| B1 | `new Label()` baseline | Constructor + eager base-chain costs |
| B2 | `new Label()` + minimal property set (`Text`, `IsVisible`) | Property-context growth and setter-specificity paths |
| B3 | `new Label()` + feature toggles (gestures, effects, dynamic resources, style class, navigation) | On-demand data-structure activation deltas |
| B4 | ResourceDictionary style materialization | MergedStyle/default-style structures and dictionary growth |
| B5 | Mock visual tree startup (500–2000 controls) | Aggregated startup allocations from shared base costs |
| B6 | App-like startup emulation (Application + Window + Page + styles) | End-to-end startup-like allocation/time envelope |

### Required benchmark dimensions

- **Allocation**: `Allocated` bytes/op, Gen0/Gen1 counts
- **Time**: mean, p95, stddev
- **Object count**: per-type counts via `gcdump` spot checks
- **Delta reporting**: before/after per DS candidate and grouped rollout

### Success criteria

- Every accepted DS change shows a non-noise allocation improvement at its relevant benchmark level
- No statistically significant startup regression in B5/B6
- No functional regressions in existing tests

---

## Allocation breakdown: `new Label()` — 3,112 bytes

> Measured on .NET 11 Preview 1 (arm64, macOS) using `GC.GetAllocatedBytesForCurrentThread()` and validated with `dotnet-gcdump` (10,000 Labels, full GC, heap snapshot).
>
> Inheritance chain: `Label` → `View` → `VisualElement` → `NavigableElement` → `StyleableElement` → `Element` → `BindableObject` → `object`

### Field summary

| Declaring class | Fields | Ref fields | Value fields | Raw field bytes |
|---|---:|---:|---:|---:|
| BindableObject | 9 | 7 | 2 | 59 |
| StyleableElement | 1 | 1 | 0 | 8 |
| Element | 26 | 25 | 1 | 224 |
| NavigableElement | 0 | 0 | 0 | 0 |
| VisualElement | 38 | 21 | 17 | 260 |
| View | 5 | 5 | 0 | 40 |
| Label | 1 | 1 | 0 | 8 |
| **Total** | **80** | **60** | **20** | **599** |

Object header (MethodTable ptr + sync block) adds 16 bytes. With alignment padding the Label object occupies **664 bytes** on the GC heap.

The remaining **~2,448 bytes** come from **29 subsidiary heap objects** allocated eagerly in constructors and field initializers.

<details>
<summary><strong>Per-field allocation detail (click to expand)</strong></summary>

### BindableObject — 9 fields (59 B raw)

| # | Field | Type | Bytes | Kind |
|---|---|---|---:|---|
| 1 | `_dispatcher` | `IDispatcher` | 8 | ref |
| 2 | `_triggerCount` | `ushort` | 2 | value |
| 3 | `_triggerSpecificity` | `Dictionary<TriggerBase, SetterSpecificity>` | 8 | ref |
| 4 | `_properties` | `Dictionary<BindableProperty, BindablePropertyContext>` | 8 | ref |
| 5 | `_applying` | `bool` | 1 | value |
| 6 | `_inheritedContext` | `WeakReference` | 8 | ref |
| 7 | `PropertyChanged` (event) | `PropertyChangedEventHandler` | 8 | ref |
| 8 | `PropertyChanging` (event) | `PropertyChangingEventHandler` | 8 | ref |
| 9 | `BindingContextChanged` (event) | `EventHandler` | 8 | ref |

### StyleableElement — 1 field (8 B raw)

| # | Field | Type | Bytes | Kind |
|---|---|---|---:|---|
| 10 | `_mergedStyle` | `MergedStyle` | 8 | ref |

### Element — 26 fields (224 B raw)

| # | Field | Type | Bytes | Kind |
|---|---|---|---:|---|
| 11 | `_bindableResources` | `IList<BindableObject>` | 8 | ref |
| 12 | `_changeHandlers` | `List<Action<object, ResourcesChangedEventArgs>>` | 8 | ref |
| 13 | `_dynamicResources` | `Dictionary<BindableProperty, (string, SetterSpecificity)>` | 8 | ref |
| 14 | `_effectControlProvider` | `IEffectControlProvider` | 8 | ref |
| 15 | `_effects` | `TrackableCollection<Effect>` | 8 | ref |
| 16 | `_id` | `Guid?` | 24 | value |
| 17 | `_parentOverride` | `WeakReference<Element>` | 8 | ref |
| 18 | `_styleId` | `string` | 8 | ref |
| 19 | `_logicalChildrenReadonly` | `IReadOnlyList<Element>` | 8 | ref |
| 20 | `_internalChildren` | `IList<Element>` | 8 | ref |
| 21 | `_realParent` | `WeakReference<Element>` | 8 | ref |
| 22 | `transientNamescope` | `INameScope` | 8 | ref |
| 23 | `_pendingHandlerUpdatesFromBPSet` | `HashSet<string>` | 8 | ref |
| 24 | `_currentPropertyBeingSet` | `BindableProperty` | 8 | ref |
| 25 | `_handler` | `IElementHandler` | 8 | ref |
| 26 | `_effectsFactory` | `EffectsFactory` | 8 | ref |
| 27 | `_previousHandler` | `IElementHandler` | 8 | ref |
| 28–36 | Events (9 fields) | Various `EventHandler<T>` | 72 | ref |

### NavigableElement — 0 fields

No instance fields. Constructor sets `Navigation = new NavigationProxy()` which goes through `SetValue` on a `BindableProperty` (stored in `_properties` dictionary, not as a field).

### VisualElement — 38 fields (260 B raw)

| # | Field | Type | Bytes | Kind |
|---|---|---|---:|---|
| 37–38 | `_inputTransparentExplicit`, `_isEnabledExplicit` | `bool` | 2 | value |
| 39 | `_effectiveVisual` | `IVisual` | 8 | ref |
| 40–44 | Proxy/delegate fields | Various | 40 | ref |
| 45 | `_measureCache` | `Dictionary<Size, SizeRequest>` | 8 | ref |
| 46 | `_batched` | `int` | 4 | value |
| 47 | `_computedConstraint` | `LayoutConstraint` | 4 | value |
| 48–50 | Platform booleans | `bool` | 3 | value |
| 51–54 | `_mockHeight/Width/X/Y` | `double` | 32 | value |
| 55 | `_selfConstraint` | `LayoutConstraint` | 4 | value |
| 56 | `_resources` | `ResourceDictionary` | 8 | ref |
| 57–61 | State booleans | `bool` | 5 | value |
| 62 | `_frame` | `Rect` (4 × double) | 32 | value |
| 63 | `_semantics` | `Semantics?` | 8 | value |
| 64–74 | Events + disposable (11 fields) | Various | 88 | ref |

### View — 5 fields (40 B raw)

| # | Field | Type | Bytes | Kind |
|---|---|---|---:|---|
| 75 | `_gestureRecognizers` | `ObservableCollection<IGestureRecognizer>` | 8 | ref |
| 76 | `_recognizerForPointerOverState` | `PointerGestureRecognizer` | 8 | ref |
| 77 | `_compositeGestureRecognizers` | `ObservableCollection<IGestureRecognizer>` | 8 | ref |
| 78 | `_gestureManager` | `GestureManager` | 8 | ref |
| 79 | `propertyMapper` | `PropertyMapper` | 8 | ref |

### Label — 1 field (8 B raw)

| # | Field | Type | Bytes | Kind |
|---|---|---|---:|---|
| 80 | `_platformConfigurationRegistry` | `Lazy<PlatformConfigurationRegistry<Label>>` | 8 | ref |

</details>

<details>
<summary><strong>Subsidiary heap allocations — 29 objects, ~2,448 B (click to expand)</strong></summary>

These are objects allocated eagerly in constructors or field initializers. Each one is a separate GC heap object that contributes to the 3,112 B total.

Measured via `dotnet-gcdump` — object counts at exactly 10,000 (1× per Label) or 30,000 (3× per Label).

| # | Type | Obj size | Per Label | Bytes/Label | Source |
|---|---|---:|---:|---:|---|
| 1 | `Int32[]` | 96 | 3× | **288** | Bucket arrays for `_properties` + `_dynamicResources` + `_defaultStyleProperties` |
| 2 | `Entry<BindableProperty, BindablePropertyContext>[]` | 192 | 1× | **192** | `_properties` dictionary internal entries array |
| 3 | `EventHandler` | 64 | 3× | **192** | `PropertyChanged` + `PropertyChanging` + `BindingContextChanged` subscriber delegates |
| 4 | `Entry<BindableProperty, (string, SetterSpecificity)>[]` | 120 | 1× | **120** | `MergedStyle._defaultStyleProperties` internal entries array |
| 5 | `Object[]` | 104 | 1× | **104** | `SetterSpecificityList<object>.Values` backing array |
| 6 | `LazyHelper` | 32 | 3× | **96** | Internal state object for 3 `Lazy<T>` fields |
| 7 | `Dictionary<BindableProperty, BindablePropertyContext>` | 80 | 1× | **80** | `BindableObject._properties` header |
| 8 | `BindableProperty[]` | 80 | 1× | **80** | `SetterSpecificityList` keys backing array |
| 9 | `Dictionary<BindableProperty, (string, SetterSpecificity)>` | 80 | 1× | **80** | `MergedStyle._defaultStyleProperties` header |
| 10 | `MergedStyle` | 80 | 1× | **80** | `StyleableElement._mergedStyle` |
| 11 | `Dictionary<Size, SizeRequest>` | 80 | 1× | **80** | `VisualElement._measureCache` |
| 12 | `Entry<string>[]` | 72 | 1× | **72** | `_pendingHandlerUpdatesFromBPSet` (HashSet) internal entries array |
| 13 | `HashSet<string>` | 64 | 1× | **64** | `Element._pendingHandlerUpdatesFromBPSet` header |
| 14 | `GestureManager` | 64 | 1× | **64** | `View._gestureManager` |
| 15 | `NotifyCollectionChangedEventHandler` | 64 | 1× | **64** | `ObservableCollection.CollectionChanged` subscriber delegate |
| 16 | `Func<PlatformConfigurationRegistry<Label>>` | 64 | 1× | **64** | Factory delegate for `Lazy<PlatformConfigurationRegistry<Label>>` |
| 17 | `EventHandler<HandlerChangingEventArgs>` | 64 | 1× | **64** | `MergedStyle` subscribes to `Element.HandlerChanging` |
| 18 | `BindingPropertyChangedDelegate` | 64 | 1× | **64** | `NavigationProperty.PropertyChanged` callback |
| 19 | `BindablePropertyContext` | 56 | 1× | **56** | Context object for the `NavigationProperty` default value set in ctor |
| 20 | `ObservableCollection<IGestureRecognizer>` | 56 | 1× | **56** | `View._gestureRecognizers` |
| 21 | `SetterSpecificityList<object>` | 40 | 1× | **40** | `BindablePropertyContext.Values` |
| 22 | `SetterSpecificityList<BindingBase>` | 40 | 1× | **40** | `BindablePropertyContext.Bindings` |
| 23 | `NavigationProxy` | 40 | 1× | **40** | Created in `NavigableElement()` ctor |
| 24 | `Lazy<List<Page>>` | 40 | 1× | **40** | Lazy pages list on NavigableElement |
| 25 | `Lazy<PlatformConfigurationRegistry<Label>>` | 40 | 1× | **40** | `Label._platformConfigurationRegistry` |
| 26 | `Lazy<NavigatingStepRequestList>` | 40 | 1× | **40** | Lazy navigation step list on NavigableElement |
| 27 | `List<IGestureRecognizer>` | 32 | 1× | **32** | `ObservableCollection` internal backing list |
| 28 | `List<BindableProperty>` | 32 | 1× | **32** | Used internally during construction |
| 29 | `SetterSpecificity[]` | 24 | 1× | **24** | `SetterSpecificityList` specificities backing array |
| | **TOTAL** | | | **2,952** | |

Remaining ~160 bytes (3,112 − 2,952) are from small transient objects not captured in the heap snapshot.

</details>

### Allocation by category

| Category | Bytes/Label | % of total | Notes |
|---|---:|---:|---|
| Label object itself | 664 | 21.3% | 80 fields + object header + padding |
| Dictionary headers + backing arrays | 712 | 22.9% | 3 dictionaries + 1 HashSet |
| SetterSpecificityList + arrays | 184 | 5.9% | BPC.Values + BPC.Bindings + backing arrays |
| MergedStyle | 80 | 2.6% | Allocated in StyleableElement ctor |
| Delegate objects | 384 | 12.3% | Event handlers + Lazy Func delegates |
| Lazy\<T\> + LazyHelper | 216 | 6.9% | 3 Lazy fields + 3 LazyHelper objects |
| Gesture infrastructure | 152 | 4.9% | GestureManager + ObservableCollection + List backing |
| NavigationProxy | 40 | 1.3% | Created via SetValue in NavigableElement ctor |
| BindablePropertyContext (Navigation) | 136 | 4.4% | BPC + two SetterSpecificityLists + backing arrays |
| Int32\[\] bucket arrays | 288 | 9.3% | 3 bucket arrays for dictionaries/hashset |
| Other | 256 | 8.2% | Remaining small objects + unaccounted transients |
| **TOTAL** | **~3,112** | **100%** | |

### Key observations

1. **Dictionaries/HashSet allocated eagerly (712 B, 23%)** — `_properties` dictionary is needed immediately but initial capacity may be too large for controls that use few properties; `_measureCache` and `_pendingHandlerUpdatesFromBPSet` could be lazy.

2. **MergedStyle + backing dictionary (280 B, 9%)** — allocated in `StyleableElement` ctor for every element, even those that never use styles.

3. **Gesture infrastructure (152 B, 5%)** — `ObservableCollection` + `List` + `GestureManager` + delegate allocated in `View` field initializer even for controls that never use gestures.

4. **3× Lazy\<T\> fields (216 B, 7%)** — each `Lazy<T>` costs ~136 B (Lazy object + LazyHelper + Func delegate). These are "lazy" but still allocate 3 objects each upfront. Consider replacing with null-check patterns in single-threaded UI paths.

5. **Delegate objects (192 B, 6%)** — eagerly subscribed during construction. Consider deferring subscriptions where safe.

6. **NavigationProxy (40 B) + BindablePropertyContext (136 B)** — `NavigableElement` ctor creates a `NavigationProxy` + full `BindablePropertyContext` via `SetValue`. Most views never use direct navigation.

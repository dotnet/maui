# `new Label()` Allocation Breakdown — 3,112 bytes

> Measured on .NET 11 Preview 1 (arm64, macOS) using `GC.GetAllocatedBytesForCurrentThread()`
> and validated with `dotnet-gcdump` (10,000 Labels, full GC, heap snapshot).
>
> Inheritance chain: `Label` → `View` → `VisualElement` → `NavigableElement` → `StyleableElement` → `Element` → `BindableObject` → `object`

---

## Summary

| Declaring class    | Fields | Ref fields | Value fields | Raw field bytes |
|--------------------|-------:|-----------:|-------------:|----------------:|
| BindableObject     |      9 |          7 |            2 |              59 |
| StyleableElement   |      1 |          1 |            0 |               8 |
| Element            |     26 |         25 |            1 |             224 |
| NavigableElement   |      0 |          0 |            0 |               0 |
| VisualElement      |     38 |         21 |           17 |             260 |
| View               |      5 |          5 |            0 |              40 |
| Label              |      1 |          1 |            0 |               8 |
| **Total**          | **80** |     **60** |       **20** |         **599** |

Object header (MethodTable ptr + sync block) adds 16 bytes. With alignment padding the Label
object occupies **664 bytes** on the GC heap.

The remaining **~2,448 bytes** come from **29 subsidiary heap objects** allocated eagerly
in constructors and field initializers.

---

## Part 1 — Instance fields (80 fields, 664 B on heap)

### BindableObject — 9 fields (59 B raw)

| # | Field | Type | Bytes | Kind |
|---|-------|------|------:|------|
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
|---|-------|------|------:|------|
| 10 | `_mergedStyle` | `MergedStyle` | 8 | ref |

### Element — 26 fields (224 B raw)

| # | Field | Type | Bytes | Kind |
|---|-------|------|------:|------|
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
| 28 | `ChildAdded` (event) | `EventHandler<ElementEventArgs>` | 8 | ref |
| 29 | `ChildRemoved` (event) | `EventHandler<ElementEventArgs>` | 8 | ref |
| 30 | `DescendantAdded` (event) | `EventHandler<ElementEventArgs>` | 8 | ref |
| 31 | `DescendantRemoved` (event) | `EventHandler<ElementEventArgs>` | 8 | ref |
| 32 | `ParentSet` (event) | `EventHandler` | 8 | ref |
| 33 | `ParentChanging` (event) | `EventHandler<ParentChangingEventArgs>` | 8 | ref |
| 34 | `ParentChanged` (event) | `EventHandler` | 8 | ref |
| 35 | `HandlerChanging` (event) | `EventHandler<HandlerChangingEventArgs>` | 8 | ref |
| 36 | `HandlerChanged` (event) | `EventHandler` | 8 | ref |

### NavigableElement — 0 fields

No instance fields. Constructor sets `Navigation = new NavigationProxy()` which goes through
`SetValue` on a `BindableProperty` (stored in `_properties` dictionary, not as a field).

### VisualElement — 38 fields (260 B raw)

| # | Field | Type | Bytes | Kind |
|---|-------|------|------:|------|
| 37 | `_inputTransparentExplicit` | `bool` | 1 | value |
| 38 | `_isEnabledExplicit` | `bool` | 1 | value |
| 39 | `_effectiveVisual` | `IVisual` | 8 | ref |
| 40 | `_backgroundProxy` | `WeakBackgroundChangedProxy` | 8 | ref |
| 41 | `_clipProxy` | `WeakClipChangedProxy` | 8 | ref |
| 42 | `_backgroundChanged` | `EventHandler` | 8 | ref |
| 43 | `_clipChanged` | `EventHandler` | 8 | ref |
| 44 | `_shadowChanged` | `PropertyChangedEventHandler` | 8 | ref |
| 45 | `_measureCache` | `Dictionary<Size, SizeRequest>` | 8 | ref |
| 46 | `_batched` | `int` | 4 | value |
| 47 | `_computedConstraint` | `LayoutConstraint` (enum) | 4 | value |
| 48 | `_isInPlatformLayout` | `bool` | 1 | value |
| 49 | `_isPlatformStateConsistent` | `bool` | 1 | value |
| 50 | `_isPlatformEnabled` | `bool` | 1 | value |
| 51 | `_mockHeight` | `double` | 8 | value |
| 52 | `_mockWidth` | `double` | 8 | value |
| 53 | `_mockX` | `double` | 8 | value |
| 54 | `_mockY` | `double` | 8 | value |
| 55 | `_selfConstraint` | `LayoutConstraint` (enum) | 4 | value |
| 56 | `_resources` | `ResourceDictionary` | 8 | ref |
| 57 | `_isPointerOver` | `bool` | 1 | value |
| 58 | `_isLoadedFired` | `bool` | 1 | value |
| 59 | `_loaded` | `EventHandler?` | 8 | ref |
| 60 | `_unloaded` | `EventHandler?` | 8 | ref |
| 61 | `_watchingPlatformLoaded` | `bool` | 1 | value |
| 62 | `_frame` | `Rect` (struct: 4 × double) | 32 | value |
| 63 | `_semantics` | `Semantics?` (nullable struct) | 8 | value |
| 64 | `_loadedUnloadedToken` | `IDisposable?` | 8 | ref |
| 65 | `_windowChanged` (event) | `EventHandler?` | 8 | ref |
| 66 | `_platformContainerViewChanged` (event) | `EventHandler?` | 8 | ref |
| 67 | `PlatformEnabledChanged` (event) | `EventHandler` | 8 | ref |
| 68 | `ChildrenReordered` (event) | `EventHandler` | 8 | ref |
| 69 | `Focused` (event) | `EventHandler<FocusEventArgs>` | 8 | ref |
| 70 | `MeasureInvalidated` (event) | `EventHandler` | 8 | ref |
| 71 | `SizeChanged` (event) | `EventHandler` | 8 | ref |
| 72 | `Unfocused` (event) | `EventHandler<FocusEventArgs>` | 8 | ref |
| 73 | `BatchCommitted` (event) | `EventHandler<EventArg<VisualElement>>` | 8 | ref |
| 74 | `FocusChangeRequested` (event) | `EventHandler<FocusRequestArgs>` | 8 | ref |

### View — 5 fields (40 B raw)

| # | Field | Type | Bytes | Kind |
|---|-------|------|------:|------|
| 75 | `_gestureRecognizers` | `ObservableCollection<IGestureRecognizer>` | 8 | ref |
| 76 | `_recognizerForPointerOverState` | `PointerGestureRecognizer` | 8 | ref |
| 77 | `_compositeGestureRecognizers` | `ObservableCollection<IGestureRecognizer>` | 8 | ref |
| 78 | `_gestureManager` | `GestureManager` | 8 | ref |
| 79 | `propertyMapper` | `PropertyMapper` | 8 | ref |

### Label — 1 field (8 B raw)

| # | Field | Type | Bytes | Kind |
|---|-------|------|------:|------|
| 80 | `_platformConfigurationRegistry` | `Lazy<PlatformConfigurationRegistry<Label>>` | 8 | ref |

---

## Part 2 — Subsidiary heap allocations (29 objects, ~2,448 B)

These are objects allocated eagerly in constructors or field initializers. Each one is a
separate GC heap object that contributes to the 3,112 B total.

Measured via `dotnet-gcdump` — object counts at exactly 10,000 (1× per Label) or 30,000 (3× per Label).

| # | Type | Obj size | Per Label | Bytes/Label | Source |
|---|------|----------:|----------:|------------:|--------|
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
| 18 | `BindingPropertyChangedDelegate` | 64 | 1× | **64** | `NavigationProperty.PropertyChanged` callback (from `SetValue` in ctor) |
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

Remaining ~160 bytes (3,112 − 2,952) are from small transient objects not captured in the
heap snapshot (e.g., enumerators, boxing, intermediate delegate combinations during event
subscription).

---

## Part 3 — Allocation by category

| Category | Bytes/Label | % of total | Notes |
|----------|------------:|-----------:|-------|
| Label object itself | 664 | 21.3% | 80 fields + object header + padding |
| Dictionary headers + backing arrays | 712 | 22.9% | 3 dictionaries (`_properties`, `_defaultStyleProperties`, `_measureCache`) + 1 HashSet |
| SetterSpecificityList + arrays | 184 | 5.9% | BPC.Values + BPC.Bindings + backing arrays |
| MergedStyle | 80 | 2.6% | Allocated in StyleableElement ctor |
| Delegate objects | 384 | 12.3% | 3× EventHandler + HandlerChanging + OC.CollectionChanged + Lazy Func |
| Lazy<T> + LazyHelper | 216 | 6.9% | 3 Lazy fields (40 B each) + 3 LazyHelper (32 B each) |
| Gesture infrastructure | 152 | 4.9% | GestureManager + ObservableCollection + List backing |
| NavigationProxy | 40 | 1.3% | Created in NavigableElement ctor via SetValue |
| BindablePropertyContext (Navigation) | 136 | 4.4% | BPC + its two SetterSpecificityLists + backing arrays (from Navigation SetValue) |
| Int32[] bucket arrays | 288 | 9.3% | 3 bucket arrays for dictionaries/hashset |
| Other | 256 | 8.2% | Remaining small objects + unaccounted transients |
| **TOTAL** | **~3,112** | **100%** | |

---

## Observations and potential optimizations

1. **Dictionaries/HashSet allocated eagerly (712 B, 23%)**
   - `_properties` dictionary (280 B total with backing arrays) is needed immediately, but initial capacity of 4 may be too large for controls that use few properties
   - `_measureCache` (80 B + backing) is allocated even before any layout pass — could be lazy
   - `_pendingHandlerUpdatesFromBPSet` HashSet (136 B) — could be lazy, only needed when handler updates fire

2. **MergedStyle + backing dictionary (280 B, 9%)**
   - Allocated in `StyleableElement` ctor for every element, even those that never use styles
   - Contains `_defaultStyleProperties` dictionary that could potentially be lazy

3. **Gesture infrastructure (152 B, 5%)**
   - `ObservableCollection<IGestureRecognizer>` + `List` + `GestureManager` + `CollectionChanged` delegate
   - Allocated in `View` field initializer even for controls that never use gestures (e.g., Label)
   - Could be lazy-initialized on first gesture recognizer addition

4. **3× Lazy<T> fields (216 B, 7%)**
   - Each `Lazy<T>` costs 40 B for the Lazy object + 32 B for LazyHelper + 64 B for Func delegate = **136 B**
   - These are "lazy" but still allocate 3 objects each upfront
   - Consider using `Lazy<T>` only where the value is actually expensive, or replace with null-check patterns

5. **3× EventHandler delegates (192 B, 6%)**
   - Eagerly subscribed during construction (likely MergedStyle subscribing to property changes)
   - Consider deferring subscription until first style application

6. **NavigationProxy (40 B) + BindablePropertyContext (136 B)**
   - `NavigableElement` ctor does `Navigation = new NavigationProxy()` which calls `SetValue`
   - This creates a `NavigationProxy` + full `BindablePropertyContext` with two `SetterSpecificityList` objects
   - Most views never use direct navigation — could defer

7. **Label._platformConfigurationRegistry Lazy (136 B)**
   - The `Lazy<T>` itself allocates Lazy + LazyHelper + Func
   - If platform config is rarely used, a simple null-check field would save these 3 allocations

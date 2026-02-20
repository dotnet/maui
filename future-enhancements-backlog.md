# Future Enhancement Opportunities — Control-Elements Memory Optimization

## Executive Summary

This document identifies **prioritized future enhancements** that build on the `label-investigation` branch's control-elements memory optimization work. The baseline PoC achieved **22.95% reduction** in `new Label()` allocations (2928 B → 2256 B) by optimizing 14 data structures (DS01-DS34).

**Key finding:** 5 high-value deferred items (DS08, DS12-DS15) require **structural redesigns** rather than simple lazy-init patches. These represent the **next tier of gains** beyond the low-hanging fruit already harvested.

---

## Part 1: Prioritized Enhancement Backlog

### HIGH PRIORITY (Expected Impact: 5-15% additional allocation reduction)

#### Enhancement #1: SetterSpecificityList inline-first-slot optimization (DS12-DS14 bundle)
**Target area:** `SetterSpecificityList<T>` storage strategy  
**Files:** `src/Controls/src/Core/SetterSpecificityList.cs`, `src/Controls/src/Core/BindableObject.cs`

**Current state:**
- `BindablePropertyContext.Values` (SetterSpecificityList<object>) preallocates 3-capacity arrays for values/keys/specificities
- Each `BindablePropertyContext` created pays **~168 B** for three backing arrays even if only 1-2 values ever exist
- DS09 attempted simple capacity reduction (3→1) but caused **broad regressions** due to immediate growth/copy on second value

**Deferred reason (from poc-evaluation-log.md):**
> DS09: Lowering initial capacity forced extra growth/copy behavior and caused broad allocation regressions.
> DS12-DS14: Requires a larger redesign (single-entry inline storage or custom small-buffer list) rather than simple capacity tweaks.

**Proposed enhancement:**
Replace array-based storage with **inline-first-slot + deferred-arrays** pattern:
```csharp
// Current: always 3 arrays
object[] _values;
BindableProperty[] _keys;
SetterSpecificity[] _specificities;

// Proposed: inline first entry, promote to arrays on second value
object _firstValue;
BindableProperty _firstKey;
SetterSpecificity _firstSpecificity;
int _count;

// Only allocate when count > 1
object[]? _additionalValues;
BindableProperty[]? _additionalKeys;
SetterSpecificity[]? _additionalSpecificities;
```

**Expected impact:**
- **Micro (B1):** -120 to -168 B per Label (save 3 arrays for NavigationProperty BPC + typical property contexts)
- **Styled (B4):** -500 to -800 B (multiple properties with single-value contexts)
- **Startup (B6):** -0.15 to -0.25 MB per 1000 controls

**Risk:** Medium-High
- Requires careful indexing logic in hot paths (GetValue/SetValue/RemoveValue)
- Must preserve setter specificity ordering semantics
- Need to validate no perf regression in multi-value property scenarios

**Validation approach:**
1. Run full B1-B6 suite to ensure no allocation regressions
2. Add targeted micro-benchmark: single-value vs multi-value property scenarios
3. Run Controls.Core.UnitTests (5535 tests) + Xaml.UnitTests (1867 tests)
4. Measure `GetValue`/`SetValue` throughput impact (expect neutral to small win)

**Prerequisites:** None (can be done independently)

**Real-world XAML SG impact:** Medium
- Most XAML properties use 1-2 setter specificity entries (default + style/local)
- Large XAML pages with many styled controls should see 3-5% allocation reduction in SG benchmark

---

#### Enhancement #2: MergedStyle lazy _implicitStyles tracking (DS15)
**Target area:** `MergedStyle` implicit style registration  
**Files:** `src/Controls/src/Core/MergedStyle.cs`, `src/Controls/src/Core/StyleableElement/StyleableElement.cs`

**Current state:**
- `MergedStyle._implicitStyles` (`List<BindableProperty>`) is allocated in constructor **even if no implicit styles ever apply**
- Adds **~32 B per element** baseline cost

**Deferred reason:**
> DS15: No change applied; implicit style registration is currently required at MergedStyle construction to preserve dynamic resource behavior and implicit-style updates. Requires redesign of implicit-style propagation hooks before deferring _implicitStyles allocation safely.

**Proposed enhancement:**
Introduce **on-demand implicit-style tracking** by:
1. Replace eager `_implicitStyles = new List<BindableProperty>()` with nullable field
2. Add `EnsureImplicitStylesTracking()` helper that initializes on first `RegisterImplicitStyle()` call
3. Guard all read sites with null checks (treat null as "no implicit styles registered")
4. Validate that dynamic resource updates and style inheritance still trigger correctly

**Expected impact:**
- **Micro (B1):** -32 B (MergedStyle list not allocated)
- **Styled (B4):** 0 B (implicit styles are used, list still allocated)
- **Startup (B6):** -16 to -32 KB per 1000 controls (depends on implicit style usage patterns)

**Risk:** Medium
- Requires careful audit of implicit style propagation lifecycle
- Must ensure dynamic resource updates still reach implicitly-styled properties
- Need to validate `OnResourcesChanged` and style inheritance paths

**Validation approach:**
1. Run B1-B6 baseline (verify win in B1, neutral in B4)
2. Add unit tests for implicit style registration with/without dynamic resources
3. Manual test: change `Application.Resources` implicit style, verify UI updates
4. Run `src/Controls/tests/Core.UnitTests` style-related tests

**Prerequisites:** None

**Real-world XAML SG impact:** Low-Medium
- Most XAML controls use implicit styles, so this primarily helps B1/headless scenarios
- Marginal impact on full app startup, but reduces baseline object size

---

#### Enhancement #3: NavigableElement lazy Navigation property (DS29 alternative)
**Target area:** `NavigableElement.Navigation` eager construction  
**Files:** `src/Controls/src/Core/NavigableElement/NavigableElement.cs`

**Current state:**
- `NavigableElement()` constructor does `Navigation = new NavigationProxy()` which triggers `SetValue`
- Creates **NavigationProxy (40 B) + BindablePropertyContext (56 B) + two SetterSpecificityLists (80 B) + backing arrays (224 B)** = **~400 B**
- **Most views never use navigation** (e.g., Label, Image, BoxView in non-navigation scenarios)

**Deferred reason:**
DS29 tried to cache delegates for MergedStyle (different approach), which regressed. Navigation lazy-init was not attempted in PoC.

**Proposed enhancement:**
Make `NavigationProperty` default value creation lazy:
1. Change `NavigationProperty` default value from eager `new NavigationProxy()` to `null`
2. Update `Navigation` property getter to use `??=` pattern: `get => (INavigation)(GetValue(NavigationProperty) ?? SetValue(NavigationProperty, new NavigationProxy()))`
3. Guard all internal navigation access with null checks or ensure lazy creation on first use

**Expected impact:**
- **Micro (B1):** -400 B (NavigationProxy + BPC + backing structures not allocated)
- **B3 (NavigationTouch):** 0 B (navigation is used, still allocated)
- **Startup (B6):** -200 to -400 KB per 1000 controls (massive impact for non-navigation-heavy trees)

**Risk:** Medium-High
- Requires API behavior validation: existing code may assume `Navigation` is always non-null
- Must audit all `Navigation.` call sites for null safety
- Breaking change if external code relies on `Navigation` never returning null

**Validation approach:**
1. Grep for all `Navigation.` usages, ensure null-safe or lazy-create
2. Run B1-B6 (expect large B1 win, neutral B3/B4)
3. Run full device tests for navigation-heavy scenarios (Shell, Modal, Push/Pop)
4. Check if any tests assert `Navigation != null` immediately after construction

**Prerequisites:** API-level review (breaking change assessment)

**Real-world XAML SG impact:** High
- Large XAML pages with many leaf controls (Label, Image, Border) that never navigate will see 5-8% allocation reduction
- Shell-based apps may still pay the cost, but most controls in content pages don't need navigation

---

### MEDIUM PRIORITY (Expected Impact: 2-5% additional reduction)

#### Enhancement #4: BindableObject._properties initial capacity tuning (DS01 refinement)
**Target area:** `BindableObject._properties` dictionary sizing  
**Files:** `src/Controls/src/Core/BindableObject.cs`

**Current state (after DS01):**
- DS01 changed `_properties` from `new Dictionary<...>(4)` to default `new Dictionary<...>()`
- Default capacity is **0**, which triggers immediate growth to **3** on first property set
- Measurement showed **mixed results**: B1 improved (-112 B), but B2/startup **regressed** (+136-240 B)

**DS01 verdict:** Kept provisionally, but flagged for revisit

**Proposed enhancement:**
Implement **adaptive initial capacity** based on control type:
1. Use **capacity 2** for base `BindableObject._properties` (middle ground between 0 and 4)
2. Measure impact across B1-B6 to find optimal sweet spot
3. Alternatively, introduce per-type capacity hints via internal constructor overload

**Expected impact:**
- **Micro (B1):** -40 to -80 B (reduce initial bucket array size)
- **B2:** 0 B (minimal growth on property sets)
- **Startup (B6):** -20 to -40 KB per 1000 controls

**Risk:** Low-Medium
- Capacity=2 may still cause one growth in property-heavy scenarios
- Need to measure against representative real-world property-set patterns

**Validation approach:**
1. Try capacity values: 0 (current), 1, 2, 3 (original DS01 baseline)
2. Run B1-B6 for each capacity, compare allocation + throughput
3. Profile real XAML page: count properties set per control, validate capacity choice

**Prerequisites:** None

**Real-world XAML SG impact:** Low
- Micro-optimization, but compounds across many controls
- Expect <2% allocation delta in SG benchmark

---

#### Enhancement #5: Custom small-dictionary for BindableProperty contexts (DS08 alternative)
**Target area:** Dictionary entry-array growth  
**Files:** `src/Controls/src/Core/BindableObject.cs`, new `SmallDictionary<TKey,TValue>` helper

**Current state:**
- `_properties` dictionary uses BCL `Dictionary<TKey,TValue>` with standard growth pattern
- Entry arrays grow in powers of 2, causing waste for controls with 1-3 properties

**Deferred reason (DS08):**
> No safe MAUI-layer change identified; dictionary entry-array growth behavior is primarily runtime-controlled unless we replace dictionaries with custom storage.

**Proposed enhancement:**
Introduce **SmallDictionary<TKey,TValue>** with inline-first-entry optimization (similar to DS12-DS14 pattern):
```csharp
// Inline first entry, promote to BCL Dictionary on second entry
internal class SmallDictionary<TKey, TValue>
{
    TKey _firstKey;
    TValue _firstValue;
    Dictionary<TKey, TValue>? _overflow;
    
    // Add/Get/Remove methods with fast-path for single entry
}
```

Replace `_properties` with `SmallDictionary<BindableProperty, BindablePropertyContext>`.

**Expected impact:**
- **Micro (B1):** -150 to -200 B (avoid dictionary header + bucket array for 1-property case)
- **B2:** -50 to -100 B (defer dictionary promotion)
- **Startup (B6):** -75 to -150 KB per 1000 controls

**Risk:** High
- Requires new data structure implementation + extensive validation
- Must match `Dictionary<TKey,TValue>` semantics (enumeration order, exception behavior)
- Potential throughput regression if inline fast-path is not well-optimized

**Validation approach:**
1. Implement `SmallDictionary<TKey,TValue>` with unit tests
2. Benchmark single-entry vs multi-entry throughput (ensure no regression)
3. Run B1-B6 suite to validate allocation wins
4. Profile GetValue/SetValue hot paths with BenchmarkDotNet

**Prerequisites:** DS12-DS14 (similar inline-slot pattern, can share design learnings)

**Real-world XAML SG impact:** Medium
- Most controls have 3-8 properties set, so inline-first optimization helps moderately
- Larger impact in minimalist XAML pages with sparse property usage

---

#### Enhancement #6: Weak reference consolidation (DS30-DS31 refinement)
**Target area:** `Element._realParent`, `_parentOverride`, `_inheritedContext`  
**Files:** `src/Controls/src/Core/Element/Element.cs`, `src/Controls/src/Core/BindableObject.cs`

**Current state:**
- Three weak references in base chain: `_realParent`, `_parentOverride` (Element), `_inheritedContext` (BindableObject)
- Each `WeakReference<T>` costs **~32-40 B**
- Total: **~96-120 B per element**

**Deferred reason (DS30-DS31):**
> DS30: Reverted after no measurable allocation gain in benchmark matrix.
> DS31: Reverted after no allocation change vs pre (B1/B2/B3/B4/B5/B6 allocations unchanged).

**Proposed enhancement:**
Audit weak reference necessity and explore:
1. **Consolidate** multiple weak refs into a single `WeakParentingState` struct/class with multiple `WeakReference<Element>` fields (reduce object count)
2. **Defer** `_parentOverride` allocation until explicit parent override occurs (rare scenario)
3. **Evaluate** if `_realParent` weak semantics are required in all scenarios (vs direct reference with explicit lifecycle management)

**Expected impact:**
- **Micro (B1):** -40 to -80 B (defer one weak ref, consolidate overhead)
- **Startup (B6):** -40 to -80 KB per 1000 controls

**Risk:** High
- Weak reference semantics are tied to GC behavior and parent lifecycle
- Changing to direct references may introduce memory leaks if not carefully managed
- Requires deep understanding of element parent-tracking contracts

**Validation approach:**
1. Audit all weak reference usage: who sets, who reads, when is null checked
2. Add stress test: create/destroy large element trees, verify no leaks with weak refs changed
3. Run B1-B6 + manual parent-cycle tests (Shell, Navigation, Modal)
4. Use memory profiler to verify no new root retention

**Prerequisites:** Detailed lifecycle audit + GC behavior validation

**Real-world XAML SG impact:** Low-Medium
- Weak refs are always allocated, so impact is uniform across all controls
- ~2-3% reduction if successful

---

### LOW PRIORITY (Expected Impact: <2% reduction, but improved maintainability)

#### Enhancement #7: Bit-packing expansion (DS32 extension)
**Target area:** Additional boolean fields in VisualElement/Element  
**Files:** `src/Controls/src/Core/VisualElement/VisualElement.cs`, `src/Controls/src/Core/Element/Element.cs`

**Current state (after DS32):**
- DS32 packed multiple `VisualElement` bools into a single `ElementFlags` enum
- Saved **8 B per element**
- Additional bool fields remain unpacked in `Element` and other base classes

**Proposed enhancement:**
Extend bit-packing to:
- Element: `_applying` (BindableObject), layout-related flags
- NavigableElement: navigation state flags
- StyleableElement: style-related flags

**Expected impact:**
- **Object size reduction:** -8 to -16 B per element (reduce padding waste)
- **Startup (B6):** -8 to -16 KB per 1000 controls

**Risk:** Low
- Well-understood pattern from DS32
- Straightforward mechanical refactoring

**Validation approach:**
1. Identify remaining bool fields in base chain
2. Apply DS32 pattern to each class
3. Run B1-B6 to measure impact
4. Verify no boxing issues or flag-access perf regression

**Prerequisites:** None

**Real-world XAML SG impact:** Low (~1% or less)

---

#### Enhancement #8: Mock bounds deferred state (DS34 extension)
**Target area:** Additional struct-heavy fields in VisualElement  
**Files:** `src/Controls/src/Core/VisualElement/VisualElement.cs`

**Current state (after DS34):**
- DS34 replaced `_mockX/_mockY/_mockWidth/_mockHeight` with deferred `MockBoundsState` reference
- Saved **24 B per element**

**Proposed enhancement:**
Apply similar deferred-state pattern to:
- `_frame` (Rect, 32 B): defer until first layout pass or when accessed
- `_semantics` (Semantics?, 8 B nullable struct): defer until accessibility is needed

**Expected impact:**
- **Micro (B1):** -32 to -40 B (defer Rect + semantics)
- **Startup (B6):** -32 to -40 KB per 1000 controls

**Risk:** Medium
- `_frame` is accessed in layout hot paths, must remain fast
- Semantics are platform-specific, need to validate no a11y regressions

**Validation approach:**
1. Measure `_frame` access frequency in layout benchmarks (ensure defer doesn't hurt perf)
2. Test semantics on iOS/Android with VoiceOver/TalkBack enabled
3. Run B1-B6 + layout-specific benchmarks (e.g., B5 MockVisualTreeStartup)

**Prerequisites:** Layout hot-path profiling

**Real-world XAML SG impact:** Low (~1-2%)

---

#### Enhancement #9: Static delegate caching for common handlers (DS29 revisited)
**Target area:** Event handler allocations  
**Files:** `src/Controls/src/Core/MergedStyle.cs`, `src/Controls/src/Core/Element/Element.cs`

**Current state:**
- DS29 attempted to cache MergedStyle delegates, but **regressed** (+80 B per control)
- Reverted and flagged as complex

**Deferred reason (DS29):**
> Attempt regressed allocations: B1 2296 B -> 2376 B, B2 3144 B -> 3224 B, B3 +80 B, B4 14699 B -> 14731 B; Revert restored DS25-level allocations.

**Proposed enhancement (alternative approach):**
Instead of instance-field caching, use **static delegate fields** for common handlers:
```csharp
// Static shared delegates
private static readonly EventHandler<HandlerChangingEventArgs> s_handlerChangingDelegate = OnHandlerChanging;

// Constructor subscribes to static delegate
_element.HandlerChanging += s_handlerChangingDelegate;
```

This avoids per-instance delegate allocation while maintaining correct behavior.

**Expected impact:**
- **Micro (B1):** -64 to -128 B (shared static delegates, no per-instance alloc)
- **Styled (B4):** -64 B
- **Startup (B6):** -64 to -128 KB per 1000 controls

**Risk:** Medium-High
- Static delegates lose `this` context, must use sender parameter explicitly
- May break if delegate logic relies on captured instance state
- Requires careful audit of all delegate subscription sites

**Validation approach:**
1. Identify safe static-delegate candidates (handlers that only use sender/args)
2. Convert one at a time, validate behavior with unit tests
3. Run B1-B6 to measure cumulative wins
4. Manual test: verify events fire correctly and state updates propagate

**Prerequisites:** Delegate usage audit

**Real-world XAML SG impact:** Low-Medium (~1-2%)

---

## Part 2: Real-World / XAML SG Impact Opportunities

### Observation: SG benchmark dilution
The PoC achieved **22.95% B1 reduction** but only **2-4% allocation reduction in XAML SG benchmark**. Why?

From `pr-draft.md`:
> 1. **Workload scope mismatch:** B1/B2 isolate base control construction; SG inflates a large XAML page with extensive resource/style work, where control-instance savings are only one part of total cost.
> 2. **Signal dilution:** many DS wins are hundreds of bytes per control, while SG operations allocate ~161 KB (SourceGen) to ~24 MB (Runtime) per op; absolute wins become small percentages.

**Key insight:** To improve SG impact without changing scope away from control-elements work, target areas where **many controls compound the savings**.

### Specific XAML SG Impact Opportunities

#### Opportunity #1: Prioritize enhancements that affect ALL controls
**Recommendation:** Focus on Enhancement #1 (SetterSpecificityList inline-first) and Enhancement #3 (lazy Navigation).

**Why:** These affect every control in the tree, so wins multiply in large XAML pages.

**Expected SG impact:** +3-5% additional allocation reduction (on top of existing 2-4%)

---

#### Opportunity #2: Measure SG benchmark with feature-toggle profiles
**Current gap:** The existing SG benchmark (`Benchmark.xaml`) is a single fixed workload.

**Recommendation:**
1. Create **SG benchmark variants** with different feature usage patterns:
   - **Minimal:** No gestures, no dynamic resources, no implicit styles
   - **Styled:** Heavy implicit/explicit style usage
   - **Interactive:** Many gesture recognizers and effects
   - **Mixed:** Realistic blend

2. Run PoC against each profile to understand **where the wins go**.

**Expected outcome:** Better visibility into which DS optimizations help which real-world scenarios, guiding future prioritization.

---

#### Opportunity #3: Extend optimizations to CollectionView/ListView item templates
**Current gap:** Base-chain optimizations help page-level controls, but **item templates are instantiated many times**.

**Recommendation:**
1. Profile item template instantiation in CollectionView/ListView (100-1000 items)
2. Identify if base-chain optimizations help or if item-template-specific work dominates
3. If base-chain matters: DS enhancements will compound heavily in scrolling scenarios
4. If template-specific: file separate issue for template inflation optimization

**Expected impact (if base-chain matters):** 5-10% reduction in large-list scenarios

---

#### Opportunity #4: Validate DS wins in Shell-based navigation scenarios
**Current gap:** SG benchmark is page-centric; Shell apps create many transient pages/controls during navigation.

**Recommendation:**
1. Add **Shell navigation benchmark**: push 10 pages, measure total allocations
2. Validate that DS wins (especially lazy Navigation, lazy GestureRecognizers) help in Shell scenarios
3. If Shell-specific overhead dominates, file separate issue for Shell navigation optimization

**Expected impact:** Validate that PoC work benefits Shell apps, or identify Shell-specific optimization track

---

## Part 3: Prerequisites and Dependencies

### Dependency Graph

```
Enhancement #1 (SetterSpecificityList inline-first)
  ├─ No prerequisites
  └─ Enables: Enhancement #5 (SmallDictionary, similar pattern)

Enhancement #2 (MergedStyle lazy _implicitStyles)
  └─ No prerequisites

Enhancement #3 (NavigableElement lazy Navigation)
  ├─ Requires: API-level review (breaking change assessment)
  └─ Enables: Broader lazy-property pattern for other rarely-used BP defaults

Enhancement #4 (BindableObject._properties capacity tuning)
  └─ No prerequisites (can be done anytime)

Enhancement #5 (SmallDictionary)
  ├─ Prerequisite: Enhancement #1 (learn inline-first pattern)
  └─ High complexity, consider after Enhancement #1 succeeds

Enhancement #6 (Weak reference consolidation)
  ├─ Prerequisite: Detailed lifecycle audit + GC stress testing
  └─ High risk, defer until after medium-priority items

Enhancement #7 (Bit-packing expansion)
  └─ No prerequisites (DS32 already validated the pattern)

Enhancement #8 (Mock bounds deferred state extension)
  ├─ Prerequisite: Layout hot-path profiling
  └─ Low risk, can be done independently

Enhancement #9 (Static delegate caching)
  ├─ Prerequisite: Delegate usage audit
  └─ Medium risk, consider after Enhancement #1-#3
```

### Recommended Implementation Order

**Phase 1 (High ROI, lower risk):**
1. Enhancement #1 (SetterSpecificityList inline-first) — biggest remaining structural win
2. Enhancement #3 (NavigableElement lazy Navigation) — massive impact if API review passes
3. Enhancement #2 (MergedStyle lazy _implicitStyles) — clean incremental win

**Phase 2 (Medium ROI, validate patterns):**
4. Enhancement #4 (Dictionary capacity tuning) — quick experiment, low risk
5. Enhancement #7 (Bit-packing expansion) — mechanical follow-up to DS32
6. Enhancement #9 (Static delegate caching) — if delegate audit is clean

**Phase 3 (Complex redesigns):**
7. Enhancement #5 (SmallDictionary) — after Enhancement #1 pattern is proven
8. Enhancement #8 (Deferred state extension) — after layout profiling
9. Enhancement #6 (Weak ref consolidation) — highest risk, requires deep audit

---

## Part 4: Validation and Testing Strategy

### Benchmark Coverage Requirements

For each enhancement, validate with:
1. **B1-B6 baseline suite** (existing benchmarks, must show no regressions)
2. **Targeted micro-benchmark** for the specific data structure (e.g., single-value vs multi-value property sets)
3. **XAML SG benchmark** (existing `LayoutBenchmark`, expect small incremental wins)
4. **Optional: Shell navigation benchmark** (if Enhancement #3 is implemented)

### Unit Test Requirements

1. **Existing test suites must pass:**
   - `src/Controls/tests/Core.UnitTests/Controls.Core.UnitTests.csproj` (5535 tests)
   - `src/Controls/tests/Xaml.UnitTests/Controls.Xaml.UnitTests.csproj` (1867 tests)

2. **New tests per enhancement:**
   - **Enhancement #1:** Unit tests for SetterSpecificityList with 0, 1, 2, 3+ entries
   - **Enhancement #2:** Tests for MergedStyle with/without implicit styles + dynamic resources
   - **Enhancement #3:** Tests for Navigation property lazy creation + null handling
   - **Enhancement #5:** Comprehensive SmallDictionary unit tests (if implemented)

### Integration Test Requirements

1. **Device tests:** Run on iOS, Android, Windows, Mac to ensure no platform-specific regressions
2. **GC stress test:** Create/destroy large element trees (10k+ controls), verify no memory leaks
3. **Shell navigation:** Push/pop many pages, verify lazy optimizations don't break navigation lifecycle
4. **XAML inflation:** Large XAML pages with varied feature usage (gestures, styles, resources, bindings)

### Performance Regression Gates

Before merging any enhancement:
1. **Allocation:** No increase in B1-B6 `Allocated/op` (within statistical noise)
2. **Throughput:** No >5% regression in `Mean` time for B1-B6
3. **GC pressure:** No increase in Gen0/Gen1 collection frequency
4. **SG benchmark:** No allocation or throughput regression (neutral or positive only)

---

## Part 5: Risk Matrix Summary

| Enhancement | Expected Alloc Win | Complexity | Risk Level | Validation Effort | Recommended Phase |
|---|---|---|---|---|---|
| #1 SetterSpecificityList inline-first | 5-10% | High | Medium-High | High | Phase 1 |
| #2 MergedStyle lazy _implicitStyles | 1-2% | Low-Medium | Medium | Medium | Phase 1 |
| #3 NavigableElement lazy Navigation | 10-15% | Medium | Medium-High | High | Phase 1 |
| #4 Dictionary capacity tuning | 1-2% | Low | Low-Medium | Low | Phase 2 |
| #5 SmallDictionary | 3-5% | Very High | High | Very High | Phase 3 |
| #6 Weak ref consolidation | 2-3% | High | High | Very High | Phase 3 |
| #7 Bit-packing expansion | <1% | Low | Low | Low | Phase 2 |
| #8 Deferred state extension | 1-2% | Medium | Medium | Medium | Phase 3 |
| #9 Static delegate caching | 1-2% | Medium | Medium-High | Medium | Phase 2 |

---

## Part 6: Stretch Goals (Research Items)

These are **exploratory ideas** beyond the scope of incremental enhancements, requiring significant design work:

### Research #1: Value-type BindablePropertyContext
Replace reference-type `BindablePropertyContext` with struct to avoid heap allocation. Requires deep redesign of `_properties` dictionary value semantics.

### Research #2: Shared/pooled SetterSpecificityList backing arrays
For common array sizes (e.g., 3-capacity), maintain a pool of reusable arrays to reduce allocation churn. Requires lifetime management.

### Research #3: Control-specific optimization subclasses
Generate optimized `LabelFast`, `ButtonFast` with hardcoded property slots instead of dictionary-based storage for the top 5-10 most common properties. API surface challenge.

### Research #4: Custom GC handle for parent tracking
Replace `WeakReference<Element>` with custom GC handle wrapper that is cheaper than BCL weak refs. Platform-specific complexity.

---

## Appendix: Baseline Metrics Reference

### PoC Final Results (from pr-draft.md)
- `new Label()` (B1): **2928 B → 2256 B** (-672 B, -22.95%)
- `new Label() + basic properties` (B2): **3648 B → 3104 B** (-544 B, -14.91%)
- ResourceDictionary style (B4): **16571 B → 14587 B** (-1984 B, -11.97%)
- Startup emulation 1000 controls (B6): **7.79 MB → 6.91 MB** (-0.88 MB, -11.30%)

### XAML SG Benchmark (from pr-draft.md)
- SourceGen Alloc: **167.64 KB → 161.37 KB** (-6.27 KB, -3.74%)
- XamlC Alloc: **345.08 KB → 338.17 KB** (-6.91 KB, -2.00%)

### Deferred Items from PoC (DS08, DS12-DS15)
- **DS08:** Entry array growth (custom dictionary needed)
- **DS12-DS14:** SetterSpecificityList backing arrays (inline-first pattern)
- **DS15:** MergedStyle._implicitStyles lazy tracking

These 5 deferred items are the **primary targets** for future enhancement work.

---

## Conclusion

The `label-investigation` PoC successfully harvested low-hanging fruit (14 data structures optimized). The **next tier of gains** requires **structural redesigns** (inline-first storage, lazy Navigation, implicit-style tracking) that were correctly deferred from the PoC due to complexity.

**Top 3 recommendations for maximum impact:**
1. **Enhancement #1** (SetterSpecificityList inline-first): 5-10% additional reduction, validates pattern for Enhancement #5
2. **Enhancement #3** (lazy Navigation): 10-15% reduction if API review passes, huge win for non-navigation controls
3. **Enhancement #2** (lazy _implicitStyles): 1-2% reduction, clean incremental win with manageable risk

**Estimated combined impact:** +15-25% additional allocation reduction on top of PoC baseline, for a **cumulative ~40-45% reduction** in `new Label()` allocations vs pre-PoC.

**XAML SG impact projection:** +5-8% allocation reduction (on top of existing 2-4%), bringing total SG benefit to **~7-12%** for typical XAML pages.

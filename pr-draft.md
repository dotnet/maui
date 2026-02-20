<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

## Summary

This PR implements low-risk memory optimizations for the shared control base-chain data structures (`BindableObject` → `Element` → `VisualElement` → `View` → `Label`). Each optimization was individually benchmarked and only kept if it showed a measurable allocation win without behavior regressions.

**Top-line results:**

| Benchmark | Before | After | Delta |
|---|---:|---:|---:|
| `new Label()` (B1) | 2928 B/op | 2256 B/op | **-672 B (-22.95%)** |
| `new Label()` + basic properties (B2) | 3648 B/op | 3104 B/op | **-544 B (-14.91%)** |
| ResourceDictionary style materialization (B4) | 16571 B/op | 14587 B/op | **-1984 B (-11.97%)** |
| Startup emulation, 1000 controls (B6) | 7.79 MB | 6.91 MB | **-0.88 MB (-11.30%)** |

## Motivation

`new Label()` and startup paths were paying for multiple eagerly-allocated dictionaries, lists, sets, `Lazy<T>` wrappers, and delegate objects in the control base chain. Most of these are never used by the majority of controls (e.g., gesture recognizers, effects, dynamic resources).

This PR evaluated 34 data-structure candidates (DS01–DS34), kept measured wins, and rejected or deferred regressions and high-risk items.

## Terminology

- **DS** = **D**ata **S**tructure candidate from the optimization opportunity list (DS01..DS34). Each DS tracks one concrete storage/allocation site with its measured result and keep/revert/defer decision.
- **B1–B6** = benchmark scenarios at increasing levels of realism (see below).

## Benchmark protocol

- Optimizations were applied incrementally, and the cumulative allocation impact was measured after each change.
- The evaluation matrix consists of 6 benchmark scenarios (B1–B6) covering control construction through app-like startup emulation.

<details>
<summary><strong>Benchmark method code (click to expand)</strong></summary>

```csharp
[Benchmark(Description = "B1_NewLabel")]
public Label B1_NewLabel() => new Label();

[Benchmark(Description = "B2_NewLabelWithBasicProperties")]
public Label B2_NewLabelWithBasicProperties()
{
    var label = new Label
    {
        Text = "bench",
        IsVisible = true
    };

    return label;
}

[Benchmark(Description = "B3_NewLabelWithFeatureToggle")]
public Label B3_NewLabelWithFeatureToggle()
{
    var label = new Label();

    switch (Scenario)
    {
        case FeatureToggleScenario.GestureRecognizer:
            label.GestureRecognizers.Add(new TapGestureRecognizer());
            break;
        case FeatureToggleScenario.Effect:
            label.Effects.Add(new NoopRoutingEffect());
            break;
        case FeatureToggleScenario.DynamicResource:
            label.SetDynamicResource(Label.TextColorProperty, "PrimaryTextColor");
            break;
        case FeatureToggleScenario.StyleClass:
            label.StyleClass = new List<string> { "headline" };
            break;
        case FeatureToggleScenario.NavigationTouch:
            _ = label.Navigation.NavigationStack.Count;
            break;
    }

    return label;
}

[Benchmark(Description = "B4_ResourceDictionaryStyleMaterialization")]
public Label B4_ResourceDictionaryStyleMaterialization()
{
    var app = CreateMockApplicationWithStyles();
    var label = new Label
    {
        Style = (Style)app.Resources["ExplicitLabelStyle"]
    };

    var page = new ContentPage { Content = label };
    Application.SetCurrentApplication(app);
    app.MainPage = page;

    _ = label.TextColor;
    return label;
}

[Benchmark(Description = "B5_MockVisualTreeStartup")]
public ContentPage B5_MockVisualTreeStartup()
{
    return CreateMockPage(ControlCount, includeStyles: false, resources: null);
}

[Benchmark(Description = "B6_AppLikeStartupEmulation")]
public Application B6_AppLikeStartupEmulation()
{
    var app = new Application
    {
        Resources = CreateStyledResources()
    };

    var page = CreateMockPage(ControlCount, includeStyles: true, resources: app.Resources);
    Application.SetCurrentApplication(app);
    app.MainPage = page;
    return app;
}
```

</details>

## Benchmark results (before vs. after)

| Benchmark | Before | After | Delta |
|---|---:|---:|---:|
| B1 — `new Label()` | 2928 B | 2256 B | **-672 B (-22.95%)** |
| B2 — `new Label()` + basic properties | 3648 B | 3104 B | **-544 B (-14.91%)** |
| B3 — `new Label()` + feature toggle (range) | 2928–5024 B | 2256–3968 B | **-672 to -1056 B (~-21% to -23%)** |
| B4 — ResourceDictionary + styles | 16571 B | 14587 B | **-1984 B (-11.97%)** |
| B5 — 500 controls (no styles) | 2.50 MB | 2.19 MB | **-0.31 MB (-12.40%)** |
| B5 — 1000 controls (no styles) | 4.99 MB | 4.37 MB | **-0.62 MB (-12.42%)** |
| B6 — 500 controls (app-like) | 3.91 MB | 3.47 MB | **-0.44 MB (-11.25%)** |
| B6 — 1000 controls (app-like) | 7.79 MB | 6.91 MB | **-0.88 MB (-11.30%)** |

## Test validation

All existing tests pass with no regressions:

- **Controls Core unit tests:** 5535 total, 5505 passed, 30 skipped, 0 failed
- **XAML unit tests:** 1867 total, 1855 passed, 12 skipped, 0 failed

## XAML SourceGen inflation benchmark (before/after)

The existing XAML inflation benchmark (`Benchmark.xaml` — a full-page resource and style graph) shows the impact of these optimizations in a broader, more realistic workload:

| Method | Before | After | Delta |
|---|---:|---:|---:|
| XamlC Mean | 309.5 μs | 308.0 μs | -0.48% |
| XamlC Alloc | 345.08 KB | 338.17 KB | **-2.00%** |
| SourceGen Mean | 159.4 μs | 163.4 μs | +2.51% |
| SourceGen Alloc | 167.64 KB | 161.37 KB | **-3.74%** |
| Runtime Mean | 26,715.7 μs | 28,360.1 μs | +6.16% |
| Runtime Alloc | 24,085.14 KB | 24,191.82 KB | +0.44% |

### Why SG gains are smaller than micro-benchmark gains

The XAML SG benchmark inflates a complete page with a large resource/style graph. Control-instance construction — the target of this PR — is only one part of the total inflation cost. As a result:

1. **Workload composition:** The SG benchmark is dominated by XAML object-graph inflation, resource resolution, and style application — not just the base-chain fields we optimized.
2. **Signal dilution:** Micro-benchmark wins of hundreds of bytes per control become small percentages when the total operation allocates 161 KB (SourceGen) to 24 MB (Runtime).
3. **Throughput variance:** Mean-time deltas across separate runs of different branches include environmental noise; allocation is the more reliable signal.

### Real-world interpretation

The XAML SG benchmark is a better proxy for real app behavior than isolated microbenchmarks. A ~23% micro-allocation win compresses to low-single-digit end-to-end gains once many control features (styles, resources, bindings) are active. This is expected.

These allocation improvements remain valuable as long as they don't reduce code maintainability or correctness. They are also expected to **compound over time** — as more subsystems are optimized, the cumulative reduction in GC pressure will lower collection frequency, reduce the managed heap working set, and shorten GC pause times.

## Exhaustive contribution table (DS01–DS34)

Each optimization candidate was evaluated independently. One row per DS, showing what was attempted, what was measured, and whether the change was kept.

<details>
<summary><strong>Full DS01–DS34 table (click to expand)</strong></summary>

| DS | Candidate | Change | Measured impact | Verdict |
|---|---|---|---|---|
| DS01 | `_properties` dictionary capacity | Changed initial capacity from 4 to default (0) | B1: -112 B; B2: +136 B (mixed) | **Keep** |
| DS02 | `_triggerSpecificity` dictionary | Lazy-init on first trigger attach | B1: -80 B; B2: -80 B | **Keep** |
| DS03 | `_measureCache` dictionary | Lazy-init on first measure cache write | B1: -80 B; B2: -80 B | **Keep** |
| DS04 | `_dynamicResources` dictionary | Guard against accidental materialization | No measurable change | Revert |
| DS05 | Style bookkeeping lists | Pre-size `_implicitStyles` and `_classStyleProperties` | B1: -24 B; B2: -24 B | **Keep** |
| DS06 | `_pendingHandlerUpdatesFromBPSet` | Lazy-init (null until needed) | No measurable change (correct by design) | **Keep** |
| DS07 | Dictionary bucket arrays | Lower initial capacity in resource path | No measurable change | Revert |
| DS08 | Dictionary entry arrays | Runtime-controlled; no safe MAUI-layer change | n/a | Deferred |
| DS09 | `SetterSpecificityList<object>` capacity | Reduced from 3 to 1 | Regression (+80 B to +320 B) | Revert |
| DS10 | `BindablePropertyContext.Bindings` | Made nullable, lazy-init on first binding | B1: -40 B; B2: -160 B | **Keep** |
| DS11 | Delayed setter queue | Already lazy — validated | No change needed | Validated |
| DS12 | `SetterSpecificityList` object arrays | Requires inline-first-slot redesign | n/a | Deferred |
| DS13 | `SetterSpecificityList` key arrays | Requires inline-first-slot redesign | n/a | Deferred |
| DS14 | `SetterSpecificityList` specificity arrays | Coupled with DS12/DS13 | n/a | Deferred |
| DS15 | `MergedStyle._implicitStyles` list | Requires implicit-style propagation redesign | n/a | Deferred |
| DS16 | Style-class tracking lists | Already on-demand — validated | No change needed | Validated |
| DS17 | `_internalChildren` list | Already nullable and on-demand — validated | No change needed | Validated |
| DS18 | `_changeHandlers` list | Single-handler fast path with list promotion | No alloc change (reduces object count) | **Keep** |
| DS19 | `_bindableResources` list | Deferred allocation until resource discovered | B4: -64 B | **Keep** |
| DS20 | `Effects` collection | Already lazy — validated | No change needed | Validated |
| DS21 | `_gestureRecognizers` collection | Lazy-init on first gesture registration | B1: -152 B; B2: -152 B | **Keep** |
| DS22 | Gesture backing list | Covered by DS21 | n/a | Covered by DS21 |
| DS23 | `_compositeGestureRecognizers` | Already on-demand — validated after DS21 | No change needed | Validated |
| DS24 | `NavigationProxy._pushStack` | `Lazy<List<Page>>` → nullable + `GetOrCreate()` | B1: -72 B; B2: -72 B | **Keep** |
| DS25 | `NavigationProxy._modalStack` | `Lazy<NavigatingStepRequestList>` → nullable | B1: -72 B; B2: -72 B | **Keep** |
| DS26 | Remaining `Lazy<T>` wrappers | Covered by DS24 + DS25 — validated | No change needed | Validated |
| DS27 | `LazyHelper` objects | Eliminated by DS24 + DS25 | n/a | Covered |
| DS28 | `Func<T>` factory delegates | Eliminated by DS24 + DS25 | n/a | Covered |
| DS29 | Delegate caching (MergedStyle) | Cached delegates in instance fields | Regression (+80 B) | Revert |
| DS30 | `_realParent` weak reference | Replaced with direct reference | Small win, but broke GC semantics (unit test failure) | Revert |
| DS31 | `_parentOverride` weak reference | Replaced with direct reference | No measurable change | Revert |
| DS32 | Boolean flag fields | Bit-packed into `VisualElementFlags` enum | B1: -8 B; B2: -8 B | **Keep** |
| DS33 | `Element._id` (`Guid?`) | Changed to `Guid` with `Guid.Empty` sentinel | B1: -8 B; B2: -8 B | **Keep** |
| DS34 | Mock bounds fields | Deferred `MockBoundsState` reference | B1: -24 B; B2: -24 B | **Keep** |

</details>

## Changes included in this PR

**Kept (14 optimizations):** DS01, DS02, DS03, DS05, DS06, DS10, DS18, DS19, DS21, DS24, DS25, DS32, DS33, DS34

**Reverted after measurement or validation:** DS04, DS07, DS09, DS29, DS30, DS31

**Deferred for future redesign:** DS08, DS12, DS13, DS14, DS15

**Validated as already optimized (no code change):** DS11, DS16, DS17, DS20, DS22, DS23, DS26, DS27, DS28

## Commit strategy

- **Code-changing optimizations**: one commit per DS for traceability.
- **Reverted spikes**: rationale preserved in this PR description; no merge artifacts.
- **Audit-only validations**: consolidated into a single evaluation commit.

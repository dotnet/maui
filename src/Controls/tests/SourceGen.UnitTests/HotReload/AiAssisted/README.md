# AI-Assisted Hot Reload Test Harness

## Purpose and Scope
This directory contains targeted XAML Incremental Hot Reload (XIHR) tests authored to cover specific regressions and scenarios reported in community issues.

The tests herein cover the **source generator boundary** and focused in-process metadata-update behavior. They compile incremental-generator output, evaluate diagnostics, inspect `UpdateComponent` source generation, and may apply Roslyn deltas to collectible test assemblies. They **do not** exercise the IDE, `dotnet watch`, Roslyn's live edit classifier, a physical device, or native rendering.

## Research Methodology
These scenarios were derived from an analysis of five GitHub issue shards (open and closed), which were triaged, deduplicated, and mapped to specific technical boundaries. The resulting 28-row canonical ledger classifies each issue into exactly one disposition (implemented, already covered, deferred to other layers, or rejected).

To add future issue-derived scenarios:
1. Identify the minimal reproduction of the XAML edit.
2. Determine if the failure is at the generator boundary (e.g., malformed code, crashed generator, dropped property) or downstream (e.g., layout, handler, IDE session poisoning).
3. If it's a generator failure, author a test in `StructuralHotReloadTests.cs` or `GeneratorRecoveryHotReloadTests.cs`.
4. Update the ledger below with the new scenario.

## Implemented AI-Assisted Tests

1. **`StructuralHotReloadTests.RootComplexElementProperty_IsNotSilentlyDropped` / `RootComplexElementProperty_AppliesToExistingPage`**
   - **Target Issue:** #36256
   - **Description:** Verifies that when replacing root content with a complex property (e.g., `CollectionView.ItemTemplate`), the generator emits compilable instantiation code and a live metadata update assigns the CollectionView and usable template to the existing page.

2. **`GeneratorRecoveryHotReloadTests.MalformedExpression_ThenRepair_RecomputesGeneratorDiagnostics`**
   - **Target Issue:** #36157
   - **Description:** Generator-boundary diagnostic recomputation probe: verifies that a repaired malformed expression no longer produces `MAUIG1003` in the generator's current run and that the resulting source compiles. It does not test IDE Error List or dotnet-watch recovery.

## Canonical Disposition Ledger

| ID | Issue | Disposition | Target/evidence | Assertion and current net11 expectation |
|---|---|---|---|---|
| XIHR-01 | [#36256](https://github.com/dotnet/maui/issues/36256) | New generator/live tests | `StructuralHotReloadTests.RootComplexElementProperty_*` | Compile/shape plus in-process metadata update; verifies complex `ItemTemplate` emission and assignment to the same live page instance. |
| XIHR-02 | [#36157](https://github.com/dotnet/maui/issues/36157) | New AI-assisted SG test | `GeneratorRecoveryHotReloadTests.MalformedExpression_ThenRepair_RecomputesGeneratorDiagnostics` | Generator diagnostics recomputation + compile only; no IDE Error List or dotnet-watch recovery claim. |
| XIHR-03 | [#36156](https://github.com/dotnet/maui/issues/36156) | Deferred IDE-host-only | IDE/Roslyn rude-edit session integration test | No generator unit test: must reproduce a compile-failing rude edit and verify a later valid edit applies in the same host session. |
| XIHR-04 | [#36155](https://github.com/dotnet/maui/issues/36155) | Existing adequate | `CSharpExpressionInterpolationTests` + `CSharpExpressionDiagnosticsTests` ternary tests | Shape/compile pass. |
| XIHR-05 | [#36158](https://github.com/dotnet/maui/issues/36158) | Existing adequate | `Maui36158Tests` | Exact diagnostic location pass. |
| XIHR-06 | [#36482](https://github.com/dotnet/maui/issues/36482) | Existing adequate | Four `DataTemplate_HotReload_*` pipeline tests | Stable named method and compile pass; device/watch deferred outside this row. |
| XIHR-07 | [#34027](https://github.com/dotnet/maui/issues/34027) | Rejected/non-HR | MSBuild/IDE contract | No edit-derived SG test. |
| XIHR-08 | [#35931](https://github.com/dotnet/maui/issues/35931) | Deferred integration/device | Android `dotnet watch` test | Must assert rendered Label changes after watch dispatch. |
| XIHR-09 | [#21083](https://github.com/dotnet/maui/issues/21083) | Rejected/non-HR | XamlC/VisualDiagnostics suite | Debug XamlC SourceInfo is not XIHR UC behavior. |
| SPEC-01 | [#35404](https://github.com/dotnet/maui/issues/35404) | Rejected/non-HR | Cross-tool gauntlet | Checklist only; no single SG test. |
| RT-01 | [#618](https://github.com/dotnet/maui/issues/618) | Existing adequate | `StyleTests.InvalidateStyleReapplies*` | Framework object-model assertions pass. |
| RT-02 | [#34722](https://github.com/dotnet/maui/issues/34722) | Existing adequate | `VisualStateManagerTests.InvalidateVisualStates*` | Framework object-model assertions pass. |
| RT-03 | [#619](https://github.com/dotnet/maui/issues/619) | Deferred framework/API | Resource refresh API + consumer tests | Existing `StaticResource` consumers must update; API absent. |
| RT-04 | [#24218](https://github.com/dotnet/maui/issues/24218) | Deferred runtime/Windows | DynamicResource reapplication integration test | Re-add resource, mutate key, assert existing target follows it. |
| RT-05 | [#35018](https://github.com/dotnet/maui/issues/35018) | Existing adequate | `Maui35018` + `HotReloadStaticResourceException` | Exception delivery/degraded reinflation pass. |
| RT-06 | [#24638](https://github.com/dotnet/maui/issues/24638) | Deferred Windows handler/layout | Windows minimum-size device test | Native effective minimum clears immediately. |
| RT-07 | [#24725](https://github.com/dotnet/maui/issues/24725) | Deferred Android/iOS handler | Native background-clear device test | Native brush becomes null/transparent; issue is `not_planned`. |
| RT-08 | [#22324](https://github.com/dotnet/maui/issues/22324) | Deferred Windows Shell/layout | Shell flyout device test | Default labels visible immediately after custom content removal. |
| RT-09 | [#17678](https://github.com/dotnet/maui/issues/17678) | Deferred layout/device | Repeated add/remove layout test | Parent height remains constant across cycles. |
| RT-10 | [#20404](https://github.com/dotnet/maui/issues/20404) | Deferred Windows layout | Grid-row device test | Arranged position changes without resize; issue is `not_planned`. |
| RT-11 | [#19075](https://github.com/dotnet/maui/issues/19075) | Deferred handler/device | Nested Brush Android/Windows test | Property notification and native repaint both required. |
| RT-12 | [#10915](https://github.com/dotnet/maui/issues/10915) | Deferred namescope/device/integration | Realized ControlTemplate/VSM test | Targeted setter remains valid and later text reload still applies. |
| RT-13 | [#15659](https://github.com/dotnet/maui/issues/15659) | Deferred Shell/device | Native tab-title test | Title updates without navigation. |
| RT-14 | [#17550](https://github.com/dotnet/maui/issues/17550) | Deferred iOS/MacCatalyst handler | Native nav-bar test | Visibility toggles immediately. |
| RT-15 | [#14377](https://github.com/dotnet/maui/issues/14377) | Deferred Windows layout | WinUI ScrollView device test | Unchanged-size child is rearranged after property change. |
| RT-16 | [#2153](https://github.com/dotnet/maui/issues/2153) | Deferred image handler/device | Native Image rerender test | Visual source/property change renders without size/Aspect nudge. |
| INFRA-01 | [#20669](https://github.com/dotnet/maui/issues/20669) | Deferred project integration | Blazor Hybrid template/watch test | XAML edit applies with `BlazorWebView` present. |
| REJECT-01 | [#21387](https://github.com/dotnet/maui/issues/21387) | Rejected/non-HR | ResourceDictionary framework backlog | Code-behind-only repro; no HR edit. |

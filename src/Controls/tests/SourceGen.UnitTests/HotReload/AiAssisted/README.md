<!-- Please let the below note in for people that find this PR -->
> [!NOTE]
> Are you waiting for the changes in this PR to be merged?
> It would be very helpful if you could [test the resulting artifacts](https://github.com/dotnet/maui/wiki/Testing-PR-Builds) from this PR and let us know in a comment if this change resolves your issue. Thank you!

# AI-Assisted Hot Reload Test Harness

## Purpose and scope

This directory contains issue-derived XAML Incremental Hot Reload (XIHR) source-generator tests and focused in-process metadata-update probes. Tests generate and compile incremental output, inspect diagnostics or `UpdateComponent`, and, where supported, apply Roslyn deltas to collectible test assemblies.

They do not exercise the IDE, `dotnet watch`, Roslyn's live edit classifier, a physical device, native rendering, or handler lifecycle. A generated-source marker proves only the documented generator branch; it does not prove runtime behavior.

## Canonical local result

With metadata updates enabled on the canonical integration branch:

| Filter | Passed | Skipped | Failed |
|---|---:|---:|---:|
| `FullyQualifiedName~HotReload.AiAssisted` | 41 | 18 | 0 |
| `FullyQualifiedName~XamlIncrementalHotReloadE2ETests` | 13 | 0 | 0 |

These are local `net11.0` Debug results with `DOTNET_MODIFIABLE_ASSEMBLIES=debug`. Theory data contributes test cases: RT-01 has three passing rows and TS-03 has two; skip-gated theories may be reported as one skipped theory.

```bash
DOTNET_MODIFIABLE_ASSEMBLIES=debug dotnet test src/Controls/tests/SourceGen.UnitTests/SourceGen.UnitTests.csproj -c Debug --filter "FullyQualifiedName~HotReload.AiAssisted"
DOTNET_MODIFIABLE_ASSEMBLIES=debug dotnet test src/Controls/tests/SourceGen.UnitTests/SourceGen.UnitTests.csproj -c Debug --filter "FullyQualifiedName~XamlIncrementalHotReloadE2ETests"
```

## Classification

| Classification | Meaning |
|---|---|
| **GREEN live** | Passing retained-object metadata-update behavior. |
| **GREEN construction** | Passing initial construction or realization behavior; no post-update runtime claim. |
| **GREEN generator guard** | Passing generation, diagnostic, source-shape, or compile assertion only. |
| **DOC-SKIP-GUARD** | Passing assertion that an unsupported writer path is explicitly skipped and still compiles. |
| **RED-PROBE (skip-gated)** | Desired live behavior is encoded but intentionally skipped for a known gap. It is not passing coverage. |
| **Capability boundary** | Harness plumbing is covered, but the in-memory host cannot faithfully supply the runtime artifact or service. |
| **Integration/host deferred** | Requires a real runtime assembly boundary, IDE/watch host, device, native handler, or lifecycle. |

## Wave-1 retained coverage

| Area | Classification | Exact method |
|---|---|---|
| #36256 complex root property | GREEN generator guard | `StructuralHotReloadTests.RootComplexElementProperty_IsNotSilentlyDropped` |
| #36256 live root replacement | GREEN live | `StructuralHotReloadTests.RootComplexElementProperty_AppliesToExistingPage` |
| Inline resources | DOC-SKIP-GUARD | `StructuralHotReloadTests.ComplexPropertyWithNestedResources_IsExplicitlySkipped` |
| Ancestor `StaticResource` | DOC-SKIP-GUARD | `StructuralHotReloadTests.ComplexPropertyWithAncestorStaticResource_IsExplicitlySkipped` |
| Nested and element-form `StaticResource` | DOC-SKIP-GUARD, two theory rows | `StructuralHotReloadTests.ComplexPropertyWithNestedStaticResourceShape_IsExplicitlySkipped` |
| Preflight diagnostics | GREEN generator guard | `StructuralHotReloadTests.StaticResourcePreflight_DiscardsParserDiagnostics` |
| #36157 recovery boundary | GREEN generator guard | `GeneratorRecoveryHotReloadTests.MalformedExpression_ThenRepair_RecomputesGeneratorDiagnostics` |

# Wave-2 living index

## Harness capabilities

| ID | Classification | Exact method and boundary |
|---|---|---|
| `cap-app-host` | GREEN construction | `HarnessCapabilityTests.ApplicationHost_ResolvesAppResources_AndRestoresPreviousApplication` |
| `cap-theme-flip` | GREEN live | `HarnessCapabilityTests.ApplicationHost_ThemeFlipIsSynchronous` |
| `cap-multi-doc` | GREEN generator guard | `HarnessCapabilityTests.MultiDocument_DictionaryOnlyEdit_TracksAllDocumentsAndCompilesPage` |
| `cap-multi-doc` | Capability boundary, skip-gated | `HarnessCapabilityTests.MultiDocument_DictionaryOnlyEdit_RetainsPageAndLabelIdentity`; nearby passing guard: `MultiDocument_DictionaryOnlyEdit_TracksAllDocumentsAndCompilesPage`. No product pass is claimed because compiled `ResourceDictionary Source=` payload is unavailable in the in-memory ALC. |
| `cap-multi-instance` | GREEN live | `HarnessCapabilityTests.MultipleInstances_RetainedRootsUpdateAndFreshRootStartsLatest` |

## Resources and themes

| ID | Classification | Exact method, guard, and issue |
|---|---|---|
| RT-01 | GREEN live, three theory rows | `ResourceAndThemeHotReloadTests.DynamicResourceKey_RenameAndReverse_UpdatesVisibleValue` |
| RT-02 | DOC-SKIP-GUARD | `ResourceAndThemeHotReloadTests.MergedDictionaries_ComplexProperty_EmitsSkipMarker` |
| RT-03 | RED-PROBE (skip-gated), #36732 | `ResourceAndThemeHotReloadTests.InlineMergedDictionaries_ReorderThenRemove_RecomputesWinner`; passing guard: RT-02 `MergedDictionaries_ComplexProperty_EmitsSkipMarker`. |
| RT-04 | DOC-SKIP-GUARD | `ResourceAndThemeHotReloadTests.BasedOnStyle_ComplexResource_EmitsSkipMarker` |
| RT-05 | RED-PROBE (skip-gated), #36732 | `ResourceAndThemeHotReloadTests.BasedOnStyle_BaseSwapAndReverse_ReappliesLiveTarget`; passing guard: RT-04 `BasedOnStyle_ComplexResource_EmitsSkipMarker`. |
| RT-06 | DOC-SKIP-GUARD | `ResourceAndThemeHotReloadTests.TriggeredStyle_ComplexProperty_EmitsSkipMarker` |
| RT-07 | RED-PROBE (skip-gated), #36732 | `ResourceAndThemeHotReloadTests.ActiveTriggerStyle_RemoveReAdd_UnappliesBeforeReattach`; passing guard: RT-06 `TriggeredStyle_ComplexProperty_EmitsSkipMarker`. |
| RT-08 | GREEN generator guard | `ResourceAndThemeHotReloadTests.AppThemeBinding_BranchEdit_IsCapturedInGeneratedComponent` |
| RT-08 | RED-PROBE (skip-gated), #36732 | `ResourceAndThemeHotReloadTests.AppThemeBinding_BranchEdit_UpdatesSelectedBranchLive`; passing guard: `AppThemeBinding_BranchEdit_IsCapturedInGeneratedComponent`. The live writer currently supplies an `IProvideValueTarget` with null `TargetProperty`. |
| RT-09 | GREEN live | `ResourceAndThemeHotReloadTests.ApplicationDynamicResource_FansOutAndFreshRootsStartLatest` |
| RT-10 | GREEN generator guard | `ResourceAndThemeHotReloadTests.SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles` |
| RT-10 | Capability boundary, skip-gated, #36732 | `ResourceAndThemeHotReloadTests.SourceMergedDictionaries_ReorderThenRemove_UsesRuntimeFallback`; passing guard: `SourceMergedDictionaries_ReorderThenRemove_TracksDocumentsAndCompiles`. The in-memory ALC has no compiled `Source=` payload. |
| RT-11 | GREEN generator guard | `ResourceAndThemeHotReloadTests.MultiDocumentMalformedThenRepair_IsAtomicAcrossPageAndDictionary` |

## Visual states and behaviors

| ID | Classification | Exact method, guard, and issue |
|---|---|---|
| VS-01 | DOC-SKIP-GUARD | `VisualStateHotReloadTests.ActiveVsmSetter_ComplexAttachedProperty_EmitsSkipMarker` |
| VS-02 | RED-PROBE (skip-gated theory), #36732 | `VisualStateHotReloadTests.ActiveVsmSetter_EditAndReverse_ReappliesImmediately`; passing guard: VS-01 `ActiveVsmSetter_ComplexAttachedProperty_EmitsSkipMarker`. |
| VS-03 | DOC-SKIP-GUARD | `VisualStateHotReloadTests.Behavior_ClearAndComplexProperty_EmitsSkipMarker` |
| VS-04 | RED-PROBE (skip-gated), #36732 | `VisualStateHotReloadTests.BehaviorCollection_RemoveReAdd_DetachesAndAttachesOnce`; passing guard: VS-03 `Behavior_ClearAndComplexProperty_EmitsSkipMarker`. |
| VS-05 | DOC-SKIP-GUARD | `VisualStateHotReloadTests.VsmState_AddRemoveReAdd_ComplexAttachedProperty_EmitsSkipMarker` |
| VS-05 | RED-PROBE (skip-gated), #36732 | `VisualStateHotReloadTests.VsmState_AddRemoveReAdd_And_FallbackReversion`; passing guard: `VsmState_AddRemoveReAdd_ComplexAttachedProperty_EmitsSkipMarker`. |
| VS-06 | DOC-SKIP-GUARD | `VisualStateHotReloadTests.ActiveVsmThemeResourceSetter_ComplexAttachedProperty_EmitsSkipMarker` |
| VS-06 | RED-PROBE (skip-gated), #36732 | `VisualStateHotReloadTests.ActiveVsmThemeResourceSetter_EditAndReverse_PreservesStateAndThemeSemantics`; passing guard: `ActiveVsmThemeResourceSetter_ComplexAttachedProperty_EmitsSkipMarker`. |

## Templates and selectors

| ID | Classification | Exact method, guard, and issue |
|---|---|---|
| TS-01 | GREEN construction/generator | `TemplateAndSelectorHotReloadTests.KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit` |
| TS-01 | RED-PROBE (skip-gated), #36482 | `TemplateAndSelectorHotReloadTests.KeyedDataTemplate_EditBody_FutureRealizationReflectsNewFactory`; passing guard: `KeyedDataTemplate_EditBody_ConstructionStableAndSourceReflectsEdit`. |
| TS-02 | GREEN construction/generator | `TemplateAndSelectorHotReloadTests.Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate` |
| TS-02 | RED-PROBE (skip-gated), #36482 | `TemplateAndSelectorHotReloadTests.Selector_FutureRealizationReflectsNewFactory`; passing guard: `Selector_ConstructionDistinguishesBranchesAndIsStableAcrossUpdate`. |
| TS-03 | GREEN construction/generator, two theory rows | `TemplateAndSelectorHotReloadTests.CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType` |
| TS-03 | RED-PROBE (skip-gated), #36482 | `TemplateAndSelectorHotReloadTests.CompiledTemplate_Retype_FutureRealizationBindsNewType`; passing guard: `CompiledTemplate_RetypeAndReverse_GeneratedSourceTracksCurrentType`. |
| TS-04 | GREEN construction | `TemplateAndSelectorHotReloadTests.ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated` |
| TS-04 | RED-PROBE (skip-gated), #36482 | `TemplateAndSelectorHotReloadTests.ControlTemplate_FutureRealizationReflectsNewVersion`; passing guard: `ControlTemplate_Construction_NamescopeXReferenceAndVsmAreIsolated`. |
| TS-05 | DOC-SKIP-GUARD | `TemplateAndSelectorHotReloadTests.BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker` |
| TS-06 | RED-PROBE (skip-gated), #36732 | `TemplateAndSelectorHotReloadTests.BindableLayoutTemplate_RetypeAndReverse_DoesNotDuplicateControllerChildren`; passing guard: TS-05 `BindableLayoutTemplate_AttachedComplex_EmitsSkipMarker`. |

## Bindings and markup

| ID | Classification | Exact method, guard, and issue |
|---|---|---|
| BM-01 | GREEN live | `BindingAndMarkupHotReloadTests.DynamicResourceToBinding_SwapAndReverse_UpdatesVisibleValue` |
| BM-02 | GREEN live | `BindingAndMarkupHotReloadTests.CustomMarkupExtension_EditAndReverse_ReprovidesValue` |
| BM-03 | DOC-SKIP-GUARD | `BindingAndMarkupHotReloadTests.MultiBinding_ComplexProperty_EmitsSkipMarker` |
| BM-04 | RED-PROBE (skip-gated), #36732 | `BindingAndMarkupHotReloadTests.DynamicResourceToBinding_RemovesDormantRegistration`; passing guard: BM-01 `DynamicResourceToBinding_SwapAndReverse_UpdatesVisibleValue`. |
| BM-05 | RED-PROBE (skip-gated theory), #36732 | `BindingAndMarkupHotReloadTests.MultiBindingChildren_RemoveReAdd_NoDuplicateExpressions`; passing guard: BM-03 `MultiBinding_ComplexProperty_EmitsSkipMarker`. |

## Nested controls

All NC provenance is an anonymized production pattern; no application-specific names or text are retained.

| ID | Classification | Exact method, guard, and issue |
|---|---|---|
| NC-01 | GREEN construction/live | `NestedControlHotReloadTests.NestedCustomControls_Construct_HaveIndependentIdentityAndNamescope` |
| NC-02 | DOC-SKIP-GUARD | `NestedControlHotReloadTests.NestedLocalResources_CustomConverter_EmitsSkipMarker` |
| NC-03 | GREEN live | `NestedControlHotReloadTests.NestedControls_LocalResources_XReference_RebindIndependently` |
| NC-04 | GREEN generator guard | `NestedControlHotReloadTests.NestedGeneratedRoots_LocalResources_EmitsDocumentedResourceDecline` |
| NC-04 | RED-PROBE (skip-gated), #36732 | `NestedControlHotReloadTests.NestedGeneratedRoots_LocalResources_XReference_RetainedInstancesAndFreshInstanceStayIndependent`; passing guard: `NestedGeneratedRoots_LocalResources_EmitsDocumentedResourceDecline`. |

## Cross-assembly invalidation

`cap-cross-asm-MC` is limited to incremental-generator reference invalidation and caching.

| ID | Classification | Exact method and boundary |
|---|---|---|
| XA-01 | GREEN generator guard | `CrossAssemblyHotReloadTests.ReferencedAssemblySwap_InvalidatesXamlPipeline` |
| XA-02 | GREEN generator guard | `CrossAssemblyHotReloadTests.UnchangedReferences_XamlPipelineCached` |

XA-01/02 cover incremental-generator invalidation and caching only. XA-03 is intentionally represented by the existing `SourceGenXamlCodeBehindTests.TestCodeBehindGenerator_ImplementationChangeDoesNotTriggerRegeneration` and `SourceGenXamlCodeBehindTests.TestCodeBehindGenerator_SignatureChangeTriggersRegeneration`. Loading a separate runtime assembly and applying deltas across that boundary remains integration/host deferred.

## Honest limitations and deferred lanes

- A compiled `ResourceDictionary Source=` payload is unavailable in the in-memory collectible ALC; `cap-multi-doc` and RT-10 therefore stop at document tracking/generation/compile, with live probes skip-gated.
- Cross-assembly tests cover generator invalidation only. Separate-runtime-assembly deltas, IDE/Hot Reload host behavior, `dotnet watch`, device lifecycle, native handlers, and rendered output remain integration lanes.
- Generated `x:Name` fields require manual C# stubs because `CodeBehindCodeWriter` is outside this harness.
- The AppThemeBinding live probe finds a null `IProvideValueTarget.TargetProperty`; RT-08's passing generator guard does not prove live re-provisioning.
- Keyed template and selector future factories remain the #36482 gap; passing construction/source guards do not prove post-update future realization.
- Complex property and collection reconciliation remains #36732. DOC-SKIP-GUARD rows prove explicit decline and compilability, not successful live reconciliation.

## Wave-1 research disposition ledger

This 28-row issue-research ledger predates the Wave-2 test IDs. Its legacy `RT-*` labels are research rows, not the Wave-2 RT-01..11 coverage identifiers above.

| ID | Issue | Disposition | Target/evidence | Assertion and current net11 expectation |
|---|---|---|---|---|
| XIHR-01 | [#36256](https://github.com/dotnet/maui/issues/36256) | New generator/live tests | `StructuralHotReloadTests.RootComplexElementProperty_*` | Compile/shape plus in-process metadata update; verifies complex `ItemTemplate` emission and assignment to the same live page instance. |
| XIHR-02 | [#36157](https://github.com/dotnet/maui/issues/36157) | New AI-assisted SG test | `GeneratorRecoveryHotReloadTests.MalformedExpression_ThenRepair_RecomputesGeneratorDiagnostics` | Generator diagnostics recomputation + compile only; no IDE Error List or dotnet-watch recovery claim. |
| XIHR-03 | [#36156](https://github.com/dotnet/maui/issues/36156) | Deferred IDE-host-only | IDE/Roslyn rude-edit session integration test | No generator unit test: must reproduce a compile-failing rude edit and verify a later valid edit applies in the same host session. |
| XIHR-04 | [#36155](https://github.com/dotnet/maui/issues/36155) | Existing adequate | `CSharpExpressionInterpolationTests` + `CSharpExpressionDiagnosticsTests` ternary tests | Shape/compile pass. |
| XIHR-05 | [#36158](https://github.com/dotnet/maui/issues/36158) | Existing adequate | `Maui36158Tests` | Exact diagnostic location pass. |
| XIHR-06 | [#36482](https://github.com/dotnet/maui/issues/36482) | Existing baseline + Wave-2 probes | Four baseline `DataTemplate_HotReload_*` pipeline tests plus TS-01..04 | Construction/source guards pass; future template/selector realization remains skip-gated under #36482. |
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

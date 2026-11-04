# TODO triage: implementation gaps, coverage, and stale-marker candidates

Snapshot: `dotnet/maui` at `43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1`.

The complete inventory contains 381 TODO-marker lines in 240 files. This report prioritizes selected clusters after reading their surrounding implementation. **Priority means suggested investigation order, not confirmed bug severity.** No runtime reproduction, performance measurement, current CI result, or live upstream issue status is claimed.

## Investigate first

### 1. Skia image downsizing returns no image

**Observed:** [SkiaImage.cs:48-73][skia-image] contains one TODO on the first `Downsize` overload. Both downsizing overloads return `null`, despite their documentation describing a resulting image. `Resize` immediately below has an implementation.

**Why it matters:** this is a concrete implementation gap in a public image operation, rather than a vague cleanup note. The companion overload is context, not a second TODO count.

**Suggested work:** confirm the intended max-dimension, aspect-ratio, no-upscale, and `disposeOriginal` contracts; compare with the other graphics backends; cover both overloads before implementing them.

### 2. Skia transform concatenation has no implemented application step

**Observed:** [SkiaCanvas.cs:831-841][skia-transform] receives a `Matrix3x2 transform` but does not use it. It reads the existing matrix; the concatenation and assignment statements are commented out next to `todo: implement me`.

**Why it matters:** callers requesting matrix concatenation do not have an implemented transform application in this method. Its exact current outcome, including the native matrix-read call, was not exercised.

**Suggested work:** cover translation, scale, rotation, multiplication order, and save/restore through the public canvas API, then align behavior with the other backends.

### 3. XAML diagnostics need separation from stale TODOs

**Observed:** [KnownMarkups.cs:604-610][known-markups] catches `XamlParseException` and throws a general exception with an `XmlnsUndeclared` message. The same file contains nine TODO-marker lines overall.

**Important distinction:** nearby branches at lines 592-599 and 613-620 already call `context.ReportDiagnostic`. Their TODOs should not all be counted as missing diagnostic implementations. The no-path and null-data-type branches at lines 532-563 also have comments about diagnostics/warnings, but the desired diagnostic policy needs confirmation.

**Suggested work:** verify the outer generator error handling, exercise malformed `x:DataType`/namespace input, and specify the desired diagnostic and source location. Refresh comments where the adjacent code already satisfies the original intent.

### 4. Screenshot rendering failures have unfinished handling

**Observed:** [Screenshot.ios.cs:40-54,70-85,97-113][screenshot] has four "test/handle this case" markers. Window/view capture does not act on a failed `TryRender`; the layer capture callback likewise has an empty failed-render branch and then calls `RenderInContext` again. One marker is a broader placeholder rather than a failed-render branch.

**Why it matters:** error and zero-size rendering contracts are not clear from these blocks. The window path can construct a result from a new empty image when no image is produced; view/layer paths allow `null`.

**Suggested work:** define intended failure/null/exception behavior, test zero-size and failed rendering, and verify layer-visibility restoration when children are skipped. Do not equate all four markers with four independent failures.

### 5. Restore coverage deliberately, not by deleting guards

| Cluster | Observed state | Recommended action |
| --- | --- | --- |
| [Issue33352.cs:1 onward][issue33352] | The test implementation is inside a block comment following "Re-enable once supporting changes are merged." | Check whether prerequisites are now present, then restore and validate the tests. The comment alone does not establish that those changes merged. |
| [BlazorWebView request interception][blazor-interception] and [HybridWebView interception][hybrid-interception] | Eight identical TODO-marker lines surround `#if !WINDOWS` conditions. In the inspected example, the guard controls custom-scheme test attributes/data; other cases have separate platform and external-service restrictions. | Track one shared WebView2 limitation with explicit platform/scenario coverage. Do not report eight whole tests disabled solely from eight marker lines. |
| [ResizetizerTests.cs:192-195][resizetizer-collect], [1043-1046][resizetizer-excludes], [1113-1116][resizetizer-selects] | Three comments say the tests have been disabled too long. Each nearby return is conditional on `!TestEnvironment.IsWindows`, so this is not evidence that they are disabled on Windows. The latter two methods contain Android build assertions after the return. | Determine intended host support and replace misleading successful early returns with the appropriate explicit coverage/skip policy. |
| [SearchBarHandlerTests.cs:335 onward][searchbar-tests] | A Windows re-enable TODO precedes a `#if !WINDOWS` block; the comment names missing native-keyboard test support. | Complete or reassess the test helper before removing the exclusion. |

These observations concern source-level inclusion/early returns. They do not establish which tests executed in any Azure DevOps run.

## Investigate next

| Candidate | Context-reviewed evidence | Boundary / suggested action |
| --- | --- | --- |
| Android legacy accessibility | [AutomationPropertiesProvider.cs:164-211][android-accessibility] generates an ID for `LabeledBy` but leaves the renderer label association commented out; another TODO concerns an Entry accessibility delegate. | Check current call sites and semantic-property mappings. This is evidence about the legacy provider, not proof that all Android accessibility labeling is broken. |
| Windows truncation parity | [Controls TextBlockExtensions.cs:105-118][windows-truncation] maps both head and middle truncation to `WordEllipsis`, with TODOs explicitly saying truncation occurs at the end. | Establish current handler reachability and platform constraints, then cover the intended visible text behavior. |
| Windows font-image DPI | [FontImageSourceService.Windows.cs:29-61][font-dpi] has three copies of a DPI TODO and explicitly resets `scale = 1` in two methods. A separate marker asks for caching. | Treat repeated DPI markers as one scaling theme; check current issue 1000 and rendering expectations before changing resolution-dependent behavior. |
| Image/font caching and Android allocation | [FontImageSourceService.Windows.cs:34][font-dpi] requests caching; [ButtonExtensions.cs:64-77][button-allocation] creates a temporary `MaterialButton` inside a background-color fallback callback. | Measure frequency/allocation cost first. No performance regression or numerical benefit was measured in this scan. |
| Hot Reload failure notification | [HotReloadHelper.cs:96-113,173-184][hot-reload] leaves view replacement unchanged after failures and writes debug output; three TODOs request notification. | There is already debug logging. Clarify user-visible failure reporting without calling the existing path wholly silent. |

## Comments that should not be implemented blindly

### Items2 flow-layout delegate defaults

[ItemsViewDelegator2.cs:98-128][items2-delegate] has three TODOs attached to `return default` for insets and spacing. However, [LayoutFactory2.cs:117-165][items2-layout] constructs compositional layouts and sets spacing there. These TODOs may be migration residue or relevant only to another layout path.

**Disposition:** establish callback reachability and layout ownership before porting old flow-layout behavior. The scan does not prove that the three default returns currently break CollectionView spacing.

### Path and stroke-shape source-generator converter stubs

[PathGeometryConverter.cs:15-22][path-converter] discards non-empty path data and emits an empty geometry. [StrokeShapeConverter.cs:64-76][stroke-converter] emits an empty `Path` for its path-data case.

There is a material reachability caveat: [NodeSGExtensions.cs:77-80][converter-routing] explicitly leaves the path/stroke known-converter mappings commented out and notes an additional numeric RoundRectangle prerequisite. [TypeConverterRegistry.cs:71-72][converter-registry] does register the stub converter objects, while the inspected active known-converter map does not select them. [NodeSGExtensions.cs:575-599][converter-fallback] also has a runtime converter fallback.

**Disposition:** treat the stubs as incomplete implementation, but do not claim that ordinary path XAML currently generates empty geometry without proving dispatch into them. Do not enable the mappings before finishing conversion behavior and parity checks.

### Internal factory scope support

[MauiFactory.cs:16-50][maui-factory] contains a scope-support TODO and special-cases singleton lifetimes. Direct construction searches found unit-test uses, not proof that the current application DI path uses this factory.

**Disposition:** establish remaining production use and intent before redesigning it. Do not generalize this comment into "MAUI does not support DI scopes."

### Android Glide disposal

[MauiCustomTarget.java:74-99][glide-target] says no one appears to dispose the C# result. The current [ImageLoaderCallback.cs:8-42][glide-callback] wraps the native disposal callback in managed image-source results.

**Disposition:** trace actual consumers and measure retained Glide resources before claiming a leak. The callback wiring is present; its existence alone also does not prove that every consumer disposes correctly.

### Old iOS WebView cookie fallback

[WebViewHandler.iOS.cs:362-374][webview-cookie] says "Implement EvaluateJavaScriptAsync," but the TODO is specifically in the fallback branch for cookie retrieval when the iOS-version check is below 11.

**Disposition:** check supported targets and branch reachability. This is not evidence that JavaScript evaluation is generally unimplemented in WebView.

### Historical references and already-implemented diagnostics

- [Build.Tasks/SetPropertiesVisitor.cs:1853][historical-marker] explicitly says the comment "used to be" a TODO. It is inventoried as a historical marker mention, not a newly identified missing feature.
- Some bare TODOs in [KnownMarkups.cs:592-620][known-markups] sit next to existing structured diagnostics. Refresh or remove them only after identifying the original remaining intent.
- Template instructions and auto-generated constructor comments are scaffolding. They should not compete with product behavior gaps in a defect-priority list.

## Version- and dependency-bound cleanup queue

Seventeen marker lines mention .NET 8/9/10/11 or `XAMCORE_5_0`. The snapshot's `global.json` selects .NET 11 tooling; a milestone mentioned in a comment is not proof that a change remains safe or required.

| Target / dependency | Marker locations | Decision needed |
| --- | --- | --- |
| .NET 8 | `IPinchGestureController.cs:16`; `RecyclerViewExtensions.cs:14` | Public ABI spelling change versus internal adapter-interface work. Preserve compatibility. |
| .NET 9 / 9-10 | `SwipeItems.cs:225`; `IHandlerDisconnectPolicies.cs:5`; `ApplicationDispatcher.cs:11`; `WrapperView.cs:147`; `AOTTemplateTest.cs:149` | Re-evaluate old removal/migration proposals and the runtime-pack property prerequisite. |
| .NET 10 | `NavigationRenderer.cs:221`; `NavigationPageToolbar.cs:186` | Verify whether the proposed toolbar/navigation restructuring is still applicable. `IHandlerDisconnectPolicies.cs:5` also mentions 10 and is counted once overall. |
| .NET 11 | `ShellItemRenderer.cs:826`; `DatePickerHandler.MacCatalyst.cs:346`; `DatePickerHandler.iOS.cs:190`; `ImageButtonHandler.iOS.cs:50`; `TimePickerHandler.MacCatalyst.cs:128`; `TimePickerHandler.iOS.cs:161` | Check planned API exposure and Shell large-title support against the actual API surface before changing anything. |
| `XAMCORE_5_0` | `IOSWebViewManager.cs:146,227` | Check the binding/API availability and current upstream resolution before removing workarounds. |

Every location above has a commit-pinned link in the [complete inventory](todo-inventory.md); filter the [CSV](todo-inventory.csv) by path or comment text.

## Practical backlog handling

Consolidate repeated markers into shared work items where they describe one mechanism: the 20 Essentials runner-setup comments, eight WebView2 guard comments, seven shape remapping comments, and repeated DPI/obsolete-API comments. Preserve per-file inventory rows so no affected surface is lost.

For low-information markers, record the missing behavior, completion condition, and issue link before deciding whether to fix or remove them. "TODO MAUI" is not a sufficient specification. External issue/PR status and supported-platform behavior were not checked by this local comment audit.

[skia-image]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Graphics/src/Graphics.Skia/SkiaImage.cs#L48-L73
[skia-transform]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Graphics/src/Graphics.Skia/SkiaCanvas.cs#L831-L841
[known-markups]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/SourceGen/KnownMarkups.cs#L530-L620
[screenshot]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Essentials/src/Screenshot/Screenshot.ios.cs#L40-L113
[issue33352]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/Issue33352.cs#L1
[blazor-interception]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/BlazorWebView/tests/DeviceTests/Elements/BlazorWebViewTests.RequestInterception.cs#L96-L105
[hybrid-interception]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/tests/DeviceTests/Elements/HybridWebView/HybridWebViewTests_Interception.cs#L89-L98
[resizetizer-collect]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/TestUtils/src/Microsoft.Maui.IntegrationTests/ResizetizerTests.cs#L192-L195
[resizetizer-excludes]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/TestUtils/src/Microsoft.Maui.IntegrationTests/ResizetizerTests.cs#L1043-L1046
[resizetizer-selects]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/TestUtils/src/Microsoft.Maui.IntegrationTests/ResizetizerTests.cs#L1113-L1116
[searchbar-tests]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/tests/DeviceTests/Handlers/SearchBar/SearchBarHandlerTests.cs#L335-L355
[android-accessibility]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/Core/Platform/Android/AutomationPropertiesProvider.cs#L164-L211
[windows-truncation]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/Core/Platform/Windows/Extensions/TextBlockExtensions.cs#L105-L118
[font-dpi]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/src/ImageSources/FontImageSourceService/FontImageSourceService.Windows.cs#L29-L61
[button-allocation]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/src/Platform/Android/ButtonExtensions.cs#L64-L77
[hot-reload]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/src/HotReload/HotReloadHelper.cs#L96-L184
[items2-delegate]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/Core/Handlers/Items2/iOS/ItemsViewDelegator2.cs#L98-L128
[items2-layout]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/Core/Handlers/Items2/iOS/LayoutFactory2.cs#L117-L165
[path-converter]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/SourceGen/TypeConverters/PathGeometryConverter.cs#L15-L22
[stroke-converter]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/SourceGen/TypeConverters/StrokeShapeConverter.cs#L64-L76
[converter-routing]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/SourceGen/NodeSGExtensions.cs#L77-L80
[converter-registry]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/SourceGen/TypeConverters/TypeConverterRegistry.cs#L71-L72
[converter-fallback]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/SourceGen/NodeSGExtensions.cs#L575-L599
[maui-factory]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/src/Hosting/Internal/MauiFactory.cs#L16-L50
[glide-target]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/AndroidNative/maui/src/main/java/com/microsoft/maui/glide/MauiCustomTarget.java#L74-L99
[glide-callback]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/src/ImageSources/Android/ImageLoaderCallback.cs#L8-L42
[webview-cookie]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Core/src/Handlers/WebView/WebViewHandler.iOS.cs#L362-L374
[historical-marker]: https://github.com/dotnet/maui/blob/43540f6afb5acc34a89cc8ef4bf0cdb48e3b7ef1/src/Controls/src/Build.Tasks/SetPropertiesVisitor.cs#L1853

# .NET MAUI Review Rules

Distilled from code reviews by senior maintainers of dotnet/maui.
Covers 142 high-discussion PRs with 2,883 review comments.

---

## 1. Handler Lifecycle & Patterns

Handlers bridge the cross-platform virtual view to the native platform view.
Lifecycle mistakes cause leaks, crashes on reconnect, and orphaned event subscriptions.

| Check | What to look for |
|-------|-----------------|
| **ConnectHandler / DisconnectHandler symmetry** | Every listener, event handler, or callback registered in `ConnectHandler` must be unregistered in `DisconnectHandler`. Asymmetry is the #1 source of handler leaks. (PR #31022, PR #32278) |
| **Don't null handler references eagerly in Disconnect** | The view might be removed and re-added to the window (e.g., Shell tab switching). Setting `_handler = null` in `DisconnectHandler` breaks reconnection. Prefer clearing subscriptions while keeping the weak reference alive. (PR #7886) |
| **Assign per-view state in AttachedToWindow / DetachedFromWindow** | For Android, register layout listeners or inset listeners in `AttachedToWindow` and unregister in `DetachedFromWindow` rather than in the constructor. This avoids leaks when views are recycled. (PR #31022, PR #32278) |
| **Mapper methods must be idempotent** | Mapper callbacks can be called at any time, not just initial setup. They must fully initialize state from scratch — don't assume defaults or rely on prior calls. (PR #15512) |
| **Use `ModifyMapping` for Controls-layer overrides** | When adding mapper properties at the Controls level that override Core behavior, use `Mapper.ModifyMapping` so user-registered mappings aren't silently replaced. (PR #15512) |
| **Commands vs Mapper properties** | Commands (`CommandMapper`) are for requests from cross-platform to platform code (`ScrollTo`, `Remove`). Mapper properties associate with `BindableProperty` values. Don't mix the two patterns. (PR #15512) |
| **Base class calls ordering** | `ConnectHandler` should call `base.ConnectHandler()` before custom setup. `DisconnectHandler` should clean up before calling `base.DisconnectHandler()`. Reversed ordering can access disposed resources. (PR #32278) |
| **Centralize listener instances via static registry** | For listeners that are per-CoordinatorLayout or per-DrawerLayout (e.g., inset listeners), prefer a static registry method over instantiating a new listener object per call. Reduces allocations and centralizes lifecycle. (PR #32278) |

---

## 2. Memory Management & Leak Prevention

iOS and Android have fundamentally different GC behaviors. iOS's reference-counting
GC is especially sensitive to cycles. Android's Java/C# bridge creates hidden roots.

| Check | What to look for |
|-------|-----------------|
| **Store handler references as `WeakReference`** | Platform views that communicate back to their handler must store `WeakReference<THandler>` — not a strong reference. A strong handler ↔ platform view cycle prevents collection, especially on iOS. (PR #7886, PR #20124) |
| **Prefer delegates/Funcs over handler references** | Layout code uses `Func<>` callbacks to communicate without coupling to handler instances. Adopt this pattern: the platform view stores a `Func<>` rather than an `IViewHandler` reference. (PR #7886) |
| **Prefer static callbacks on iOS** | iOS tends to do better cleaning things up when the callback target is a static method. Move gesture recognizer callbacks and event handlers into static methods where feasible, passing state through the sender/tag. (PR #7886) |
| **Unsubscribe and dispose Android listeners** | Android listeners that extend `Java.Lang.Object` should be unsubscribed (`view.SetOnXxxListener(null)`) in `DisconnectHandler` to remove Java's reference. Also call `_listener?.Dispose(); _listener = null;` to release the Java peer. The unsubscribe is often more important than the Dispose — if the handler is GC'd, the listener will be collected too, but Java's reference to the listener prevents that from happening. (PR #31022) |
| **Closures capturing UIKit views are leaks** | Lambdas that close over `UIView`, `UIScrollView`, or any `NSObject` subclass create hidden strong references that the iOS GC cannot break. Extract the view access into a local, use a weak reference capture, or mark the lambda `static` to force no captured variables at compile time. (PR #13499) |
| **NSString and other IDisposable iOS types** | Static `NSString` constants (e.g., notification names) should be declared once as `static readonly` fields, not allocated on every use. Repeated allocation of undisposed `NSString` increases memory pressure. (PR #13499) |
| **CollectionView attached property memory impact** | Data stored on `BindableObject` via attached properties persists for the CollectionView's lifetime. For per-item or per-layout data, store it on the handler instance instead. CollectionView memory is critical. (PR #29496) |
| **Add memory leak device tests for new subscriptions** | When adding new event subscriptions or handler references, consider adding a leak-detection device test that verifies the handler/view can be collected after disconnect. (PR #20124, PR #31022) |

---

## 3. Safe Area (iOS/macCatalyst)

Safe area is the most frequently regressed subsystem. macOS CI runs on macOS 14/15
(~28px title bar inset), but local dev may use macOS 26+ (~0px). Fixes must pass CI.

| Check | What to look for |
|-------|-----------------|
| **Opt-in model: only `ISafeAreaView` / `ISafeAreaView2` views** | Only views implementing `ISafeAreaView` should receive safe area adjustments. Non-safe-area views must return empty padding, not device insets. (PR #30337) |
| **No double-padding across ancestor chain** | Before applying safe area adjustments, walk ancestors checking if any already handle the same edges. Edge-aware: parent handling `Top` does not block child handling `Bottom`. (PR #30337, PR #15512) |
| **Raw vs adjusted inset comparison** | `_safeArea` is filtered by `GetSafeAreaForEdge` (zeros edges per `SafeAreaRegions`); raw `UIView.SafeAreaInsets` includes all edges. Never compare them — compare raw-to-raw or adjusted-to-adjusted. (PR #30337) |
| **Never gate view callbacks on window-level insets** | Comparing `Window.SafeAreaInsets` to filter per-view `SafeAreaInsetsDidChange` callbacks blocks legitimate updates. On macCatalyst with custom TitleBar, the view's insets change without the window's changing, causing a 28px shift on CI. (PR #30337) |
| **Use constants for magic strings** | Property names like `"SafeAreaInsets"` used in mapper lookups must be constants (e.g., `PlatformConfiguration.iOSSpecific.Page.SafeAreaInsetsProperty.PropertyName`). Bare strings will break silently if renamed. (PR #15512) |
| **New safe area types belong in `src/Core`** | Types like `SafeAreaRegions` must be in `Core.csproj` so that `ISafeAreaView2` can reference them. Don't add core interface dependencies to `Controls.csproj`. (PR #30337) |
| **Don't use ISafeAreaView2 if ISafeAreaView can be extended** | Before creating a versioned interface like `ISafeAreaView2`, check if the existing interface can be extended with default interface methods or deprecated members. Prefer extending over versioning. (PR #30337 — StephaneDelcroix) |

---

## 4. Layout & Measure

Layout is the hot path. Mistakes cause infinite measure loops, incorrect sizing,
and subtle clipping issues that only manifest on specific device/OS combinations.

| Check | What to look for |
|-------|-----------------|
| **Propagate changes through virtual view, not INCC** | Collection changes (e.g., map elements, picker items) should propagate via `Handler.UpdateValue(PropertyName)` at the Controls level, not via `INotifyCollectionChanged` from the platform view. INCC creates tight coupling. (PR #7886) |
| **Guard against infinite ContentSize oscillation** | On iOS, `MauiScrollView` can enter infinite loops when content with very large `ContentSize` triggers layout → resize → layout cycles. Add a guard (e.g., `EqualsAtPixelLevel` comparison) to break sub-pixel oscillation caused by animation noise. (PR #26629, PR #30337) |
| **Compare sizes at pixel resolution** | Safe area and content size comparisons must account for device-pixel rounding. Use `EqualsAtPixelLevel` (threshold ~0.0000001pt) to prevent oscillation during `TranslateToAsync` animations. (PR #30337) |
| **Don't apply padding in aspect ratio calculations** | When computing image aspect ratios on iOS, include padding for sizing but not for the aspect ratio computation itself. Padding should be added after the aspect ratio is applied. (PR #26629) |
| **Matrix test nesting for visibility propagation** | When testing `IsVisible` propagation through nested containers, test the full matrix: 2-level and 3-level nesting, set/unset sequences. A single level test misses propagation bugs. (PR #20154 — mattleibow) |

---

## 5. Platform-Specific Code

| Check | What to look for |
|-------|-----------------|
| **File extensions determine compilation targets** | `.android.cs` → Android only. `.ios.cs` → iOS AND MacCatalyst. `.maccatalyst.cs` → MacCatalyst only. `.windows.cs` → Windows only. Both `.ios.cs` and `.maccatalyst.cs` compile for MacCatalyst — check for duplicate definitions. (PR #24396) |
| **Use type aliases for namespace collisions** | `View`, `Color`, `Context` exist in both MAUI and Android/iOS/Windows namespaces. Use `using AView = Android.Views.View;`, `using NativeScrollView = Microsoft.UI.Xaml.Controls.ScrollViewer;`, etc. Prevents subtle misresolution bugs. (PR #3351 — mattleibow) |
| **Platform views must live in `Microsoft.Maui.Platform` namespace** | Custom native view wrappers (e.g., `MauiHybridWebView`, `MauiHybridWebViewClient`) must be in `Platform/<PlatformName>/` with `namespace Microsoft.Maui.Platform`. Don't put them in handler directories. (PR #22880 — mattleibow) |
| **Don't change handler generic type parameters** | Changing the generic type on a handler (e.g., `ViewHandler<IScrollView, UIScrollView>` → `ViewHandler<IScrollView, MauiScrollView>`) is a binary breaking API change. Leave existing generics as-is. (PR #13499 — PureWeen) |
| **Align cross-platform behavior** | When fixing behavior on one platform, verify the behavior is consistent on others. A flyout state fix on iOS should match Windows behavior. Reviewers will ask: "What does Windows/Android do here?" (PR #26701 — jsuarezruiz) |
| **Reuse platform APIs via extension methods** | If a platform already has a method (e.g., `FileSystemPlatformOpenAppPackageFile`), use it. Don't duplicate file-opening logic in handler-specific code. (PR #22880 — mattleibow) |

---

## 6. Android Platform

| Check | What to look for |
|-------|-----------------|
| **Store `Context` in a local before repeated use** | Accessing Android's `Context` property calls into Java each time (JNI marshaling). Store it in a local: `var context = Context;` then use the local. Same applies to any property that crosses the Java/C# bridge. (PR #26789 — jonathanpeppers) |
| **Android resource files need correct build action** | Files used in Android tests must be declared with the correct build action (e.g., `AndroidResource`, `EmbeddedResource`). A file at `/red.png` with no build action causes `FileNotFoundException` on Android only. (PR #14109 — jonathanpeppers) |
| **Use `PlatformLogger` for Android native code** | In Java code under `src/Core/AndroidNative/`, use `PlatformLogger` for logging — not `android.util.Log` directly. This ensures consistent log formatting. (PR #29780 — jonathanpeppers) |
| **Use `FragmentActivity` carefully with Glide** | When resolving Glide request managers, check Glide's own source for the correct `Activity` lookup pattern. Don't add unnecessary `FragmentActivity` checks that diverge from Glide's implementation. (PR #29780 — jonathanpeppers) |
| **Tests in Java won't run without CI changes** | New Java test projects require AzDO/yaml/CI pipeline updates. If CI changes aren't included, the tests won't execute. Write device tests in C# instead. (PR #29780 — jonathanpeppers) |

---

## 7. iOS/macCatalyst Platform

| Check | What to look for |
|-------|-----------------|
| **iOS has trouble collecting objects with handler references** | When an iOS platform view holds a reference to its handler, the reference-counting GC often cannot break the cycle. Use delegates, `Func<>`, or `WeakReference` patterns. See `DatePickerHandler.ios.cs` (`DatePickerDelegate`) for the recommended proxy pattern. (PR #7886 — PureWeen) |
| **Subscribe/unsubscribe in `MovedToWindow`** | For iOS callbacks that need handler access, subscribe when the view moves to a window and unsubscribe when removed. This is more reliable than constructor/dispose for iOS lifecycle. (PR #7886 — PureWeen) |
| **Store notification `UserInfo` before repeated access** | Accessing `notification.UserInfo` multiple times with null-conditional (`?.`) is wasteful and obscures null checks. Store `var userInfo = notification.UserInfo;`, null-check once, then use the methods. (PR #13499 — mandel-macaque) |
| **Check `Handle == IntPtr.Zero` for disposed native objects** | A `UICollectionView` or other native object may be disposed but not null. Check `collectionView?.Handle == IntPtr.Zero` as an additional guard against accessing disposed objects. (PR #29397 — jsuarezruiz) |
| **Wrap CollectionView layout callbacks in try-catch** | Layout factory callbacks can fire after disposal. Wrap in `catch (Exception ex) when (ex is ObjectDisposedException or InvalidOperationException)` to prevent crashes. (PR #29397 — jsuarezruiz) |
| **`UIImage.FromImage` creates a copy** | Calling `UIImage.FromImage` allocates a new instance. If you only need to transform the existing image, modify in place when possible to avoid double memory usage. (PR #26016 — jsuarezruiz) |
| **Use `IUITextInput` interface for cursor/text range** | When accessing `SelectedTextRange` or similar text input properties across `UITextField`/`UITextView`, use the `IUITextInput` interface to avoid duplicating code for each concrete type. (PR #13499 — mandel-macaque) |
| **macCatalyst defaults `UseSafeArea` to `true`** | Unlike iOS where `UseSafeArea` defaults to `false`, macCatalyst defaults to `true`. Code that assumes `false` across Apple platforms will misbehave on macCatalyst. (PR #30337) |

---

## 8. Windows Platform

| Check | What to look for |
|-------|-----------------|
| **Appium on WinUI can't retrieve non-accessible elements** | `BoxView` and other elements without text aren't visible to the WinUI accessibility tree. Use `Label` or elements with text content for Appium-based UI test assertions on Windows. (PR #19371 — PureWeen) |
| **Check null/empty collections before `.FirstOrDefault()`** | On Windows, calling `.FirstOrDefault().ToPlatform()` on an empty (but non-null) `Accelerators` collection will throw. Add an empty-collection guard. (PR #14740 — mattleibow) |
| **WebView2 assembly identity differs between WinUI and WPF** | The WebView2 types have different assembly identities across WinUI, WPF, and WinForms. You cannot directly share WebView2 helper code between these targets. (PR #654 — Eilon) |
| **Prefix new MSBuild properties with `Maui`** | Any new MSBuild property (e.g., `EnableXamlLoading`) must be prefixed with `Maui` (e.g., `MauiEnableXamlLoading`) to avoid collisions with WindowsAppSdk or other framework properties. (PR #19310 — jonathanpeppers) |
| **Apply theme per window, not to all windows** | In `OnWindowHandlerChanged`, don't call `ApplyThemeToAllWindows()` — it iterates all windows redundantly (N+N-1+...+1 total applications for N windows). Apply theme only to the specific window from the sender. (PR #31714 — jsuarezruiz) |
| **Track applied state to avoid redundant work** | Theme and style application should track whether it's already been applied per window (e.g., using a `Dictionary<Window, bool>`). Re-applying on every handler change without checking if the theme actually changed is a performance hit. (PR #31714 — jsuarezruiz) |

---

## 9. Navigation & Shell

| Check | What to look for |
|-------|-----------------|
| **Shell re-adds views on tab switch** | Shell removes and re-adds platform views when users switch tabs or flyouts. Code that nullifies state in `DisconnectHandler` or `RemovedFromSuperview` will break on re-navigation. (PR #7886 — PureWeen) |
| **Test flyout state after window resize AND rotation** | Flyout visibility bugs often differ between maximize/restore and rotate-to-landscape/rotate-back scenarios. Create separate tests for each rather than combining into a single test method. (PR #26701 — PureWeen) |
| **Split unrelated behaviors into separate tests** | When a test covers two different behaviors (e.g., flyout-after-maximize AND flyout-after-rotation), split into separate test methods. Combined tests obscure which scenario fails. (PR #26701 — PureWeen) |
| **Security: validate WebView URL mapping** | When loading local HTML content in `WebView`, the local file mapping hostname should be random or scoped to prevent cross-origin file access. An app calling `LoadHtml` with `baseUrl: null` could inadvertently expose local files. (PR #7672 — Eilon) |
| **More tab behavior is platform-specific** | iOS's "More" tab for overflow Shell items may not have a standard Apple-defined behavior. Verify against Apple HIG before assuming push navigation from the More tab is correct. (PR #26292 — mattleibow) |

---

## 10. CollectionView

Two handler implementations exist. Items/ is the only implementation for Android
and Windows. Items2/ is the current handler for iOS/MacCatalyst (Items/ iOS is deprecated).

| Check | What to look for |
|-------|-----------------|
| **Verify correct handler directory for platform** | Android/Windows changes go in `Handlers/Items/`. iOS/MacCatalyst changes go in `Handlers/Items2/`. Items2/ has NO Android or Windows code. Modifying the wrong directory is a no-op on the target platform. (PR #29397, PR #31336) |
| **Don't copy already-marshaled ObjC arrays** | iOS binding APIs like `indexPathsForVisibleItems` already copy the ObjC array to managed code via `CFArray.ArrayFromHandle`. Creating a second copy (e.g., `.ToArray()`) is wasteful. (PR #31336 — jonathanpeppers) |
| **MeasureFirstItem cache must distinguish item types** | When `MeasureFirstItem` is used with grouped CollectionView, the GroupHeader size gets cached and applied to all items if the cache key doesn't distinguish between headers and data items. Causes clipping. (PR #27847 — PureWeen) |
| **CollectionView inside ScrollView causes infinite loops** | Placing a CollectionView inside a ScrollView (which users shouldn't do, but do) can cause infinite layout loops on iOS due to unbounded `ContentSize`. Add guards. (PR #26629 — rmarinho) |
| **Include gallery samples alongside tests** | When fixing EmptyView, DataTemplateSelector, or other complex CollectionView features, add samples to `Controls.Sample/Pages/Controls/CollectionViewGalleries/` for manual validation alongside automated tests. (PR #25418 — jsuarezruiz) |
| **Validate item class names are unique** | ViewModels inside test files (e.g., `CollectionViewViewModel`) must be namespaced uniquely (e.g., `Issue29130ViewModel`). Duplicate class names across issues cause build errors. (PR #29496 — jsuarezruiz) |

---

## 11. Threading & Async

| Check | What to look for |
|-------|-----------------|
| **Use `Interlocked.Increment` for counters accessed from UI thread** | Even counters "most likely" called on the UI thread should use `Interlocked` for safety. A debounce counter without thread safety can miss update requests. (PR #13499 — PureWeen) |
| **Don't reset debounce counters — roll them over** | Resetting a debounce count to 0 can cause race conditions where an update request is missed. Roll the counter over at a high threshold (e.g., 100) instead. (PR #13499 — PureWeen) |
| **Check if already on main thread before dispatching** | Before calling `MainThread.BeginInvokeOnMainThread` or `Dispatcher.Dispatch`, check if you're already on the main thread. Unnecessary dispatch adds latency and can reorder operations. (PR #26153 — PureWeen) |
| **`Task.Delay` in tests needs justification** | A `Task.Delay(100)` in a device test is a flaky test waiting to happen. Reviewers will ask: "How do we know 100ms is enough?" Prefer `WaitForMainThread()` or similar deterministic helpers. (PR #14619 — jonathanpeppers) |
| **Create helper methods for common async test patterns** | Repeated `await InvokeOnMainThreadAsync(() => { })` should be extracted into a `WaitForMainThread()` helper. Duplicate async boilerplate is error-prone. (PR #14619 — jonathanpeppers) |
| **Check `DispatcherQueue` for null before posting** | On Windows, `DispatcherQueue` can be null if the Window is disposed. Guard dispatch calls with a null check. (PR #31714 — jsuarezruiz) |

---

## 12. XAML & Bindings

| Check | What to look for |
|-------|-----------------|
| **Compiled bindings require explicit `x:DataType`** | Every `{Binding}` with an explicit `Source` must have an explicit `x:DataType` on the binding or a parent element. Missing `x:DataType` prevents source generation and causes runtime reflection fallback. (PR #32444 — simonrozsival) |
| **Set `x:DataType` on root element for page-level BindingContext** | When a page sets `BindingContext` in code-behind, the root `ContentPage` element needs `x:DataType="local:MyViewModel"` to enable compiled bindings for all child elements. (PR #32444 — simonrozsival) |
| **Don't subscribe on every BindingContext change** | In `TemplatedCell` and similar reusable views, subscribing to events on BindingContext change without checking for diff will accumulate multiple subscriptions. Only subscribe if the BindingContext actually changed. (PR #14619 — rmarinho) |
| **Use `SetValue(BP, ...)` when BindableProperty exists** | When a source generator heuristic determines a BindableProperty will be generated, the generated code must use `.SetValue(BP, ...)` instead of calling the property setter directly. (PR #32597 — simonrozsival) |
| **Remove dead code from source generation** | When source-gen skips properties via `SkipProperties`, the `CreateValuesVisitor` should also skip `new` instantiations for those properties. Leftover dead code confuses reviewers and wastes binary size. (PR #32474 — simonrozsival) |
| **Future-proof message types with records** | When adding a new cross-platform messaging API (e.g., `SendRawMessage`), pass a `record` type instead of a bare `string`. This allows adding fields later without breaking the API. (PR #22880 — mattleibow) |
| **Markup extension recognition should be semantic** | Don't just check suffix (`FooExtension`); query the compilation for types implementing `IMarkupExtension`. Also support prefixed extensions (`{local:MyExtension}`). (PR #33693 — simonrozsival) |
| **Auto-escape XML-unfriendly characters in expressions** | Users should not need to type `&gt;` in XAML C# expressions. Use `<![CDATA[...]]>` or automatic escaping for `<`, `>`, `&&`, `||` in expression contexts. (PR #33693 — simonrozsival) |

---

## 13. Testing

| Check | What to look for |
|-------|-----------------|
| **UI tests must run on all applicable platforms** | Do not restrict tests to a single platform unless there is a specific technical limitation. Tests should always cover iOS, Android, and Windows unless the fix is platform-specific. (PR #32064 — PureWeen) |
| **Snapshot baselines must be updated across all platforms** | Changing background color, font, or layout requires updating snapshot baselines for Android, iOS, MacCatalyst, and Windows. Stale baselines cause false failures. (PR #25129, PR #27399 — jsuarezruiz) |
| **Screenshot size must match capture method** | If the screenshot capture mechanism changes (e.g., different DPI or crop), all baselines need regeneration. A size mismatch (e.g., 1920×1080 vs 789×563) means the capture changed, not the rendering. (PR #25129 — jsuarezruiz) |
| **Use `VerifyScreenshot(retryTimeout:)` instead of `Task.Delay`** | `VerifyScreenshot` has built-in retry logic. Adding `Task.Delay` before it is redundant. Use `retryTimeout: TimeSpan.FromSeconds(2)` for animations. (PR #32064) |
| **Make test labels visible even if content is clipped** | When testing clipping or canvas rendering, position a label at the edge of the drawn content so screenshots prove content was actually drawn, not just that the clip removed everything. (PR #28353 — mattleibow) |
| **Memory consumption tests on Android** | Use Appium's `GetMemoryInfo()` helper to validate memory consumption in tests. Set a threshold and assert memory stays below it to detect regressions. (PR #26789 — jsuarezruiz) |
| **Tests that verify two behaviors need two methods** | A test called `ShouldFlyoutBeVisibleAfterMaximizingWindow` that also tests rotation is actually two tests. Split them so failures pinpoint the exact regression. (PR #26701 — PureWeen) |
| **Test types should match test project infrastructure** | Source generator tests belong in `SourceGen.UnitTests.csproj`, not `Xaml.UnitTests.csproj`. Tests that don't need `[Values] XamlInflator` shouldn't use it — running 3 identical iterations wastes CI time. (PR #32474 — simonrozsival) |

---

## 14. Performance

| Check | What to look for |
|-------|-----------------|
| **Cache JNI property access in locals** | Android properties like `Context`, `Resources`, `ContentDescription` cross the Java/C# bridge on every access. Store in a local variable when used more than once in a method. (PR #26789 — jonathanpeppers) |
| **Avoid closures that increase GC pressure** | Closures capturing UIKit objects create extra GC-tracked objects with references to captured elements. Even if technically correct, this increases memory pressure. Prefer explicit parameter passing or static methods. (PR #13499 — mandel-macaque) |
| **`ValueTuple.GetHashCode()` may allocate** | `ValueTuple<T1,...>.GetHashCode()` for large tuples can allocate. For hash-code–critical types like `SetterSpecificity`, implement `GetHashCode()` explicitly to avoid boxing and allocation. (PR #13818 — jonathanpeppers) |
| **Prefer `IReadOnlyList<T>` on public APIs** | Public methods should return `IReadOnlyList<T>` or `IReadOnlyCollection<T>` instead of mutable `List<T>`. This communicates intent and prevents callers from corrupting internal state. (PR #14740 — mattleibow) |
| **Avoid double-copy of managed arrays from ObjC** | iOS binding APIs already marshal ObjC arrays into C# collections. Calling `.ToArray()` or `.ToList()` on the result creates a redundant copy. (PR #31336 — jonathanpeppers) |
| **Use `StringBuilder` or `List<string>` for code generation** | In source generators, building output strings with repeated `+=` is O(n²). Use `StringBuilder`, `List<string>` with `string.Join`, or append patterns that avoid intermediate allocations. (PR #32420 — simonrozsival) |
| **Allocate early, check cheap conditions first** | In validation chains, test simple boolean flags and null checks before allocating strings or doing I/O. This is especially important in mapper callbacks which run frequently. (PR #28077 — jsuarezruiz) |
| **Don't remove caches without benchmarks** | If a cache (like `SetterSpecificityList`) was added with measured performance wins, removing or replacing it requires proving the alternative provides equivalent or better performance. Use `dotnet-trace` and `speedscope.app`. (PR #17756 — jonathanpeppers) |
| **Flatten nested LINQ into single-level iteration** | Nested `foreach` + `SelectMany` over chained mappers can be flattened: `foreach (var key in Chained.Reverse().SelectMany(c => c.GetKeys()))`. But always benchmark to verify improvement. (PR #28077 — jsuarezruiz) |

---

## 15. Error Handling & Null Safety

| Check | What to look for |
|-------|-----------------|
| **Check if stream is seekable before copying** | When processing image or data streams, check `stream.CanSeek` first. If seekable, use the original stream directly instead of copying to a `MemoryStream`. (PR #29769 — jsuarezruiz) |
| **Pattern match instead of cast + null check** | Prefer `if (localCursor is CGRect rect)` over `as` + null check. Pattern matching is safer with value types (where `as` doesn't compile) and more readable. (PR #13499 — tj-devel709) |
| **Consistent null initialization** | Don't initialize nullable reference variables to `null` in some places but not others. Be consistent — either always set `= null` or never set it (let the default apply). (PR #13499 — mandel-macaque) |
| **Use `?.` chains judiciously** | Multiple `?.` on the same expression (e.g., `notification.UserInfo?.ObjectForKey(...)?.ToString()`) is harder to debug. Store intermediates and null-check explicitly when the chain is more than 2 deep. (PR #13499 — mandel-macaque) |
| **Fallback when `MauiContext` is null** | If `Window.MauiContext` is null, fall back to `Application.Current.FindMauiContext()` rather than letting a `NullReferenceException` propagate. (PR #24808 — jsuarezruiz) |
| **Initialize WebView cookies in `CoreWebView2Initialized`** | Cookie preloading that runs before `CoreWebView2` is initialized hits null references. Wire initialization into the `CoreWebView2Initialized` event. (PR #24846 — PureWeen) |
| **Try-catch for fire-and-forget platform calls** | Platform calls that can fail but whose failures are non-critical should be wrapped in `try/catch` or use `FireAndForget`. Unhandled exceptions in these paths crash the app. (PR #22880 — PureWeen) |
| **Guard against empty collections in chained calls** | `collection?.FirstOrDefault().ToPlatform()` throws if the collection is empty (not null). Check `.Any()` or use null-conditional on the result of `FirstOrDefault()`. (PR #14740 — mattleibow) |

---

## 16. API Design & Public API

| Check | What to look for |
|-------|-----------------|
| **Adding to an interface is a breaking change** | Adding members to a public interface (e.g., `IMenuElement`) is a binary breaking change. Use default interface methods (verify platform support), create a new interface (`IMenuElement2`), or add an extension method. (PR #14740 — PureWeen) |
| **Never modify `PublicAPI.Shipped.txt`** | To remove a shipped API, copy the line to `PublicAPI.Unshipped.txt` prefixed with `*REMOVED*`. Never edit the Shipped file directly. (PR #14740 — mattleibow) |
| **Don't expose setters that do nothing** | If a property setter is not wired to any handler or has no observable effect, remove it. Dead setters confuse users and accumulate API surface that can't be removed later. (PR #14740 — PureWeen) |
| **Obsolete with proper message format** | Obsolete attributes need a period at the end of the message. iOS Cecil tests enforce this convention. (PR #13499 — mandel-macaque) |
| **Public `MauiAppBuilder` extension methods require careful review** | Once added to the builder pattern, extension methods cannot be removed. Establish a process for evaluating what belongs on the builder vs what library authors should register themselves. (PR #2137 — mattleibow) |
| **Avoid breaking Xamarin.Forms migration** | When replacing types (e.g., `Position` → `Location`), consider keeping a compatibility class that inherits from the new type. This eases porting from Xamarin.Forms. (PR #7886 — jsuarezruiz) |
| **Use named constants instead of magic numbers** | Bit-shift offsets, specificity values, and layout thresholds should be `const int` with descriptive names. `new SetterSpecificity(100, 0, 0, 0)` is opaque; `SetterSpecificity.Implicit` communicates intent. (PR #13818 — jonathanpeppers) |

---

## 17. Image Handling

| Check | What to look for |
|-------|-----------------|
| **Image source services must return native image representations** | `IImageSourceService.GetDrawableAsync` should return a result that represents the actual image data, not just a status. This enables usage without a view (e.g., notification icons). (PR #2360 — mattleibow) |
| **Verify Glide callback thread assumptions** | Glide callbacks like `onResourceReady` are assumed to be on the main thread because they call `view.setImageDrawable`. But this isn't documented — verify by checking Glide's source. (PR #14109 — jonathanpeppers) |
| **Clean up bitmaps on overlay add/remove cycles** | If an overlay is removed and re-added, resources loaded during the first add must be either cached or reloadable. Disposal without a reload path causes blank overlays. (PR #3351 — mattleibow) |
| **Inner vs outer corner radius for clipping** | When using a `RoundRectangle` for clipping (e.g., inside a `Border`), the corner radius for the inner clip is different from the outer border radius. Use the inner value. Clipping with the outer value leaves visible gaps. (PR #10964 — jsuarezruiz) |

---

## 18. Gestures

| Check | What to look for |
|-------|-----------------|
| **Use `Tap` over `Click` for mobile platforms** | In UI tests, `Tap` is more flexible and mobile-appropriate. `Click` may not be converted to `Tap` on all platforms, causing test failures on Android/iOS while passing on Windows. (PR #19371 — PureWeen) |
| **Ensure gesture tests verify their precondition** | A test that verifies "GetPosition returns correct coordinates" must confirm the element was actually tapped. If the element isn't tappable (e.g., iOS BoxView issue), the test passes trivially. Use a verifiable side effect. (PR #19371 — PureWeen) |
| **Span tap region calculation across multiple lines** | The `CGRect` region for each `Span` in a `FormattedString` is incorrectly calculated when text wraps to multiple lines (inherited from Xamarin.Forms). Verify tap targets with multi-line text. (PR #15544 — jsuarezruiz) |
| **EventArgs naming convention** | Custom event args should follow `{Event}EventArgs : EventArgs` naming. E.g., if the event is `Touch`, the args class should be `TouchEventArgs`, not `DiagnosticsTouchArgs`. (PR #3351 — mattleibow) |

---

## 19. Build & MSBuild

| Check | What to look for |
|-------|-----------------|
| **Use `dotnet-public` feed to avoid dependency confusion** | NuGet sources must use the `dotnet-public` feed. Adding arbitrary third-party feeds creates a dependency confusion attack surface. New package sources require approval. (PR #5020 — jonathanpeppers) |
| **Build tasks can't depend on optional packages** | `Controls.Build.Tasks.csproj` cannot reference optional NuGet packages like Maps. Build tasks must only depend on core assemblies that are always present. (PR #7886 — mattleibow) |
| **Feature flag properties belong in `src/Core`** | Feature flags (e.g., `IsXamlLoadingSupported`) should be in `Core.csproj` / `Microsoft.Maui.dll` since all assemblies depend on it. Don't scatter feature flags across Controls or platform-specific assemblies. (PR #19310 — jonathanpeppers) |
| **Follow naming conventions: `Support` suffix → `IsXSupported`** | MSBuild property: `XamlLoadingSupport`. C# property: `IsXamlLoadingSupported`. This matches the runtime feature switch pattern used across .NET. (PR #19310 — simonrozsival) |
| **Document feature switch breakage and alternatives** | For each new feature switch, document: (1) which APIs break when disabled, (2) code examples of what users should do instead. (PR #19310 — jonathanpeppers) |
| **Targets in NuGet vs workload have different import timing** | Moving MSBuild targets from workload to NuGet changes when they're imported relative to package restore. Test the sequence: install VS → new project → NuGet restore → build. (PR #11206 — jonathanpeppers) |
| **Never commit `cgmanifest.json` or `templatestrings.json`** | These are auto-generated during CI. AI-generated PRs frequently include changes to these files. Always reset them before committing. (PR #11206) |

---

## 20. Accessibility

| Check | What to look for |
|-------|-----------------|
| **Don't disable font scaling globally** | An implicit style that disables font scaling across the app makes it inaccessible to partially-sighted users. "Rather have an ugly app that a partially blind person can use instead of a beautiful one they can't." (PR #1774 — PureWeen) |
| **WinUI Appium requires accessible elements** | Elements without text content (e.g., `BoxView`) are invisible to Appium on WinUI. Use `Label` or set `AutomationProperties.Name` for elements that need to be located in Windows UI tests. (PR #19371 — PureWeen) |
| **Test accessibility properties propagate** | When a control exposes `AutomationProperties`, verify the native accessibility tree reflects them. A broken binding silently removes accessibility. (PR #25011 — jsuarezruiz) |

---

## 21. Regression Prevention (Lessons from Reverts & Candidate Failures)

Rules distilled from 30 reverted PRs and 50 candidate-branch failures. When a PR touches these areas, apply extra scrutiny.

| Check | What to look for |
|-------|-----------------|
| **CollectionView changes need broad scenario coverage** | CV is the single highest-regression component (15 candidate failures). Any change to layout, scroll, spacing, cell alignment, Header/Footer, or `KeepLastItemInView` must be tested across all four: empty collection, single item, many items, and with grouping. A fix for one layout scenario routinely breaks another. (CollectionView candidate failures, multiple PRs) |
| **Style/theme changes have cascading effects** | `ApplyToDerivedTypes`, implicit styles, and `AppThemeBinding` interact in non-obvious ways. A fix to one style propagation path often breaks another — this pattern caused two separate reverts for PR #9648 and broke source gen in PR #32728. When touching style resolution or `AppThemeColor`, test: explicit style, implicit style, derived-type style, and dark/light theme switching. |
| **Test the fix scenario AND adjacent scenarios** | Most reverts happen because the fix works for the reported issue but breaks a neighboring case. ToolbarItem image fix (PR #28833, reverted twice) fixed one image mode while breaking others. Entry `SelectionLength` (PR #26213) fixed selection but broke focus. Require authors to enumerate what adjacent behaviors they checked. |
| **Never remove `InternalsVisibleTo` without auditing NuGet consumers** | IVT removal looks like cleanup but silently breaks community packages that depend on internal APIs. At least one revert went all the way back to a release branch because of this. Flag any `InternalsVisibleTo` deletion and ask: "Have you checked whether any published NuGet consumers reference this?" |
| **ToolbarItem/Image changes must be tested across all image source types** | Font images, file images, and tinted images each go through different code paths. PR #28833 (reverted twice) and PR #26048 show this pattern. When modifying `ImageSourceExtensions` or any ToolbarItem rendering code, verify all three modes: `FontImageSource`, `FileImageSource`, and `BitmapImageSource` with tinting. |
| **Entry/Editor focus and selection state is fragile** | `CursorPosition`, `SelectionLength`, keyboard show/hide, and focus order interact tightly on every platform. PR #26213 shows how a `SelectionLength` change broke Entry focus. Flag any handler change that touches these properties and ask whether focus behavior was verified after the change — especially when the keyboard is dismissed and re-shown. |
| **Measurement timing on iOS lags behind property changes** | `UIButton.TitleLabel.Bounds` and `UIView` frame values are not updated synchronously after a property change — the layout pass hasn't run yet. PR #25122 (tj-devel709) shows the correct pattern: measure the title manually instead of reading `.Bounds`. Flag any iOS handler code that reads frame/bounds immediately after setting a property, without deferring to the next layout pass. |
| **Template changes need all-template validation** | A template fix for MAUI app can break Blazor hybrid or multi-project templates. `MapStaticAssets` vs `UseStaticFiles` (candidate failure) shows how a single API swap breaks one template variant silently. Any `src/Templates/` change must be validated against all template IDs: `maui`, `maui-blazor`, `maui-blazor-web`, `mauilib`, `maui-multiproject`. |
| **Candidate-branch PRs must not mix concerns** | PureWeen's explicit rule (PR #33838): candidate fixes should not bundle unrelated flakiness fixes. A PR that mixes "fix the regression" with "also fix this flaky test" makes bisection impossible and obscures what actually fixed the candidate failure. Flag mixed-concern candidate PRs. |
| **Screenshot tests must prove content was drawn, not just that clipping worked** | mattleibow on PR #28353: "give this label a negative margin so that we can see that the image is drawn at all." A clipping test that passes when the view renders nothing is not a useful test. Flag screenshot tests for `GraphicsView`, `Image`, or any drawing surface that don't verify the content region itself — use a visible element inside the clip boundary as a sentinel. |
| **Arithmetic in index/position calculations needs explicit parentheses** | mattleibow on PR #23369: "This line of code is a bit ambiguous for a quick read." Silent operator-precedence bugs in scroll offset, index math, or spacing calculations are hard to spot and have caused gesture/tap regressions. Flag expressions like `a + b * c` or `offset - padding / 2` without explicit parentheses when they appear in position or size computations. |
| **Major dependency upgrades need broad platform validation** | WindowsAppSDK upgrade (PR #32174) was reverted because it broke too many things simultaneously. Flag PRs that bump `WindowsAppSDK`, `Microsoft.Maui.*` NuGet versions, or other platform SDK dependencies, and ask whether CI was green on all platforms (Android, iOS, MacCatalyst, Windows) before merge, not just the changed platform. |
| **ContentPresenter BindingContext propagation breaks explicit TemplateBindings** | Propagating `BindingContext` through `ContentPresenter` overwrites `{TemplateBinding}` values that were set explicitly. This was a reverted PR. Flag any handler or renderer change that sets or propagates `BindingContext` on a `ContentPresenter` or control template root without verifying that `TemplateBinding` expressions still resolve correctly. |

---

## 22. Most Frequently Regressed Components

Components ranked by regression frequency from 366 analyzed PRs (reverts + candidate fixes + regression fixes):

| Component | Regressions | Key Risk Areas |
|-----------|-------------|----------------|
| CollectionView | 15 | Layout, scroll position, spacing, cell alignment, Header/Footer |
| Image/Graphics | 15 | Aspect ratio, CornerRadius, Background, DrawString |
| Theme/Style | 8 | AppThemeBinding, implicit styles, ApplyToDerivedTypes |
| CarouselView | 7 | ScrollTo, CurrentItem, ItemSpacing, loop mode |
| Gesture/Tap | 7 | TapGestureRecognizer, SwipeView, outside-tap dismiss |
| Button/Entry | 7 | Dynamic resize, focus/selection, AppThemeBinding colors |
| Toolbar | 5 | Icon color, back button, BarTextColor across modes |
| Shell/TabBar | 4 | TabBarIsVisible, Shell crashes, section rendering |

Use this table as a triage guide: PRs touching these components warrant a more thorough pass through sections 1–21 above, with particular attention to the adjacent-scenario rule (§21 row 3) and the component-specific rows in this section.

---

## 23. Trim / NativeAOT Safety

Trim and NativeAOT warnings (IL2026, IL3050) indicate the linker/ILC cannot prove code
is safe. Incorrect fixes (suppression) cause silent runtime failures in published apps.
This section is sourced from the HybridWebViewHandler IL3050 incident (Issue #34867,
PR #34958) where multiple AI agents recommended suppression instead of the correct
structural fix.

| Check | What to look for |
|-------|-----------------|
| **`[UnconditionalSuppressMessage]` for IL2026/IL3050 is almost always wrong** | If a PR adds `[UnconditionalSuppressMessage("AOT", "IL3050:...")]` or `[UnconditionalSuppressMessage("Trimming", "IL2026:...")]`, it is very likely hiding a real problem rather than fixing it. The correct response is to restructure the code so the analyzer can prove safety — typically via the extract-method pattern below. Only accept suppression if the PR includes proof that the code path is genuinely unconditionally safe (e.g., the type is explicitly preserved). (Issue #34867 — simonrozsival) |
| **`[FeatureGuard]` does NOT suppress indirect annotation chains** | `[FeatureGuard(typeof(RequiresDynamicCodeAttribute))]` on a `RuntimeFeature` property suppresses IL3050 only for **direct** calls to `[RequiresDynamicCode]` methods inside the guarded block. It does **not** suppress warnings from indirect chains — e.g., generic type parameters with `[DynamicallyAccessedMembers]` that trace back to an annotated type. If a PR claims "the feature guard handles it" but the warning goes through a generic like `AddHandler<T, TRender>()`, the guard is NOT sufficient. (Issue #34867 — simonrozsival) |
| **Extract-method pattern for feature-guarded annotated types** | When registering a type annotated with `[RequiresDynamicCode]` or `[RequiresUnreferencedCode]` inside a `[FeatureGuard]`-protected block, and the registration goes through a generic API with `[DynamicallyAccessedMembers]`, extract the call into a separate method annotated with the same attributes. This converts the indirect chain into a direct call that the `[FeatureGuard]` can suppress. Reference: `AppHostBuilderExtensions.AddHybridWebViewHandler()` in `src/Controls/src/Core/Hosting/AppHostBuilderExtensions.cs`. (PR #34958 — simonrozsival) |
| **Don't reference annotated types from the extracted helper's attributes** | When creating the extracted helper method, use a local `const string` for the annotation message instead of referencing a const on the annotated type (e.g., `HybridWebViewHandler.DynamicFeatures`). Accessing a const on the type embeds the type reference in the caller's IL metadata, re-introducing the very problem the extraction was meant to solve. (PR #34958, commit 2 — simonrozsival) |
| **Read the full annotation chain before proposing any fix** | For any IL2026/IL3050 issue, trace: (1) which method emits the warning, (2) what callee/generic causes it, (3) whether the chain is direct (method call) or indirect (generic constraint / `[DynamicallyAccessedMembers]`), (4) whether a `[FeatureGuard]` exists and whether it covers the chain type. Do not propose a fix until this chain is documented. (Issue #34867) |
| **`#pragma warning disable` for ILxxxx is equally wrong** | Same reasoning as `[UnconditionalSuppressMessage]` — it hides the problem. `#pragma warning disable IL3050` in production code should be flagged with the same severity. |

---

## What NOT to Flag

Do not waste reviewer time on these:

| Category | Why |
|----------|-----|
| **Style/formatting** | CI enforces via `dotnet format`. |
| **Missing XML docs on non-public APIs** | Not required by MAUI convention. |
| **Test naming preferences** | Unless names are genuinely misleading. |
| **`var` vs explicit types** | Project allows both; consistency within a file is sufficient. |
| **Micro-optimizations in cold paths** | Readability wins unless profiling proves it's a hot path. |
| **Single-use LINQ vs foreach** | Either is fine; don't bikeshed. |
| **Comment style** | Only flag if a comment is factually wrong or stale. |
| **PR commit count/squash** | That's the author's workflow choice. |

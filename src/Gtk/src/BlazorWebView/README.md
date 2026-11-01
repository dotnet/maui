# WebView, HybridWebView and BlazorWebView on GTK 4

On Linux the controls run on **WebKitGTK 6** (`libwebkitgtk-6.0`) through `WebkitGtkSharp4` / `JavaScriptCoreSharp4`. WebKitGTK exists on Linux only
(gvsbuild ships no WebKit): where it cannot be loaded each handler creates a `Gtk.Label` that says why, so an app with a web view still starts.
`MAUI_GTK_DISABLE_WEBKIT=1` forces that placeholder; `GtkWebKit.IsAvailable` / `UnavailableReason` report the state.

**Windows:** `WebView` runs on **WebView2** (`Microsoft.Web.WebView2`, referenced by `Microsoft.Maui.Gtk`; needs the WebView2 Runtime, part of Windows 11 and Edge).
`GtkWebBackend` picks the engine at run time (Linux -> WebKitGTK, Windows -> WebView2, else a placeholder label); without the runtime the label reads
"WebView2 Runtime not installed (https://go.microsoft.com/fwlink/p/?LinkId=2124703)". `MAUI_GTK_DISABLE_WEBVIEW2=1` forces the placeholder.
`HybridWebView` and `BlazorWebView` run on WebView2 too (see "HybridWebView" and "BlazorWebView" below); the seams below are what they use.

### WebView2 backend

| Piece | What it does |
|---|---|
| `MauiGtkWebView2Host` | empty focusable `Gtk.Widget` that owns the `CoreWebView2Controller`: created on *realize* with the toplevel's HWND (`gdk_win32_surface_get_handle`), closed on *unrealize*. Bounds = widget bounds + the surface transform, times `gdk_surface_get_scale`, pushed on size-allocate, map and every frame-clock layout (a position-only change does not allocate); visible exactly while mapped (a hidden ancestor hides it); GTK focus -> `MoveFocus`, a click inside -> GTK `grab_focus`, Tab out -> toplevel `SetFocus` + `child_focus`; toplevel moves (`EVENT_OBJECT_LOCATIONCHANGE`) -> `NotifyParentWindowPositionChanged` |
| `GtkWebView2Environment` | availability probe (`GetAvailableBrowserVersionString`) and the one shared `CoreWebView2Environment`; user data folder `WEBVIEW2_USER_DATA_FOLDER`, else `%LOCALAPPDATA%\<process name>\WebView2` |
| `IGtkWebEngine` | what the WebView handler needs from an engine: `GtkWebKitEngine` (the former handler code) and `MauiGtkWebView2Engine`. WebView2 queues scripts and the last navigation until the controller exists, and replays the address after a re-realization (history is lost) |
| Seams for HybridWebView (used) / BlazorWebView | `GtkWebBackend.CreatePlatformView` / `CreateEngine`; `MauiGtkWebView2Host.CoreWebView2Initialized` (raised per controller before the first navigation: add `WebResourceRequested` filters, document-created scripts, `WebMessageReceived`), `.CoreWebView2`, `.Controller`, `.Services` (fakes in tests); `GtkWebView2Environment.GetAsync` |

WebView2 notes: the control exists only once the host is in a realized window (a view that is not in a window loads nothing yet); `HtmlWebViewSource.BaseUrl` is ignored and HTML is limited to 2 MB;
the web view is a native child window, see "airspace" under Known limitations in the main README.

| Control | Handler | Assembly | Register |
|---|---|---|---|
| `WebView` | `GtkWebViewHandler` (`: WebViewHandler`) | `Microsoft.Maui.Gtk` | automatic with `UseMauiGtkCore()` (type map entry) |
| `HybridWebView` | `GtkHybridWebViewHandler` (`: HybridWebViewHandler`) | `Microsoft.Maui.Gtk` | automatic with `UseMauiGtkCore()` |
| `BlazorWebView` | `GtkBlazorWebViewHandler` (`: BlazorWebViewHandler`) | `Microsoft.AspNetCore.Components.WebView.Maui.Gtk` | `builder.Services.AddMauiBlazorWebView().UseGtkPlatformHandler();` |

Runtime packages: `libwebkitgtk-6.0-4` (Debian/Ubuntu), `webkitgtk6.0` (Fedora). Containers without user namespaces need
`WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1`.

## WebView

| Member | Implementation |
|---|---|
| `Source` | `UrlWebViewSource` -> `LoadUri` (WebView2: `Navigate`), `HtmlWebViewSource` -> `LoadHtml(html, baseUrl)` (WebView2: `NavigateToString`, no base URL) |
| `Navigating` | raised by the handler for loads the app starts (`Source`, `Reload`) and by the engine for navigations the page starts (links, forms, back/forward, redirects: WebKit `decide-policy`, WebView2 `NavigationStarting`); `Cancel` stops the load. WebKit repeats it for the app's own loads, WebView2 does not. `target=_blank` links load in the same view |
| `Navigated` | WebKit `load-changed` Finished / `load-failed`, WebView2 `NavigationCompleted`; a navigation cancelled in `Navigating` raises nothing |
| `GoBack`, `GoForward`, `Reload` | set the event (`Back`/`Forward`/`Refresh`) that the next `Navigating`/`Navigated` report; `CanGoBack/Forward` follow the back-forward list |
| `UserAgent` | `Settings.UserAgent` (the engine default is read back when unset; WebView2 once the controller exists) |
| `Eval`, `EvaluateJavaScriptAsync` | `EvaluateJavascript` / `ExecuteScriptAsync`; strings come back as they are, other values as JSON, `undefined`/`null` as `null` |
| `ProcessTerminated` | `web-process-terminated` / `ProcessFailed` (browser or render process exited) |

Cookies: on WebView2 the container's cookies for a URL are written to the browser before each navigation to it and the browser's cookies for it are added to the container after a successful load
(cookies deleted in the browser stay in the container); the WebKitGTK `Cookies` container is not synchronised.

Implementation note: GtkSharp's event wrappers of the `gboolean`-returning WebKit signals (`decide-policy`, `load-failed`) never fire, so `GtkWebKitSignals` connects them with `g_signal_connect_data`.

## HybridWebView

- The web app is served from the app package (the folder of the executable; `HybridRoot` is a sub-folder of it, as `GtkFileSystem` reads package files) through the **`app://0.0.0.1/`** custom scheme (WebKit does not let an app serve `https`; iOS does the same).
  The scheme is registered once on the default `WebContext` as secure and CORS-enabled (`fetch()` to a custom scheme needs it) and routed to the requesting view.
- `hybridwebview.js` already speaks the iOS protocol: JavaScript posts to `window.webkit.messageHandlers.webwindowinterop` (registered on the view's `UserContentManager`), .NET answers with `window.external.receiveMessage(...)`.
- `InvokeDotNet` is a `fetch` to `__hwvInvokeDotNet` answered by `GtkHybridWebViewResponder`. **WebKitGTK exposes no request headers**, so the `X-Maui-Invoke-Token`/`Origin` check of the other platforms is replaced by: `POST` with a body, issued by a page of the app origin. Script messages are accepted only while the view shows a page of the app origin.
- App-side `WebResourceRequested` interception (`WebViewInitializationCompleted`'s `CoreWebView2`) is not available.
- **Windows (WebView2):** the app origin is **`https://0.0.0.1/`** (as on Windows and Android). `GtkHybridWebView2Bridge` adds a `WebResourceRequested` filter `https://0.0.0.1/*` and answers it with the same `GtkHybridWebViewResponder` (WebView2 deferral completed on the GTK thread; the response body is a `MemoryStream` WebView2 reads after the handler returned). WebView2 exposes request headers, so `__hwvInvokeDotNet` gets the `X-Maui-Invoke-Token` + `Origin`/`Referer` check of the other platforms. `hybridwebview.js` posts with `chrome.webview.postMessage(encodeURIComponent(msg))` (`WebMessageReceived`, accepted from the app origin only) and receives `PostWebMessageAsString(Uri.EscapeDataString(msg))`; messages sent before the controller exists are kept and posted when it is configured. `EvaluateJavaScriptAsync`/`InvokeJavaScriptAsync` go through the engine's `ExecuteScriptAsync`. The shared WebView2 environment is used, so `WebViewInitializationStartedEventArgs.BrowserExecutableFolder`/`UserDataFolder`/`EnvironmentOptions` and the controller options are not applied (`WebViewInitializationCompleted` carries no `CoreWebView2`; use `MauiGtkWebView2Host.CoreWebView2Initialized`).

## BlazorWebView

- `GtkWebViewManager : WebViewManager` navigates to `app://0.0.0.1/`, serves the framework files, the host page and `wwwroot/**` through the scheme (client-side routes without an extension fall back on the host page), and carries messages as `window.external.sendMessage` / `__dispatchMessageCallback("...")`.
- **WebView2 (Windows):** the app is served from `https://0.0.0.1/` (as on Windows MAUI): `WebView2Transport : IGtkBlazorTransport` (`BlazorWebView/GtkWebView2Transport.cs`) adds the filter `https://0.0.0.1/*` and answers `WebResourceRequested` with `GtkWebViewManager.RespondAsync` (a deferral, completed on the GTK thread), registers a document-created script that defines `window.external.sendMessage` / `receiveMessage` over `chrome.webview` and starts Blazor on `DOMContentLoaded`, receives page messages in `WebMessageReceived` (`MessageReceived` ignores pages outside the origin) and sends with `PostWebMessageAsString`. Every new `CoreWebView2` (first realization, re-realization) is configured before it navigates: the start page loads only after the script is registered. Links the page starts (`engine.Navigating`, `target=_blank`) go through `UrlLoading` like on WebKit. `GtkWebViewManager` takes the origin as a parameter (`app://0.0.0.1/` on WebKit). The shared `SharedSource/WebView2WebViewManager.cs` is not used (it is bound to the WinForms/WPF/WinUI controls). The control exists once the view is in a realized window; the airspace limits of the main README apply. the host page references `_framework/blazor.webview.js` with `autostart="false"` (as on every MAUI platform).
- `IBlazorWebView.UrlLoading` decides links: `OpenInWebView` (default for the app origin), `OpenExternally` (system browser through `Launcher`), `CancelLoad`.
- Files come from `<executable folder>/<content root>` (`GtkMauiAssetFileProvider`, rooted, `..` refused). `Microsoft.AspNetCore.Components.WebView.Maui.Gtk.targets` (imported automatically by the NuGet package; `Import` it for a project reference) copies the **static web assets** (`wwwroot/**`, `_framework/blazor.webview.js`, `_content/**`) to `<output>/wwwroot`, the job `ConvertStaticWebAssetsToMauiAssets` does on MAUI. It needs a project that uses the static web assets targets (`Microsoft.NET.Sdk.Razor`, as the MAUI Blazor template does). `GtkBlazorCopyStaticWebAssets=false` opts out.
- Developer tools (`AddBlazorWebViewDeveloperTools()`) turn on WebKit's `enable-developer-extras`, or WebView2's `AreDevToolsEnabled` and default context menus (off otherwise).
- `BlazorWebViewInitializing/Initialized` fire with empty arguments: the net10.0 event args carry no GTK-specific members. Customise the view through `BlazorWebView.HandlerChanged` (`handler.PlatformView` is the `WebKit.WebView` on Linux, the WebView2 host widget on Windows).

Sample: `src/Gtk/samples/Maui.Gtk.BlazorSample` (`Microsoft.NET.Sdk.Razor`, one `BlazorWebView`, a counter component).

```csharp
builder.UseMauiApp<App>().UseMauiGtkCore();
builder.Services.AddMauiBlazorWebView().UseGtkPlatformHandler();   // after the GTK backend
```

## Tests

`src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests/Web`.

| Group | Needs | Where it runs |
|---|---|---|
| Pure logic: scripts and message framing, availability decision, `GtkHybridWebViewResponder`, `GtkWebViewManager` against a fake transport (both origins), the WebView2 shim script, request filter and response headers (`WebView2BlazorProtocolTests`), file provider | nothing | everywhere |
| Engine choice, WebView2 availability decision, WebView2 script-result conversion (`WebBackendTests`); `MauiGtkWebView2Host` against a fake controller (`WebView2HostTests`: realize/unrealize, bounds at 100/150/200 %, position-only change, map/ancestor visibility, focus, toplevel moves) | GTK 4 | everywhere |
| Placeholder handlers (`[GtkFact]`) | GTK 4 | Windows and Linux (with an engine the same tests assert the real view) |
| WebView on the platform engine (`[WebEngineFact]`, `WebViewHandlerTests`): navigation, cancel, history, JavaScript, user agent, disconnect | WebKitGTK 6 + display, or the WebView2 Runtime | Linux (WebKit) and Windows (WebView2); skipped with the reason where no engine is usable |
| Real WebView2 (`[WebView2Fact]`, `WebView2EngineTests`): queued operations, controller bounds and visibility, child window + input, re-realization, `CoreWebView2Initialized`, cookies, new-window links | WebView2 Runtime | Windows only |
| HybridWebView page load, `InvokeDotNet`, raw messages, `EvaluateJavaScriptAsync`/`InvokeJavaScriptAsync`, foreign-origin messages, 400/404/405 answers (`[WebEngineFact]`, `HybridWebViewHandlerTests`; token header check `[WebView2Fact]`) | WebKitGTK 6 + display, or the WebView2 Runtime | Linux (WebKit) and Windows (WebView2); skipped with the reason where no engine is usable |
| Blazor render + click, app origin, `UrlLoading` for foreign links, disconnect (`[WebEngineFact]`, `BlazorWebViewHandlerTests`) | WebKitGTK 6 + display, or the WebView2 Runtime | Linux (WebKit) and Windows (WebView2); skipped with the reason where no engine is usable | set `MAUI_GTK_REQUIRE_NATIVE=1` to make an unusable runtime a failure |

```bash
# Debian 13 / WSL, headless
WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1 MAUI_GTK_REQUIRE_NATIVE=1 GDK_BACKEND=x11 \
  dbus-run-session -- xvfb-run -a --server-args="-screen 0 1280x1024x24" \
  dotnet test src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests --filter "FullyQualifiedName~Microsoft.Maui.Gtk.UnitTests.Web"
```

The test project uses `Microsoft.NET.Sdk.Razor` so the Blazor static web assets reach the test output (`wwwroot/_framework/blazor.webview.js`).

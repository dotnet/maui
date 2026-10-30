# WebView, HybridWebView and BlazorWebView on GTK 4

All three controls run on **WebKitGTK 6** (`libwebkitgtk-6.0`) through `WebkitGtkSharp4` / `JavaScriptCoreSharp4`. WebKitGTK exists on Linux only
(gvsbuild ships no WebKit): where it cannot be loaded each handler creates a `Gtk.Label` that says why, so an app with a web view still starts.
`MAUI_GTK_DISABLE_WEBKIT=1` forces that placeholder; `GtkWebKit.IsAvailable` / `UnavailableReason` report the state.

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
| `Source` | `UrlWebViewSource` -> `LoadUri`, `HtmlWebViewSource` -> `LoadHtml(html, baseUrl)` |
| `Navigating` | raised by the handler for loads the app starts (`Source`, `Reload`; WebKit raises no policy decision for those) and from WebKit's `decide-policy` for navigations the page starts (links, forms, back/forward); `Cancel` stops the load. `target=_blank` links load in the same view |
| `Navigated` | `load-changed` Finished (Success) / `load-failed` (Failure); a navigation cancelled in `Navigating` raises nothing |
| `GoBack`, `GoForward`, `Reload` | set the event (`Back`/`Forward`/`Refresh`) that the next `Navigating`/`Navigated` report; `CanGoBack/Forward` follow the back-forward list |
| `UserAgent` | `Settings.UserAgent` (the WebKit default is read back when unset) |
| `Eval`, `EvaluateJavaScriptAsync` | `EvaluateJavascript`; strings come back as they are, other values as JSON, `undefined`/`null` as `null` |
| `ProcessTerminated` | `web-process-terminated` |

Not supported: the `Cookies` container is not synchronised.

Implementation note: GtkSharp's event wrappers of the `gboolean`-returning WebKit signals (`decide-policy`, `load-failed`) never fire, so `GtkWebKitSignals` connects them with `g_signal_connect_data`.

## HybridWebView

- The web app is served from the app package (the folder of the executable; `HybridRoot` is a sub-folder of it, as `GtkFileSystem` reads package files) through the **`app://0.0.0.1/`** custom scheme (WebKit does not let an app serve `https`; iOS does the same).
  The scheme is registered once on the default `WebContext` as secure and CORS-enabled (`fetch()` to a custom scheme needs it) and routed to the requesting view.
- `hybridwebview.js` already speaks the iOS protocol: JavaScript posts to `window.webkit.messageHandlers.webwindowinterop` (registered on the view's `UserContentManager`), .NET answers with `window.external.receiveMessage(...)`.
- `InvokeDotNet` is a `fetch` to `__hwvInvokeDotNet` answered by `GtkHybridWebViewResponder`. **WebKitGTK exposes no request headers**, so the `X-Maui-Invoke-Token`/`Origin` check of the other platforms is replaced by: `POST` with a body, issued by a page of the app origin. Script messages are accepted only while the view shows a page of the app origin.
- `WebResourceRequested` interception is not available.

## BlazorWebView

- `GtkWebViewManager : WebViewManager` navigates to `app://0.0.0.1/`, serves the framework files, the host page and `wwwroot/**` through the scheme (client-side routes without an extension fall back on the host page), and carries messages as `window.external.sendMessage` / `__dispatchMessageCallback("...")`.
- A user script (document end, top frame) defines `window.external`, then calls `Blazor.start()`; the host page references `_framework/blazor.webview.js` with `autostart="false"` (as on every MAUI platform).
- `IBlazorWebView.UrlLoading` decides links: `OpenInWebView` (default for the app origin), `OpenExternally` (system browser through `Launcher`), `CancelLoad`.
- Files come from `<executable folder>/<content root>` (`GtkMauiAssetFileProvider`, rooted, `..` refused). `Microsoft.AspNetCore.Components.WebView.Maui.Gtk.targets` (imported automatically by the NuGet package; `Import` it for a project reference) copies the **static web assets** (`wwwroot/**`, `_framework/blazor.webview.js`, `_content/**`) to `<output>/wwwroot`, the job `ConvertStaticWebAssetsToMauiAssets` does on MAUI. It needs a project that uses the static web assets targets (`Microsoft.NET.Sdk.Razor`, as the MAUI Blazor template does). `GtkBlazorCopyStaticWebAssets=false` opts out.
- Developer tools (`AddBlazorWebViewDeveloperTools()`) turn on WebKit's `enable-developer-extras`.
- `BlazorWebViewInitializing/Initialized` fire with empty arguments: the net10.0 event args carry no GTK-specific members. Customise the view through `BlazorWebView.HandlerChanged` (`handler.PlatformView` is the `WebKit.WebView`).

Sample: `src/Gtk/samples/Maui.Gtk.BlazorSample` (`Microsoft.NET.Sdk.Razor`, one `BlazorWebView`, a counter component).

```csharp
builder.UseMauiApp<App>().UseMauiGtkCore();
builder.Services.AddMauiBlazorWebView().UseGtkPlatformHandler();   // after the GTK backend
```

## Tests

`src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests/Web`.

| Group | Needs | Where it runs |
|---|---|---|
| Pure logic: scripts and message framing, availability decision, `GtkHybridWebViewResponder`, `GtkWebViewManager` against a fake transport, file provider | nothing | everywhere |
| Placeholder handlers (`[GtkFact]`) | GTK 4 | Windows and Linux (on Linux with WebKit the same tests assert the real view) |
| Real web views (`[WebKitFact]`): navigation, JavaScript, history, user agent, Hybrid messaging and `InvokeDotNet`, Blazor render + click | WebKitGTK 6 + display | Linux only; skipped with the reason on Windows; set `MAUI_GTK_REQUIRE_NATIVE=1` to make an unusable runtime a failure |

```bash
# Debian 13 / WSL, headless
WEBKIT_DISABLE_SANDBOX_THIS_IS_DANGEROUS=1 MAUI_GTK_REQUIRE_NATIVE=1 GDK_BACKEND=x11 \
  dbus-run-session -- xvfb-run -a --server-args="-screen 0 1280x1024x24" \
  dotnet test src/Gtk/tests/Microsoft.Maui.Gtk.UnitTests --filter "FullyQualifiedName~Microsoft.Maui.Gtk.UnitTests.Web"
```

The test project uses `Microsoft.NET.Sdk.Razor` so the Blazor static web assets reach the test output (`wwwroot/_framework/blazor.webview.js`).

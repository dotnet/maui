# External BlazorWebView backends

`BlazorWebView` ships built-in handlers for Android, iOS, MacCatalyst and Windows. A third-party
package can supply a handler for any other platform by implementing
`Microsoft.AspNetCore.Components.WebView.Maui.IBlazorWebViewHandler` and registering it with
`IMauiBlazorWebViewBuilder.UsePlatformHandler`.

This document describes the public seams such a backend needs. Everything here is usable from a
package that only references the shipped MAUI NuGet packages — no `InternalsVisibleTo`, no
reflection, and no copied source. `src/BlazorWebView/tests/MauiBlazorWebView.ExternalHandler.UnitTests`
is an in-repo assembly that is deliberately *not* granted `InternalsVisibleTo` and exercises each of
these seams the way an external backend would.

## Registration

```csharp
builder.Services
    .AddMauiBlazorWebView()
    .UsePlatformHandler<MyPlatformBlazorWebViewHandler>();
```

Registration is last-registration-wins through the MAUI handler collection, so call
`UsePlatformHandler` *after* `AddMauiBlazorWebView()` and after any downstream library that calls
`AddMauiBlazorWebView()` again.

## Surfacing the native web view

`BlazorWebViewInitializedEventArgs` declares a strongly typed `WebView` property only for the target
frameworks that MAUI has a built-in backend for. A backend for any other platform sets the
platform-neutral `NativeWebView` property instead:

```csharp
VirtualView.BlazorWebViewInitializing(new BlazorWebViewInitializingEventArgs());
VirtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs
{
    NativeWebView = PlatformView,
});
```

On target frameworks where `WebView` exists, both properties are backed by the same value: assigning
`WebView` assigns `NativeWebView`, and `WebView` returns whatever `NativeWebView` holds — or `null`
if the stored value is not of the platform's web view type. Existing app code that reads
`e.WebView` on Android, iOS, MacCatalyst or Windows is unaffected.

## Root components

`RootComponent.AddToWebViewManagerAsync` and `RootComponent.RemoveFromWebViewManagerAsync` apply the
validation that MAUI's built-in handlers rely on (a `Selector` is required for both, and a
`ComponentType` is required to add). A backend should call them rather than reimplementing the
validation, so that error messages and ordering stay consistent across platforms:

```csharp
foreach (var rootComponent in VirtualView.RootComponents)
{
    // Before the page is attached this completes synchronously.
    _ = rootComponent.AddToWebViewManagerAsync(_webViewManager);
}
```

Call both methods on the `WebViewManager.Dispatcher` thread, and keep handling
`RootComponentsCollection.CollectionChanged` so components added or removed after startup are
applied to the manager.

## Static content hot reload

Static content hot reload (serving updated `wwwroot` assets, most notably CSS, without restarting
the app) is exposed through `BlazorWebViewStaticContentHotReload`. A backend participates in two
places, mirroring what the built-in handlers do.

Once, after creating the `WebViewManager` and before navigating:

```csharp
BlazorWebViewStaticContentHotReload.AttachToWebViewManagerIfEnabled(_webViewManager);
```

And while resolving each static content request, before the response is handed to the web view:

```csharp
BlazorWebViewStaticContentHotReload.TryReplaceResponseContent(
    _contentRootRelativeToAppRoot,
    requestAbsoluteUri,
    ref statusCode,
    ref content,
    headers);
```

Both members are no-ops when hot reload is unavailable — that is, when
`System.Reflection.Metadata.MetadataUpdater.IsSupported` is `false` — so they can be called
unconditionally. Static content hot reload is distinct from Razor component hot reload, which needs
no handler participation.

## Windows note

On Windows, a handler that owns a `WebView2` must either expose it directly as its `PlatformView`,
so the framework can close it when the window is destroyed, or close the wrapped control from its
own disconnect logic. This preserves the built-in workaround for microsoft-ui-xaml issue 6872.

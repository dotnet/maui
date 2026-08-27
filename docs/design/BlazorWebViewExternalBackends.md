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

## Dispatcher

A `WebViewManager` needs a Blazor `Dispatcher`. Rather than writing an adapter, construct MAUI's
public `MauiDispatcher` over the `IDispatcher` from the handler's services, so Blazor's thread
affinity matches the rest of MAUI on that platform:

```csharp
var dispatcher = new MauiDispatcher(Services!.GetRequiredService<IDispatcher>());
```

## Surfacing the native web view

`BlazorWebViewInitializedEventArgs` declares a strongly typed `WebView` property only for the target
frameworks that MAUI has a built-in backend for. A backend for any other platform supplies the
native control through the constructor instead:

```csharp
VirtualView.BlazorWebViewInitializing(new BlazorWebViewInitializingEventArgs());
VirtualView.BlazorWebViewInitialized(new BlazorWebViewInitializedEventArgs(PlatformView));
```

App code then reads it from the platform-neutral, read-only `PlatformWebView` property:

```csharp
blazorWebView.BlazorWebViewInitialized += (s, e) =>
{
    var native = (MyPlatformWebView)e.PlatformWebView!;
};
```

`PlatformWebView` is deliberately read-only and write-once: only the handler raising the event can
supply the value, so one event subscriber cannot change what later subscribers observe. On target
frameworks where the strongly typed `WebView` property exists, both properties report the same
instance, and `WebView` reports `null` if the stored value is not of that platform's web view type.
The property is scoped to the MAUI package; the WPF and WinForms BlazorWebView packages keep only
their existing strongly typed `WebView` property.

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
places, mirroring what the built-in handlers do — the built-in MAUI handlers call this same public
seam.

Once, after creating the `WebViewManager` and before navigating:

```csharp
_ = BlazorWebViewStaticContentHotReload.TryAttachToWebViewManager(_webViewManager);
```

`TryAttachToWebViewManager` returns `null` when hot reload is not supported by the current runtime
and nothing was attached, or a `Task` that completes when the notifier root component has been
registered. Awaiting it is optional when attaching before navigation, because registration completes
synchronously until a page is attached. Attaching is idempotent per `WebViewManager` instance:
repeat calls return the task from the first attach rather than failing on the notifier's fixed root
component selector.

On teardown, **if your handler disposes its `WebViewManager` — as all the built-in handlers do — there
is nothing to detach.** Disposing tears the notifier down along with the renderer, and a reconnected
handler builds a new manager, which attaches cleanly. Do not call detach and then immediately dispose:

```csharp
// DON'T: the detach is still in flight when disposal tears down the renderer underneath it.
_ = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(_webViewManager);
await _webViewManager.DisposeAsync();
```

`TryDetachFromWebViewManager` is for the narrower case of a handler that **keeps a manager alive** and
wants to stop, and possibly later restart, hot reload on it — detaching is what allows a later
`TryAttachToWebViewManager` on that same manager to take effect. Await it if you need the notifier to
be gone before you continue:

```csharp
var detach = BlazorWebViewStaticContentHotReload.TryDetachFromWebViewManager(_webViewManager);
if (detach is not null)
{
    await detach;
}
```

It returns `null` when nothing was attached, so it is safe to call unconditionally and is idempotent.
If the manager is disposed while a detach is still in flight, the returned task completes successfully
rather than faulting, so discarding it cannot produce an unobserved exception. Detaching is never
required to prevent a leak — the attachment is tracked weakly and the notifier unsubscribes when its
root component is disposed.

And while resolving each static content request:

```csharp
if (BlazorWebViewStaticContentHotReload.TryGetUpdatedStaticContent(
        _contentRootRelativeToAppRoot, requestAbsoluteUri, out var hotReloaded, out var hotReloadedContentType))
{
    originalContent?.Dispose();
    statusCode = 200;
    content = hotReloaded!;
    if (hotReloadedContentType is not null)
    {
        headers["Content-Type"] = hotReloadedContentType;
    }
}
```

This is a query, not a mutation: it reports content and lets the handler apply it to whatever
response state it owns. The returned stream is fresh per call and the caller is responsible for
disposing it, along with any content it had already resolved. The in-box Android, iOS, Tizen and
Windows web view managers apply hot-reloaded content through exactly this call, so the public seam
is the same code path MAUI itself ships on.

Both members are inert when hot reload is unavailable — that is, when
`System.Reflection.Metadata.MetadataUpdater.IsSupported` is `false` — so they can be called
unconditionally. Static content hot reload is distinct from Razor component hot reload, which needs
no handler participation.

## What is intentionally not public

The static content **response cache** and its policy helpers (`StaticContentResponseCache`,
`StaticContentResponseCachePolicy`, `StaticContentCacheControl`, `QueryStringHelper`) remain
internal. They implement the caching behavior that `BlazorWebView.StaticContentCacheControlProvider`
opts into, but the storage shape, eviction, entry-size limits and `Cache-Control`/`Pragma` parsing
are implementation details we want to keep free to change. An external backend is expected to
implement its own request caching, and to call
`IBlazorWebView.StaticContentCacheControlProvider` — which *is* public — to let apps influence the
`Cache-Control` header it emits. If you need the shared cache itself, please open an issue
describing the scenario rather than duplicating the internals.

## Windows note

On Windows, a handler that owns a `WebView2` must either expose it directly as its `PlatformView`,
so the framework can close it when the window is destroyed, or close the wrapped control from its
own disconnect logic. This preserves the built-in workaround for microsoft-ui-xaml issue 6872.

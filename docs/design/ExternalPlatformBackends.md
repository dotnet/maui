# External platform backends and handler contracts

This document describes the platform-neutral handler contracts added in .NET 11, why they exist, and
how an out-of-tree platform backend adopts them.

## The problem

Almost every per-control handler interface in `Microsoft.Maui.Core` declares `PlatformView` through a
compile-time type alias that changes per target framework. `ILabelHandler` is representative:

```csharp
#if __IOS__ || MACCATALYST
using PlatformView = Microsoft.Maui.Platform.MauiLabel;
#elif MONOANDROID
using PlatformView = AndroidX.AppCompat.Widget.AppCompatTextView;
#elif WINDOWS
using PlatformView = Microsoft.UI.Xaml.Controls.TextBlock;
#elif TIZEN
using PlatformView = Tizen.UIExtensions.NUI.Label;
#elif (NETSTANDARD || !PLATFORM) || (NET6_0_OR_GREATER && !IOS && !TIZEN)
using PlatformView = System.Object;
#endif

public partial interface ILabelHandler : IViewHandler
{
    new ILabel VirtualView { get; }
    new PlatformView PlatformView { get; }
}
```

The alias is baked into the compiled contract. That means the interface can only ever be implemented by
a handler whose native view type is *exactly* the type .NET MAUI picked for that target framework.

An external backend — a platform implementation that ships outside dotnet/maui, such as a standalone
Tizen backend — owns its own native types, so it cannot satisfy the contract:

| Target framework the backend consumes | Compiled `PlatformView` type | Error |
| --- | --- | --- |
| A platform TFM MAUI ships (`-android`, `-ios`, `-windows`, historically `-tizen`) | MAUI's own native type | `CS0738` — the implementing member's return type does not match |
| The platform-neutral TFM (`net11.0`) | `System.Object` (non-nullable) | `CS9333` — "type must be `object` to match implemented member", plus `CS8766` because `ViewHandler.PlatformView` is `object?` |

Both failures were reproduced against the shipped
`Microsoft.Maui.Core 11.0.0-preview.7.26406.9` package with an ordinary `net11.0` class library:

```csharp
public class MyNativeLabel { }

public class MyLabelHandler : ViewHandler<ILabel, MyNativeLabel>, ILabelHandler
{
    public MyLabelHandler() : base(LabelHandler.Mapper) { }
    protected override MyNativeLabel CreatePlatformView() => new();
    ILabel ILabelHandler.VirtualView => VirtualView;
    MyNativeLabel ILabelHandler.PlatformView => PlatformView;   // CS9333
}
```

```text
error CS9333: 'MyLabelHandler.PlatformView': type must be 'object' to match implemented member 'ILabelHandler.PlatformView'
error CS8766: Nullability of reference types in return type of 'object? ViewHandler.PlatformView.get'
              doesn't match implicitly implemented member 'object ILabelHandler.PlatformView.get'
```

`CS8766` is a warning, but external backends that build with `TreatWarningsAsErrors` (the .NET MAUI
default under `ContinuousIntegrationBuild`) see it as an error.

46 handler interfaces in `Microsoft.Maui.Core` use the alias pattern. `ILabelHandler`,
`IContentViewHandler`, `ILayoutHandler` and `IWindowHandler` are the ones a minimal backend hits first,
because they are needed for the application → window → page → content → layout → label vertical slice.

`ILayoutHandler` is the most damaging of the four, because it is the only one of them that carries real
behavior — `Add`, `Remove`, `Clear`, `Insert`, `Update` and `UpdateZIndex` — rather than just typed
accessors.

## The solution

Three new public interfaces. Nothing was moved, renamed, or removed, so the change is source and
binary compatible.

### `IElementHandler<out TVirtualView, out TPlatformView>`

```csharp
namespace Microsoft.Maui;

public interface IElementHandler<out TVirtualView, out TPlatformView> : IElementHandler
    where TVirtualView : IElement
    where TPlatformView : class
{
    new TVirtualView VirtualView { get; }
    new TPlatformView PlatformView { get; }
}
```

### `IViewHandler<out TVirtualView, out TPlatformView>`

```csharp
namespace Microsoft.Maui;

public interface IViewHandler<out TVirtualView, out TPlatformView>
    : IElementHandler<TVirtualView, TPlatformView>, IViewHandler
    where TVirtualView : IView
    where TPlatformView : class
{
    new TVirtualView VirtualView { get; }
}
```

### `ILayoutHandler<out TPlatformView>`

```csharp
namespace Microsoft.Maui;

public interface ILayoutHandler<out TPlatformView> : IViewHandler<ILayout, TPlatformView>
    where TPlatformView : class
{
    void Add(IView view);
    void Remove(IView view);
    void Clear();
    void Insert(int index, IView view);
    void Update(int index, IView view);
    void UpdateZIndex(IView view);
}
```

### Why this is the smallest change that covers everything

`ElementHandler<TVirtualView, TPlatformView>` and `ViewHandler<TVirtualView, TPlatformView>` now
implement the matching new interface. Their existing public `VirtualView` and `PlatformView` members
already have exactly the right signatures, so **no handler anywhere needed a new member** — in the box
or out of it. Every one of the 46 aliased handlers, and every third-party handler derived from
`ViewHandler<,>`, satisfies the new contracts automatically.

`TPlatformView` is covariant, so `object` acts as the platform-neutral wildcard:

```csharp
// Matches the in-box LabelHandler on every platform AND any external backend's label handler.
if (label.Handler is IViewHandler<ILabel, object> labelHandler)
{
    ILabel virtualView = labelHandler.VirtualView;
    object nativeLabel = labelHandler.PlatformView;
}
```

`ILayoutHandler<TPlatformView>` is intentionally **not** related to the existing `ILayoutHandler` by
inheritance. Relating them would force `ILayoutHandler` to hide the inherited members, which would break
any type that implements `Add`/`Remove`/... as *explicit* interface implementations. Keeping them
unrelated means a handler that implements those members as ordinary public methods — as the in-box
`LayoutHandler` does — satisfies both interfaces at once. `LayoutHandler` therefore declares both.

### What deliberately did not change

* No member was moved off `ILabelHandler`, `IContentViewHandler`, `ILayoutHandler` or `IWindowHandler`.
* No aliased interface gained or lost a base interface.
* Property mappers, command mappers, handler registration and `ToHandler`/`ToPlatform` are untouched.
* Existing casts such as `Handler as ILayoutHandler` in the Controls compatibility layer keep working
  against in-box handlers exactly as before.

### Consuming the contracts

Because the neutral contracts come from `ElementHandler<,>`/`ViewHandler<,>`, any handler derived from
those bases — including handlers compiled against an older .NET MAUI — satisfies
`IElementHandler<TVirtualView, object>` / `IViewHandler<TVirtualView, object>` automatically.

`ILayoutHandler<TPlatformView>` is the exception: it declares members, so a type only satisfies it if it
lists the interface. In-box `LayoutHandler` does. A pre-existing third-party type that implements
`ILayoutHandler` with *explicit* interface implementations does not. Framework and consumer code that
needs to cover both should test for both:

```csharp
if (handler is ILayoutHandler<object> neutral)
{
    neutral.Add(view);
}
else if (handler is ILayoutHandler legacy)
{
    legacy.Add(view);
}
```

### How Controls reaches a layout handler

`Microsoft.Maui.Controls.Layout` does **not** cast to either layout interface. It raises the
child-management operations as **command mapper keys**:

```csharp
// src/Controls/src/Core/Layout/Layout.cs
Handler?.Invoke(nameof(ILayoutHandler.Add), new LayoutHandlerUpdate(index, view));
Handler?.Invoke(nameof(ILayoutHandler.Clear));
```

That dispatch is type-agnostic, so it reaches an external backend's handler exactly as it reaches the
in-box one. Referencing `ILayoutHandler` for its member *names* via `nameof` is always allowed — only
*implementing* it is blocked — so an external backend should register those same key strings in its
command mapper, and Controls interop keeps working unchanged. This is what
`ControlsLayoutCommandsReachExternalLayoutHandler` verifies end to end.

**One known gap.** The obsolete `Microsoft.Maui.Controls.Compatibility.Layout<T>.LayoutHandler` property
is declared as `ILayoutHandler` and evaluates `Handler as ILayoutHandler`, so it returns `null` for a
handler that implements only `ILayoutHandler<TPlatformView>`:

```csharp
// src/Controls/src/Core/LegacyLayouts/Layout.cs:35
public ILayoutHandler LayoutHandler => Handler as ILayoutHandler;
```

This is the **only** remaining runtime cast to `ILayoutHandler` in .NET MAUI. It is a public convenience
property that no in-box code path reads, and its declaring type carries `[Obsolete]`, so it does not
affect layout behavior for an external backend. It is documented here because for that one legacy
property, `ILayoutHandler<TPlatformView>` is a compile-time contract only. Converting it was deliberately
left out of scope: changing a shipped public property's semantics is a larger, separable decision.

Binary compatibility was verified by compiling handlers against
`Microsoft.Maui.Core 11.0.0-preview.7.26406.9` and loading them against a build that includes this
change: derived handlers, explicit-interface-implementation handlers, and all existing `is`/cast
relationships continue to work with no `TypeLoadException`.

### Handler lifecycle and `PlatformView` nullability

`IElementHandler.PlatformView` is `object?`, but
`IElementHandler<TVirtualView, TPlatformView>.PlatformView` is non-nullable `TPlatformView`, matching the
existing non-nullable `ElementHandler<,>.PlatformView` / `ViewHandler<,>.PlatformView` properties it is
satisfied by. Those properties **throw** rather than return `null` when the handler is not connected:

```csharp
public new TPlatformView PlatformView
{
    get => (TPlatformView?)base.PlatformView
        ?? throw new InvalidOperationException($"PlatformView cannot be null here");
    ...
}
```

That is pre-existing behavior and is deliberately not changed here, but it has a consequence worth
knowing when consuming the typed contracts: a handler is only guaranteed to have a `PlatformView`
between `SetVirtualView` and `DisconnectHandler`. `DisconnectHandler` clears `PlatformView` before
invoking the disconnect callback, so reading `PlatformView` through a typed contract after disconnect
throws.

For code that may run outside a connected window — teardown paths, weak-reference caches, diagnostics —
prefer the nullable base member, which never throws:

```csharp
// Safe at any point in the lifecycle.
if (handler.PlatformView is FakeNativeLabel nativeLabel) { /* ... */ }

// Only safe while the handler is connected.
FakeNativeLabel nativeLabel = typedHandler.PlatformView;
```

Property and command mappers always run while the handler is connected, so mapper code can use the
typed member freely — which is what the in-box handlers and the tests here do.

## Adopting this from an external backend

An external backend derives from the public generic handler base classes and stops trying to implement
the aliased interfaces.

> **Note on the native type.** On a platform target framework,
> `ViewHandler<TVirtualView, TPlatformView>` constrains `TPlatformView` to that platform's base view
> type (`UIKit.UIView`, `Android.Views.View`, `Microsoft.UI.Xaml.FrameworkElement`,
> `Tizen.NUI.BaseComponents.View`). An external backend's views already satisfy that. The constraint was
> never the problem — the *aliased interfaces* were, because they pin `PlatformView` to one exact
> concrete type. `ElementHandler<TVirtualView, TPlatformView>` constrains `TPlatformView` only to
> `class`, on every target framework.

```csharp
using Microsoft.Maui;
using Microsoft.Maui.Handlers;

// The backend's own native type. MAUI knows nothing about it.
public class MyNativeLabel
{
    public string? Text { get; set; }
}

public class MyLabelHandler : ViewHandler<ILabel, MyNativeLabel>
{
    public static IPropertyMapper<ILabel, MyLabelHandler> Mapper =
        new PropertyMapper<ILabel, MyLabelHandler>(ViewMapper)
        {
            [nameof(ILabel.Text)] = MapText,
        };

    public MyLabelHandler() : base(Mapper) { }

    protected override MyNativeLabel CreatePlatformView() => new MyNativeLabel();

    public static void MapText(MyLabelHandler handler, ILabel label) =>
        handler.PlatformView.Text = label.Text;
}

// No extra members required — this already holds:
IViewHandler<ILabel, MyNativeLabel> typed = new MyLabelHandler();
IViewHandler<ILabel, object> neutral = typed;
```

A layout handler additionally declares `ILayoutHandler<TPlatformView>` so that consumers can drive child
management without knowing the backend:

```csharp
public class MyLayoutHandler : ViewHandler<ILayout, MyNativeLayout>, ILayoutHandler<MyNativeLayout>
{
    public static CommandMapper<ILayout, MyLayoutHandler> CommandMapper =
        new CommandMapper<ILayout, MyLayoutHandler>(ViewCommandMapper)
        {
            // Referencing ILayoutHandler for its member *names* is always allowed;
            // only implementing it is blocked. These are the keys Controls' Layout raises.
            [nameof(ILayoutHandler.Add)] = MapAdd,
            [nameof(ILayoutHandler.Remove)] = MapRemove,
            [nameof(ILayoutHandler.Clear)] = MapClear,
            [nameof(ILayoutHandler.Insert)] = MapInsert,
            [nameof(ILayoutHandler.Update)] = MapUpdate,
            [nameof(ILayoutHandler.UpdateZIndex)] = MapUpdateZIndex,
        };

    public void Add(IView view) { /* ... */ }
    public void Remove(IView view) { /* ... */ }
    public void Clear() { /* ... */ }
    public void Insert(int index, IView view) { /* ... */ }
    public void Update(int index, IView view) { /* ... */ }
    public void UpdateZIndex(IView view) { /* ... */ }

    // ...
}
```

Handler registration is unchanged:

```csharp
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler<Label, MyLabelHandler>();
    handlers.AddHandler<VerticalStackLayout, MyLayoutHandler>();
});
```

## Tests

`src/Core/tests/ExternalBackend` is a separate assembly
(`Microsoft.Maui.Core.ExternalBackend.TestSupport`) that is deliberately **not** in Core's
`InternalsVisibleTo` list and builds with `TreatWarningsAsErrors`. It contains fake native view types and
handlers for label, content view, layout and window, so everything it compiles is provably public API and
provably warning-free for an external consumer.

It is **shared with the container-view work** from
[#37854](https://github.com/dotnet/maui/pull/37854), which introduced the same assembly to prove
`ViewHandler.SetContainerView` is reachable from a non-friend assembly. Both efforts need exactly the
same thing — an assembly outside Core's friend list that models an external backend — so they use one
project rather than two. Those sources (`ExternalBackendViewHandler.cs`, `ExternalPlatformViews.cs`) use
platform-agnostic native types, which satisfy `ViewHandler<,>`'s `TPlatformView` constraint only where
that constraint is `class`, so they are scoped to the platform-neutral and netstandard target frameworks
— precisely the set they were compiled against before this project became multi-targeted.

It **mirrors `Microsoft.Maui.Core`'s own `TargetFrameworks` exactly** —
`netstandard2.1;netstandard2.0;$(_MauiDotNetTfm);$(MauiPlatforms)` — because the failure it guards
against is target-framework specific: `CS9333`/`CS8766` on the platform-neutral and netstandard TFMs,
`CS0738` on the platform TFMs. A target framework Core ships but the guard project does not compile
would be an untested target framework, and the gap would be silent.

Both projects expand the same `$(MauiPlatforms)`, so the platform set — **including whether Tizen is
enabled** — cannot drift between them. Tizen is currently absent from real builds only because
`IncludeTizenTargetFrameworks` is off repo-wide, **not** because the guard project lacks sources for it:
`FakeNativeViews.Tizen.cs` exists and derives from `Tizen.NUI.BaseComponents.View`, so the matrix builds
the moment the workload is re-enabled. `ExternalBackendTargetFrameworkTests` enforces all of this — it
compares the two `TargetFrameworks` expressions directly and asserts a fake-native-view source exists for
every platform.

The fake native view types are per-platform, because
`ViewHandler<TVirtualView, TPlatformView>` constrains `TPlatformView` differently per TFM:

| Target framework | `ViewHandler<,>` constraint on `TPlatformView` | Fake native view derives from |
| --- | --- | --- |
| `netstandard2.0`, `netstandard2.1`, `net11.0` | `class` | *(nothing)* |
| `net11.0-android` | `Android.Views.View` | `Android.Views.View` |
| `net11.0-ios`, `net11.0-maccatalyst` | `UIKit.UIView` | `UIKit.UIView` |
| `net11.0-windows` | `Microsoft.UI.Xaml.FrameworkElement` | `Microsoft.UI.Xaml.Controls.Panel` |
| `net11.0-tizen` *(gated off)* | `Tizen.NUI.BaseComponents.View` | `Tizen.NUI.BaseComponents.View` |

This mirrors reality: a real external backend's views *do* derive from the platform's base view type.
What makes it external is that its **concrete** types are its own, and so are never the types the aliased
interfaces are pinned to (`MauiLabel`, `AppCompatTextView`, `TextBlock`,
`Tizen.UIExtensions.NUI.Label`) — which is exactly what produces `CS0738`. Note the Tizen fake derives
from the platform SDK's own `Tizen.NUI.BaseComponents.View` rather than from
`Tizen.UIExtensions.NUI.ViewGroup`, which is what .NET MAUI's in-box Tizen views use: an external backend
depends on the platform SDK, not on .NET MAUI's platform helper packages.

`ElementHandler<,>` constrains `TPlatformView` only to `class` on every TFM, so the fake native
*window* type is a single definition shared across all of them.

Fake members are prefixed `Fake` (`FakeText`, `FakeOpacity`, `FakeChildren`) so the same handler sources
compile on every TFM without colliding with a real platform base type's members — for example
`FrameworkElement.Opacity` on Windows.

`ExternalBackendTfmContracts.cs` compiles unchanged on every TFM and asserts, at compile time, that the
backend's own native type survives a round trip through the typed and neutral contracts, that the layout
contract's behavior is callable, and that .NET MAUI's in-box handlers satisfy the same neutral contracts.

`src/Core/tests/UnitTests/Handlers/ExternalPlatformBackendTests.cs` exercises runtime behavior on the
neutral TFM: contract satisfaction, covariance, handler registration, property-mapper composition off
`ViewHandler.ViewMapper`, Controls' `Layout` command routing reaching an external layout handler, and
backward-compatibility assertions that the aliased interfaces are unchanged.

## Follow-up coverage

The generic contracts cover every handler automatically, so no per-control follow-up is required for
recognition or typed access. Two areas remain for a fully self-contained external backend and are
tracked separately:

1. **Behavior-carrying handler interfaces.** `ILayoutHandler` is the only one of the four required
   interfaces with behavior, and it now has a neutral counterpart. Other interfaces that carry behavior
   rather than just accessors (for example `IScrollViewHandler`) can get the same treatment on demand,
   following the `ILayoutHandler<TPlatformView>` pattern.
2. **Handler infrastructure that is still `internal` or TFM-locked.** `ViewHandler.ContainerView`'s
   setter, `MauiContext.AddSpecific`, `LifecycleEventServiceExtensions.InvokeLifecycleEvents`,
   `Microsoft.Maui.Handlers.LayoutExtensions` and `HandlerNotFoundException` are not reachable from an
   external assembly. Those are separate from the interface-shape problem this document addresses.

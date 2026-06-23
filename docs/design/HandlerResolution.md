Handler Resolution
===

# Introduction

Handlers are the platform components used to render a cross-platform `View` on the screen. Each view type is associated with a handler that knows how to create and manage the corresponding platform-native control.

## Declaring a Handler with `[ElementHandler]`

Most built-in .NET MAUI views declare their handler using the `[ElementHandler]` attribute directly on the view class:

```csharp
[ElementHandler(typeof(ButtonHandler))]
public partial class Button : View, IButton { ... }
```

This is the primary mechanism for associating views with handlers. It is trimmer-safe and AOT-friendly because the handler type is statically referenced.

The attribute is declared with `Inherited = false`, so each view type must explicitly declare it. However, `MauiHandlersFactory` walks the type's base class hierarchy (`Type.BaseType`) when looking for the attribute, so a base class attribute acts as a fallback for derived types that don't declare their own.

### Guidance for library authors

`ElementHandlerAttribute` is currently an internal framework optimization used by .NET MAUI controls. Third-party libraries should continue to register handlers with `ConfigureMauiHandlers(... AddHandler ...)`:

```csharp
builder.ConfigureMauiHandlers(handlers =>
{
	handlers.AddHandler<MyControl, MyControlHandler>();
});
```

If an attribute-based API is made public for third-party libraries, prefer `[ElementHandler(typeof(THandler))]` for concrete controls when the handler type can be selected statically and the handler has a public parameterless constructor. Use a custom `ElementHandlerAttribute` subclass only when handler selection needs additional logic. The Android Material UI controls are the intended pattern: their nested attributes override `GetHandlerType()` and return either the Material 3 handler or the default handler based on `RuntimeFeature.IsMaterial3Enabled`, so the inactive handler implementation does not need to be rooted unconditionally.

Continue to use `ConfigureMauiHandlers(... AddHandler ...)` when an application needs to override an existing handler, when registering handlers for interfaces, or when a handler must be constructed through DI because it requires constructor parameters.

## Registering a Handler in Code

```csharp
builder.ConfigureMauiHandlers(handlers =>
       {
          handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
       }
```

DI registration should only be used to override an existing `[ElementHandler]` declaration or when the element type is an interface (e.g., `IScrollView`). DI-registered handlers take priority over `[ElementHandler]` attributes when the registered type is assignable from the requested element type.

## Resolution Order

Both `MauiHandlersFactory.GetHandler(Type)` and `MauiHandlersFactory.GetHandlerType(Type)` follow the same resolution order:

1. **Exact DI registration** — checks if a handler was registered for this exact type via `AddHandler`
2. **Assignable DI registration** — uses `RegisteredHandlerServiceTypeSet` to find the best matching concrete or interface registration (e.g., a handler registered for `Button` matches a derived `FancyButton`, and a handler registered for `IScrollView` matches a `ScrollView` instance)
3. **`[ElementHandler]` attribute** — walks the type's base class hierarchy looking for the attribute; lookup results are cached per requested view type
4. **`IContentView` fallback** — returns `ContentViewHandler` for any `IContentView` implementation
5. **`GetHandlerType` returns `null`** / **`GetHandler` throws `HandlerNotFoundException`** — if none of the above matched

`GetHandler(Type)` is the primary API for creating a handler instance. `GetHandlerType(Type)` returns the handler `Type` without instantiating it and is used by code paths that need to compare handler types or create handlers through DI fallback. If a DI registration was made with an implementation factory instead of an implementation type, `GetHandlerType(Type)` returns `null` because there is no registered handler type to report; it does not fall through to an inherited `[ElementHandler]` default for that view.

### Handler Instantiation

How a handler instance is created depends on how it was resolved:

- **DI-registered handlers** (steps 1 & 2): Instantiated through `MauiFactory.GetService()`, which uses `Activator.CreateInstance` on the registered `ImplementationType`, or invokes the `ImplementationFactory` delegate if one was provided. `GetHandlerType(Type)` can only report DI registrations that provide an `ImplementationType`.
- **`[ElementHandler]` attribute** (step 3): Instantiated directly via `Activator.CreateInstance` — no DI involvement.
- **Fallback in `ElementExtensions.ToHandler()`**: When `Activator.CreateInstance` fails with a `MissingMethodException` (e.g., the handler requires constructor parameters), `ActivatorUtilities.CreateInstance` is used instead, which supports constructor injection from the DI container.

> **Note:** Handlers registered via `[ElementHandler]` must have a public parameterless constructor.
> They are instantiated with `Activator.CreateInstance()`, not through the DI container.
> The `ActivatorUtilities.CreateInstance()` fallback only applies to DI-registered handlers
> resolved through `ElementExtensions.ToHandler()`.

### Controls mapper remapping

Controls-specific mapper remaps are independent of handler resolution. `ElementHandler.SetVirtualView()` calls the internal Controls remap hook on the virtual view before mapper updates, so remaps run regardless of whether the handler came from exact DI registration, assignable DI registration, `[ElementHandler]`, or the `IContentView` fallback.

Each remappable Controls type owns its own one-time gate and calls `base.RemapForControls()` before applying its mapper changes. Non-mapper command dependency setup, such as `CommandProperty.DependsOn(...)`, remains in the relevant control type initialization so binding behavior is available before a handler is attached.

Because these remaps now run lazily on the first handler attach, startup code that customizes the same built-in mapper keys before any instance is attached can still be replaced by the first Controls remap. Custom mapper keys and new mappings are unaffected. If an app or library needs to customize a built-in key that Controls remaps with `ReplaceMapping`, apply that customization after the first attach for that control type or reapply it after the Controls remap has run.

## Types used in the resolution of Handlers to Views

### `MauiFactory`

```csharp
public interface IMauiFactory : IServiceProvider
```

```csharp
public class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
```

[`MauiFactory`](https://github.com/dotnet/maui/blob/main/src/Core/src/Hosting/Internal/MauiFactory.cs) started out as our home grown Dependency Resolver. Prior to going all in on the `Ms.Ext.ServiceProvider` we were using MauiFactory. The initial intent behind  `MauiFactory` was primarily performance based. We were concerned that the `Ms.Ext.ServiceProvider` would cause startup performance issues so we created a very simple resolver. We do not have any metrics that prove/disprove the performance of `MauiFactory` vs `MS.Ext.ServiceProvider`. At this point the `MauiFactory` does fulfill some needs that don't seem to quite fit inside the `IServiceProvider` box. 

#### Features of the `MauiFactory`
- Handler registration can theoretically happen at any time because it happens via the `IMauiHandlerFactory`. All you have to do is grab the `IMauiHandlersCollection` from `IMauiHandlerFactory` and add an additional Handler. This isn't a common use case but we do make use of this feature [here](https://github.com/dotnet/maui/blob/main/src/Controls/samples/Controls.Sample/Controls/BordelessEntry/BordelessEntryServiceBuilder.cs#L41).
- `MauiFactory` allows you to register two types that have no relationship with each other. `Button` and `ButtonHandler` don't implement any of the same interfaces, `Button` isn't the "Concrete Implementation" of `ButtonHandler`.
- `MauiFactory` has support for `ctor` resolution but we currently have it disabled in all cases.
  - Handlers will currently attempt to instantiate through [Extensions.DependencyInjection.ActivatorUtilities.CreateInstance](https://github.com/dotnet/maui/blob/cc53f0979baf5d6bb8a5d6bf84b64f3cf591c56f/src/Core/src/Platform/ElementExtensions.cs#L34 ) if a default constructor hasn't been created. So the ctor resolution feature of `MauiFactory` probably doesn't have any currently useful purpose.
- `MauiFactory` currently doesn't support Scoped Services which is the main reason why we switched to `Ms.Ext.DI` for our main implementation. .NET MAUI Blazor requires Scoped Services and we've started using Scoped Services as well for multi-window.
- `MauiFactory` retrieves the handler type registered for the requested type. Interface-based registration matching is now handled by `RegisteredHandlerServiceTypeSet`, which finds the most specific matching interface to avoid ambiguity (the old behavior of matching any `IView`-implementing interface has been fixed — see https://github.com/dotnet/maui/issues/1298).

### IMauiHandlersFactory

`IMauiHandlersFactory` is the interface used to request handlers for a `View`

```csharp
public class MauiHandlersFactory : MauiFactory, IMauiHandlersFactory
```

`builder.Services.AddSingleton<IMauiHandlersFactory, MauiHandlersFactory>`


```csharp
public interface IMauiHandlersFactory : IMauiFactory
{
    Type? GetHandlerType(Type iview);
    IElementHandler? GetHandler(Type type);
    IElementHandler? GetHandler<T>() where T : IElement;
    IMauiHandlersCollection GetCollection();
}
```

Access to the HandlerFactory is provided through the `IMauiContext`

```csharp
public interface IMauiContext
{
    IServiceProvider Services { get; }

    IMauiHandlersFactory Handlers { get; }
}
```

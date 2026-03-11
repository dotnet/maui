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

## Registering a Handler in Code

```csharp
builder.ConfigureMauiHandlers(handlers =>
       {
          handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
       }
```

DI registration should only be used to override an existing `[ElementHandler]` declaration or when the element type is an interface (e.g., `IScrollView`). DI-registered handlers take priority over `[ElementHandler]` attributes when registered for the exact same type.

## Resolution Order

Both `MauiHandlersFactory.GetHandler(Type)` and `MauiHandlersFactory.GetHandlerType(Type)` follow the same resolution order:

1. **Exact DI registration** — checks if a handler was registered for this exact type via `AddHandler`
2. **`[ElementHandler]` attribute** — walks the type's base class hierarchy looking for the attribute
3. **Interface-based DI registration** — uses `RegisteredHandlerServiceTypeSet` to find the best matching interface registration (e.g., a handler registered for `IScrollView` matches a `ScrollView` instance)
4. **`IContentView` fallback** — returns `ContentViewHandler` for any `IContentView` implementation
5. **`GetHandlerType` returns `null`** / **`GetHandler` throws `HandlerNotFoundException`** — if none of the above matched

### Handler Instantiation

How a handler instance is created depends on how it was resolved:

- **DI-registered handlers** (steps 1 & 3): Instantiated through `MauiFactory.GetService()`, which uses `Activator.CreateInstance` on the registered `ImplementationType`, or invokes the `ImplementationFactory` delegate if one was provided.
- **`[ElementHandler]` attribute** (step 2): Instantiated directly via `Activator.CreateInstance` — no DI involvement.
- **Fallback in `ElementExtensions.ToHandler()`**: When `Activator.CreateInstance` fails with a `MissingMethodException` (e.g., the handler requires constructor parameters), `ActivatorUtilities.CreateInstance` is used instead, which supports constructor injection from the DI container.

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

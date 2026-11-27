Handler Resolution
===

# Introduction

Handlers are the platform components used to render a cross platform `View` on the screen. Every platform registers a handler against a .NET Maui type.  

## Registering a Handler in Code

```csharp
builder.ConfigureMauiHandlers(handlers =>
       {
          handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
       }
```

## Handler Instantiation

How a handler instance is created depends on how it was resolved:

- **DI-registered handlers**: Instantiated through `MauiFactory.GetService()`, which uses `Activator.CreateInstance` on the registered `ImplementationType`, or invokes the `ImplementationFactory` delegate if one was provided. Factory delegates receive a service provider for the current `IMauiContext`, so they can resolve that context (and, on Android, its `Context`).
- **`[ElementHandler]` attribute**: The internal attribute requires a public parameterless constructor and creates the handler directly — no DI involvement.
- **Constructor injection**: `ElementExtensions.ToHandler()` delegates activation to the internal `MauiHandlersFactory.GetHandlerWithInjection()` path. The built-in factory checks whether the resolved handler type has a public parameterless constructor. If it does, `Activator.CreateInstance` is used; otherwise, `ActivatorUtilities.CreateInstance` supplies constructor arguments from the current context's DI container (including its Android `Context`) without first throwing a missing-constructor exception. Value-type handlers retain their default construction through `Activator.CreateInstance`. A failure to resolve a dependency propagates without retrying the activation. Custom `IMauiHandlersFactory` implementations retain the exception-based fallback for `MissingMethodException`, including when wrapped in `HandlerNotFoundException`.

Constructor selection is checked on each activation, so no process-wide activation-strategy cache or separate invalidation is needed. Handler resolution still runs first on every request, and factory delegates are never bypassed. Each activation resolves dependencies from the current context, and separately built `MauiApp` instances do not affect one another. Handler types created through factory delegates and constructor injection are recorded when activated, so Hot Reload handler replacement applies to them.

> **Note:** Direct `GetHandler()` callers keep the parameterless-constructor behavior: a registered handler type
> without a public parameterless constructor throws `MissingMethodException`. Declaring handlers with a
> parameterless constructor (and registering an implementation factory with
> `AddHandler<TType>(Func<IServiceProvider, IElementHandler>)` when constructor arguments are required)
> remains the recommended approach.

## Controls mapper remapping

Controls-specific mapper remaps are independent of handler resolution and run lazily, the first time an instance of each remappable Controls type is attached to a handler. The Controls `Element` handler setter calls the internal remap hook before attaching any handler, including compatibility renderers that do not derive from `ElementHandler`. `ElementHandler.SetVirtualView()` also calls the hook, through the internal `IControlsMapperRemappable` interface, for handlers attached directly outside the Controls path and for remappable types that are not elements, such as `Toolbar`. Both run before the element's properties are first mapped. Remaps therefore run regardless of how the handler was registered or created, and `UseMauiApp` no longer applies them at startup.

Each remappable Controls type overrides the internal `RemapForControls()` method, calls `base.RemapForControls()` first, and then applies its own mapper changes through a blocking, one-time `OneTimeInitializationAction`. Attaching a `Label`, for example, applies the `Element`, `VisualElement` and `Label` remaps in that order, but not the remaps of unrelated control types. Concurrent callers wait for a remap that is in progress to finish, and unrelated control types do not contend on a global lock. Non-mapper command dependency setup, such as `CommandProperty.DependsOn(...)`, remains in the relevant control type initialization so binding behavior is available before a handler is attached. Remaps that install a platform view factory (`TabbedViewHandler.PlatformViewFactory`, and `RadioButtonHandler.PlatformViewFactory` on Android) keep a factory the app has already installed instead of replacing it.

Controls property and command mapper changes update the framework mapping underneath application customizations. The built-in Core handler mappers that Controls remaps seal their framework mappings after construction, so every later indexer assignment is recorded as an application customization, whether it replaces a local key, a key supplied by a chained parent mapper, or a key no mapper defines yet. `SwitchHandler.Mapper` and the `ButtonHandler.TextButtonMapper` and `ButtonHandler.ImageButtonMapper` parents are not sealed; on those, only replacing an existing key is recorded, and assigning a key they do not define yet stores it directly. Direct indexer replacements and `AppendToMapping`, `PrependToMapping`, `ModifyMapping`, and `ReplaceMapping` calls are recorded in order and recomposed whenever a Controls remap changes the framework mapping, so on sealed mappers they behave the same whether they are registered before or after the first control instance is attached. `Label` still applies its Android `IsTextSelectable` and Apple `FlowDirection` additions with the public `AppendToMapping`, so they are recorded as application customizations when the first `Label` is attached and keep their timing-dependent order relative to app customizations of those keys. Mappings read through `GetProperty`, `GetCommand`, or the indexers before a lazy remap still observe that remap: sealed built-in mappings are held in cells that framework remaps update in place, and a mapping read for a customized key resolves on invocation to the composition as of the read. It includes later framework remaps but not later application customizations, so wrapping it in a replacement (`var previous = mapper[key]; mapper[key] = ...`) cannot invoke itself.

Customizations recorded on a child mapper for a key it inherits from a chained parent keep the previous capture semantics for application changes: the child observes the parent's application customizations recorded before its own first customization of that key, but not later ones. Framework mappings and remaps of the parent, including keys the parent only gains during a lazy remap, are always observed. A framework remap of a key on the child, including a key it inherits, places it before every application customization, so the child then builds on the parent's framework mapping only and no longer observes application customizations of that key on the parent, even ones registered before the first control instance is attached.

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
- `MauiFactory` creates type-based registrations through a virtual activation hook. `MauiHandlersFactory` overrides that hook for context-aware activation through `ToHandler()`, selecting constructor injection when no public parameterless constructor exists. Public `GetHandler()` calls continue to require a parameterless constructor for type-based registrations.
- `MauiFactory` currently doesn't support Scoped Services which is the main reason why we switched to `Ms.Ext.DI` for our main implementation. .NET MAUI Blazor requires Scoped Services and we've started using Scoped Services as well for multi-window.
- `MauiFactory` retrieves all base types from the requested type and all implemented interfaces. It first iterates over base types and then if nothing is found it loops through the interfaces. The interface behavior currently leads to some odd behavior because everything implements `IView`. This means that if a handler isn't registered then `MauiFactory` just returns a random handler because technically every single handler is registered against a cross platform view that implements`IView`. https://github.com/dotnet/maui/issues/1298
  - We should probably remove the interface matching part of `MauiFactory` 

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

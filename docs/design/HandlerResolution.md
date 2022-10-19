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

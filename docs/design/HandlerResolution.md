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

If an attribute-based API is made public for third-party libraries, prefer `[ElementHandler(typeof(THandler))]` for concrete controls when the handler type can be selected statically and the handler has a public parameterless constructor. Use a custom `ElementHandlerAttribute` subclass only when handler selection needs additional logic. The Android Material UI controls are the intended pattern: their nested attributes override `GetHandlerType()` and return either the Material 3 handler or the default handler based on `RuntimeFeature.IsMaterial3Enabled`, so the inactive handler implementation does not need to be rooted unconditionally. `CollectionView` uses the same technique on Windows, selecting `CollectionViewHandler2` or the legacy `CollectionViewHandler` based on `RuntimeFeature.IsWindowsCollectionView2HandlerEnabled`.

Continue to use `ConfigureMauiHandlers(... AddHandler ...)` when an application needs to override an existing handler, when registering handlers for interfaces, or when a handler must be constructed through DI because it requires constructor parameters.

## Registering a Handler in Code

```csharp
builder.ConfigureMauiHandlers(handlers =>
       {
          handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
       }
```

DI registration should only be used to override an existing `[ElementHandler]` declaration or when the element type is an interface (e.g., `IScrollView`). An exact `AddHandler` registration always overrides an attribute-based default. An assignable registration applies to derived types that do not declare their own handler, preserving the behavior of the previous built-in DI registrations.

`TryAddHandler` has different override semantics. In an app using the Controls defaults, it does not replace a handler declared directly on the same type with `[ElementHandler]`, nor the `IContentView` and `IContentButton` defaults. It can still register a handler for a custom derived type that only inherits an attribute fallback. Apps that do not enable the Controls defaults can use `TryAddHandler` normally. This policy is stored with each app's handler collection rather than in process-wide state, so separately built `MauiApp` instances do not affect one another.

`UseMauiApp()` enables this policy before the handler-registration callbacks execute, regardless of where those callbacks were added in the builder chain. `AddMauiControlsHandlers()` is retained as a non-obsolete compatibility no-op: existing calls can remain without warnings. It returns the same collection without changing registrations, enabling the defaults policy, or triggering mapper remapping. Calling it without `UseMauiApp()` does not opt a standalone handler collection into Controls defaults.

## Resolution Order

Both `MauiHandlersFactory.GetHandler(Type)` and `MauiHandlersFactory.GetHandlerType(Type)` follow the same resolution order:

1. **Exact DI registration** — checks if a handler was registered for this exact type via `AddHandler`
2. **Direct `[ElementHandler]` attribute** — uses an attribute declared on the requested type
3. **Most specific concrete candidate** — compares the most specific registered base class (from `RegisteredHandlerServiceTypeSet`) with the nearest inherited `[ElementHandler]` attribute. An inherited attribute stands in for the default registration its declaring class had before handlers became trimmable, so the more specific of the two wins: a handler registered for `Button` applies to a derived `FancyButton`, but a registration for `View` or `ContentView` does not divert a subclass of an attributed `Button` or `Frame` from that control's handler. A registration on the declaring class itself overrides its attribute.
4. **Interface registration** — when no concrete candidate exists, uses the most specific registered interface (e.g., a handler registered for `IScrollView` matches a `ScrollView` instance). All matching interfaces are compared before reporting an ambiguity, so registrations for two unrelated interfaces are only ambiguous when no matching interface derives from both.
5. **Content interface defaults** — returns `ContentButtonHandler` for `IContentButton`, then `ContentViewHandler` for any other `IContentView` implementation. With the Controls defaults enabled, these compete with registered interfaces exactly like the interface defaults Controls used to register: they outrank registrations for more general interfaces such as `IView` or `IButton`, a registration for the same or a more specific interface wins, and an unrelated interface registration is ambiguous. Without the Controls defaults they are only a last resort.
6. **`GetHandlerType` returns `null`** / **`GetHandler` throws `HandlerNotFoundException`** — if none of the above matched

Registered concrete types always outrank registered interfaces, matching the previous built-in registrations, which were all concrete except the content interfaces.

The direct/inherited attribute lookup and the selected assignable candidate are cached per requested view type in thread-safe dictionaries. The full resolution result is not cached: exact registrations remain live so app customization can override defaults, and each handler request still creates a new handler instance.

These caches are owned by each handler factory. Mutations of the built-in handler collection (`Add`, `Insert`, indexer replacement, `Remove`, `RemoveAt`, and `Clear`) advance its version. The next lookup replaces the cached attribute lookups and assignable candidates when that version changes. Assignable resolution is rebuilt from the current descriptors, including registrations added through the underlying DI collection APIs. Custom collections without a mutation version are resolved without retaining these caches between requests.

Hot Reload metadata updates, cache clearing, and reset also advance a generation checked by every factory. Invalidation happens before views are reloaded. Registration replacement through Hot Reload additionally advances the collection version. Handlers resolved from `[ElementHandler]` or a content interface default have no collection descriptor for a replacement to find, so a pending replacement of such a handler type is registered for the requested view type the next time the factory resolves or activates it, and that request already uses the replacement. Collection mutation itself retains the normal `IServiceCollection` threading contract: callers must not mutate it concurrently with handler resolution.

`GetHandler(Type)` is the primary API for creating a handler instance. `GetHandlerType(Type)` returns the handler `Type` without instantiating it and is used by code paths that need to compare handler types or create handlers through DI fallback.

### Handler Instantiation

How a handler instance is created depends on how it was resolved:

- **DI-registered handlers** (steps 1, 3 & 4): Instantiated through `MauiFactory.GetService()`, which uses `Activator.CreateInstance` on the registered `ImplementationType`, or invokes the `ImplementationFactory` delegate if one was provided. Factory delegates receive a service provider for the current `IMauiContext`, so they can resolve that context (and, on Android, its `Context`). `GetHandlerType(Type)` can only report DI registrations that provide an `ImplementationType`; a matching factory registration still wins resolution, but `GetHandlerType(Type)` returns `null` rather than falling through to a lower-precedence attribute.
- **`[ElementHandler]` attribute** (steps 2 & 3): Instantiated directly via `Activator.CreateInstance` — no DI involvement.
- **Constructor injection**: `ElementExtensions.ToHandler()` delegates activation to the internal `MauiHandlersFactory.GetHandlerWithInjection()` path. The built-in factory checks whether the resolved handler type has a public parameterless constructor. If it does, `Activator.CreateInstance` is used; otherwise, `ActivatorUtilities.CreateInstance` supplies constructor arguments from the current context's DI container (including its Android `Context`) without first throwing a missing-constructor exception. Value-type handlers retain their default construction through `Activator.CreateInstance`. A failure to resolve a dependency propagates without retrying the activation. Custom `IMauiHandlersFactory` implementations retain the exception-based fallback for `MissingMethodException`, including when wrapped in `HandlerNotFoundException`.

Constructor selection is checked on each activation, so no activation-strategy cache or separate invalidation is needed. Handler resolution still runs first on every request, and factory delegates are never bypassed. Each activation resolves dependencies from the current context, and separately built `MauiApp` instances do not affect one another. A handler type that needs constructor injection is never retained as a singleton: even when registered with a singleton descriptor, each activation creates a new handler from the current context's dependencies, as before. Handler types created through factory delegates and constructor injection are recorded when activated, so Hot Reload handler replacement applies to them. Direct `GetHandler()` callers keep the parameterless-constructor behavior.

> **Note:** Handlers resolved via `[ElementHandler]` are instantiated with `Activator.CreateInstance()`
> (not through the DI container), so a public parameterless constructor is expected on the common path.
> The factory's internal activation path used by `ElementExtensions.ToHandler()` provides a safety net:
> if no public parameterless constructor exists, it uses `ActivatorUtilities.CreateInstance()`, which
> supplies constructor arguments from the DI container. This applies to any type-based handler resolved through
> `ToHandler()` — including `[ElementHandler]`-resolved handlers, not only DI-registered ones — but callers
> that use `GetHandler()` directly do not get it.
> Declaring handlers with a parameterless constructor (and registering an implementation factory with
> `AddHandler<TType>(Func<IServiceProvider, IElementHandler>)` when constructor arguments are required)
> remains the recommended approach.

### Controls mapper remapping

Controls-specific mapper remaps are independent of handler resolution and run lazily, the first time an instance of each remappable Controls type is attached to a handler. The Controls `Element` handler setter calls the internal remap hook before attaching any handler, including compatibility renderers that do not derive from `ElementHandler`. `Toolbar`, which is not an element, calls it from its own handler setter the same way. `ElementHandler.SetVirtualView()` also calls the hook, through the internal `IControlsMapperRemappable` interface, for handlers attached directly outside these paths. All of them run before the element's properties are first mapped. Remaps therefore run regardless of whether the handler came from exact DI registration, assignable DI registration, `[ElementHandler]`, or a content interface fallback, and `UseMauiApp` no longer applies them at startup.

Each remappable Controls type overrides the internal `RemapForControls()` method, calls `base.RemapForControls()` first, and then applies its own mapper changes through a blocking, one-time `OneTimeInitializationAction`. Attaching a `Label`, for example, applies the `Element`, `VisualElement` and `Label` remaps in that order, but not the remaps of unrelated control types. Concurrent callers wait for a remap that is in progress to finish, and unrelated control types do not contend on a global lock. Non-mapper command dependency setup, such as `CommandProperty.DependsOn(...)`, remains in the relevant control type initialization so binding behavior is available before a handler is attached. Remaps that install a platform view factory (`TabbedViewHandler.PlatformViewFactory`, and `RadioButtonHandler.PlatformViewFactory` on Android) keep a factory the app has already installed instead of replacing it.

Controls property and command mapper changes update the framework mapping underneath application customizations. The built-in Core handler mappers that Controls remaps seal their framework mappings after construction, so every later indexer assignment is recorded as an application customization, whether it replaces a local key, a key supplied by a chained parent mapper, or a key no mapper defines yet. `SwitchHandler.Mapper` and the `ButtonHandler.TextButtonMapper` and `ButtonHandler.ImageButtonMapper` parents are not sealed; on those, only replacing an existing key is recorded, and assigning a key they do not define yet stores it directly. Direct indexer replacements and `AppendToMapping`, `PrependToMapping`, `ModifyMapping`, and `ReplaceMapping` calls are recorded in order and recomposed whenever a Controls remap changes the framework mapping, so on sealed mappers they behave the same whether they are registered before or after the first control instance is attached. `Label` still applies its Android `IsTextSelectable` and Apple `FlowDirection` additions with the public `AppendToMapping` from its static constructor, so they are recorded as application customizations when the `Label` type is initialized, as before remapping became lazy, and keep their timing-dependent order relative to app customizations of those keys: an app append registered before the first `Label` is created runs before them, and one registered afterwards runs after them.

Each recorded customization registers its mapping through the mapper's `SetPropertyCore`, so transformations made by an override of that method apply to customized keys too, once per customization; a direct replacement through `Add` or the indexer has already passed through the override and is not transformed again. A typed mapping added through the public helpers maps a virtual view of another type through the mapper's current chained mappers, as a mapping added through `Add` does. `GetProperty`, `GetCommand`, and the indexers return mappings that are safe to wrap in a replacement (`var previous = mapper[key]; mapper[key] = ...`): they include framework remaps that run later but not application customizations recorded later, so a wrapper can never invoke itself.

- A local mapping of a sealed mapper is held in a cell that framework remaps update in place.
- A customized key returns the registration that includes the customizations recorded so far. Repeated reads return the same delegate until the key is customized again, without allocating.
- An inherited key of a sealed built-in mapper returns a mapping bound to that mapper, which also observes a later framework remap of the key on that mapper. Mapper internals, and comparisons with a built-in mapping by reference, use the chained mapping itself.
- An inherited key of a mapper that is not sealed, such as one an app creates over `ViewHandler.ViewMapper`, returns the chained mapping itself, as before. Replacing that key on the child does not change the returned mapping.

Customizations recorded on a child mapper for a key it inherits from a chained parent keep the previous capture semantics for application changes: the child observes the parent's application customizations recorded before its own first customization of that key, but not later ones, and keeps building on the chained mappers it had at that point even if its `Chained` property is later replaced or cleared. Framework mappings and remaps of the parent, including keys the parent only gains during a lazy remap, are always observed. A framework remap of a key the child inherits builds on the parent's mapping as it is when the remap runs, on the first attachment of a control of that type, including the parent's application customizations registered before then, as when the remap captured the previous mapping.

A child records only application changes it can order: customizations recorded through the mapper APIs and replacements of existing keys. Untracked changes have no position in that order, so a child customization observes them like framework mappings, which it must observe so that remaps running after the customization are not lost. Untracked changes are the first assignment of a key on a mapper that is not sealed, including the built-in initializers of such mappers, and any change to a chained mapper that is not a `PropertyMapper` or `CommandMapper`. For example, appending to a key on a child and only then assigning that key for the first time on an unsealed parent makes the child run the parent's new mapping before the appended one. When the parent already has the key at that point, or is sealed, the child keeps the mapping it captured.

Mapper customization is a startup operation. Like registering mappings in earlier releases, customizing a mapper while another thread updates properties or invokes commands through it, or through a mapper chained to it, is not supported.

Merged mapper views and compositions are validated against a stamp that every mapper change draws from a counter shared by all mappers of its kind, so replacing a chained mapper, even with one that changed less often, refreshes everything built on it.

### Behavior changes

Compared with the previous release, where Controls remaps ran during `UseMauiApp` and replaced mappings by capturing and re-registering them:

- **App customizations registered before a remap now compose on top of it.** Previously, a Controls remap that ran after an app's `AppendToMapping`, `PrependToMapping`, `ModifyMapping`, or `ReplaceMapping`, for example one registered in a handler's static constructor before `UseMauiApp`, wrapped or replaced the app's change. Now the Controls remap is placed underneath the app's change, whenever the first control of that type is attached. Callbacks appended to two related keys can therefore run in a different order. For example, with app callbacks appended to `StepperHandler.Mapper`'s `Increment` and `Interval` keys before `UseMauiApp`, changing `Stepper.Increment` used to run the app's `Increment` callback before the Controls redirect to `Interval`, and now runs it after the `Interval` callback. To migrate, don't rely on the order of callbacks across keys. A callback that needs the native state another key's update produces should read it from the virtual view, or update that state itself.
- **Inherited mappings of built-in mappers are bound to the mapper they are read from.** `GetProperty` and `GetCommand` on a sealed built-in handler mapper, such as `LabelHandler.Mapper`, return a mapping bound to that mapper for keys inherited from `ViewHandler.ViewMapper`, rather than the `ViewMapper` delegate itself. Invoking or wrapping the returned mapping behaves as before. To check whether a handler mapper still uses the shared `ViewMapper` mapping, compare the behavior rather than the delegates.
- **Controls remaps run when the first control of each type is attached.** Lazy remapping is what allows an app to trim the handlers and mapper code of controls it never uses, so there is no switch that restores startup remapping. Before the first instance of a Controls type is attached, the global mappers do not contain the keys that type's remap adds, such as the `BackgroundColor`, `BackgroundImageSource` and `SemanticProperties.*` redirects on `ViewHandler.ViewMapper` or `TextType` on `LabelHandler.Mapper`. A custom mapper built by copying `GetKeys()` and `GetProperty()` delegates into an unchained mapper during startup no longer gets those keys, so its handlers ignore changes to those properties. To migrate, chain the custom mapper to the built-in one instead, for example `new PropertyMapper<ILabel, ILabelHandler>(LabelHandler.Mapper) { ... }`, or customize the built-in mapper with `AppendToMapping`, `PrependToMapping`, `ModifyMapping` or `ReplaceMapping`. Chained mappers and recorded customizations observe a remap whenever it runs. If a copy is unavoidable, take it after a control of that type has been attached, for example from its `HandlerChanged` event.
- **Framework remaps of inherited keys capture the parent's customizations at first attachment.** A remap of a key a handler mapper inherits builds on the parent's mapping as it is when the remap runs. It now also includes app customizations of that key on the parent registered after `UseMauiApp` but before the first control of that type is attached. To have a customization apply to every handler, register it before the first control is attached, for example in `MauiProgram`, or customize the child mapper's key directly.
- **Platform view factories.** An app factory assigned to `TabbedViewHandler.PlatformViewFactory`, or to `RadioButtonHandler.PlatformViewFactory` on Android, before the first `TabbedPage` or `RadioButton` is attached is kept and replaces the Controls factory, as a factory assigned after `UseMauiApp` did before. Reading the property during startup to wrap the Controls factory now returns `null`, because the Controls factory is installed on first attachment.

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

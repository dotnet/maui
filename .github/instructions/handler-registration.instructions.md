---
description: "Rules for declaring built-in handler defaults, handler resolution, and lazy Controls mapper remaps"
applyTo:
  - "src/Controls/src/Core/**/*.Mapper.cs"
  - "src/Controls/src/Core/Hosting/**"
  - "src/Controls/src/Xaml/Hosting/**"
  - "src/Core/src/Hosting/**"
  - "src/Core/src/Handlers/Element/**"
  - "src/Core/src/PropertyMapper*.cs"
  - "src/Core/src/CommandMapper*.cs"
  - "src/Core/src/MappingCustomizationSequence.cs"
  - "src/Core/src/Platform/ElementExtensions.cs"
  - "src/Core/src/HotReload/**"
  - "src/Controls/tests/DeviceTests/*HandlerResolution*"
  - "src/Controls/tests/Core.UnitTests/HostBuilderHandlerTests.cs"
  - "src/Core/tests/UnitTests/Hosting/HostBuilderHandlerTests.cs"
  - "src/Core/tests/UnitTests/*Mapper*Tests.cs"
  - "src/Controls/tests/DeviceTests/MapperTests.cs"
  - "src/Controls/tests/Core.UnitTests/*Mapper*Tests.cs"
  - "docs/design/HandlerResolution.md"
---
# Handler Registration, Resolution, and Controls Remaps

Built-in handlers are trimmable: each control declares its default handler with an attribute, and Controls mapper changes run lazily on first attachment. [docs/design/HandlerResolution.md](../../docs/design/HandlerResolution.md) is the source of truth for resolution order and mapper composition; keep it in sync when changing either.

## Declaring Built-in Defaults
- Declare a built-in control's default handler with the internal `[ElementHandler(typeof(XHandler))]` attribute on the control class (see `Button`, `Label`, `Shell`, `CollectionView`).
- Scope the attribute with `#if` to the platforms where the handler type exists. Do not add a platform (e.g. `TIZEN`) unless the handler is implemented there.
- Do NOT add built-in defaults as DI registrations. The eager Controls registration table is gone, and `AddMauiControlsHandlers()` is a compatibility no-op that must stay a no-op. `ConfigureMauiHandlers`/`AddHandler` registrations are for app and library overrides, interface registrations, and the rare handler that must not be statically rooted (HybridWebView stays a deferred DI factory registration in `SetupDefaults`).
- Give attribute-resolved handlers a public parameterless constructor. `ToHandler()` falls back to constructor injection from the current `IMauiContext` (including its Android `Context`), but direct `IMauiHandlersFactory.GetHandler()` calls do not. When a handler needs constructor arguments, register an `AddHandler<TType>(Func<IServiceProvider, IElementHandler>)` factory. Its provider resolves the current `IMauiContext` (and the Android `Context`) only on context-taking activation paths such as `ToHandler()`; the public `GetHandler(Type)` passes the handler factory itself, so a factory that requires the context must tolerate or avoid direct `GetHandler` calls. Do not add constructor-activation caches; the factory checks constructors per activation.
- Custom `IMauiHandlersFactory` implementations are unsupported because they cannot see attribute defaults. Do not add code or docs that rely on replacing the factory; customize through `ConfigureMauiHandlers`.

## Feature-Switch-Dependent Handlers
- When the handler depends on a feature switch, declare a nested `internal sealed class XHandlerAttribute : ElementHandlerAttribute` on the control, apply it as `[XHandler]`, and override `GetHandlerType()` to branch on a `RuntimeFeature` property with `[FeatureSwitchDefinition]`. The trimmer can then remove the inactive handler. Examples: Android Material3 controls (`Label`, `Entry`, `Picker`, ... on `RuntimeFeature.IsMaterial3Enabled`), Windows `CollectionView` (`IsWindowsCollectionView2HandlerEnabled`), Apple `Shell`/`ShellItem` (`IsiOSShellHandlerEnabled`).
- Annotate the override with `[return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]`.
- `GetHandlerType()` may return `null` to declare no handler in the current configuration (Apple `ShellItem` when the compatibility `ShellRenderer` manages items). Resolution then continues as if the attribute were absent.

## Resolution Rules
`MauiHandlersFactory.GetHandler(Type)` and `GetHandlerType(Type)` must resolve identically, in this order:
1. Exact registration for the type.
2. `[ElementHandler]` declared directly on the type.
3. The more specific of the nearest registered base class and the nearest inherited attribute. An inherited attribute ranks like the old built-in registration of its declaring class, so a registration for `Button` applies to `FancyButton : Button`, but one for `View` does not divert it from `ButtonHandler`.
4. The most specific registered interface. With Controls defaults enabled, the `IContentButton`/`IContentView` defaults compete here as interface registrations.
5. Content-interface fallbacks: `ContentButtonHandler`, then `ContentViewHandler`.

- Registered concrete types outrank registered interfaces. An exact `AddHandler` always overrides an attribute.
- With Controls defaults enabled (`UseMauiApp`), `TryAddHandler` does not replace a directly attributed default or the content-interface defaults. It still registers derived types that only inherit an attribute.
- Cache only per-type attribute lookups and assignable candidates, invalidated by collection version and Hot Reload generation. Do not cache the full resolution result; exact registrations must stay live.

## Lazy Controls Mapper Remaps
Remaps run lazily, the first time an instance of each remappable type is attached to a handler, not at `UseMauiApp`. `Element`'s handler setter and `ElementHandler.SetVirtualView()` (through `IControlsMapperRemappable`, which also covers non-elements such as `Toolbar`) invoke the hook before properties are first mapped. Each remappable type owns a one-time remap and calls its base first, so attaching a `Label` applies the `Element`, `VisualElement` and `Label` remaps in that order:

```csharp
static readonly OneTimeInitializationAction s_remappedForControls = new(RemapForControlsOnce);
internal override void RemapForControls()
{
	base.RemapForControls();
	s_remappedForControls.InvokeOnce();
}

static void RemapForControlsOnce()
{
	ButtonHandler.Mapper.ReplaceMappingForControls<Button, IButtonHandler>(nameof(Text), MapText);
}
```

- Inside a remap, use the internal framework helpers: `ReplaceMappingForControls`, `AppendToMappingForControls`, `PrependToMappingForControls`, `ModifyMappingForControls` (property mappers), and `ModifyMappingForControls`/`PrependToMappingForControls` (command mappers). They compose underneath app customizations, as if the remap had run at startup.
- Do NOT use the public `AppendToMapping`, `PrependToMapping`, `ModifyMapping`, `ReplaceMapping`, or the mapper indexer inside a remap. Those are the app customization layer; a lazy framework call through them would reorder or override app customizations. The only exceptions are Label's Apple `FlowDirection` and Android `IsTextSelectable` additions: they deliberately keep the public `AppendToMapping`, so they are recorded as app customizations when the first `Label` attaches and keep their timing-dependent order relative to app customizations of those keys. Do not copy this pattern.
- When a lazy remap sets a public static factory such as `PlatformViewFactory`, use `??=`. A factory the app sets during startup must win (`RadioButton.Mapper.cs`, `TabbedPage.Mapper.cs`).
- Keep non-mapper setup, such as `CommandProperty.DependsOn(...)`, in control type initialization (a static constructor). Bindings need it before any handler attaches.
- A Core handler property or command mapper that Controls remaps must end its initializer with `.WithFrameworkMappingsSealed()`. Every later assignment is then recorded as an app customization, including keys the mapper does not define yet, and composes on top of the lazy remap. `SwitchHandler.Mapper`, `ButtonHandler.TextButtonMapper` and `ButtonHandler.ImageButtonMapper` are not sealed: on them only replacing an existing key is recorded, and assigning a new key stores it directly. Don't assume sealed semantics there, and treat sealing them as a behavior change.
- A framework remap of a key on a child mapper, including a key it inherits from a chained parent, places the framework mapping before every app customization. The child then builds on the parent's framework mapping only and stops observing app customizations of that key made on the parent, even ones registered before the first attach. Before remapping an inherited key on a child mapper, check whether apps customize it on the parent (for example `ViewHandler.ViewMapper`).

## Testing
- `Controls.Core.UnitTests` targets a non-platform TFM, so platform-specific attribute branches need device tests (`ShellHandlerResolutionTests.iOS.cs`, `Material3HandlerResolutionTests.Android.cs`). Assert both `GetHandlerType` and actual handler creation.
- Device tests that toggle an `AppContext` switch must use a `[CollectionDefinition(..., DisableParallelization = true)]` collection and restore the original value in `finally`.
- Global mappers (`ViewHandler.ViewMapper`, `ButtonHandler.Mapper`, ...) only contain the Controls remaps after the relevant control type has attached a handler. A test that reads their keys or mappings must trigger the remap first, for example `new VisualElement().RemapForControls();` (see `VisualElementMapperTests`), so it doesn't depend on test order.
- Mapper-preservation tests should cover app customizations made both before and after the lazy remap. Include keys that are absent until the remap, keys inherited from a chained mapper, and, for unsealed mappers, the narrower replace-only recording.
- CI runs Core unit tests through xunit.console, which can load the test assembly twice. Do not rely on `Type` identity across assembly loads (Hot Reload handler replacement matches by type name).

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

Built-in handlers are trimmable: each control declares its default handler with an attribute, and Controls mapper changes run lazily on first attachment. [docs/design/HandlerResolution.md](../../docs/design/HandlerResolution.md) is the source of truth for resolution order and mapper composition; keep it in sync when changing either. Record any change that existing apps can observe in its "Behavior changes" section, with migration guidance.

## Declaring Built-in Defaults
- Declare a built-in control's default handler with the internal `[ElementHandler(typeof(XHandler))]` attribute on the control class (see `Button`, `Label`, `Shell`, `CollectionView`).
- Scope the attribute with `#if` to the platforms where the handler type exists. Do not add a platform (e.g. `TIZEN`) unless the handler is implemented there.
- Do NOT add built-in defaults as DI registrations. The eager Controls registration table is gone, and `AddMauiControlsHandlers()` is a compatibility no-op that must stay a no-op. `ConfigureMauiHandlers`/`AddHandler` registrations are for app and library overrides and interface registrations. The one existing built-in exception is `HybridWebView`. It is not declared with `[ElementHandler]` and keeps its `AddHandler<HybridWebView>(static _ => new HybridWebViewHandler())` factory in `SetupDefaults`; leave it as is. Don't copy it for new defaults: a factory delegate only defers construction, and the handler stays rooted for every app.
- Give attribute-resolved handlers a public parameterless constructor. `ToHandler()` falls back to constructor injection from the current `IMauiContext` (including its Android `Context`), but direct `IMauiHandlersFactory.GetHandler()` calls do not. When a handler needs constructor arguments, register an `AddHandler<TType>(Func<IServiceProvider, IElementHandler>)` factory. Its provider resolves the current `IMauiContext` (and the Android `Context`) only on context-taking activation paths such as `ToHandler()`; the public `GetHandler(Type)` passes the handler factory itself, so a factory that requires the context must tolerate or avoid direct `GetHandler` calls. Do not add constructor-activation caches; the factory checks constructors per activation. Never retain a handler type that needs constructor injection as a singleton, even under a singleton descriptor: each activation must create a new handler from the current context.
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
4. The most specific registered interface. Compare every matching interface before reporting an ambiguity: two unrelated registered interfaces are ambiguous only when no matching interface derives from both. With Controls defaults enabled, the `IContentButton`/`IContentView` defaults compete here as interface registrations.
5. Content-interface fallbacks: `ContentButtonHandler`, then `ContentViewHandler`.

- Registered concrete types outrank registered interfaces. An exact `AddHandler` always overrides an attribute.
- With Controls defaults enabled (`UseMauiApp`), `TryAddHandler` does not replace a directly attributed default or the content-interface defaults. It still registers derived types that only inherit an attribute. To replace a built-in handler, use `AddHandler`, which wins regardless of whether its callback runs before or after `UseMauiApp`. `RemoveAll` followed by `TryAddHandler` no longer works, because there is no default registration to remove, and `AddMauiControlsHandlers()` does not restore a replaced built-in handler.
- Cache only per-type attribute lookups and assignable candidates, invalidated by collection version and Hot Reload generation. Do not cache the full resolution result; exact registrations must stay live.
- Hot Reload handler replacement must cover every resolution path. Attribute and content-default handlers have no collection descriptor, so the factory registers a pending replacement for the requested view type the next time it resolves or activates it. Keep that replacement scoped to the collection passed to `RegisterHandlers`, and reinstall it if it is removed.

## Lazy Controls Mapper Remaps
Remaps run lazily, the first time an instance of each remappable type is attached to a handler, not at `UseMauiApp`. `Element`'s handler setter, `Toolbar`'s own handler setter (it is not an element), and `ElementHandler.SetVirtualView()` (through `IControlsMapperRemappable`, for handlers attached outside those paths) invoke the hook before properties are first mapped. A new remappable non-element type must call `RemapForControls()` from its handler setter too. Each remappable type owns a one-time remap and calls its base first, so attaching a `Label` applies the `Element`, `VisualElement` and `Label` remaps in that order:

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
- Do NOT use the public `AppendToMapping`, `PrependToMapping`, `ModifyMapping`, `ReplaceMapping`, or the mapper indexer inside a remap. Those are the app customization layer; a lazy framework call through them would reorder or override app customizations.
- The only framework code that uses public appends is Label's Android `IsTextSelectable` and Apple `FlowDirection` additions. They run from `static Label()`, not the lazy remap, so they keep main's type-initialization timing and are recorded as app customizations. An app append registered before the first `Label` is created runs before them; one registered afterwards runs after them. Keep them out of `RemapForControlsOnce()`, and do not copy this pattern for new mappings.
- When a lazy remap sets a public static factory such as `PlatformViewFactory`, do it in an internal `InstallPlatformViewFactory()` helper that uses `??=`, called from the remap. A factory the app sets during startup must win (`RadioButton.Mapper.cs`, `TabbedPage.Mapper.cs`). Reading the property during startup returns `null`, because the Controls factory is installed on first attachment.
- Keep non-mapper setup, such as `CommandProperty.DependsOn(...)`, in control type initialization (a static constructor). Bindings need it before any handler attaches.
- Mapper customization is a startup operation: customizing a mapper while another thread updates properties or invokes commands through it, or through a mapper chained to it, is unsupported. Don't add locking to support that.
- A Core handler property or command mapper that Controls remaps must end its initializer with `.WithFrameworkMappingsSealed()`. Every later assignment is then recorded as an app customization, including keys the mapper does not define yet, and composes on top of the lazy remap. `SwitchHandler.Mapper`, `ButtonHandler.TextButtonMapper` and `ButtonHandler.ImageButtonMapper` are not sealed: on them only replacing an existing key is recorded, and assigning a new key stores it directly. Don't assume sealed semantics there, and treat sealing them as a behavior change.
- A framework remap of a key a child mapper inherits builds on the parent's mapping as it is when the remap runs, on first attachment of that control type. That includes app customizations of the key on the parent registered before then, but not later ones. Before remapping an inherited key, consider apps that customize it on the parent (for example `ViewHandler.ViewMapper`), and disclose the capture timing.
- Reading an inherited key through the indexer, `GetProperty` or `GetCommand` returns a mapping bound to the child on a sealed built-in mapper, and the chained parent mapping itself on an unsealed mapper. Do not compare a sealed mapper's returned mapping with a built-in delegate by reference. Mapper internals and Controls code that needs the chained delegate itself use the internal `PropertyMapper.GetPropertyDirect` (see `VisualElement.Animations.cs`).

## Testing
- `Controls.Core.UnitTests` targets a non-platform TFM, so platform-specific attribute branches need device tests (`ShellHandlerResolutionTests.iOS.cs`, `Material3HandlerResolutionTests.Android.cs`). Assert both `GetHandlerType` and actual handler creation.
- Device tests that toggle an `AppContext` switch must use a `[CollectionDefinition(..., DisableParallelization = true)]` collection and restore the original value in `finally`.
- Global mappers (`ViewHandler.ViewMapper`, `ButtonHandler.Mapper`, ...) only contain the Controls remaps after the relevant control type has attached a handler. A test that reads their keys or mappings must trigger the remap first, for example `new VisualElement().RemapForControls();` (see `VisualElementMapperTests`), so it doesn't depend on test order.
- Mapper-preservation tests should cover app customizations made both before and after the lazy remap. Include keys that are absent until the remap, keys inherited from a chained mapper (including parent customizations made between `UseMauiApp` and the first attachment), and, for unsealed mappers, the narrower replace-only recording and the unbound chained mapping returned for inherited keys.
- CI runs Core unit tests through xunit.console, which can load the test assembly twice. Do not rely on `Type` identity across assembly loads (Hot Reload handler replacement matches by type name).

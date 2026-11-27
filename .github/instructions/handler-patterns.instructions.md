---
applyTo:
  - "src/Core/src/Handlers/**"
  - "src/Controls/src/Core/Handlers/**"
---
# Handler Mapper and Property Patterns

## Property Update Flow
- Property updates MUST go through `Handler.UpdateValue(nameof(Property))` — never call mapper methods directly
- Direct calls bypass user-registered `AppendToMapping`/`PrependToMapping` customizations
- Map dependencies before dependents — if property B reads property A, A's mapper must run first
- `CommandMapper` entries return void and use the `(handler, view, args)` signature

## Lifecycle Management
- **ConnectHandler**: Register listeners, subscribe to events, capture native defaults BEFORE applying cross-platform properties
- **DisconnectHandler**: Unsubscribe all events, dispose platform resources, null out references
- Call `base.ConnectHandler`/`base.DisconnectHandler` — base class performs platform hookup/teardown (NOT mapper initialization, which happens in `SetVirtualView`)

## Null Safety in Callbacks
- Null-check `VirtualView` before access in every mapper method and platform callback — it is null during disconnect
- Validate `MauiContext` before use — throw `InvalidOperationException` with descriptive message if null
- After async operations, verify the handler is still connected before applying results

## Native Defaults Preservation
- Capture native default values (colors, fonts, styles) in `ConnectHandler` BEFORE any cross-platform property is applied
- Clearing a cross-platform property (setting to null/default) must restore the captured native default, not a hardcoded fallback
- On Windows, preserve WinUI XAML style-applied values; on Android, cache theme-inherited drawables; on iOS, capture UIAppearance defaults

## Mapper Extensibility
- When extending existing mappers, ensure the base mapper chain is called — do not silently replace
- Static mapper methods should be `public static` to enable platform-specific overrides
- Use `nameof()` for property keys — avoid magic strings
- End built-in property and command mapper initializers with `.WithFrameworkMappingsSealed()` so later assignments are recorded as app customizations
- Controls-layer changes to a Core handler mapper belong in the control's lazy `RemapForControls()` and use the internal `*ForControls` helpers, not the public `AppendToMapping`/`ModifyMapping`/`ReplaceMapping` app customization APIs

## Registering Built-in Handlers
- Declare a built-in control's default handler with `[ElementHandler]` on the control class, not with a DI registration; DI (`ConfigureMauiHandlers`/`AddHandler`) is for app and library overrides
- Give attribute-resolved handlers a public parameterless constructor — direct `GetHandler()` calls do not use constructor injection
- See [handler-registration.instructions.md](handler-registration.instructions.md) and [docs/design/HandlerResolution.md](../../docs/design/HandlerResolution.md) for feature-switch handler selection, resolution precedence, lazy remaps, and testing

## Fire-and-Forget Async
- Use `.FireAndForget(handler)` for async operations in mapper methods — never use bare `async void`
- The `FireAndForget` overload accepting a handler logs exceptions through the handler's service provider

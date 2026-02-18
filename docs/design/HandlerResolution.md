Handler Resolution
===

# Introduction

Handlers are the platform components used to render a cross-platform `View` on the screen. Each view type is associated with a handler that knows how to create and manage the corresponding platform-native control.

## How Handlers Are Registered

### 1. `[ElementHandler]` Attribute (Primary Mechanism)

Most built-in .NET MAUI views declare their handler using the `[ElementHandler]` attribute directly on the view class:

```csharp
[ElementHandler(typeof(ButtonHandler))]
public partial class Button : View, IButton { ... }
```

This approach is trimmer-safe and AOT-friendly because the handler type is statically referenced. The attribute is discovered at runtime by walking the type's class hierarchy (`Type.BaseType`) until a type with `[ElementHandler]` is found.

On Android, `ElementHandlerAttribute.CreateHandler` probes for a constructor accepting `Android.Content.Context` before falling back to a parameterless constructor. This supports legacy compatibility renderers (e.g., `FrameRenderer`, `ListViewRenderer`, `TableViewRenderer`).

### 2. `AddHandler` via DI (Override / Extension Mechanism)

Handlers can also be registered at startup via `ConfigureMauiHandlers`:

```csharp
builder.ConfigureMauiHandlers(handlers =>
{
    handlers.AddHandler(typeof(Button), typeof(ButtonHandler));
});
```

This is primarily used by:
- Third-party libraries (e.g., Maps, BlazorWebView)
- App developers overriding a built-in handler
- Compatibility shims

DI-registered handlers take priority over `[ElementHandler]` attributes when registered for the exact type.

### 3. `IContentView` Fallback

`ContentViewHandler` is the default fallback handler for any type implementing `IContentView` that doesn't have a more specific handler. This covers types like `ContentPresenter` and `TemplatedView` without requiring explicit registration.

## Resolution Order

`MauiHandlersFactory.GetHandler(Type, IMauiContext)` resolves handlers in this order:

1. **Exact DI registration** — checks if a handler is registered for this exact type via `AddHandler`
2. **`[ElementHandler]` attribute** — walks the type's base class hierarchy looking for the attribute
3. **Interface-based DI registration** — uses `RegisteredHandlerServiceTypeSet` to find the best matching interface registration (e.g., a handler registered for `IScrollView` matches a `ScrollView` instance)
4. **`IContentView` fallback** — returns `ContentViewHandler` for any `IContentView` implementation
5. **Throws `HandlerNotFoundException`** — if none of the above matched

## Types Used in Handler Resolution

### `IMauiHandlersFactory`

The main interface for requesting handlers:

```csharp
public interface IMauiHandlersFactory : IMauiFactory
{
    Type? GetHandlerType(Type iview);
    IElementHandler? GetHandler(Type type, IMauiContext context);
}
```

Access to the factory is provided through `IMauiContext.Handlers`.

### `MauiHandlersFactory`

The concrete implementation of `IMauiHandlersFactory`. Extends `MauiFactory` (a lightweight DI-like resolver) and adds the `[ElementHandler]` attribute lookup and `IContentView` fallback logic.

### `ElementHandlerAttribute`

Attribute placed on view types to declare their handler:

```csharp
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ElementHandlerAttribute : Attribute
{
    public ElementHandlerAttribute(Type handlerType);
    public virtual IElementHandler CreateHandler(IMauiContext context);
    public Type HandlerType { get; }
}
```

`Inherited = false` ensures each view type must explicitly declare its handler — the attribute is not inherited from base classes. However, the factory's lookup walks the base class hierarchy, so a base class attribute acts as a fallback for derived types that don't declare their own.

### `RegisteredHandlerServiceTypeSet`

Tracks which types have been registered via `AddHandler`. Used by `MauiHandlersFactory` to resolve interface-based registrations when no exact match or `[ElementHandler]` attribute is found. Finds the "most specific" matching interface to avoid ambiguity.

## Mapper Remapping

Many controls override default mapper entries in their static constructors (e.g., `Button` overrides `VisualElement`'s `Background` mapper entry). To ensure base class static constructors run before derived ones, each level in the hierarchy uses this pattern:

```csharp
// In VisualElement.Mapper.cs
static VisualElement()
{
    Element.s_forceStaticConstructor = true;
    // ... VisualElement-specific mapper overrides
}

private protected new static bool s_forceStaticConstructor;
```

Writing to the base class's `s_forceStaticConstructor` field forces the runtime to execute the base class static constructor first. This ensures mapper entries are applied in the correct order (base → derived).

In DEBUG builds, `RemappingDebugHelper.AssertBaseClassForRemapping` validates that no intermediate type in the hierarchy is accidentally skipped.

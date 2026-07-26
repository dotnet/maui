# Lazy ResourceDictionary Specification

## Problem

1. **Slow inflation time**: All resources are instantiated upfront during XAML inflation, even if never used
2. **No `x:Shared` support**: Resources that need parenting (e.g., `StrokeShape`) cannot be StaticResources because the same instance is shared - causing crashes when multiple elements try to parent the same object

## Proposed Solution

Store `Func<object>` factories instead of object instances in ResourceDictionary. Resources are created on-demand when accessed.

### API Changes

```csharp
public class ResourceDictionary
{
    // NEW: Add a resource factory instead of an instance
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddFactory(string key, Func<object> factory, bool shared = true);
    
    // NEW: Add an implicit style factory (uses TargetType.FullName as key)
    [EditorBrowsable(EditorBrowsableState.Never)]
    public void AddFactory(Type targetType, Func<Style> factory, bool shared = true);
    
    // MODIFIED: TryGetValue now invokes factory if stored value is a factory
    // For shared=true: invoke once, cache result
    // For shared=false: invoke every time (x:Shared="false")
}
```

### Internal Storage

```csharp
// Wrapper to distinguish factories from regular values
internal sealed class LazyResource
{
    public Func<object> Factory { get; }
    public bool Shared { get; }
    private object? _cachedValue;
    
    public object GetValue()
    {
        if (!Shared) return Factory();
        return _cachedValue ??= Factory();
    }
}

// _innerDictionary stores either:
// - object (existing behavior)
// - LazyResource (new lazy behavior)
```

### Behavior

| Scenario | `shared=true` (default) | `shared=false` |
|----------|------------------------|----------------|
| First access | Invoke factory, cache result, return | Invoke factory, return |
| Subsequent | Return cached | Invoke factory, return |

## Source Generator Integration

**SourceGen-only for now**: XamlC support would add significant complexity. Runtime inflator support is feasible (existing PR) but not in scope for initial implementation.

```csharp
// Generated code (SourceGen)
Resources.AddFactory("MyColor", () => new Color(...), shared: true);
Resources.AddFactory("MyShape", () => new RoundRectangle { ... }, shared: false);

// Implicit style
Resources.AddFactory(typeof(Label), () => new Style(typeof(Label)) { ... }, shared: true);
```

### XAML Syntax

```xaml
<!-- Default: x:Shared="true" (cached, current behavior) -->
<Style x:Key="MyStyle" TargetType="Button">...</Style>

<!-- New: x:Shared="false" (new instance per access) -->
<RoundRectangle x:Key="MyShape" x:Shared="false" CornerRadius="10"/>
```

## Scope

| Inflator | Lazy RD | x:Shared | Notes |
|----------|---------|----------|-------|
| **SourceGen** | ✅ | ✅ | Initial implementation |
| **Runtime** | ❌ | ❌ | Feasible, future work |
| **XamlC** | ❌ | ❌ | Complex, not planned |

## Exclusions

**Value types** (structs like `Thickness`, `CornerRadius`) are excluded from lazy treatment - no benefit since they're copied on access.

## Feature Flag

LazyRD is **enabled by default** for all XAML files compiled with SourceGen. This provides improved performance out of the box.

To opt-out per file:
```xml
<MauiXaml Update="MyPage.xaml" LazyRD="false" />
```

To opt-out globally:
```xml
<MauiXamlLazyRD>false</MauiXamlLazyRD>
```

## API Surface

`AddFactory` is `[EditorBrowsable(Never)]` - generated code only, not for direct use.

**Public iteration behavior:**
- `Keys` → returns keys only, does NOT invoke factories
- `Values` → resolves lazy values (invokes factory, caches if shared)
- `GetEnumerator()` / `foreach` → returns `KeyValuePair<string, object>` with resolved values

Internal `_innerDictionary` stores `LazyResource`, but public API always resolves — no breaking change.

## Benefits

- ⚡ **Faster startup**: Resources created only when needed
- 🔧 **x:Shared for free**: `StrokeShape` in StaticResource finally works
- 📦 **No runtime cost for unused resources**

## Risks

- ⚠️ `x:Shared` only works with SourceGen (acceptable trade-off)

## Future Considerations

- **ApplyToDerivedTypes optimization**: Store `ApplyToDerivedTypes` bool alongside `Type targetType` in implicit style registration, so we don't need to inflate the style just to check this property during style resolution.

- **Default `x:Shared=false` via attribute**: Mark types like `StrokeShape`, `RoundRectangle`, etc. with an attribute (e.g., `[NotSharedByDefault]`) so SourceGen automatically treats them as `x:Shared="false"` without explicit XAML markup.

- **Immutable reference types**: Some reference types are effectively immutable (e.g., `Color` if it were a class, `SolidColorBrush` with frozen state). These could potentially skip lazy creation since sharing is safe. Consider an `[Immutable]` or `[SharedByDefault]` attribute to opt specific types out of lazy creation overhead while still being shareable.

- **Thread safety**: The current `LazyResource.GetValue()` implementation uses a simple check-then-cache pattern without synchronization. This mirrors `ResourceDictionary` itself which uses `Dictionary<string, object>` (not thread-safe). If multi-threaded resource access becomes a requirement, consider using `Lazy<T>` with `LazyThreadSafetyMode.ExecutionAndPublication`. For now, the simpler implementation is preferred since MAUI resource access is UI-thread-only in practice.

- **DynamicResource with runtime-added lazy resources**: `AddFactory` fires `ValuesChanged` with the internal `LazyResource` wrapper instead of the resolved value. This breaks DynamicResource bindings that exist before the resource is added at runtime. Not an issue for SourceGen (resources added during `InitializeComponent` before bindings). Fix: resolve lazy values in `OnValueChanged` or have `Element.OnResourcesChanged` handle `LazyResource` resolution.

- **Nested ResourceDictionaries inside lazy resources**: If a lazy resource has its own `.Resources` property (e.g., a ControlTemplate with embedded resources), those nested resources may not be populated. The lambda uses `stopOnResourceDictionary: true` to prevent infinite recursion. Edge case - needs investigation if real-world usage requires this.

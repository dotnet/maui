# Trimmable Styles

## Overview

When XAML is compiled by the source generator, styles are generated so that the trimmer can remove a style's setters, triggers and behaviors (and everything they reference) when the app never creates an instance of the style's `TargetType`.

## How it works

For a style with a compile-time `TargetType`, the generator creates the style by type name and populates it lazily:

```csharp
var style = new Style("MyApp.MyControl, MyApp, ...");
bool lazyInitialization(Style __style, BindableObject __target)
{
    if (__target is global::MyApp.MyControl target)
    {
        if (__style is null)
            return true;
        // setters, triggers and behaviors are created here
        return true;
    }
    return false;
}
style.LazyInitialization = new Func<Style, BindableObject, bool>(lazyInitialization);
```

The initializer is a local function wrapped in an explicit delegate rather than a lambda. A lambda that captures nothing is cached in a compiler-generated static field, and XAML Hot Reload cannot add fields to a compiler-generated class from an earlier update.

When `MyControl` is never instantiated, NativeAOT can fold the type check to `false` and remove the setter code. Constructed generic target types use `typeof(T)` instead of a type name, which keeps only the type's identity.

Styles whose `TargetType` is only known at runtime (for example `{x:Static}`) are created eagerly with `new Style(typeof(...))`, as before.

## Behavior differences

- `Style.Setters`, `Triggers` and `Behaviors` contain the XAML-declared items only after the style was first applied to an instance of its `TargetType`.
- The style is only applied to instances of its `TargetType` (or derived types). Applying it to any other element, which can only happen when `Style` is set explicitly (e.g. `{StaticResource}`), applies nothing and logs a warning: `Style TargetType X is not compatible with element target type Y. The style is not applied.`
- `StaticResource` values inside setters are resolved when the style is first applied rather than when the page is loaded.

The source generator reports `MAUIX2024` when an element's `Style` is set to a `{StaticResource}` style declared in the same file whose `TargetType` is not compatible with the element.

## Feature Flag

Trimmable styles are **enabled by default** for all XAML files compiled with SourceGen. Disabling them restores eager styles, which apply to mismatched elements as before but cannot be trimmed.

To opt-out per file:
```xml
<MauiXaml Update="MyPage.xaml" TrimmableStyles="false" />
```

To opt-out globally:
```xml
<MauiXamlTrimmableStyles>false</MauiXamlTrimmableStyles>
```

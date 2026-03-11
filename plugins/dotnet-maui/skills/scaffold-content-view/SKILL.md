---
name: scaffold-content-view
description: >
  Scaffold a reusable .NET MAUI ContentView with XAML, code-behind, and
  BindableProperty declarations. Covers the full BindableProperty pattern
  including propertyChanged callbacks, exposing custom properties, consuming
  the view from a parent page, and deciding between ContentView and a custom
  control with handlers.
  USE FOR: "ContentView", "custom view", "reusable component", "user control",
  "BindableProperty", "composite control", "scaffold view", "create component",
  "reusable UI".
  DO NOT USE FOR: full-screen pages (use scaffold-page), native platform controls
  (use maui-custom-handlers), or data binding concepts (use maui-data-binding).
---

# Scaffold a Reusable .NET MAUI ContentView

Use this skill whenever you need a self-contained, reusable UI component that
can be dropped into any page. A `ContentView` is the .NET MAUI equivalent of a
user control — it composes existing controls and exposes its own bindable
properties.

---

## Step 1 — Create the XAML File

File name: **`{ComponentName}View.xaml`** (e.g. `RatingView.xaml`).
Place it in a `Controls/` or `Views/Controls/` folder.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentView xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="MyApp.Controls.RatingView"
             x:Name="this">

    <HorizontalStackLayout Spacing="4">
        <Label Text="{Binding Source={x:Reference this}, Path=Label}"
               FontSize="14"
               VerticalTextAlignment="Center" />
        <Label Text="{Binding Source={x:Reference this}, Path=Value, StringFormat='{0}/5'}"
               FontSize="18"
               FontAttributes="Bold"
               VerticalTextAlignment="Center" />
    </HorizontalStackLayout>

</ContentView>
```

> **Key pattern:** Give the root element `x:Name="this"` and bind to its
> properties with `{Binding Source={x:Reference this}, Path=PropertyName}`.
> This avoids confusion with the parent page's `BindingContext`.

---

## Step 2 — Create the Code-Behind with BindableProperties

File name: **`{ComponentName}View.xaml.cs`**.

```csharp
namespace MyApp.Controls;

public partial class RatingView : ContentView
{
    public RatingView()
    {
        InitializeComponent();
    }

    // --- Label property ---------------------------------------------------

    public static readonly BindableProperty LabelProperty =
        BindableProperty.Create(
            propertyName: nameof(Label),
            returnType: typeof(string),
            declaringType: typeof(RatingView),
            defaultValue: string.Empty);

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // --- Value property (with propertyChanged callback) -------------------

    public static readonly BindableProperty ValueProperty =
        BindableProperty.Create(
            propertyName: nameof(Value),
            returnType: typeof(int),
            declaringType: typeof(RatingView),
            defaultValue: 0,
            propertyChanged: OnValueChanged);

    public int Value
    {
        get => (int)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    private static void OnValueChanged(BindableObject bindable, object oldValue, object newValue)
    {
        var control = (RatingView)bindable;
        // React to the new value — update visuals, raise events, etc.
        control.ValueChangedHandler?.Invoke(control, (int)newValue);
    }

    // --- Optional event for consumers ------------------------------------

    public event Action<RatingView, int>? ValueChangedHandler;
}
```

### BindableProperty Anatomy

```
BindableProperty.Create(
    propertyName:    nameof(MyProp),          // Must match the CLR property name
    returnType:      typeof(string),          // CLR type of the property
    declaringType:   typeof(MyView),          // The class that owns the property
    defaultValue:    string.Empty,            // Default when not set by consumer
    propertyChanged: OnMyPropChanged          // Optional callback (static)
);
```

The **CLR wrapper** (`get`/`set`) must call `GetValue`/`SetValue` — never add
side-effect logic in the wrapper because the XAML binding engine bypasses it
and calls `SetValue` directly.

---

## Step 3 — Consume the ContentView from a Parent Page

### Register the xmlns in the parent page

```xml
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:controls="clr-namespace:MyApp.Controls"
             x:Class="MyApp.Views.ProductPage">
```

### Use the control

```xml
<controls:RatingView Label="Quality"
                     Value="{Binding QualityRating}" />

<controls:RatingView Label="Price"
                     Value="{Binding PriceRating}" />
```

Bindable properties support one-way and two-way data binding just like
built-in MAUI controls. Set `defaultBindingMode: BindingMode.TwoWay` in
`BindableProperty.Create` if the control needs to write values back to the
source.

---

## Step 4 — Advanced: BindableProperty with Validation and Coercion

```csharp
public static readonly BindableProperty ValueProperty =
    BindableProperty.Create(
        propertyName: nameof(Value),
        returnType: typeof(int),
        declaringType: typeof(RatingView),
        defaultValue: 0,
        defaultBindingMode: BindingMode.TwoWay,
        validateValue: (_, v) => (int)v >= 0 && (int)v <= 5,
        coerceValue: (_, v) => Math.Clamp((int)v, 0, 5),
        propertyChanged: OnValueChanged);
```

| Parameter | Purpose |
|---|---|
| `validateValue` | Returns `false` to reject the value and throw `ArgumentException` |
| `coerceValue` | Adjusts the value silently before it is stored |
| `defaultBindingMode` | Controls whether the binding is `OneWay`, `TwoWay`, etc. |

---

## ContentView vs Custom Handler — When to Use Which

| Criteria | ContentView | Custom Handler |
|---|---|---|
| Composition of existing MAUI controls | ✅ Best choice | Overkill |
| Needs platform-native rendering | ❌ Not suitable | ✅ Required |
| Needs to wrap a native SDK view | ❌ Not suitable | ✅ Required |
| Performance-critical drawing | Consider `GraphicsView` | ✅ Better |
| Reusable across projects with no native deps | ✅ Easy to share | More complex |
| Development speed | ✅ Fast | Slower (per-platform code) |

**Rule of thumb:** If you can build it by composing `Label`, `Button`, `Entry`,
`Image`, `Grid`, etc., use a `ContentView`. If you need to render something
that doesn't exist in the MAUI control set or must wrap a platform SDK, create
a custom handler instead (see the `maui-custom-handlers` skill).

---

## Project Structure Example

```
MyApp/
├── Controls/
│   ├── RatingView.xaml            ← new
│   ├── RatingView.xaml.cs         ← new
│   ├── CardView.xaml
│   └── CardView.xaml.cs
├── Views/
│   └── ProductPage.xaml           ← consumes RatingView
├── ViewModels/
│   └── ProductViewModel.cs
└── MauiProgram.cs
```

---

## Quick Checklist

1. **`x:Class`** in XAML matches the fully qualified class name in the code-behind.
2. **`x:Name="this"`** on the root `ContentView` element for self-referencing bindings.
3. **Every public property** exposed to consumers is a `BindableProperty`.
4. **CLR wrappers** only call `GetValue`/`SetValue` — no side effects.
5. **`propertyChanged`** callback is `static`; cast `bindable` to access instance members.
6. **`declaringType`** is the ContentView subclass, not `ContentView` itself.
7. **Consumer page** declares an `xmlns` pointing to the control's namespace.
8. **`.xaml` and `.xaml.cs`** share the same name and directory.

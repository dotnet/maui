---
name: maui-animations
description: >
  .NET MAUI view animations, custom animations, easing functions,
  rotation, scale, translation, and fade effects.
  USE FOR: "animate view", "fade in", "fade out", "slide animation", "scale animation",
  "rotate view", "translate view", "easing function", "custom animation", "animation chaining",
  "ViewExtensions animation".
  DO NOT USE FOR: gesture recognition (use maui-gestures), custom drawing (use maui-graphics-drawing),
  or static layout changes (use maui-data-binding).
---

# .NET MAUI Animations

## Built-in ViewExtensions

All animation methods are extension methods on `VisualElement` and return `Task<bool>` for `await` chaining.

### Methods

| Method | Description |
|---|---|
| `FadeTo(opacity, length, easing)` | Animate `Opacity` |
| `RotateTo(degrees, length, easing)` | Animate `Rotation` |
| `RotateXTo(degrees, length, easing)` | Animate `RotationX` (3D) |
| `RotateYTo(degrees, length, easing)` | Animate `RotationY` (3D) |
| `ScaleTo(scale, length, easing)` | Animate `Scale` uniformly |
| `ScaleXTo(scale, length, easing)` | Animate `ScaleX` |
| `ScaleYTo(scale, length, easing)` | Animate `ScaleY` |
| `TranslateTo(x, y, length, easing)` | Animate `TranslationX`/`TranslationY` |
| `RelScaleTo(delta, length, easing)` | Relative scale increment |
| `RelRotateTo(delta, length, easing)` | Relative rotation increment |

- `length` defaults to 250 ms. `easing` defaults to `Easing.Linear`.
- Call `view.CancelAnimations()` to stop all running animations on that view.

### Composite Animations

```csharp
// Parallel – all run at the same time
await Task.WhenAll(
    view.FadeTo(1, 500),
    view.ScaleTo(1.5, 500),
    view.RotateTo(360, 500));

// Sequential – one after the other
await view.FadeTo(0, 250);
await view.TranslateTo(100, 0, 500);
await view.FadeTo(1, 250);
```

## Custom Animation Class

Use `Animation` for fine-grained control with child animations and timing ratios.

```csharp
var parent = new Animation();

// Child animations with begin/end ratios (0.0–1.0)
parent.Add(0.0, 0.5, new Animation(v => view.Opacity = v, 0, 1));
parent.Add(0.5, 1.0, new Animation(v => view.Scale = v, 1, 2, Easing.SpringOut));

// Commit to run
parent.Commit(
    owner: view,
    name: "MyAnimation",
    rate: 16,        // ms per frame
    length: 1000,    // total duration ms
    easing: Easing.Linear,
    finished: (v, cancelled) => { /* cleanup */ },
    repeat: () => false);  // return true to loop
```

### Constructor

```csharp
new Animation(
    callback: v => view.Scale = v,  // Action<double>
    start: 0.0,
    end: 1.0,
    easing: Easing.CubicInOut);
```

### Cancelling

```csharp
view.AbortAnimation("MyAnimation");
```

> **Gotcha:** Returning `true` from a child animation's `repeat` callback does not repeat the parent animation. Only the `repeat` callback passed to `Commit` on the parent controls parent repetition.

## AnimationExtensions.Animate

Animate any property on any object:

```csharp
view.Animate<double>(
    name: "opacity",
    transform: v => v,       // Func<double, T>
    callback: v => view.Opacity = v,
    rate: 16,
    length: 500,
    easing: Easing.SinInOut,
    finished: (v, cancelled) => { });
```

## Easing Functions

| Easing | Curve |
|---|---|
| `Easing.Linear` | Constant speed |
| `Easing.SinIn` | Smooth accelerate |
| `Easing.SinOut` | Smooth decelerate |
| `Easing.SinInOut` | Smooth both |
| `Easing.CubicIn` | Sharp accelerate |
| `Easing.CubicOut` | Sharp decelerate |
| `Easing.CubicInOut` | Sharp both |
| `Easing.BounceIn` | Bounce at start |
| `Easing.BounceOut` | Bounce at end |
| `Easing.SpringIn` | Spring at start |
| `Easing.SpringOut` | Spring at end |

### Custom Easing

```csharp
var customEase = new Easing(t => t * t * t);
await view.ScaleTo(2, 500, customEase);
```

## Accessibility: Power-Save Mode

Check `VisualElement.IsAnimationEnabled` before running animations. This is `false` when the OS power-save / reduced-motion mode is active.

```csharp
if (view.IsAnimationEnabled)
    await view.FadeTo(1, 500);
else
    view.Opacity = 1;
```

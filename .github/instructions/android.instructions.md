---
applyTo:
  - "**/*.Android.cs"
  - "**/Android/**/*.cs"
  - "**/Platforms/Android/**/*.cs"
  - "**/Platform/Android/**/*.cs"
---

# Android Platform Development Guidelines

Guidance for Android handler implementations and platform code.

## Common Build Issues

### View Namespace Collision

**Problem:** Both MAUI and Android have a `View` type, causing ambiguous reference errors when implementing Android listeners or using Android View APIs.

**Namespace collision between:**
- `Microsoft.Maui.Controls.View` (MAUI's View type)
- `Android.Views.View` (Android's native View type)

**Common symptoms:**
```
error CS0234: The type or namespace name 'Views' does not exist in the namespace 'Android'
error CS0104: 'View' is an ambiguous reference between 'Microsoft.Maui.Controls.View' and 'Android.Views.View'
```

**When this occurs:**
- Implementing Android listeners: `IOnLayoutChangeListener`, `IOnClickListener`, `IOnTouchListener`
- Using Android View APIs directly
- Working with RecyclerView, ViewGroup, or Android layout classes
- Accessing Android view properties or methods

**Solution: Use type aliases**

Add type alias at the top of the file:

```csharp
using AView = Android.Views.View;
```

Then use the alias throughout the file:

```csharp
class LayoutChangeListener : Java.Lang.Object, AView.IOnLayoutChangeListener
{
    public void OnLayoutChange(AView v, int left, int top, int right, int bottom, 
                               int oldLeft, int oldTop, int oldRight, int oldBottom)
    {
        // Use AView to unambiguously refer to Android.Views.View
    }
}
```

**Common type aliases used in MAUI Android code:**

| Alias | Full Type | When to Use |
|-------|-----------|-------------|
| `AView` | `Android.Views.View` | View APIs, listeners |
| `AColor` | `Android.Graphics.Color` | vs `Microsoft.Maui.Graphics.Color` |
| `ALayoutParams` | `Android.Views.ViewGroup.LayoutParams` | Layout parameters |
| `AContext` | `Android.Content.Context` | vs `Microsoft.Maui.ApplicationModel.Context` (rare) |

**Alternative: Use global:: qualifier**

If type alias would conflict with existing code:

```csharp
class MyListener : Java.Lang.Object, global::Android.Views.View.IOnLayoutChangeListener
{
    public void OnLayoutChange(global::Android.Views.View v, ...)
    {
        // Explicit namespace qualification
    }
}
```

## Android Handler Best Practices

### Lifecycle Management

When implementing Android listeners/callbacks in handlers:

1. **Register in ConnectHandler:**
```csharp
protected override void ConnectHandler(RecyclerView platformView)
{
    base.ConnectHandler(platformView);
    _listener = new MyListener(this);
    platformView.AddSomeListener(_listener);
}
```

2. **Unregister in DisconnectHandler:**
```csharp
protected override void DisconnectHandler(RecyclerView platformView)
{
    if (_listener != null)
    {
        platformView.RemoveSomeListener(_listener);
        _listener?.Dispose();
        _listener = null;
    }
    base.DisconnectHandler(platformView);
}
```

3. **Always dispose Java.Lang.Object derivatives** to prevent memory leaks

### Thread Safety

- Android View APIs are **UI-thread-only**
- Handler methods are called on UI thread by default
- If accessing from background thread, use `platformView.Post(() => { })`

## Quick Reference

| Issue | Solution |
|-------|----------|
| View ambiguous reference | Add `using AView = Android.Views.View;` |
| Color ambiguous reference | Add `using AColor = Android.Graphics.Color;` |
| Listener not working | Check lifecycle (register/unregister) |
| Memory leak | Ensure Dispose() called on Java.Lang.Object |
| Threading error | Use `platformView.Post()` for UI thread |

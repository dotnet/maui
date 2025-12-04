---
applyTo: "**/Handlers/**/*.cs, **/*.iOS.cs, **/*.Android.cs, **/*.Windows.cs, **/*.MacCatalyst.cs"
---

# MAUI Handler Development Guardrails

Essential rules for developing MAUI handlers. These are **non-negotiable guardrails** that apply to every handler change.

## Property Mapper Rules

### 1. Always Null-Check PlatformView

```csharp
// ❌ BAD - will crash if PlatformView is null
public static void MapText(IButtonHandler handler, IButton button)
{
    handler.PlatformView.Text = button.Text;
}

// ✅ GOOD - null-safe
public static void MapText(IButtonHandler handler, IButton button)
{
    handler.PlatformView?.UpdateText(button);
}
```

### 2. Guard Against Null Virtual Views

```csharp
// ✅ GOOD - check both handler components
public static void MapText(IButtonHandler handler, IButton button)
{
    if (handler.PlatformView is null || button is null)
        return;
    
    handler.PlatformView.UpdateText(button);
}
```

### 3. Never Throw from Mappers

Mappers should be resilient - log errors but don't throw:

```csharp
// ❌ BAD - exception propagates up
public static void MapImage(IImageHandler handler, IImage image)
{
    var source = image.Source ?? throw new ArgumentNullException(nameof(image.Source));
    // ...
}

// ✅ GOOD - gracefully handle edge cases
public static void MapImage(IImageHandler handler, IImage image)
{
    if (image.Source is null)
    {
        handler.PlatformView?.ClearImage();
        return;
    }
    // ...
}
```

## DisconnectHandler Rules

### 4. Always Unsubscribe Events in DisconnectHandler

Memory leaks occur when event handlers aren't removed:

```csharp
// ❌ BAD - missing event unsubscription
protected override void DisconnectHandler(PlatformView platformView)
{
    base.DisconnectHandler(platformView);
}

// ✅ GOOD - clean up all subscriptions
protected override void DisconnectHandler(PlatformView platformView)
{
    platformView.Click -= OnClick;
    platformView.LongPress -= OnLongPress;
    platformView.Touch -= OnTouch;
    base.DisconnectHandler(platformView);
}
```

### 5. Release Platform Resources

```csharp
protected override void DisconnectHandler(PlatformView platformView)
{
    // Release images/bitmaps
    platformView.SetImageDrawable(null);
    
    // Cancel pending operations
    _loadImageCancellation?.Cancel();
    
    // Unsubscribe events
    platformView.Click -= OnClick;
    
    base.DisconnectHandler(platformView);
}
```

## ConnectHandler Rules

### 6. Defensive Event Subscription

Handlers may be recycled - always unsubscribe before subscribing:

```csharp
// ❌ BAD - may attach multiple times
protected override void ConnectHandler(PlatformView platformView)
{
    platformView.Click += OnClick;
    base.ConnectHandler(platformView);
}

// ✅ GOOD - clean slate
protected override void ConnectHandler(PlatformView platformView)
{
    platformView.Click -= OnClick; // Remove first
    platformView.Click += OnClick; // Then attach
    base.ConnectHandler(platformView);
}
```

## Cross-Platform Consistency

### 7. When Modifying One Platform, Check Others

If you change `ButtonHandler.Android.cs`, verify:
- `ButtonHandler.iOS.cs` doesn't need the same fix
- `ButtonHandler.Windows.cs` doesn't need the same fix
- `ButtonHandler.MacCatalyst.cs` doesn't need the same fix

Run the same test on all platforms when possible.

### 8. Platform-Specific Files Must Implement Same Logic

All platform handlers should produce consistent behavior:

```csharp
// If Android does this:
public static void MapIsEnabled(IButtonHandler handler, IButton button)
{
    handler.PlatformView?.UpdateEnabled(button);
}

// iOS/Windows/Mac should follow the same pattern (with platform-appropriate code)
```

## Async in Handlers

### 9. Never Block on Async

```csharp
// ❌ BAD - deadlock risk
public static void MapImage(IImageHandler handler, IImage image)
{
    handler.LoadImageAsync().Wait();
    handler.LoadImageAsync().GetAwaiter().GetResult();
}

// ✅ GOOD - fire-and-forget with proper handling
public static void MapImage(IImageHandler handler, IImage image)
{
    _ = handler.LoadImageAsync();
}

// ✅ BETTER - use async void for mappers that need await
public static async void MapImage(IImageHandler handler, IImage image)
{
    try
    {
        await handler.LoadImageAsync();
    }
    catch (Exception ex)
    {
        // Log but don't propagate
        Debug.WriteLine($"Image load failed: {ex}");
    }
}
```

### 10. UI Updates Must Be on Main Thread

```csharp
// ✅ Always use MainThread for UI updates in async scenarios
public static async void MapImage(IImageHandler handler, IImage image)
{
    var bitmap = await LoadBitmapAsync();
    
    await MainThread.InvokeOnMainThreadAsync(() =>
    {
        handler.PlatformView?.SetImageBitmap(bitmap);
    });
}
```

## Common Handler Patterns

### Property Mapper Entry

```csharp
public static IPropertyMapper<IButton, IButtonHandler> Mapper = 
    new PropertyMapper<IButton, IButtonHandler>(ViewHandler.ViewMapper)
{
    [nameof(IButton.Text)] = MapText,
    [nameof(IButton.TextColor)] = MapTextColor,
};
```

### Mapper Method Signature

```csharp
// Standard signature - must be static
public static void MapText(IButtonHandler handler, IButton button)
{
    handler.PlatformView?.UpdateText(button);
}
```

### Extension Method for Platform Updates

```csharp
// Keep platform-specific logic in extension methods
public static class ButtonExtensions
{
    public static void UpdateText(this MauiButton platformButton, IButton button)
    {
        platformButton.Text = button.Text ?? string.Empty;
    }
}
```

## Quick Checklist Before PR

- [ ] All mappers null-check `PlatformView`
- [ ] `DisconnectHandler` unsubscribes ALL events subscribed in `ConnectHandler`
- [ ] `ConnectHandler` unsubscribes before subscribing (defensive)
- [ ] Platform resources released in `DisconnectHandler`
- [ ] No `.Wait()` or `.GetAwaiter().GetResult()` in handlers
- [ ] Cross-platform files checked for consistency
- [ ] Error cases handled without throwing from mappers

## See Also

For deeper understanding of handler architecture and trade-offs:
- [MAUI C# Expert Agent](../agents/maui-csharp-expert.agent.md) - Handler philosophy, when to use property vs command mappers, platform differences explained

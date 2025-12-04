---
description: "Common fix patterns for implementing solutions in .NET MAUI code"
---

# Common Fix Patterns

This document contains reusable patterns for implementing fixes in .NET MAUI. Use these as templates when developing solutions.

---

## Table of Contents

- [Null Safety Patterns](#null-safety-patterns)
- [Platform-Specific Code Patterns](#platform-specific-code-patterns)
- [Property Change Patterns](#property-change-patterns)
- [Lifecycle Patterns](#lifecycle-patterns)
- [Threading Patterns](#threading-patterns)
- [Handler Patterns](#handler-patterns)

---

## Null Safety Patterns

### Basic Null Check

**Use when**: Accessing properties or methods that might be null

```csharp
// ❌ BAD: No null check
public void UpdateValue()
{
    Handler.UpdateProperty();  // Crash if Handler is null
}

// ✅ GOOD: Proper null check
public void UpdateValue()
{
    if (Handler is null)
    {
        Console.WriteLine("[WARN] UpdateValue called but Handler is null");
        return;
    }
    
    Handler.UpdateProperty();
}
```

---

### Null-Conditional Operator

**Use when**: Single method call or property access

```csharp
// ❌ BAD: Ignoring possible null
handler.PlatformView!.UpdateSomething();  // ! suppresses warning

// ✅ GOOD: Null-conditional operator
handler.PlatformView?.UpdateSomething();

// ✅ ALSO GOOD: Pattern matching
if (handler.PlatformView is not null)
    handler.PlatformView.UpdateSomething();
```

---

### Null Coalescing

**Use when**: Providing default values

```csharp
// Set default if property is null
var text = control.Text ?? string.Empty;

// Set default if null or whitespace
var title = string.IsNullOrWhiteSpace(control.Title) 
    ? "Default Title" 
    : control.Title;
```

---

### Early Return Pattern

**Use when**: Multiple null checks needed

```csharp
public static void MapFlowDirection(IHandler handler, IView view)
{
    // Early returns for null checks
    if (handler is null) return;
    if (view is null) return;
    if (handler.PlatformView is null) return;
    
    // Actual logic - no nesting needed
    UpdateFlowDirection(handler.PlatformView, view.FlowDirection);
}
```

---

## Platform-Specific Code Patterns

### Platform-Specific Files

**Use when**: Entire implementation differs per platform

**File structure**:
```
Controls/
├── MyControl.cs                    # Shared code
├── MyControl.ios.cs               # iOS + MacCatalyst
├── MyControl.maccatalyst.cs       # MacCatalyst only (rare)
├── MyControl.android.cs           # Android only
└── MyControl.windows.cs           # Windows only
```

**Pattern**:
```csharp
// MyControl.cs - Shared definition
public partial class MyControl : View
{
    public static readonly BindableProperty MyPropertyProperty = 
        BindableProperty.Create(nameof(MyProperty), typeof(string), typeof(MyControl));
    
    public string MyProperty
    {
        get => (string)GetValue(MyPropertyProperty);
        set => SetValue(MyPropertyProperty, value);
    }
    
    // Platform-specific implementation called from shared code
    partial void OnMyPropertyChanged();
    
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        base.OnPropertyChanged(propertyName);
        
        if (propertyName == MyPropertyProperty.PropertyName)
            OnMyPropertyChanged();
    }
}

// MyControl.ios.cs - iOS implementation
#if IOS || MACCATALYST
using UIKit;

namespace Microsoft.Maui.Controls
{
    public partial class MyControl
    {
        partial void OnMyPropertyChanged()
        {
            // iOS-specific implementation
            if (Handler?.PlatformView is UIView view)
            {
                view.AccessibilityLabel = MyProperty;
            }
        }
    }
}
#endif

// MyControl.android.cs - Android implementation
#if ANDROID
using Android.Views;

namespace Microsoft.Maui.Controls
{
    public partial class MyControl
    {
        partial void OnMyPropertyChanged()
        {
            // Android-specific implementation
            if (Handler?.PlatformView is View view)
            {
                view.ContentDescription = MyProperty;
            }
        }
    }
}
#endif
```

**Important**: `.ios.cs` files compile for **both** iOS and MacCatalyst. Use `.maccatalyst.cs` only when behavior must differ from iOS.

---

### Conditional Compilation

**Use when**: Small platform-specific sections within shared files

```csharp
public void UpdateLayout()
{
    // Shared code
    var bounds = CalculateBounds();
    
#if IOS || MACCATALYST
    // iOS-specific code
    if (Handler?.PlatformView is UIKit.UIView view)
    {
        view.SetNeedsLayout();
        view.LayoutIfNeeded();
    }
#elif ANDROID
    // Android-specific code
    if (Handler?.PlatformView is Android.Views.View view)
    {
        view.RequestLayout();
        view.ForceLayout();
    }
#elif WINDOWS
    // Windows-specific code
    if (Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement element)
    {
        element.InvalidateMeasure();
        element.InvalidateArrange();
    }
#endif
    
    // More shared code
}
```

---

### Handler Property Mappers

**Use when**: Mapping cross-platform properties to platform-specific views

```csharp
// In handler file (e.g., CollectionViewHandler.ios.cs)
public static void MapFlowDirection(ICollectionViewHandler handler, ICollectionView collectionView)
{
#if IOS || MACCATALYST
    if (handler.PlatformView is UICollectionView platformView)
    {
        platformView.UpdateFlowDirection(collectionView);
    }
#elif ANDROID
    if (handler.PlatformView is RecyclerView platformView)
    {
        platformView.UpdateFlowDirection(collectionView);
    }
#endif
}

// Extension methods for platform-specific updates
#if IOS || MACCATALYST
internal static void UpdateFlowDirection(this UICollectionView view, ICollectionView collectionView)
{
    var attribute = collectionView.FlowDirection == FlowDirection.RightToLeft
        ? UISemanticContentAttribute.ForceRightToLeft
        : UISemanticContentAttribute.ForceLeftToRight;
    
    view.SemanticContentAttribute = attribute;
}
#endif
```

---

## Property Change Patterns

### Bindable Property with Handler Update

**Use when**: Property changes should trigger handler updates

```csharp
public static readonly BindableProperty MyPropertyProperty = 
    BindableProperty.Create(
        nameof(MyProperty), 
        typeof(string), 
        typeof(MyControl),
        defaultValue: null,
        propertyChanged: OnMyPropertyChanged);

public string MyProperty
{
    get => (string)GetValue(MyPropertyProperty);
    set => SetValue(MyPropertyProperty, value);
}

private static void OnMyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
{
    if (bindable is MyControl control)
    {
        // Notify handler to update platform view
        control.Handler?.UpdateValue(nameof(MyProperty));
    }
}
```

---

### Property Change with Validation

**Use when**: Property values need validation before applying

```csharp
public static readonly BindableProperty MyPropertyProperty = 
    BindableProperty.Create(
        nameof(MyProperty), 
        typeof(int), 
        typeof(MyControl),
        defaultValue: 0,
        validateValue: ValidateMyProperty,
        propertyChanged: OnMyPropertyChanged);

public int MyProperty
{
    get => (int)GetValue(MyPropertyProperty);
    set => SetValue(MyPropertyProperty, value);
}

private static bool ValidateMyProperty(BindableObject bindable, object value)
{
    if (value is int intValue)
    {
        // Validation logic
        return intValue >= 0 && intValue <= 100;
    }
    return false;
}

private static void OnMyPropertyChanged(BindableObject bindable, object oldValue, object newValue)
{
    if (bindable is MyControl control)
    {
        Console.WriteLine($"[DEBUG] MyProperty changed: {oldValue} -> {newValue}");
        control.Handler?.UpdateValue(nameof(MyProperty));
    }
}
```

---

### Coercing Property Values

**Use when**: Property values need adjustment based on other properties

```csharp
public static readonly BindableProperty MaxValueProperty = 
    BindableProperty.Create(
        nameof(MaxValue), 
        typeof(double), 
        typeof(MyControl),
        defaultValue: 100.0,
        propertyChanged: OnMaxValueChanged);

public static readonly BindableProperty CurrentValueProperty = 
    BindableProperty.Create(
        nameof(CurrentValue), 
        typeof(double), 
        typeof(MyControl),
        defaultValue: 0.0,
        coerceValue: CoerceCurrentValue);

public double MaxValue
{
    get => (double)GetValue(MaxValueProperty);
    set => SetValue(MaxValueProperty, value);
}

public double CurrentValue
{
    get => (double)GetValue(CurrentValueProperty);
    set => SetValue(CurrentValueProperty, value);
}

private static void OnMaxValueChanged(BindableObject bindable, object oldValue, object newValue)
{
    // Force CurrentValue to re-coerce when MaxValue changes
    if (bindable is MyControl control)
    {
        control.CoerceValue(CurrentValueProperty);
    }
}

private static object CoerceCurrentValue(BindableObject bindable, object value)
{
    if (bindable is MyControl control && value is double doubleValue)
    {
        // Ensure CurrentValue never exceeds MaxValue
        return Math.Min(doubleValue, control.MaxValue);
    }
    return value;
}
```

---

## Lifecycle Patterns

### Handler Connection

**Use when**: Setting up platform view when handler connects

```csharp
protected override void ConnectHandler(PlatformView platformView)
{
    base.ConnectHandler(platformView);
    
    Console.WriteLine($"[LIFECYCLE] Handler connected: {platformView.GetType().Name}");
    
    // Subscribe to platform events
    platformView.SomeEvent += OnPlatformEvent;
    
    // Initial setup
    UpdatePlatformView(platformView);
}
```

---

### Handler Disconnection with Cleanup

**Use when**: Cleaning up when handler disconnects

```csharp
protected override void DisconnectHandler(PlatformView platformView)
{
    Console.WriteLine("[LIFECYCLE] Handler disconnecting");
    
    if (platformView is not null)
    {
        // Unsubscribe from events
        platformView.SomeEvent -= OnPlatformEvent;
        
        // Clear references
        _cachedReference = null;
        
        // Platform-specific cleanup
#if IOS || MACCATALYST
        if (platformView is UIView uiView)
        {
            uiView.RemoveFromSuperview();
        }
#elif ANDROID
        if (platformView is Android.Views.View androidView)
        {
            androidView.Dispose();
        }
#endif
    }
    
    base.DisconnectHandler(platformView);
}
```

---

### Control Disposal

**Use when**: Implementing IDisposable pattern

```csharp
public class MyControl : View, IDisposable
{
    private bool _disposed;
    
    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        
        if (disposing)
        {
            // Dispose managed resources
            Handler?.DisconnectHandler();
            _managedResource?.Dispose();
        }
        
        // Free unmanaged resources (if any)
        
        _disposed = true;
    }
    
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
```

---

## Threading Patterns

### Dispatch to Main Thread

**Use when**: Updating UI from background thread

```csharp
public async Task UpdateFromBackgroundAsync()
{
    // Background work
    var data = await FetchDataAsync();
    
    // Switch to main thread for UI update
    Dispatcher.Dispatch(() =>
    {
        MyProperty = data;
        Console.WriteLine("[UI] Property updated on main thread");
    });
}
```

---

### Delayed Dispatch

**Use when**: Waiting for layout or animation completion

```csharp
private void OnLoaded(object sender, EventArgs e)
{
    // Wait for layout to complete
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine($"[DEBUG] Final bounds: {this.Bounds}");
        ValidateLayout();
    });
}
```

---

### Async Property Setter

**Use when**: Property change triggers async operation

```csharp
private string _imageSource;
private bool _isLoadingImage;

public string ImageSource
{
    get => _imageSource;
    set
    {
        if (_imageSource == value)
            return;
        
        _imageSource = value;
        OnPropertyChanged();
        
        // Trigger async load without blocking
        _ = LoadImageAsync(value);
    }
}

private async Task LoadImageAsync(string source)
{
    if (_isLoadingImage)
        return;
    
    _isLoadingImage = true;
    
    try
    {
        var image = await DownloadImageAsync(source);
        
        // Update UI on main thread
        Dispatcher.Dispatch(() =>
        {
            DisplayImage(image);
        });
    }
    finally
    {
        _isLoadingImage = false;
    }
}
```

---

## Handler Patterns

### Registering Property Mappers

**Use when**: Creating or modifying handlers

```csharp
public partial class MyControlHandler : ViewHandler<IMyControl, PlatformView>
{
    public static IPropertyMapper<IMyControl, MyControlHandler> Mapper = 
        new PropertyMapper<IMyControl, MyControlHandler>(ViewHandler.ViewMapper)
        {
            [nameof(IMyControl.MyProperty)] = MapMyProperty,
            [nameof(IMyControl.AnotherProperty)] = MapAnotherProperty,
        };
    
    public MyControlHandler() : base(Mapper)
    {
    }
    
    public static void MapMyProperty(MyControlHandler handler, IMyControl view)
    {
        handler.PlatformView?.UpdateMyProperty(view);
    }
    
    public static void MapAnotherProperty(MyControlHandler handler, IMyControl view)
    {
        handler.PlatformView?.UpdateAnotherProperty(view);
    }
}
```

---

### Modifying Existing Handler

**Use when**: Adding or overriding mappings in existing handlers

```csharp
// In MauiProgram.cs or handler configuration
public static MauiAppBuilder ConfigureHandlers(this MauiAppBuilder builder)
{
    builder.ConfigureMauiHandlers(handlers =>
    {
        // Add new mapping
        handlers.AddHandler<CollectionView, CollectionViewHandler>();
        
        // Modify existing mapping
        CollectionViewHandler.Mapper.AppendToMapping(
            nameof(ICollectionView.FlowDirection), 
            (handler, view) =>
            {
                // Custom FlowDirection handling
                handler.PlatformView?.UpdateFlowDirection(view);
            });
    });
    
    return builder;
}
```

---

### Command Mapper Pattern

**Use when**: Handling imperative operations (not property changes)

```csharp
public static IPropertyMapper<IMyControl, MyControlHandler> Mapper = 
    new PropertyMapper<IMyControl, MyControlHandler>(ViewHandler.ViewMapper)
    {
        // ... property mappings ...
    };

public static CommandMapper<IMyControl, MyControlHandler> CommandMapper = 
    new CommandMapper<IMyControl, MyControlHandler>(ViewHandler.ViewCommandMapper)
    {
        [nameof(IMyControl.ScrollTo)] = MapScrollTo,
        [nameof(IMyControl.Refresh)] = MapRefresh,
    };

public static void MapScrollTo(MyControlHandler handler, IMyControl view, object? args)
{
    if (args is ScrollToArgs scrollArgs)
    {
        handler.PlatformView?.ScrollToItem(scrollArgs.Index, scrollArgs.Animated);
    }
}
```

---

## When to Use Which Pattern

| Scenario | Pattern | Reference |
|----------|---------|-----------|
| Accessing nullable property | [Null Safety](#null-safety-patterns) | Basic Null Check |
| Platform-specific method call | [Platform-Specific](#platform-specific-code-patterns) | Conditional Compilation |
| Entire platform-specific file | [Platform-Specific](#platform-specific-code-patterns) | Platform-Specific Files |
| Property changes need handler update | [Property Change](#property-change-patterns) | Bindable Property |
| Setup when handler connects | [Lifecycle](#lifecycle-patterns) | Handler Connection |
| Cleanup when handler disconnects | [Lifecycle](#lifecycle-patterns) | Handler Disconnection |
| UI update from background | [Threading](#threading-patterns) | Dispatch to Main Thread |
| Wait for layout completion | [Threading](#threading-patterns) | Delayed Dispatch |
| Map property to platform | [Handler](#handler-patterns) | Property Mappers |
| Execute platform command | [Handler](#handler-patterns) | Command Mapper |

---

## Related Documentation

- [Instrumentation Guide](../instrumentation.md) - Adding debug logging to patterns
- [Solution Development](../issue-resolver-agent/solution-development.md) - Applying patterns to fixes
- [Platform Workflows](platform-workflows.md) - Testing your fixes

---

**Last Updated**: November 2025

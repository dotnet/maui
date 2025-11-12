---
description: "Guidelines for instrumenting .NET MAUI source code for debugging, testing, and validating changes"
---

# .NET MAUI Instrumentation Guide

This guide provides patterns and techniques for adding instrumentation to .NET MAUI source code to debug issues, validate PR changes, and understand runtime behavior.

**Common Command Patterns**: For UDID extraction, device boot, builds, and error checking patterns, see [Common Testing Patterns](common-testing-patterns.md).

**Guiding Principle: Use cross-platform MAUI APIs for all instrumentation.**

## When to Use Instrumentation

- **Validating PR changes**: Measure actual behavior before/after changes
- **Debugging layout issues**: Capture frame positions, sizes, margins
- **Testing behavior**: Verify view properties and state
- **Performance analysis**: Measure timing, allocations, render cycles
- **Understanding code flow**: Trace execution paths and state changes

## Quick Start: Sandbox App Instrumentation

The recommended approach is to use the Sandbox app (`src/Controls/samples/Controls.Sample.Sandbox`) for instrumentation:

1. Modify `MainPage.xaml` to reproduce the scenario
2. Add instrumentation code to `MainPage.xaml.cs`
3. Build and deploy to simulator/device
4. Capture console output
5. Revert changes when done

## Cross-Platform Instrumentation (Preferred Method)

### Measuring Size

**Use `SizeChanged` events for size measurements:**

```csharp
public MainPage()
{
    InitializeComponent();
    
    // Subscribe to size changes
    if (Content is View view)
    {
        view.SizeChanged += async (s, e) =>
        {
            // Wait for layout pass to complete
            await Task.Delay(1);
            
            Console.WriteLine("========== VIEW INFO ==========");
            Console.WriteLine($"Size: W={view.Width}, H={view.Height}");
            Console.WriteLine($"Bounds: {view.Bounds}");
            Console.WriteLine($"Margin: {view.Margin}");
            
            if (view is Layout layout)
            {
                Console.WriteLine($"Padding: {layout.Padding}");
            }
            
            Console.WriteLine("================================");
        };
    }
}
```

### Getting Absolute Screen Position

To get the absolute position of an element on the screen, you need to access the platform view and use platform-specific APIs.

**iOS/MacCatalyst:**

```csharp
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange
    
    #if IOS || MACCATALYST
    if (view.Handler?.PlatformView is UIKit.UIView platformView)
    {
        // Find the root superview
        var superview = platformView;
        while (superview.Superview != null)
        {
            superview = superview.Superview;
        }
        
        // Convert the view's bounds to the root coordinate system
        var convertedRect = platformView.ConvertRectToView(platformView.Bounds, superview);
        
        Console.WriteLine($"Screen Position: X={convertedRect.X}, Y={convertedRect.Y}");
        Console.WriteLine($"Screen Size: W={convertedRect.Width}, H={convertedRect.Height}");
    }
    #endif
};
```

**Android:**

```csharp
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange
    
    #if ANDROID
    if (view.Handler?.PlatformView is Android.Views.View platformView)
    {
        int[] location = new int[2];
        platformView.GetLocationOnScreen(location);
        
        Console.WriteLine($"Screen Position: X={location[0]}, Y={location[1]}");
        Console.WriteLine($"Screen Size: W={platformView.Width}, H={platformView.Height}");
    }
    #endif
};
```

**Windows:**

```csharp
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange
    
    #if WINDOWS
    if (view.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement platformElement)
    {
        var transform = platformElement.TransformToVisual(null);
        var point = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
        
        Console.WriteLine($"Screen Position: X={point.X}, Y={point.Y}");
        Console.WriteLine($"Screen Size: W={platformElement.ActualWidth}, H={platformElement.ActualHeight}");
    }
    #endif
};
```

**Monitoring Specific Named Elements:**

```csharp
// In XAML: <Label x:Name="MyLabel" Text="Test" />
// In code-behind:
MyLabel.SizeChanged += async (s, e) =>
{
    // Wait for layout pass to complete
    await Task.Delay(1);
    
    Console.WriteLine($"Label: W={MyLabel.Width}, H={MyLabel.Height}");
    Console.WriteLine($"Label Bounds: {MyLabel.Bounds}");
};
```

### Traversing the Visual Tree

**Use `IVisualTreeElement` to walk the MAUI element hierarchy:**

```csharp
using Microsoft.Maui.Controls;

// Walk down the tree
void LogVisualChildren(IVisualTreeElement element, int depth = 0)
{
    var indent = new string(' ', depth * 2);
    Console.WriteLine($"{indent}{element.GetType().Name}");
    
    if (element is View view)
    {
        Console.WriteLine($"{indent}  Size: W={view.Width}, H={view.Height}");
        Console.WriteLine($"{indent}  Margin: {view.Margin}");
    }
    
    // Recurse into children
    foreach (var child in element.GetVisualChildren())
    {
        LogVisualChildren(child, depth + 1);
    }
}

// Usage - call after layout completes
async void OnSizeChanged(object sender, EventArgs e)
{
    await Task.Delay(1); // Wait for layout pass
    LogVisualChildren(this);
}
```

**Walking Up the Tree:**

```csharp
// Get parent elements
var current = myView as IVisualTreeElement;
while (current != null)
{
    Console.WriteLine($"Parent: {current.GetType().Name}");
    
    if (current is View view)
    {
        Console.WriteLine($"  Size: W={view.Width}, H={view.Height}");
    }
    
    current = current.GetVisualParent();
}
```

**Finding Specific Elements:**

```csharp
// Find all Labels in the visual tree
void FindAllLabels(IVisualTreeElement element)
{
    if (element is Label label)
    {
        Console.WriteLine($"Found Label: '{label.Text}' at W={label.Width}, H={label.Height}");
    }
    
    foreach (var child in element.GetVisualChildren())
    {
        FindAllLabels(child);
    }
}

// Usage
FindAllLabels(this);
```

## Performance Instrumentation

### Timing Measurements

```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

// Code to measure

stopwatch.Stop();
Console.WriteLine($"Operation took: {stopwatch.ElapsedMilliseconds}ms");
```

### Layout Performance

```csharp
private int _layoutCount = 0;

protected override void OnSizeAllocated(double width, double height)
{
    base.OnSizeAllocated(width, height);
    _layoutCount++;
    Console.WriteLine($"Layout #{_layoutCount}: W={width}, H={height}");
}
```

## Testing Workflow with Instrumentation

### Step 1: Add Instrumentation to Sandbox App

Edit `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs`:

```csharp
public MainPage()
{
    InitializeComponent();
    
    // Monitor content size changes
    if (Content is View view)
    {
        view.SizeChanged += LogLayoutInfo;
    }
}

private async void LogLayoutInfo(object? sender, EventArgs e)
{
    if (sender is View view)
    {
        // Wait for layout pass to complete
        await Task.Delay(1);
        
        Console.WriteLine("========== LAYOUT INFO ==========");
        Console.WriteLine($"Size: W={view.Width}, H={view.Height}");
        Console.WriteLine($"Bounds: {view.Bounds}");
        Console.WriteLine($"Margin: {view.Margin}");
        Console.WriteLine("=================================");
    }
}
```

### Step 2: Build and Deploy

**iOS:**
```bash
# Build
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios

# Check build succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build failed"
    exit 1
fi

# Find iPhone Xs with highest iOS version
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# Check UDID was found
if [ -z "$UDID" ] || [ "$UDID" = "null" ]; then
    echo "❌ ERROR: No iPhone Xs simulator found. Please create one."
    exit 1
fi

# Boot and install
xcrun simctl boot $UDID 2>/dev/null || true

# Check simulator is booted
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
if [ "$STATE" != "Booted" ]; then
    echo "❌ ERROR: Simulator failed to boot. Current state: $STATE"
    exit 1
fi

xcrun simctl install $UDID artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app

# Check install succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: App installation failed"
    exit 1
fi

# Launch with console capture
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/ios_output.log 2>&1 &

# Check launch didn't immediately fail
if [ $? -ne 0 ]; then
    echo "❌ ERROR: App launch failed"
    exit 1
fi

sleep 5
cat /tmp/ios_output.log
```

**Android:**
```bash
# Build and deploy
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run

# Check build/deploy succeeded
if [ $? -ne 0 ]; then
    echo "❌ ERROR: Build or deployment failed"
    exit 1
fi

# Monitor logcat
adb logcat | grep "LAYOUT INFO"
```

### Step 3: Compare Before/After

1. Test WITHOUT PR changes (checkout main branch for changed file)
2. Capture baseline output
3. Test WITH PR changes
4. Capture new output
5. Compare the results

### Step 4: Clean Up

```bash
# Revert all Sandbox changes
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/
```

## Best Practices

### Wait for Layout to Complete

✅ **Do**: Use `await Task.Delay(1)` in SizeChanged for accurate measurements
```csharp
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for platform arrange to complete
    Console.WriteLine($"Size: {view.Width}x{view.Height}");
    // Now platform view positions are also accurate
};
```

❌ **Don't**: Read measurements immediately in SizeChanged
```csharp
view.SizeChanged += (s, e) =>
{
    // Platform arrange hasn't completed yet!
    Console.WriteLine($"Size: {view.Width}x{view.Height}");
};
```

**Why:** `SizeChanged` fires before the platform's arrange call completes. Wait for the layout pass to finish to get accurate measurements.

### Use Visual Tree APIs for Hierarchy Inspection

✅ **Do**: Use `IVisualTreeElement` methods
```csharp
foreach (var child in element.GetVisualChildren()) { ... }
var parent = element.GetVisualParent();
```

### Use SizeChanged Events (Not Arbitrary Delays)

✅ **Do**: Use `SizeChanged` with `Task.Delay(1)` for layout completion
```csharp
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange
    // Now measurements are accurate
};
```

❌ **Don't**: Use arbitrary delays without SizeChanged
```csharp
// Bad: Guessing when layout completes
await Task.Delay(500); // Unreliable: may not match actual layout timing
// Measure here (may be inaccurate)
```

**Why use SizeChanged:** It fires when the view changes, ensuring you measure at the right time. The `Task.Delay(1)` ensures the platform arrange completes.

### Console Output Formatting

```csharp
// Good: Clear, structured output
Console.WriteLine("=== Frame Info ===");
Console.WriteLine($"X={view.Bounds.X}, Y={view.Bounds.Y}, W={view.Width}, H={view.Height}");
Console.WriteLine("==================");

// Bad: Unclear, hard to parse
Console.WriteLine(view.ToString());
```

### Avoid Common Mistakes

❌ **Don't** read position immediately in SizeChanged
✅ **Do** use `await Task.Delay(1)` to wait for arrange

❌ **Don't** use arbitrary delays without SizeChanged
✅ **Do** subscribe to `SizeChanged` with proper delay

❌ **Don't** instrument directly in MAUI source code (unless debugging MAUI itself)
✅ **Do** use Sandbox app for testing

❌ **Don't** commit instrumentation code
✅ **Do** revert changes after testing

## Advanced Techniques

### Capturing Screenshots (iOS)

```bash
# After launching app
xcrun simctl io $UDID screenshot /tmp/screenshot.png
```

### Monitoring Property Changes

```csharp
view.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == "Bounds")
    {
        Console.WriteLine($"Bounds changed: {view.Bounds}");
    }
};
```

## Troubleshooting

### "SizeChanged not firing"

Ensure the view is actually changing size or trigger it manually:
```csharp
// Force a layout
view.InvalidateMeasure();

// Or check if size is already set
if (view.Width > 0 && view.Height > 0)
{
    Console.WriteLine($"Current size: {view.Width}x{view.Height}");
}
```

### "Console.WriteLine not showing"

**iOS:** Use `xcrun simctl launch --console-pty` to capture output
**Android:** Use `adb logcat` to view logs
**Windows:** Output appears in Visual Studio Output window

### "Width/Height are -1"

Subscribe to SizeChanged with proper delay:
```csharp
// Wrong - might be -1 if layout hasn't run
Console.WriteLine($"Width: {view.Width}"); // Could be -1

// Right - wait for size and layout
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange
    Console.WriteLine($"Width: {view.Width}"); // Actual value after layout
};
```

## Related Documentation

- `.github/instructions/safearea-testing.instructions.md` - **Specialized guide for SafeArea testing** (measure children, not parents)
- `docs/DevelopmentTips.md` - General development tips including Sandbox usage
- `docs/UITesting-Guide.md` - UI testing patterns
- `.github/instructions/uitests.instructions.md` - UI test creation guidelines

## Quick Reference

**Measure size:**
```csharp
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange to complete
    Console.WriteLine($"Size: {view.Width}x{view.Height}");
    Console.WriteLine($"Bounds: {view.Bounds}");
};
```

**Get screen position (platform-specific):**
```csharp
#if IOS || MACCATALYST
var platformView = view.Handler?.PlatformView as UIKit.UIView;
var superview = platformView;
while (superview?.Superview != null) superview = superview.Superview;
var rect = platformView.ConvertRectToView(platformView.Bounds, superview);
Console.WriteLine($"Position: X={rect.X}, Y={rect.Y}");
#elif ANDROID
var platformView = view.Handler?.PlatformView as Android.Views.View;
int[] location = new int[2];
platformView.GetLocationOnScreen(location);
Console.WriteLine($"Position: X={location[0]}, Y={location[1]}");
#elif WINDOWS
var platformElement = view.Handler?.PlatformView as Microsoft.UI.Xaml.FrameworkElement;
var transform = platformElement.TransformToVisual(null);
var point = transform.TransformPoint(new Windows.Foundation.Point(0, 0));
Console.WriteLine($"Position: X={point.X}, Y={point.Y}");
#endif
```

**Walk visual tree:**
```csharp
foreach (var child in element.GetVisualChildren()) { /* ... */ }
var parent = element.GetVisualParent();
```

**Capture output:**
- iOS: `xcrun simctl launch --console-pty ... > /tmp/output.log 2>&1`
- Android: `adb logcat | grep "YOUR MARKER"`

**Clean up:**
```bash
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/
```

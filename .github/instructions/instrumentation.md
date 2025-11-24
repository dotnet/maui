---
description: "Comprehensive guide for instrumenting .NET MAUI apps for debugging, testing, and validation"
---

# .NET MAUI Instrumentation Guide

This guide provides comprehensive patterns and techniques for adding instrumentation to .NET MAUI apps to debug issues, validate changes, and understand runtime behavior.

**Target App**: Use the Sandbox app (`src/Controls/samples/Controls.Sample.Sandbox/`) for all testing and instrumentation.

**Common Commands**: For build, deploy, and error handling patterns, see [Common Testing Patterns](common-testing-patterns.md).

---

## Table of Contents

1. [When to Use Instrumentation](#when-to-use-instrumentation)
2. [Quick Start: Basic Pattern](#quick-start-basic-pattern)
3. [Key Instrumentation Techniques](#key-instrumentation-techniques)
4. [Common Instrumentation Patterns](#common-instrumentation-patterns)
5. [Advanced Techniques](#advanced-techniques)
6. [Platform-Specific Positioning](#platform-specific-positioning)
7. [Best Practices](#best-practices)
8. [Capturing Output](#capturing-output)
9. [Troubleshooting](#troubleshooting)
10. [Quick Reference](#quick-reference)

---

## When to Use Instrumentation

**✅ Use instrumentation for:**
- Reproducing reported issues
- Validating PR changes and fixes
- Debugging layout and positioning issues
- Testing control behavior and properties
- Performance analysis and timing measurements
- Understanding code execution flow

**✅ Use Sandbox app for:**
- Manual testing and reproduction
- Quick experimentation
- PR validation
- Issue investigation

**❌ Don't use Sandbox app for:**
- Writing automated UI tests (use TestCases.HostApp instead)
- Running automated test suites
- CI/CD validation

---

## Quick Start: Basic Pattern

### Step 1: Create Visual Test Scenario (XAML)

Edit `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml`:

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage"
             Title="Issue #XXXXX Test">

    <!-- Use colored backgrounds for visual debugging -->
    <Grid x:Name="RootGrid" BackgroundColor="Red">

        <!-- The element you're testing -->
        <ContentView x:Name="TestElement"
                     BackgroundColor="Yellow"
                     Loaded="OnLoaded">
            <Label Text="Test Content"
                   x:Name="ContentLabel"/>
        </ContentView>
    </Grid>
</ContentPage>
```

**Key XAML patterns:**
- Use `x:Name` on elements you'll reference in code
- Use contrasting `BackgroundColor` values for visual debugging (red parent, yellow child)
- Hook into `Loaded` event for measurement timing
- Keep layout simple and focused on the issue

### Step 2: Add Instrumentation (Code-Behind)

Edit `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs`:

```csharp
using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnLoaded(object sender, EventArgs e)
        {
            // Wait for layout to complete
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
            {
                CaptureState("OnLoaded");
            });
        }

        private void CaptureState(string context)
        {
            Console.WriteLine($"=== TEST OUTPUT: {context} ===");

            // Basic measurements
            Console.WriteLine($"Element Bounds: {TestElement.Bounds}");
            Console.WriteLine($"Element Width: {TestElement.Width}, Height: {TestElement.Height}");

            // Child content measurements
            if (ContentLabel != null)
            {
                Console.WriteLine($"Content Bounds: {ContentLabel.Bounds}");
                Console.WriteLine($"Content Position: X={ContentLabel.X}, Y={ContentLabel.Y}");
            }

            // Screen dimensions
            var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
            var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
            Console.WriteLine($"Screen Size: {screenWidth}x{screenHeight}");

            Console.WriteLine("=== END TEST OUTPUT ===\n");
        }
    }
}
```

### Step 3: Build, Deploy, and Capture

**⚡ Option A: Use BuildAndRunSandbox.ps1 Script (Recommended)**

When using with Appium for UI interaction:

```powershell
# 1. Create Appium test script with your instrumentation logic
cp .github/scripts/templates/RunWithAppiumTest.template.cs SandboxAppium/RunWithAppiumTest.cs
# Edit RunWithAppiumTest.cs to add your test logic

# 2. Run everything with one command
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform [android|ios]
```

The script handles device detection, building, deployment, Appium management, and log capture. See [Common Testing Patterns](common-testing-patterns.md) for details.

---

**Option B: Manual Build and Capture** (for console-only instrumentation without Appium):

**iOS**:
```bash
# Get UDID, boot, build, install, launch
UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')
xcrun simctl boot $UDID 2>/dev/null || true
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios
xcrun simctl install $UDID artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/output.log 2>&1 &
sleep 8 && cat /tmp/output.log
```

**Android**:
```bash
export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)
dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run
adb logcat | grep "TEST OUTPUT"
```

### Step 4: Clean Up

```bash
# Always revert Sandbox changes when done
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/
```

---

## Key Instrumentation Techniques

### 1. Console Output with Markers

**Why**: Output is captured in device logs and easy to search

```csharp
Console.WriteLine("=== TEST OUTPUT ===");  // Marker for easy searching
Console.WriteLine($"Value: {someValue}");
Console.WriteLine("=== END TEST OUTPUT ===");
```

**With unique markers for filtering:**
```csharp
Console.WriteLine($"[ISSUE-12345] FlowDirection changed to {value}");
Console.WriteLine($"[ISSUE-12345] Measurements: {width}x{height}");

// Later, filter logs:
// iOS: xcrun simctl spawn booted log stream | grep "ISSUE-12345"
// Android: adb logcat | grep "ISSUE-12345"
```

### 2. Timing: When to Measure

Two approaches depending on your scenario:

#### Approach A: Loaded Event (Simple, One-Time Measurements)

**Best for**: Initial state capture, one-time measurements, Sandbox app testing

```csharp
private void OnLoaded(object sender, EventArgs e)
{
    // Wait for layout pass to complete
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        // Now measure - layout is guaranteed complete
        CaptureState("AfterLoad");
    });
}
```

**Why this works:**
- ✅ Simpler code for one-time measurements
- ✅ Reliable for initial state capture
- ✅ No async complexity needed
- ✅ Works consistently across platforms

#### Approach B: SizeChanged Event (Dynamic, Event-Driven)

**Best for**: Dynamic scenarios, property changes, animations, responsive layouts

```csharp
public MainPage()
{
    InitializeComponent();

    // Subscribe to size changes
    if (Content is View view)
    {
        view.SizeChanged += async (s, e) =>
        {
            // Wait for platform arrange to complete
            await Task.Delay(1);

            Console.WriteLine("========== VIEW INFO ==========");
            Console.WriteLine($"Size: W={view.Width}, H={view.Height}");
            Console.WriteLine($"Bounds: {view.Bounds}");
            Console.WriteLine("================================");
        };
    }
}
```

**Why this works:**
- ✅ Fires on actual size changes
- ✅ Precise timing (1ms wait for platform arrange)
- ✅ Captures dynamic property changes
- ✅ Event-driven, not arbitrary delays

**When to use which:**
- **Loaded + 500ms**: Sandbox app testing, issue reproduction, PR validation
- **SizeChanged + 1ms**: Dynamic behavior testing, animation debugging, property change testing

### 3. Measure Child Elements (Critical for SafeArea)

**CRITICAL**: For SafeArea and padding tests, measure CHILD content position, not parent container size

```csharp
// ❌ WRONG: Measuring parent (size stays constant)
Console.WriteLine($"Container size: {RootGrid.Width}x{RootGrid.Height}");

// ✅ CORRECT: Measuring child position (shows padding effect)
Console.WriteLine($"Child Y: {ContentLabel.Y}");
Console.WriteLine($"Gap from top: {ContentLabel.Y}");
```

See [SafeArea Testing Instructions](safearea-testing.md) for detailed SafeArea testing patterns.

### 4. Calculate Gaps from Screen Edges

**Why**: Essential for SafeArea and positioning validation

```csharp
private void CapturePositionData()
{
    var element = ContentLabel;

    // Get screen dimensions
    var screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
    var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;

    // Calculate gaps from edges
    double topGap = element.Y;
    double bottomGap = screenHeight - (element.Y + element.Height);
    double leftGap = element.X;
    double rightGap = screenWidth - (element.X + element.Width);

    Console.WriteLine($"Top Gap: {topGap}");
    Console.WriteLine($"Bottom Gap: {bottomGap}");
    Console.WriteLine($"Left Gap: {leftGap}");
    Console.WriteLine($"Right Gap: {rightGap}");
}
```

### 5. Color Code Elements for Visual Debugging

**Why**: Makes visual inspection easier, especially for SafeArea issues

```xml
<!-- Visual hierarchy with colors -->
<Grid BackgroundColor="Red">               <!-- Parent -->
    <ContentView BackgroundColor="Yellow">  <!-- Middle -->
        <Label BackgroundColor="Green"/>    <!-- Child -->
    </ContentView>
</Grid>
```

When SafeArea padding is applied, you'll see the red background where gaps appear.

---

## Common Instrumentation Patterns

### Pattern 1: Property Change Testing

Test property toggles multiple times to catch timing/race conditions:

```csharp
private async void OnLoaded(object sender, EventArgs e)
{
    Console.WriteLine("=== TEST: Property Toggle ===");

    for (int i = 0; i < 5; i++)
    {
        TestElement.FlowDirection = FlowDirection.RightToLeft;
        await Task.Delay(500);
        Console.WriteLine($"Iteration {i}: RTL Bounds = {TestElement.Bounds}");

        TestElement.FlowDirection = FlowDirection.LeftToRight;
        await Task.Delay(500);
        Console.WriteLine($"Iteration {i}: LTR Bounds = {TestElement.Bounds}");
    }

    Console.WriteLine("=== END TEST ===");
}
```

### Pattern 2: Nested Content Measurement

Measure parent-child relationships:

```csharp
private void OnLoaded(object sender, EventArgs e)
{
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine("=== TEST: Parent and Child ===");
        Console.WriteLine($"Parent Bounds: {RootGrid.Bounds}");
        Console.WriteLine($"Parent Position: X={RootGrid.X}, Y={RootGrid.Y}");

        Console.WriteLine($"Child Bounds: {ContentLabel.Bounds}");
        Console.WriteLine($"Child Position: X={ContentLabel.X}, Y={ContentLabel.Y}");

        // Calculate child position relative to parent
        var relativeX = ContentLabel.X - RootGrid.X;
        var relativeY = ContentLabel.Y - RootGrid.Y;
        Console.WriteLine($"Child relative to parent: X={relativeX}, Y={relativeY}");
        Console.WriteLine("=== END TEST ===");
    });
}
```

### Pattern 3: Collection/List Testing

Add large data sets for scrolling and performance tests:

```csharp
public MainPage()
{
    InitializeComponent();

    // Large data set for performance/scrolling test
    var items = Enumerable.Range(1, 100).Select(i => $"Item {i}").ToList();
    TestCollectionView.ItemsSource = items;
}

private void OnLoaded(object sender, EventArgs e)
{
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        var collection = TestCollectionView;
        var count = collection.ItemsSource?.Cast<object>().Count() ?? 0;
        Console.WriteLine($"=== TEST: {count} items loaded ===");
        Console.WriteLine($"Collection Bounds: {collection.Bounds}");
    });
}
```

### Pattern 4: Event Sequence Tracking

Track when events fire and in what order:

```csharp
private int _eventCount = 0;

private void OnSomeEvent(object sender, EventArgs e)
{
    _eventCount++;
    Console.WriteLine($"=== Event #{_eventCount} at {DateTime.Now:HH:mm:ss.fff} ===");
    Console.WriteLine($"Sender: {sender?.GetType().Name}");

    // Capture state at event time
    CaptureState($"Event{_eventCount}");
}
```

### Pattern 5: Timing Measurements

Measure how long operations take:

```csharp
private async void OnButtonClicked(object sender, EventArgs e)
{
    var sw = System.Diagnostics.Stopwatch.StartNew();

    // Operation being measured
    await SomeAsyncOperation();

    sw.Stop();
    Console.WriteLine($"Operation took: {sw.ElapsedMilliseconds}ms");
}
```

### Pattern 6: Before/After Comparison

Compare behavior before and after your changes:

```csharp
private void TestScenario()
{
    Console.WriteLine("=== BEFORE STATE ===");
    CaptureState("Before");

    // Make change
    TestElement.SomeProperty = newValue;

    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine("=== AFTER STATE ===");
        CaptureState("After");
    });
}
```

---

## Advanced Techniques

### Visual Tree Traversal

Use `IVisualTreeElement` to walk the MAUI element hierarchy:

#### Walking Down the Tree

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
private void OnLoaded(object sender, EventArgs e)
{
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        LogVisualChildren(this);
    });
}
```

#### Walking Up the Tree

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

#### Finding Specific Elements

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

### Performance Instrumentation

#### Timing Measurements

```csharp
var stopwatch = System.Diagnostics.Stopwatch.StartNew();

// Code to measure

stopwatch.Stop();
Console.WriteLine($"Operation took: {stopwatch.ElapsedMilliseconds}ms");
```

#### Layout Performance

```csharp
private int _layoutCount = 0;

protected override void OnSizeAllocated(double width, double height)
{
    base.OnSizeAllocated(width, height);
    _layoutCount++;
    Console.WriteLine($"Layout #{_layoutCount}: W={width}, H={height}");
}
```

### Property Change Monitoring

```csharp
view.PropertyChanged += (s, e) =>
{
    if (e.PropertyName == "Bounds")
    {
        Console.WriteLine($"Bounds changed: {view.Bounds}");
    }
};
```

---

## Platform-Specific Positioning

Sometimes you need the absolute screen position, not just relative bounds. Use platform-specific APIs:

### iOS/MacCatalyst

```csharp
#if IOS || MACCATALYST
if (TestElement.Handler?.PlatformView is UIKit.UIView platformView)
{
    // Find the root superview (window)
    var rootView = platformView;
    while (rootView.Superview != null)
    {
        rootView = rootView.Superview;
    }

    // Convert view bounds to root coordinate system
    var screenRect = platformView.ConvertRectToView(platformView.Bounds, rootView);

    Console.WriteLine($"Screen Position: X={screenRect.X}, Y={screenRect.Y}");
    Console.WriteLine($"Screen Size: W={screenRect.Width}, H={screenRect.Height}");
}
#endif
```

### Android

```csharp
#if ANDROID
if (TestElement.Handler?.PlatformView is Android.Views.View platformView)
{
    int[] location = new int[2];
    platformView.GetLocationOnScreen(location);

    Console.WriteLine($"Screen Position: X={location[0]}, Y={location[1]}");
    Console.WriteLine($"Screen Size: W={platformView.Width}, H={platformView.Height}");
}
#endif
```

### Windows

```csharp
#if WINDOWS
if (TestElement.Handler?.PlatformView is Microsoft.UI.Xaml.FrameworkElement platformElement)
{
    var transform = platformElement.TransformToVisual(null);
    var point = transform.TransformPoint(new Windows.Foundation.Point(0, 0));

    Console.WriteLine($"Screen Position: X={point.X}, Y={point.Y}");
    Console.WriteLine($"Screen Size: W={platformElement.ActualWidth}, H={platformElement.ActualHeight}");
}
#endif
```

---

## Best Practices

### ✅ DO

- **Use descriptive markers** in console output for easy filtering
- **Wait for layout completion** using `Loaded` event + `Dispatcher.DispatchDelayed` (simple scenarios)
- **Use SizeChanged + Task.Delay(1)** for dynamic property testing
- **Measure child elements** for SafeArea and padding tests (not parent size)
- **Calculate gaps from edges** for positioning validation
- **Use colored backgrounds** for visual debugging (red parent, yellow child)
- **Log at decision points** to understand execution flow
- **Test multiple iterations** for timing/race condition issues
- **Include timestamps** when timing matters
- **Clean up after testing** - always revert Sandbox changes

### ❌ DON'T

- **Don't measure immediately** in Loaded event without delay (layout not complete)
- **Don't use arbitrary delays** without event hooks (use Loaded or SizeChanged)
- **Don't measure parent size** when testing SafeArea (measure child position instead)
- **Don't clutter output** with unnecessary logging
- **Don't forget to clean up** after testing (revert Sandbox changes)
- **Don't commit instrumentation code** to the repository
- **Don't use platform-specific code** unless absolutely necessary (prefer cross-platform APIs)

### Console Output Formatting

```csharp
// Good: Clear, structured output
Console.WriteLine("=== Frame Info ===");
Console.WriteLine($"X={view.Bounds.X}, Y={view.Bounds.Y}, W={view.Width}, H={view.Height}");
Console.WriteLine("==================");

// Bad: Unclear, hard to parse
Console.WriteLine(view.ToString());
```

---

## Capturing Output

### iOS

```bash
# Launch with console capture
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/ios_output.log 2>&1 &

# Wait for output
sleep 5

# View logs
cat /tmp/ios_output.log

# Filter for specific marker
cat /tmp/ios_output.log | grep "TEST OUTPUT"

# Or stream live
xcrun simctl spawn booted log stream --predicate 'eventMessage contains "TEST OUTPUT"'
```

### Android

```bash
# Monitor logcat in real-time
adb logcat | grep "TEST OUTPUT"

# Or capture to file
adb logcat > /tmp/android_output.log &
# ... run test ...
kill %1  # Stop logcat
cat /tmp/android_output.log | grep "TEST OUTPUT"
```

### Taking Screenshots

**iOS:**
```bash
# After launching app
xcrun simctl io $UDID screenshot /tmp/screenshot.png
```

**Android:**
```bash
adb exec-out screencap -p > /tmp/screenshot.png
```

---

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

**iOS**: Use `xcrun simctl launch --console-pty` to capture output
**Android**: Use `adb logcat` to view logs
**Windows**: Output appears in Visual Studio Output window

### "Width/Height are -1"

Subscribe to layout completion event:

```csharp
// Wrong - might be -1 if layout hasn't run
Console.WriteLine($"Width: {view.Width}"); // Could be -1

// Right - wait for layout with Loaded event
private void OnLoaded(object sender, EventArgs e)
{
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine($"Width: {view.Width}"); // Actual value after layout
    });
}

// OR use SizeChanged for dynamic scenarios
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange
    Console.WriteLine($"Width: {view.Width}"); // Actual value
};
```

### "Measurements don't match visual appearance"

1. **Check you're measuring the right element** - Child position, not parent size (especially for SafeArea)
2. **Ensure layout is complete** - Use proper timing (Loaded + 500ms or SizeChanged + 1ms)
3. **Verify screen density** - Divide by `DeviceDisplay.MainDisplayInfo.Density` for logical pixels
4. **Check for transforms** - Element might have scale, rotation, or translation applied

---

## Quick Reference

### Basic Measurement Pattern

```csharp
private void OnLoaded(object sender, EventArgs e)
{
    Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
    {
        Console.WriteLine("=== TEST OUTPUT ===");
        Console.WriteLine($"Bounds: {TestElement.Bounds}");
        Console.WriteLine($"Size: {TestElement.Width}x{TestElement.Height}");
        Console.WriteLine("=== END TEST OUTPUT ===");
    });
}
```

### SizeChanged for Dynamic Scenarios

```csharp
view.SizeChanged += async (s, e) =>
{
    await Task.Delay(1); // Wait for arrange to complete
    Console.WriteLine($"Size: {view.Width}x{view.Height}");
    Console.WriteLine($"Bounds: {view.Bounds}");
};
```

### Screen Position (Platform-Specific)

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

### Visual Tree Traversal

```csharp
foreach (var child in element.GetVisualChildren()) { /* ... */ }
var parent = element.GetVisualParent();
```

### Capture Output

```bash
# iOS
xcrun simctl launch --console-pty $UDID com.microsoft.maui.sandbox > /tmp/output.log 2>&1 &
sleep 5 && cat /tmp/output.log | grep "TEST OUTPUT"

# Android
adb logcat | grep "TEST OUTPUT"
```

### Clean Up

```bash
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/
```

---

## Related Documentation

- [Common Testing Patterns](common-testing-patterns.md) - Build, deploy, error handling commands
- [SafeArea Testing Instructions](safearea-testing.md) - **Specialized guide for SafeArea testing** (measure children, not parents)
- [Edge Case Testing](edge-case-testing.md) - Comprehensive test scenario checklist
- [UI Tests Instructions](uitests.instructions.md) - Creating automated UI tests
- [Appium Control Scripts](appium-control.instructions.md) - UI automation patterns
- `docs/DevelopmentTips.md` - General development tips
- `docs/UITesting-Guide.md` - UI testing patterns

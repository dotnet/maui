---
description: "Guidelines for testing SafeArea changes in .NET MAUI PRs"
---

# Testing SafeArea Changes

This guide provides specific patterns for testing SafeArea-related PRs in .NET MAUI. SafeArea bugs are notoriously tricky to test because the effects are often visual and position-based.

## Understanding SafeArea Behavior

### Key Concept: Measure Children, Not Parents

**CRITICAL PRINCIPLE**: When testing SafeArea changes, you must measure the **CHILD CONTENT** position, not the container with SafeAreaEdges applied.

**Why?**
- SafeAreaEdges adds **padding** to the parent container
- The parent container size remains constant
- The **child content gets pushed inward** by the padding
- You detect SafeArea changes by measuring where the child content ends up on screen

### Visual Example

```
Without SafeArea padding:
┌─────────────────────────┐
│ Container (MainGrid)    │
│ ┌─────────────────────┐ │
│ │ Child (YellowContent)│ │  ← Child fills to edges
│ │                      │ │
│ └─────────────────────┘ │
└─────────────────────────┘

With SafeArea bottom padding:
┌─────────────────────────┐
│ Container (MainGrid)    │
│ ┌─────────────────────┐ │
│ │ Child (YellowContent)│ │
│ │                      │ │  ← Child is pushed up
│ └─────────────────────┘ │
│ ▓▓▓▓▓▓▓ Padding ▓▓▓▓▓▓▓ │  ← This is the gap we measure
└─────────────────────────┘
```

## Setting Up SafeArea Tests

### 1. XAML Structure

Use a colored container with SafeAreaEdges and a different-colored child to make gaps visible:

```xaml
<ContentPage SafeAreaEdges="None">
    <!-- Red container with SafeAreaEdges - this is what gets padding -->
    <Grid x:Name="MainGrid" BackgroundColor="Red" SafeAreaEdges="None">
        
        <!-- Yellow child - THIS is what we measure -->
        <!-- When MainGrid gets padding, yellow gets pushed inward -->
        <Grid x:Name="YellowContent" BackgroundColor="Yellow" SafeAreaEdges="None">
            <!-- Your test content here -->
        </Grid>
    </Grid>
</ContentPage>
```

**Visual indicators:**
- Red background becomes visible when padding is applied (bug indicator)
- Yellow should fill screen when no padding (correct behavior)

### 2. Instrumentation Code

Measure the **child content** position and calculate gaps from screen edges:

```csharp
private void LogMeasurements(string context)
{
    Console.WriteLine($"\n========== {context} ==========");
    
    // CRITICAL: Measure YellowContent (child), NOT MainGrid (parent)
    
#if IOS || MACCATALYST
    if (YellowContent.Handler?.PlatformView is UIKit.UIView platformView)
    {
        // Get root view (screen bounds)
        var rootView = platformView;
        while (rootView.Superview != null)
            rootView = rootView.Superview;
        
        // Convert child position to screen coordinates
        var screenRect = platformView.ConvertRectToView(platformView.Bounds, rootView);
        
        // Calculate gaps from screen edges
        double topGap = screenRect.Y;
        double bottomGap = rootView.Bounds.Height - (screenRect.Y + screenRect.Height);
        
        Console.WriteLine($"Yellow Content Screen Y: {screenRect.Y}");
        Console.WriteLine($"Yellow Content Height: {screenRect.Height}");
        Console.WriteLine($"Yellow Content Bottom: {screenRect.Y + screenRect.Height}");
        Console.WriteLine($"Screen Height: {rootView.Bounds.Height}");
        Console.WriteLine($"Top gap from screen edge: {topGap}");
        Console.WriteLine($"Bottom gap from screen edge: {bottomGap}");
    }
#elif ANDROID
    if (YellowContent.Handler?.PlatformView is Android.Views.View platformView)
    {
        int[] location = new int[2];
        platformView.GetLocationOnScreen(location);
        
        var rootView = platformView.RootView;
        int rootHeight = rootView?.Height ?? 0;
        
        double topGap = location[1];
        double bottomGap = rootHeight - (location[1] + platformView.Height);
        
        Console.WriteLine($"Yellow Content Screen Y: {location[1]}");
        Console.WriteLine($"Yellow Content Height: {platformView.Height}");
        Console.WriteLine($"Yellow Content Bottom: {location[1] + platformView.Height}");
        Console.WriteLine($"Screen Height: {rootHeight}");
        Console.WriteLine($"Top gap from screen edge: {topGap}");
        Console.WriteLine($"Bottom gap from screen edge: {bottomGap}");
    }
#endif
    Console.WriteLine("=================================\n");
}
```

### 3. Test Sequence

Always test in this order:

```csharp
// 1. Baseline: None (no padding)
MainGrid.SafeAreaEdges = SafeAreaEdges.None;
await Task.Delay(500);
LogMeasurements("1. SafeAreaEdges.None (baseline)");

// 2. The test case (e.g., SoftInput with keyboard hidden)
MainGrid.SafeAreaEdges = new SafeAreaEdges(
    SafeAreaRegions.None,
    SafeAreaRegions.None,
    SafeAreaRegions.None,
    SafeAreaRegions.SoftInput  // Bottom only
);
await Task.Delay(500);
LogMeasurements("2. Bottom=SoftInput (keyboard HIDDEN)");

// 3. Reference: All (expected padding)
MainGrid.SafeAreaEdges = SafeAreaEdges.All;
await Task.Delay(500);
LogMeasurements("3. SafeAreaEdges.All");
```

## Interpreting Results

### What to Look For

**Key metric**: "Bottom gap from screen edge"

**Expected values for typical SafeArea SoftInput bug:**
```
1. SafeAreaEdges.None
   - Bottom gap: 0 ✅ (content fills to screen bottom)

2. Bottom=SoftInput (keyboard HIDDEN)
   - WITHOUT PR FIX: Bottom gap: 34 ❌ (bug - padding incorrectly applied)
   - WITH PR FIX: Bottom gap: 0 ✅ (correct - no padding when keyboard hidden)

3. SafeAreaEdges.All  
   - Bottom gap: 34 ✅ (expected - safe area padding for home indicator)
```

**iPhone Xs typical gaps:**
- Status bar area (top): ~88px on iPhone Xs
- Home indicator area (bottom): ~34px on devices with home indicator
- Status bar varies by device and iOS version

### Common Mistakes

❌ **Wrong: Measuring the parent container**
```csharp
// This won't show the bug!
if (MainGrid.Handler?.PlatformView is UIKit.UIView platformView)
{
    var screenRect = platformView.ConvertRectToView(...);
    // MainGrid size stays the same regardless of SafeAreaEdges
}
```

✅ **Correct: Measuring the child content**
```csharp
// This shows the bug!
if (YellowContent.Handler?.PlatformView is UIKit.UIView platformView)
{
    var screenRect = platformView.ConvertRectToView(...);
    // YellowContent gets pushed inward when padding is applied
}
```

❌ **Wrong: Comparing heights without position**
```csharp
// Height alone doesn't tell you about padding
Console.WriteLine($"Height: {view.Height}");
```

✅ **Correct: Calculate gap from screen edge**
```csharp
// Gap reveals if padding was applied
double bottomGap = rootView.Bounds.Height - (screenRect.Y + screenRect.Height);
Console.WriteLine($"Bottom gap from screen edge: {bottomGap}");
```

## Platform-Specific Considerations

### iOS
- Home indicator area varies by device model
- iPhone Xs and earlier: ~34px
- iPhone with notch/Dynamic Island: Variable
- iPad: No home indicator gap
- Use iPhone Xs (iOS 26+) for consistent testing

### Android
- Navigation bar height varies by device and Android version
- Some devices have no navigation bar (gesture navigation)
- Test on emulator with navigation bar visible
- Gap will be ~48-96dp depending on device

## Validation Checklist

Before concluding your test:

- [ ] Measured the CHILD content, not the parent container
- [ ] Calculated gaps from screen edges (top and bottom)
- [ ] Tested all three scenarios (None, test case, All)
- [ ] Compared gap values between baseline and test case
- [ ] Verified expected gap for SafeAreaEdges.All (reference check)
- [ ] Tested both WITH and WITHOUT PR changes
- [ ] Documented actual pixel/point values in review

## When Tests Don't Show Expected Results

If your measurements don't show the expected difference:

1. **PAUSE immediately** - Don't switch to code-only review
2. **Verify you're measuring the child**, not the parent
3. **Check the test setup**:
   - Is SafeAreaEdges set on the parent container?
   - Is the child a direct child of the container?
   - Are you calculating gaps correctly?
4. **Ask for help**: "My test isn't showing the expected difference. Here's what I'm measuring: [paste code]. Can you help me understand what's wrong?"

**Never give up and switch to code-only review.** Always pause and ask for guidance if tests aren't working as expected.

## Example Test Output (Good)

```
========== 1. SafeAreaEdges.None (baseline) ==========
Yellow Content Screen Y: 88
Yellow Content Height: 724
Yellow Content Bottom: 812
Screen Height: 812
Top gap from screen edge: 88
Bottom gap from screen edge: 0    ← Content fills to bottom
=================================

========== 2. Bottom=SoftInput (keyboard HIDDEN) ==========
Yellow Content Screen Y: 88
Yellow Content Height: 690        ← Reduced by 34px
Yellow Content Bottom: 778         ← Doesn't reach screen bottom
Screen Height: 812
Top gap from screen edge: 88
Bottom gap from screen edge: 34   ← BUG DETECTED! Should be 0
=================================

========== 3. SafeAreaEdges.All ==========
Yellow Content Screen Y: 88
Yellow Content Height: 690
Yellow Content Bottom: 778
Screen Height: 812
Top gap from screen edge: 88
Bottom gap from screen edge: 34   ← Expected padding
=================================
```

**Analysis**: The bug is clearly visible - test 2 shows a 34px gap when it should show 0px (matching test 1).

## Related Documentation

- `.github/instructions/instrumentation.md` - General instrumentation patterns
- `.github/copilot-instructions.md` - Build and deployment guidelines

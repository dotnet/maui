---
description: "Standard edge cases to test for .NET MAUI UI and layout changes"
---

# Edge Case Testing Guide

This guide provides a comprehensive list of edge cases to test when validating fixes, new features, or reviewing PRs in .NET MAUI.

## Why Test Edge Cases

**Testing only the "happy path" misses:**
- Null reference exceptions
- Empty state rendering issues
- Performance problems with large data
- Race conditions with rapid changes
- Layout problems with extreme sizes
- Platform-specific quirks

**Edge case testing:**
- Reveals bugs before users encounter them
- Validates robustness of implementations
- Proves fixes don't introduce regressions
- Tests assumptions in code

## When to Test Edge Cases

### For Issue Resolution
✅ Test edge cases **after** verifying the main fix works
✅ Document which edge cases were tested in PR description

### For PR Review
✅ Test edge cases **beyond** what PR author tested
✅ Report any edge cases that fail or behave unexpectedly

### For New Features
✅ Test edge cases **during** development, not just at the end
✅ Add automated tests for critical edge cases

## Standard Edge Cases by Category

### 1. Data/Content Edge Cases

Test with various data states:

#### Empty State
```csharp
// Empty collections
TestCollectionView.ItemsSource = new List<string>();
TestCollectionView.ItemsSource = Array.Empty<string>();

// Null values
TestCollectionView.ItemsSource = null;
TestLabel.Text = null;
TestEntry.Text = null;

// Empty strings
TestLabel.Text = string.Empty;
TestLabel.Text = "";
```

**What to verify:**
- No crashes
- Appropriate empty state display
- Layout still correct
- No visual glitches

#### Single Item
```csharp
// Single item in collection
TestCollectionView.ItemsSource = new[] { "Single Item" };

// Minimal text
TestLabel.Text = "A";
```

**What to verify:**
- Proper sizing with one item
- No layout issues
- Scrolling disabled/enabled appropriately

#### Large Data Sets
```csharp
// Many items for scrolling/virtualization test
TestCollectionView.ItemsSource = Enumerable.Range(1, 100).Select(i => $"Item {i}").ToList();

// Very long text
TestLabel.Text = new string('A', 10000);
```

**What to verify:**
- Performance is acceptable
- Scrolling works smoothly
- Virtualization works correctly
- Memory usage is reasonable

#### Extreme Values
```csharp
// Very large numbers
TestLabel.Text = int.MaxValue.ToString();
TestLabel.Text = double.MaxValue.ToString();

// Unicode and special characters
TestLabel.Text = "🎉 Emoji 日本語 العربية";
TestLabel.Text = "Line1\nLine2\nLine3";  // Multiline
```

**What to verify:**
- Text renders correctly
- No truncation issues
- Layout handles content size

### 2. Property Change Edge Cases

#### Rapid Property Changes
```csharp
// Toggle property rapidly (test race conditions)
private async void TestRapidChanges()
{
    for (int i = 0; i < 10; i++)
    {
        TestElement.FlowDirection = FlowDirection.RightToLeft;
        await Task.Delay(50);
        TestElement.FlowDirection = FlowDirection.LeftToRight;
        await Task.Delay(50);
    }
}
```

**What to verify:**
- No crashes or exceptions
- Final state is correct
- No flickering or visual glitches
- Properties settle to correct values

#### Property Order Dependency
```csharp
// Set properties in different orders
// Order 1:
TestElement.Width = 200;
TestElement.Height = 100;
TestElement.BackgroundColor = Colors.Red;

// Order 2:
TestElement.BackgroundColor = Colors.Red;
TestElement.Height = 100;
TestElement.Width = 200;
```

**What to verify:**
- Order doesn't matter (unless it should)
- Final result is same regardless of order

#### Default vs Explicit Values
```csharp
// Test default values
var element = new Label();  // All defaults

// Test explicit same-as-default
var element2 = new Label { TextColor = Colors.Black };  // Explicit default
```

**What to verify:**
- Default behavior works
- Explicit defaults behave same as implicit

### 3. Layout Edge Cases

#### Extreme Sizes
```csharp
// Very large
TestElement.WidthRequest = 10000;
TestElement.HeightRequest = 10000;

// Very small
TestElement.WidthRequest = 1;
TestElement.HeightRequest = 1;

// Zero size
TestElement.WidthRequest = 0;
TestElement.HeightRequest = 0;
```

**What to verify:**
- No crashes with extreme sizes
- Layout algorithm handles edge cases
- Clipping works correctly

#### Nested Layouts
```xml
<!-- Multiple levels of nesting -->
<Grid>
    <VerticalStackLayout>
        <HorizontalStackLayout>
            <Grid>
                <Label Text="Deep nesting" />
            </Grid>
        </HorizontalStackLayout>
    </VerticalStackLayout>
</Grid>
```

**What to verify:**
- Layout calculation correct at all levels
- Performance acceptable
- No stack overflow or recursion issues

#### Constrained vs Unconstrained
```csharp
// Parent with constraints
<Grid WidthRequest="300" HeightRequest="200">
    <Label Text="Constrained parent" />
</Grid>

// Parent without constraints (fill available space)
<Grid>
    <Label Text="Unconstrained parent" />
</Grid>
```

**What to verify:**
- Child respects parent constraints
- Filling behavior works correctly

#### Margin and Padding Combinations
```csharp
// Various margin/padding combinations
TestElement.Margin = new Thickness(10);
TestElement.Padding = new Thickness(5);

TestElement.Margin = new Thickness(0);
TestElement.Padding = new Thickness(0);

TestElement.Margin = new Thickness(10, 5, 15, 20);  // Different values
TestElement.Padding = new Thickness(5, 10, 15, 20);
```

**What to verify:**
- Spacing calculated correctly
- No overlap or gaps
- RTL properly handled

### 4. Timing and Lifecycle Edge Cases

#### Before Layout Complete
```csharp
public MainPage()
{
    InitializeComponent();
    
    // Immediately query before layout runs
    var bounds = TestElement.Bounds;  // Might be 0,0,0,0
    Console.WriteLine($"Immediate bounds: {bounds}");
}
```

**What to verify:**
- Code handles pre-layout state gracefully
- No crashes from uninitialized values

#### During Navigation
```csharp
// Change properties while navigating
private async void OnButtonClicked(object sender, EventArgs e)
{
    TestElement.Text = "Changing...";
    await Navigation.PushAsync(new NewPage());
}
```

**What to verify:**
- No crashes during navigation
- State is preserved correctly
- Visual updates don't cause issues

#### Page Appearing/Disappearing
```csharp
protected override void OnAppearing()
{
    base.OnAppearing();
    // Test behavior when page appears
}

protected override void OnDisappearing()
{
    base.OnDisappearing();
    // Test behavior when page disappears
}
```

**What to verify:**
- Lifecycle methods work correctly
- State is managed properly
- Resources are cleaned up

### 5. Platform-Specific Edge Cases

#### Orientation Changes
```csharp
// Test in both orientations
// Portrait (default)
// Landscape (rotate device/simulator)
```

**What to verify:**
- Layout adapts correctly
- Content remains visible
- No crashes on rotation
- State preserved across rotation

#### Different Screen Sizes
Test on:
- Small phones (iPhone SE, small Android)
- Large phones (iPhone Pro Max, large Android)
- Tablets (iPad, Android tablets)
- Foldables (if applicable)

**What to verify:**
- Layout scales appropriately
- Text is readable
- Touch targets are adequate size

#### Dark Mode / Light Mode
```csharp
// Test in both modes
Application.Current.UserAppTheme = AppTheme.Dark;
Application.Current.UserAppTheme = AppTheme.Light;
```

**What to verify:**
- Colors visible in both modes
- Contrast is sufficient
- Custom colors adapt correctly

#### RTL (Right-to-Left) Languages
```csharp
// Test RTL layout
TestElement.FlowDirection = FlowDirection.RightToLeft;

// Test with RTL text
TestLabel.Text = "مرحبا" + " Hello";  // Mixed RTL/LTR
```

**What to verify:**
- Layout mirrors correctly
- Padding/margins swap sides
- Text alignment correct
- Icons/images positioned correctly

#### Safe Areas (iOS/Android)
```csharp
// Test with different SafeAreaEdges
TestElement.SafeAreaEdges = SafeAreaEdges.Top;
TestElement.SafeAreaEdges = SafeAreaEdges.Bottom;
TestElement.SafeAreaEdges = SafeAreaEdges.All;
TestElement.SafeAreaEdges = SafeAreaEdges.None;
```

**What to verify:**
- Content respects safe areas
- Notches/home indicators avoided
- Status bar area handled

See `.github/instructions/safearea-testing.instructions.md` for detailed SafeArea testing patterns.

### 6. Interaction Edge Cases

#### Rapid User Input
```csharp
// Rapid button taps
private int _tapCount = 0;
private void OnButtonTapped(object sender, EventArgs e)
{
    _tapCount++;
    Console.WriteLine($"Tap #{_tapCount}");
}
// Test: Tap button 10+ times rapidly
```

**What to verify:**
- No crashes from rapid input
- All events handled correctly
- No duplicate processing
- UI remains responsive

#### Simultaneous Gestures
```csharp
// Multiple touch points (if applicable)
// Pinch to zoom while scrolling
// Two-finger gestures
```

**What to verify:**
- Gestures don't conflict
- Priority is handled correctly
- No erratic behavior

#### Focus and Keyboard
```csharp
// Tab through Entry controls rapidly
// Show/hide keyboard quickly
TestEntry1.Focus();
await Task.Delay(100);
TestEntry2.Focus();
await Task.Delay(100);
TestEntry1.Focus();
```

**What to verify:**
- Focus moves correctly
- Keyboard shows/hides appropriately
- No visual glitches
- ScrollView adjusts for keyboard

### 7. State Management Edge Cases

#### Property Value Boundaries
```csharp
// Test at boundaries
TestElement.Opacity = 0;     // Fully transparent
TestElement.Opacity = 1;     // Fully opaque
TestElement.Opacity = 0.5;   // Semi-transparent

TestElement.Rotation = 0;
TestElement.Rotation = 360;
TestElement.Rotation = -180;
```

**What to verify:**
- Boundary values work correctly
- No visual artifacts
- Negative values handled (if applicable)

#### Conditional Property Sets
```csharp
// If/else property setting
if (condition)
    TestElement.BackgroundColor = Colors.Red;
else
    TestElement.BackgroundColor = Colors.Blue;

// Test with condition true and false
```

**What to verify:**
- Both branches work
- No leftover state from previous value

### 8. Collection-Specific Edge Cases

#### Selection
```csharp
// Single selection
TestCollectionView.SelectionMode = SelectionMode.Single;
TestCollectionView.SelectedItem = items[0];

// Multiple selection
TestCollectionView.SelectionMode = SelectionMode.Multiple;
TestCollectionView.SelectedItems = new ObservableCollection<object> { items[0], items[2] };

// No selection
TestCollectionView.SelectedItem = null;
```

**What to verify:**
- Selection visual state correct
- Selection events fire
- Changing selection mode works

#### Scrolling Edge Cases
```csharp
// Scroll to specific positions
TestCollectionView.ScrollTo(0, position: ScrollToPosition.Start);
TestCollectionView.ScrollTo(items.Count - 1, position: ScrollToPosition.End);
TestCollectionView.ScrollTo(50, position: ScrollToPosition.Center);
```

**What to verify:**
- Scrolling works smoothly
- Virtualization doesn't break
- End of list handled correctly

#### Dynamic Data Changes
```csharp
// Add items
items.Add("New Item");

// Remove items
items.RemoveAt(0);

// Clear all
items.Clear();

// Replace items
items[0] = "Updated Item";
```

**What to verify:**
- UI updates correctly
- No visual glitches during updates
- Performance is acceptable

### 9. Error Condition Edge Cases

#### Exception Handling
```csharp
try
{
    // Operation that might throw
    TestElement.SomeProperty = someValue;
}
catch (Exception ex)
{
    Console.WriteLine($"Exception: {ex.Message}");
}
```

**What to verify:**
- Exceptions are caught appropriately
- Error messages are helpful
- App doesn't crash
- Recovery is possible

#### Invalid Values
```csharp
// Test with invalid but non-crashing values
TestElement.WidthRequest = -100;  // Negative size
TestElement.Opacity = 5;          // > 1.0
TestElement.Rotation = 1000000;   // Very large
```

**What to verify:**
- Invalid values are clamped/rejected
- No crashes
- Reasonable default used

## Edge Case Testing Checklist

When testing a fix or new feature, go through this checklist:

### Data States
- [ ] Null values
- [ ] Empty collections/strings
- [ ] Single item
- [ ] Large data sets (100+ items)
- [ ] Very long text
- [ ] Special characters/emoji

### Property Changes
- [ ] Rapid property toggling (10+ times)
- [ ] Different property set orders
- [ ] Default vs explicit values
- [ ] Extreme values

### Layout
- [ ] Very large sizes
- [ ] Very small/zero sizes
- [ ] Deeply nested layouts
- [ ] Different margin/padding combinations

### Timing
- [ ] Before layout complete
- [ ] During navigation
- [ ] During page lifecycle events

### Platform-Specific
- [ ] Portrait and landscape
- [ ] Different screen sizes
- [ ] Dark mode and light mode
- [ ] RTL layout
- [ ] Safe area variations

### Interaction
- [ ] Rapid user input
- [ ] Multiple simultaneous gestures (if applicable)
- [ ] Focus changes
- [ ] Keyboard show/hide

### Collections (if applicable)
- [ ] Empty collection
- [ ] Single item
- [ ] Large collection (100+ items)
- [ ] Selection modes
- [ ] Dynamic data changes (add/remove/update)

### Error Handling
- [ ] Invalid values
- [ ] Exception scenarios
- [ ] Recovery from errors

## Documenting Edge Case Testing

When testing edge cases, document your findings:

```markdown
## Edge Cases Tested

### Data States
- ✅ Empty ItemsSource - No crash, shows empty state correctly
- ✅ Null ItemsSource - Handles gracefully, no exception
- ✅ Single item - Layout correct, no scrolling
- ✅ 100 items - Scrolling smooth, virtualization working
- ❌ Very long text (10000 chars) - Layout breaks, text truncated [BUG FOUND]

### Property Changes
- ✅ Rapid FlowDirection toggle (10x) - No flicker, final state correct
- ✅ Different property order - Results identical
- ✅ Extreme sizes (10000x10000) - Clipping works, no crash

### Platform-Specific
- ✅ Portrait mode - Works correctly
- ✅ Landscape mode - Works correctly
- ✅ Dark mode - Visible and correct contrast
- ✅ RTL layout - Mirrors correctly, padding swapped
- ✅ SafeAreaEdges.All - Content respects safe areas

### Interaction
- ✅ Rapid tapping (20+ taps) - All handled, no duplicate events
- ✅ Quick focus changes - Focus moves correctly, no crash

### Recommendation
Fix works well overall. Found one issue with very long text that should be addressed before merging.
```

## Related Documentation

- `.github/instructions/instrumentation.instructions.md` - How to instrument test code
- `.github/instructions/safearea-testing.instructions.md` - SafeArea-specific edge cases
- `.github/instructions/uitests.instructions.md` - Creating automated edge case tests
- `.github/copilot-instructions.md` - Platform-specific considerations

## Quick Reference

**Start with data edge cases**: Null, empty, single, large
**Then test behavior edge cases**: Rapid changes, extreme values
**Finally test platform edge cases**: Orientation, screen sizes, RTL, dark mode

**Document what you test**: Both what works AND what doesn't

**Goal**: Find issues before users do!

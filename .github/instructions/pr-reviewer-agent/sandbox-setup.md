# Sandbox Setup and Instrumentation

## Modify Sandbox App for Testing

After fetching the PR, modify the Sandbox app to test the scenario.

**File Locations**:
- `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml` (UI layout)
- `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs` (code-behind)

**General Instrumentation Pattern**:

**MainPage.xaml** - Create visual test scenario:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage">
    
    <!-- Use colored backgrounds for visual debugging -->
    <Grid x:Name="RootGrid" BackgroundColor="Red">
        
        <!-- The element you're testing -->
        <ContentView x:Name="TestElement" 
                     BackgroundColor="Yellow"
                     SafeAreaEdges="Top,Bottom"
                     Loaded="OnLoaded">
            <Label Text="Test Content" 
                   x:Name="ContentLabel"/>
        </ContentView>
    </Grid>
</ContentPage>
```

**MainPage.xaml.cs** - Capture measurements on Loaded:
```csharp
using Microsoft.Maui.Controls;

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
                // Capture measurements
                var element = (View)sender;
                var bounds = element.Bounds;
                var frame = element.Frame;
                
                Console.WriteLine("=== TEST OUTPUT ===");
                Console.WriteLine($"Element Bounds: {bounds}");
                Console.WriteLine($"Element Frame: {frame}");
                Console.WriteLine($"Element Y: {bounds.Y}");
                Console.WriteLine($"Element Height: {bounds.Height}");
                Console.WriteLine($"Screen Height: {DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density}");
                
                // For nested content (SafeArea testing)
                if (ContentLabel != null)
                {
                    Console.WriteLine($"Content Bounds: {ContentLabel.Bounds}");
                    Console.WriteLine($"Content Y: {ContentLabel.Bounds.Y}");
                    
                    // Calculate gaps from edges
                    double topGap = ContentLabel.Bounds.Y;
                    double screenHeight = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density;
                    double bottomGap = screenHeight - (ContentLabel.Bounds.Y + ContentLabel.Bounds.Height);
                    
                    Console.WriteLine($"Top Gap: {topGap}");
                    Console.WriteLine($"Bottom Gap: {bottomGap}");
                }
                
                Console.WriteLine("=== END TEST OUTPUT ===");
            });
        }
    }
}
```

**Key Instrumentation Techniques**:

1. **Use `Console.WriteLine`** - Output is captured in test logs
2. **Add markers** - "=== TEST OUTPUT ===" makes it easy to find in logs
3. **Use `Loaded` event** - Ensures measurements happen after layout
4. **Add delay** - `Dispatcher.DispatchDelayed(500ms)` ensures layout completed
5. **Capture absolute position** - Not just size, but position from edges
6. **Calculate gaps** - Especially for SafeArea testing (top/bottom gaps)
7. **Color code elements** - Makes visual inspection easier (red parent, yellow child)
8. **Measure correct elements** - Children positions, not parent sizes

**Common Instrumentation Patterns**:

**Pattern 1: Property Change Testing**
```csharp
// Test property toggle multiple times
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
}
```

**Pattern 2: Nested Content Measurement**
```csharp
// Measure child within parent
private void OnLoaded(object sender, EventArgs e)
{
    Console.WriteLine("=== TEST: Parent and Child ===");
    Console.WriteLine($"Parent Bounds: {RootGrid.Bounds}");
    Console.WriteLine($"Child Bounds: {ContentLabel.Bounds}");
    Console.WriteLine($"Child Absolute Y: {ContentLabel.Bounds.Y + RootGrid.Bounds.Y}");
}
```

**Pattern 3: Collection/List Testing**
```csharp
// Add large data set for scrolling test
public MainPage()
{
    InitializeComponent();
    
    var items = Enumerable.Range(1, 100).Select(i => $"Item {i}").ToList();
    TestCollectionView.ItemsSource = items;
}

private void OnLoaded(object sender, EventArgs e)
{
    Console.WriteLine($"=== TEST: {TestCollectionView.ItemsSource.Count} items loaded ===");
}
```

## Validation Checkpoint (WHEN to Use It)

**What is the validation checkpoint?**
- A pause in the review process where you show the user your test setup BEFORE running it
- Allows the user to verify you're testing the right thing
- Prevents wasting 20+ minutes on incorrect tests

**When to use the validation checkpoint:**

✅ **REQUIRED for these situations:**
1. **SafeArea testing** - High risk of measuring the wrong element
2. **First time testing a specific control type** - Unfamiliar with how to test it
3. **Complex layout scenarios** - Nested controls, multiple layers
4. **Unclear PR description** - Not certain what behavior to test
5. **Multiple possible interpretations** - Could test in several different ways
6. **After previous test attempt failed** - Need to adjust approach

❌ **Skip validation checkpoint for:**
- Simple, straightforward property tests (color, text, visibility)
- Clear PR with obvious test scenario
- Repetitive testing of same control type you've tested before
- User explicitly said "just test it quickly"

**What to show in validation checkpoint:**

```markdown
## Validation Checkpoint

Before building and running the test (which takes time), I want to confirm my test setup:

**Test Scenario**: [Describe what you're testing]

**Sandbox Modifications**:
```xml
<!-- Show relevant XAML snippet -->
<ContentView x:Name="TestElement" 
             SafeAreaEdges="Top,Bottom"
             BackgroundColor="Yellow">
    <Label x:Name="ContentLabel" Text="Test" />
</ContentView>
```

```csharp
// Show instrumentation code
Console.WriteLine($"Content Y: {ContentLabel.Bounds.Y}");
Console.WriteLine($"Top Gap: {topGap}");
```

**What I'm measuring**: [Explain what data you'll capture]
- Content position from top edge (should show gap with SafeArea)
- Content position from bottom edge (should show gap with SafeArea)

**Why this approach**: [Justify why you're measuring these specific things]
- SafeArea testing requires measuring child position, not parent size
- Gaps from edges will show if padding is applied correctly

**Does this test setup look correct?** If yes, I'll proceed with build/deploy/test. If not, please let me know what to adjust.
```

**Why validation checkpoint exists:**

❌ **Problem without validation checkpoint:**
```
Agent: "Let me test this SafeArea fix..."
[20 minutes of building, deploying, testing with WRONG measurements]
Agent: "Here are the results: parent size is 400x800"
User: "No, you should have measured the child position, not parent size"
Agent: "Let me try again..."
[Another 20 minutes wasted]
```

✅ **With validation checkpoint:**
```
Agent: "Let me test this SafeArea fix..."
Agent: "Validation Checkpoint - I plan to measure child Y position and gaps"
User: "Yes, that's correct" OR "No, also measure X position for RTL"
Agent: [Builds and tests with CORRECT setup on first try]
[20 minutes saved]
```

**Example Validation Checkpoint (SafeArea)**:

```markdown
## Validation Checkpoint - SafeArea Test Setup

**Test Scenario**: Verify SafeArea respects Top and Bottom edges for Label within ContentView

**Sandbox Setup**:
- Red parent Grid (full screen)
- Yellow ContentView with SafeAreaEdges="Top,Bottom"
- Label inside ContentView

**Measurements I'll capture**:
1. `ContentLabel.Bounds.Y` - Position from ContentView top
2. `TopGap` - Calculated as `ContentLabel.Bounds.Y`
3. `BottomGap` - Calculated as `screenHeight - (Y + Height)`

**Why measuring child, not parent**:
- Per `.github/instructions/safearea-testing.instructions.md`, SafeArea affects child position
- Parent ContentView size stays constant
- Only child Label will be inset from edges

**Expected Results**:
- WITHOUT PR: Top gap should be ~0 (bug)
- WITH PR: Top gap should be ~47px (status bar height)

Does this test setup correctly validate the SafeArea fix?
```

**When user confirms**, proceed with build/deploy/test. If user corrects you, adjust the setup and show updated checkpoint.

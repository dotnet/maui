# Issue Reproduction Guidelines

**Quick Links**:
- Complete iOS reproduction workflow: [quick-ref.md#complete-ios-reproduction-workflow](quick-ref.md#complete-ios-reproduction-workflow)
- Complete Android reproduction workflow: [quick-ref.md#complete-android-reproduction-workflow](quick-ref.md#complete-android-reproduction-workflow)
- Instrumentation templates: [quick-ref.md#instrumentation-templates](quick-ref.md#instrumentation-templates)
- Checkpoint 1 template: [quick-ref.md#checkpoint-1](quick-ref.md#checkpoint-1-after-reproduction)

## Sandbox App Setup for Reproduction

**Default app for issue reproduction**: `src/Controls/samples/Controls.Sample.Sandbox/`

**Why use Sandbox app:**
- Fast build times (~2 minutes)
- Simple, empty app you can modify freely
- Easy to add instrumentation
- Perfect for quick reproduction and testing

**See [quick-ref.md](quick-ref.md) for complete copy-paste commands to build and run Sandbox app on iOS and Android.**

### Creating Reproduction Test Case

**File Locations:**
- `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml` (UI layout)
- `src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml.cs` (code-behind)

**General Pattern:**

**MainPage.xaml** - Reproduce the user's scenario:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.MainPage"
             Title="Issue #XXXXX Reproduction">
    
    <!-- Recreate the exact scenario from the issue report -->
    <VerticalStackLayout Padding="20" Spacing="10">
        <Label Text="Testing Issue #XXXXX" 
               FontSize="18" 
               FontAttributes="Bold"/>
        
        <!-- Add the control/scenario that triggers the bug -->
        <CollectionView x:Name="TestCollection"
                        ItemsSource="{Binding Items}">
            <!-- Configure exactly as reported in the issue -->
        </CollectionView>
        
        <Button Text="Trigger Issue" 
                Clicked="OnTriggerIssue"
                AutomationId="TriggerButton"/>
        
        <Label x:Name="StatusLabel" 
               Text="Status will appear here"
               AutomationId="StatusLabel"/>
    </VerticalStackLayout>
</ContentPage>
```

**MainPage.xaml.cs** - Add instrumentation:
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
            
            // Initialize data if needed
            BindingContext = new ViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Add instrumentation to capture initial state
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
            {
                CaptureState("OnAppearing");
            });
        }

        private void OnTriggerIssue(object sender, EventArgs e)
        {
            Console.WriteLine("=== TRIGGERING ISSUE #XXXXX ===");
            
            // Reproduce the exact steps from the issue report
            
            Dispatcher.DispatchDelayed(TimeSpan.FromMilliseconds(500), () =>
            {
                CaptureState("AfterTrigger");
            });
        }

        private void CaptureState(string context)
        {
            Console.WriteLine($"=== STATE CAPTURE: {context} ===");
            Console.WriteLine($"Control Bounds: {TestCollection.Bounds}");
            Console.WriteLine($"Control Frame: {TestCollection.Frame}");
            
            // Add platform-specific measurements
            #if IOS || MACCATALYST
            var platformView = TestCollection.Handler?.PlatformView as UIKit.UIView;
            if (platformView != null)
            {
                Console.WriteLine($"Platform Frame: {platformView.Frame}");
            }
            #elif ANDROID
            var platformView = TestCollection.Handler?.PlatformView as Android.Views.View;
            if (platformView != null)
            {
                Console.WriteLine($"Platform Position: ({platformView.GetX()}, {platformView.GetY()})");
                Console.WriteLine($"Platform Size: {platformView.Width}x{platformView.Height}");
            }
            #endif
            
            Console.WriteLine("=== END STATE CAPTURE ===");
        }
    }
}
```

## Build and Deploy for Reproduction

**See [Common Testing Patterns](../common-testing-patterns.md) for detailed build and deploy commands with error checking.**

### Recommended: Use BuildAndRunSandbox.ps1

**Automated workflow** - Handles build, deploy, Appium, and logging:

```bash
# 1. Create Appium test script
cp .github/scripts/templates/RunWithAppiumTest.template.cs SandboxAppium/RunWithAppiumTest.cs

# 2. Edit SandboxAppium/RunWithAppiumTest.cs:
#    - Set ISSUE_NUMBER (replace 00000)
#    - Set PLATFORM ("android" or "ios")
#    - CUSTOMIZE test logic to match your Sandbox app UI:
#      * Use AutomationId values from your XAML
#      * Interact with your specific controls
#      * Template is just a starting point - modify completely!

# 3. Modify Sandbox app if needed
#    Edit: src/Controls/samples/Controls.Sample.Sandbox/MainPage.xaml
#    Add AutomationId attributes for Appium interaction

# 4. Run complete test
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform android
```

**What it does**:
- ✅ Builds Sandbox app
- ✅ Auto-detects devices
- ✅ Manages Appium (starts/stops automatically)
- ✅ Captures all logs (`SandboxAppium/appium.log`, `SandboxAppium/logcat.log`)
- ✅ Runs your Appium test

**When to use**: Issue reproduction with UI interaction (taps, swipes, etc.)

### Manual Workflows (For non-UI reproduction)

**iOS Manual** (when you need console logging without Appium):
```bash
# Follow detailed patterns in Common Testing Patterns
# 1. Get UDID, 2. Boot device, 3. Build, 4. Install, 5. Launch
```
See [Common Testing Patterns - iOS](../../common-testing-patterns.md#complete-ios-reproduction-workflow)

**Manual Workflows are rarely needed** - The BuildAndRunSandbox.ps1 script handles all common scenarios.

**Only use manual workflows for**:
- Advanced debugging scenarios
- Non-UI reproduction without Appium
- When script doesn't support your use case

See [Common Testing Patterns](../../common-testing-patterns.md) for manual command patterns.

### Troubleshooting

If builds or deployments fail, see:
- [Common Testing Patterns: Error Handling](../common-testing-patterns.md#common-error-handling-patterns)
- [Error Handling: Build Errors](error-handling.md#build-errors-during-reproduction)

## Verification Points

**After building and running, verify:**

✅ **App launches successfully**
- No crashes on startup
- UI loads correctly

✅ **Issue reproduces**
- Observe the same broken behavior as reported
- Console logs show the problem
- Screenshots/measurements confirm the bug

✅ **Instrumentation works**
- Console output is captured
- Measurements are being logged
- Platform-specific data is available

**Document reproduction results:**

```markdown
## ✅ Issue Reproduced Successfully

**Platform**: iOS 18.0 (iPhone 15 Pro Simulator)
**Build**: Debug, net10.0-ios

**Reproduction Steps**:
1. Launch app
2. Tap "Trigger Issue" button
3. Observe [specific broken behavior]

**Console Output**:
```
=== TRIGGERING ISSUE #12345 ===
=== STATE CAPTURE: AfterTrigger ===
Control Bounds: {X=0 Y=0 Width=393 Height=600}
Platform Frame: {{0, 0}, {393, 600}}
Expected value: 393, Actual value: 0  ❌ BUG CONFIRMED
=== END STATE CAPTURE ===
```

**Visual Observation**:
[Description of what you see that's wrong]

**Expected Behavior**:
[What should happen instead]
```

## If Issue Does NOT Reproduce

**Try these steps:**

1. **Different platforms** - Test on iOS, Android, Windows, Mac
2. **Different scenarios** - Modify the test case with variations
3. **Different timing** - Add delays, test on page load vs. button click
4. **Different data** - Empty collections, single items, large datasets
5. **Different configurations** - RTL, dark mode, different screen sizes

**If still cannot reproduce:**

```markdown
## ⚠️ Unable to Reproduce

**Platforms tested**: iOS 18.0, Android 14.0
**Scenarios tried**:
1. [Exact steps from issue report] - No bug observed
2. [Variation 1] - No bug observed
3. [Variation 2] - No bug observed

**Configuration used**:
```xaml
[Your XAML setup]
```

**Console output** (showing no error):
```
[Actual console output]
```

**Questions for issue reporter**:
1. Which exact MAUI version are you using?
2. Is this only on a physical device vs. simulator?
3. Can you provide the complete XAML/code sample?
4. Are there any other settings or conditions we should know about?
```

## Instrumentation Patterns

See `.github/instructions/instrumentation.instructions.md` for comprehensive patterns.

**Quick patterns for common scenarios:**

**Layout/positioning issues:**
```csharp
private void CaptureLayout(View view, string label)
{
    Console.WriteLine($"=== {label} ===");
    Console.WriteLine($"Bounds: {view.Bounds}");
    Console.WriteLine($"Frame: {view.Frame}");
    Console.WriteLine($"Width: {view.Width}, Height: {view.Height}");
    
    #if IOS || MACCATALYST
    var uiView = view.Handler?.PlatformView as UIKit.UIView;
    Console.WriteLine($"Platform Frame: {uiView?.Frame}");
    #endif
}
```

**Collection/list issues:**
```csharp
private void CaptureCollectionState(CollectionView collection)
{
    Console.WriteLine("=== Collection State ===");
    Console.WriteLine($"ItemsSource Count: {(collection.ItemsSource as IList)?.Count ?? 0}");
    Console.WriteLine($"SelectedItem: {collection.SelectedItem}");
    Console.WriteLine($"Bounds: {collection.Bounds}");
}
```

**Event/behavior issues:**
```csharp
private int eventCount = 0;

private void OnSomeEvent(object sender, EventArgs e)
{
    eventCount++;
    Console.WriteLine($"=== Event Fired #{eventCount} ===");
    Console.WriteLine($"Sender: {sender?.GetType().Name}");
    Console.WriteLine($"Timestamp: {DateTime.Now:HH:mm:ss.fff}");
}
```

## Next Steps After Reproduction

Once you've successfully reproduced the issue:

1. **Capture evidence** - Save console logs, take screenshots
2. **Investigate root cause** - Use instrumentation to understand WHY
3. **Move to solution development** - See `solution-development.md`

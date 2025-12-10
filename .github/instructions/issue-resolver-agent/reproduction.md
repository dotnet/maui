# Issue Reproduction Guidelines

**Quick Links**:
- Reproduction workflow: [quick-ref.md#reproduction-workflows](quick-ref.md#reproduction-workflows)
- Instrumentation templates: [quick-ref.md#instrumentation-templates](quick-ref.md#instrumentation-templates)
- Checkpoint 1 template: [quick-ref.md#checkpoint-1](quick-ref.md#checkpoint-1-after-reproduction)

## TestCases.HostApp for Issue Reproduction

**All issue reproduction is done in**: `src/Controls/tests/TestCases.HostApp/`

**Why use TestCases.HostApp:**
- Consistent with UI test infrastructure
- Same app used for reproduction and final tests
- Organized structure with Issues/ folder
- Built-in support for Appium testing
- All fixes include tests in the same workflow

**See [quick-ref.md](quick-ref.md) for complete copy-paste commands to build and run HostApp tests on iOS and Android.**

### Creating Reproduction Test Case

**File Locations:**
- `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml` (UI layout)
- `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml.cs` (code-behind)
- `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs` (UI test)

**General Pattern:**

**IssueXXXXX.xaml** - Reproduce the user's scenario:
```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.Issues.IssueXXXXX"
             Title="Issue XXXXX - Brief Description">
    
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

**IssueXXXXX.xaml.cs** - Add instrumentation:
```csharp
using Microsoft.Maui.Controls;
using System;

namespace Maui.Controls.Sample.Issues
{
    [Issue(IssueTracker.Github, XXXXX, "Brief description of the issue", PlatformAffected.All)]
    public partial class IssueXXXXX : ContentPage
    {
        public IssueXXXXX()
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

**ALWAYS use BuildAndRunHostApp.ps1 for reproduction testing:**

```bash
# 1. Create test page in HostApp to reproduce the issue
#    Files: src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml
#           src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml.cs
#    Add controls and AutomationId attributes for testing

# 2. Create UI test to interact with the test page
#    File: src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs
#    - Inherit from _IssuesUITest
#    - Add test method that reproduces the bug
#    - Use Appium to interact with UI
#    - Add assertions to verify the bug exists

# 3. Run the test (handles everything):
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"
# OR for iOS:
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

**What the script does**:
- ✅ Builds TestCases.HostApp for target platform
- ✅ Auto-detects device/emulator
- ✅ Manages Appium server (starts if needed, stops when done)
- ✅ Runs dotnet test with your filter
- ✅ Captures all logs automatically to `CustomAgentLogsTmp/UITests/` directory:
  - `appium.log` - Appium server logs
  - `android-device.log` or `ios-device.log` - Device logs (filtered to app)
  - `test-output.log` - Test execution results

**This is the ONLY way to run reproduction tests.** No manual build/deploy commands.

### Troubleshooting

If the script fails, see:
- [Error Handling: Build Errors](error-handling.md#build-errors-during-reproduction)
- Generated log files in `CustomAgentLogsTmp/UITests/` directory

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

See `.github/instructions/instrumentation.md` for comprehensive patterns.

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

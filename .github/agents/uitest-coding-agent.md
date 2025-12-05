---
name: uitest-coding-agent
description: Specialized agent for writing new UI tests for .NET MAUI with proper syntax, style, and conventions
---

# UI Test Coding Agent

You are a specialized agent for writing high-quality UI tests for the .NET MAUI repository following established conventions and best practices.

## Purpose

Write new UI tests that:
- Follow .NET MAUI UI test conventions
- Are maintainable and clear
- Run reliably across platforms
- Actually test the stated behavior

## Quick Decision: Should You Use This Agent?

**YES, use this agent if:**
- User says "write a UI test for issue #XXXXX"
- User says "add test coverage for..."
- User says "create automated test for..."
- Need to write NEW test files

**NO, use different agent if:**
- "Test this PR" → use `sandbox-agent`
- "Review this PR" → use `pr-reviewer`
- "Investigate issue #XXXXX" → use `issue-resolver`
- Only need manual verification → use `sandbox-agent`

---

## 🚨 CRITICAL: Always Use BuildAndRunHostApp.ps1 Script

**✅ ONLY DO THIS:**
```bash
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [android|ios|maccatalyst] -TestFilter "IssueXXXXX"
```

📖 **Complete documentation**: See [uitests.instructions.md](../instructions/uitests.instructions.md#running-ui-tests-locally) for:
- Full script usage and all parameters
- What the script handles automatically
- Manual commands for rapid development
- Troubleshooting guide and error handling

---

## 🚨 CRITICAL: Unique Issue Tracker Requirement

**ONE test page per issue number** - Multiple test pages with the same issue number will cause crashes!

```
❌ CRASH ERROR:
Please provide unique tracker + issue number combo: Github330080
```

**Why this happens**: The test infrastructure requires unique `[Issue(tracker, number)]` combinations across ALL test pages.

**Solutions**:

**✅ Option 1: Single comprehensive test page (PREFERRED)**
```csharp
// One test page testing multiple scenarios
[Issue(IssueTracker.Github, 33008, "Multiple test scenarios", PlatformAffected.iOS)]
public partial class Issue33008 : ContentPage
{
    // All test UI in one page
    // Multiple SearchBars, buttons, etc.
}

// One test file with multiple [Test] methods
public class Issue33008 : _IssuesUITest
{
    [Test] public void Scenario1() { }
    [Test] public void Scenario2() { }
    [Test] public void Scenario3() { }
}
```

**❌ Option 2 WILL CRASH: Multiple test pages (DON'T DO THIS)**
```csharp
// ❌ WRONG: Multiple files with same issue number
Issue33008SearchBar.xaml     // [Issue(Github, 33008, ...)]
Issue33008SearchHandler.xaml  // [Issue(Github, 33008, ...)]
// Result: CRASH - duplicate tracker combo
```

**Rule**: If you need multiple test scenarios for one issue, put them ALL in ONE test page with multiple test methods.

---

## Two-Project Requirement

**CRITICAL**: Every UI test requires code in TWO projects:

1. **HostApp UI Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/`)
   - XAML: `IssueXXXXX.xaml`
   - Code-behind: `IssueXXXXX.xaml.cs`
   - Contains actual UI that demonstrates/reproduces the issue

2. **NUnit Test** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`)
   - Test file: `IssueXXXXX.cs`
   - Contains Appium-based automated test

## File Templates

### HostApp XAML (`IssueXXXXX.xaml`)

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.Issues.IssueXXXXX"
             Title="Issue XXXXX - [Brief Description]">

    <StackLayout>
        <!-- Elements must have AutomationId for test automation -->
        <Button x:Name="TestButton"
                AutomationId="TestButton"
                Text="Trigger Action"
                Clicked="OnButtonClicked"/>
        
        <Label x:Name="ResultLabel"
               AutomationId="ResultLabel"
               Text="Initial State"/>
    </StackLayout>
</ContentPage>
```

### HostApp Code-Behind (`IssueXXXXX.xaml.cs`)

```csharp
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Issues;

// CRITICAL: Must have Issue attribute with ALL parameters
[Issue(IssueTracker.Github, XXXXX, "Brief description of the issue", PlatformAffected.All)]
public partial class IssueXXXXX : ContentPage
{
    public IssueXXXXX()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        // Implement behavior that demonstrates the issue/fix
        ResultLabel.Text = "Action Completed";
    }
}
```

### NUnit Test (`IssueXXXXX.cs`)

```csharp
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class IssueXXXXX : _IssuesUITest
    {
        public override string Issue => "Brief description of the issue";

        public IssueXXXXX(TestDevice device) : base(device) { }

        [Test]
        [Category(UITestCategories.Button)] // Use ONE category - the most specific
        public void DescriptiveTestMethodName()
        {
            // Wait for element to be ready
            App.WaitForElement("TestButton");

            // Interact with UI
            App.Tap("TestButton");

            // Verify expected behavior
            var result = App.FindElement("ResultLabel").GetText();
            Assert.That(result, Is.EqualTo("Action Completed"));
        }
    }
}
```

## Test Category Selection

**CRITICAL**: Use ONLY ONE `[Category]` attribute per test.

**Always check**: `src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs` for the authoritative list.

**Selection rule**: Choose the MOST SPECIFIC category that applies.

**Common categories** (examples):
- `UITestCategories.Button` - Button-specific tests
- `UITestCategories.Entry` - Entry-specific tests
- `UITestCategories.CollectionView` - CollectionView tests
- `UITestCategories.Layout` - Layout-related tests
- `UITestCategories.Navigation` - Navigation tests
- `UITestCategories.SafeAreaEdges` - SafeArea tests
- `UITestCategories.Gestures` - Gesture tests

**How to choose**:
```bash
# List all available categories
grep -E "public const string [A-Za-z]+ = " src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs
```

## Platform Coverage Rules

**Default**: Tests should run on ALL platforms unless there's a technical reason.

**DO NOT use platform directives unless**:
- Platform-specific API is being tested
- Known limitation prevents test on a platform
- Platforms are expected to behave differently

```csharp
// ✅ Good: Runs everywhere
[Test]
[Category(UITestCategories.Button)]
public void ButtonClickWorks()
{
    App.WaitForElement("TestButton");
    App.Tap("TestButton");
}

// ❌ Bad: Unnecessarily limited
#if ANDROID
[Test]
[Category(UITestCategories.Button)]
public void ButtonClickWorks() { }
#endif
```

## AutomationId Requirements

**CRITICAL**: Every interactive element MUST have an `AutomationId`.

```xml
<!-- ✅ Good -->
<Button AutomationId="SaveButton" Text="Save"/>
<Entry AutomationId="UsernameEntry"/>
<Label AutomationId="StatusLabel"/>

<!-- ❌ Bad: Missing AutomationId -->
<Button Text="Save"/>
<Entry/>
<Label/>
```

**Platform-specific locators**:
```csharp
// Android: Use MobileBy.Id()
var button = App.FindElement(MobileBy.Id("SaveButton"));

// iOS: Use MobileBy.AccessibilityId()
var button = App.FindElement(MobileBy.AccessibilityId("SaveButton"));
```

**Platform-agnostic approach** (preferred when possible):
```csharp
// Works on both platforms if AutomationId is set properly
App.WaitForElement("SaveButton");
App.Tap("SaveButton");
var text = App.FindElement("StatusLabel").GetText();
```

## Common Test Patterns

### Basic Interaction Test
```csharp
[Test]
[Category(UITestCategories.Button)]
public void ButtonClickUpdatesLabel()
{
    App.WaitForElement("TestButton");
    App.Tap("TestButton");
    
    var labelText = App.FindElement("ResultLabel").GetText();
    Assert.That(labelText, Is.EqualTo("Clicked"));
}
```

### Navigation Test
```csharp
[Test]
[Category(UITestCategories.Navigation)]
public void NavigationDoesNotCrash()
{
    App.WaitForElement("NavigateButton");
    App.Tap("NavigateButton");
    
    // Wait for new page
    App.WaitForElement("BackButton");
    
    // Verify navigation succeeded
    Assert.Pass("Navigation completed without crash");
}
```

### Input Validation Test
```csharp
[Test]
[Category(UITestCategories.Entry)]
public void EntryAcceptsInput()
{
    App.WaitForElement("UsernameEntry");
    
    App.EnterText("UsernameEntry", "testuser");
    App.DismissKeyboard();
    
    var enteredText = App.FindElement("UsernameEntry").GetText();
    Assert.That(enteredText, Is.EqualTo("testuser"));
}
```

### Layout Measurement Test
```csharp
[Test]
[Category(UITestCategories.Layout)]
public void ElementHasCorrectSize()
{
    App.WaitForElement("TestElement");
    
    var rect = App.FindElement("TestElement").GetRect();
    
    Assert.That(rect.Width, Is.GreaterThan(0));
    Assert.That(rect.Height, Is.GreaterThan(0));
}
```

### Screenshot Verification Test
```csharp
[Test]
[Category(UITestCategories.Button)]
public void ButtonAppearsCorrectly()
{
    App.WaitForElement("TestButton");
    
    // Visual verification
    VerifyScreenshot();
}
```

## Best Practices

### 1. Always Wait Before Interacting
```csharp
// ✅ Good
App.WaitForElement("TestButton");
App.Tap("TestButton");

// ❌ Bad: May fail if element not ready
App.Tap("TestButton");
```

### 2. Use Descriptive Names
```csharp
// ✅ Good
public void ButtonClickUpdatesLabelText() { }

// ❌ Bad
public void Test1() { }
```

### 3. Add Meaningful Assertions
```csharp
// ✅ Good: Verifies behavior
Assert.That(result, Is.EqualTo("Expected"));

// ❌ Bad: Empty test
App.Tap("TestButton");
// No verification
```

### 4. Clean Up State
```csharp
[Test]
public void TestModifiesGlobalState()
{
    // Test code that modifies state
    
    // Most tests don't need cleanup
    // Framework provides fresh page instance
}
```

### 5. Avoid Task.Delay Unless Necessary

**⚠️ GUIDELINE**: Prefer Appium's built-in synchronization, but use delays when tests require them.

```csharp
// ✅ Preferred: Let Appium handle waiting
App.Tap("SetTextButton");
var status = App.FindElement("StatusLabel").GetText();

// ⚠️ Use When Necessary: Some tests need explicit delays
App.Tap("AnimateButton");
Task.Delay(500).Wait();  // Animation takes time
var position = App.FindElement("MovingElement").GetRect();
```

**When delays ARE needed**:
- Animations must complete before measurement
- Async operations without observable UI changes
- Platform-specific timing requirements
- Tests fail consistently without delay

**When delays are NOT needed**:
- Waiting for element to appear (use `App.WaitForElement()`)
- Waiting for element to become interactive (use `App.WaitForElement()`)
- Simple button taps and text updates (Appium waits automatically)

**Rule of thumb**:
1. Try without delay first
2. If test fails consistently, add minimal delay
3. Document WHY delay is needed

```csharp
// ✅ Good: Documented necessary delay
App.Tap("StartAnimationButton");
Task.Delay(500).Wait();  // Wait for animation to complete
var finalPosition = App.FindElement("AnimatedElement").GetRect();

// ❌ Bad: Unnecessary delay (FindElement waits automatically)
App.Tap("Button");
Task.Delay(500).Wait();
var text = App.FindElement("Label").GetText();
```

### 6. Use Appropriate Waits (When Needed)
```csharp
// ✅ Good: Wait for specific element with custom timeout
App.WaitForElement("ResultLabel", timeout: TimeSpan.FromSeconds(10));

// ✅ Good: Standard wait (uses default timeout)
App.WaitForElement("ResultLabel");
```

## Checklist Before Committing

- [ ] Two files created (XAML + NUnit test)
- [ ] File names match: `IssueXXXXX`
- [ ] **UNIQUE** `[Issue()]` tracker+number combo (no duplicates across all test files)
- [ ] `[Issue()]` attribute present with all parameters
- [ ] Inherits from `_IssuesUITest`
- [ ] ONE `[Category]` attribute from UITestCategories
- [ ] All interactive elements have `AutomationId`
- [ ] Test uses `App.WaitForElement()` before interactions
- [ ] Delays (`Task.Delay()`) only used when necessary and documented
- [ ] Test has meaningful assertions
- [ ] Test method name is descriptive
- [ ] No unnecessary platform directives
- [ ] Compiled both HostApp and test projects
- [ ] Ran test locally and verified it passes

---

## After Creating Test Files

### Step 1: Verify Files Are Correct

**Before running:**
- [ ] HostApp XAML file: `TestCases.HostApp/Issues/IssueXXXXX.xaml`
- [ ] HostApp code-behind: `TestCases.HostApp/Issues/IssueXXXXX.xaml.cs`
- [ ] NUnit test: `TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`
- [ ] All `AutomationId` values match between XAML and test
- [ ] `[Issue()]` attribute present with all parameters
- [ ] ONE `[Category()]` attribute

### Step 2: Run the Test

```bash
# Default: Android
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"

# Or iOS (default: iPhone Xs, iOS 18.5)
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX"
```

### Step 3: If Test Passes ✅

**Report to user:**
```markdown
✅ **Test Created and Validated**

**Files created:**
- `TestCases.HostApp/Issues/IssueXXXXX.xaml[.cs]`
- `TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

**Test result:** PASS on [Android/iOS]

**What the test validates:**
- [Describe what behavior is tested]

**Ready to commit.**
```

### Step 4: If Test Fails ❌

**See "Troubleshooting" section** - check for:
1. App crashes (check device logs)
2. Element not found (verify AutomationIds)
3. Build errors (check script output)

---

## Running Tests Locally

📖 **Complete documentation**: See [uitests.instructions.md](../instructions/uitests.instructions.md#running-ui-tests-locally) for:
- Full BuildAndRunHostApp.ps1 script usage
- All filter options and parameters  
- Device/iOS version selection
- Manual commands for rapid development
- Troubleshooting guide

**Quick reference:**
```bash
# Run specific test
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [android|ios|maccatalyst] -TestFilter "IssueXXXXX"

# Run by category
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform [android|ios|maccatalyst] -Category "Button"

# Run with specific device (iOS)
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "IssueXXXXX" -DeviceUdid "UDID"

echo "✅ Found iPhone Xs with iOS 18.5: $UDID"

# Step 3: Run test with specific device
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "Issue12345" -DeviceUdid "$UDID"
```

**Finding different device/version combinations:**
```bash
# iPhone 16 Pro with any iOS version
UDID=$(xcrun simctl list devices available --json | jq -r '
  .devices[][] | select(.name == "iPhone 16 Pro") | .udid' | head -1)

# Any device with iOS 18.0
UDID=$(xcrun simctl list devices available --json | jq -r '
  .devices | to_entries 
  | map(select(.key | contains("iOS-18-0"))) 
  | map(.value) | flatten | .[0].udid')

# Specific device with specific iOS version
UDID=$(xcrun simctl list devices available --json | jq -r '
  .devices | to_entries 
  | map(select(.key | contains("iOS-18-5"))) 
  | map(.value) | flatten 
  | map(select(.name == "iPhone 15")) | first | .udid')
```

### Run Multiple Tests by Category

```bash
# Run all Button tests on Android
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -Category "Button"

# Run all Layout tests on iOS
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -Category "Layout"
```

### Verify Test Results

After the script completes:

1. **Check exit code** - Script exits 0 on success, non-zero on failure
2. **Read captured logs** - Script output shows log directory location
3. **Review test output** - All NUnit output is captured and displayed

### If Test Fails - Element Not Found

**🚨 CRITICAL**: If your test can't find an element, DO NOT assume the test is wrong!

**The app may have crashed or not loaded correctly.**

**IMMEDIATELY CHECK**:

1. **Check device logs for crashes**:
   ```bash
   # Script shows log location - look for these patterns
   # Android: grep -i "FATAL\|crash\|exception" <log-directory>/android-device.log
   # iOS: grep -i "terminating\|exception\|crash" <log-directory>/ios-device.log
   ```

2. **Verify app actually loaded**:
   - Check logs for HostApp initialization messages
   - Look for "Issue XXXXX" page creation
   - If app crashed: Fix the crash before fixing the test

3. **Common root causes**:
   - **App crashed on launch** - Exception in XAML/code-behind
   - **XAML parse error** - Missing event handler method
   - **AutomationId mismatch** - XAML name doesn't match test
   - **Wrong page displayed** - Navigation went elsewhere

**Debugging steps**:
1. Check logs for crashes/exceptions (script captures these)
2. If crashed: Fix exception in XAML/code-behind
3. If XAML error: Verify event handler exists (e.g., `OnButtonClicked`)
4. If no crash: Verify AutomationIds match exactly between XAML and test

**DO NOT**:
- ❌ Try different AutomationIds without checking logs
- ❌ Add delays/sleeps hoping element appears
- ❌ Run manual adb/xcrun commands to investigate
- ❌ Assume app is "just loading slowly"

### If Test Fails - Other Reasons

**🚨 DO NOT debug with manual commands!**

Instead:

1. **Read the captured logs** - Script tells you where they are
2. **Check test output** - Script shows NUnit results
3. **Verify assertions** - Are you testing the right thing?
4. **If unclear** - Ask for help with the log output

### Common Issues and Solutions

**"Element not found" errors:**
```bash
# 1. Verify AutomationId exists in XAML
grep "AutomationId" src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml

# 2. Check test is using correct AutomationId
grep "WaitForElement\|FindElement" src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs

# 3. Re-run test - script will show if element is missing
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"
```

**Test builds but fails:**
- Check device logs (script captures these automatically)
- Verify XAML event handlers exist in code-behind
- Ensure [Issue] attribute is present on HostApp page

**Can't find test:**
- Verify test class name matches: `IssueXXXXX`
- Check test inherits from `_IssuesUITest`
- Ensure test has `[Test]` attribute

## Common Mistakes to Avoid

### Missing AutomationId
```xml
<!-- ❌ Bad -->
<Button Text="Click Me"/>

<!-- ✅ Good -->
<Button AutomationId="ClickMeButton" Text="Click Me"/>
```

### Multiple Categories
```csharp
// ❌ Bad
[Category(UITestCategories.Button)]
[Category(UITestCategories.Layout)]
public void TestButton() { }

// ✅ Good: Pick ONE
[Category(UITestCategories.Button)]
public void TestButton() { }
```

### No Base Class
```csharp
// ❌ Bad
public class IssueXXXXX { }

// ✅ Good
public class IssueXXXXX : _IssuesUITest { }
```

### Missing Issue Attribute
```csharp
// ❌ Bad
public partial class IssueXXXXX : ContentPage { }

// ✅ Good
[Issue(IssueTracker.Github, 12345, "Description", PlatformAffected.All)]
public partial class IssueXXXXX : ContentPage { }
```

### Using Task.Delay or Thread.Sleep
```csharp
// ⚠️ Avoid when possible (try without delay first)
App.Tap("Button");
Task.Delay(500).Wait();
var text = App.FindElement("Label").GetText();

// ✅ Good: Use when necessary with explanation
App.Tap("AnimateButton");
Task.Delay(500).Wait();  // Animation duration requires wait
var position = App.FindElement("Element").GetRect();

// ✅ Better: Prefer Appium's synchronization when possible
App.Tap("Button");
var text = App.FindElement("Label").GetText();  // Waits automatically
```

**Guideline**: Try without delays first. Add only if tests fail consistently, and document why the delay is needed.

### Duplicate Issue Tracker Numbers
```
// ❌ Bad: Multiple test pages with same issue number
Issue33008SearchBar.xaml      [Issue(Github, 33008, ...)]
Issue33008SearchHandler.xaml  [Issue(Github, 33008, ...)]
// RESULT: App crashes with "unique tracker + issue number combo" error

// ✅ Good: One test page with multiple test methods
Issue33008.xaml               [Issue(Github, 33008, ...)]
Issue33008.cs                 { Test1(), Test2(), Test3() }
```

## Troubleshooting

**Test Won't Compile**:
- Check namespace: `Microsoft.Maui.TestCases.Tests.Issues`
- Verify base class: `_IssuesUITest`
- Ensure constructor: `public IssueXXXXX(TestDevice device) : base(device) { }`
- Run: `pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"`
- Build errors will be shown in script output

**Element Not Found - CRITICAL**:
🚨 **DO NOT assume test is wrong - check logs first!**
1. Run test with script: `pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"`
2. Check device logs (script shows location) for crashes
3. If app crashed: Fix the crash before fixing the test
4. If no crash: Verify AutomationId exists in XAML and matches test code
5. See detailed section: [If Test Fails - Element Not Found](#if-test-fails---element-not-found)

**Test Flaky**:
- Add appropriate waits with `App.WaitForElement()`
- Don't rely on fixed sleeps (`Thread.Sleep`)
- Check for race conditions in test code
- Re-run multiple times: `pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "IssueXXXXX"`
- Review captured logs for timing issues

**Script Reports Test Failure**:
1. ✅ Read the script output - shows what failed
2. ✅ Check log directory (script shows path)
3. ✅ Look for device logs with crash info
4. ❌ DON'T run manual commands to investigate

## Key Resources

- [UI Testing Guide](../instructions/uitests.instructions.md) - Complete UI test documentation

- [UITestCategories.cs](../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs) - All available categories

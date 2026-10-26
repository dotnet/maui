---
description: "Comprehensive guidance for creating automated UI tests for .NET MAUI using Appium and NUnit."
date: 2025-10-30
---

# UI Testing Guide for .NET MAUI Repository

Comprehensive guidance for creating automated UI tests for .NET MAUI using Appium and NUnit. This document works in conjunction with the main Copilot Instructions.

## Quick Reference

**Two-part requirement for any UI test:**
1. HostApp page in `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml`
2. NUnit test in `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

## Prerequisites

### All Platforms

**Appium Package**: All UI tests use `Appium.WebDriver@8.0.1` (latest stable version)

```bash
# Restore tools (required)

dotnet tool restore

# Install Node.js LTS from https://nodejs.org

# Provision Appium

dotnet build ./src/Provisioning/Provisioning.csproj -t:ProvisionAppium -p:SkipAppiumDoctor="true"
```

### Windows-Specific

- Enable Developer Mode in Windows Settings
- Install Windows App Driver v1.2.1 from https://github.com/microsoft/WinAppDriver/releases
- Ensure `%USERPROFILE%\AppData\Roaming\npm` is in PATH

### Android-Specific

- Set `ANDROID_HOME` environment variable
- Set `JAVA_HOME` environment variable
- Install Android API 30 SDK with x86/x64 emulator image

#### Launch directly to a test page

The Android HostApp accepts a `test` string intent extra containing the exact
issue description (from `IssueAttribute`) or gallery title, matched
case-insensitively. For example, after installing the HostApp:

```bash
adb shell am start -S -n com.microsoft.maui.uitests/com.microsoft.maui.uitests.MainActivity --es test '"Direct test launch"'
```

This creates the requested page as the initial window page without searching or
tapping through the gallery. Use `-S` to stop the existing app before launching:
the extra is read when the window is created, not when an already-running activity
is brought to the foreground. Omit `--es test ...` for normal gallery startup.
An empty or unrecognized test also opens the gallery; an unrecognized test is
reported in debug output.

### iOS/MacCatalyst-Specific

- macOS required
- Xcode with command-line tools installed
- iOS Simulator configured

#### Launch directly on iOS

iOS and MacCatalyst use the `test` environment variable with the same exact issue
description or gallery title. After installing the iOS HostApp on a simulator:

```bash
SIMCTL_CHILD_test="Direct test launch" xcrun simctl launch --terminate-running-process <simulator-udid> com.microsoft.maui.uitests
```

For a manually launched MacCatalyst app, set `test` in the app process's environment.
On Windows, pass the description or gallery title as one `--test=...` command-line
argument:

```powershell
.\Controls.TestCases.HostApp.exe '--test=Direct test launch'
```

On all platforms, missing, empty, or unrecognized startup targets open the normal
gallery. Unknown targets are reported in debug output. A startup target is consumed
when the initial window is created, so terminate the previous app instance before
changing it.

### Direct launch in the UI test runner

`_IssuesUITest` and `_GalleryUITest` configure their startup targets automatically
on Android, iOS, and MacCatalyst.
Fixture setup no longer searches for and taps the gallery's Go To Test button.
Resets relaunch the current fixture's target, including fixtures that set
`ResetAfterEachTest`; gallery subpage buttons are still selected after launch.
Fixture teardown closes the app rather than launching an unused gallery.
Tests of normal gallery startup can derive directly from `UITest` instead.

Starting a new test and resetting the app stop with an explicit error if the
previous app cannot be terminated. OS-command fallback termination succeeds only
when the command starts, completes within its timeout, and exits with code zero; a failed command
must not be treated as a successful close.

Android, iOS, and MacCatalyst reuse the driver session and send the current target
with each app launch. Android forwards the configured device UDID to Appium as
well, so multiple running emulators do not select the wrong device. Windows uses
`appArguments`; because WinAppDriver cannot change these capabilities on an
existing session, changing the startup target recreates that session. Windows
therefore retains gallery navigation by default until its native timing is
validated; set `MAUI_UITEST_DIRECT_LAUNCH=true` to opt into direct fixture startup.
Set it to `false` on any platform to compare the legacy navigation path.
Do not infer a Windows speedup from eliminating gallery navigation alone.

The startup argument, session reuse, and launch-command unit tests run without
native UI automation:

```bash
dotnet test src/TestUtils/tests/UITest.Appium.Tests/UITest.Appium.Tests.csproj
```

## Creating a UI Test

### Step 1: Create the HostApp Page

**File:** `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml`

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             Title="Issue XXXXX - Issue Title">

    <VerticalStackLayout Padding="20" Spacing="10">
        <!-- CRITICAL: Every interactive element needs AutomationId -->
        
        <Label Text="Instructions or description"
               AutomationId="InstructionLabel"/>
        
        <Button Text="Click Me"
                AutomationId="ClickButton"
                Clicked="OnButtonClicked"/>
        
        <Label Text="Result will appear here"
               AutomationId="ResultLabel"
               IsVisible="False"/>
    </VerticalStackLayout>
</ContentPage>
```

**File:** `src/Controls/tests/TestCases.HostApp/Issues/IssueXXXXX.xaml.cs`

```csharp
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, XXXXX, "Issue description", PlatformAffected.All)]
public partial class IssueXXXXX : ContentPage
{
    public IssueXXXXX()
    {
        InitializeComponent();
    }

    private void OnButtonClicked(object sender, EventArgs e)
    {
        ResultLabel.IsVisible = true;
        ResultLabel.Text = "Expected behavior verified";
    }
}
```

### Step 2: Create the NUnit Test

**File:** `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/IssueXXXXX.cs`

```csharp
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IssueXXXXX : _IssuesUITest
{
    public IssueXXXXX(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Clear description of what the issue is testing";
    
    [Test]
    [Category(UITestCategories.Button)]  // Use most appropriate category
    public void TestMethodName()
    {
        // Arrange - Setup initial state
        App.WaitForElement("ClickButton");
    
        // Act - Perform user interaction
        App.Tap("ClickButton");
    
        // Assert - Verify result
        App.WaitForElement("ResultLabel");
        
        // Optional: Screenshot verification
        VerifyScreenshot();
    }
}
```

## Common UI Test Operations

### Element Interaction

```csharp
// Single tap
App.Tap("ButtonAutomationId");

// Double tap
App.DoubleTap("ElementAutomationId");

// Long press
App.TouchAndHold("ElementAutomationId");

// Text entry
App.EnterText("EntryAutomationId", "Sample text");
App.ClearText("EntryAutomationId");

// Slider
App.SetSliderValue("SliderAutomationId", 0.5);

// Stepper
App.IncreaseStepper("StepperAutomationId");
App.DecreaseStepper("StepperAutomationId");
```

### Gestures

```csharp
// Swipe left to right
App.SwipeLeftToRight();

// Swipe right to left
App.SwipeRightToLeft();

// Drag and drop
App.DragAndDrop("SourceId", "TargetId");

// Pinch to zoom
App.PinchToZoomIn("ImageId");
App.PinchToZoomOut("ImageId");

// Tap coordinates
App.TapCoordinates(100, 100);
```

### Scrolling

```csharp
// Scroll down
App.ScrollDown("CollectionViewId");

// Scroll up
App.ScrollUp("CollectionViewId");

// Scroll to element
App.ScrollTo("TargetElementId", down: true);

// With custom strategy
App.ScrollDown("CollectionViewId",
ScrollStrategy.Gesture,
swipePercentage: 0.5);
```

### Waiting and Queries

```csharp
// Wait for element
App.WaitForElement("ElementAutomationId",
timeout: TimeSpan.FromSeconds(10));

// Wait until present (with retries)
App.QueryUntilPresent(() => App.WaitForElement("ElementId"));

// Wait until not present
App.QueryUntilNotPresent(() => App.WaitForElement("ElementId"));
```

### Navigation and Device

```csharp
// Navigate back
App.Back();

// App background/foreground
App.BackgroundApp();
App.ForegroundApp();

// Keyboard
App.DismissKeyboard();
bool isShown = App.IsKeyboardShown();
App.PressEnter();

// Orientation
App.SetOrientationLandscape();
App.SetOrientationPortrait();

// Theme (Android/iOS)
App.SetLightMode();
App.SetDarkMode();

// Screen recording (Android/iOS/Windows)
App.StartRecording();
// ... perform actions
App.StopRecording();

// Screenshots
App.Screenshot("TestName_Step1");
VerifyScreenshot();  // Automated screenshot comparison
```

## Test Categories

Use the most appropriate category per test:

```csharp
[Category(UITestCategories.Button)]
[Category(UITestCategories.Label)]
[Category(UITestCategories.Entry)]
[Category(UITestCategories.CollectionView)]
[Category(UITestCategories.ListView)]
[Category(UITestCategories.Navigation)]
[Category(UITestCategories.Layout)]
[Category(UITestCategories.Gestures)]
[Category(UITestCategories.Shell)]
[Category(UITestCategories.Border)]
[Category(UITestCategories.Image)]
[Category(UITestCategories.Slider)]
[Category(UITestCategories.Stepper)]
```

Apply the category that match the primary control being tested. For tests involving multiple controls, use the most relevant category.

**Only ONE category per test method.**

## Platform-Specific Tests

### Creating Platform-Specific Test Projects

Each platform can have unique tests:

**Android Tests** (`src/Controls/tests/TestCases.Android.Tests/IssueXXXXX.cs`):

```csharp
[Test]
[Category(UITestCategories.Android)]
public void AndroidOnlyFeature()
{
App.ToggleWifi();
// Android-specific test logic
}
```

**iOS Tests** (`src/Controls/tests/TestCases.iOS.Tests/IssueXXXXX.cs`):

```csharp
[Test]
[Category(UITestCategories.iOS)]
public void iOSOnlyFeature()
{
    App.Shake();
    // iOS-specific test logic
}
```

## Running Tests

### Quick Start: Running Specific Tests

For rapid development and debugging, you can run specific tests directly:

**Android:**

1. Deploy TestCases.HostApp to Android emulator/device:
   ```bash
   # Use local dotnet if available, otherwise use global dotnet
   ./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
   # OR:
   dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
   ```

2. Run specific test:
   ```bash
   dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj --filter "FullyQualifiedName~Issue11311"
   ```

**iOS (3-step process):**

1. **Find iPhone Xs with highest API level:**
   ```bash
   # Extract UDID of iPhone Xs with highest iOS version
   UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

   # Verify UDID was found
   if [ -z "$UDID" ]; then
       echo "ERROR: No iPhone Xs simulator found. Please create an iPhone Xs simulator before running iOS tests."
       exit 1
   fi

   echo "Using iPhone Xs with UDID: $UDID"
   ```

2. **Build the iOS app:**
   ```bash
   # Use local dotnet if available, otherwise use global dotnet
   ./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios
   # OR:
   dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios
   ```

3. **Boot simulator and install app (non-blocking):**
   ```bash
   # Boot the simulator (will error if already booted, which is fine)
   xcrun simctl boot $UDID 2>/dev/null || true

   # Install the app to the simulator
   xcrun simctl install $UDID artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app

   # Verify simulator is booted
   xcrun simctl list devices | grep "$UDID"
   ```

4. **Run specific test:**
   ```bash
   dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj --filter "FullyQualifiedName~Issue11311"
   ```

**MacCatalyst:**

1. Deploy TestCases.HostApp to MacCatalyst:
   ```bash
   # Use local dotnet if available, otherwise use global dotnet
   ./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-maccatalyst -t:Run
   # OR:
   dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-maccatalyst -t:Run
   ```

2. Run specific test:
   ```bash
   dotnet test src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj --filter "FullyQualifiedName~Issue11311"
   ```

### Full Test Suite: Using Cake Build System

For comprehensive CI-like test runs:

Android, iOS, MacCatalyst, and Windows CI runs retry once after a failure using
the shared UI test runner. For an exact category group such as
`TestCategory=Editor|TestCategory=Effects|TestCategory=Essentials`, the second
attempt runs only the individual tests that failed, except for feature matrices.
Exact NUnit `fullname` selectors distinguish parameterized cases and retain the
original category/shard constraint. A generated `retry.runsettings` file uses
`NUnit.Where` instead of a
VSTest command-line filter, avoiding argument quoting and length limits.
NUnit XML provides inherited fixture and method categories; only categories in
the original group are selected. Missing, incomplete, unmappable, or ambiguous
test names fall back to the original filter with a warning. Custom filters retain
their original selection, and local runs do not retry. No extra retry is added.

Feature-matrix fixtures follow the `*FeatureTests` class naming convention.
When one fails, the retry selects that exact NUnit fixture instance, including
passing prerequisite tests, so ordered navigation and shared application state
are preserved. Other feature matrices and passing ordinary tests are not
selected. The original category/shard constraint still applies: tests outside
that lane are not added. This costs a full fixture rerun only after a failure,
without adding per-test app resets or extra retry attempts. New feature matrices
must retain this naming convention to receive fixture-level retries.

Feature-matrix subpage navigation belongs in fixture setup, not an `Order(1)`
test. Override `_GalleryUITest.GallerySubPageButton` with the entry button's
AutomationId and remove the corresponding wait/tap from the first test. Setup
then opens the subpage even when only a later method or a category shard is
selected. This reuses the existing once-per-fixture navigation without enabling
`ResetAfterEachTest`. Navigation setup alone does not remove other focus,
flyout, data, or nested-navigation dependencies; whole-fixture retries preserve
those existing ordered flows within the original shard. Tests should still
establish their own prerequisites, particularly when split across shards or
selected individually for local runs.

Explicitly separated preparation and test-project build phases on Android, iOS,
MacCatalyst, and Windows CI allow at most five total attempts per failed phase,
before test execution starts. These retries do not restart completed tests.
Local runs and device-test Cake targets do not retry these phases. Shared agent
provisioning also uses bounded setup-only recovery; see
[Setup-only retries](../.github/docs/maui-ci-facts.md#setup-only-retries) for its
coverage and limits.

NUnit XML is checked whether VSTest succeeds or throws. Fixture-only failures
such as `OneTimeTearDown` errors in other fixtures still retry their affected
categories because they cannot safely be attributed to individual tests. They
are recorded as TRX run errors if they persist. Missing or malformed reports are
logged without replacing an existing VSTest exception.

The published TRX combines untouched first-attempt results with retry results.
Failures not reported by the retry, or only skipped/inconclusive on retry, remain
failures. Failed NUnit cases must be confirmed passed by complete retry NUnit XML
for the same fully qualified name and assembly, even if VSTest reports success.
Fixture errors are cleared
only when retry NUnit XML confirms that the same fixture in the same assembly
completed successfully; omitted, skipped, or incomplete fixtures remain run
errors, even if all retained test cases passed.

Available raw TRX and NUnit XML files are copied before XML parsing,
reconciliation, or retry selection, including for full-filter retries. The
first attempt's snapshots are under `nunit-<id>/raw/` and the retry's snapshots
under `nunit-<id>/retry/raw/`, beneath the result directory. Retry output always
uses a separate directory, even when the first TRX is missing or malformed.
Only the combined top-level TRX is published; if the first TRX cannot be read,
the retry TRX is published instead. If raw reports cannot be archived, the runner
stops without retrying or overwriting the available evidence.

**Android:**

```powershell
./build.ps1 -Script eng/devices/android.cake --target=uitest-build
./build.ps1 -Script eng/devices/android.cake --target=uitest
```

**iOS:**

```powershell
./build.ps1 -Script eng/devices/ios.cake --target=uitest-build
./build.ps1 -Script eng/devices/ios.cake --target=uitest
```

**Windows:**

```powershell
./build.ps1 -Script eng/devices/windows.cake --target=uitest-build
./build.ps1 -Script eng/devices/windows.cake --target=uitest
```

**MacCatalyst:**

```powershell
./build.ps1 -Script eng/devices/catalyst.cake --target=uitest-build
./build.ps1 -Script eng/devices/catalyst.cake --target=uitest
```

### Filter Tests by Category

```bash
# Single category

dotnet cake eng/devices/android.cake --target=uitest --test-filter="TestCategory=Button"

# Multiple categories

dotnet cake eng/devices/android.cake --target=uitest --test-filter="TestCategory=Button|TestCategory=Navigation"

# Specific test name

dotnet cake eng/devices/android.cake --target=uitest --test-filter="FullyQualifiedName~IssueXXXXX"
```

### Visual Studio

1. Build test project (TestCases.Shared.Tests.csproj)
2. Tests appear in Test Explorer
3. Right-click and run

### Visual Studio Code

1. Install C# Dev Kit with .NET MAUI Extension
2. Build test project first
3. Open Testing panel
4. Click play icon next to test

## Best Practices

### AutomationId Best Practices

```xaml
// BAD - Generic names
<Button AutomationId="Button1"/>
<Label AutomationId="Label2"/>

// GOOD - Descriptive names
<Button AutomationId="SubmitButton"/>
<Label AutomationId="StatusLabel"/>
```

Do:

* Always set AutomationId on testable elements.
* Use descriptive, meaningful identifiers.
* Keep IDs consistent across test runs.
* Use unique IDs within the same view.
* Document complex ID schemes.

Don't:

* Use generic IDs like "button1" or "test".
* Reuse the same ID for different elements.
* Include spaces or special characters.
* Change IDs between test runs.
* Rely on Text property for identification.

### Always Wait for Elements

```csharp
// BAD - May fail
App.Tap("ButtonId");

// GOOD - Wait first
App.WaitForElement("ButtonId");
App.Tap("ButtonId");
```

### Test Orientation Changes

```csharp
[Test]
public void ResponsiveLayoutTest()
{
    App.WaitForElement("MainLayout");
    VerifyScreenshot("Portrait");

    App.SetOrientationLandscape();
    App.WaitForElement("MainLayout");
    VerifyScreenshot("Landscape");
}
```

### Best Practices for Assertions

Do:

* Use `VerifyScreenshot()` as primary validation.
* Wait for elements before asserting.
* Use descriptive assertion messages.
* Test one concept per test method.
* Validate both positive and negative cases.

Don't:

* Assert immediately without waiting.
* Use `Thread.Sleep()` instead of proper waits.
* Test multiple unrelated concepts in one test.
* Skip assertions (tests should validate something).
* Rely solely on element existence without visual validation.

## Troubleshooting

### Android App Crashes on Launch

If you encounter navigation fragment errors or resource ID issues when launching the Android HostApp:

```
java.lang.IllegalArgumentException: No view found for id 0x7f0800f8 (com.microsoft.maui.uitests:id/inward) for fragment NavigationRootManager_ElementBasedFragment
```

**Solution:** Read the crash logs to find the full exception and investigate the root cause:

```bash
# Monitor logcat for the full crash details
adb logcat -c  # Clear logcat buffer
adb logcat | grep -E "(FATAL|AndroidRuntime|Exception|Error|Crash)"
```

**Debugging Steps:**

1. **Find the full exception** in logcat - look for the complete stack trace

2. **Investigate the root cause**:
   - Does the resource ID (`0x7f0800f8`) actually exist in the APK?
   - Is the fragment being created before the resource is available?
   - Are there initialization order issues?
   - Check for null references or missing dependencies

3. **If you can't determine the fix**, ask for guidance with:
   - The full exception stack trace
   - What you've tried so far
   - Any patterns you've noticed

3. Check Android emulator is running:
   ```bash
   adb devices
   ```

## Pre-Commit Checklist

Before committing UI tests:

- Both projects compile without errors
- HostApp has XAML with proper `AutomationId` values
- Test project references match AutomationIds
- Naming convention matches (IssueXXXXX)
- Only ONE `[Category]` attribute per test
- Tests pass locally on at least one platform
- No build warnings


## Example: Complete Test

```csharp
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue22769 : _IssuesUITest
{
    public Issue22769(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Background set to Transparent doesn't have the same behavior as BackgroundColor Transparent";
    
    [Test]
    [Category(UITestCategories.Navigation)]
    public void ModalPageBackgroundShouldBeTransparent()
    {
        // Wait for initial button
        App.WaitForElement("NavigateToModalButton");
        
        // Tap to navigate to modal
        App.Tap("NavigateToModalButton");
        
        // Verify modal page loaded
        App.WaitForElement("ModalPageLabel");
        
        // Verify transparent background allows seeing underlying content
        VerifyScreenshot();
    }
    
    [SetUp]
    public void Setup()
    {
        App.SetOrientationPortrait();
    }
}
```

## Xamarin.UITest Migration

If migrating from Xamarin.UITest:

| Xamarin.UITest | .NET MAUI Appium |
|---|---|
| `App.Tap(c => c.Marked("Id"))` | `App.Tap("Id")` |
| `App.WaitForElement(c => c.Marked("Id"))` | `App.WaitForElement("Id")` |
| `App.ScrollDownTo(...)` | `App.ScrollTo("ElementId", down: true)` |
| `App.SetOrientation(...)` | `App.SetOrientationPortrait()` |
| `App.Screenshot(...)` | `App.Screenshot(...)`/`VerifyScreenshot()` |

## Additional Resources

- [UITesting-Architecture.md](design/UITesting-Architecture.md) - CI/CD integration, advanced patterns, and architecture decisions
- [UI Testing Instructions](../.github/instructions/uitests.instructions.md) - Agent-specific UI testing guidelines
- [Appium Documentation](https://appium.io/docs/en/latest/)
- [NUnit Documentation](https://docs.nunit.org/)
- [.NET MAUI Testing Wiki](https://github.com/dotnet/maui/wiki/UITests)
- [UI Tests Pipeline](../eng/pipelines/ui-tests.yml) - CI/CD pipeline for UI tests

**Last Updated:** October 2025
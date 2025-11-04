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

### iOS/MacCatalyst-Specific

- macOS required
- Xcode with command-line tools installed
- iOS Simulator configured

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
   # View all iPhone Xs devices with their iOS versions
   xcrun simctl list devices available | awk '/^--.*iOS/ {version=$0} /iPhone Xs/ {print version " -> " $0}'

   # Extract UDID of iPhone Xs with highest iOS version (last in list)
   UDID=$(xcrun simctl list devices available | grep "iPhone Xs" | tail -1 | sed -n 's/.*(\([A-F0-9-]*\)).*/\1/p')

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

**Solution:** Build with `--no-incremental` to force a clean build and regenerate Android resource IDs:

```bash
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run --no-incremental
```

**Debugging Steps:**

1. Monitor logcat for crash details:
   ```bash
   adb logcat -c  # Clear logcat buffer
   adb logcat | grep -E "(FATAL|AndroidRuntime|Exception|Error|Crash)" &
   ```

2. Try clean build if incremental builds fail:
   ```bash
   dotnet clean src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj
   dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
   ```

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
- [Appium Documentation](http://appium.io/docs/en/about-appium/intro/)
- [NUnit Documentation](https://docs.nunit.org/)
- [.NET MAUI Testing Wiki](https://github.com/dotnet/maui/wiki/UITests)
- [GitHub Actions UI Tests Workflow](https://github.com/dotnet/maui/blob/main/.github/workflows/ui-tests.yml)

**Last Updated:** October 2025
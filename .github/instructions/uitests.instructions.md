---
applyTo: "src/Controls/tests/TestCases.Shared.Tests/**,src/Controls/tests/TestCases.HostApp/**"
---

# UI Testing Guidelines for .NET MAUI

## Overview

This document provides specific guidance for GitHub Copilot when writing UI tests for the .NET MAUI repository.



**Critical Principle**: UI tests should run on all applicable platforms (iOS, Android, Windows, MacCatalyst) by default unless there is a specific technical limitation.

## UI Test Structure

### Two-Project Requirement

**CRITICAL: Every UI test requires code in TWO separate projects:**

1. **HostApp UI Test Page** (`src/Controls/tests/TestCases.HostApp/Issues/`)
   - Create the actual UI page that demonstrates the feature or reproduces the issue
   - Use XAML with proper `AutomationId` attributes on interactive controls for test automation
   - Follow naming convention: `IssueXXXXX.xaml` and `IssueXXXXX.xaml.cs`
   - XXXXX should correspond to a GitHub issue number when applicable
   - Ensure the UI provides clear visual feedback for the behavior being tested
   - Code-behind must include `[Issue()]` attribute with tracker, number, description, and platform

2. **NUnit Test Implementation** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`)
   - Create corresponding Appium-based NUnit tests that inherit from `_IssuesUITest`
   - Use the `AutomationId` values to locate and interact with UI elements
   - Follow naming convention: `IssueXXXXX.cs` (matches the HostApp XAML file)
   - Include appropriate `[Category(UITestCategories.XYZ)]` attributes (only ONE per test)
   - Test should validate expected behavior through UI interactions and assertions

### Base Class and Infrastructure

- Each test class must inherit from `_IssuesUITest`
- The `_IssuesUITest` base class provides:
  - The `App` property for interacting with UI elements
  - Test initialization and setup
  - Helper methods for common UI test operations
- The test infrastructure automatically handles platform detection and page navigation

### Naming Conventions

**Test Files:**
- Pattern: `IssueXXXXX.cs` where XXXXX corresponds to a GitHub issue number
- Must match the corresponding XAML file in TestCases.HostApp

**Test Methods:**
- Use descriptive names that clearly explain what behavior is being verified
- ✅ Good: `VerifySafeAreaBottomPaddingWithKeyboard()`, `ButtonClickUpdatesLabel()`
- ❌ Bad: `Test1()`, `TestMethod()`, `RunTest()`

**AutomationId Values:**
- Always use unique, descriptive `AutomationId` values
- Reference the same `AutomationId` in both XAML and test code
- Use PascalCase for AutomationId values

## Complete Test Example

**HostApp Code-Behind** (`TestCases.HostApp/Issues/Issue12345.xaml.cs`):
```csharp
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12345, "Description of the issue being tested", PlatformAffected.All)]
public partial class Issue12345 : ContentPage
{
    public Issue12345()
    {
        InitializeComponent();
    }
}
```

**NUnit Test** (`TestCases.Shared.Tests/Tests/Issues/Issue12345.cs`):
```csharp
public class Issue12345 : _IssuesUITest
{
    public override string Issue => "Description of the issue being tested";

    public Issue12345(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.Layout)] // Pick the most appropriate category
    public void ButtonClickUpdatesLabel()
    {
        // Wait for element to be ready
        App.WaitForElement("TestButton");

        // Interact with the UI
        App.Tap("TestButton");

        // Verify expected behavior
        var labelText = App.FindElement("ResultLabel").GetText();
        Assert.That(labelText, Is.EqualTo("Expected Text"));

        // Optional: Visual verification
        VerifyScreenshot();
    }
}
```

## Common Patterns

### Waiting for Elements
```csharp
App.WaitForElement("AutomationId");
```

### Interacting with Elements
```csharp
App.Tap("AutomationId");
App.FindElement("AutomationId").GetText();
var rect = App.WaitForElement("AutomationId").GetRect();
```

### Assertions
```csharp
Assert.That(actualValue, Is.EqualTo(expectedValue).Within(tolerance));
Assert.That(rect.Height, Is.LessThanOrEqualTo(maxHeight));
```

### Screenshot Verification
```csharp
// Verify visual appearance (automated comparison)
VerifyScreenshot();

// With custom name
VerifyScreenshot("CustomTestName");

// Manual screenshot for debugging
App.Screenshot("TestStep1");
```

## Test Categories

### Category Guidelines
- Use appropriate categories from `UITestCategories`
- **Only ONE** `[Category]` attribute per test
- Pick the most specific category that applies

### Test Categories

**CRITICAL**: Always check [UITestCategories.cs](../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs) for the authoritative, complete list of categories.

**Selection rule**: Choose the MOST SPECIFIC category that applies to your test. If multiple categories seem applicable, choose the one that best describes the primary focus of the test.

**Common categories** (examples only - not exhaustive):
- **SafeArea**: `SafeAreaEdges` - Safe area and padding tests
- **Basic controls**: `Button`, `Label`, `Entry`, `Editor` - Specific control tests
- **Collection controls**: `CollectionView`, `ListView`, `CarouselView` - Collection control tests
- **Layout**: `Layout` - Layout-related tests
- **Navigation**: `Shell`, `Navigation`, `TabbedPage` - Navigation tests
- **Interaction**: `Gestures`, `Focus`, `Accessibility` - Interaction tests
- **Lifecycle**: `Window`, `Page`, `LifeCycle` - Page lifecycle tests

**List all categories programmatically**:
```bash
grep -E "public const string [A-Za-z]+ = " src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs
```

**Important**: When a new UI test category is added to `UITestCategories.cs`, also update `eng/pipelines/common/ui-tests.yml` to include the new category.

## Platform Coverage

### Default Behavior

**DO NOT** use platform-specific conditional compilation directives (`#if ANDROID`, `#if IOS`, `#if WINDOWS`, `#if MACCATALYST`, etc.) unless there is a specific reason.

Tests in the `TestCases.Shared.Tests` project should run on all applicable platforms by default. The test infrastructure automatically handles platform detection.

### When Platform Directives Are Acceptable

Only use platform-specific directives when:

1. **Platform-specific API is being tested** - The test validates behavior that only exists on one platform
2. **Known platform limitation** - There is a documented bug or limitation that prevents the test from running on a specific platform
3. **Different expected behavior** - The platforms are expected to behave differently for valid reasons

### Examples

**✅ Correct - Runs on all platforms:**
```csharp
[Test]
[Category(UITestCategories.SafeAreaEdges)]
public void SoftInputBehaviorTest()
{
    // This test runs on all applicable platforms
    App.WaitForElement("ContentGrid");
    // Test code...
}
```

**❌ Incorrect - Unnecessarily limits to one platform:**
```csharp
#if ANDROID
[Test]
[Category(UITestCategories.SafeAreaEdges)]
public void SoftInputBehaviorTest()
{
    // This unnecessarily limits the test to Android only
    // Unless there's a specific reason, tests should run on all platforms
    App.WaitForElement("ContentGrid");
    // Test code...
}
#endif
```

## Running UI Tests Locally

**CRITICAL: ALWAYS use the BuildAndRunHostApp.ps1 script to run UI tests. NEVER run `dotnet test` or `dotnet build` commands manually.**

### BuildAndRunHostApp.ps1 Script (ONLY Way to Run Tests)

**Script location**: `.github/scripts/BuildAndRunHostApp.ps1`

**Usage:**
```powershell
# Run specific test on Android
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -TestFilter "FullyQualifiedName~Issue12345"

# Run specific test on iOS
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "FullyQualifiedName~Issue12345"

# Run specific test on MacCatalyst
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform maccatalyst -TestFilter "FullyQualifiedName~Issue12345"

# Run tests by category
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform android -Category "SafeAreaEdges"

# Run specific test with custom device (iOS only)
pwsh .github/scripts/BuildAndRunHostApp.ps1 -Platform ios -TestFilter "Issue12345" -DeviceUdid "12345678-1234567890ABCDEF"
```

**What the script handles automatically:**
- ✅ Automatic device detection and boot (iPhone Xs for iOS, first available for Android)
- ✅ Building TestCases.HostApp (always fresh build)
- ✅ App installation and deployment
- ✅ Running your NUnit test via `dotnet test`
- ✅ Complete log capture to `CustomAgentLogsTmp/UITests/` directory:
  - `android-device.log` or `ios-device.log` - Device logs filtered to HostApp
  - `test-output.log` - Test execution output

**Why you must use the script:**
- The script ensures correct device targeting and environment variables
- It handles platform-specific quirks and setup requirements
- It provides consistent test execution across all platforms
- It captures logs automatically for debugging
- Manual `dotnet` commands often fail due to missing environment setup

### Prerequisites: Kill Existing Appium Processes

**CRITICAL**: Before running UITests with BuildAndRunHostApp.ps1, always kill any existing Appium processes. The UITest framework needs to start its own Appium server, and having a stale process running will cause the tests to fail with an error like:

```
AppiumServerHasNotBeenStartedLocallyException: The local appium server has not been started.
Time 120000 ms for the service starting has been expired!
```

**Solution: Always kill existing Appium processes before running tests:**

```bash
# Kill any Appium processes on port 4723
lsof -i :4723 | grep LISTEN | awk '{print $2}' | xargs kill -9 2>/dev/null && echo "✅ Killed existing Appium processes" || echo "ℹ️ No Appium processes running on port 4723"
```

**Why this is needed:** The UITest framework automatically starts and manages its own Appium server. If there's already an Appium process running (from a previous test run or manual testing), the framework will timeout trying to start a new one.

### Troubleshooting

**Android App Crashes on Launch:**

If you encounter navigation fragment errors or resource ID issues:
```
java.lang.IllegalArgumentException: No view found for id 0x7f0800f8 (com.microsoft.maui.uitests:id/inward) for fragment NavigationRootManager_ElementBasedFragment
```

**Solution:** Read the crash logs to find the actual exception:
```bash
# Monitor logcat for the crash
adb logcat | grep -E "(FATAL|AndroidRuntime|Exception|Error|Crash)"
```

**Debugging steps:**
1. **Find the exception** in logcat - look for the stack trace
2. **Investigate the root cause** - What line of code is throwing? Why?
3. **Check for null references** - Are required resources missing?
4. **Verify resource IDs exist** - Check if the ID referenced actually exists in the app
5. If you can't determine the fix, **ask for guidance** with the full exception details

**iOS App Crashes on Launch or Won't Start with Appium:**

If the iOS app crashes when launched by Appium or manually with `xcrun simctl launch`:

**Solution:** Read the crash logs to find the actual exception:
```bash
# Capture crash logs
xcrun simctl spawn booted log stream --predicate 'processImagePath contains "TestCases.HostApp"' --level=debug > /tmp/ios_crash.log 2>&1 &
LOG_PID=$!

# Try to launch the app
xcrun simctl launch $UDID com.microsoft.maui.uitests

# Wait a moment for crash
sleep 3

# Stop log capture
kill $LOG_PID

# Review the crash log
cat /tmp/ios_crash.log | grep -A 20 -B 5 "Exception"
```

**Debugging steps:**
1. **Find the exception** in the crash log - look for stack traces
2. **Investigate the root cause** - What's causing the crash?
3. **Check for missing resources** - Are all required files included in the bundle?
4. **Verify Info.plist** - Are required keys present?
5. **Check for platform-specific issues** - iOS version compatibility, permissions, etc.
6. If you can't determine the fix, **ask for guidance** with the full exception details

## Before Committing

Verify the following checklist before committing UI tests:

- [ ] Compile both the HostApp project and TestCases.Shared.Tests project successfully
- [ ] Verify AutomationId references match between XAML and test code
- [ ] Ensure file names follow the `IssueXXXXX` pattern and match between projects
- [ ] Ensure test methods have descriptive names
- [ ] Verify test inherits from `_IssuesUITest`
- [ ] Confirm only ONE `[Category]` attribute is used per test
- [ ] Verify tests run on all applicable platforms (iOS, Android, Windows, MacCatalyst) unless platform-specific
- [ ] Document any platform-specific limitations with clear comments
- [ ] Test passes locally on at least one platform

### Test State Management

- Tests should be independent and not rely on state from other tests
- The test infrastructure handles navigation to the test page and basic cleanup
- If your test modifies global app state, consider whether cleanup is needed
- Most tests don't require explicit cleanup as each test gets a fresh page instance

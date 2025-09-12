# .NET MAUI Appium Test Guide

This comprehensive guide documents all the APIs, patterns, and best practices for writing Appium tests in the .NET MAUI repository. It serves as both an instruction guide for developers and a reference for Copilot agents.

## Table of Contents

1. [Overview](#overview)
2. [Project Structure](#project-structure)
3. [Core API Reference](#core-api-reference)
4. [Test Base Classes](#test-base-classes)
5. [Common Testing Patterns](#common-testing-patterns)
6. [Step-by-Step Guide for New Tests](#step-by-step-guide-for-new-tests)
7. [Visual Regression Testing](#visual-regression-testing)
8. [Platform-Specific Testing](#platform-specific-testing)
9. [Configuration and Setup](#configuration-and-setup)
10. [Best Practices](#best-practices)
11. [Common Pitfalls](#common-pitfalls)

## Overview

The .NET MAUI test infrastructure uses **Appium** for cross-platform UI testing with **NUnit** as the testing framework. Tests are organized into categories and support visual regression testing with platform-specific implementations.

### Key Technologies
- **UITest.Appium** - Core Appium integration
- **UITest.Core** - Core testing utilities
- **NUnit.Framework** - Test framework
- **VisualTestUtils** - Visual regression testing
- **ImageMagick** - Image processing

## Project Structure

```
src/Controls/tests/TestCases.Shared.Tests/
├── Tests/
│   ├── Issues/           # Issue-specific tests (Issue1146.cs, Issue17022.cs, etc.)
│   ├── CollectionView/   # CollectionView-specific tests
│   ├── Concepts/         # Conceptual test categories
│   ├── FeatureMatrix/    # Feature matrix tests
│   └── *.cs             # Control-specific tests (ButtonUITests.cs, etc.)
├── ViewContainers/       # Remote interaction helpers
├── UITest.cs            # Main test base class
├── UITestCategories.cs  # Test categories definition
└── Various utility files
```

## Core API Reference

### IApp Interface (Primary Interaction API)

#### Element Discovery
```csharp
// Wait for element to appear (with timeout)
App.WaitForElement("AutomationId");
App.WaitForElement("AutomationId", "Custom error message", TimeSpan.FromSeconds(30));
App.WaitForElementTillPageNavigationSettled("AutomationId");

// Wait for element to disappear
App.WaitForNoElement("AutomationId");
App.WaitForNoElement("AutomationId", "Custom error message", TimeSpan.FromMinutes(1));

// Find elements
IUIElement element = App.FindElement("AutomationId");
IReadOnlyCollection<IUIElement> elements = App.FindElements("AutomationId");

// Query elements (advanced)
var query = App.Query.ByAccessibilityId("AutomationId").First();
var queryByClass = App.Query.ByClass("XCUIElementTypeNavigationBar").First();
```

#### User Interactions
```csharp
// Tap/Click
App.Tap("AutomationId");
element.Click();

// Text input
App.EnterText("AutomationId", "text to enter");
App.ClearText("AutomationId");

// Drag and Drop
App.DragAndDrop("sourceAutomationId", "targetAutomationId");

// Swipe gestures
App.SwipeLeftToRight("AutomationId");
App.SwipeRightToLeft("AutomationId");

// Scroll operations
App.ScrollDown("AutomationId");
App.ScrollUp("AutomationId");
```

#### Navigation and App State
```csharp
// Navigation
App.Back();
App.NavigateToGallery("Gallery Name");
App.NavigateTo("Test Name");

// App state management
App.ResetApp();
App.Screenshot(); // Returns byte[] or saves to file

// Device operations
App.SetOrientationPortrait();
App.ToggleSystemAnimations(false); // Android/Mac only
```

### IUIElement Interface (Element-specific operations)

```csharp
// Get element properties
string text = element.GetText();
Rectangle rect = element.GetRect();
T attribute = element.GetAttribute<T>("AttributeName");

// Element interactions
element.Click();
element.Clear();
element.SendKeys("text");
```

### Advanced Query Operations

```csharp
// XPath queries (iOS/Mac)
App.FindElement(AppiumQuery.ByXPath("//XCUIElementTypeWindow"));

// Complex queries
var backButton = App.Query.ByClass("XCUIElementTypeNavigationBar")
                         .First()
                         .ByClass("XCUIElementTypeButton")
                         .First();
```

## Test Base Classes

### 1. UITest (Main Base Class)

The primary base class for all UI tests with visual regression capabilities.

```csharp
public abstract class UITest : UITestBase
{
    protected UITest(TestDevice testDevice) : base(testDevice) { }
    
    // Key methods:
    public void VerifyScreenshot(string? name = null, /* params */);
    public void VerifyScreenshotOrSetException(ref Exception? exception, /* params */);
    protected void VerifyInternetConnectivity();
}
```

**Usage Example:**
```csharp
public class MyTest : UITest
{
    public MyTest(TestDevice device) : base(device) { }
    
    [Test]
    public void TestSomething()
    {
        // Test implementation
        VerifyScreenshot(); // Visual regression test
    }
}
```

### 2. _IssuesUITest (Issue-specific Tests)

Base class for testing specific GitHub issues.

```csharp
public abstract class _IssuesUITest : UITest
{
    public abstract string Issue { get; }
    
    // Automatically navigates to the issue in FixtureSetup
    private void NavigateToIssue(string issue);
}
```

**Usage Example:**
```csharp
public class Issue1146 : _IssuesUITest
{
    public override string Issue => "Disabled Switch in Button Gallery not rendering on all devices";
    
    public Issue1146(TestDevice testDevice) : base(testDevice) { }
    
    [Test]
    [Category(UITestCategories.Switch)]
    public void TestSwitchDisable()
    {
        App.WaitForElement("switch");
        // Test implementation
    }
}
```

### 3. CoreGalleryBasePageTest (Gallery-based Tests)

Base class for testing control galleries.

```csharp
public abstract class CoreGalleryBasePageTest : UITest
{
    protected abstract void NavigateToGallery();
}
```

### 4. _ViewUITests (View Control Tests)

Base class for testing view controls with common state and event testing.

```csharp
public abstract class _ViewUITests : CoreGalleryBasePageTest
{
    // Common tests for all views
    [Test] public virtual void IsEnabled();
    [Test] public virtual void IsVisible();
    
    // Helper methods
    internal StateViewContainerRemote GoToStateRemote();
    internal EventViewContainerRemote GoToEventRemote();
    internal ViewContainerRemote GoToRemote();
}
```

## Common Testing Patterns

### 1. Issue Testing Pattern

```csharp
public class IssueXXXXX : _IssuesUITest
{
    public override string Issue => "Brief description of the issue";
    
    public IssueXXXXX(TestDevice device) : base(device) { }
    
    [Test]
    [Category(UITestCategories.SomeCategory)]
    public void TestMethod()
    {
        // 1. Wait for required elements
        App.WaitForElement("RequiredElement");
        
        // 2. Perform actions
        App.Tap("ButtonId");
        
        // 3. Verify results
        var result = App.FindElement("ResultLabel").GetText();
        Assert.AreEqual("Expected", result);
        
        // 4. Optional: Visual verification
        VerifyScreenshot();
    }
}
```

### 2. Control Gallery Testing Pattern

```csharp
public class ButtonUITests : _ViewUITests
{
    const string ButtonGallery = "Button Gallery";
    
    public ButtonUITests(TestDevice device) : base(device) { }
    
    protected override void NavigateToGallery()
    {
        App.NavigateToGallery(ButtonGallery);
    }
    
    [Test]
    [Category(UITestCategories.Button)]
    public void Clicked()
    {
        var remote = GoToEventRemote();
        
        var textBeforeClick = remote.GetEventLabel().GetText();
        Assert.That(textBeforeClick, Is.EqualTo("Event: Clicked (none)"));
        
        remote.TapView();
        
        var textAfterClick = remote.GetEventLabel().GetText();
        Assert.That(textAfterClick, Is.EqualTo("Event: Clicked (fired 1)"));
    }
}
```

### 3. Property Testing with ViewContainerRemote

```csharp
[Test]
public void TestProperty()
{
    var remote = GoToStateRemote();
    
    // Get property value via BindableProperty
    var enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
    Assert.That(enabled, Is.True);
    
    // Change state
    remote.TapStateButton();
    
    // Verify property changed
    enabled = remote.GetProperty<bool>(View.IsEnabledProperty);
    Assert.That(enabled, Is.False);
}
```

### 4. Cross-Platform Testing Pattern

```csharp
[Test]
[TestCase("NewNavigationPageButton", false)]
[TestCase("NewNavigationPageTransparentButton", false)]
public void CrossPlatformTest(string buttonId, bool expectedBehavior)
{
    App.WaitForElement(buttonId).Click();
    
    if (Device == TestDevice.iOS || Device == TestDevice.Mac)
    {
        // iOS-specific verification
        var element = App.FindElement("iOSSpecificElement");
        Assert.IsNotNull(element);
    }
    else if (Device == TestDevice.Android)
    {
        // Android-specific verification
        var element = App.FindElement("AndroidSpecificElement");
        Assert.IsNotNull(element);
    }
    else // Windows
    {
        // Windows-specific verification
        var element = App.FindElement("WindowsSpecificElement");
        Assert.IsNotNull(element);
    }
}
```

## Step-by-Step Guide for New Tests

### Step 1: Determine Test Type

Choose the appropriate base class:
- **Issue fix**: Use `_IssuesUITest`
- **Control testing**: Use `_ViewUITests` 
- **Gallery testing**: Use `CoreGalleryBasePageTest`
- **Complex scenario**: Use `UITest` directly

### Step 2: Create Test Class

```csharp
// For Issue tests:
public class IssueXXXXX : _IssuesUITest
{
    public override string Issue => "Issue description from GitHub";
    public IssueXXXXX(TestDevice device) : base(device) { }
}

// For Control tests:
public class MyControlUITests : _ViewUITests
{
    const string MyControlGallery = "MyControl Gallery";
    
    public MyControlUITests(TestDevice device) : base(device) { }
    
    protected override void NavigateToGallery()
    {
        App.NavigateToGallery(MyControlGallery);
    }
}
```

### Step 4: Write Test Methods

```csharp
[Test]
[Category(UITestCategories.YourCategory)]
public void TestMethodName()
{
    // 1. Setup - Navigate to test scenario (handled by base class for issues)
    
    // 2. Wait for elements to be ready
    App.WaitForElement("RequiredElement");
    
    // 3. Perform user actions
    App.Tap("ButtonToTest");
    App.EnterText("InputField", "test data");
    
    // 4. Verify expected behavior
    var result = App.FindElement("ResultElement").GetText();
    Assert.AreEqual("Expected Result", result);
    
    // 5. Optional: Visual verification
    VerifyScreenshot();
}
```

### Step 5: Add Proper Categories

Use categories from `UITestCategories.cs`:

```csharp
[Category(UITestCategories.Button)]
[Category(UITestCategories.Navigation)]
[Category(UITestCategories.Visual)]
```

### Step 6: Handle Platform Differences

```csharp
#if IOS
[Test]
public void iOSSpecificTest() { /* iOS only */ }
#endif

// Or conditional logic within tests:
[Test]
public void CrossPlatformTest()
{
    if (Device == TestDevice.iOS)
    {
        // iOS-specific code
    }
    else if (Device == TestDevice.Android)
    {
        // Android-specific code
    }
}
```

## Visual Regression Testing

### Basic Screenshot Verification

```csharp
[Test]
public void VisualTest()
{
    App.WaitForElement("TestElement");
    
    // Take and verify screenshot
    VerifyScreenshot();
    
    // Named screenshot
    VerifyScreenshot("CustomScreenshotName");
    
    // With tolerance for dynamic content
    VerifyScreenshot("DynamicContent", tolerance: 2.0);
    
    // With cropping
    VerifyScreenshot("CroppedView", cropTop: 50, cropBottom: 100);
}
```

### Batch Screenshot Verification

```csharp
[Test]
public void MultipleScreenshots()
{
    Exception? exception = null;
    
    App.WaitForElement("FirstView");
    VerifyScreenshotOrSetException(ref exception, "FirstView");
    
    App.Tap("NextButton");
    App.WaitForElement("SecondView");
    VerifyScreenshotOrSetException(ref exception, "SecondView");
    
    if (exception is not null) 
        throw exception;
}
```

### Screenshot Parameters

```csharp
VerifyScreenshot(
    name: "TestName",                    // Optional: Screenshot name
    retryDelay: TimeSpan.FromSeconds(1), // Optional: Retry delay
    cropTop: 100,                        // Optional: Crop from top
    cropBottom: 50,                      // Optional: Crop from bottom
    tolerance: 3.0                       // Optional: Tolerance percentage (0-15%)
#if MACUITEST || WINTEST
    , includeTitleBar: false             // Optional: Include title bar
#endif
);
```

## Platform-Specific Testing

### Conditional Compilation

```csharp
#if ANDROID
    // Android-specific code
#elif IOS
    // iOS-specific code
#elif MACCATALYST
    // Mac Catalyst-specific code
#elif WINDOWS
    // Windows-specific code
#endif
```

### Runtime Platform Detection

```csharp
[Test]
public void CrossPlatformTest()
{
    switch (Device)
    {
        case TestDevice.Android:
            // Android logic
            break;
        case TestDevice.iOS:
            // iOS logic
            break;
        case TestDevice.Mac:
            // Mac logic
            break;
        case TestDevice.Windows:
            // Windows logic
            break;
    }
}
```

### Platform-Specific Constants

```csharp
public abstract class _IssuesUITest : UITest
{
#if ANDROID
    protected const string FlyoutIconAutomationId = "Open navigation drawer";
    protected const string BackButtonAutomationId = "Navigate up";
#else
    protected const string FlyoutIconAutomationId = "OK";
    protected const string BackButtonAutomationId = "Back";
#endif
}
```

## Configuration and Setup

### Test Configuration

The framework automatically configures platform-specific settings:

```csharp
public override IConfig GetTestConfig()
{
    IConfig config = new Config();
    config.SetProperty("AppId", "com.microsoft.maui.uitests");
    
    switch (_testDevice)
    {
        case TestDevice.Android:
            config.SetProperty("DeviceName", Environment.GetEnvironmentVariable("DEVICE_SKIN") ?? "");
            config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("PLATFORM_VERSION") ?? "");
            break;
        case TestDevice.iOS:
            config.SetProperty("DeviceName", Environment.GetEnvironmentVariable("DEVICE_NAME") ?? "iPhone Xs");
            config.SetProperty("PlatformVersion", Environment.GetEnvironmentVariable("PLATFORM_VERSION") ?? "18.0");
            break;
        // Additional platform configurations...
    }
    
    return config;
}
```

### Test Setup and Teardown

```csharp
public override void TestSetup()
{
    base.TestSetup();
    
    // Set portrait orientation for mobile devices
    var device = App.GetTestDevice();
    if (device == TestDevice.Android || device == TestDevice.iOS)
    {
        try
        {
            App.SetOrientationPortrait();
        }
        catch
        {
            Thread.Sleep(1000);
            App.SetOrientationPortrait();
        }
    }
}

[TearDown]
public void TestTearDown()
{
    if (Device != TestDevice.Windows)
        this.Back();
}
```

### Environment Variables

Key environment variables used by the test framework:

- `DEVICE_SKIN` - Android device skin
- `PLATFORM_VERSION` - Platform version for iOS/Android
- `DEVICE_UDID` - Device UDID for physical devices
- `DEVICE_NAME` - Device name for iOS simulator
- `WINDOWS_APP_PATH` - Path to Windows application
- `BUILD_ARTIFACTSTAGINGDIRECTORY` - CI artifacts directory
- `TEST_CONFIGURATION_ARGS` - Additional test configuration

## Best Practices

### 1. Reliable Element Waiting

```csharp
// ✅ Good: Wait with meaningful timeout and error message
App.WaitForElement("ElementId", "Element did not appear", TimeSpan.FromSeconds(30));

// ❌ Bad: No waiting or insufficient timeout
App.FindElement("ElementId"); // May fail if element not ready
App.WaitForElement("ElementId", timeout: TimeSpan.FromSeconds(1)); // Too short
```

### 2. Meaningful Test Names and Categories

```csharp
// ✅ Good: Descriptive test name and appropriate category
[Test]
[Category(UITestCategories.Button)]
[Category(UITestCategories.Navigation)]
public void ButtonNavigatesToCorrectPage()
{
    // Clear test purpose
}

// ❌ Bad: Vague test name, no category
[Test]
public void Test1()
{
    // Unclear purpose
}
```

### 3. Robust Element Identification

```csharp
// ✅ Good: Use AutomationId (most reliable)
App.WaitForElement("LoginButton");

// ✅ Acceptable: Use accessibility labels
App.WaitForElement("Navigate up"); // Android back button

// ❌ Avoid: Text-based selectors (fragile)
App.WaitForElement("Click me"); // Text can change
```

### 4. Proper Error Handling

```csharp
[Test]
public void TestWithErrorHandling()
{
    try
    {
        App.WaitForElement("Element");
        // Test logic
    }
    catch (TimeoutException)
    {
        // Take screenshot for debugging
        App.Screenshot();
        throw;
    }
}
```

### 5. Visual Testing Best Practices

```csharp
// ✅ Good: Use tolerance for dynamic content
VerifyScreenshot("DynamicContent", tolerance: 2.0);

// ✅ Good: Crop out dynamic areas
VerifyScreenshot("StaticArea", cropTop: 100, cropBottom: 50);

// ✅ Good: Batch verification for multiple screenshots
Exception? exception = null;
VerifyScreenshotOrSetException(ref exception, "Screen1");
VerifyScreenshotOrSetException(ref exception, "Screen2");
if (exception is not null) throw exception;
```

### 6. Platform-Specific Handling

```csharp
[Test]
public void PlatformSpecificTest()
{
    // Handle platform differences explicitly
    if (Device == TestDevice.Android)
    {
        // Android animations may need time to settle
        Thread.Sleep(350);
    }
    
    // Platform-specific element IDs
    var backButtonId = Device == TestDevice.Android ? "Navigate up" : "Back";
    App.WaitForElement(backButtonId);
}
```

### 7. Memory and Performance Testing

```csharp
[Test]
[Category(UITestCategories.Performance)]
public void MemoryTest()
{
    // Perform memory-intensive operations
    
    // Use helper for memory test validation
    App.AssertMemoryTest();
}
```

## Common Pitfalls

### 1. Race Conditions

```csharp
// ❌ Problem: Not waiting for element
App.Tap("Button");
var result = App.FindElement("Result").GetText(); // May fail

// ✅ Solution: Wait for element after action
App.Tap("Button");
App.WaitForElement("Result");
var result = App.FindElement("Result").GetText();
```

### 2. Platform Assumptions

```csharp
// ❌ Problem: Assuming all platforms behave the same
App.WaitForElement("Back"); // iOS/Windows
App.Tap("Back");

// ✅ Solution: Handle platform differences
var backButton = Device == TestDevice.Android ? "Navigate up" : "Back";
App.WaitForElement(backButton);
App.Tap(backButton);
```

### 3. Flaky Visual Tests

```csharp
// ❌ Problem: Not accounting for animations/timing
VerifyScreenshot(); // May fail due to animations

// ✅ Solution: Add delays or tolerance
if (Device == TestDevice.Android)
{
    Thread.Sleep(350); // Wait for animations
}
VerifyScreenshot("AnimatedContent", tolerance: 3.0);
```

### 4. Inadequate Test Categories

```csharp
// ❌ Problem: No categories
[Test]
public void ButtonTest() { }

// ✅ Solution: Proper categorization
[Test]
[Category(UITestCategories.Button)]
[Category(UITestCategories.Gestures)]
public void ButtonTapTest() { }
```

### 5. Test Setup and Failure Handling

The test framework includes built-in retry logic for test setup and automatic screenshot capture when tests fail. This helps handle transient issues and provides debugging information when problems occur.

### 6. Incorrect Visual Test Environment

```csharp
// The framework validates correct device configurations for visual tests:
// Android: API30 emulator, 1080x1920, 420dpi
// iOS: iPhone Xs (iOS 18.0) or iPhone X (iOS 16.4)
// Failure to use correct configuration will cause test failures
```

### 7. Not Using Utility Extensions

```csharp
// ❌ Problem: Manual navigation logic
App.FindElement("SearchBar").Clear();
App.FindElement("SearchBar").SendKeys("Gallery Name");
App.FindElement("GoToTestButton").Click();

// ✅ Solution: Use utility extensions
App.NavigateToGallery("Gallery Name");
```

## Utility Classes and Extensions

### UtilExtensions Class

```csharp
// Navigation helpers
App.NavigateToGallery("Gallery Name");
App.NavigateTo("Test Name");

// Platform-specific back navigation
this.Back(); // Works across all platforms

// Memory testing
App.AssertMemoryTest();

// Rectangle helpers
int centerX = rect.CenterX();
int centerY = rect.CenterY();
```

### ViewContainer Remote Classes

#### BaseViewContainerRemote
- `GetView()` - Get the main view element
- `GetViews()` - Get collection of view elements
- `TapView()` - Tap the main view
- `GetProperty<T>(BindableProperty)` - Get view property value

#### StateViewContainerRemote
- `GetStateLabel()` - Get state label element
- `TapStateButton()` - Tap state change button

#### EventViewContainerRemote
- `GetEventLabel()` - Get event label element

### Test Categories (UITestCategories)

Available categories for test classification:
- Control-specific: `Button`, `Label`, `Entry`, `CollectionView`, etc.
- Feature-specific: `Navigation`, `Gestures`, `Animation`, `Focus`, etc.
- Platform-specific: `Shell`, `TabbedPage`, `FlyoutPage`, etc.
- Quality-specific: `Performance`, `Accessibility`, `Visual`, `Flaky`, etc.

---

## Conclusion

This guide provides comprehensive coverage of the .NET MAUI Appium testing framework. When writing new tests:

1. **Choose the appropriate base class** based on your test scenario
2. **Use proper AutomationIds** for reliable element identification  
3. **Handle platform differences** explicitly
4. **Add appropriate test categories** for organization
5. **Include visual regression tests** when testing UI behavior
6. **Follow established naming conventions** and patterns
7. **Handle timing and animations** appropriately for each platform

For additional examples, refer to the existing test files in the `Tests/` directory, particularly the `Issues/` folder for real-world test scenarios.
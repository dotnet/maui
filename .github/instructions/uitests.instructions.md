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
   - **Prefer C# only** (`.cs` file) unless testing XAML-specific features (bindings, templates, styles)
   - Add `AutomationId` attributes on interactive controls for test automation
   - Follow naming convention: `IssueXXXXX.cs` (C# only) or `IssueXXXXX.xaml` + `IssueXXXXX.xaml.cs` (when XAML required)
   - XXXXX should correspond to a GitHub issue number when applicable
   - Ensure the UI provides clear visual feedback for the behavior being tested
   - Class must include `[Issue()]` attribute with tracker, number, description, and platform

2. **NUnit Test Implementation** (`src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`)
   - Create corresponding Appium-based NUnit tests that inherit from `_IssuesUITest`
   - Use the `AutomationId` values to locate and interact with UI elements
   - Follow naming convention: `IssueXXXXX.cs` (matches the HostApp page file)
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
- Must match the corresponding HostApp page file name in TestCases.HostApp (either `.cs` only or `.xaml`)

**Test Methods:**
- Use descriptive names that clearly explain what behavior is being verified
- ✅ Good: `VerifySafeAreaBottomPaddingWithKeyboard()`, `ButtonClickUpdatesLabel()`
- ❌ Bad: `Test1()`, `TestMethod()`, `RunTest()`

**AutomationId Values:**
- Always use unique, descriptive `AutomationId` values
- Reference the same `AutomationId` in both C# code (or XAML if used) and test code
- Use PascalCase for AutomationId values

## Complete Test Example

### Example 1: C# Only (Preferred for Most Tests)

**HostApp Page** (`TestCases.HostApp/Issues/Issue12345.cs`):
```csharp
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12345, "Button click updates label text", PlatformAffected.All)]
public class Issue12345 : ContentPage
{
    public Issue12345()
    {
        var resultLabel = new Label
        {
            Text = "Initial Text",
            AutomationId = "ResultLabel"
        };

        Content = new VerticalStackLayout
        {
            Children =
            {
                new Button
                {
                    Text = "Click Me",
                    AutomationId = "TestButton",
                    Command = new Command(() => resultLabel.Text = "Expected Text")
                },
                resultLabel
            }
        };
    }
}
```

### Example 2: XAML (When Testing XAML-Specific Features)

**HostApp XAML** (`TestCases.HostApp/Issues/Issue12346.xaml`):
```xaml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             x:Class="Maui.Controls.Sample.Issues.Issue12346">
    <VerticalStackLayout>
        <Button Text="Click Me" 
                AutomationId="TestButton"
                Clicked="OnButtonClicked" />
        <Label Text="Initial Text" 
               x:Name="ResultLabel"
               AutomationId="ResultLabel" />
    </VerticalStackLayout>
</ContentPage>
```

**HostApp Code-Behind** (`TestCases.HostApp/Issues/Issue12346.xaml.cs`):
```csharp
namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 12346, "Testing XAML binding behavior", PlatformAffected.All)]
public partial class Issue12346 : ContentPage
{
    public Issue12346()
    {
        InitializeComponent();
    }

    void OnButtonClicked(object sender, EventArgs e)
    {
        ResultLabel.Text = "Expected Text";
    }
}
```

### NUnit Test (Same for Both Examples)

**NUnit Test** (`TestCases.Shared.Tests/Tests/Issues/Issue12345.cs` or `Issue12346.cs`):
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

// With tolerance (0.0-100.0 percentage) - use sparingly
VerifyScreenshot(tolerance: 0.5); // Allow 0.5% difference for cross-machine rendering variance

// PREFERRED: Keep retrying for up to 2 seconds (for animations)
VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));

// Combined: tolerance for rendering variance + retryTimeout for timing
VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

// Manual screenshot for debugging
App.Screenshot("TestStep1");
```

**CRITICAL - VerifyScreenshot() Built-in Features:**

`VerifyScreenshot()` **already includes** stability mechanisms. Do NOT add redundant delays:

| Feature | Behavior | Parameter |
|---------|----------|-----------|
| **Android delay** | Automatic 350ms wait for animations | Built-in, cannot override |
| **Retry logic** | Default: retries once; with retryTimeout: keeps retrying | Built-in |
| **Retry delay** | 500ms delay between retry attempts | `retryDelay: TimeSpan` (customizable) |
| **Retry timeout** | Total time to keep retrying | `retryTimeout: TimeSpan` (PREFERRED for flaky tests) |
| **Tolerance** | Allow percentage difference (0-100) | `tolerance: double` (default: 0.0) |

**When to customize:**
- ✅ Use `retryTimeout` parameter for animations with variable timing (PREFERRED approach)
- ✅ Use small `tolerance` (0.5%) for cross-machine rendering variance, NOT to hide timing issues
- ✅ Use `retryDelay` if you need to change the delay between retry attempts
- ❌ **DO NOT** add `Task.Delay()` or `Thread.Sleep()` before `VerifyScreenshot()` - use `retryTimeout` instead

## Writing Robust UI Tests

### Best Practices for Screenshot Tests

When writing tests that use `VerifyScreenshot()`, follow these patterns to avoid flakiness:

```
┌─────────────────────────────────────────────────────────────────┐
│ 1. UNDERSTAND TEST INFRASTRUCTURE                               │
│    - Read UITest.cs base class implementation                   │
│    - Understand built-in retry/delay/tolerance mechanisms       │
│    - Check what helpers/extensions already exist                │
├─────────────────────────────────────────────────────────────────┤
│ 2. USE PROPER WAITING PATTERNS                                  │
│    - Use WaitForElement before interacting with elements        │
│    - Use retryTimeout for screenshots after animations          │
│    - Never use arbitrary Task.Delay() before VerifyScreenshot   │
├─────────────────────────────────────────────────────────────────┤
│ 3. APPLY MINIMAL TOLERANCES                                     │
│    - Use retryTimeout for timing issues (preferred)             │
│    - Use small tolerance (0.5%) only for rendering variance     │
│    - Never use tolerance > 5% without justification             │
└─────────────────────────────────────────────────────────────────┘
```

### Common Flaky Test Patterns

| Symptom | Root Cause | Fix Pattern | Anti-Pattern |
|---------|------------|-------------|--------------|
| **Visual diff in screenshot** | Animation not finished | `VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2))` | ❌ Adding `Task.Delay()` before |
| **Element not found** | Element not rendered yet | `App.WaitForElement("Id", timeout: TimeSpan.FromSeconds(10))` | ❌ `Thread.Sleep()` then `FindElement()` |
| **Timeout on interaction** | Page not fully loaded | Wait for specific element that indicates ready state | ❌ Arbitrary 3-second delay |
| **Inconsistent rect/position** | Layout not settled | Multiple `GetRect()` calls with comparison | ❌ Single `GetRect()` after delay |
| **WebView failures** | External URL/network | Use mock URLs instead of external URLs | ❌ Adding longer timeouts |

### Anti-Patterns (DO NOT DO)

| Anti-Pattern | Why It's Wrong | Better Alternative |
|--------------|----------------|-------------------|
| ❌ `Task.Delay(500).Wait()` before `VerifyScreenshot()` | VerifyScreenshot already has built-in retry; use retryTimeout instead | Use `VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2))` |
| ❌ `Thread.Sleep(2000)` before element interaction | Arbitrary wait; doesn't guarantee element is ready | `App.WaitForElement("Id", timeout: ...)` |
| ❌ Adding tolerance > 5% without justification | Hides real bugs; too permissive | Use `retryTimeout` for timing issues; small tolerance (0.5%) for rendering variance |
| ❌ Using external URLs in WebView tests | External dependency; unreliable | Use mock URLs or local content |
| ❌ Fixing symptoms without understanding infrastructure | Redundant fixes; doesn't address root cause | Read `UITest.cs` first (step 1 above) |

### When to Use What

**VerifyScreenshot() parameters (preferred):**
```csharp
// Animation timing issues - keep retrying for up to 2 seconds
// This is the PREFERRED approach for flaky screenshot tests
VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));

// Small tolerance for cross-machine rendering variance + retryTimeout for timing
// Use 0.5% tolerance as safety margin, NOT to hide timing issues
VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

// Legacy: retryDelay only changes delay BETWEEN retries (default 500ms)
// retryTimeout is preferred because it keeps trying until success
VerifyScreenshot(retryDelay: TimeSpan.FromSeconds(1));
```

**Key difference: retryDelay vs retryTimeout:**
- `retryDelay`: Delay between retry attempts (default 500ms). Only retries ONCE.
- `retryTimeout`: Total time to keep retrying. Retries every `retryDelay` until timeout.
- **Prefer `retryTimeout`** for animations with variable completion times.

**WaitForElement (for element readiness):**
```csharp
// Wait up to 10 seconds for element to appear
App.WaitForElement("ButtonId", timeout: TimeSpan.FromSeconds(10));

// Then interact
App.Tap("ButtonId");
```

**Task.Delay/Thread.Sleep (avoid if possible):**
```csharp
// AVOID: With retryTimeout, you rarely need explicit delays anymore
// 
// Old pattern (before retryTimeout):
// Task.Delay(300).Wait();
// VerifyScreenshot(tolerance: 2.0);
//
// New pattern (preferred):
VerifyScreenshot(tolerance: 0.5, retryTimeout: TimeSpan.FromSeconds(2));

// ONLY use explicit delays when:
// 1. Waiting for non-element state with no screenshot (rare)
// 2. External system delay that can't be detected otherwise
// 3. After exhausting other options AND documenting why
```

### Understanding Test Infrastructure

**Key files to understand when writing UI tests:**

1. **UITest.cs** - Base class with `VerifyScreenshot()` implementation
   - Path: `src/Controls/tests/TestCases.Shared.Tests/UITest.cs`
   - Contains: retry logic, tolerance parsing, platform-specific delays

2. **_IssuesUITest.cs** - Issues test base class
   - Path: `src/Controls/tests/TestCases.Shared.Tests/_IssuesUITest.cs`
   - Contains: Navigation helpers, common patterns

3. **Extension methods** - Platform-specific helpers
   - Path: `src/Controls/tests/TestCases.Shared.Tests/` (various extension files)
   - Contains: Existing helpers for common operations

**Find existing patterns:**
```bash
# See VerifyScreenshot implementation (including retryTimeout)
grep -A 30 "public void VerifyScreenshot" src/Controls/tests/TestCases.Shared.Tests/UITest.cs

# Find existing tests using retryTimeout (preferred pattern)
grep -r "retryTimeout" src/Controls/tests/TestCases.Shared.Tests/Tests/

# Find existing tolerance patterns
grep -r "tolerance:" src/Controls/tests/TestCases.Shared.Tests/Tests/
```

### Infrastructure Notes

**Tolerance regex handles multiple locales:** The tolerance parsing uses regex pattern `\d+[.,]\d+` to match both `.` and `,` as decimal separators (e.g., "2.5%" or "2,5%"). If tolerance appears to not be applied, verify the regex patterns in `UITest.cs` `VerifyWithTolerance()` method.

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

Tests should run on all applicable platforms by default. The test infrastructure handles platform detection automatically.

### No Inline #if Directives in Test Methods

**Do NOT use `#if ANDROID`, `#if IOS`, etc. directly in test methods.** Platform-specific behavior must be hidden behind extension methods for readability.

**Note:** This rule is about **code cleanliness**, not platform scope. Using `#if ANDROID ... #else ...` still compiles for all platforms - the issue is that inline directives make test logic hard to read and maintain.

```csharp
// ❌ BAD - inline #if in test method (hard to read)
[Test]
public void MyTest()
{
#if ANDROID
    App.TapCoordinates(100, 200);
#else
    App.Tap("MyElement");
#endif
}

// ✅ GOOD - platform logic in extension method (clean test)
[Test]
public void MyTest()
{
    App.TapElementCrossPlatform("MyElement");
}
```

Move platform-specific logic to extension methods to keep test code clean and readable.

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
- [ ] Verify AutomationId references match between HostApp UI (C# or XAML) and test code
- [ ] Ensure file names follow the `IssueXXXXX` pattern and match between projects
- [ ] Ensure test methods have descriptive names
- [ ] Verify test inherits from `_IssuesUITest`
- [ ] Confirm only ONE `[Category]` attribute per test
- [ ] No inline `#if` directives in test code (use extension methods)
- [ ] Test passes locally on at least one platform

### Test State Management

- Tests should be independent and not rely on state from other tests
- The test infrastructure handles navigation to the test page and basic cleanup
- If your test modifies global app state, consider whether cleanup is needed
- Most tests don't require explicit cleanup as each test gets a fresh page instance

## Best Practices

### Default: C# Over XAML

**Use C# files (`.cs`) for UI tests. Only use XAML files (`.xaml`) when the test scenario requires XAML-specific features.**

**When to use C# only (`.cs` file):**
- ✅ Simple control tests (Button, Label, Entry, etc.)
- ✅ Layout tests (Grid, StackLayout, FlexLayout, etc.)
- ✅ Navigation tests
- ✅ Event handling tests
- ✅ Property tests
- ✅ Most UI behavior tests

**When XAML is required (`.xaml` + `.xaml.cs` files):**
- ✅ Testing XAML binding syntax
- ✅ Testing XAML templates (DataTemplate, ControlTemplate)
- ✅ Testing XAML styles and resources
- ✅ Testing XAML markup extensions
- ✅ Testing XamlC compilation behavior
- ✅ Testing XAML-specific parsing or compilation issues

**Examples:**

```csharp
// ✅ GOOD: C# only test (most common pattern)
public class Issue12345 : ContentPage
{
    public Issue12345()
    {
        Content = new StackLayout
        {
            Children =
            {
                new Label { Text = "Hello", AutomationId = "MyLabel" },
                new Button { Text = "Click Me", AutomationId = "MyButton" }
            }
        };
    }
}
```

```xaml
<!-- ❌ AVOID unless testing XAML bindings/templates/styles -->
<ContentPage ...>
    <StackLayout>
        <Label Text="Hello" AutomationId="MyLabel" />
        <Button Text="Click Me" AutomationId="MyButton" />
    </StackLayout>
</ContentPage>
```

### Use Test Helper Base Classes

**ALWAYS check for and use existing test helper base classes instead of creating from scratch:**

| Base Class | Use For | Example |
|------------|---------|---------|
| `TestShell` | Shell-related tests | `public class Issue12345 : TestShell` |
| `TestContentPage` | ContentPage tests needing `Init()` pattern | `public class Issue12345 : TestContentPage` |
| `TestNavigationPage` | NavigationPage tests | `public class Issue12345 : TestNavigationPage` |
| `ContentPage` | Simple page tests (direct inheritance) | `public class Issue12345 : ContentPage` |

**TestShell provides:**
- Platform-specific automation IDs for flyout and back buttons
- Helper methods: `AddContentPage()`, `AddBottomTab()`, `AddTopTab()`, `AddFlyoutItem()`
- Abstract `Init()` method for setup
- `DisplayedPage` property for accessing current page

**TestContentPage/TestNavigationPage provide:**
- Abstract `Init()` method for deferred initialization
- Cleaner separation of setup logic

**Example:**

```csharp
// ✅ GOOD: Using TestShell for Shell tests
[Issue(IssueTracker.Github, 12345, "Shell navigation bug", PlatformAffected.All)]
public class Issue12345 : TestShell
{
    protected override void Init()
    {
        AddContentPage(new ContentPage 
        { 
            Content = new Label { Text = "Test" } 
        });
    }
}

// ❌ BAD: Creating Shell from scratch
public class Issue12345 : Shell
{
    public Issue12345()
    {
        Items.Add(new ShellItem { ... }); // Verbose, error-prone
    }
}
```

### Avoid Obsolete APIs

**NEVER use obsolete APIs in new tests. Use modern equivalents:**

| ❌ Obsolete API | ✅ Modern API | Notes |
|----------------|--------------|-------|
| `Application.MainPage` | `Window.Page` | Access via `this.Window.Page` in ContentPage |
| `Application.MainPage` | `Application.Current.Windows[0].Page` | When not in Page context |
| `Frame` | `Border` | Frame is deprecated, use Border instead |
| `Device.BeginInvokeOnMainThread` | `Dispatcher.Dispatch` or `MainThread.BeginInvokeOnMainThread` | Modern threading APIs |

**Examples:**

```csharp
// ✅ GOOD: Modern Window API
this.Window.Page = new NavigationPage(new MyPage());

// ❌ BAD: Obsolete Application.MainPage
Application.MainPage = new NavigationPage(new MyPage());

// ✅ GOOD: Border
new Border { Content = new Label { Text = "Hello" } }

// ❌ BAD: Frame (deprecated)
new Frame { Content = new Label { Text = "Hello" } }
```

### Use UITest Optimized Controls for Screenshot Tests

**For tests that use `VerifyScreenshot()`, use UITest optimized controls instead of standard text input controls.** These controls provide `IsCursorVisible` to prevent cursor blinking from causing flaky screenshot comparisons.

| Standard Control | UITest Control | Purpose |
|------------------|----------------|---------|
| `Entry` | `UITestEntry` | Text input without cursor blink |
| `Editor` | `UITestEditor` | Multi-line input without cursor blink |
| `SearchBar` | `UITestSearchBar` | Search input without cursor blink |

**Example:**

```csharp
// For screenshot tests, use UITest controls (UITestEntry, UITestEditor, UITestSearchBar)
var entry = new UITestEntry
{
    Placeholder = "Enter text",
    IsCursorVisible = false,  // Prevents flaky screenshots
    AutomationId = "TestEntry"
};

// For non-screenshot tests, standard Entry is fine
var entry = new Entry { Placeholder = "Enter text", AutomationId = "TestEntry" };
```

**Location:** `src/Controls/tests/TestCases.HostApp/Controls/UITest*.cs`

### Check Similar Tests for Patterns

**Before creating a new test, search for similar tests to reuse patterns:**

```bash
# Find similar control tests
grep -r "class.*Issue.*Button" src/Controls/tests/TestCases.HostApp/Issues/*.cs

# Find Shell tests
grep -r "TestShell" src/Controls/tests/TestCases.HostApp/Issues/*.cs

# Find tests for specific control
grep -r "CollectionView" src/Controls/tests/TestCases.HostApp/Issues/*.cs

# Find tests using UITest optimized controls
grep -r "UITestEntry\|UITestEditor\|UITestSearchBar" src/Controls/tests/TestCases.HostApp/Issues/*.cs
```

**Reuse established patterns:**
- AutomationId naming conventions
- Test structure and layout
- Common helper methods
- Platform-specific workarounds
- UITest optimized control usage

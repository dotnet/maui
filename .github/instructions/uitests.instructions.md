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

### Common Categories
See [UITestCategories.cs](../../src/Controls/tests/TestCases.Shared.Tests/UITestCategories.cs) for the complete list.

Examples:
- `SafeAreaEdges` - Safe area and padding tests
- `Button`, `Label`, `Entry`, `Editor` - Specific control tests
- `CollectionView`, `ListView`, `CarouselView` - Collection control tests
- `Layout` - Layout-related tests
- `Shell`, `Navigation`, `TabbedPage` - Navigation tests
- `Gestures`, `Focus`, `Accessibility` - Interaction tests
- `Window`, `Page`, `LifeCycle` - Page lifecycle tests

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

### Quick Test Execution (for rapid development)

When developing and debugging a specific test:

**Android:**
1. Deploy the TestCases.HostApp:
   ```bash
   # Use local dotnet if available, otherwise use global dotnet
   ./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
   # OR:
   dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
   ```

2. Run your specific test:
   ```bash
   # Set DEVICE_UDID environment variable so Appium tests know which device to use
   # Get the device ID from: adb devices
   export DEVICE_UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)
   
   # Run the test
   dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj --filter "FullyQualifiedName~Issue12345"
   ```

**iOS (4-step process):**

**Important: Device and iOS Version Selection**

When the user requests to run tests on iOS:
- **Default behavior (no device, no iOS version specified)**: Use iPhone Xs with the highest available iOS version
- **User specifies iOS version only** (e.g., "iOS 26.0", "iOS 18.4"): 
  - First, try to find iPhone Xs with that iOS version
  - If iPhone Xs not available for that iOS version, use ANY available device with that iOS version
  - Set `IOS_VERSION` variable, leave `DEVICE_NAME` empty to allow fallback
- **User specifies device only** (e.g., "iPhone 16 Pro", "iPhone 15"): 
  - Set `DEVICE_NAME` variable with the specified device
  - Use highest available iOS version for that device
- **User specifies both device AND iOS version**: Set both `IOS_VERSION` and `DEVICE_NAME` variables

Examples of interpreting user requests:
- "Run on iOS 26.0" → Set `IOS_VERSION="26.0"`, leave `DEVICE_NAME` empty (will try iPhone Xs first, then fallback to any iOS 26.0 device)
- "Run on iPhone 16 Pro" → Set `DEVICE_NAME="iPhone 16 Pro"`, use highest iOS version
- "Run on iPhone 15 with iOS 18.0" → Set both `DEVICE_NAME="iPhone 15"` and `IOS_VERSION="18.0"`
- No specific request → Use defaults (iPhone Xs with highest iOS)

**Step 1: Find iOS Simulator**

```bash
# Set device name and iOS version based on user request
# Leave DEVICE_NAME empty when user only specifies iOS version (to allow fallback)
DEVICE_NAME="${DEVICE_NAME:-}"
IOS_VERSION="${IOS_VERSION:-}"

# Determine search strategy
if [ -z "$IOS_VERSION" ]; then
    # No iOS version specified - use iPhone Xs with highest available iOS
    DEVICE_NAME="${DEVICE_NAME:-iPhone Xs}"
    JQ_FILTER='
      .devices 
      | to_entries 
      | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) 
      | map({
          key: .key,
          version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)),
          devices: .value
        })
      | sort_by(.version)
      | reverse
      | map(select(.devices | any(.name == "'"$DEVICE_NAME"'")))
      | first
      | .devices[]
      | select(.name == "'"$DEVICE_NAME"'")
      | .udid'
else
    # Specific iOS version requested
    IOS_VERSION_FILTER="iOS-${IOS_VERSION//./-}"
    
    if [ -z "$DEVICE_NAME" ]; then
        # iOS version specified, but no device - try iPhone Xs first, then fallback to any device
        JQ_FILTER='
          .devices 
          | to_entries 
          | map(select(.key | contains("'"$IOS_VERSION_FILTER"'"))) 
          | first
          | .value
          | (map(select(.name == "iPhone Xs")) + .)[0]
          | .udid'
    else
        # Both iOS version and device specified
        JQ_FILTER='
          .devices 
          | to_entries 
          | map(select(.key | contains("'"$IOS_VERSION_FILTER"'"))) 
          | map(.value)
          | flatten
          | map(select(.name == "'"$DEVICE_NAME"'"))
          | first
          | .udid'
    fi
fi

# Extract UDID using the constructed filter
UDID=$(xcrun simctl list devices available --json | jq -r "$JQ_FILTER")

# Get the actual device name that was found
if [ ! -z "$UDID" ] && [ "$UDID" != "null" ]; then
    FOUND_DEVICE=$(xcrun simctl list devices available --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .name')
fi

# Verify UDID was found and is not empty
if [ -z "$UDID" ] || [ "$UDID" = "null" ]; then
    if [ -z "$IOS_VERSION" ]; then
        DEVICE_NAME="${DEVICE_NAME:-iPhone Xs}"
        echo "ERROR: No $DEVICE_NAME simulator found. Please create a $DEVICE_NAME simulator before running iOS tests."
    elif [ -z "$DEVICE_NAME" ]; then
        echo "ERROR: No simulator found for iOS $IOS_VERSION. Please install iOS $IOS_VERSION runtime."
    else
        echo "ERROR: No $DEVICE_NAME simulator found for iOS $IOS_VERSION. Please create one before running iOS tests."
    fi
    exit 1
fi

echo "Using $FOUND_DEVICE with UDID: $UDID"
```

**Examples of device/version selection:**
```bash
# Default: iPhone Xs with highest iOS version
# (no environment variables needed)

# Specific iOS version (will try iPhone Xs first, then any device):
export IOS_VERSION="26.0"
# Leave DEVICE_NAME unset

# Specific device with highest iOS version:
export DEVICE_NAME="iPhone 16 Pro"
# Leave IOS_VERSION unset

# Specific device AND specific iOS version:
export DEVICE_NAME="iPhone 15"
export IOS_VERSION="18.0"
```

**Step 2: Build the iOS app**
```bash
# Use local dotnet if available, otherwise use global dotnet
./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios
# OR:
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios
```

**Step 3: Boot simulator and install app**
```bash
# Boot the simulator (will error if already booted, which is fine)
xcrun simctl boot $UDID 2>/dev/null || true

# Verify simulator is booted
STATE=$(xcrun simctl list devices --json | jq -r --arg udid "$UDID" '.devices[][] | select(.udid == $udid) | .state')
if [ "$STATE" != "Booted" ]; then
    echo "ERROR: Simulator failed to boot. Current state: $STATE"
    exit 1
fi
echo "Simulator is booted and ready"

# Install the app to the simulator
xcrun simctl install $UDID artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app
```

**Step 4: Run your specific test**
```bash
# Set DEVICE_UDID environment variable so Appium tests know which device to use
export DEVICE_UDID=$UDID

# Run the test
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj --filter "FullyQualifiedName~Issue12345"
```

**MacCatalyst:**

**Step 1: Deploy TestCases.HostApp to MacCatalyst**
```bash
# Use local dotnet if available, otherwise use global dotnet
./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-maccatalyst -t:Run
# OR:
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-maccatalyst -t:Run
```

**Step 2: Run your specific test**
```bash
dotnet test src/Controls/tests/TestCases.Mac.Tests/Controls.TestCases.Mac.Tests.csproj --filter "FullyQualifiedName~Issue12345"
```

### Troubleshooting

**Android App Crashes on Launch:**

If you encounter navigation fragment errors or resource ID issues:
```
java.lang.IllegalArgumentException: No view found for id 0x7f0800f8 (com.microsoft.maui.uitests:id/inward) for fragment NavigationRootManager_ElementBasedFragment
```

**Solution:** Build with `--no-incremental` to force a clean build:
```bash
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run --no-incremental
```

**Other debugging steps:**
1. Monitor logcat: `adb logcat | grep -E "(FATAL|AndroidRuntime|Exception|Error|Crash)"`
2. Try clean build: `dotnet clean src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj`
3. Check emulator: `adb devices`

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

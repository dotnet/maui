# UI Testing for .NET MAUI

This document describes how to run UI tests for the .NET MAUI repository locally and in CI/CD environments.

## Overview

The UI tests are built using:

- **Appium WebDriver**: For automating mobile app interactions
- **NUnit**: Test framework for .NET UI tests
- **xUnit**: Test framework for unit tests
- **Android Emulator**: Primary target platform for testing
- **TestCases.HostApp**: The app under test containing issue reproductions and feature demonstrations

## Prerequisites

### Local Development

1. **Android SDK and Emulator**
   - Install Android Studio or use `dotnet android` tools
   - Create an Android emulator (API level 29+)
   - Ensure emulator is started before running tests

2. **Node.js and Appium**
   ```bash
   dotnet build ./src/Provisioning/Provisioning.csproj -t:ProvisionAppium -p:SkipAppiumDoctor="true"
   ```

3. **Java 11+ (required for Android development)**

## Running UI Tests Locally

### Step 1: Build and Deploy the TestCases.HostApp

First, build and deploy the Android APK to your emulator/device:

```bash
# From the repository root - use local dotnet if available, otherwise use global dotnet
./bin/dotnet/dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run

# If local dotnet is not available, use global dotnet
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-android -t:Run
```

This will:
- Build the TestCases.HostApp for Android
- Deploy it to the connected Android emulator/device
- Start the app automatically

### Step 2: Start Android Emulator (if not already running)

Create and start an Android emulator:

```bash
# Using dotnet android tools
dotnet android sdk install --package 'system-images;android-33;default;x86_64'
dotnet android avd create --name MAUITestEmu --sdk 'system-images;android-33;default;x86_64' --force
dotnet android avd start --name MAUITestEmu --wait-boot --gpu guest --no-snapshot --no-audio --no-window
```

Or use Android Studio to create and start an emulator.

### Step 3: Run the UI Tests

Run specific tests or test categories:

```bash
# Run a specific test by issue number
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj --filter "FullyQualifiedName~Issue11311"

# Run all tests for a specific category
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj --filter "Category=CollectionView"

# Run all tests for TabbedPage
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj --filter "Category=TabbedPage"

# Run all UI tests (warning: this takes a long time)
dotnet test src/Controls/tests/TestCases.Android.Tests/Controls.TestCases.Android.Tests.csproj
```

## Running UI Tests on iOS

### Step 1: Build and Deploy the TestCases.HostApp to iOS

Build and deploy the iOS app to the simulator (defaults to iOS 18.5 iPhone Xs):

```bash
# First, get the UDID for iPhone Xs iOS 18.5
UDID=$(xcrun simctl list devices | grep -A 20 "iOS 18.5" | grep "iPhone Xs" | sed -n 's/.*(\([^)]*\)).*/\1/p')

# Build the iOS app first
dotnet build src/Controls/tests/TestCases.HostApp/Controls.TestCases.HostApp.csproj -f net10.0-ios

# Then manually install to simulator (avoids hanging on Run target)
xcrun simctl install $UDID artifacts/bin/Controls.TestCases.HostApp/Debug/net10.0-ios/iossimulator-arm64/Controls.TestCases.HostApp.app
```

### Step 2: Run the iOS UI Tests

Run specific tests or test categories on iOS:

```bash
# Run a specific test by issue number
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj --filter "FullyQualifiedName~Issue11311"

# Run all tests for a specific category
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj --filter "Category=CollectionView"

# Run all tests for TabbedPage
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj --filter "Category=TabbedPage"

# Run all UI tests (warning: this takes a long time)
dotnet test src/Controls/tests/TestCases.iOS.Tests/Controls.TestCases.iOS.Tests.csproj
```

## Test Structure

### Test Projects Organization

- **TestCases.HostApp** (`src/Controls/tests/TestCases.HostApp/`) - The app under test
  - Contains issue reproduction pages in `Issues/` folder
  - Each issue has both `.xaml` and `.xaml.cs` files
  - Uses `AutomationId` attributes for test automation

- **TestCases.Shared.Tests** (`src/Controls/tests/TestCases.Shared.Tests/`) - Shared test logic
  - Contains the actual test implementations in `Tests/Issues/`
  - Tests inherit from `_IssuesUITest` base class
  - Uses NUnit framework with Appium WebDriver

- **Platform-Specific Test Projects**:
  - `TestCases.Android.Tests` - Android UI tests
  - `TestCases.iOS.Tests` - iOS UI tests
  - `TestCases.Mac.Tests` - macOS UI tests
  - `TestCases.WinUI.Tests` - Windows UI tests

### Base Test Classes

The `_IssuesUITest` class provides common functionality:

- Android driver initialization and configuration
- APK discovery and installation
- Cleanup and disposal
- Common test utilities and assertions

### Test Implementation Pattern

Each UI test follows this pattern:

```csharp
public class Issue11311 : _IssuesUITest
{
    public Issue11311(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "[Regression] CollectionView NSRangeException";

    [Test]
    [Category(UITestCategories.CollectionView)]
    [Category(UITestCategories.TabbedPage)]
    public void CollectionViewWithFooterShouldNotNSRangeExceptionCrashOnDisplay()
    {
        // Test implementation using Appium WebDriver
        App.FindElement("Success");
    }
}
```

## Adding New UI Tests

To add new UI tests for an issue or feature:

### 1. Create the HostApp Page

Create the reproduction page in `src/Controls/tests/TestCases.HostApp/Issues/`:

```csharp
// IssueXXXXX.cs
[Issue(IssueTracker.None, XXXXX, "Issue Description", PlatformAffected.All)]
public class IssueXXXXX : TestContentPage
{
    protected override void Init()
    {
        // Create UI with AutomationId attributes
        var button = new Button 
        { 
            Text = "Test Button",
            AutomationId = "TestButton"
        };
        
        Content = new StackLayout { Children = { button } };
    }
}
```

### 2. Create the Test Implementation

Create the test in `src/Controls/tests/TestCases.Shared.Tests/Tests/Issues/`:

```csharp
// IssueXXXXX.cs
public class IssueXXXXX : _IssuesUITest
{
    public IssueXXXXX(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "Issue Description";

    [Test]
    [Category(UITestCategories.Button)] // Use appropriate category
    public void TestMethodName()
    {
        // Use Appium WebDriver API to interact with UI
        var button = App.FindElement("TestButton");
        button.Click();
        
        // Add assertions to verify expected behavior
        Assert.That(button.Displayed, Is.True);
    }
}
```

### 3. Test Categories

Use appropriate test categories:

- `UITestCategories.CollectionView`
- `UITestCategories.ListView`
- `UITestCategories.Button`
- `UITestCategories.Layout`
- `UITestCategories.TabbedPage`
- `UITestCategories.Navigation`
- `UITestCategories.Compatibility`

## CI/CD Integration

The UI tests run automatically in GitHub Actions:

### Build Job
- Builds the Android APK on the CI environment
- Uploads APK as build artifact for testing

### UI Test Job
- Downloads the APK artifact
- Sets up Android emulator and Appium
- Installs the app and runs tests
- Uploads test results and logs

## Performance Considerations

- **UI tests are slower than unit tests** - expect several minutes for full test runs
- **Run specific tests during development** - use filters to run only relevant tests
- **Use appropriate timeouts** - balance speed and reliability with proper wait strategies
- **Emulator performance** - ensure adequate resources for stable test execution

## Troubleshooting

### Common Issues

1. **App not installed**: Ensure TestCases.HostApp is built and deployed before running tests
2. **Emulator not running**: Start Android emulator before running tests
3. **Appium connection issues**: Verify Appium server is running on correct port
4. **Element not found**: Check AutomationId matches between XAML and test code

### Debug Tips

- Use `App.Screenshot()` to capture screen state during test failures
- Add logging to test methods to trace execution
- Verify emulator is accessible via `adb devices`
- Check Appium logs for connection and driver issues

## Resources

- [Appium Documentation](http://appium.io/docs/)
- [Appium .NET Quickstart](http://appium.io/docs/en/latest/quickstart/test-dotnet/)
- [.NET MAUI Testing Documentation](https://learn.microsoft.com/en-us/dotnet/maui/deployment/testing)
- [Android Testing Best Practices](https://developer.android.com/training/testing)
- [NUnit Documentation](https://docs.nunit.org/)

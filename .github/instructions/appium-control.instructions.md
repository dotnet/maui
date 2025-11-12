---
description: "Quick reference for creating standalone Appium control scripts for manual debugging and exploration of .NET MAUI applications."
---

# Appium Control Script Instructions

Create standalone C# scripts for manual Appium-based debugging and exploration of .NET MAUI apps. Use these when you need direct control outside of automated tests.

## When to Use

- **Manual debugging** - Interactive exploration of app behavior
- **Quick experiments** - Test UI interactions without writing full tests
- **Investigation** - Reproduce issues or explore edge cases
- **Learning** - Understand how Appium interacts with MAUI apps
- **Prototyping** - Test Appium interactions before creating full UI tests

**Not for automated testing** - For automated UI tests, use the established test infrastructure in `src/Controls/tests/`.

## Quick Start with Sandbox App

The fastest way to experiment with Appium is using the Sandbox app (`src/Controls/samples/Controls.Sample.Sandbox`):

1. **Modify `MainPage.xaml`** to add controls with `AutomationId` attributes
2. **Build and deploy** with `--no-incremental` (recommended to avoid crashes):
   ```bash
   # Build
   dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios --no-incremental
   
   # Get device UDID
   UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')
   
   # Boot and install
   xcrun simctl boot $UDID 2>/dev/null || true
   xcrun simctl install $UDID artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app
   
   # Set environment variable
   export DEVICE_UDID=$UDID
   ```

3. **Start Appium** and run your control script
4. **Clean up** when done: `git checkout -- src/Controls/samples/Controls.Sample.Sandbox/`

## Prerequisites

Ensure Appium server is running on `http://localhost:4723` before running your script.

## Basic Template

Create a file named `appium-control.cs` in the repository root (this file is already gitignored):

```csharp
#:package Appium.WebDriver@8.0.1

using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Support.UI;

// CRITICAL: Get device UDID from environment variable
// Without this, Appium will randomly select a device
var udid = Environment.GetEnvironmentVariable("DEVICE_UDID");
if (string.IsNullOrEmpty(udid))
{
    Console.WriteLine("❌ ERROR: DEVICE_UDID environment variable not set!");
    Console.WriteLine("Set it with: export DEVICE_UDID=<your-device-udid>");
    Environment.Exit(1);
}

Console.WriteLine($"Target device UDID: {udid}");

// iOS Configuration
var iOSOptions = new AppiumOptions();
iOSOptions.PlatformName = "iOS";
iOSOptions.AutomationName = "XCUITest";
iOSOptions.AddAdditionalAppiumOption("appium:bundleId", "com.microsoft.maui.sandbox");
iOSOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);  // CRITICAL: Target specific device
iOSOptions.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);

// Android Configuration (alternative - comment/uncomment as needed)
/*
var androidOptions = new AppiumOptions();
androidOptions.PlatformName = "Android";
androidOptions.AutomationName = "UIAutomator2";
androidOptions.AddAdditionalAppiumOption("appium:bundleId", "com.microsoft.maui.uitests");
androidOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);  // CRITICAL: Target specific device
androidOptions.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
*/

// Connect to Appium server
var serverUri = new Uri("http://localhost:4723");

// For iOS:
using var driver = new IOSDriver(serverUri, iOSOptions);

// For Android (alternative):
// using var driver = new AndroidDriver(serverUri, androidOptions);

Console.WriteLine("Connected to app! Starting interaction...");

// Your interactive code here
// Example: Find and tap a button
var button = driver.FindElement(MobileBy.AccessibilityId("ClickButton"));
button.Click();

Console.WriteLine("Button clicked!");

// Keep session alive for manual exploration
Console.WriteLine("Press Enter to quit...");
Console.ReadLine();
```

## Key Differences Between Platforms

### iOS
- **Driver**: `IOSDriver`
- **Automation**: `AutomationName = "XCUITest"`
- **App Path**: `.app` bundle in `iossimulator-arm64/` folder
- **Device Name**: Find with `xcrun simctl list devices`

### Android
- **Driver**: `AndroidDriver`
- **Automation**: `AutomationName = "UIAutomator2"`
- **App Path**: `.apk` file with `-Signed` suffix
- **Device Name**: Get from `adb devices` command

## Running the Script

```bash
# Set DEVICE_UDID environment variable
export DEVICE_UDID=<your-device-udid>

# Start Appium server (in separate terminal or background)
appium &

# Run your script
dotnet run appium-control.cs
```

**Getting the device UDID:**
- **iOS**: See [Quick Start](#quick-start-with-sandbox-app) for UDID extraction command
- **Android**: `adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1`

For a complete end-to-end workflow with the Sandbox app, see the [Quick Start](#quick-start-with-sandbox-app) section above.

## Common Appium Operations

```csharp
// Find elements
var element = driver.FindElement(MobileBy.AccessibilityId("AutomationId"));
var byText = driver.FindElement(MobileBy.Name("Button Text"));

// Interact
element.Click();
element.SendKeys("text input");
element.Clear();

// Query
var text = element.Text;
var isDisplayed = element.Displayed;
var location = element.Location;
var size = element.Size;

// Wait for element
var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
wait.Until(d => d.FindElement(MobileBy.AccessibilityId("ElementId")));

// Take screenshot
var screenshot = driver.GetScreenshot();
screenshot.SaveAsFile("screenshot.png");

// Navigate
driver.Navigate().Back();

// Background app
driver.BackgroundApp(TimeSpan.FromSeconds(5));
```

## Troubleshooting

**"DEVICE_UDID environment variable not set"**
- Set it before running: `export DEVICE_UDID=<your-udid>`
- For iOS: Use the UDID from `xcrun simctl list devices`
- For Android: Use the device ID from `adb devices`

**"Could not connect to Appium server"**
- Verify Appium is running: `curl http://localhost:4723/status`
- Start Appium: `appium &`

**"App crashes on launch" or "Cannot launch application"**
- Rebuild with `--no-incremental` flag (incremental builds can leave app bundles inconsistent)
- See [UI Testing Guide](../../docs/UITesting-Guide.md) for detailed troubleshooting steps

**"Device not found" (iOS)**
- Verify DEVICE_UDID is set: `echo $DEVICE_UDID`
- List devices: `xcrun simctl list devices available | grep "iPhone"`
- Boot simulator: `xcrun simctl boot $DEVICE_UDID`

**"Device not found" (Android)**
- Verify DEVICE_UDID is set: `echo $DEVICE_UDID`
- List devices: `adb devices`
- Start emulator via Android Studio or: `emulator -avd [avd-name]`

## Additional Resources

- [Appium Documentation](http://appium.io/docs) - Complete API reference and guides
- [UI Testing Guide](../../docs/UITesting-Guide.md) - Full automated testing documentation
- [Selenium WebDriver Docs](https://www.selenium.dev/documentation/webdriver/) - Core WebDriver concepts

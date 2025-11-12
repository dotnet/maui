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

**IMPORTANT: Create script files inside project directory (e.g., `SandboxAppium/`), not in repository root or `/tmp`.**

```csharp
#:package Appium.WebDriver@8.0.1

using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;

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

**⚠️ Important: Create scripts inside project directory (e.g., `SandboxAppium/`), not in `/tmp` or repository root.**

```bash
# 1. Create script folder
mkdir -p SandboxAppium && cd SandboxAppium

# 2. Copy the Basic Template above into a .cs file (or use the Quick Start example)

# 3. Set DEVICE_UDID environment variable
export DEVICE_UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# 4. Start Appium (separate terminal or background)
appium &

# 5. Run your script
dotnet run yourscript.cs
```

**For Android:** Replace iOS UDID command with: `adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1`

**Complete workflow:** See [Quick Start with Sandbox App](#quick-start-with-sandbox-app) above for full end-to-end example.

## Common Appium Operations

### Finding and Interacting with Elements

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
```

### Device Control

**⚠️ Important: Use Appium APIs for device operations, not platform tools.**

```csharp
// Rotate device and validate
driver.Orientation = ScreenOrientation.Landscape;
Thread.Sleep(2000); // Wait for animation

if (driver.Orientation != ScreenOrientation.Landscape)
    Console.WriteLine("❌ Rotation failed!");
```

**Why:** `driver.Orientation` actually rotates the device. Platform tools like `xcrun simctl status_bar --orientation` only change the status bar appearance, not the device state.

### Screenshots and Other Operations

```csharp
// Take screenshot
var screenshot = driver.GetScreenshot();
screenshot.SaveAsFile("/tmp/screenshot.png");

// Navigate
driver.Navigate().Back();

// Background app
driver.BackgroundApp(TimeSpan.FromSeconds(5));
```

## Advanced Patterns

For Shell-specific testing patterns (e.g., opening flyouts), see [UI Tests Instructions](uitests.instructions.md).

## Troubleshooting

**"#: directives can be only used in file-based programs"**
- ❌ **Problem**: Script file is in `/tmp` or outside project directory
- ✅ **Solution**: Move script into project folder (e.g., `SandboxAppium/`)
- **Why**: The `#:package` directive only works inside a project context in .NET 10

**"DEVICE_UDID environment variable not set"**
- Set it before running: `export DEVICE_UDID=<your-udid>`
- For iOS: Use the UDID from `xcrun simctl list devices`
- For Android: Use the device ID from `adb devices`

**"Could not connect to Appium server"**
- Verify Appium is running: `curl http://localhost:4723/status`
- Start Appium: `appium &` (or `appium --log-level error &` for less noise)

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

**"Appium server keeps stopping"**
- Check if port 4723 is already in use: `lsof -i :4723`
- Kill existing process: `kill -9 <PID>`
- Restart Appium: `appium &`

**"Rotation doesn't work" or "Device stays in same orientation"**
- ✅ **Use Appium API**: `driver.Orientation = ScreenOrientation.Landscape`
- ❌ **Don't use**: `xcrun simctl status_bar --orientation` (only changes status bar, not device)
- ✅ **Always validate**: Check `driver.Orientation` after rotation
- ✅ **Wait for animation**: `Thread.Sleep(2000)` after rotation command

## Additional Resources

- [Appium Documentation](http://appium.io/docs) - Complete API reference and guides
- [UI Testing Guide](../../docs/UITesting-Guide.md) - Full automated testing documentation
- [Selenium WebDriver Docs](https://www.selenium.dev/documentation/webdriver/) - Core WebDriver concepts

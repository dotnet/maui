---
description: "Quick reference for creating standalone Appium control scripts for manual debugging and exploration of .NET MAUI applications."
---

# Appium Control Script Instructions

Create standalone C# scripts for manual Appium-based debugging and exploration of .NET MAUI apps. Use these when you need direct control outside of automated tests.

**🚨 CRITICAL: Use .NET 10 Native Scripting (NOT dotnet-script)**

This repository uses **.NET 10's built-in scripting features** with the `#:package` directive. Do NOT use `dotnet-script` or the old `#r` directive syntax.

**🚨 CRITICAL: Appium is the ONLY way to interact with device UI**

- ✅ **ALWAYS use Appium** for tapping, swiping, finding elements, rotating device, etc.
- ❌ **NEVER use** `adb shell input tap`, `adb shell input swipe`, or coordinate-based commands
- **Why**: Appium provides reliable element location, proper waits, and cross-device compatibility

**Common Command Patterns**: For UDID extraction, device boot, and build patterns, see [Common Testing Patterns](common-testing-patterns.md).

## When to Use

- **Manual debugging** - Interactive exploration of app behavior
- **Quick experiments** - Test UI interactions without writing full tests
- **Investigation** - Reproduce issues or explore edge cases
- **Learning** - Understand how Appium interacts with MAUI apps
- **Prototyping** - Test Appium interactions before creating full UI tests
- **PR validation** - Testing PRs that affect UI behavior

**Not for automated testing** - For automated UI tests, use the established test infrastructure in `src/Controls/tests/`.

## Quick Start with Sandbox App

The fastest way to experiment with Appium is using the Sandbox app (`src/Controls/samples/Controls.Sample.Sandbox`):

1. **Modify `MainPage.xaml`** to add controls with `AutomationId` attributes

2. **Build and deploy**:

   **iOS:**
   ```bash
   # Build
   dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-ios

   # Get device UDID
   UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

   # Verify UDID was found
   if [ -z "$UDID" ]; then
       echo "❌ ERROR: No iPhone Xs simulator found"
       exit 1
   fi
   echo "Using iPhone Xs with UDID: $UDID"

   # Boot and install
   xcrun simctl boot $UDID 2>/dev/null || true
   xcrun simctl install $UDID artifacts/bin/Maui.Controls.Sample.Sandbox/Debug/net10.0-ios/iossimulator-arm64/Maui.Controls.Sample.Sandbox.app

   # Set environment variable
   export DEVICE_UDID=$UDID
   ```

   **Android:**
   ```bash
   # Get device UDID
   UDID=$(adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1)

   # Verify UDID was found
   if [ -z "$UDID" ]; then
       echo "❌ ERROR: No Android device/emulator found"
       exit 1
   fi
   echo "Using Android device: $UDID"

   # Set environment variable
   export DEVICE_UDID=$UDID

   # Build and deploy (this handles install + launch)
   dotnet build src/Controls/samples/Controls.Sample.Sandbox/Maui.Controls.Sample.Sandbox.csproj -f net10.0-android -t:Run

   # Verify app is running
   sleep 3
   if adb -s $UDID shell pidof com.microsoft.maui.sandbox > /dev/null; then
       echo "✅ App is running"
   else
       echo "❌ App failed to start"
       exit 1
   fi
   ```

3. **Start Appium and run your control script**:
   ```bash
   # Start Appium in background
   appium --log-level error &

   # Wait for Appium to be ready
   sleep 3

   # Or verify it's ready (optional):
   # curl http://localhost:4723/status

   # Run your script (from SandboxAppium/ directory)
   cd SandboxAppium
   
   # Run with .NET 10's native scripting (NOT dotnet-script)
   dotnet run yourscript.cs
   ```

**Note**: The Sandbox app and SandboxAppium folder are set up for iterative development. You can modify your script and re-run it multiple times without cleaning up. Only clean up when you're completely done with your debugging session.

## Cleanup (Optional)

When you're finished with your debugging session and ready to clean up:

```bash
# Revert Sandbox changes
git checkout -- src/Controls/samples/Controls.Sample.Sandbox/

# Remove SandboxAppium folder (it's gitignored)
rm -rf SandboxAppium

# Kill Appium server
lsof -i :4723 | grep LISTEN | awk '{print $2}' | xargs kill -9
```

## Prerequisites

Ensure Appium server is running on `http://localhost:4723` before running your script.

## Basic Template

**🚨 CRITICAL: .NET 10 Scripting Requirements**

1. **Use `#:package` directive** (NOT `#r` or dotnet-script syntax)
2. **Create scripts inside project directory** (e.g., `SandboxAppium/`), not in `/tmp` or repository root
3. **Run with `dotnet run`** (NOT `dotnet script` or `dotnet-script`)
4. **Requires .NET 10 SDK** - These scripts use .NET 10's native scripting features

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
androidOptions.AddAdditionalAppiumOption("appium:appPackage", "com.microsoft.maui.sandbox");
androidOptions.AddAdditionalAppiumOption("appium:appActivity", "com.microsoft.maui.sandbox.MainActivity");
androidOptions.AddAdditionalAppiumOption("appium:noReset", true);
androidOptions.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);  // CRITICAL: Target specific device
androidOptions.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
*/

// Connect to Appium server
var serverUri = new Uri("http://localhost:4723");

Console.WriteLine("Connecting to Appium server...");

try
{
    // For iOS:
    using var driver = new IOSDriver(serverUri, iOSOptions);

    // For Android (alternative):
    // using var driver = new AndroidDriver(serverUri, androidOptions);

    Console.WriteLine("✅ Connected to app! Starting interaction...");

    // CRITICAL PLATFORM DIFFERENCE: Element locators differ between iOS and Android
    // iOS: AutomationId maps to accessibility identifier → use MobileBy.AccessibilityId()
    // Android: AutomationId maps to resource-id → use MobileBy.Id()

    // Wait for app to fully load
    Thread.Sleep(3000);

    // Your interactive code here
    // Example for iOS: Find and tap a button
    var button = driver.FindElement(MobileBy.AccessibilityId("ClickButton"));
    // Example for Android: Find and tap a button
    // var button = driver.FindElement(MobileBy.Id("ClickButton"));

    button.Click();
    Console.WriteLine("Button clicked!");

    // Keep session alive for manual exploration
    Console.WriteLine("Press Enter to quit...");
    Console.ReadLine();
}
catch (Exception ex)
{
    Console.WriteLine($"❌ ERROR: Failed to connect to Appium or launch app");
    Console.WriteLine($"Error: {ex.Message}");
    Console.WriteLine("\nTroubleshooting steps:");
    Console.WriteLine("1. Verify Appium is running: curl http://localhost:4723/status");
    Console.WriteLine("2. Check device UDID is correct: echo $DEVICE_UDID");
    Console.WriteLine("3. Verify app is installed on device");
    Console.WriteLine("4. Check Appium logs for detailed error information");
    Environment.Exit(1);
}
```

## Key Differences Between Platforms

### iOS
- **Driver**: `IOSDriver`
- **Automation**: `AutomationName = "XCUITest"`
- **App Path**: `.app` bundle in `iossimulator-arm64/` folder
- **Device Name**: Find with `xcrun simctl list devices`
- **Element Locator**: `MobileBy.AccessibilityId("AutomationId")` - AutomationIds map to accessibility identifiers
- **App Options**: Use `appium:bundleId` to specify the app

### Android
- **Driver**: `AndroidDriver`
- **Automation**: `AutomationName = "UIAutomator2"`
- **App Path**: `.apk` file with `-Signed` suffix
- **Device Name**: Get from `adb devices` command
- **Element Locator**: `MobileBy.Id("AutomationId")` - AutomationIds map to `resource-id` attributes
- **App Options**: Use `appium:appPackage` and `appium:appActivity` (format: `{packageId}.MainActivity`)
- **Deployment**: Use `dotnet build -t:Run` to build, install, and launch the app in one command

## Running the Script

**🚨 CRITICAL: .NET 10 Native Scripting (NOT dotnet-script)**

- ✅ **DO**: Use `dotnet run yourscript.cs` (.NET 10 native scripting)
- ✅ **DO**: Use `#:package Appium.WebDriver@8.0.1` directive
- ❌ **DON'T**: Use `dotnet script` or `dotnet-script` commands
- ❌ **DON'T**: Use `#r` directive syntax (that's for dotnet-script, not .NET 10)
- ❌ **DON'T**: Create scripts in `/tmp` or repository root

**⚠️ Important: Create scripts inside project directory (e.g., `SandboxAppium/`), not in `/tmp` or repository root.**

```bash
# 1. Create script folder
mkdir -p SandboxAppium && cd SandboxAppium

# 2. Copy the Basic Template above into a .cs file (or use the Quick Start example)

# 3. Set DEVICE_UDID environment variable
export DEVICE_UDID=$(xcrun simctl list devices available --json | jq -r '.devices | to_entries | map(select(.key | startswith("com.apple.CoreSimulator.SimRuntime.iOS"))) | map({key: .key, version: (.key | sub("com.apple.CoreSimulator.SimRuntime.iOS-"; "") | split("-") | map(tonumber)), devices: .value}) | sort_by(.version) | reverse | map(select(.devices | any(.name == "iPhone Xs"))) | first | .devices[] | select(.name == "iPhone Xs") | .udid')

# 4. Start Appium (separate terminal or background)
appium --log-level error &

# Wait for Appium to be ready
sleep 3

# 5. Run your script
dotnet run yourscript.cs
```

**For Android:** Replace iOS UDID command with: `adb devices | grep -v "List" | grep "device" | awk '{print $1}' | head -1`

**Complete workflow:** See [Quick Start with Sandbox App](#quick-start-with-sandbox-app) above for full end-to-end example.

## ❌ Wrong vs ✅ Right Approach

### Opening a Flyout Menu

**❌ WRONG - Using ADB commands (brittle, unreliable)**:
```bash
# DON'T DO THIS - coordinates are device-specific and unreliable
adb shell input tap 100 100  # Guess where hamburger menu is
sleep 2  # Hope it opened
```

**✅ RIGHT - Using Appium (reliable, verifiable)**:
```csharp
// Find the hamburger menu by its accessibility properties
var flyoutButton = driver.FindElement(
    MobileBy.XPath("//android.widget.ImageButton[@content-desc='Open navigation drawer']")
);
flyoutButton.Click();

// Verify it actually opened
var flyoutItems = driver.FindElements(MobileBy.XPath("//android.widget.TextView[contains(@text, 'Item')]"));
Console.WriteLine($"Flyout opened with {flyoutItems.Count} items");
```

### Rotating Device

**❌ WRONG - Using simctl/ADB**:
```bash
# DON'T DO THIS - doesn't guarantee app orientation changed
adb shell content insert --uri content://settings/system --bind name:s:user_rotation --bind value:i:1
```

**✅ RIGHT - Using Appium**:
```csharp
// Appium handles rotation properly and waits for completion
driver.Orientation = ScreenOrientation.Landscape;

// Verify rotation succeeded
if (driver.Orientation != ScreenOrientation.Landscape)
{
    Console.WriteLine("❌ Rotation failed!");
}
```

### Taking Screenshots

**✅ BOTH are acceptable** (Appium preferred for consistency):
```csharp
// Appium (preferred - works cross-platform)
var screenshot = driver.GetScreenshot();
screenshot.SaveAsFile("/tmp/screenshot.png");

// ADB (acceptable for Android-only scenarios)
// adb exec-out screencap -p > screenshot.png
```

## Common Appium Operations

### Finding and Interacting with Elements

**CRITICAL**: Element locators differ by platform!

```csharp
// Find elements by AutomationId
// iOS: Use MobileBy.AccessibilityId()
var element = driver.FindElement(MobileBy.AccessibilityId("AutomationId"));

// Android: Use MobileBy.Id()
// var element = driver.FindElement(MobileBy.Id("AutomationId"));

// Find by visible text (works on both platforms)
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
- **Read the crash logs** to find the exception (iOS: `xcrun simctl spawn booted log stream`, Android: `adb logcat`)
- **Investigate the root cause** from the exception stack trace
- **If you can't fix it**, ask for guidance with the full exception details
- See [Common Testing Patterns: App Crashes](../common-testing-patterns.md#error-app-crashes-on-launch) for detailed log capture commands

**"Device not found" (iOS)**
- Verify DEVICE_UDID is set: `echo $DEVICE_UDID`
- List devices: `xcrun simctl list devices available | grep "iPhone"`
- Boot simulator: `xcrun simctl boot $DEVICE_UDID`

**"Device not found" (Android)**
- Verify DEVICE_UDID is set: `echo $DEVICE_UDID`
- List devices: `adb devices`
- Start emulator: See [Common Testing Patterns: Android Emulator Startup](common-testing-patterns.md#android-emulator-startup-with-error-checking) for the correct background daemon pattern

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

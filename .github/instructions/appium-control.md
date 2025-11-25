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

**Use BuildAndRunSandbox.ps1 Script**

The script handles all building, deployment, Appium management, and log capture:

```powershell
# 1. Copy Appium template
cp .github/scripts/templates/RunWithAppiumTest.template.cs CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs

# 2. Edit the template for your test scenario

# 3. Run everything with one command
pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform [android|ios]
```

The script automatically:
- Detects and boots devices
- Builds and deploys the Sandbox app
- Starts/stops Appium server
- Runs your Appium test script
- Captures all logs to `CustomAgentLogsTmp/Sandbox/` directory

See [Common Testing Patterns](common-testing-patterns.md) for details.



## How It Works

When you run `BuildAndRunSandbox.ps1`:

1. **Creates test script from template**: The script copies `.github/scripts/RunWithAppiumTest.template.cs` to `CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs`
2. **You customize the test**: Modify `RunWithAppiumTest.cs` to interact with your Sandbox app UI
3. **Script handles everything**: Builds app, manages Appium, runs test, captures logs
4. **Review results**: All output in `CustomAgentLogsTmp/Sandbox/` directory

## Customizing the Appium Test

The template file (`.github/scripts/RunWithAppiumTest.template.cs`) provides a complete working example with:

**✅ Platform detection and configuration** (iOS/Android)
**✅ Device UDID handling** (automatically set by PS1 script)
**✅ PID capture for log filtering** (Android only)
**✅ Basic app connection and interaction patterns**

**Your job**: Modify the `// YOUR TEST LOGIC HERE` section to:
- Find elements using `AutomationId` values from your Sandbox XAML
- Interact with the UI (tap buttons, enter text, etc.)
- Verify expected behavior
- Take screenshots if needed

### Key Template Features

The template includes everything needed:

**Platform-specific configuration**:
```csharp
// Detects platform from DEVICE_UDID and configures Appium accordingly
if (udid.Contains("-")) // iOS UDID format
{
    options.PlatformName = "iOS";
    options.AutomationName = "XCUITest";
    options.AddAdditionalAppiumOption("appium:bundleId", "com.microsoft.maui.sandbox");
}
else // Android UDID format
{
    options.PlatformName = "Android";
    options.AutomationName = "UIAutomator2";
    options.AddAdditionalAppiumOption("appium:appPackage", "com.microsoft.maui.sandbox");
    options.AddAdditionalAppiumOption("appium:appActivity", "com.microsoft.maui.sandbox.MainActivity");
}
```

**PID capture for Android log filtering**:
```csharp
// Android: Capture PID so PS1 script can filter device logs
if (isAndroid)
{
    var pidCommand = $"adb -s {udid} shell pidof com.microsoft.maui.sandbox";
    var pidProcess = Process.Start(new ProcessStartInfo
    {
        FileName = "bash",
        Arguments = $"-c \"{pidCommand}\"",
        RedirectStandardOutput = true,
        UseShellExecute = false
    });
    
    if (pidProcess != null)
    {
        var pid = pidProcess.StandardOutput.ReadToEnd().Trim();
        Console.WriteLine($"[APPIUM_PID]{pid}[/APPIUM_PID]");
    }
}
```

**YOUR CUSTOMIZATION POINT**:
```csharp
// YOUR TEST LOGIC HERE
// Example: Find and tap a button
if (isIOS)
{
    var button = driver.FindElement(MobileBy.AccessibilityId("MyButton"));
    button.Click();
}
else // Android
{
    var button = driver.FindElement(MobileBy.Id("MyButton"));
    button.Click();
}

Console.WriteLine("✅ Test completed successfully!");
```

**Platform-specific element locators**:
- **iOS**: `MobileBy.AccessibilityId("AutomationId")` - Maps to accessibility identifier
- **Android**: `MobileBy.Id("AutomationId")` - Maps to resource-id

### Example Customization

Here's how to modify the template for a simple button tap test:

```csharp
// In RunWithAppiumTest.cs, replace the "YOUR TEST LOGIC HERE" section:

Thread.Sleep(2000); // Wait for app to load

if (isIOS)
{
    var button = driver.FindElement(MobileBy.AccessibilityId("TestButton"));
    Console.WriteLine("Found button, tapping...");
    button.Click();
    
    var label = driver.FindElement(MobileBy.AccessibilityId("ResultLabel"));
    Console.WriteLine($"Result: {label.Text}");
}
else // Android
{
    var button = driver.FindElement(MobileBy.Id("TestButton"));
    Console.WriteLine("Found button, tapping...");
    button.Click();
    
    var label = driver.FindElement(MobileBy.Id("ResultLabel"));
    Console.WriteLine($"Result: {label.Text}");
}

Console.WriteLine("✅ Test completed successfully!");
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

## Running the Script

**The BuildAndRunSandbox.ps1 script handles all setup and execution.** See [Quick Start with Sandbox App](#quick-start-with-sandbox-app) for the complete workflow.

**ALWAYS use the BuildAndRunSandbox.ps1 script** - it manages all building, deployment, Appium server, and log capture automatically.

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

**"DEVICE_UDID environment variable not set"**
- Set it before running: `export DEVICE_UDID=<your-udid>`
- For iOS: Use the UDID from `xcrun simctl list devices`
- For Android: Use the device ID from `adb devices`

**"Could not connect to Appium server"**
- Verify Appium is running: `curl http://localhost:4723/status`
- Start Appium: `appium &` (or `appium --log-level error &` for less noise)

**"Device not found" (Android)**
- Verify DEVICE_UDID is set: `echo $DEVICE_UDID`
- List devices: `adb devices`
- Start emulator: See [Common Testing Patterns: Android Emulator Startup](common-testing-patterns.md#android-emulator-startup-with-error-checking) for the correct background daemon pattern

**"Rotation doesn't work" or "Device stays in same orientation"**
- ✅ **Use Appium API**: `driver.Orientation = ScreenOrientation.Landscape`
- ❌ **Don't use**: `xcrun simctl status_bar --orientation` (only changes status bar, not device)
- ✅ **Always validate**: Check `driver.Orientation` after rotation
- ✅ **Wait for animation**: `Thread.Sleep(2000)` after rotation command

## Additional Resources

- [Appium Documentation](http://appium.io/docs) - Complete API reference and guides
- [UI Testing Guide](../../docs/UITesting-Guide.md) - Full automated testing documentation
- [Selenium WebDriver Docs](https://www.selenium.dev/documentation/webdriver/) - Core WebDriver concepts

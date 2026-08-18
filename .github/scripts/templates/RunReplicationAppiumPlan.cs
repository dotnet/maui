#!/usr/bin/env dotnet run
#:package Appium.WebDriver@8.0.1
#:property WindowsAppSdkBootstrapInitialize=false
#:property WindowsAppSdkDeploymentManagerInitialize=false

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

const string planFileName = "appium-plan.json";
var platform = RequireEnvironmentValue("REPLICATION_PLATFORM").ToLowerInvariant();
var udid = RequireEnvironmentValue("DEVICE_UDID");
var planPath = Path.GetFullPath(planFileName);
if (!File.Exists(planPath))
{
    throw new InvalidOperationException($"Trusted Appium plan is missing: {planPath}");
}

var planJson = File.ReadAllText(planPath);
var plan = ReadPlan(planJson);
ValidatePlan(plan, platform);

Console.WriteLine($"Running issue {plan.IssueNumber} Appium plan on {platform}.");
Process? launchedWindowsApp = null;
try
{
    using var driver = CreateDriver(platform, udid, out launchedWindowsApp);
    var catalystFramesDirectory = platform == "catalyst"
        ? RequireEnvironmentValue("MAUI_REPLICATION_CATALYST_FRAMES_DIRECTORY")
        : null;
    var catalystFrameIndex = 0;
    CaptureCatalystFrame(driver, catalystFramesDirectory, ref catalystFrameIndex);
    WriteRecordingStartMarker();

    for (var index = 0; index < plan.Steps.Count; index++)
    {
        var step = plan.Steps[index];
        Console.WriteLine($"STEP {index + 1}/{plan.Steps.Count}: {step.Description}");
        try
        {
            ExecuteStep(
                driver,
                platform,
                launchedWindowsApp,
                step,
                isFinalStep: index == plan.Steps.Count - 1);
        }
        catch (Exception stepException)
            when (DescribeUnexpectedAppTermination(launchedWindowsApp, step, stepException)
                is { } termination)
        {
            throw new InvalidOperationException(
                $"REPLICATION_APP_TERMINATED step={index + 1} action='{step.Action}' {termination}",
                stepException);
        }
        CaptureCatalystFrame(driver, catalystFramesDirectory, ref catalystFrameIndex);
    }

    Console.WriteLine($"REPLICATION_ACTIONS_COMPLETED issue={plan.IssueNumber}");
}
finally
{
    if (launchedWindowsApp is { HasExited: false })
    {
        launchedWindowsApp.Kill(entireProcessTree: true);
        launchedWindowsApp.WaitForExit(10_000);
    }
    launchedWindowsApp?.Dispose();
}

return;

static string RequireEnvironmentValue(string name)
{
    var value = Environment.GetEnvironmentVariable(name);
    if (string.IsNullOrWhiteSpace(value))
    {
        throw new InvalidOperationException($"Required environment value '{name}' is missing.");
    }
    return value;
}

static void WriteRecordingStartMarker()
{
    var markerPath = Environment.GetEnvironmentVariable(
        "MAUI_REPLICATION_RECORDING_START_MARKER");
    if (string.IsNullOrWhiteSpace(markerPath))
    {
        return;
    }
    if (!Path.IsPathFullyQualified(markerPath))
    {
        throw new InvalidOperationException(
            "Recording start marker path must be fully qualified.");
    }
    var parent = Path.GetDirectoryName(markerPath);
    if (string.IsNullOrWhiteSpace(parent) || !Directory.Exists(parent))
    {
        throw new InvalidOperationException(
            "Recording start marker directory is unavailable.");
    }

    using var stream = new FileStream(
        markerPath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.None);
    using var writer = new StreamWriter(stream);
    writer.Write(
        DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            .ToString(System.Globalization.CultureInfo.InvariantCulture));
}

static void CaptureCatalystFrame(
    AppiumDriver driver,
    string? framesDirectory,
    ref int frameIndex)
{
    if (framesDirectory is null)
    {
        return;
    }
    if (!Path.IsPathFullyQualified(framesDirectory) || !Directory.Exists(framesDirectory))
    {
        throw new InvalidOperationException(
            "Trusted Catalyst frame directory is missing or not fully qualified.");
    }
    if (frameIndex >= 128)
    {
        throw new InvalidOperationException("Catalyst Appium frame limit was exceeded.");
    }

    var framePath = Path.Combine(framesDirectory, $"frame-{frameIndex:D4}.png");
    var screenshot = ((ITakesScreenshot)driver).GetScreenshot();
    using var stream = new FileStream(
        framePath,
        FileMode.CreateNew,
        FileAccess.Write,
        FileShare.None);
    stream.Write(screenshot.AsByteArray);
    Console.WriteLine($"Captured Catalyst evidence frame {frameIndex}.");
    frameIndex++;
}

static ReplicationPlan ReadPlan(string json)
{
    using var document = JsonDocument.Parse(
        json,
        new JsonDocumentOptions
        {
            AllowTrailingCommas = false,
            CommentHandling = JsonCommentHandling.Disallow,
            MaxDepth = 10
        });
    var root = document.RootElement;
    RequireProperties(root, "schemaVersion", "issueNumber", "steps");

    var plan = new ReplicationPlan
    {
        SchemaVersion = root.GetProperty("schemaVersion").GetInt32(),
        IssueNumber = root.GetProperty("issueNumber").GetInt32()
    };
    foreach (var stepElement in root.GetProperty("steps").EnumerateArray())
    {
        RequireProperties(
            stepElement,
            "action",
            "description",
            "locator",
            "value",
            "timeoutSeconds");
        ReplicationLocator? locator = null;
        var locatorElement = stepElement.GetProperty("locator");
        if (locatorElement.ValueKind != JsonValueKind.Null)
        {
            RequireProperties(locatorElement, "strategy", "value");
            locator = new ReplicationLocator
            {
                Strategy = locatorElement.GetProperty("strategy").GetString() ?? string.Empty,
                Value = locatorElement.GetProperty("value").GetString() ?? string.Empty
            };
        }

        var valueElement = stepElement.GetProperty("value");
        plan.Steps.Add(new ReplicationStep
        {
            Action = stepElement.GetProperty("action").GetString() ?? string.Empty,
            Description = stepElement.GetProperty("description").GetString() ?? string.Empty,
            Locator = locator,
            Value = valueElement.ValueKind == JsonValueKind.Null
                ? null
                : valueElement.GetString(),
            TimeoutSeconds = stepElement.GetProperty("timeoutSeconds").GetInt32()
        });
    }
    return plan;
}

static void RequireProperties(JsonElement element, params string[] expected)
{
    if (element.ValueKind != JsonValueKind.Object)
    {
        throw new InvalidOperationException("Trusted Appium plan contains a non-object value.");
    }
    var actual = new HashSet<string>(StringComparer.Ordinal);
    foreach (var property in element.EnumerateObject())
    {
        if (!actual.Add(property.Name))
        {
            throw new InvalidOperationException(
                $"Trusted Appium plan contains duplicate property '{property.Name}'.");
        }
    }
    if (!actual.SetEquals(expected))
    {
        throw new InvalidOperationException(
            "Trusted Appium plan does not match the exact property contract.");
    }
}

static AppiumDriver CreateDriver(string platform, string udid, out Process? launchedWindowsApp)
{
    launchedWindowsApp = null;
    var server = new Uri("http://127.0.0.1:4723");
    var options = new AppiumOptions();
    options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);

    switch (platform)
    {
        case "android":
            options.PlatformName = "Android";
            options.AutomationName = "UIAutomator2";
            options.AddAdditionalAppiumOption("appium:appPackage", "com.microsoft.maui.sandbox");
            options.AddAdditionalAppiumOption(
                "appium:appActivity",
                "com.microsoft.maui.sandbox.MainActivity");
            options.AddAdditionalAppiumOption("appium:noReset", true);
            options.AddAdditionalAppiumOption("appium:dontStopAppOnReset", true);
            options.AddAdditionalAppiumOption(
                "appium:uiautomator2ServerInstallTimeout",
                300_000);
            options.AddAdditionalAppiumOption(
                "appium:uiautomator2ServerLaunchTimeout",
                120_000);
            options.AddAdditionalAppiumOption("appium:adbExecTimeout", 180_000);
            options.AddAdditionalAppiumOption("appium:androidInstallTimeout", 300_000);
            options.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);
            return new AndroidDriver(server, options);
        case "ios":
            options.PlatformName = "iOS";
            options.AutomationName = "XCUITest";
            options.AddAdditionalAppiumOption("appium:bundleId", "com.microsoft.maui.sandbox");
            options.AddAdditionalAppiumOption("appium:noReset", true);
            options.AddAdditionalAppiumOption("appium:forceAppLaunch", false);
            options.AddAdditionalAppiumOption("appium:shouldTerminateApp", false);
            options.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);
            return new IOSDriver(server, options);
        case "catalyst":
            options.PlatformName = "Mac";
            options.AutomationName = "Mac2";
            options.AddAdditionalAppiumOption("appium:bundleId", "com.microsoft.maui.sandbox");
            options.AddAdditionalAppiumOption("appium:skipAppKill", true);
            return new MacDriver(server, options);
        case "windows":
            var appPath = RequireEnvironmentValue("REPLICATION_WINDOWS_APP_PATH");
            launchedWindowsApp = Array.Find(
                Process.GetProcessesByName(Path.GetFileNameWithoutExtension(appPath)),
                process => !process.HasExited);
            launchedWindowsApp ??= Process.Start(new ProcessStartInfo(appPath)
                {
                    UseShellExecute = false,
                    WorkingDirectory = Path.GetDirectoryName(appPath)
                        ?? throw new InvalidOperationException("Windows app directory is unavailable.")
                }) ?? throw new InvalidOperationException("Windows Sandbox process did not start.");
            var launchedWindowsProcessId = launchedWindowsApp.Id;
            var windowDeadline = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < windowDeadline)
            {
                launchedWindowsApp.Refresh();
                if (launchedWindowsApp.HasExited)
                {
                    throw new InvalidOperationException(
                        $"Windows Sandbox process {launchedWindowsProcessId} exited before creating a window.");
                }
                if (launchedWindowsApp.MainWindowHandle != IntPtr.Zero)
                {
                    break;
                }
                System.Threading.Thread.Sleep(250);
            }
            if (launchedWindowsApp.MainWindowHandle == IntPtr.Zero)
            {
                throw new TimeoutException("Windows Sandbox did not create a top-level window within 30 seconds.");
            }
            options.PlatformName = "Windows";
            options.AutomationName = "Windows";
            options.DeviceName = "WindowsPC";
            options.AddAdditionalAppiumOption(
                "appTopLevelWindow",
                $"0x{launchedWindowsApp.MainWindowHandle.ToInt64():X}");
            return new WindowsDriver(server, options);
        default:
            throw new InvalidOperationException($"Unsupported replication platform '{platform}'.");
    }
}

static void ExecuteStep(
    AppiumDriver driver,
    string platform,
    Process? launchedWindowsApp,
    ReplicationStep step,
    bool isFinalStep)
{
    var timeout = TimeSpan.FromSeconds(step.TimeoutSeconds);
    switch (step.Action)
    {
        case "waitFor":
            _ = WaitForElement(driver, platform, step.Locator!, timeout);
            break;
        case "tap":
            WaitForElement(driver, platform, step.Locator!, timeout).Click();
            break;
        case "clear":
            WaitForElement(driver, platform, step.Locator!, timeout).Clear();
            break;
        case "enterText":
            WaitForElement(driver, platform, step.Locator!, timeout).SendKeys(step.Value!);
            break;
        case "assertExists":
            _ = WaitForElement(driver, platform, step.Locator!, timeout);
            break;
        case "assertNotExists":
            WaitForAbsence(driver, platform, step.Locator!, timeout);
            break;
        case "assertTextEquals":
            AssertElementText(
                driver,
                platform,
                step,
                timeout,
                contains: false,
                isFinalStep);
            break;
        case "assertTextContains":
            AssertElementText(
                driver,
                platform,
                step,
                timeout,
                contains: true,
                isFinalStep);
            break;
        case "assertAppClosed":
            AssertAppClosed(platform, launchedWindowsApp, timeout);
            break;
        case "back":
            driver.Navigate().Back();
            break;
        case "restartApp":
            var appId = platform switch
            {
                "android" => "com.microsoft.maui.sandbox",
                "ios" => "com.microsoft.maui.sandbox",
                _ => throw new InvalidOperationException(
                    $"restartApp is unsupported on replication platform '{platform}'.")
            };
            driver.TerminateApp(appId);
            driver.ActivateApp(appId);
            break;
        case "swipe":
            Swipe(driver, platform, step.Value!);
            break;
        case "dragPath":
            DragPath(
                driver,
                platform,
                WaitForElement(driver, platform, step.Locator!, timeout),
                step.Value!);
            break;
        case "setOrientation":
            driver.Orientation = step.Value switch
            {
                "portrait" => ScreenOrientation.Portrait,
                "landscape" => ScreenOrientation.Landscape,
                _ => throw new InvalidOperationException(
                    $"Unsupported orientation '{step.Value}'.")
            };
            break;
        default:
            throw new InvalidOperationException($"Unsupported Appium action '{step.Action}'.");
    }
}

static string? DescribeUnexpectedAppTermination(
    Process? launchedWindowsApp,
    ReplicationStep step,
    Exception exception)
{
    // A step that deliberately waits for the app to close is not a crash.
    if (string.Equals(step.Action, "assertAppClosed", StringComparison.OrdinalIgnoreCase))
    {
        return null;
    }

    if (launchedWindowsApp is not null)
    {
        var processId = launchedWindowsApp.Id;
        launchedWindowsApp.Refresh();
        if (launchedWindowsApp.HasExited)
        {
            // ExitCode throws for a process this runner attached to instead of
            // starting, so the identity is reported without it.
            return $"The app under test (process {processId}) exited " +
                "before this step completed, so every " +
                "later element lookup failed against a closed window. If the issue " +
                "reports a crash, hang, or unhandled exception then this termination " +
                "is the reported defect: assert it deliberately with an assertAppClosed " +
                "step instead of looking for an element. If the issue does not report a " +
                "crash, the scenario itself is crashing and must be simplified.";
        }
    }

    if (!IndicatesLostAppWindow(exception))
    {
        return null;
    }

    return "The automation session lost the app window, which means the app under " +
        "test closed or crashed before this step completed. If the issue reports a " +
        "crash, hang, or unhandled exception then this termination is the reported " +
        "defect: assert it deliberately with an assertAppClosed step instead of " +
        "looking for an element. If the issue does not report a crash, the scenario " +
        "itself is crashing and must be simplified.";
}

static bool IndicatesLostAppWindow(Exception exception)
{
    for (var current = exception; current is not null; current = current.InnerException)
    {
        if (current is NoSuchWindowException)
        {
            return true;
        }

        var message = current.Message;
        if (message.Contains("no such window", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("window has been closed", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("currently selected window", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}

static void AssertAppClosed(
    string platform,
    Process? launchedWindowsApp,
    TimeSpan timeout)
{
    if (platform != "windows" || launchedWindowsApp is null)
    {
        throw new InvalidOperationException(
            "assertAppClosed is supported only for the trusted Windows Sandbox process.");
    }

    var processId = launchedWindowsApp.Id;
    var deadline = Stopwatch.StartNew();
    while (deadline.Elapsed < timeout)
    {
        launchedWindowsApp.Refresh();
        if (launchedWindowsApp.HasExited)
        {
            Console.WriteLine(
                $"BUG REPRODUCED: Windows Sandbox process {processId} exited after the reported trigger.");
            return;
        }
        System.Threading.Thread.Sleep(200);
    }

    throw new TimeoutException(
        "Windows Sandbox process remained open after the reported crash trigger.");
}

static IWebElement WaitForElement(
    AppiumDriver driver,
    string platform,
    ReplicationLocator locator,
    TimeSpan timeout)
{
    var candidates = CreateLocatorCandidates(locator, platform);
    var wait = new WebDriverWait(driver, timeout);
    IWebElement? found = null;
    try
    {
        found = wait.Until(current =>
        {
            foreach (var by in candidates)
            {
                try
                {
                    var element = current.FindElement(by);
                    if (element.Displayed)
                    {
                        return element;
                    }
                }
                catch (NoSuchElementException)
                {
                }
                catch (StaleElementReferenceException)
                {
                }
                catch (InvalidSelectorException)
                {
                }
            }

            return null;
        });
    }
    catch (WebDriverTimeoutException timedOut)
    {
        // WebDriverWait raises its own timeout, so a throw placed after the wait
        // never runs. Without this catch the inventory below never reached the
        // agent and every retry re-guessed the same absent identifier.
        throw new WebDriverTimeoutException(
            DescribeMissingElement(driver, platform, locator, candidates),
            timedOut);
    }

    return found ?? throw new WebDriverTimeoutException(
        DescribeMissingElement(driver, platform, locator, candidates));
}

static string DescribeMissingElement(
    AppiumDriver driver,
    string platform,
    ReplicationLocator locator,
    IReadOnlyList<By> candidates)
{
    return
        $"Element was not visible: {locator.Strategy}={locator.Value}. " +
        $"Tried {candidates.Count} equivalent locator(s) on {platform}: " +
        string.Join(" | ", candidates.Select(by => by.ToString())) +
        ". Confirm the element sets AutomationId and is accessibility-visible." +
        Environment.NewLine +
        DescribeAddressableElements(driver);
}

// A locator timeout otherwise reports only what was searched for, which leaves
// the next attempt guessing at identifiers that may never have existed. Listing
// what the running app actually exposes turns that guess into a choice.
static string DescribeAddressableElements(AppiumDriver driver)
{
    const string ElementInventoryStart = "<<<REPLICATION_VISIBLE_ELEMENTS";
    const string ElementInventoryEnd = "REPLICATION_VISIBLE_ELEMENTS>>>";
    const int MaximumDescribedElements = 40;
    const int MaximumDescribedValueLength = 60;

    string source;
    try
    {
        source = driver.PageSource ?? string.Empty;
    }
    catch (Exception exception)
    {
        return $"{ElementInventoryStart} unavailable: {exception.GetType().Name}. {ElementInventoryEnd}";
    }

    if (source.Length == 0)
    {
        return $"{ElementInventoryStart} unavailable: the driver returned an empty page source. {ElementInventoryEnd}";
    }

    var seen = new HashSet<string>(StringComparer.Ordinal);
    var described = new List<string>();
    foreach (Match match in Regex.Matches(
        source,
        "(?<attribute>resource-id|content-desc|AutomationId|accessibilityIdentifier|name|label|text|value)=\"(?<value>[^\"]*)\"",
        RegexOptions.IgnoreCase))
    {
        if (described.Count >= MaximumDescribedElements)
        {
            break;
        }

        var value = match.Groups["value"].Value.Trim();
        if (value.Length == 0 || value.Length > MaximumDescribedValueLength)
        {
            continue;
        }

        // Android reports a package-qualified resource id; only the local part
        // is addressable through an AutomationId locator.
        var separator = value.LastIndexOf('/');
        if (separator >= 0 && separator < value.Length - 1)
        {
            value = value.Substring(separator + 1);
        }

        var attribute = match.Groups["attribute"].Value.ToLowerInvariant();
        var entry = $"{attribute}={value}";
        if (seen.Add(entry))
        {
            described.Add(entry);
        }
    }

    if (described.Count == 0)
    {
        return $"{ElementInventoryStart} none: the app exposes no identifying attributes on any element. {ElementInventoryEnd}";
    }

    return ElementInventoryStart + " " + string.Join(" | ", described) + " " + ElementInventoryEnd;
}

static void WaitForAbsence(
    AppiumDriver driver,
    string platform,
    ReplicationLocator locator,
    TimeSpan timeout)
{
    var candidates = CreateLocatorCandidates(locator, platform);
    var wait = new WebDriverWait(driver, timeout);
    if (!wait.Until(current => candidates.All(by =>
    {
        try
        {
            return current.FindElements(by).Count == 0;
        }
        catch (InvalidSelectorException)
        {
            return true;
        }
    })))
    {
        throw new InvalidOperationException(
            $"Element remained present: {locator.Strategy}={locator.Value}");
    }
}

// MAUI maps AutomationId onto a different native attribute per platform
// (content-desc or resource-id on Android, name on iOS, AutomationId on
// Windows), so a single strategy can miss an element that is really there.
// Every candidate still requires the same named element, so the oracle is
// unchanged; only the lookup is made robust.
static IReadOnlyList<By> CreateLocatorCandidates(
    ReplicationLocator locator,
    string platform)
{
    var primary = CreateLocator(locator);
    if (locator.Strategy is not ("accessibilityId" or "id"))
    {
        return new[] { primary };
    }

    var value = locator.Value;
    var candidates = new List<By> { primary };
    var alternate = locator.Strategy == "accessibilityId"
        ? MobileBy.Id(value)
        : MobileBy.AccessibilityId(value);
    candidates.Add(alternate);

    // An AutomationId that contains a quote cannot be embedded in an XPath
    // literal safely, so fall back to the attribute strategies only.
    var xpathSafe = !value.Contains("'", StringComparison.Ordinal) && !value.Contains("\"", StringComparison.Ordinal);
    var xpathValue = value;
    switch (platform)
    {
        case "android":
            candidates.Add(MobileBy.AndroidUIAutomator(
                $"new UiSelector().resourceIdMatches(\".*/{Regex.Escape(value)}\")"));
            if (xpathSafe)
            {
                candidates.Add(MobileBy.XPath(
                    $"//*[@content-desc='{xpathValue}' or @resource-id='{xpathValue}' or substring-after(@resource-id, '/')='{xpathValue}']"));
            }

            break;
        case "ios":
            if (xpathSafe)
            {
                candidates.Add(MobileBy.XPath(
                    $"//*[@name='{xpathValue}' or @label='{xpathValue}']"));
            }

            break;
        case "windows":
            if (xpathSafe)
            {
                candidates.Add(MobileBy.XPath(
                    $"//*[@AutomationId='{xpathValue}']"));
            }

            break;
    }

    return candidates;
}

static IWebElement? FindFirstDisplayed(
    ISearchContext context,
    IReadOnlyList<By> candidates)
{
    foreach (var by in candidates)
    {
        try
        {
            var element = context.FindElement(by);
            if (element.Displayed)
            {
                return element;
            }
        }
        catch (NoSuchElementException)
        {
        }
        catch (StaleElementReferenceException)
        {
        }
        catch (InvalidSelectorException)
        {
        }
    }

    return null;
}

static By CreateLocator(ReplicationLocator locator) =>
    locator.Strategy switch
    {
        "id" => MobileBy.Id(locator.Value),
        "accessibilityId" => MobileBy.AccessibilityId(locator.Value),
        "xpath" => MobileBy.XPath(locator.Value),
        "className" => MobileBy.ClassName(locator.Value),
        "androidText" => MobileBy.AndroidUIAutomator(
            $"new UiSelector().text(\"{locator.Value}\")"),
        _ => throw new InvalidOperationException(
            $"Unsupported locator strategy '{locator.Strategy}'.")
    };

static void AssertElementText(
    AppiumDriver driver,
    string platform,
    ReplicationStep step,
    TimeSpan timeout,
    bool contains,
    bool isFinalStep)
{
    var expected = step.Value!;
    var candidates = CreateLocatorCandidates(step.Locator!, platform);
    var wait = new WebDriverWait(driver, timeout);
    var actual = string.Empty;
    try
    {
        wait.Until(current =>
        {
            var element = FindFirstDisplayed(current, candidates);
            try
            {
                if (element is null)
                {
                    return false;
                }

                actual = ReadElementText(element);
                if (platform == "android" &&
                    string.IsNullOrWhiteSpace(actual) &&
                    IsAndroidTextVisible(current, expected, contains))
                {
                    actual = expected;
                }
                return contains
                    ? actual.Contains(expected, StringComparison.Ordinal)
                    : string.Equals(actual, expected, StringComparison.Ordinal);
            }
            catch (NoSuchElementException)
            {
                return false;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }
    catch (WebDriverTimeoutException exception)
    {
        var normalizedActual = actual.Trim();
        if (isFinalStep &&
            platform == "android" &&
            string.IsNullOrWhiteSpace(normalizedActual))
        {
            normalizedActual = ReadVisibleAndroidNegativeVerdict(driver);
        }
        if (isFinalStep &&
            (normalizedActual.StartsWith("PASS:", StringComparison.Ordinal) ||
             normalizedActual.StartsWith("NO BUG:", StringComparison.Ordinal)))
        {
            var sentinel =
                $"REPLICATION_NOT_REPRODUCED actual='{normalizedActual}'";
            Console.WriteLine(sentinel);
            throw new InvalidOperationException(sentinel, exception);
        }
        var comparison = contains ? "contain" : "equal";
        // An empty reading means the element itself was never found, so naming
        // only the expected text leaves the next attempt guessing at the same
        // absent identifier. Report what the app actually exposed instead.
        var inventory = string.IsNullOrWhiteSpace(actual)
            ? Environment.NewLine +
              $"The element was never found. Tried {candidates.Count} equivalent locator(s) on {platform}: " +
              string.Join(" | ", candidates.Select(by => by.ToString())) + "." +
              Environment.NewLine +
              DescribeAddressableElements(driver)
            : string.Empty;
        throw new InvalidOperationException(
            $"Expected element text to {comparison} '{expected}', actual '{actual}'." + inventory,
            exception);
    }
}

static string ReadElementText(IWebElement element)
{
    if (!string.IsNullOrWhiteSpace(element.Text))
    {
        return element.Text;
    }

    foreach (var attribute in new[] { "text", "value", "label", "name" })
    {
        var value = element.GetAttribute(attribute);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
    }

    return string.Empty;
}

static bool IsAndroidTextVisible(
    IWebDriver driver,
    string expected,
    bool contains)
{
    var escaped = expected
        .Replace("\\", "\\\\", StringComparison.Ordinal)
        .Replace("\"", "\\\"", StringComparison.Ordinal);
    var selector = contains
        ? $"new UiSelector().textContains(\"{escaped}\")"
        : IsReplicationVerdictPrefix(expected)
            ? $"new UiSelector().textStartsWith(\"{escaped}\")"
            : $"new UiSelector().text(\"{escaped}\")";
    return driver.FindElements(MobileBy.AndroidUIAutomator(selector))
        .Any(element => element.Displayed);
}

static bool IsReplicationVerdictPrefix(string expected) =>
    expected is "PASS:" or "NO BUG:" or "BUG REPRODUCED:";

static string ReadVisibleAndroidNegativeVerdict(AppiumDriver driver)
{
    foreach (var prefix in new[] { "PASS:", "NO BUG:" })
    {
        var selector = $"new UiSelector().textStartsWith(\"{prefix}\")";
        foreach (var element in driver.FindElements(
            MobileBy.AndroidUIAutomator(selector)))
        {
            if (!element.Displayed)
            {
                continue;
            }

            var actual = ReadElementText(element).Trim();
            if (actual.StartsWith(prefix, StringComparison.Ordinal))
            {
                return actual;
            }
        }
    }

    return string.Empty;
}

static void Swipe(AppiumDriver driver, string platform, string direction)
{
    if (platform == "windows")
    {
        throw new InvalidOperationException("Swipe is not supported by the Windows adapter.");
    }

    if (platform == "ios" || platform == "catalyst")
    {
        driver.ExecuteScript(
            "mobile: swipe",
            new Dictionary<string, object> { ["direction"] = direction });
        return;
    }

    var size = driver.Manage().Window.Size;
    driver.ExecuteScript(
        "mobile: swipeGesture",
        new Dictionary<string, object>
        {
            ["left"] = 0,
            ["top"] = 0,
            ["width"] = size.Width,
            ["height"] = size.Height,
            ["direction"] = direction,
            ["percent"] = 0.75
        });
}

static void DragPath(
    AppiumDriver driver,
    string platform,
    IWebElement element,
    string path)
{
    // A SwipeView, pan, or drag defect is triggered by one pointer that stays
    // down while it changes direction, which a single cardinal swipe cannot
    // express. Issue 37089 was abandoned for exactly that reason.
    if (platform is "windows" or "catalyst")
    {
        throw new InvalidOperationException(
            $"dragPath is not supported by the {platform} adapter.");
    }

    var segments = ParseDragSegments(path);
    var size = driver.Manage().Window.Size;
    var origin = element.Location;
    var extent = element.Size;
    var x = origin.X + (extent.Width / 2);
    var y = origin.Y + (extent.Height / 2);

    var finger = new PointerInputDevice(PointerKind.Touch, "finger");
    var sequence = new ActionSequence(finger);
    sequence.AddAction(finger.CreatePointerMove(
        CoordinateOrigin.Viewport, x, y, TimeSpan.Zero));
    sequence.AddAction(finger.CreatePointerDown(MouseButton.Touch));
    sequence.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(250)));

    foreach (var (dx, dy) in segments)
    {
        x = Math.Clamp(x + (int)Math.Round(dx * size.Width), 1, size.Width - 2);
        y = Math.Clamp(y + (int)Math.Round(dy * size.Height), 1, size.Height - 2);
        sequence.AddAction(finger.CreatePointerMove(
            CoordinateOrigin.Viewport, x, y, TimeSpan.FromMilliseconds(320)));
        sequence.AddAction(finger.CreatePause(TimeSpan.FromMilliseconds(140)));
    }

    sequence.AddAction(finger.CreatePointerUp(MouseButton.Touch));
    driver.PerformActions(new List<ActionSequence> { sequence });
}

static List<(double Dx, double Dy)> ParseDragSegments(string path)
{
    var segments = new List<(double, double)>();
    foreach (var raw in path.Split(';', StringSplitOptions.RemoveEmptyEntries))
    {
        var parts = raw.Split(',');
        if (parts.Length != 2 ||
            !double.TryParse(
                parts[0],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var dx) ||
            !double.TryParse(
                parts[1],
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var dy))
        {
            throw new InvalidOperationException($"Invalid dragPath segment '{raw}'.");
        }
        if (Math.Abs(dx) > 1 || Math.Abs(dy) > 1)
        {
            throw new InvalidOperationException(
                $"dragPath segment '{raw}' moves further than the screen.");
        }
        segments.Add((dx, dy));
    }

    if (segments.Count is < 2 or > 4)
    {
        throw new InvalidOperationException(
            "dragPath requires between two and four movement segments.");
    }

    return segments;
}

static void ValidatePlan(ReplicationPlan plan, string platform)
{
    if (plan.SchemaVersion != 1 ||
        plan.IssueNumber <= 0 ||
        plan.Steps.Count is < 1 or > 20)
    {
        throw new InvalidOperationException("Trusted Appium plan has invalid bounds.");
    }

    var assertions = new HashSet<string>(StringComparer.Ordinal)
    {
        "assertExists",
        "assertAppClosed",
        "assertNotExists",
        "assertTextEquals",
        "assertTextContains"
    };
    if (!assertions.Contains(plan.Steps[^1].Action))
    {
        throw new InvalidOperationException(
            "Trusted Appium plan must end with a deterministic assertion.");
    }

    foreach (var step in plan.Steps)
    {
        if (step.Locator?.Strategy != "androidText")
        {
            continue;
        }

        if (platform != "android" ||
            step.Locator.Value.Length is < 1 or > 200 ||
            !Regex.IsMatch(step.Locator.Value, @"^[A-Za-z0-9 _.,:;!?()/+=-]+$"))
        {
            throw new InvalidOperationException(
                "Trusted Appium plan contains an invalid Android text locator.");
        }
    }
}

sealed class ReplicationPlan
{
    public int SchemaVersion { get; set; }

    public int IssueNumber { get; set; }

    public List<ReplicationStep> Steps { get; set; } = [];
}

sealed class ReplicationStep
{
    public string Action { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public ReplicationLocator? Locator { get; set; }

    public string? Value { get; set; }

    public int TimeoutSeconds { get; set; }
}

sealed class ReplicationLocator
{
    public string Strategy { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;
}

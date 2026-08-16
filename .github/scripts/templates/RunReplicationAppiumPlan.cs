#!/usr/bin/env dotnet run
#:package Appium.WebDriver@8.0.1

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Text.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.Enums;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Mac;
using OpenQA.Selenium.Appium.Windows;
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

    for (var index = 0; index < plan.Steps.Count; index++)
    {
        var step = plan.Steps[index];
        Console.WriteLine($"STEP {index + 1}/{plan.Steps.Count}: {step.Description}");
        ExecuteStep(driver, platform, step);
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
            options.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);
            return new AndroidDriver(server, options);
        case "ios":
            options.PlatformName = "iOS";
            options.AutomationName = "XCUITest";
            options.AddAdditionalAppiumOption("appium:bundleId", "com.microsoft.maui.sandbox");
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
            launchedWindowsApp = Process.Start(new ProcessStartInfo(appPath)
            {
                UseShellExecute = false,
                WorkingDirectory = Path.GetDirectoryName(appPath)
                    ?? throw new InvalidOperationException("Windows app directory is unavailable.")
            }) ?? throw new InvalidOperationException("Windows Sandbox process did not start.");
            var windowDeadline = DateTime.UtcNow.AddSeconds(30);
            while (DateTime.UtcNow < windowDeadline)
            {
                launchedWindowsApp.Refresh();
                if (launchedWindowsApp.HasExited)
                {
                    throw new InvalidOperationException(
                        $"Windows Sandbox exited with code {launchedWindowsApp.ExitCode} before creating a window.");
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

static void ExecuteStep(AppiumDriver driver, string platform, ReplicationStep step)
{
    var timeout = TimeSpan.FromSeconds(step.TimeoutSeconds);
    switch (step.Action)
    {
        case "waitFor":
            _ = WaitForElement(driver, step.Locator!, timeout);
            break;
        case "tap":
            WaitForElement(driver, step.Locator!, timeout).Click();
            break;
        case "clear":
            WaitForElement(driver, step.Locator!, timeout).Clear();
            break;
        case "enterText":
            WaitForElement(driver, step.Locator!, timeout).SendKeys(step.Value!);
            break;
        case "assertExists":
            _ = WaitForElement(driver, step.Locator!, timeout);
            break;
        case "assertNotExists":
            WaitForAbsence(driver, step.Locator!, timeout);
            break;
        case "assertTextEquals":
            AssertElementText(driver, step, timeout, contains: false);
            break;
        case "assertTextContains":
            AssertElementText(driver, step, timeout, contains: true);
            break;
        case "back":
            driver.Navigate().Back();
            break;
        case "swipe":
            Swipe(driver, platform, step.Value!);
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

static IWebElement WaitForElement(
    AppiumDriver driver,
    ReplicationLocator locator,
    TimeSpan timeout)
{
    var by = CreateLocator(locator);
    var wait = new WebDriverWait(driver, timeout);
    return wait.Until(current =>
    {
        try
        {
            var element = current.FindElement(by);
            return element.Displayed ? element : null;
        }
        catch (NoSuchElementException)
        {
            return null;
        }
        catch (StaleElementReferenceException)
        {
            return null;
        }
    }) ?? throw new WebDriverTimeoutException(
        $"Element was not visible: {locator.Strategy}={locator.Value}");
}

static void WaitForAbsence(
    AppiumDriver driver,
    ReplicationLocator locator,
    TimeSpan timeout)
{
    var by = CreateLocator(locator);
    var wait = new WebDriverWait(driver, timeout);
    if (!wait.Until(current => current.FindElements(by).Count == 0))
    {
        throw new InvalidOperationException(
            $"Element remained present: {locator.Strategy}={locator.Value}");
    }
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
    ReplicationStep step,
    TimeSpan timeout,
    bool contains)
{
    var expected = step.Value!;
    var by = CreateLocator(step.Locator!);
    var wait = new WebDriverWait(driver, timeout);
    var actual = string.Empty;
    try
    {
        wait.Until(current =>
        {
            try
            {
                var element = current.FindElement(by);
                if (!element.Displayed)
                {
                    return false;
                }

                actual = element.Text ?? string.Empty;
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
        var comparison = contains ? "contain" : "equal";
        throw new InvalidOperationException(
            $"Expected element text to {comparison} '{expected}', actual '{actual}'.",
            exception);
    }
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

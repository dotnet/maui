#!/usr/bin/env dotnet run
#:package Appium.WebDriver@8.0.1

/*
 * Template: Appium Test Script for .NET MAUI Issue Reproduction
 * 
 * INSTRUCTIONS FOR AGENT:
 * 1. Copy this file to: CustomAgentLogsTmp/Sandbox/RunWithAppiumTest.cs
 * 2. Replace XXXXX with actual issue number (const ISSUE_NUMBER)
 * 3. Set PLATFORM to "android" or "ios"
 * 4. Implement test logic in the "Test Logic" section
 * 5. Run via: pwsh .github/scripts/BuildAndRunSandbox.ps1 -Platform android
 * 
 * IMPORTANT:
 * - For Android: Script outputs SANDBOX_APP_PID=<pid> which PowerShell captures for logcat filtering
 * - For iOS: PID capture not needed (iOS uses different logging mechanism)
 * - The BuildAndRunSandbox.ps1 script will automatically capture all logs
 * 
 * 🚨 CRITICAL - ANDROID REQUIREMENT:
 * - The "appium:noReset" capability MUST be set to true for Android (line 63)
 * - Without it, Appium clears app data causing Fast Deployment to fail
 * - This results in "No assemblies found" crash on app launch
 * - ⚠️  NEVER REMOVE the noReset capability - Android tests depend on it
 */

using System;
using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using OpenQA.Selenium.Appium.Enums;

// ========== CONFIGURATION ==========

const int ISSUE_NUMBER = 00000; // TODO: Replace with actual issue number

// ========== DEVICE SETUP ==========

var udid = Environment.GetEnvironmentVariable("DEVICE_UDID");
if (string.IsNullOrEmpty(udid))
{
    Console.WriteLine("❌ ERROR: DEVICE_UDID environment variable not set!");
    Console.WriteLine("This should be set automatically by BuildAndRunSandbox.ps1 script.");
    Environment.Exit(1);
}

// Auto-detect platform from UDID format
// iOS UDIDs contain hyphens and are longer (e.g., AC8BCB28-A72D-4A2D-90E7-E78FF0BA07EE)
// Android UDIDs are shorter (e.g., emulator-5554, 192.168.1.100:5555)
string PLATFORM = udid.Contains("-") && udid.Length > 20 ? "ios" : "android";

Console.WriteLine($"═══════════════════════════════════════════════════════");
Console.WriteLine($"  Testing Issue #{ISSUE_NUMBER} on {PLATFORM.ToUpper(System.Globalization.CultureInfo.InvariantCulture)}");
Console.WriteLine($"  Device UDID: {udid}");
Console.WriteLine($"═══════════════════════════════════════════════════════\n");

// ========== APPIUM CONNECTION ==========
// Note: PID will be captured after connecting to app

var serverUri = new Uri("http://localhost:4723");
AppiumOptions options;

// ========== PLATFORM-SPECIFIC OPTIONS ==========

if (PLATFORM == "android")
{
    options = new AppiumOptions();
    options.PlatformName = "Android";
    options.AutomationName = "UIAutomator2";
    options.AddAdditionalAppiumOption("appium:appPackage", "com.microsoft.maui.sandbox");
    options.AddAdditionalAppiumOption("appium:appActivity", "com.microsoft.maui.sandbox.MainActivity");
    
    // 🚨 CRITICAL: noReset MUST be set to true for Android
    // Without this, Appium clears app data between runs causing Fast Deployment to fail
    // This results in "No assemblies found" crash immediately on app launch
    // ⚠️  DO NOT REMOVE THIS LINE - Android tests will fail without it
    options.AddAdditionalAppiumOption("appium:noReset", true);
    
    options.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);
    options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
}
else if (PLATFORM == "ios")
{
    options = new AppiumOptions();
    options.PlatformName = "iOS";
    options.AutomationName = "XCUITest";
    options.AddAdditionalAppiumOption("appium:bundleId", "com.microsoft.maui.sandbox");
    options.AddAdditionalAppiumOption(MobileCapabilityType.Udid, udid);
    options.AddAdditionalAppiumOption("appium:newCommandTimeout", 300);
}
else
{
    Console.WriteLine($"❌ ERROR: Unsupported platform: {PLATFORM}");
    Environment.Exit(1);
    return; // Make compiler happy
}

// ========== CONNECT TO APPIUM ==========

Console.WriteLine("Connecting to Appium server...");

try
{
    AppiumDriver driver;
    if (PLATFORM == "android")
    {
        driver = new AndroidDriver(serverUri, options);
    }
    else // ios
    {
        driver = new IOSDriver(serverUri, options);
    }
    
    using (driver)
    {
        Console.WriteLine("✅ Connected to Appium and launched app!\n");
    
        // ========== GET APP PID (for logcat filtering) ==========
        
        if (PLATFORM == "android")
        {
            try
            {
                var getPidProcess = System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "adb",
                    Arguments = $"-s {udid} shell pidof -s com.microsoft.maui.sandbox",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (getPidProcess != null)
                {
                    getPidProcess.WaitForExit();
                    var pid = getPidProcess.StandardOutput.ReadToEnd().Trim();

                    if (!string.IsNullOrEmpty(pid))
                    {
                        Console.WriteLine($"SANDBOX_APP_PID={pid}");
                        Console.WriteLine($"✅ Captured app PID: {pid}\n");
                    }
                    else
                    {
                        Console.WriteLine("⚠️  Warning: Could not get app PID (app may not be running yet)\n");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"⚠️  Warning: Failed to capture PID: {ex.Message}\n");
            }
        }
    
        // Wait for app to load
        Thread.Sleep(3000);
    
    // ========== TEST LOGIC ==========
    // TODO: Implement test logic here
    
    Console.WriteLine("🔹 Looking for test elements...");
    
    // Example: Find button by text (adjust based on actual UI)
    try
    {
        // Platform-specific element locators:
        // Android: MobileBy.Id("AutomationId") - maps to resource-id
        // iOS: MobileBy.AccessibilityId("AutomationId") - maps to accessibility identifier
        
        IWebElement button;
        if (PLATFORM == "android")
        {
            // Android: Find by resource-id or XPath
            button = driver.FindElement(MobileBy.XPath("//*[contains(@text, 'Test Button')]"));
        }
        else
        {
            // iOS: Find by accessibility id or XPath
            button = driver.FindElement(MobileBy.AccessibilityId("TestButton"));
        }
        
        Console.WriteLine($"✅ Found button: {button.Text}");
        
        // Take screenshot before test (for debugging/documentation only)
        // NOTE: NEVER use screenshots for validation - always use Appium element queries
        // NOTE: Script runs FROM CustomAgentLogsTmp/Sandbox/ so use relative paths
        var screenshotBefore = driver.GetScreenshot();
        screenshotBefore.SaveAsFile("issue_XXXXX_before.png");
        Console.WriteLine("📸 Screenshot saved: issue_XXXXX_before.png");
        
        // Perform action
        Console.WriteLine("\n🔘 Tapping button...");
        button.Click();
        
        // Wait for result
        Thread.Sleep(3000);
        
        // Validate result
        // TODO: Add validation logic here
        // Example: Check if status label shows success
        
        var statusElements = driver.FindElements(
            MobileBy.XPath("//*[contains(@text, 'Success') or contains(@text, 'Failed')]")
        );
        
        if (statusElements.Count > 0)
        {
            var statusText = statusElements[0].Text;
            Console.WriteLine($"\n📊 Status: {statusText}");
            
            if (statusText.Contains("Success", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("✅ TEST PASSED: Issue is resolved!");
            }
            else if (statusText.Contains("Failed", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("❌ TEST FAILED: Issue still exists");
            }
        }
        else
        {
            Console.WriteLine("⚠️  Could not find status indicator");
        }
        
        // Take screenshot after test (for debugging/documentation only)
        // NOTE: NEVER use screenshots for validation - always use Appium element queries
        // NOTE: Script runs FROM CustomAgentLogsTmp/Sandbox/ so use relative paths
        var screenshotAfter = driver.GetScreenshot();
        screenshotAfter.SaveAsFile("issue_XXXXX_after.png");
        Console.WriteLine("📸 Screenshot saved: issue_XXXXX_after.png");
        
        // Check if UI is visible (not blank)
        var pageSource = driver.PageSource;
        if (string.IsNullOrWhiteSpace(pageSource) || pageSource.Length < 100)
        {
            Console.WriteLine("\n❌ BUG REPRODUCED: UI appears blank!");
            Console.WriteLine("   Page source is empty or too small");
        }
        else
        {
            Console.WriteLine("\n✅ UI is visible (not blank)");
        }
        
    }
    catch (NoSuchElementException ex)
    {
        Console.WriteLine($"❌ Could not find expected UI element: {ex.Message}");
        Console.WriteLine("\nPage source:");
        Console.WriteLine(driver.PageSource);
        Environment.Exit(1);
    }
    
        // ========== END TEST LOGIC ==========
        
        Console.WriteLine("\n" + new string('═', 55));
        Console.WriteLine("  Test completed successfully");
        Console.WriteLine(new string('═', 55) + "\n");
        
        // Optional: Keep app open for manual inspection
        // Console.WriteLine("Press Enter to close app...");
        // Console.ReadLine();
    }
}
catch (Exception ex)
{
    Console.WriteLine($"\n❌ ERROR: Test failed");
    Console.WriteLine($"Exception: {ex.Message}");
    Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
    
    Console.WriteLine("\n🔍 Troubleshooting:");
    Console.WriteLine("  1. Verify Appium is running: curl http://localhost:4723/status");
    Console.WriteLine("  2. Check device UDID: echo $DEVICE_UDID");
    Console.WriteLine("  3. Verify app is installed on device");
    Console.WriteLine("  4. Review Appium logs for detailed error information");
    
    Environment.Exit(1);
}

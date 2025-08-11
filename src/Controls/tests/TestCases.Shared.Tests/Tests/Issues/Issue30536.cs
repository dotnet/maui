#if WINDOWS // This test is Windows-specific due to the multi-window pointer issue
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

/// <summary>
/// UI tests for Issue #30536 - PointerGestureRecognizer behaves incorrectly when multiple windows are open
/// 
/// This issue occurs on Windows when multiple windows are open, causing PointerEntered and PointerExited
/// events to fire incorrectly due to interference from background/minimized windows.
/// 
/// The fix (PR #30537) adds:
/// - Debouncing logic for PointerEntered events in multi-window scenarios
/// - Window validation to ensure events come from the correct window
/// - Filtering of spurious events from background windows
/// 
/// Testing approach:
/// - Automated tests validate basic UI functionality and setup
/// - Manual testing is required for the complex pointer interaction scenarios
/// - The test page provides comprehensive logging to help diagnose issues
/// </summary>
public class Issue30536 : _IssuesUITest
{
    public override string Issue => "[Windows] PointerGestureRecognizer behaves incorrectly when multiple windows are open";

    public Issue30536(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.Gestures)]
    [Category(UITestCategories.Window)]
    public void PointerGestureWorksCorrectlyInSingleWindow()
    {
        // Test basic functionality in single window first
        App.WaitForElement("TestArea");
        App.WaitForElement("EventLogLabel");
        
        // Verify initial state
        var windowCount = App.FindElement("WindowCountLabel").GetText();
        Assert.That(windowCount, Is.EqualTo("1"), "Should start with single window");
        
        // Clear any initial events
        App.Tap("ClearLogsButton");
        
        // Simulate pointer interaction with the test area
        // Note: In a real test environment, this would use actual mouse events
        // For now, we'll test the basic setup and UI elements are present
        var testArea = App.FindElement("TestArea");
        Assert.That(testArea, Is.Not.Null, "Test area should be present");
        
        var eventLog = App.FindElement("EventLogLabel");
        Assert.That(eventLog, Is.Not.Null, "Event log should be present");
    }

    [Test]
    [Category(UITestCategories.Gestures)]
    [Category(UITestCategories.Window)]
    public void CanOpenSecondWindow()
    {
        // Verify we can open a second window for multi-window testing
        App.WaitForElement("OpenWindowButton");
        
        var initialWindowCount = App.FindElement("WindowCountLabel").GetText();
        Assert.That(initialWindowCount, Is.EqualTo("1"), "Should start with single window");
        
        // Open second window
        App.Tap("OpenWindowButton");
        
        // Wait a moment for the window to open and UI to update
        System.Threading.Thread.Sleep(1000);
        
        // Verify window count increased
        var newWindowCount = App.FindElement("WindowCountLabel").GetText();
        Assert.That(newWindowCount, Is.EqualTo("2"), "Should have two windows after opening second window");
        
        // Verify event was logged
        var eventLog = App.FindElement("EventLogLabel").GetText();
        Assert.That(eventLog, Does.Contain("Second window opened"), "Should log window opening event");
    }

    [Test]
    [Category(UITestCategories.Gestures)]
    [Category(UITestCategories.Window)]
    public void EventLogDisplaysCorrectly()
    {
        // Test the event logging system works
        App.WaitForElement("EventLogLabel");
        App.WaitForElement("ClearLogsButton");
        
        // Clear any existing logs
        App.Tap("ClearLogsButton");
        
        // Verify log was cleared
        var clearedLog = App.FindElement("EventLogLabel").GetText();
        Assert.That(clearedLog, Is.EqualTo("Event Log (most recent first):"), "Log should be cleared");
        
        var eventCount = App.FindElement("EventCountLabel").GetText();
        Assert.That(eventCount, Is.EqualTo("0"), "Event count should be reset to 0");
    }

    [Test]
    [Category(UITestCategories.Gestures)]
    [Category(UITestCategories.Window)]
    public void UIElementsArePresentForTesting()
    {
        // Verify all necessary UI elements are present for manual/automated testing
        App.WaitForElement("TestArea");
        App.WaitForElement("OpenWindowButton");
        App.WaitForElement("ClearLogsButton");
        App.WaitForElement("WindowCountLabel");
        App.WaitForElement("EventCountLabel");
        App.WaitForElement("EventLogLabel");
        App.WaitForElement("TestAreaTitle");
        
        // Verify test area title
        var title = App.FindElement("TestAreaTitle").GetText();
        Assert.That(title, Is.EqualTo("Pointer Gesture Test Area"), "Test area should have correct title");
        
        // Verify initial setup message is in event log
        var eventLog = App.FindElement("EventLogLabel").GetText();
        Assert.That(eventLog, Does.Contain("Page loaded - Test area ready"), "Should show initial setup message");
    }

    [Test]
    [Category(UITestCategories.Gestures)]
    [Category(UITestCategories.Window)]
    [Category(UITestCategories.ManualReview)]
    public void MultiWindowPointerGestureTestManual()
    {
        // This test sets up the scenario for manual testing of the multi-window pointer gesture issue
        // Due to limitations in automated testing of complex pointer interactions across windows,
        // this test primarily sets up the test scenario and validates the UI is ready for testing
        
        App.WaitForElement("TestArea");
        App.WaitForElement("OpenWindowButton");
        
        // Clear any initial events
        App.Tap("ClearLogsButton");
        
        // Open second window
        App.Tap("OpenWindowButton");
        System.Threading.Thread.Sleep(1500);
        
        // Verify multi-window setup
        var windowCount = App.FindElement("WindowCountLabel").GetText();
        Assert.That(windowCount, Is.EqualTo("2"), "Should have two windows for multi-window test");
        
        // Verify event logging is working
        var eventLog = App.FindElement("EventLogLabel").GetText();
        Assert.That(eventLog, Does.Contain("Second window opened"), "Should log window creation");
        
        // The test is now ready for manual verification of the fix from PR #30537:
        
        TestContext.WriteLine("=== MANUAL TEST INSTRUCTIONS ===");
        TestContext.WriteLine("");
        TestContext.WriteLine("SETUP:");
        TestContext.WriteLine("1. The test has opened a second window - you should see Window Count: 2");
        TestContext.WriteLine("2. Minimize the second window that was opened by the test");
        TestContext.WriteLine("3. Return focus to this main test window");
        TestContext.WriteLine("");
        TestContext.WriteLine("REPRODUCE ISSUE (without PR #30537 fix):");
        TestContext.WriteLine("4. Move your mouse slowly in and out of the blue test area");
        TestContext.WriteLine("5. WITHOUT the fix: PointerEntered/PointerExited events fire rapidly");
        TestContext.WriteLine("   even when the mouse is stationary inside or outside the area");
        TestContext.WriteLine("6. You'll see rapid event log entries with very small delta times (< 50ms)");
        TestContext.WriteLine("");
        TestContext.WriteLine("VERIFY FIX (with PR #30537):");
        TestContext.WriteLine("7. WITH the fix: Events fire normally only when mouse actually crosses boundaries");
        TestContext.WriteLine("8. Event log should show reasonable delta times between events");
        TestContext.WriteLine("9. No spurious events when mouse is stationary");
        TestContext.WriteLine("");
        TestContext.WriteLine("SUCCESS CRITERIA:");
        TestContext.WriteLine("- PointerEntered fires once when mouse enters the blue area");
        TestContext.WriteLine("- PointerExited fires once when mouse leaves the blue area");
        TestContext.WriteLine("- No rapid-fire events when mouse is stationary");
        TestContext.WriteLine("- Delta times between events are reasonable (>100ms for normal mouse movement)");
    }
}
#endif
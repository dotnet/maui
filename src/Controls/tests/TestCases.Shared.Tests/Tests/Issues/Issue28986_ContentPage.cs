#if TEST_FAILS_ON_WINDOWS && TEST_FAILS_ON_CATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_ContentPage : _IssuesUITest
{
    public override string Issue => "Test SafeArea ContentPage for per-edge safe area control";

    public Issue28986_ContentPage(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void SafeAreaContentPageBasicFunctionality()
    {
        // Wait for the page to load and get initial settings
        var initialSettings = App.WaitForElement("CurrentSettings").GetText();

        // 1. Verify initial state - should be Default (as set in constructor)
        Assert.That(initialSettings, Does.Contain("Left: Default, Top: Default, Right: Default, Bottom: Default"));

        // Get a reference element to measure positions relative to the ContentPage
        var scrollViewContent = App.WaitForElement("CurrentSettings");
        var contentPageWithDefaultSettings = scrollViewContent.GetRect();

        // 2. Change SafeAreaEdges to All
        App.Tap("ResetAllButton");
        var allSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(allSettings, Does.Contain("Left: All, Top: All, Right: All, Bottom: All"));

        // Verify that content position changes when SafeAreaEdges changes from Default to All
        var contentPageSafeAreaEdgesAll = App.WaitForElement("CurrentSettings").GetRect();
        Assert.That(contentPageSafeAreaEdgesAll.Y, Is.GreaterThanOrEqualTo(contentPageWithDefaultSettings.Y),
            "ContentPage content should move down (greater Y position) when SafeAreaEdges changes from Default to All");

        // 3. Change SafeAreaEdges to None (edge-to-edge)
        App.Tap("ResetNoneButton");
        var noneSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(noneSettings, Does.Contain("Left: None, Top: None, Right: None, Bottom: None"));

        // Verify that content position changes when SafeAreaEdges is None
        var contentPageSafeAreaEdgesNone = App.WaitForElement("CurrentSettings").GetRect();
        Assert.That(contentPageSafeAreaEdgesNone.Y, Is.LessThanOrEqualTo(contentPageSafeAreaEdgesAll.Y),
            "ContentPage content should move up (lesser Y position) when SafeAreaEdges changes from All to None");

        // 4. Test individual edge control - Top edge specifically
        App.Tap("ResetNoneButton"); // Start with None

        // Change only the top edge to All
        App.Tap("TopPicker");
        App.Tap("All"); // Select "All" from the picker

        var topAllSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(topAllSettings, Does.Contain("Top: All"));

        // Verify that content moves down when top edge is set to All
        var contentPageTopAll = App.WaitForElement("CurrentSettings").GetRect();
        Assert.That(contentPageTopAll.Y, Is.GreaterThanOrEqualTo(contentPageSafeAreaEdgesNone.Y),
            "ContentPage content should move down when only top SafeAreaEdge is set to All");

        // 5. Test SoftInput functionality if available
        var softInputEntry = App.FindElement("SoftInputTestEntry");
        if (softInputEntry != null)
        {
            // Reset to test SoftInput
            App.Tap("ResetNoneButton");

            // Change only bottom edge to SoftInput
            App.Tap("BottomPicker");
            App.Tap("SoftInput");

            var softInputSettings = App.WaitForElement("CurrentSettings").GetText();
            Assert.That(softInputSettings, Does.Contain("Bottom: SoftInput"));

            // Tap the entry to potentially show keyboard
            App.Tap("SoftInputTestEntry");

            // The behavior here would depend on platform-specific keyboard handling
            // We mainly verify the setting was applied correctly
            App.DismissKeyboard();
        }

        // 6. Reset to Default to verify we can return to initial state
        App.Tap("ResetDefaultButton");
        var finalSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(finalSettings, Does.Contain("Left: Default, Top: Default, Right: Default, Bottom: Default"));
    }

    [Test]
    [Category(UITestCategories.SafeAreaEdges)]
    public void SafeAreaContentPageIndividualEdgeControl()
    {
        // Test individual edge controls
        App.WaitForElement("CurrentSettings");

        // Start with all None
        App.Tap("ResetNoneButton");
        var noneSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(noneSettings, Does.Contain("Left: None, Top: None, Right: None, Bottom: None"));

        // Test Left edge
        App.Tap("LeftPicker");
        App.Tap("All");
        var leftAllSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(leftAllSettings, Does.Contain("Left: All"));
        Assert.That(leftAllSettings, Does.Contain("Top: None, Right: None, Bottom: None"));

        // Test Right edge  
        App.Tap("RightPicker");
        App.Tap("Container");
        var rightContainerSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(rightContainerSettings, Does.Contain("Right: Container"));
        Assert.That(rightContainerSettings, Does.Contain("Left: All")); // Should still be All

        // Test Bottom edge
        App.Tap("BottomPicker");
        App.Tap("Default");
        var bottomDefaultSettings = App.WaitForElement("CurrentSettings").GetText();
        Assert.That(bottomDefaultSettings, Does.Contain("Bottom: Default"));
        Assert.That(bottomDefaultSettings, Does.Contain("Left: All"));
        Assert.That(bottomDefaultSettings, Does.Contain("Right: Container"));
    }
}
#endif

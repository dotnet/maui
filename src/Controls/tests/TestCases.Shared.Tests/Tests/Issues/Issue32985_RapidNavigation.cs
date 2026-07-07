using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32985_RapidNavigation : _IssuesUITest
{
    public Issue32985_RapidNavigation(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Shell rapid overlapping navigations should not drop pages";

    [Test]
    [Category(UITestCategories.Shell)]
    public void RapidOverlappingPushShouldNotDropPages()
    {
        // Tap button that fires two rapid PushAsync calls
        App.WaitForElement("RapidPushButton");
        App.Tap("RapidPushButton");

        // Page 3 (second push) should be visible.
        App.WaitForElement("Page3Label");

        // Verify stack depth is 3 (root + page2 + page3)
        App.WaitForElement("CheckStackButton");
        App.Tap("CheckStackButton");

        var depth = App.WaitForElement("FinalStackDepth").GetText();
        Assert.That(depth, Is.EqualTo("Depth: 3"),
            "Navigation stack should have 3 pages (root + 2 rapid pushes)");

        // Go back to page 2
        App.Tap("GoBackButton");
        App.WaitForElement("Page2Label");

        // Go back to root
        App.Tap("GoBackFromPage2Button");
        App.WaitForElement("RapidPushButton");
    }
}

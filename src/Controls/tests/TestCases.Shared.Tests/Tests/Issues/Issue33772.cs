using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33772 : _IssuesUITest
{
    public Issue33772(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "Shell SearchHandler SearchBoxVisibility does not update when changed dynamically";

    [Test, Order(1)]
    [Category(UITestCategories.Shell)]
    public void SearchHandlerVisibilityChangesToExpanded()
    {
        App.WaitForElement("TitleLabel");
        App.Tap("ExpandButton");
        VerifyScreenshot();
    }

    [Test, Order(2)]
    [Category(UITestCategories.Shell)]
    public void SearchHandlerVisibilityChangesToCollapsible()
    {
        // Wait for the page to load
        App.WaitForElement("TitleLabel");
        App.Tap("CollapsibleButton");
        VerifyScreenshot();
    }
}

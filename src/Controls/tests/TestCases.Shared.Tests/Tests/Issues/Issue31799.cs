#if TEST_FAILS_ON_CATALYST // Issue Link : https://github.com/dotnet/maui/issues/32900
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31799 : _IssuesUITest
{
    public Issue31799(TestDevice device) : base(device)
    {
    }

    public override string Issue => "WinUI TabbedPage can have multiple tabs selected";

    [Test]
    [Category(UITestCategories.TabbedPage)]
    public void VerifyTabbedPageDoesNotHaveMultipleTabsSelected()
    {
        App.WaitForElement("SelectedTabLabel");
        App.TapTab("Unselected Tab");

        App.WaitForElement("CreateTabbedPageButton");
        App.Tap("CreateTabbedPageButton");

        App.WaitForElement("NewSelectedTabLabel");
        VerifyScreenshot();
    }
}
#endif
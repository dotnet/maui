using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33340 : _IssuesUITest
{
    public Issue33340(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "NavigationBar overlaps StatusBar in TabbedPage";

    [Test]
    [Category(UITestCategories.TabbedPage)]
    public void NavigationBarLayoutWithMixedHasNavigationBar()
    {
        App.WaitForElement("Tab4");
        App.Tap("Tab4");
        VerifyScreenshot();
    }
}

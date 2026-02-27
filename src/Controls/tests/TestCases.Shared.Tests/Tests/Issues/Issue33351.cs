using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33351 : _IssuesUITest
{
    public Issue33351(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Changing Shell Tab Visibility when navigating back multiple pages ignores Shell Tab Visibility";

    [Test]
    [Category(UITestCategories.Shell)]
    public void TabBarVisibilityAfterMultiLevelPopToRoot()
    {
        App.WaitForElement("RootPage");
        App.WaitForElement("TabBarVisibleLabel");

        App.Tap("PushPage1Button");
        App.WaitForElement("TabBarHiddenLabel");

        App.Tap("PushPage2Button");
        App.WaitForElement("Page2");
        App.WaitForElement("TabBarHiddenLabel2");

        App.Tap("PopToRootButton");

        App.WaitForElement("RootPage");
        App.WaitForElement("TabBarVisibleLabel");

        VerifyScreenshot();
    }
}

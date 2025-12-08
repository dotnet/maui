using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32329 : _IssuesUITest
{
    public Issue32329(TestDevice device) : base(device)
    {
    }

    public override string Issue => "TabBar not visible on Mac Catalyst";

    [Test]
    [Category(UITestCategories.Shell)]
    public void TabBarShouldBeVisibleOnMacCatalyst()
    {
        App.WaitForElement("HomePageLabel");
        VerifyScreenshot();
    }
}

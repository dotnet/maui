using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class IssueNavbarVisible : _IssuesUITest
{
    public IssueNavbarVisible(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "NavBar visibility does not update when switching tabs";

    [Test]
    [Category(UITestCategories.Shell)]
    public void NavBarUpdatesWhenSwitchingShellContent()
    {
#if WINDOWS
        App.TapTab("First Tab");
        App.WaitForElement("Page 1");
        App.Tap("Page 1");
        App.TapTab("First Tab");
        App.WaitForElement("Page");
        App.Tap("Page");
        App.WaitForElement("Page1Label");
#else
        App.TapTab("Page 1");
        App.TapTab("Page");
        App.WaitForElement("Page1Label");
#endif
        VerifyScreenshot();
    }
}
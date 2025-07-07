#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21963 : _IssuesUITest
{
    public Issue21963(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Shell.MenuItemTemplate Sometime Does Not Work";

    [Test]
    [Category(UITestCategories.Shell)]
    public void VerifyFlyoutAndTemplatesPresentInitially()
    {

        App.WaitForElement("ShellContent 1");
        App.WaitForElement("ShellContent 2");
        App.WaitForElement("MenuItem 1");
        App.WaitForElement("MenuItem 2");
        App.WaitForElement("ShellContent 3");
    }
}
#endif
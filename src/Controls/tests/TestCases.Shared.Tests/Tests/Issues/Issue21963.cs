#if TEST_FAILS_ON_WINDOWS// This test fails on Windows: Shell.MenuItemTemplate behaves correctly,AutomationId does not work as expected on the Windows platform.
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
        App.WaitForElement("Issue21963ShellContent1");
        App.WaitForElement("Issue21963ShellContent2");
        App.WaitForElement("Issue21963MenuItem1");
        App.WaitForElement("Issue21963MenuItem2");
        App.WaitForElement("Issue21963ShellContent3");
    }
}
#endif
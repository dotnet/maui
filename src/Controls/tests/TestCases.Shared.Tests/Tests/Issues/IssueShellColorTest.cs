using Microsoft.Maui.TestCases.Tests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Maui.Controls.TestCases.Tests.Issues;

public class IssueShellColorTest : _IssuesUITest
{
    public IssueShellColorTest(TestDevice device) : base(device)
    {
    }

    public override string Issue => "Shell NavigationBar colors persist when navigating between pages";

    [Test]
    [Category(UITestCategories.Shell)]
    public void ShellColorsResetOnNavigation()
    {
#if WINDOWS
        App.TapTab("Tab1");
        App.WaitForElement("Page2");
        App.Tap("Page2");
        App.TapTab("Tab1");
        App.WaitForElement("Page1");
        App.Tap("Page1");
        App.WaitForElement("Page1Label");
#else
        App.TapTab("Page2");
        App.TapTab("Page1");
        App.WaitForElement("Page1Label");
#endif
        VerifyScreenshot();
    }
}
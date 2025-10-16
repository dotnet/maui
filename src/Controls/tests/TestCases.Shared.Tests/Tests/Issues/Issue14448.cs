using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue14448 : _IssuesUITest
{
    public Issue14448(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "maui title bar disappears and does not re-appear on iOS when using shell.searchhandler";

    [Test]
    [Category(UITestCategories.Shell)]
    public void ShellTitleShouldNotDisappear()
    {
        // Try to locate the search field by placeholder text
        App.WaitForElement("SearchHandler");

        // Tap on the search field to focus it
        App.Tap("SearchHandler");

        // Enter text into the search field
        App.EnterText("SearchHandler", "Item 1");
        // Dismiss keyboard by tapping return/search on iOS
        App.PressEnter();
        VerifyScreenshot();
    }
}
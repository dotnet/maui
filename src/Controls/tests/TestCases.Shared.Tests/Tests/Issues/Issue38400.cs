using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38400 : _IssuesUITest
{
    public Issue38400(TestDevice device) : base(device) { }

    public override string Issue => "TabbedPage BarBackgroundColor should work with Theme change";

    [Test]
    [Category(UITestCategories.TabbedPage)]
    public void TabbedPageBarBackgroundColorUpdatesWithThemeChange()
    {
        App.WaitForElement("DarkThemeButton1");
        App.Tap("DarkThemeButton1");
        VerifyScreenshot();
    }
}

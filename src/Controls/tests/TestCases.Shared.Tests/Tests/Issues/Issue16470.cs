using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue16470 : _IssuesUITest
{
    public Issue16470(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "TabbedPage tab titles are truncated instead of scrolling on Android";

    [Test]
    [Category(UITestCategories.TabbedPage)]
    public void TabTitlesShouldNotBeTruncated()
    {
        // Wait for the first tab content to confirm the page loaded
        App.WaitForElement("Tab1Content");

        // Verify that the tab bar shows full titles and is scrollable
        VerifyScreenshot();
    }
}

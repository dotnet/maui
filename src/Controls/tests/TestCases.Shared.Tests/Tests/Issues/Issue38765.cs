using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38765 : _IssuesUITest
{
    public Issue38765(TestDevice device) : base(device) { }

    public override string Issue => "TabbedPage: Selected tab icon loses SelectedTabColor after rebuilding Children";

    [Test]
    [Category(UITestCategories.TabbedPage)]
    public void TabbedPageSelectedTabColorTest()
    {
        App.WaitForElement("Issue38765RebuildTabs");
        App.Tap("Issue38765RebuildTabs");
        VerifyScreenshot();
    }
}

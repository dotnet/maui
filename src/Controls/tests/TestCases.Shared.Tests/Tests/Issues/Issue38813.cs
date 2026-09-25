using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38813 : _IssuesUITest
{
    public Issue38813(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "Shell flyout should not hang";

    [Test]
    [Category(UITestCategories.Shell)]
    public void ShellFlyoutShouldNotHang()
    {
        App.WaitForElement("Issue38813Button");
        App.Tap("Issue38813Button");
        App.WaitForElement("Issue38813Label");
    }
}

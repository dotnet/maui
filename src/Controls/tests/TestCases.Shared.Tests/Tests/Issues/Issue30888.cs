using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30888 : _IssuesUITest
{
    public Issue30888(TestDevice device)
        : base(device)
    { }

    public override string Issue => "Flyout page toolbar items not rendered on iOS";

    [Test]
    [Category(UITestCategories.FlyoutPage)]
    [Category(UITestCategories.ToolbarItem)]
    public void VerifyFlyoutPageToolbarItemsRenderOnIOS()
    {
        App.WaitForElement("DetailContent");
        VerifyScreenshot();
    }
}
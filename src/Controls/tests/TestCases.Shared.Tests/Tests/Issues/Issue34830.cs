using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34830 : _IssuesUITest
{
    public Issue34830(TestDevice device) : base(device)
    {
    }

    public override string Issue => "[iOS/Mac] FlyoutPage RTL FlowDirection is not working properly";

    [Test, Order(1)]
    [Category(UITestCategories.FlyoutPage)]
    public void FlyoutPageWithRTLDirection()
    {
        App.WaitForElement("ShowRightToLeft");
        App.WaitForElement("OpenRootView");
        App.Tap("OpenRootView");
        VerifyScreenshot();
    }

    [Test, Order(2)]
    [Category(UITestCategories.FlyoutPage)]
    public void FlyoutPageWithLTRDirection()
    {
        App.Tap("CloseRootView");
        App.WaitForElement("ShowLeftToRight");
        App.Tap("ShowLeftToRight");
        App.WaitForElement("OpenRootView");
        App.Tap("OpenRootView");
        VerifyScreenshot();
    }
}
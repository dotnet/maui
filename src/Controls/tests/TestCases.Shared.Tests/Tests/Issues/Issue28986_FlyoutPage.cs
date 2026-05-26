#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_FlyoutPage : _IssuesUITest
{
    public override string Issue => "Test SafeArea Flyout Page for per-edge safe area control";

    public Issue28986_FlyoutPage(TestDevice device) : base(device)
    {
    }

    [Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ToolbarExtendsAllTheWayLeftAndRight_FlyoutPage()
	{
		// 1. Test loads - verify essential elements are present
		App.WaitForElement("ContentGrid");
		App.SetOrientationLandscape();
		App.WaitForElement("ContentGrid");
#if ANDROID
		VerifyScreenshot(cropLeft: 125);
#else
		VerifyScreenshot();
#endif
	}
}
#endif

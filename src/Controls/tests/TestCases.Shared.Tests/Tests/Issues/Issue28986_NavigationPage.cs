#if IOS || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28986_NavigationPage : _IssuesUITest
{
    public override string Issue => "Test SafeArea Navigation Page for per-edge safe area control";

    public Issue28986_NavigationPage(TestDevice device) : base(device)
    {
    }

    [Test]
	[Category(UITestCategories.SafeAreaEdges)]
	public void ToolbarExtendsAllTheWayLeftAndRight_NavigationPage()
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

#if TEST_FAILS_ON_WINDOWS //The Flyout Menu closes when resizing or maximizing the window.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21105 : _IssuesUITest
{
	public Issue21105(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "iOS: Flyout Menu Backdrop Area does not Readjust on Orientation Change";

	[Test]
	[Category(UITestCategories.Shell)]
	public void ShouldUpdateFlyoutBackdropOnSizeChange()
	{
		App.WaitForElement("OpenFlyoutButton");
		App.Tap("OpenFlyoutButton");
#if ANDROID || IOS
		App.SetOrientationLandscape();
#elif MACCATALYST
		App.EnterFullScreen();
#endif
		VerifyScreenshot();
	}
}
#endif
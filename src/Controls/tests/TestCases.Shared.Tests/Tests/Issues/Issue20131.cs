using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue20131 : _IssuesUITest
{
	public Issue20131(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "Shell Flyout Header and Footer not respecting safe area on iOS in landscape mode";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutShouldRespectSafeAreaInLandscape()
	{
		App.WaitForElement("FlyoutHeader");
#if ANDROID || IOS
		App.SetOrientationLandscape();
#elif MACCATALYST
		App.EnterFullScreen();
#endif
		VerifyScreenshot();
	}
}

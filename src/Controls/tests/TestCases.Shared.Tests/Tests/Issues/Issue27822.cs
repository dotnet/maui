#if ANDROID || IOS //The test fails on Windows and MacCatalyst because the SetOrientation method, which is intended to change the device orientation, is only supported on mobile platforms iOS and Android.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27822 : _IssuesUITest
{
	public Issue27822(TestDevice device) : base(device)
	{
	}

	public override string Issue => "LinearGradientBrush in Shell FlyoutBackground not working";


	[Test]
	[Category(UITestCategories.Shell)]
	public void Issue27822Test()
	{
		App.SetOrientationLandscape();
		App.WaitForElement("navigateButton");
		App.Tap("navigateButton");
		App.WaitForElement("welcomeLabel");
		App.SetOrientationPortrait();
		VerifyScreenshot();
	}
}
#endif
#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue1418 : _IssuesUITest
{
	public Issue1418(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Shell flyout should honor Material 3 surface color instead of hardcoded background_light";

	[Test]
	[Category(UITestCategories.Shell)]
	public void FlyoutBackgroundHonorsMaterial3SurfaceColor()
	{
		App.WaitForElement("OpenFlyoutButton");
		App.Tap("OpenFlyoutButton");
		App.WaitForElement("Groceries Two");

		VerifyScreenshot();
	}
}
#endif

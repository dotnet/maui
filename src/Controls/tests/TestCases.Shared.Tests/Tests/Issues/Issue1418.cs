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

	public override string Issue => "Shell top-tab colors should follow Material 3";

	[Test]
	[Category(UITestCategories.Shell)]
	public void TopTabColorsFollowMaterial3WhenSwitchingTabs()
	{
		App.WaitForElement("TAB TWO");
		App.Tap("TAB TWO");
		App.WaitForElement("Issue1418PageTwoLabel");

		VerifyScreenshot();
	}
}
#endif

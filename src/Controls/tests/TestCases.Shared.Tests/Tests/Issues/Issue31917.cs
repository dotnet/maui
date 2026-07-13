#if ANDROID || IOS || MACCATALYST // Excludes Windows because SwipeView automation doesn't work on Windows (#14777).
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31917 : _IssuesUITest
{
	public Issue31917(TestDevice testDevice) : base(testDevice) { }

	public override string Issue => "SwipeItemView and SwipeItem background doesn't update on AppTheme change (Light/Dark)";

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemBackgroundShouldUpdateOnAppThemeChange()
	{

		App.WaitForElement("changeThemeButton");
		App.WaitForElement("TestSwipeView");

		// Reveal the swipe items by swiping left-to-right on the SwipeView
		App.SwipeLeftToRight("TestSwipeView");

		// Take screenshot before theme change
		VerifyScreenshot("Issue31917BeforeThemeChange");

		// Change theme
		App.Tap("changeThemeButton");

		// Wait for the theme change button to be interactive again before capturing the screenshot
		App.WaitForElement("changeThemeButton");

		// Take screenshot after theme change to verify background colors updated
		VerifyScreenshot("Issue31917AfterThemeChange");
	}
}
#endif

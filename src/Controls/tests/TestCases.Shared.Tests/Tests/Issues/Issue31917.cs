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

		// Reveal the swipe items by swiping left on the SwipeView
		App.SwipeLeftToRight("TestSwipeView");
		
		App.WaitForElement("TestSwipeItem");
		App.WaitForElement("TestSwipeItemView");

		// Take screenshot before theme change
		VerifyScreenshot("BeforeThemeChange");

		// Change theme
		App.Tap("changeThemeButton");

		// Wait a moment for theme change to apply
		System.Threading.Thread.Sleep(500);

		// Take screenshot after theme change to verify background colors updated
		VerifyScreenshot("AfterThemeChange");
	}
}
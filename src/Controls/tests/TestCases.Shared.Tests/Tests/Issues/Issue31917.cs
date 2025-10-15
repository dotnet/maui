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
		Exception? exception = null;
		
		App.WaitForElement("changeThemeButton");

		// Reveal the swipe items by swiping left on the SwipeView
		App.SwipeLeftToRight("TestSwipeView");

		// Take screenshot before theme change
		VerifyScreenshotOrSetException(ref exception, "Issue31917BeforeThemeChange");

		// Change theme
		App.Tap("changeThemeButton");

		// Wait a moment for theme change to apply
		Thread.Sleep(500);

		// Take screenshot after theme change to verify background colors updated
		VerifyScreenshotOrSetException(ref exception, "Issue31917AfterThemeChange");
		
		if (exception != null)
		{
			throw exception;
		}
	}
}
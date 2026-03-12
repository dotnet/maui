using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34210 : _IssuesUITest
{
	public override string Issue => "SwipeItem ignores FontImageSource rendered size on Android";

	public Issue34210(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemFontImageSourceSizeIsRespected()
	{
		// Swipe left to reveal the SwipeItem (causes icon to be rendered)
		App.WaitForElement("SwipeContent");
		App.SwipeLeftToRight("SwipeContent");

		// Wait for the swipe animation to complete
		System.Threading.Thread.Sleep(500);

		// Take a screenshot to visually verify the icon size
		// FontImageSource.Size=20 should render as ~20-22dp icon
		// Before the fix, Android renders it at ~50dp (containerHeight/2 = 100/2)
		VerifyScreenshot("SwipeItem_FontImageSource_Revealed");

		// The screenshot baseline will show the bug: icon is much larger than 20dp
		// When the fix is applied, the icon should be the correct 20dp size
	}
}

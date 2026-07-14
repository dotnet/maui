using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36154 : _IssuesUITest
{
	public Issue36154(TestDevice testDevice) : base(testDevice)
	{
	}

	public override string Issue => "WebView inside SwipeView no longer responds to swipe gestures on Android";

	// Verify that swiping left on the WebView reveals the Right swipe items
	[Test]
	[Category(UITestCategories.SwipeView)]
	public void Issue36154SwipeViewShouldRevealItems()
	{
#if WINDOWS
		// WinUI cannot locate SwipeView through WebDriver, so use its WebView child for the gesture coordinates.
		const string swipeTarget = "TheWebView";
#else
		const string swipeTarget = "TheSwipeView";
#endif
		var rect = App.WaitForElement(swipeTarget).GetRect();
		var centerX = rect.X + rect.Width / 2;
		var centerY = rect.Y + rect.Height / 2;

		// Swipe left (finger moves left) → reveals RightItems
		App.DragCoordinates(centerX, centerY, centerX - 200, centerY);

		Assert.That(App.WaitForElement("ResultLabel").GetText(), Is.EqualTo("RIGHT invoked!"));
	}
}

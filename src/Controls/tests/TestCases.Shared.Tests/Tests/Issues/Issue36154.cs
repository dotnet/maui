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
		App.DragCoordinates(centerX, centerY, centerX - 300, centerY);

		// WaitForTextToBePresentInElement returns false (rather than throwing) on timeout, so assert on
		// it: otherwise the test would pass even if the SwipeView invoke callback never updated the label.
		Assert.That(
			App.WaitForTextToBePresentInElement("ResultLabel", "RIGHT invoked!"),
			Is.True,
			"Timed out waiting for ResultLabel to display 'RIGHT invoked!' after the swipe.");
	}
}

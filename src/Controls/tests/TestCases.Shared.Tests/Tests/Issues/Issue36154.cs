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
	public void SwipeLeftRevealsRightItems()
	{
		var rect = App.WaitForElement("TheWebView").GetRect();
		var centerX = rect.X + rect.Width / 2;
		var centerY = rect.Y + rect.Height / 2;

		// Swipe left (finger moves left) → reveals RightItems
		App.DragCoordinates(centerX, centerY, centerX - 200, centerY);

		App.WaitForElement("RightItem");
	}

	// Verify that swiping right on the WebView reveals the Left swipe items
	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeRightRevealsLeftItems()
	{
		var rect = App.WaitForElement("TheWebView").GetRect();
		var centerX = rect.X + rect.Width / 2;
		var centerY = rect.Y + rect.Height / 2;

		// Swipe right (finger moves right) → reveals LeftItems
		App.DragCoordinates(centerX, centerY, centerX + 200, centerY);

		App.WaitForElement("LeftItem");
	}

	// Verify that the WebView can still scroll vertically inside the SwipeView
	[Test]
	[Category(UITestCategories.SwipeView)]
	public void WebViewCanScrollVertically()
	{
		var rect = App.WaitForElement("TheWebView").GetRect();
		var centerX = rect.X + rect.Width / 2;
		var startY = rect.Y + rect.Height * 3 / 4;
		var endY = rect.Y + rect.Height / 4;

		// Scroll up (finger moves up) inside the WebView — should NOT trigger swipe items
		App.DragCoordinates(centerX, startY, centerX, endY);

		// SwipeView should NOT have triggered — result label stays at default
		var resultText = App.WaitForElement("ResultLabel").GetText();
		Assert.That(resultText, Is.EqualTo("Swipe result will appear here"),
			"WebView vertical scroll should not trigger SwipeView items");
	}
}

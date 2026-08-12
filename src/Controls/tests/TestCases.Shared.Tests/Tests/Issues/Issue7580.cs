using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue7580 : _IssuesUITest
{
	public override string Issue => "Changing visibility on a SwipeItem causes multiple items to be executed";

	public Issue7580(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.SwipeView)]
	public void SwipeItemVisibilityChangeShouldNotInvokeTwice()
	{
		App.WaitForElement("SwipeTarget");
		App.WaitForElement("InvokeCountLabel");

		var initialCount = App.FindElement("InvokeCountLabel").GetText();
		Assert.That(initialCount, Is.EqualTo("InvokeCount: 0"));

		var contentRect = App.WaitForElement("SwipeContent").GetRect();
		var centerY = contentRect.Y + contentRect.Height / 2;
		// Here contentRect.X is negative value on mac, so we need to make sure we don't start dragging from a negative X coordinate
		var startX = Math.Max(contentRect.X + 20, 0);
		var endX = contentRect.X + contentRect.Width - 5;

		App.DragCoordinates(startX, centerY, endX, centerY);

#if WINDOWS
		App.WaitForElement("InvokeCount: 1");
#else
		var afterFirstSwipe = App.FindElement("InvokeCountLabel").GetText();
		Assert.That(afterFirstSwipe, Is.EqualTo("InvokeCount: 1"),
			"SwipeItem command should be invoked exactly once per swipe");
#endif

		var status = App.FindElement("StatusLabel").GetText();
		Assert.That(status, Is.EqualTo("Status: IsCompleted=False"),
			"IsCompleted should have toggled once from True to False");
	}
}
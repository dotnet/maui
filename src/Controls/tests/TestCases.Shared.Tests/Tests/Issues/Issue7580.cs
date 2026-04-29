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

		var rect = App.WaitForElement("SwipeTarget").GetRect();
		var centerY = rect.Y + rect.Height / 2;
		var startX = rect.X + 20;
		var endX = startX + 600;

		App.DragCoordinates(startX, centerY, endX, centerY);

		var afterFirstSwipe = App.FindElement("InvokeCountLabel").GetText();
		Assert.That(afterFirstSwipe, Is.EqualTo("InvokeCount: 1"),
			"SwipeItem command should be invoked exactly once per swipe");

		var status = App.FindElement("StatusLabel").GetText();
		Assert.That(status, Is.EqualTo("Status: IsCompleted=False"),
			"IsCompleted should have toggled once from True to False");
	}
}
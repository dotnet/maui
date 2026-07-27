#if TEST_FAILS_ON_CATALYST //The test fails on Mac because PinchToZoomIn does not work.                     
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24252 : _IssuesUITest
{
	public override string Issue => "Overlapping gesture recognizers should only fire on the topmost child view on Windows";

	public Issue24252(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Gestures)]
	public void OverlappingGesturesShouldOnlyFireChild()
	{
		// Pan: drag on the child box
		App.WaitForElement("PanStatusLabel");
		var panChild = App.WaitForElement("PanChildBox");
		var panRect = panChild.GetRect();
		App.DragCoordinates(panRect.CenterX(), panRect.CenterY(), panRect.CenterX() + 50, panRect.CenterY() + 50);

		Assert.That(App.WaitForElement("PanStatusLabel").GetText(), Is.EqualTo("Child triggered"),
		 "Only the child PanGestureRecognizer should fire when dragging the child view.");

		//// Swipe: swipe right on the child box
		App.WaitForElement("SwipeStatusLabel");
		App.SwipeLeftToRight("SwipeChildBox", swipePercentage: 1.5, swipeSpeed: 100);

		Assert.That(App.WaitForElement("SwipeStatusLabel").GetText(), Is.EqualTo("Child triggered"),
		 "Only the child SwipeGestureRecognizer should fire when swiping on the child view.");

		// Pinch: pinch on the child box
		App.WaitForElement("PinchStatusLabel");
		App.PinchToZoomIn("PinchChildBox");

		Assert.That(App.WaitForElement("PinchStatusLabel").GetText(), Is.EqualTo("Child triggered"),
		 "Only the child PinchGestureRecognizer should fire when pinching on the child view.");
	}
}
#endif
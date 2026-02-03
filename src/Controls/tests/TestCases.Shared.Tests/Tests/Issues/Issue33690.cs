using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33690 : _IssuesUITest
{
	public Issue33690(TestDevice device) : base(device) { }

	public override string Issue => "PointerGestureRecognizer does not fire off PointerMove event on Android";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void PointerMovedEventShouldFireOnAndroid()
	{
		App.WaitForElement("TestImage");
		var pressedLabel = App.FindElement("PointerPressedLabel");
		var movedLabel = App.FindElement("PointerMovedLabel");
		var releasedLabel = App.FindElement("PointerReleasedLabel");
		Assert.That(pressedLabel.GetText(), Is.EqualTo("Pointer Pressed: 0"));
		Assert.That(movedLabel.GetText(), Is.EqualTo("Pointer Moved: 0"));
		Assert.That(releasedLabel.GetText(), Is.EqualTo("Pointer Released: 0"));
		var imageRect = App.WaitForElement("TestImage").GetRect();
		var startX = imageRect.X + (imageRect.Width / 4);
		var startY = imageRect.Y + (imageRect.Height / 4);
		var endX = imageRect.X + (imageRect.Width * 3 / 4);
		var endY = imageRect.Y + (imageRect.Height * 3 / 4);

		App.DragCoordinates(startX, startY, endX, endY);
		pressedLabel = App.FindElement("PointerPressedLabel");
		movedLabel = App.FindElement("PointerMovedLabel");
		releasedLabel = App.FindElement("PointerReleasedLabel");
		Assert.That(pressedLabel.GetText(), Does.Not.EqualTo("Pointer Pressed: 0"),
			"PointerPressed event should have fired");
		Assert.That(movedLabel.GetText(), Does.Not.EqualTo("Pointer Moved: 0"),
			"PointerMoved event should have fired");
		Assert.That(releasedLabel.GetText(), Does.Not.EqualTo("Pointer Released: 0"),
			"PointerReleased event should have fired");
	}
}

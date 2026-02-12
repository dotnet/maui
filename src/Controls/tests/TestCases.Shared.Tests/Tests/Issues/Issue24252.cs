using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24252 : _IssuesUITest
{
	public override string Issue => "PanGestureRecognizer behaves differently on Windows to other platforms";

	public Issue24252(TestDevice device) : base(device) { }

	[Test]
	[Category(UITestCategories.Gestures)]
	public void OverlappingPanGesturesShouldOnlyFireChild()
	{
		App.WaitForElement("StatusLabel");
		var childImage = App.WaitForElement("ChildImage");
		var rect = childImage.GetRect();

		// Drag on the child
		App.DragCoordinates(rect.CenterX(), rect.CenterY(), rect.CenterX() + 50, rect.CenterY() + 50);

		var statusText = App.WaitForElement("StatusLabel").GetText();

		Assert.That(statusText, Is.EqualTo("Child triggered"),
		 "Only the child PanGestureRecognizer should fire when dragging the child view.");
	}
}
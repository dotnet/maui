using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15280 : _IssuesUITest
{
	public Issue15280(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Swipe gestures attached to rotated controls are rotated on Android";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void SwipeGesturesOnRotatedControlsShouldWorkCorrectly()
	{
		App.WaitForElement("RotatedImage");

		App.SwipeLeftToRight("RotatedImage");
		Assert.That(App.WaitForElement("DirectionLabel").GetText(), Is.EqualTo("Swiped: RIGHT"));

		App.SwipeRightToLeft("RotatedImage");
		Assert.That(App.WaitForElement("DirectionLabel").GetText(), Is.EqualTo("Swiped: LEFT"));
	}
}
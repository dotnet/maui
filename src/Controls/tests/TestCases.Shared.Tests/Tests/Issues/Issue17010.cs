using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue17010 : _IssuesUITest
{
	public Issue17010(TestDevice device) : base(device)
	{
	}

	public override string Issue => "Unable to Update iOS SwipeGesture Direction at Runtime";

	[Test]
	[Category(UITestCategories.Gestures)]
	public void SwipeGestureDirectionShouldChangeAtRuntime()
	{
		Assert.That(App.WaitForElement("DirectionLabel").GetText(), Is.EqualTo("Direction: Right"));
		
		App.SwipeLeftToRight("DirectionLabel");
		Assert.That(App.WaitForElement("DirectionLabel").GetText(), Is.EqualTo("Direction: Left"));

		App.SwipeRightToLeft("DirectionLabel");
		Assert.That(App.WaitForElement("DirectionLabel").GetText(), Is.EqualTo("Direction: Right"));
	}
}
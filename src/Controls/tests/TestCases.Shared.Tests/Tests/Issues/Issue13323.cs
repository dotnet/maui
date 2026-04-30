using Microsoft.Maui.TestCases.Tests;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppUITests.Issues;

public class Issue13323 : _IssuesUITest
{
	public Issue13323(TestDevice device) : base(device) { }

	public override string Issue => "CarouselView on Android does not work if HorizontalTextAlignment in Entry is not Start";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselView_EntryTap_DoesNotChangePosition()
	{
		App.WaitForElement("CarouselView13323", timeout: TimeSpan.FromSeconds(30));

		// Swipe on the Label area (not the Entry) to avoid gesture interception
		App.SwipeRightToLeft("SwipeArea");
		App.SwipeRightToLeft("SwipeArea");

		App.WaitForElement("Position:2", timeout: TimeSpan.FromSeconds(15));

		App.Tap("CenterEntry");

		App.WaitForElement("Position:2", timeout: TimeSpan.FromSeconds(15));

		var positionAfter = App.FindElement("PositionLabel").GetText();
		Assert.That(positionAfter, Is.EqualTo("Position:2"), $"CarouselView jumped after tapping Center-aligned Entry: {positionAfter}");

		App.DismissKeyboard();
		App.WaitForKeyboardToHide();
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselView_Loop_EntryTap_DoesNotChangePosition()
	{
		App.WaitForElement("LoopCarouselView13323", timeout: TimeSpan.FromSeconds(30));

		App.SwipeRightToLeft("LoopSwipeArea");
		App.SwipeRightToLeft("LoopSwipeArea");

		App.WaitForElement("LoopPosition:2", timeout: TimeSpan.FromSeconds(15));

		App.Tap("LoopCenterEntry");

		App.WaitForElement("LoopPosition:2", timeout: TimeSpan.FromSeconds(15));

		var positionAfter = App.FindElement("LoopPositionLabel").GetText();
		Assert.That(positionAfter, Is.EqualTo("LoopPosition:2"), $"CarouselView (Loop=true) jumped after tapping Center-aligned Entry: {positionAfter}");

		App.DismissKeyboard();
		App.WaitForKeyboardToHide();
	}
}

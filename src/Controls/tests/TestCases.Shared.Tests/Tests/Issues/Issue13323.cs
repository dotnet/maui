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
		App.WaitForElement("CarouselView13323");

		App.SwipeRightToLeft("CarouselView13323");
		App.SwipeRightToLeft("CarouselView13323");

		App.WaitForElement("Position:2");

		App.Tap("CenterEntry");

		// Position must remain at 2 after tapping Center-aligned Entry
		App.WaitForElement("Position:2");

		var positionAfter = App.FindElement("PositionLabel").GetText();
		Assert.That(positionAfter, Is.EqualTo("Position:2"), $"CarouselView jumped after tapping Center-aligned Entry: {positionAfter}");

		App.DismissKeyboard();
		App.WaitForKeyboardToHide();
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselView_Loop_EntryTap_DoesNotChangePosition()
	{
		App.WaitForElement("LoopCarouselView13323");

		App.SwipeRightToLeft("LoopCarouselView13323");
		App.SwipeRightToLeft("LoopCarouselView13323");

		App.WaitForElement("LoopPosition:2");

		App.Tap("LoopCenterEntry");

		// Position must remain at 2 after tapping Center-aligned Entry with Loop=true
		App.WaitForElement("LoopPosition:2");

		var positionAfter = App.FindElement("LoopPositionLabel").GetText();
		Assert.That(positionAfter, Is.EqualTo("LoopPosition:2"), $"CarouselView (Loop=true) jumped after tapping Center-aligned Entry: {positionAfter}");
	}
}

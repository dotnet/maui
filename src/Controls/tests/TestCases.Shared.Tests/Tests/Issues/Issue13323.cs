#if TEST_FAILS_ON_WINDOWS // Related issue: https://github.com/dotnet/maui/issues/29412
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
		NavigateToSecondItem("CarouselView13323", "GoToItem2");
		App.WaitForTextToBePresentInElement("PositionLabel", "Position:2");
		App.WaitForElement("CenterEntry_2");

		App.Tap("CenterEntry_2");

		Assert.That(App.FindElement("PositionLabel").GetText(), Is.EqualTo("Position:2"),
			"CarouselView jumped after tapping Center-aligned Entry.");

		App.DismissKeyboard();
#if ANDROID
		App.WaitForKeyboardToHide();
#endif
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void CarouselView_Loop_EntryTap_DoesNotChangePosition()
	{
		NavigateToSecondItem("LoopCarouselView13323", "LoopGoToItem2");
		App.WaitForTextToBePresentInElement("LoopPositionLabel", "LoopPosition:2");
		App.WaitForElement("LoopCenterEntry_2");

		App.Tap("LoopCenterEntry_2");

		Assert.That(App.FindElement("LoopPositionLabel").GetText(), Is.EqualTo("LoopPosition:2"),
			"CarouselView (Loop=true) jumped after tapping Center-aligned Entry.");

		App.DismissKeyboard();
#if ANDROID
		App.WaitForKeyboardToHide();
#endif
	}

	// Navigates the given CarouselView to item 2. On iOS the classic CarouselView does not reliably
	// respond to the programmatic ScrollTo in CI, so we drive it with real swipe gestures (the same
	// approach used by Issue29261). On the other platforms the "Go to Item 2" button is reliable.
	void NavigateToSecondItem(string carouselId, string goToButtonId)
	{
		App.WaitForElement(carouselId);
#if IOS
		App.ScrollRight(carouselId, ScrollStrategy.Gesture, 0.9, 500);
		App.ScrollRight(carouselId, ScrollStrategy.Gesture, 0.9, 500);
#else
		App.WaitForElement(goToButtonId);
		App.Tap(goToButtonId);
#endif
	}
}
#endif
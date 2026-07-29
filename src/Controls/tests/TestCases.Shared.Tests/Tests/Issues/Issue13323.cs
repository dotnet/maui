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

		App.Tap("GoToItem2");
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
		App.WaitForElement("LoopCarouselView13323", timeout: TimeSpan.FromSeconds(30));

		App.Tap("LoopGoToItem2");
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
}

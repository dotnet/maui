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
		App.WaitForElement("CarouselView13323", timeout: TimeSpan.FromSeconds(30));
		App.WaitForElement("PositionLabel", timeout: TimeSpan.FromSeconds(15));

		App.Tap("GoToItem2");
		App.WaitForElement("CenterEntry_2", timeout: TimeSpan.FromSeconds(10));

		var positionBefore = App.FindElement("PositionLabel").GetText();
		Assert.That(positionBefore, Is.EqualTo("Position:2"), $"CarouselView did not reach Position:2 before tapping Entry. Actual: {positionBefore}");

		App.Tap("CenterEntry_2");

		var positionAfter = App.FindElement("PositionLabel").GetText();
		Assert.That(positionAfter, Is.EqualTo("Position:2"), $"CarouselView jumped after tapping Center-aligned Entry. Before: {positionBefore}, After: {positionAfter}");

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
		App.WaitForElement("LoopPositionLabel", timeout: TimeSpan.FromSeconds(15));

		App.Tap("LoopGoToItem2");
		App.WaitForElement("LoopCenterEntry_2", timeout: TimeSpan.FromSeconds(10));

		var positionBefore = App.FindElement("LoopPositionLabel").GetText();
		Assert.That(positionBefore, Is.EqualTo("LoopPosition:2"), $"Loop CarouselView did not reach LoopPosition:2 before tapping Entry. Actual: {positionBefore}");

		App.Tap("LoopCenterEntry_2");

		var positionAfter = App.FindElement("LoopPositionLabel").GetText();
		Assert.That(positionAfter, Is.EqualTo("LoopPosition:2"), $"CarouselView (Loop=true) jumped after tapping Center-aligned Entry. Before: {positionBefore}, After: {positionAfter}");

		App.DismissKeyboard();
#if ANDROID
		App.WaitForKeyboardToHide();
#endif
	}
}
#endif

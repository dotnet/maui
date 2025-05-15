#if TEST_FAILS_ON_CATALYST				// On Catalyst, Swipe actions not supported in Appium.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue29411 : _IssuesUITest
{
	public Issue29411(TestDevice device) : base(device) { }

	public override string Issue => "[Android] CarouselView.Loop = false causes crash on Android when changed at runtime";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerifyLoopChangeAtRuntimeShouldNotCrash()
	{
		App.WaitForElement("CarouselView");
		App.Tap("Button");
		var text = App.FindElement("Label").GetText();
		Assert.That(text, Is.EqualTo("Item 1"));
		App.SwipeRightToLeft("CarouselView");
		App.WaitForElement("CarouselView");
	}
}

#endif
#if !MACCATALYST //Appium Swipe is not on MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Tests.Issues;

public class Issue29411 : _IssuesUITest
{
	public Issue29411(TestDevice device) : base(device)
	{
	}

	public override string Issue => "[Android] CarouselView.Loop = false causes crash on Android when changed at runtime";

    [Test]
	[Category(UITestCategories.CarouselView)]
	public void ChangingLoopToFalseShouldNotCrash()
	{
		App.WaitForElement("MauiButton");
		App.Tap("MauiButton");
		App.SwipeRightToLeft("MauiCarouselView");
        App.WaitForElement("MauiCarouselView");
	}
}
#endif
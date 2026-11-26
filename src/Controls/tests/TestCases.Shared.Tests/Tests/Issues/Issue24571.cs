#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST // iOS/Mac: ItemsViewDelegator2 compares the visible index path, which is a loop position for a looping CarouselView, with ItemCount, so the event isn't raised; this fix is Android only
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue24571 : _IssuesUITest
{
	public Issue24571(TestDevice device) : base(device) { }

	public override string Issue => "RemainingItemsThresholdReached is not raised for a looping CarouselView";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void RemainingItemsThresholdReachedFiresForLoopingCarouselView()
	{
		App.WaitForElement("CarouselView");
		App.Tap("ScrollToLastButton");
		Assert.That(App.WaitForTextEqualToElement("StatusLabel", "Threshold reached"), Is.True);
	}
}
#endif

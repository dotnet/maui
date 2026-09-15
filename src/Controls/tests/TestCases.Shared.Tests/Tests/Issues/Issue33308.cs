#if TEST_FAILS_ON_WINDOWS // Windows does not consistently capture the same item on scroll down, so this test may be flaky and is skipped on Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue33308 : _IssuesUITest
{
	public override string Issue => "CarouselView vertical MandatorySingle snap points should settle to exactly one item per swipe on iOS";

	public Issue33308(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void VerticalCarouselMandatorySingleSnapAdvancesOneCard()
	{
		App.WaitForElement("CardsCarousel");

		App.ScrollDown("CardsCarousel", ScrollStrategy.Gesture, 0.99, swipeSpeed: 500, withInertia: true);

		App.WaitForElement("Card 2");

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}
#endif

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
		App.WaitForElement("Card 1");

		App.ScrollDown("Card 1", ScrollStrategy.Gesture, 0.99, swipeSpeed: 50, withInertia: true);

		VerifyScreenshot(retryTimeout: TimeSpan.FromSeconds(2));
	}
}

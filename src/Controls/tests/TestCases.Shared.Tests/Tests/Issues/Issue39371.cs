using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue39371 : _IssuesUITest
{
	public Issue39371(TestDevice device)
		: base(device)
	{
	}

	public override string Issue =>
		"[iOS] Vertical CarouselView does not update Position or raise PositionChanged";

	[Test]
	[Category(UITestCategories.CarouselView)]
	public void ScrollToUpdatesPositionAndRaisesPositionChanged()
	{
		App.WaitForElement("VerticalCarouselView");
		App.Tap("ScrollToNextButton");

		App.RetryAssert(() =>
		{
			Assert.That(App.FindElement("PositionLabel").GetText(), Is.EqualTo("Position: 1"));
			Assert.That(App.FindElement("PositionChangedLabel").GetText(), Is.EqualTo("PositionChanged: 1"));
		});
	}
}

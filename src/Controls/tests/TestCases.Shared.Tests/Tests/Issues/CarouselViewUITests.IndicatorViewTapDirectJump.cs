using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class IndicatorViewTapDirectJump : _IssuesUITest
	{
		const string CarouselId = "jumpCarousel";
		const string IndicatorId = "jumpIndicator";
		const string PositionLabelId = "jumpPositionLabel";

		public IndicatorViewTapDirectJump(TestDevice device)
			: base(device) { }

		public override string Issue =>
			"IndicatorView dot tap only advances by plus/minus 1 instead of jumping directly to the tapped dot";

		// Repros https://github.com/dotnet/maui/issues/27007.
		[Test]
		[Category(UITestCategories.IndicatorView)]
		public void TappingLastDotJumpsDirectlyToLastItem()
		{
			App.WaitForElement(CarouselId, timeout: TimeSpan.FromSeconds(30));
			App.WaitForElement(IndicatorId);

			// The carousel has 5 items. Tapping the rightmost dot should jump
			// directly to Item 4. On iOS/Catalyst, UIPageControl only interprets
			// taps as "left half = back 1" / "right half = forward 1", so the
			// carousel only advances to Item 1 — bug #27007.
			var indicatorRect = App.FindElement(IndicatorId).GetRect();
			var lastDotX = indicatorRect.X + indicatorRect.Width - 10;
			var centerY = indicatorRect.Y + indicatorRect.Height / 2;
			App.TapCoordinates(lastDotX, centerY);

			Thread.Sleep(800);

			var posText = App.FindElement(PositionLabelId).GetText();
			Assert.That(
				posText,
				Is.EqualTo("Pos:4"),
				"Tapping the last indicator dot must jump the CarouselView directly to the last item (Pos:4). On iOS/Catalyst, the current UIPageControl-based IndicatorView only advances by 1 (Pos:1), which is the bug under test."
			);
		}
	}
}

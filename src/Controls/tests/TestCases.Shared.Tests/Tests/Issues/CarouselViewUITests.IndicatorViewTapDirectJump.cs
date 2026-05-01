#if TEST_FAILS_ON_IOS && TEST_FAILS_ON_CATALYST && TEST_FAILS_ON_WINDOWS // iOS/Catalyst: https://github.com/dotnet/maui/issues/27007 (the bug under test). Windows: IndicatorView UI automation not working — see Issue31063.
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
		const int TotalItems = 5;
		const int LastDotIndex = TotalItems - 1;

		public IndicatorViewTapDirectJump(TestDevice device)
			: base(device) { }

		public override string Issue =>
			"IndicatorView dot tap only advances by plus/minus 1 instead of jumping directly to the tapped dot";

		// Repros https://github.com/dotnet/maui/issues/27007.
		[Test]
		[Category(UITestCategories.CarouselView)]
		public void TappingLastDotJumpsDirectlyToLastItem()
		{
			App.WaitForElement(CarouselId, timeout: TimeSpan.FromSeconds(30));

			// Use the WaitForElement result directly to avoid a stale-rect race
			// between the implicit wait and the subsequent FindElement.
			var indicatorRect = App.WaitForElement(IndicatorId).GetRect();

			// The dots are laid out evenly across the IndicatorView's bounds. Tap the
			// horizontal center of the slot for the last dot using a proportional
			// formula so the test does not depend on IndicatorSize/padding constants.
			// On iOS/Catalyst, UIPageControl only interprets taps as "left half = back 1"
			// / "right half = forward 1", so the carousel only advances to Item 1 — bug #27007.
			var lastDotX = indicatorRect.X + (LastDotIndex + 0.5f) * indicatorRect.Width / TotalItems;
			var centerY = indicatorRect.Y + indicatorRect.Height / 2;
			App.TapCoordinates(lastDotX, centerY);

			// Replace fragile Thread.Sleep with a deterministic text-presence wait
			// so the failure mode on regression is a clear timeout rather than a flaky race.
			Assert.That(
				App.WaitForTextToBePresentInElement(PositionLabelId, "Pos:4", TimeSpan.FromSeconds(5)),
				Is.True,
				"PositionLabel did not update to 'Pos:4' within 5s after tapping the last indicator dot."
			);

			var posText = App.FindElement(PositionLabelId).GetText();
			Assert.That(
				posText,
				Is.EqualTo("Pos:4"),
				"Tapping the last indicator dot must jump the CarouselView directly to the last item (Pos:4). On iOS/Catalyst, the current UIPageControl-based IndicatorView only advances by 1 (Pos:1), which is the bug under test."
			);
		}
	}
}
#endif

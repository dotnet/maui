#if IOS || MACCATALYST // This regression is specific to the iOS/MacCatalyst CollectionView2 handler.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue37427 : _IssuesUITest
{
	public Issue37427(TestDevice device)
		: base(device)
	{
	}

	public override string Issue => "CollectionView item content renders with zero width on iOS";

	[Test]
	[Category(UITestCategories.CollectionView)]
	public void DynamicallyAddedContentRendersAfterCellRealization()
	{
		App.WaitForElement("37427CollectionView");

		// Scroll to a card that requires cell realization beyond the initial viewport.
		App.ScrollTo("37427Card10");
		AssertPreviewColorsRender("37427Card10", "after downward scroll");

		// Jump back to the first card so Card 10 leaves the viewport and its cell can be reused.
		App.ScrollTo("37427Card1", down: false);
		AssertPreviewColorsRender("37427Card1", "after upward recycle");

		// Scroll down again to validate that downward recycling also works.
		App.ScrollTo("37427Card10");
		AssertPreviewColorsRender("37427Card10", "after downward recycle");
	}

	void AssertPreviewColorsRender(string cardId, string phase)
	{
		var cardBounds = App.WaitForElement(cardId).GetRect();

		var previewBounds = App.WaitForElement($"{cardId}PreviewColors").GetRect();
		var colorWidths = new double[5];
		Assert.Multiple(() =>
		{
			for (var colorIndex = 0; colorIndex < 5; colorIndex++)
			{
				var colorBounds = App.WaitForElement($"{cardId}Color{colorIndex}").GetRect();
				Assert.That(colorBounds.Width, Is.GreaterThan(0), $"{phase}: color {colorIndex} should have a non-zero width.");
				Assert.That(colorBounds.Height, Is.GreaterThan(0), $"{phase}: color {colorIndex} should have a non-zero height.");
				colorWidths[colorIndex] = colorBounds.Width;
			}
		});

		// All color boxes use Star columns and should fill the preview grid.
		var averageWidth = colorWidths.Average();
		var totalWidth = colorWidths.Sum();
		Assert.Multiple(() =>
		{
			Assert.That(previewBounds.Width, Is.EqualTo(cardBounds.Width - 24).Within(8),
				$"{phase}: preview width ({previewBounds.Width:F1}) should fill the card ({cardBounds.Width:F1}) inside its horizontal margin.");
			Assert.That(totalWidth, Is.EqualTo(previewBounds.Width).Within(2),
				$"{phase}: color widths ({totalWidth:F1}) should fill the preview grid ({previewBounds.Width:F1}).");

			for (var colorIndex = 0; colorIndex < colorWidths.Length; colorIndex++)
			{
				Assert.That(colorWidths[colorIndex], Is.EqualTo(averageWidth).Within(averageWidth * 0.15),
					$"{phase}: color {colorIndex} width ({colorWidths[colorIndex]:F1}) should be proportional to average ({averageWidth:F1}).");
			}
		});
	}
}
#endif

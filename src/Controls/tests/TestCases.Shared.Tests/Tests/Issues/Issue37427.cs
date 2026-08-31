#if IOS || MACCATALYST
using ImageMagick;
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
		var collection = App.WaitForElement("37427CollectionView").GetRect();
		App.ScrollDown("37427CollectionView", ScrollStrategy.Gesture, 0.8, 500, withInertia: false);

		var window = App.FindElement(AppiumQuery.ByXPath("//XCUIElementTypeWindow")).GetRect();
		var previews = Enumerable.Range(1, 10)
			.Select(card => (card, element: App.FindElements($"37427Card{card}PreviewColors").FirstOrDefault()))
			.Where(preview => preview.element is not null)
			.Select(preview => (preview.card, rect: preview.element!.GetRect()))
			.Where(preview =>
				preview.rect.Y >= collection.Y &&
				preview.rect.Y + preview.rect.Height <= collection.Y + collection.Height)
			.ToArray();

		Assert.That(previews, Has.Length.GreaterThanOrEqualTo(3),
			"Expected at least three fully visible preview bars after the scroll gesture.");
		Assert.That(previews.Select(preview => preview.card), Has.Some.GreaterThanOrEqualTo(7),
			"Expected at least one card realized beyond the initial viewport.");

		using var screenshot = new MagickImage(App.Screenshot());
		// iOS screenshots use native pixels while Appium rectangles use points.
		// Mac2 returns both in screen coordinates.
#if IOS
		var scale = (double)screenshot.Width / window.Width;
#else
		const double scale = 1;
#endif
		using var pixels = screenshot.GetPixels();

		foreach (var (card, preview) in previews)
		{
			for (var column = 0; column < 5; column++)
			{
				var x = (int)((preview.X + preview.Width * (column + 0.5) / 5) * scale);
				var y = (int)((preview.Y + preview.Height / 2) * scale);
				Assert.That(x, Is.InRange(0, (int)screenshot.Width - 1));
				Assert.That(y, Is.InRange(0, (int)screenshot.Height - 1));

				var color = pixels.GetPixel(x, y).ToColor();
				Assert.That(color, Is.Not.Null);

				var channelRange = Math.Max(color!.R, Math.Max(color.G, color.B)) -
					Math.Min(color.R, Math.Min(color.G, color.B));

				Assert.That(channelRange, Is.GreaterThan(100),
					$"Card {card}, preview column {column} should contain a rendered color, but sampled {color} at ({x},{y}); " +
					$"preview={preview}, window={window}, image={screenshot.Width}x{screenshot.Height}, scale={scale}.");

				var colorBounds = App.WaitForElement($"37427Card{card}Color{column}").GetRect();
				Assert.That(colorBounds.Width, Is.GreaterThan(0));
				Assert.That(colorBounds.Height, Is.GreaterThan(0));
			}
		}
	}
}
#endif

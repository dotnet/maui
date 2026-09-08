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

		foreach (var (card, _) in previews)
		{
			for (var column = 0; column < 5; column++)
			{
				var colorBounds = App.WaitForElement($"37427Card{card}Color{column}").GetRect();
				Assert.That(colorBounds.Width, Is.GreaterThan(0),
					$"Card {card}, preview column {column} should have positive width.");
				Assert.That(colorBounds.Height, Is.GreaterThan(0),
					$"Card {card}, preview column {column} should have positive height.");
			}
		}

		previews = previews
			.Select(preview => (preview.card, rect: App.WaitForElement($"37427Card{preview.card}PreviewColors").GetRect()))
			.ToArray();

		using var screenshot = new MagickImage(App.Screenshot());
		double scaleX;
		double scaleY;
#if MACCATALYST
		// The Mac2 driver captures the desktop and reports absolute desktop coordinates.
		scaleX = 1;
		scaleY = 1;
		Assert.That(
			window.X >= 0 &&
			window.Y >= 0 &&
			window.X + window.Width <= screenshot.Width &&
			window.Y + window.Height <= screenshot.Height,
			Is.True,
			$"Expected the Appium window to fit inside the desktop screenshot, but got " +
			$"window={window}, image={screenshot.Width}x{screenshot.Height}.");
#else
		// iOS screenshots use native pixels while Appium rectangles use full-screen points.
		Assert.That(window.X, Is.Zero);
		Assert.That(window.Y, Is.Zero);
		scaleX = (double)screenshot.Width / window.Width;
		scaleY = (double)screenshot.Height / window.Height;
		Assert.That(scaleX, Is.EqualTo(scaleY).Within(0.05),
			$"Expected the Appium window to use the screenshot's full-screen coordinate space, but got " +
			$"window={window}, image={screenshot.Width}x{screenshot.Height}, scale={scaleX}x{scaleY}.");
#endif
		using var pixels = screenshot.GetPixels();

		foreach (var (card, preview) in previews)
		{
			Assert.That(
				preview.X >= window.X &&
				preview.Y >= window.Y &&
				preview.X + preview.Width <= window.X + window.Width &&
				preview.Y + preview.Height <= window.Y + window.Height,
				Is.True,
				$"Card {card} preview {preview} should be contained in the Appium window {window}.");

			for (var column = 0; column < 5; column++)
			{
				var x = (int)((preview.X + preview.Width * (column + 0.5) / 5) * scaleX);
				var y = (int)((preview.Y + preview.Height / 2) * scaleY);
				Assert.That(x, Is.InRange(0, (int)screenshot.Width - 1),
					$"Card {card}, preview column {column} mapped outside screenshot width; " +
					$"preview={preview}, window={window}, image={screenshot.Width}x{screenshot.Height}, scale={scaleX}x{scaleY}.");
				Assert.That(y, Is.InRange(0, (int)screenshot.Height - 1),
					$"Card {card}, preview column {column} mapped outside screenshot height; " +
					$"preview={preview}, window={window}, image={screenshot.Width}x{screenshot.Height}, scale={scaleX}x{scaleY}.");

				var color = pixels.GetPixel(x, y).ToColor();
				Assert.That(color, Is.Not.Null);

				var red = (int)color!.R;
				var green = (int)color.G;
				var blue = (int)color.B;
				var isExpectedColor = column switch
				{
					0 => red - green > 40 && red - blue > 40,
					1 => red - green > 40 && green - blue > 40,
					2 => red - green > 15 && green - blue > 40,
					3 => green - red > 20 && green - blue > 20,
					4 => blue - red > 40 && blue - green > 40,
					_ => false,
				};

				Assert.That(isExpectedColor, Is.True,
					$"Card {card}, preview column {column} should contain its expected rendered color, but sampled {color} at ({x},{y}); " +
					$"preview={preview}, window={window}, image={screenshot.Width}x{screenshot.Height}, scale={scaleX}x{scaleY}.");
			}
		}
	}
}
#endif

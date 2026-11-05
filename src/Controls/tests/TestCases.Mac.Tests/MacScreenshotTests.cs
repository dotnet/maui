using ImageMagick;
using ImageMagick.Drawing;
using NUnit.Framework;

namespace Microsoft.Maui.TestCases.Tests;

public class MacScreenshotTests
{
	const int Width = 160;
	const int Height = 100;

	[TestCase(0.75)]
	[TestCase(0.77)]
	[TestCase(1)]
	[TestCase(1.25)]
	[TestCase(1.5)]
	[TestCase(2)]
	[TestCase(3)]
	[Category(UITestCategories.Image)]
	public void CropExcludesDesktopAndWindowBorder(double scale)
	{
		using var light = Capture(MagickColors.White, scale);
		using var dark = Capture(MagickColors.Black, scale);

		Assert.That(light.Width, Is.EqualTo(Width));
		Assert.That(light.Height, Is.EqualTo(Height));
		Assert.That(light.Compare(dark, ErrorMetric.Absolute), Is.Zero, "Desktop color must not affect the crop.");

		using var pixels = light.GetPixels();
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				var color = pixels.GetPixel(x, y).ToColor()!;
				Assert.That(color.A, Is.AnyOf((byte)0, (byte)255), $"Partial alpha at ({x},{y}).");
				if (color.A != 0)
					Assert.That(color, Is.EqualTo(MagickColors.Lime), $"Non-content pixel at ({x},{y}).");
			}
		}

		Assert.That(pixels.GetPixel(Width / 2, Height / 2).ToColor(), Is.EqualTo(MagickColors.Lime));
		Assert.That(pixels.GetPixel(4, Height / 2).ToColor(), Is.EqualTo(MagickColors.Lime));
		Assert.That(pixels.GetPixel(Width - 5, Height / 2).ToColor(), Is.EqualTo(MagickColors.Lime));
		Assert.That(pixels.GetPixel(Width / 2, 4).ToColor(), Is.EqualTo(MagickColors.Lime));
		Assert.That(pixels.GetPixel(Width / 2, Height - 5).ToColor(), Is.EqualTo(MagickColors.Lime));
		Assert.That(pixels.GetPixel(0, Height - 1).ToColor()!.A, Is.Zero);
		Assert.That(pixels.GetPixel(Width - 1, Height - 1).ToColor()!.A, Is.Zero);
		for (int y = Height - 28; y < Height; y++)
		{
			for (int x = 0; x < 28; x++)
				Assert.That(pixels.GetPixel(x, y).ToColor()!.A, Is.EqualTo(pixels.GetPixel(Width - 1 - x, y).ToColor()!.A));
		}
	}

	static MagickImage Capture(MagickColor desktop, double scale)
	{
		int width = (int)Math.Round(Width * scale);
		int height = (int)Math.Round(Height * scale);
		const int offset = 10;
		using var image = new MagickImage(desktop, (uint)(width + offset * 2), (uint)(height + offset * 2));
		new Drawables()
			.FillColor(MagickColors.Lime)
			.StrokeColor(MagickColors.Magenta)
			.StrokeWidth(2 * scale)
			.RoundRectangle(offset, offset, offset + width - 1, offset + height - 1, 24 * scale, 24 * scale)
			.Draw(image);

		return new MagickImage(UITest.CropMacScreenshot(image, new MagickGeometry(offset, offset, (uint)width, (uint)height), Width, Height));
	}
}

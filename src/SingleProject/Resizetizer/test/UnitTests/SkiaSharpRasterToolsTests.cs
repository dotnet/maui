using System;
using System.IO;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class SkiaSharpRasterToolsTests
	{
		public class Resize : IDisposable
		{
			readonly string DestinationFilename;
			readonly TestLogger Logger;

			public Resize()
			{
				DestinationFilename = Path.GetTempFileName();
				Logger = new TestLogger();
			}

			public void Dispose()
			{
				//Logger.Persist();
				//File.Copy(DestinationFilename, "output.png", true);
				File.Delete(DestinationFilename);
			}

			[Fact]
			public void BasicNoScaleNoResizeReturnsOriginalSize()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				info.Resize = false;
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(1792, resultImage.Width);
				Assert.Equal(1792, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.White, pixmap.GetPixelColor(350, 350));
			}

			[Fact]
			public void BasicNoScaleReturnsOriginalSize()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(1792, resultImage.Width);
				Assert.Equal(1792, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.White, pixmap.GetPixelColor(350, 350));
			}

			[Fact]
			public void BasicWithDownScaleReturnsDownScaledSize()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 0.5m);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(896, resultImage.Width);
				Assert.Equal(896, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.White, pixmap.GetPixelColor(175, 175));
			}

			[Fact]
			public void BasicWithColorsKeepsColors()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera_color.png";
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(256, resultImage.Width);
				Assert.Equal(256, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(37, 137));
				Assert.Equal(SKColors.Lime, pixmap.GetPixelColor(81, 137));
				Assert.Equal(SKColors.Blue, pixmap.GetPixelColor(125, 137));
			}

			[Fact]
			public void WithBaseSizeResizes()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera_color.png";
				info.BaseSize = new SKSize(512, 512);
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(512, resultImage.Width);
				Assert.Equal(512, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(74, 274));
				Assert.Equal(SKColors.Lime, pixmap.GetPixelColor(162, 274));
				Assert.Equal(SKColors.Blue, pixmap.GetPixelColor(250, 274));
			}

			[Fact]
			public void WithBaseSizeAndScaleResizes()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera_color.png";
				info.BaseSize = new SKSize(512, 512);
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 0.5m);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(256, resultImage.Width);
				Assert.Equal(256, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(37, 137));
				Assert.Equal(SKColors.Lime, pixmap.GetPixelColor(81, 137));
				Assert.Equal(SKColors.Blue, pixmap.GetPixelColor(125, 137));
			}

			[Fact]
			public void ColorizedReturnsColored()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				info.TintColor = SKColors.Red;
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(1792, resultImage.Width);
				Assert.Equal(1792, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(350, 350));
			}

			[Fact]
			public void ColorizedWithAlphaReturnsColored()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				info.TintColor = SKColors.Red.WithAlpha(127);
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(1792, resultImage.Width);
				Assert.Equal(1792, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red.WithAlpha(127), pixmap.GetPixelColor(350, 350));
			}

			[Fact]
			public void ColorizedWithNamedReturnsColored()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				info.TintColor = SKColors.Red;
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(1792, resultImage.Width);
				Assert.Equal(1792, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(350, 350));
			}

			[Fact]
			public void ColorizedWithColorsReplacesColors()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera_color.png";
				info.TintColor = SKColors.Red;
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(256, resultImage.Width);
				Assert.Equal(256, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(37, 137));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(81, 137));
				Assert.Equal(SKColors.Red, pixmap.GetPixelColor(125, 137));
			}

			[Fact]
			public void ColorizedWithAlphaWithColorsReplacesColors()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera_color.png";
				info.TintColor = SKColors.Red.WithAlpha(127);
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(256, resultImage.Width);
				Assert.Equal(256, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(10, 10));
				Assert.Equal(SKColors.Red.WithAlpha(127), pixmap.GetPixelColor(37, 137));
				Assert.Equal(SKColors.Red.WithAlpha(127), pixmap.GetPixelColor(81, 137));
				Assert.Equal(SKColors.Red.WithAlpha(127), pixmap.GetPixelColor(125, 137));
			}
		}

#pragma warning disable CS0618 // Type or member is obsolete
		public class FilterQualityTests : IDisposable
		{
			readonly string DestinationFilename;
			readonly string DestinationFilename2;
			readonly TestLogger Logger;

			public FilterQualityTests()
			{
				DestinationFilename = Path.GetTempFileName();
				DestinationFilename2 = Path.GetTempFileName();
				Logger = new TestLogger();
			}

			public void Dispose()
			{
				File.Delete(DestinationFilename);
				File.Delete(DestinationFilename2);
			}

			[Fact]
			public void DefaultFilterQualityIsHigh()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				var tools = new SkiaSharpRasterTools(info, Logger);

				Assert.Equal(SKFilterQuality.High, tools.Paint.FilterQuality);
			}

			[Theory]
			[InlineData(SKFilterQuality.None)]
			[InlineData(SKFilterQuality.Low)]
			[InlineData(SKFilterQuality.Medium)]
			[InlineData(SKFilterQuality.High)]
			public void FilterQualityIsAppliedFromInfo(SKFilterQuality quality)
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				info.FilterQuality = quality;
				var tools = new SkiaSharpRasterTools(info, Logger);

				Assert.Equal(quality, tools.Paint.FilterQuality);
			}

			[Fact]
			public void ResizeWithNoneFilterQualityProducesValidImage()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				info.FilterQuality = SKFilterQuality.None;
				info.BaseSize = new SKSize(100, 100);
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(100, resultImage.Width);
				Assert.Equal(100, resultImage.Height);
			}

			[Fact]
			public void DefaultFilterQualityProducesIdenticalOutputToHardcodedHigh()
			{
				// Resize with default (should be High)
				var infoDefault = new ResizeImageInfo();
				infoDefault.Filename = "images/camera.png";
				infoDefault.BaseSize = new SKSize(200, 200);
				var toolsDefault = new SkiaSharpRasterTools(infoDefault, Logger);
				var dpiPath = new DpiPath("", 1);
				toolsDefault.Resize(dpiPath, DestinationFilename);

				// Resize with explicit High
				var infoHigh = new ResizeImageInfo();
				infoHigh.Filename = "images/camera.png";
				infoHigh.BaseSize = new SKSize(200, 200);
				infoHigh.FilterQuality = SKFilterQuality.High;
				var toolsHigh = new SkiaSharpRasterTools(infoHigh, Logger);
				toolsHigh.Resize(dpiPath, DestinationFilename2);

				// Pixel-by-pixel comparison: must be identical
				using var bmpDefault = SKBitmap.Decode(DestinationFilename);
				using var bmpHigh = SKBitmap.Decode(DestinationFilename2);

				Assert.Equal(bmpDefault.Width, bmpHigh.Width);
				Assert.Equal(bmpDefault.Height, bmpHigh.Height);

				for (int y = 0; y < bmpDefault.Height; y++)
				{
					for (int x = 0; x < bmpDefault.Width; x++)
					{
						Assert.Equal(bmpDefault.GetPixel(x, y), bmpHigh.GetPixel(x, y));
					}
				}
			}

			[Fact]
			public void DifferentFilterQualitiesProduceDifferentPixelOutput()
			{
				// When downscaling a large image, None (nearest neighbor) vs High (bicubic)
				// should produce measurably different pixel data
				var dpiPath = new DpiPath("", 1);

				var infoNone = new ResizeImageInfo();
				infoNone.Filename = "images/camera.png";
				infoNone.BaseSize = new SKSize(100, 100);
				infoNone.FilterQuality = SKFilterQuality.None;
				var toolsNone = new SkiaSharpRasterTools(infoNone, Logger);
				toolsNone.Resize(dpiPath, DestinationFilename);

				var infoHigh = new ResizeImageInfo();
				infoHigh.Filename = "images/camera.png";
				infoHigh.BaseSize = new SKSize(100, 100);
				infoHigh.FilterQuality = SKFilterQuality.High;
				var toolsHigh = new SkiaSharpRasterTools(infoHigh, Logger);
				toolsHigh.Resize(dpiPath, DestinationFilename2);

				using var bmpNone = SKBitmap.Decode(DestinationFilename);
				using var bmpHigh = SKBitmap.Decode(DestinationFilename2);

				// Same dimensions
				Assert.Equal(bmpNone.Width, bmpHigh.Width);
				Assert.Equal(bmpNone.Height, bmpHigh.Height);

				// Count pixels that differ between None and High quality
				int differentPixels = 0;
				for (int y = 0; y < bmpNone.Height; y++)
				{
					for (int x = 0; x < bmpNone.Width; x++)
					{
						if (bmpNone.GetPixel(x, y) != bmpHigh.GetPixel(x, y))
							differentPixels++;
					}
				}

				// The outputs MUST differ - this proves FilterQuality actually affects rendering
				Assert.True(differentPixels > 0,
					"FilterQuality.None and FilterQuality.High should produce different pixel output when downscaling");
			}

			[Theory]
			[InlineData(SKFilterQuality.None)]
			[InlineData(SKFilterQuality.Low)]
			[InlineData(SKFilterQuality.Medium)]
			[InlineData(SKFilterQuality.High)]
			public void AllFilterQualitiesProduceCorrectlySizedOutput(SKFilterQuality quality)
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.png";
				info.BaseSize = new SKSize(256, 256);
				info.FilterQuality = quality;
				var tools = new SkiaSharpRasterTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(256, resultImage.Width);
				Assert.Equal(256, resultImage.Height);
			}
		}
#pragma warning restore CS0618 // Type or member is obsolete
	}
}

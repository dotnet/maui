using System;
using System.IO;
using SkiaSharp;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class SkiaSharpSvgToolsTests
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
				File.Copy(DestinationFilename, "output.png", true);
				File.Delete(DestinationFilename);
			}

			[Fact]
			public void BasicNoScaleReturnsOriginalSize()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				var tools = new SkiaSharpSvgTools(info, Logger);
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
			public void BasicNoScaleNoResizeReturnsOriginalSize()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				info.Resize = false;
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera.svg";
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera_color.svg";
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera_color.svg";
				info.BaseSize = new SKSize(512, 512);
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera_color.svg";
				info.BaseSize = new SKSize(512, 512);
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera.svg";
				info.TintColor = SKColors.Red;
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera.svg";
				info.TintColor = SKColors.Red.WithAlpha(127);
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera.svg";
				info.TintColor = SKColors.Red;
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera_color.svg";
				info.TintColor = SKColors.Red;
				var tools = new SkiaSharpSvgTools(info, Logger);
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
				info.Filename = "images/camera_color.svg";
				info.TintColor = SKColors.Red.WithAlpha(127);
				var tools = new SkiaSharpSvgTools(info, Logger);
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

			[Fact]
			public void ColorsInCssCanBeUsed()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/not_working.svg";
				var tools = new SkiaSharpSvgTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(24, resultImage.Width);
				Assert.Equal(24, resultImage.Height);

				using var pixmap = resultImage.PeekPixels();
				Assert.Equal(SKColors.Empty, pixmap.GetPixelColor(2, 2));
				Assert.Equal(0xFF71559B, pixmap.GetPixelColor(2, 6));
			}

			[Fact]
			public void SvgImageWithDecodingIssue_15442()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/find_icon.svg";
				var tools = new SkiaSharpSvgTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(200, resultImage.Width);
				Assert.Equal(200, resultImage.Height);

				using (var image = SKImage.FromBitmap(resultImage))
				using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
				using (var stream = File.OpenWrite("images/find_icon.svg.png"))
				{
					data.SaveTo(stream);
				}

				using var pixmap = resultImage.PeekPixels();

				Assert.Equal((SKColor)0x00000000, pixmap.GetPixelColor(10, 10));
				Assert.Equal((SKColor)0xFFA5ADF6, pixmap.GetPixelColor(81, 137));
				Assert.Equal((SKColor)0xFF635DF7, pixmap.GetPixelColor(125, 137));

				Assert.Equal((SKColor)0xFFA5ADF6, pixmap.GetPixelColor(22, 62));
				Assert.Equal((SKColor)0xFFA5ADF6, pixmap.GetPixelColor(72, 109));
				Assert.Equal((SKColor)0xFFA5ADF6, pixmap.GetPixelColor(131, 23));
				Assert.Equal((SKColor)0xFFA5ADF6, pixmap.GetPixelColor(178, 153));
				Assert.Equal((SKColor)0xFFA5ADF6, pixmap.GetPixelColor(124, 180));
			}

			[Fact]
			public void SvgImageWithDecodingIssue_12109()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/warning.svg";
				var tools = new SkiaSharpSvgTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(42, resultImage.Width);
				Assert.Equal(37, resultImage.Height);

				using (var image = SKImage.FromBitmap(resultImage))
				using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
				using (var stream = File.OpenWrite("images/warning.svg.png"))
				{
					data.SaveTo(stream);
				}

				using var pixmap = resultImage.PeekPixels();

				Assert.Equal((SKColor)0x00000000, pixmap.GetPixelColor(10, 10));
				Assert.Equal((SKColor)0xffe26b00, pixmap.GetPixelColor(20, 3));
				Assert.Equal((SKColor)0xffe26b00, pixmap.GetPixelColor(20, 34));
			}
		}
	}
}

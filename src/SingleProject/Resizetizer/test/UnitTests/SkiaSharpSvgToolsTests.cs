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

		public class ResizeQualityTests : IDisposable
		{
			readonly string DestinationFilename;
			readonly string DestinationFilename2;
			readonly TestLogger Logger;

			public ResizeQualityTests()
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
			public void DefaultQualityMapsToLinearMipmapSampling()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				var tools = new SkiaSharpSvgTools(info, Logger);

				Assert.Equal(
					new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear),
					tools.SamplingOptions);
			}

			[Fact]
			public void BestQualityMapsToMitchellCubicSampling()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				info.Quality = ResizeQuality.Best;
				var tools = new SkiaSharpSvgTools(info, Logger);

				Assert.Equal(
					new SKSamplingOptions(SKCubicResampler.Mitchell),
					tools.SamplingOptions);
			}

			[Fact]
			public void FastestQualityMapsToNearestNeighborSampling()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				info.Quality = ResizeQuality.Fastest;
				var tools = new SkiaSharpSvgTools(info, Logger);

				Assert.Equal(
					new SKSamplingOptions(SKFilterMode.Nearest, SKMipmapMode.None),
					tools.SamplingOptions);
			}

			[Fact]
			public void ResizeWithFastestQualityProducesValidImage()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				info.Quality = ResizeQuality.Fastest;
				info.BaseSize = new SKSize(100, 100);
				var tools = new SkiaSharpSvgTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(100, resultImage.Width);
				Assert.Equal(100, resultImage.Height);
			}

			[Fact]
			public void ResizeWithBestQualityProducesValidImage()
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				info.Quality = ResizeQuality.Best;
				info.BaseSize = new SKSize(100, 100);
				var tools = new SkiaSharpSvgTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(100, resultImage.Width);
				Assert.Equal(100, resultImage.Height);
			}

			[Fact]
			public void DefaultQualityProducesIdenticalOutputToExplicitAuto()
			{
				var dpiPath = new DpiPath("", 1);

				var infoDefault = new ResizeImageInfo();
				infoDefault.Filename = "images/camera.svg";
				infoDefault.BaseSize = new SKSize(100, 100);
				var toolsDefault = new SkiaSharpSvgTools(infoDefault, Logger);
				toolsDefault.Resize(dpiPath, DestinationFilename);

				var infoAuto = new ResizeImageInfo();
				infoAuto.Filename = "images/camera.svg";
				infoAuto.BaseSize = new SKSize(100, 100);
				infoAuto.Quality = ResizeQuality.Auto;
				var toolsAuto = new SkiaSharpSvgTools(infoAuto, Logger);
				toolsAuto.Resize(dpiPath, DestinationFilename2);

				using var bmpDefault = SKBitmap.Decode(DestinationFilename);
				using var bmpAuto = SKBitmap.Decode(DestinationFilename2);

				Assert.Equal(bmpDefault.Width, bmpAuto.Width);
				Assert.Equal(bmpDefault.Height, bmpAuto.Height);

				for (int y = 0; y < bmpDefault.Height; y++)
				{
					for (int x = 0; x < bmpDefault.Width; x++)
					{
						Assert.Equal(bmpDefault.GetPixel(x, y), bmpAuto.GetPixel(x, y));
					}
				}
			}

			[Fact]
			public void DifferentQualitiesProduceDifferentPixelOutput()
			{
				// SVG downscaling: Fastest (nearest) vs Auto (bilinear+mipmaps)
				var dpiPath = new DpiPath("", 1);

				var infoFastest = new ResizeImageInfo();
				infoFastest.Filename = "images/camera.svg";
				infoFastest.BaseSize = new SKSize(100, 100);
				infoFastest.Quality = ResizeQuality.Fastest;
				var toolsFastest = new SkiaSharpSvgTools(infoFastest, Logger);
				toolsFastest.Resize(dpiPath, DestinationFilename);

				var infoAuto = new ResizeImageInfo();
				infoAuto.Filename = "images/camera.svg";
				infoAuto.BaseSize = new SKSize(100, 100);
				infoAuto.Quality = ResizeQuality.Auto;
				var toolsAuto = new SkiaSharpSvgTools(infoAuto, Logger);
				toolsAuto.Resize(dpiPath, DestinationFilename2);

				using var bmpFastest = SKBitmap.Decode(DestinationFilename);
				using var bmpAuto = SKBitmap.Decode(DestinationFilename2);

				Assert.Equal(bmpFastest.Width, bmpAuto.Width);
				Assert.Equal(bmpFastest.Height, bmpAuto.Height);

				int differentPixels = 0;
				for (int y = 0; y < bmpFastest.Height; y++)
				{
					for (int x = 0; x < bmpFastest.Width; x++)
					{
						if (bmpFastest.GetPixel(x, y) != bmpAuto.GetPixel(x, y))
							differentPixels++;
					}
				}

				Assert.True(differentPixels > 0,
					"SVG: Fastest and Auto should produce different pixel output when downscaling");
			}

			[Theory]
			[InlineData("Auto")]
			[InlineData("Best")]
			[InlineData("Fastest")]
			public void AllQualitiesProduceCorrectlySizedOutput(string qualityName)
			{
				var quality = Enum.Parse<ResizeQuality>(qualityName);
				var info = new ResizeImageInfo();
				info.Filename = "images/camera.svg";
				info.BaseSize = new SKSize(256, 256);
				info.Quality = quality;
				var tools = new SkiaSharpSvgTools(info, Logger);
				var dpiPath = new DpiPath("", 1);

				tools.Resize(dpiPath, DestinationFilename);

				using var resultImage = SKBitmap.Decode(DestinationFilename);
				Assert.Equal(256, resultImage.Width);
				Assert.Equal(256, resultImage.Height);
			}
		}
	}
}

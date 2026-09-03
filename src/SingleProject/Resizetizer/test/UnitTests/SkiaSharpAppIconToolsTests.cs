using System.IO;
using SkiaSharp;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class SkiaSharpAppIconToolsTests
	{
		public class Resize : BaseTest
		{
			readonly string DestinationFilename;
			readonly TestLogger Logger;

			public Resize(ITestOutputHelper outputHelper)
				: base(outputHelper)
			{
				DestinationFilename = Path.Combine(DestinationDirectory, Path.GetRandomFileName() + ".png");
				Logger = new TestLogger();
			}

			[Theory]
			// nice increments
			[InlineData(0.5, 0.5, "appicon.svg", "appiconfg.svg", 512, 512)]
			[InlineData(0.5, 1, "appicon.svg", "appiconfg.svg", 512, 512)]
			[InlineData(0.5, 2, "appicon.svg", "appiconfg.svg", 512, 512)]
			[InlineData(1, 0.5, "appicon.svg", "appiconfg.svg", 1024, 1024)]
			[InlineData(1, 1, "appicon.svg", "appiconfg.svg", 1024, 1024)]
			[InlineData(1, 1, "dotnet_background.svg", "dotnet_logo.svg", 456, 456)]
			[InlineData(1, 2, "appicon.svg", "appiconfg.svg", 1024, 1024)]
			[InlineData(2, 0.5, "appicon.svg", "appiconfg.svg", 2048, 2048)]
			[InlineData(2, 1, "appicon.svg", "appiconfg.svg", 2048, 2048)]
			[InlineData(2, 2, "appicon.svg", "appiconfg.svg", 2048, 2048)]
			[InlineData(3, 3, "appicon.svg", "appiconfg.svg", 3072, 3072)]
			// scary increments
			[InlineData(0.3, 0.3, "appicon.svg", "appiconfg.svg", 307, 307)]
			[InlineData(0.3, 0.7, "appicon.svg", "appiconfg.svg", 307, 307)]
			[InlineData(0.7, 0.7, "appicon.svg", "appiconfg.svg", 717, 717)]
			[InlineData(1, 0.7, "appicon.svg", "appiconfg.svg", 1024, 1024)]
			[InlineData(2, 0.7, "appicon.svg", "appiconfg.svg", 2048, 2048)]
			[InlineData(2.3, 2.3, "appicon.svg", "appiconfg.svg", 2355, 2355)]
			[InlineData(0.3, 1, "appicon.svg", "appiconfg.svg", 307, 307)]
			[InlineData(0.3, 2, "appicon.svg", "appiconfg.svg", 307, 307)]
			// scary increments
			[InlineData(0.3, 0.3, "appicon.svg", "appiconfg-red-512.svg", 307, 307)]
			[InlineData(0.3, 0.7, "appicon.svg", "appiconfg-red-512.svg", 307, 307)]
			[InlineData(0.7, 0.7, "appicon.svg", "appiconfg-red-512.svg", 717, 717)]
			[InlineData(1, 0.7, "appicon.svg", "appiconfg-red-512.svg", 1024, 1024)]
			[InlineData(2, 0.7, "appicon.svg", "appiconfg-red-512.svg", 2048, 2048)]
			[InlineData(0.3, 1, "appicon.svg", "appiconfg-red-512.svg", 307, 307)]
			[InlineData(0.3, 2, "appicon.svg", "appiconfg-red-512.svg", 307, 307)]
			public void BasicTest(double dpi, double fgScale, string bg, string fg, int exWidth, int exHeight)
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/" + bg;
				info.ForegroundFilename = "images/" + fg;
				info.ForegroundScale = fgScale;
				info.IsAppIcon = true;

				var tools = new SkiaSharpAppIconTools(info, Logger);
				var dpiPath = new DpiPath("", (decimal)dpi);

				tools.Resize(dpiPath, DestinationFilename);

				AssertFileSize(DestinationFilename, exWidth, exHeight);
				AssertFileMatches(DestinationFilename, new object[] { dpi.ToString("0.#"), fgScale.ToString("0.#"), bg, fg });
			}

			[Theory]
			[InlineData(1, 1, "appicon.svg", "prismicon.svg", 1024, 1024)]
			public void ComplexIconTest(double dpi, double fgScale, string bg, string fg, int exWidth, int exHeight)
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/" + bg;
				info.ForegroundFilename = "images/" + fg;
				info.ForegroundScale = fgScale;
				info.IsAppIcon = true;

				var tools = new SkiaSharpAppIconTools(info, Logger);
				var dpiPath = new DpiPath("", (decimal)dpi);

				tools.Resize(dpiPath, DestinationFilename);

				AssertFileSize(DestinationFilename, exWidth, exHeight);
				AssertFileMatches(DestinationFilename, new object[] { dpi, fgScale, bg, fg });
			}

			[Theory]
			[InlineData(0.5, 1, "dotnet_background.svg", "tall_image.png", 228, 228)]
			[InlineData(0.5, 1, "dotnet_background.svg", "wide_image.png", 228, 228)]
			[InlineData(0.5, 1, "tall_image.png", "camera.svg", 60, 320)]
			[InlineData(0.5, 1, "wide_image.png", "camera.svg", 320, 60)]
			[InlineData(1, 0.5, "dotnet_background.svg", "tall_image.png", 456, 456)]
			[InlineData(1, 0.5, "dotnet_background.svg", "wide_image.png", 456, 456)]
			[InlineData(1, 0.5, "tall_image.png", "camera.svg", 119, 640)]
			[InlineData(1, 0.5, "wide_image.png", "camera.svg", 640, 119)]
			[InlineData(1, 1, "dotnet_background.svg", "tall_image.png", 456, 456)]
			[InlineData(1, 1, "dotnet_background.svg", "wide_image.png", 456, 456)]
			[InlineData(1, 1, "tall_image.png", "camera.svg", 119, 640)]
			[InlineData(1, 1, "wide_image.png", "camera.svg", 640, 119)]
			public void DiffPropoprtionWithoutBaseUseBackgroundSize(double dpi, double fgScale, string bg, string fg, int exWidth, int exHeight)
			{
				var info = new ResizeImageInfo
				{
					Filename = "images/" + bg,
					ForegroundFilename = "images/" + fg,
					ForegroundScale = fgScale,
					IsAppIcon = true,
					Color = SKColors.Orange,
				};

				var tools = new SkiaSharpAppIconTools(info, Logger);
				var dpiPath = new DpiPath("", (decimal)dpi);

				tools.Resize(dpiPath, DestinationFilename);

				AssertFileSize(DestinationFilename, exWidth, exHeight);
				AssertFileMatches(DestinationFilename, new object[] { dpi, fgScale, bg, fg });
			}

			[Theory]
			[InlineData(0.5, 1, "wide_image.png", "camera.svg", 150, 150)]
			[InlineData(0.5, 1, "tall_image.png", "camera.svg", 150, 150)]
			[InlineData(0.5, 1, "dotnet_background.svg", "wide_image.png", 150, 150)]
			[InlineData(0.5, 1, "dotnet_background.svg", "tall_image.png", 150, 150)]
			[InlineData(1, 0.5, "wide_image.png", "camera.svg", 300, 300)]
			[InlineData(1, 0.5, "tall_image.png", "camera.svg", 300, 300)]
			[InlineData(1, 0.5, "dotnet_background.svg", "wide_image.png", 300, 300)]
			[InlineData(1, 0.5, "dotnet_background.svg", "tall_image.png", 300, 300)]
			[InlineData(1, 1, "wide_image.png", "camera.svg", 300, 300)]
			[InlineData(1, 1, "tall_image.png", "camera.svg", 300, 300)]
			[InlineData(1, 1, "dotnet_background.svg", "wide_image.png", 300, 300)]
			[InlineData(1, 1, "dotnet_background.svg", "tall_image.png", 300, 300)]
			public void DiffPropoprtionWithBaseSize(double dpi, double fgScale, string bg, string fg, int exWidth, int exHeight)
			{
				var info = new ResizeImageInfo
				{
					Filename = "images/" + bg,
					ForegroundFilename = "images/" + fg,
					ForegroundScale = fgScale,
					IsAppIcon = true,
					Color = SKColors.Orange,
					BaseSize = new SKSize(300, 300),
				};

				var tools = new SkiaSharpAppIconTools(info, Logger);
				var dpiPath = new DpiPath("", (decimal)dpi);

				tools.Resize(dpiPath, DestinationFilename);

				AssertFileSize(DestinationFilename, exWidth, exHeight);
				AssertFileMatches(DestinationFilename, new object[] { dpi, fgScale, bg, fg });
			}

			[Theory]
			[InlineData(0.5, 1, "dotnet_background.svg", "tall_image.png", 150, 150)]
			[InlineData(0.5, 1, "dotnet_background.svg", "wide_image.png", 150, 150)]
			[InlineData(0.5, 1, "tall_image.png", "camera.svg", 150, 150)]
			[InlineData(0.5, 1, "wide_image.png", "camera.svg", 150, 150)]
			[InlineData(1, 0.5, "dotnet_background.svg", "tall_image.png", 300, 300)]
			[InlineData(1, 0.5, "dotnet_background.svg", "wide_image.png", 300, 300)]
			[InlineData(1, 0.5, "tall_image.png", "camera.svg", 300, 300)]
			[InlineData(1, 0.5, "wide_image.png", "camera.svg", 300, 300)]
			[InlineData(1, 1, "dotnet_background.svg", "tall_image.png", 300, 300)]
			[InlineData(1, 1, "dotnet_background.svg", "wide_image.png", 300, 300)]
			[InlineData(1, 1, "tall_image.png", "camera.svg", 300, 300)]
			[InlineData(1, 1, "wide_image.png", "camera.svg", 300, 300)]
			public void DiffPropoprtionWithDpiSize(double dpi, double fgScale, string bg, string fg, int exWidth, int exHeight)
			{
				var info = new ResizeImageInfo
				{
					Filename = "images/" + bg,
					ForegroundFilename = "images/" + fg,
					ForegroundScale = fgScale,
					IsAppIcon = true,
					Color = SKColors.Orange,
				};

				var tools = new SkiaSharpAppIconTools(info, Logger);
				var dpiPath = new DpiPath("", (decimal)dpi, size: new SKSize(300, 300));

				tools.Resize(dpiPath, DestinationFilename);

				AssertFileSize(DestinationFilename, exWidth, exHeight);
				AssertFileMatches(DestinationFilename, new object[] { dpi, fgScale, bg, fg });
			}
		}
	}
}

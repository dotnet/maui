using System;
using System.IO;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class SkiaSharpAppIconToolsTests
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

			[Theory]
			// nice increments
			[InlineData(0.5, 0.5, "appicon.svg", "appiconfg.svg")]
			[InlineData(0.5, 1, "appicon.svg", "appiconfg.svg")]
			[InlineData(0.5, 2, "appicon.svg", "appiconfg.svg")]
			[InlineData(1, 0.5, "appicon.svg", "appiconfg.svg")]
			[InlineData(1, 1, "appicon.svg", "appiconfg.svg")]
			[InlineData(1, 2, "appicon.svg", "appiconfg.svg")]
			[InlineData(2, 0.5, "appicon.svg", "appiconfg.svg")]
			[InlineData(2, 1, "appicon.svg", "appiconfg.svg")]
			[InlineData(2, 2, "appicon.svg", "appiconfg.svg")]
			[InlineData(3, 3, "appicon.svg", "appiconfg.svg")]
			// scary increments
			[InlineData(0.3, 0.3, "appicon.svg", "appiconfg.svg")]
			[InlineData(0.3, 0.7, "appicon.svg", "appiconfg.svg")]
			[InlineData(0.7, 0.7, "appicon.svg", "appiconfg.svg")]
			[InlineData(1, 0.7, "appicon.svg", "appiconfg.svg")]
			[InlineData(2, 0.7, "appicon.svg", "appiconfg.svg")]
			[InlineData(2.3, 2.3, "appicon.svg", "appiconfg.svg")]
			[InlineData(0.3, 1, "appicon.svg", "appiconfg.svg")]
			[InlineData(0.3, 2, "appicon.svg", "appiconfg.svg")]
			// scary increments
			[InlineData(0.3, 0.3, "appicon.svg", "appiconfg-red-512.svg")]
			[InlineData(0.3, 0.7, "appicon.svg", "appiconfg-red-512.svg")]
			[InlineData(0.7, 0.7, "appicon.svg", "appiconfg-red-512.svg")]
			[InlineData(1, 0.7, "appicon.svg", "appiconfg-red-512.svg")]
			[InlineData(2, 0.7, "appicon.svg", "appiconfg-red-512.svg")]
			[InlineData(0.3, 1, "appicon.svg", "appiconfg-red-512.svg")]
			[InlineData(0.3, 2, "appicon.svg", "appiconfg-red-512.svg")]
			public void BasicTest(double dpi, double fgScale, string bg, string fg)
			{
				var info = new ResizeImageInfo();
				info.Filename = "images/" + bg;
				info.ForegroundFilename = "images/" + fg;
				info.ForegroundScale = fgScale;
				info.IsAppIcon = true;

				var tools = new SkiaSharpAppIconTools(info, Logger);
				var dpiPath = new DpiPath("", (decimal)dpi);

				tools.Resize(dpiPath, DestinationFilename);

				//File.Copy(DestinationFilename, $"output-{dpi}-{fgScale}-{bg}-{fg}.png", true);
			}
		}
	}
}

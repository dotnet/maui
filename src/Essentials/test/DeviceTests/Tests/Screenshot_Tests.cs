using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Screenshot_Tests
	{
		[Fact]
		public async Task GetPngScreenshot()
		{
			ScreenshotResult mediaFile = null;

			for (int i = 0; i < 2; i++)
			{
				try
				{
					mediaFile = await Screenshot.CaptureAsync();
					break;
				}
				catch { }
				await Task.Delay(1000);
			}

			var png = await mediaFile.OpenReadAsync(ScreenshotFormat.Png);
			Assert.True(png.Length > 0);
		}

		[Fact]
		public async Task GetJpegScreenshot()
		{
			ScreenshotResult mediaFile = null;

			for (int i = 0; i < 2; i++)
			{
				try
				{
					mediaFile = await Screenshot.CaptureAsync();
					break;
				}
				catch { }
				await Task.Delay(1000);
			}

			var png = await mediaFile.OpenReadAsync(ScreenshotFormat.Jpeg);
			Assert.True(png.Length > 0);
		}
	}
}

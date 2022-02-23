using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	public class Screenshot_Tests
	{
		[Fact]
		public Task GetPngScreenshot()
		{
			return Utils.OnMainThread(async () =>
			{
				if (CanExecuteTest())
				{
					await Task.Delay(100);
					ScreenshotResult mediaFile = await Screenshot.CaptureAsync();
					var png = await mediaFile.OpenReadAsync(ScreenshotFormat.Png);
					Assert.True(png.Length > 0);
				}
			});
		}

		[Fact]
		public Task GetJpegScreenshot()
		{
			return Utils.OnMainThread(async () =>
			{
				if (CanExecuteTest())
				{
					await Task.Delay(100);
					ScreenshotResult mediaFile = await Screenshot.CaptureAsync();
					var png = await mediaFile.OpenReadAsync(ScreenshotFormat.Jpeg);
					Assert.True(png.Length > 0);
				}
			});
		}

		static bool CanExecuteTest()
		{
#if __IOS__
			return PlatformVersion.IsAtLeast(13);
#else
			return true;
#endif
		}
	}
}

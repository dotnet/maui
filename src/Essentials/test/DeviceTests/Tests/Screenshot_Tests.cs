using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Media;
using Microsoft.Maui.Platform;
using Xunit;

namespace Microsoft.Maui.Essentials.DeviceTests
{
	[Category("Screenshot")]
	public class Screenshot_Tests
	{
		[UIFact]
		public async Task GetPngScreenshot()
		{
				await Task.Delay(100);

				IScreenshotResult mediaFile = await Screenshot.CaptureAsync();
				var png = await mediaFile.OpenReadAsync(ScreenshotFormat.Png);

				Assert.True(png.Length > 0);
		}

		[UIFact]
		public async Task GetJpegScreenshot()
		{
				await Task.Delay(100);

				IScreenshotResult mediaFile = await Screenshot.CaptureAsync();
				var png = await mediaFile.OpenReadAsync(ScreenshotFormat.Jpeg);

				Assert.True(png.Length > 0);
		}

		[UIFact]
		public async Task CaptureStaticAsync()
		{
				await Task.Delay(100);

				IScreenshotResult mediaFile = await Screenshot.CaptureAsync();

				Assert.True(mediaFile.Width > 0);
				Assert.True(mediaFile.Height > 0);

				using (var stream = await mediaFile.OpenReadAsync())
					Assert.True(stream.Length > 0);
		}

		[UIFact]
		public async Task CaptureAsync()
		{
				await Task.Delay(100);

				IScreenshotResult mediaFile = await Screenshot.Default.CaptureAsync();

				Assert.True(mediaFile.Width > 0);
				Assert.True(mediaFile.Height > 0);

				using (var stream = await mediaFile.OpenReadAsync())
					Assert.True(stream.Length > 0);
		}

#if IOS || MACCATALYST

		[UIFact]
		public async Task CaptureWindowAsync()
		{
				await Task.Delay(100);

				var window = WindowStateManager.Default.GetCurrentUIWindow();

				IScreenshotResult mediaFile = await Screenshot.Default.CaptureAsync(window);

				Assert.True(mediaFile.Width > 0);
				Assert.True(mediaFile.Height > 0);

				using (var stream = await mediaFile.OpenReadAsync())
					Assert.True(stream.Length > 0);
		}

		[UIFact]
		public async Task CaptureViewAsync()
		{
				await Task.Delay(100);

				var window = WindowStateManager.Default.GetCurrentUIWindow();
				var view = window.RootViewController.View;

				IScreenshotResult mediaFile = await Screenshot.Default.CaptureAsync(view);

				Assert.True(mediaFile.Width > 0);
				Assert.True(mediaFile.Height > 0);

				using (var stream = await mediaFile.OpenReadAsync())
					Assert.True(stream.Length > 0);
		}

		[UITheory]
		[InlineData(true)]
		[InlineData(false)]
		public async Task CaptureLayerAsync(bool skipChildren)
		{
				await Task.Delay(100);

				var window = WindowStateManager.Default.GetCurrentUIWindow();
				var view = window.RootViewController.View;
				var layer = view.Layer;

				IScreenshotResult mediaFile = await Screenshot.Default.CaptureAsync(layer, skipChildren);

				Assert.True(mediaFile.Width > 0);
				Assert.True(mediaFile.Height > 0);

				using (var stream = await mediaFile.OpenReadAsync())
					Assert.True(stream.Length > 0);
		}

#endif
	}
}

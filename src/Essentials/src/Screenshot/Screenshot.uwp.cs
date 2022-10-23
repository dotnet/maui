using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.Maui.ApplicationModel;
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
		public bool IsCaptureSupported =>
			true;

		public Task<IScreenshotResult> CaptureAsync()
		{
			var element = WindowStateManager.Default.GetActiveWindow(true);

			return CaptureAsync(element);
		}

		public Task<IScreenshotResult> CaptureAsync(Window window) =>
			CaptureAsync(window.Content);

		public async Task<IScreenshotResult> CaptureAsync(UIElement element)
		{
			var bmp = new RenderTargetBitmap();

			// NOTE: Return to the main thread so we can access view properties such as
			//       width and height. Do not ConfigureAwait!
			await bmp.RenderAsync(element);

			// get the view information first
			var width = bmp.PixelWidth;
			var height = bmp.PixelHeight;

			// then potentially move to a different thread
			var pixels = await bmp.GetPixelsAsync().AsTask().ConfigureAwait(false);

			return new ScreenshotResult(width, height, pixels);
		}
	}

	partial class ScreenshotResult
	{
		readonly byte[] bytes;

		public ScreenshotResult(int width, int height, IBuffer pixels)
		{
			Width = width;
			Height = height;
			bytes = pixels?.ToArray() ?? throw new ArgumentNullException(nameof(pixels));
		}

		async Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality)
		{
			var ms = new InMemoryRandomAccessStream();
			await EncodeAsync(format, ms).ConfigureAwait(false);
			return ms.AsStreamForRead();
		}

		Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
		{
			var ms = destination.AsRandomAccessStream();
			return EncodeAsync(format, ms);
		}

		Task<byte[]> PlatformToPixelBufferAsync() =>
			Task.FromResult(bytes);

		async Task EncodeAsync(ScreenshotFormat format, IRandomAccessStream ms)
		{
			var f = ToBitmapEncoder(format);

			var encoder = await BitmapEncoder.CreateAsync(f, ms).AsTask().ConfigureAwait(false);
			encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)Width, (uint)Height, 96, 96, bytes);
			await encoder.FlushAsync().AsTask().ConfigureAwait(false);
		}

		static Guid ToBitmapEncoder(ScreenshotFormat format) =>
			format switch
			{
				ScreenshotFormat.Jpeg => BitmapEncoder.JpegEncoderId,
				ScreenshotFormat.Png => BitmapEncoder.PngEncoderId,
				_ => throw new ArgumentOutOfRangeException(nameof(format))
			};
	}
}

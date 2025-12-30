using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Microsoft.Maui.ApplicationModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media.Imaging;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;

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
			if (element is WebView2 webView)
			{
				InMemoryRandomAccessStream stream = new();
				await webView.CoreWebView2.CapturePreviewAsync(Web.WebView2.Core.CoreWebView2CapturePreviewImageFormat.Png, stream);

				BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
				PixelDataProvider provider = await decoder.GetPixelDataAsync();
				byte[] byteArray = provider.DetachPixelData();

				return new ScreenshotResult((int)decoder.PixelWidth, (int)decoder.PixelHeight, byteArray, decoder.DpiX, decoder.DpiY);
			}
			else
			{
				var bmp = new RenderTargetBitmap();

				// NOTE: Return to the main thread so we can access view properties such as
				//       width and height. Do not ConfigureAwait!
				await bmp.RenderAsync(element);

				// get the view information first
				var width = bmp.PixelWidth;
				var height = bmp.PixelHeight;

				// then potentially move to a different thread
				IBuffer pixels = await bmp.GetPixelsAsync().AsTask().ConfigureAwait(false);

				return new ScreenshotResult(width, height, pixels);
			}
		}
	}

	partial class ScreenshotResult
	{
		readonly double _dpiX;
		readonly double _dpiY;
		readonly byte[] _bytes;

		internal ScreenshotResult(int width, int height, byte[] bytes, double dpiX, double dpiY)
		{
			Width = width;
			Height = height;
			_bytes = bytes;
			_dpiX = dpiX;
			_dpiY = dpiY;
		}

		public ScreenshotResult(int width, int height, IBuffer pixels)
		{
			Width = width;
			Height = height;
			_bytes = pixels.ToArray() ?? throw new ArgumentNullException(nameof(pixels));
			_dpiX = 96;
			_dpiY = 96;
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
			Task.FromResult(_bytes);

		async Task EncodeAsync(ScreenshotFormat format, IRandomAccessStream ms)
		{
			var f = ToBitmapEncoder(format);

			var encoder = await BitmapEncoder.CreateAsync(f, ms).AsTask().ConfigureAwait(false);
			encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)Width, (uint)Height, _dpiX, _dpiY, _bytes);
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

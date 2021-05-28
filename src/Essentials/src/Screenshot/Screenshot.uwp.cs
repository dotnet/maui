using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#if NET6_0 || NET5_0
using Microsoft.UI.Xaml;
#else
using Windows.UI.Xaml;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class Screenshot
	{
		internal static bool PlatformIsCaptureSupported =>
			true;

		static Task<ScreenshotResult> PlatformCaptureAsync()
		{
			throw new NotImplementedException("WINUI");
			//var element = Window.Current?.Content as FrameworkElement;
			//if (element == null)
			//    throw new InvalidOperationException("Unable to find main window content.");

			//var bmp = new RenderTargetBitmap();

			//// NOTE: Return to the main thread so we can access view properties such as
			////       width and height. Do not ConfigureAwait!
			//await bmp.RenderAsync(element);

			//// get the view information first
			//var width = bmp.PixelWidth;
			//var height = bmp.PixelHeight;

			//// then potentially move to a different thread
			//var pixels = await bmp.GetPixelsAsync().AsTask().ConfigureAwait(false);

			//return new ScreenshotResult(width, height, pixels);
		}
	}

	public partial class ScreenshotResult
	{
		readonly byte[] bytes;

		public ScreenshotResult(int width, int height, IBuffer pixels)
		{
			Width = width;
			Height = height;
			bytes = pixels?.ToArray() ?? throw new ArgumentNullException(nameof(pixels));
		}

		internal async Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format)
		{
			var f = format switch
			{
				ScreenshotFormat.Jpeg => BitmapEncoder.JpegEncoderId,
				_ => BitmapEncoder.PngEncoderId
			};

			var ms = new InMemoryRandomAccessStream();

			var encoder = await BitmapEncoder.CreateAsync(f, ms).AsTask().ConfigureAwait(false);
			encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)Width, (uint)Height, 96, 96, bytes);
			await encoder.FlushAsync().AsTask().ConfigureAwait(false);

			return ms.AsStreamForRead();
		}
	}
}

using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
#if WINDOWS
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media.Imaging;
#else
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
#endif

namespace Microsoft.Maui.Essentials
{
	public static partial class Screenshot
	{
		internal static bool PlatformIsCaptureSupported =>
			true;

		static async Task<ScreenshotResult> PlatformCaptureAsync()
		{
			var element = Window.Current?.Content as FrameworkElement;
			if (element == null)
				throw new InvalidOperationException("Unable to find main window content.");

			// NOTE: Return to the main thread so we can access view properties such as
			//       width and height. Do not ConfigureAwait!
			var bmp = await element.RenderAsync();

			// get the view information first
			var width = bmp.PixelWidth;
			var height = bmp.PixelHeight;

			// then potentially move to a different thread
			var pixels = await bmp.AsPixelsAsync();

			return new ScreenshotResult(width, height, pixels);
		}

		public static async Task<RenderTargetBitmap> RenderAsync (this FrameworkElement element)
		{
			var bitmap = new RenderTargetBitmap();
			await bitmap.RenderAsync(element);
			return bitmap;
		}

		public static async Task<byte[]> RenderAsJPEGAsync(this FrameworkElement element)
		{
			var memoryStream = new InMemoryRandomAccessStream();
			BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, memoryStream);
			return await element.RenderAsImageAsync(enc, memoryStream);
		}

		public static async Task<byte[]> RenderAsPNGAsync(this FrameworkElement element)
		{
			var memoryStream = new InMemoryRandomAccessStream();
			BitmapEncoder enc = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, memoryStream);
			return await element.RenderAsImageAsync(enc, memoryStream);
		}

		public static async Task<byte[]> RenderAsImageAsync(this FrameworkElement element, BitmapEncoder enc, IRandomAccessStream memoryStream)
		{
			var bitmap = await element.RenderAsync();
			var pixels = await bitmap.AsPixelsAsync();
			return await bitmap.AsImageBytesAsync(pixels, enc, memoryStream);
		}

		public static async Task<IBuffer> AsPixelsAsync(this RenderTargetBitmap bitmap) =>
			 await bitmap.GetPixelsAsync().AsTask().ConfigureAwait(false);


		public static async Task<byte[]> AsImageBytesAsync(this RenderTargetBitmap bitmap, IBuffer pixelBuffer, BitmapEncoder enc, IRandomAccessStream memoryStream)
		{
			var alphaMode = enc.EncoderInformation.CodecId == BitmapEncoder.JpegEncoderId ? BitmapAlphaMode.Ignore : BitmapAlphaMode.Premultiplied;
			enc.SetPixelData(BitmapPixelFormat.Bgra8, alphaMode, (uint)bitmap.PixelWidth, (uint)bitmap.PixelHeight, 1, 1, pixelBuffer.ToArray());
			await enc.FlushAsync();
			await memoryStream.FlushAsync();
			var stream = memoryStream.AsStream();
			byte[] result = new byte[stream.Length];
			stream.Read(result, 0, result.Length);
			return result;
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

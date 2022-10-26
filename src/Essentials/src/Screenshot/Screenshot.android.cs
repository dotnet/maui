using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Views;
using Java.Nio;
using Microsoft.Maui.ApplicationModel;

namespace Microsoft.Maui.Media
{
	partial class ScreenshotImplementation : IPlatformScreenshot, IScreenshot
	{
		static IWindowManager windowManager;

		static IWindowManager WindowManager =>
			windowManager ??= Application.Context.GetSystemService(Context.WindowService) as IWindowManager;

		public bool IsCaptureSupported => true;

		public Task<IScreenshotResult> CaptureAsync()
		{
			if (WindowManager?.DefaultDisplay?.Flags.HasFlag(DisplayFlags.Secure) == true)
				throw new UnauthorizedAccessException("Unable to take a screenshot of a secure window.");

			var activity = ActivityStateManager.Default.GetCurrentActivity(true);

			return CaptureAsync(activity);
		}

		public Task<IScreenshotResult> CaptureAsync(Activity activity)
		{
			var view = activity?.Window?.DecorView?.RootView;
			if (view == null)
				throw new InvalidOperationException("Unable to find the main window.");

			return CaptureAsync(view);
		}

		public Task<IScreenshotResult> CaptureAsync(View view)
		{
			_ = view ?? throw new ArgumentNullException(nameof(view));

			var bitmap = Render(view);
			var result = bitmap is null ? null : new ScreenshotResult(bitmap);

			return Task.FromResult<IScreenshotResult>(result);
		}

		static Bitmap Render(View view)
		{
			var bitmap = RenderUsingCanvasDrawing(view);

			if (bitmap == null)
				bitmap = RenderUsingDrawingCache(view);

			return bitmap;
		}

		static Bitmap RenderUsingCanvasDrawing(View view)
		{
			try
			{
				if (view?.LayoutParameters == null || Bitmap.Config.Argb8888 == null)
					return null;
				var width = view.Width;
				var height = view.Height;

				var bitmap = Bitmap.CreateBitmap(width, height, Bitmap.Config.Argb8888);
				if (bitmap == null)
					return null;

				using (var canvas = new Canvas(bitmap))
					view.Draw(canvas);

				return bitmap;
			}
			catch (Exception)
			{
				return null;
			}
		}

		static Bitmap RenderUsingDrawingCache(View view)
		{
#pragma warning disable CS0618 // Type or member is obsolete
#pragma warning disable CA1416 // Validate platform compatibility
#pragma warning disable CA1422 // Validate platform compatibility
			try
			{
				var enabled = view.DrawingCacheEnabled;
				view.DrawingCacheEnabled = true;
				view.BuildDrawingCache();
				var cachedBitmap = view.DrawingCache;
				if (cachedBitmap == null)
					return null;
				var bitmap = Bitmap.CreateBitmap(cachedBitmap);
				view.DrawingCacheEnabled = enabled;
				return bitmap;
			}
			catch (Exception)
			{
				return null;
			}
#pragma warning restore CA1422 // Validate platform compatibility
#pragma warning restore CA1416 // Validate platform compatibility
#pragma warning restore CS0618 // Type or member is obsolete
		}
	}

	partial class ScreenshotResult
	{
		readonly Bitmap bmp;

		internal ScreenshotResult(Bitmap bmp)
			: base()
		{
			this.bmp = bmp;

			Width = bmp.Width;
			Height = bmp.Height;
		}

		Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format, int quality)
		{
			var result = new MemoryStream();
			PlatformCopyToAsync(result, format, quality);
			result.Position = 0;
			return Task.FromResult<Stream>(result);
		}

		Task PlatformCopyToAsync(Stream destination, ScreenshotFormat format, int quality)
		{
			var f = ToCompressFormat(format);
			bmp.Compress(f, quality, destination);
			return Task.CompletedTask;
		}

		Task<byte[]> PlatformToPixelBufferAsync()
		{
			var byteBuffer = ByteBuffer.AllocateDirect(bmp.ByteCount);
			bmp.CopyPixelsToBuffer(byteBuffer);
			byte[] byt = new byte[bmp.ByteCount];
			Marshal.Copy(byteBuffer.GetDirectBufferAddress(), byt, 0, bmp.ByteCount);
			return Task.FromResult(byt);
		}

		static Bitmap.CompressFormat ToCompressFormat(ScreenshotFormat format) =>
			format switch
			{
				ScreenshotFormat.Png => Bitmap.CompressFormat.Png!,
				ScreenshotFormat.Jpeg => Bitmap.CompressFormat.Jpeg!,
				_ => throw new ArgumentOutOfRangeException(nameof(format)),
			};
	}
}

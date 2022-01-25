using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;

namespace Microsoft.Maui.Essentials
{
	public static partial class Screenshot
	{
		static bool PlatformIsCaptureSupported =>
			true;

		static Task<ScreenshotResult> PlatformCaptureAsync()
		{
			if (Platform.WindowManager?.DefaultDisplay?.Flags.HasFlag(DisplayFlags.Secure) == true)
				throw new UnauthorizedAccessException("Unable to take a screenshot of a secure window.");

			var view = Platform.GetCurrentActivity(true)?.Window?.DecorView?.RootView;
			if (view == null)
				throw new NullReferenceException("Unable to find the main window.");

			var result = new ScreenshotResult(view.Render());

			return Task.FromResult(result);
		}

		public static Bitmap Render(this View view)
		{
			var bitmap = RenderUsingCanvasDrawing(view);

			if (bitmap == null)
				bitmap = RenderUsingDrawingCache(view);

			return bitmap;
		}

		public static byte[] RenderAsJPEG(this View view, int quality = 100) => view?.RenderAsImage(Bitmap.CompressFormat.Jpeg, quality);

		public static byte[] RenderAsPNG(this View view, int quality = 100) => view?.RenderAsImage(Bitmap.CompressFormat.Png, quality);

		public static byte[] RenderAsImage(this View view, Bitmap.CompressFormat format, int quality = 100)
		{
			byte[] imageBytes = null;

			using (var bitmap = Render(view))
			{
				if (bitmap != null)
				{
					imageBytes = bitmap.AsImageBytes(format, quality);
					if (!bitmap.IsRecycled)
						bitmap.Recycle();
				}
			}

			return imageBytes;
		}

		public static byte[] AsImageBytes(this Bitmap bitmap, Bitmap.CompressFormat format, int quality = 100)
		{
			byte[] byteArray = null;
			using (var mem = new MemoryStream())
			{
				bitmap.Compress(format, quality, mem);
				byteArray = mem.ToArray();
			}

			return byteArray;
		}

		public static Bitmap RenderUsingCanvasDrawing(this View view)
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

		static Bitmap RenderUsingDrawingCache(this View view)
		{
#pragma warning disable CS0618 // Type or member is obsolete
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
#pragma warning restore CS0618 // Type or member is obsolete

		}
	}

	public partial class ScreenshotResult
	{
		readonly Bitmap bmp;

		internal ScreenshotResult(Bitmap bmp)
			: base()
		{
			this.bmp = bmp;

			Width = bmp.Width;
			Height = bmp.Height;
		}

		internal Task<Stream> PlatformOpenReadAsync(ScreenshotFormat format)
		{
			var f = format switch
			{
				ScreenshotFormat.Jpeg => Bitmap.CompressFormat.Jpeg,
				_ => Bitmap.CompressFormat.Png,
			};

			var result = new MemoryStream(bmp.AsImageBytes(f, 100)) as Stream;
			return Task.FromResult(result);
		}
	}
}

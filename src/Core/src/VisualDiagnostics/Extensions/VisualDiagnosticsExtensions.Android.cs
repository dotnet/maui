using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Views;
using GL = Android.Opengl;

namespace Microsoft.Maui
{
	public static class VisualDiagnosticsAndroidExtensions
	{
		internal static Task<byte[]?> RenderAsPng(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return Task.FromResult<byte[]?>(null);

			return Task.FromResult(nativeView.RenderAsPng());
		}

		static Bitmap? Render(View view)
		{
			var bitmap = RenderUsingCanvasDrawing(view);

			if (bitmap == null)
				bitmap = RenderUsingDrawingCache(view);

			return bitmap;
		}

		static byte[]? RenderAsPng(this View view)
		{
			byte[]? pngBytes = null;

			using (var bitmap = Render(view))
			{
				if (bitmap != null)
				{
					pngBytes = AsPNGBytes(bitmap);
					if (!bitmap.IsRecycled)
						bitmap.Recycle();
				}
			}

			return pngBytes;
		}

		static byte[]? AsPNGBytes(Bitmap bitmap)
		{
			byte[]? byteArray = null;
			using (var mem = new MemoryStream())
			{
				bitmap.Compress(Bitmap.CompressFormat.Png, 100, mem);
				byteArray = mem.ToArray();
			}

			return byteArray;
		}

		static Bitmap? RenderUsingCanvasDrawing(View view)
		{
			try
			{
				if (view == null || view.LayoutParameters == null || Bitmap.Config.Argb8888 == null)
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

		static Bitmap? RenderUsingDrawingCache(View view)
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
}

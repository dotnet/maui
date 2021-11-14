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
		internal static ViewTransform? GetViewTransform(this IView view)
		{
			var nativeView = view.GetNative(true);
			if (nativeView == null)
				return null;
			return GetViewTransform(nativeView);
		}

		internal static ViewTransform? GetViewTransform(View view)
		{
			if (view == null || view.Matrix == null || view.Matrix.IsIdentity)
				return null;

			var m = new float[16];
			var v = new float[16];
			var r = new float[16];

			GL.Matrix.SetIdentityM(r, 0);
			GL.Matrix.SetIdentityM(v, 0);
			GL.Matrix.SetIdentityM(m, 0);

			GL.Matrix.TranslateM(v, 0, view.Left, view.Top, 0);
			GL.Matrix.TranslateM(v, 0, view.PivotX, view.PivotY, 0);
			GL.Matrix.TranslateM(v, 0, view.TranslationX, view.TranslationY, 0);
			GL.Matrix.ScaleM(v, 0, view.ScaleX, view.ScaleY, 1);
			GL.Matrix.RotateM(v, 0, view.RotationX, 1, 0, 0);
			GL.Matrix.RotateM(v, 0, view.RotationY, 0, 1, 0);
			GL.Matrix.RotateM(m, 0, view.Rotation, 0, 0, 1);

			GL.Matrix.MultiplyMM(r, 0, v, 0, m, 0);
			GL.Matrix.TranslateM(m, 0, r, 0, -view.PivotX, -view.PivotY, 0);

			return new ViewTransform
			{
				M11 = m[0],
				M12 = m[1],
				M13 = m[2],
				M14 = m[3],
				M21 = m[4],
				M22 = m[5],
				M23 = m[6],
				M24 = m[7],
				M31 = m[8],
				M32 = m[9],
				M33 = m[10],
				M34 = m[11],
				OffsetX = m[12],
				OffsetY = m[13],
				OffsetZ = m[14],
				M44 = m[15]
			};
		}

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

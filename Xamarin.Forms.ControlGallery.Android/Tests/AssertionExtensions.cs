using AView = Android.Views.View;
using AColor = Android.Graphics.Color;
using Android.Graphics;
using System;
using NUnit.Framework;
using System.IO;

#if __ANDROID_29__
#else
using Android.Support.V7.Widget;
#endif

namespace Xamarin.Forms.ControlGallery.Android.Tests
{
	internal static class AssertionExtensions
	{
		public static string CreateColorAtPointError(this Bitmap bitmap, AColor expectedColor, int x, int y)
		{
			using (var ms = new MemoryStream())
			{
				bitmap.Compress(Bitmap.CompressFormat.Png, 0, ms);
				var imageAsString = Convert.ToBase64String(ms.ToArray());
				return $"Expected {expectedColor} at point {x},{y} in renderered view. This is what it looked like:<img>{imageAsString}</img>";
			}
		}

		public static AColor ColorAtPoint(this Bitmap bitmap, int x, int y)
		{
			int pixel = bitmap.GetPixel(x, y);

			int red = AColor.GetRedComponent(pixel);
			int blue = AColor.GetBlueComponent(pixel);
			int green = AColor.GetGreenComponent(pixel);

			return AColor.Rgb(red, green, blue);
		}

		public static Bitmap ToBitmap(this AView view)
		{
			var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);
			var canvas = new Canvas(bitmap);
			canvas.Save();
			canvas.Translate(0, 0);
			view.Draw(canvas);
			canvas.Restore();

			return bitmap;
		}

		public static Bitmap AssertColorAtPoint(this Bitmap bitmap, AColor expectedColor, int x, int y)
		{
			Assert.That(bitmap.ColorAtPoint(x, y), Is.EqualTo(expectedColor),
				() => bitmap.CreateColorAtPointError(expectedColor, x, y));

			return bitmap;
		}

		public static Bitmap AssertColorAtCenter(this Bitmap bitmap, AColor expectedColor) 
		{
			return bitmap.AssertColorAtPoint(expectedColor, bitmap.Width / 2, bitmap.Height / 2);
		}

		public static Bitmap AssertColorAtBottomLeft(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, 0);
		}

		public static Bitmap AssertColorAtBottomRight(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, bitmap.Width - 1, 0);
		}

		public static Bitmap AssertColorAtTopLeft(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, bitmap.Height - 1);
		}

		public static Bitmap AssertColorAtTopRight(this Bitmap bitmap, AColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, bitmap.Width - 1, bitmap.Height - 1);
		}

		public static Bitmap AssertColorAtPoint(this AView view, AColor expectedColor, int x, int y)
		{
			var bitmap = view.ToBitmap();
			Assert.That(bitmap.ColorAtPoint(x, y), Is.EqualTo(expectedColor),
				() => bitmap.CreateColorAtPointError(expectedColor, x, y));

			return bitmap;
		}

		public static Bitmap AssertColorAtCenter(this AView view, AColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static Bitmap AssertColorAtBottomLeft(this AView view, AColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static Bitmap AssertColorAtBottomRight(this AView view, AColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static Bitmap AssertColorAtTopLeft(this AView view, AColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static Bitmap AssertColorAtTopRight(this AView view, AColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtTopRight(expectedColor);
		}
	}
}
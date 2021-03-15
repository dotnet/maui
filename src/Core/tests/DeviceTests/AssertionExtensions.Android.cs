using System;
using System.IO;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Text;
using Android.Views;
using Android.Widget;
using Xunit;
using AColor = Android.Graphics.Color;
using AView = Android.Views.View;

namespace Microsoft.Maui.DeviceTests
{
	internal static partial class AssertionExtensions
	{
		public static string CreateColorAtPointError(this Bitmap bitmap, AColor expectedColor, int x, int y)
		{
			return CreateColorError(bitmap, $"Expected {expectedColor} at point {x},{y} in renderered view.");
		}

		public static string CreateColorError(this Bitmap bitmap, string message)
		{
			using (var ms = new MemoryStream())
			{
				bitmap.Compress(Bitmap.CompressFormat.Png, 0, ms);
				var imageAsString = Convert.ToBase64String(ms.ToArray());
				return $"{message}. This is what it looked like:<img>{imageAsString}</img>";
			}
		}

		public static AColor ColorAtPoint(this Bitmap bitmap, int x, int y, bool includeAlpha = false)
		{
			int pixel = bitmap.GetPixel(x, y);

			int red = AColor.GetRedComponent(pixel);
			int blue = AColor.GetBlueComponent(pixel);
			int green = AColor.GetGreenComponent(pixel);

			if (includeAlpha)
			{
				int alpha = AColor.GetAlphaComponent(pixel);
				return AColor.Argb(alpha, red, green, blue);
			}
			else
			{
				return AColor.Rgb(red, green, blue);
			}
		}

		public static async Task<Bitmap> ToBitmap(this AView view)
		{
			var layout = new FrameLayout(view.Context);
			layout.LayoutParameters = new FrameLayout.LayoutParams(500, 500);
			view.LayoutParameters = new FrameLayout.LayoutParams(FrameLayout.LayoutParams.WrapContent, FrameLayout.LayoutParams.WrapContent)
			{
				Gravity = GravityFlags.Center
			};

			var act = view.Context.GetActivity();
			var rootView = act.FindViewById<FrameLayout>(Android.Resource.Id.Content);

			rootView.AddView(layout);

			layout.AddView(view);
			layout.Measure(500, 500);
			layout.Layout(0, 0, 500, 500);

			await Task.Delay(100);

			var bitmap = Bitmap.CreateBitmap(view.Width, view.Height, Bitmap.Config.Argb8888);
			using (var canvas = new Canvas(bitmap))
			{
				view.Draw(canvas);
			}

			rootView.RemoveView(layout);

			return bitmap;
		}

		public static Bitmap AssertColorAtPoint(this Bitmap bitmap, AColor expectedColor, int x, int y)
		{
			Assert.Equal(bitmap.ColorAtPoint(x, y), expectedColor);

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

		public static Task<Bitmap> AssertContainsColor(this AView view, Maui.Color expectedColor) =>
			AssertContainsColor(view, expectedColor.ToNative());

		public static async Task<Bitmap> AssertContainsColor(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();

			for (int x = 1; x < view.Width; x++)
			{
				for (int y = 1; y < view.Height; y++)
				{
					if (bitmap.ColorAtPoint(x, y, true) == expectedColor)
					{
						return bitmap;
					}
				}
			}

			Assert.True(false, CreateColorError(bitmap, $"Color {expectedColor} not found."));
			return bitmap;
		}

		public static async Task<Bitmap> AssertColorAtPoint(this AView view, AColor expectedColor, int x, int y)
		{
			var bitmap = await view.ToBitmap();
			Assert.Equal(bitmap.ColorAtPoint(x, y), expectedColor);

			return bitmap;
		}

		public static async Task<Bitmap> AssertColorAtCenter(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtBottomLeft(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtBottomRight(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtTopLeft(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static async Task<Bitmap> AssertColorAtTopRight(this AView view, AColor expectedColor)
		{
			var bitmap = await view.ToBitmap();
			return bitmap.AssertColorAtTopRight(expectedColor);
		}

		public static TextUtils.TruncateAt ToNative(this LineBreakMode mode) =>
			mode switch
			{
				LineBreakMode.NoWrap => null,
				LineBreakMode.WordWrap => null,
				LineBreakMode.CharacterWrap => null,
				LineBreakMode.HeadTruncation => TextUtils.TruncateAt.Start,
				LineBreakMode.TailTruncation => TextUtils.TruncateAt.End,
				LineBreakMode.MiddleTruncation => TextUtils.TruncateAt.Middle,
				_ => throw new ArgumentOutOfRangeException(nameof(mode))
			};
	}
}
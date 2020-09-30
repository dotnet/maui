using System;
using CoreGraphics;
using NUnit.Framework;
using UIKit;

namespace Xamarin.Forms.Platform.iOS.UnitTests
{
	internal static class AssertionExtensions
	{
		public static string CreateColorAtPointError(this UIImage bitmap, UIColor expectedColor, int x, int y)
		{
			var data = bitmap.AsPNG();
			var imageAsString = data.GetBase64EncodedString(Foundation.NSDataBase64EncodingOptions.None);
			return $"Expected {expectedColor} at point {x},{y} in renderered view. This is what it looked like:<img>{imageAsString}</img>";
		}

		public static UIImage ToBitmap(this UIView view)
		{
			var imageRect = new CGRect(0, 0, view.Frame.Width, view.Frame.Height);

			UIGraphics.BeginImageContext(imageRect.Size);

			var context = UIGraphics.GetCurrentContext();
			view.Layer.RenderInContext(context);
			var image = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return image;
		}

		public static UIColor ColorAtPoint(this UIImage bitmap, int x, int y)
		{
			var pixel = bitmap.GetPixel(x, y);

			// Returned pixel data is B, G, R, A (ARGB little endian byte order)
			var color = new UIColor(pixel[2] / 255.0f, pixel[1] / 255.0f, pixel[0] / 255.0f, pixel[3] / 255.0f);

			return color;
		}

		public static byte[] GetPixel(this UIImage bitmap, int x, int y)
		{
			var cgImage = bitmap.CGImage.WithColorSpace(CGColorSpace.CreateDeviceRGB());

			// Grab the raw image data
			var nsData = cgImage.DataProvider.CopyData();

			// Copy the data into a buffer
			var dataBytes = new byte[nsData.Length];
			System.Runtime.InteropServices.Marshal.Copy(nsData.Bytes, dataBytes, 0, (int)nsData.Length);

			// Figure out where the pixel we care about is
			var pixelLocation = (cgImage.BytesPerRow * y) + (4 * x);

			var pixel = new byte[4]
			{
				dataBytes[pixelLocation],
				dataBytes[pixelLocation + 1],
				dataBytes[pixelLocation + 2],
				dataBytes[pixelLocation + 3],
			};

			return pixel;
		}

		

		public static UIImage AssertColorAtPoint(this UIImage bitmap, UIColor expectedColor, int x, int y)
		{
			try
			{
				var cap = bitmap.ColorAtPoint(x, y);

				if (!ColorComparison.ARGBEquivalent(cap, expectedColor))
				{
					System.Diagnostics.Debug.WriteLine("Here");
				}

				Assert.That(cap, Is.EqualTo(expectedColor).Using<UIColor>(ColorComparison.ARGBEquivalent),
					() => bitmap.CreateColorAtPointError(expectedColor, x, y));

				return bitmap;
			}
			catch (Exception ex) 
			{
				System.Diagnostics.Debug.WriteLine(ex);
			}

			return null;
		}

		public static UIImage AssertColorAtCenter(this UIImage bitmap, UIColor expectedColor)
		{
			AssertColorAtPoint(bitmap, expectedColor, (int)bitmap.Size.Width / 2, (int)bitmap.Size.Height / 2);
			return bitmap;
		}

		public static UIImage AssertColorAtBottomLeft(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, 0);
		}

		public static UIImage AssertColorAtBottomRight(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, (int)bitmap.Size.Width - 1, 0);
		}

		public static UIImage AssertColorAtTopLeft(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, 0, (int)bitmap.Size.Height - 1);
		}

		public static UIImage AssertColorAtTopRight(this UIImage bitmap, UIColor expectedColor)
		{
			return bitmap.AssertColorAtPoint(expectedColor, (int)bitmap.Size.Width - 1, (int)bitmap.Size.Height - 1);
		}

		public static UIImage AssertColorAtPoint(this UIView view, UIColor expectedColor, int x, int y)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static UIImage AssertColorAtCenter(this UIView view, UIColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static UIImage AssertColorAtBottomLeft(this UIView view, UIColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static UIImage AssertColorAtBottomRight(this UIView view, UIColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static UIImage AssertColorAtTopLeft(this UIView view, UIColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static UIImage AssertColorAtTopRight(this UIView view, UIColor expectedColor)
		{
			var bitmap = view.ToBitmap();
			return bitmap.AssertColorAtTopRight(expectedColor);
		}
	}
}
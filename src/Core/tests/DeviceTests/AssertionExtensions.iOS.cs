using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using CoreGraphics;
using Foundation;
using UIKit;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.Maui.DeviceTests
{
	internal static partial class AssertionExtensions
	{
		public static string CreateColorAtPointError(this UIImage bitmap, UIColor expectedColor, int x, int y)
		{
			var data = bitmap.AsPNG();
			var imageAsString = data.GetBase64EncodedString(Foundation.NSDataBase64EncodingOptions.None);
			return $"Expected {expectedColor} at point {x},{y} in renderered view. This is what it looked like:<img>{imageAsString}</img>";
		}

		public static string CreateColorError(this UIImage bitmap, string message)
		{
			var data = bitmap.AsPNG();
			var imageAsString = data.GetBase64EncodedString(Foundation.NSDataBase64EncodingOptions.None);
			return $"{message}. This is what it looked like:<img>{imageAsString}</img>";
		}

		// TODO: attach this view to the UI if anything breaks
		public static Task AttachAndRun(this UIView view, Action action)
		{
			action();
			return Task.CompletedTask;
		}

		public static Task<UIImage> ToUIImage(this UIView view)
		{
			if (view.Superview is WrapperView wrapper)
				view = wrapper;

			var imageRect = new CGRect(0, 0, view.Frame.Width, view.Frame.Height);

			UIGraphics.BeginImageContext(imageRect.Size);

			var context = UIGraphics.GetCurrentContext();
			view.Layer.RenderInContext(context);
			var image = UIGraphics.GetImageFromCurrentImageContext();

			UIGraphics.EndImageContext();

			return Task.FromResult(image);
		}

		public static UIColor ColorAtPoint(this UIImage bitmap, int x, int y)
		{
			var pixel = bitmap.GetPixel(x, y);

			var color = new UIColor(
				pixel[0] / 255.0f,
				pixel[1] / 255.0f,
				pixel[2] / 255.0f,
				pixel[3] / 255.0f);

			return color;
		}

		public static byte[] GetPixel(this UIImage bitmap, int x, int y)
		{
			var cgImage = bitmap.CGImage;
			var width = cgImage.Width;
			var height = cgImage.Height;
			var colorSpace = CGColorSpace.CreateDeviceRGB();
			var bitsPerComponent = 8;
			var bytesPerRow = 4 * width;
			var componentCount = 4;

			var dataBytes = new byte[width * height * componentCount];

			using var context = new CGBitmapContext(
				dataBytes,
				width, height,
				bitsPerComponent, bytesPerRow,
				colorSpace,
				CGBitmapFlags.ByteOrder32Big | CGBitmapFlags.PremultipliedLast);

			context.DrawImage(new CGRect(0, 0, width, height), cgImage);

			var pixelLocation = (bytesPerRow * y) + componentCount * x;

			var pixel = new byte[]
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
			var cap = bitmap.ColorAtPoint(x, y);

			if (!ColorComparison.ARGBEquivalent(cap, expectedColor))
				Assert.Equal(cap, expectedColor, new ColorComparison());

			return bitmap;
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

		public static async Task<UIImage> AssertColorAtPoint(this UIView view, UIColor expectedColor, int x, int y)
		{
			var bitmap = await view.ToUIImage();
			return bitmap.AssertColorAtPoint(expectedColor, x, y);
		}

		public static async Task<UIImage> AssertColorAtCenter(this UIView view, UIColor expectedColor)
		{
			var bitmap = await view.ToUIImage();
			return bitmap.AssertColorAtCenter(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtBottomLeft(this UIView view, UIColor expectedColor)
		{
			var bitmap = await view.ToUIImage();
			return bitmap.AssertColorAtBottomLeft(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtBottomRight(this UIView view, UIColor expectedColor)
		{
			var bitmap = await view.ToUIImage();
			return bitmap.AssertColorAtBottomRight(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtTopLeft(this UIView view, UIColor expectedColor)
		{
			var bitmap = await view.ToUIImage();
			return bitmap.AssertColorAtTopLeft(expectedColor);
		}

		public static async Task<UIImage> AssertColorAtTopRight(this UIView view, UIColor expectedColor)
		{
			var bitmap = await view.ToUIImage();
			return bitmap.AssertColorAtTopRight(expectedColor);
		}

		public static async Task<UIImage> AssertContainsColor(this UIView view, UIColor expectedColor)
		{
			var bitmap = await view.ToUIImage();
			return bitmap.AssertContainsColor(expectedColor);
		}

		public static Task<UIImage> AssertContainsColor(this UIView view, Microsoft.Maui.Graphics.Color expectedColor) =>
			AssertContainsColor(view, expectedColor.ToNative());

		public static UIImage AssertContainsColor(this UIImage bitmap, UIColor expectedColor)
		{
			for (int x = 0; x < bitmap.Size.Width; x++)
			{
				for (int y = 0; y < bitmap.Size.Height; y++)
				{
					if (ColorComparison.ARGBEquivalent(bitmap.ColorAtPoint(x, y), expectedColor))
					{
						return bitmap;
					}
				}
			}

			Assert.True(false, CreateColorError(bitmap, $"Color {expectedColor} not found."));
			return bitmap;
		}

		public static UILineBreakMode ToNative(this LineBreakMode mode) =>
		mode switch
		{
			LineBreakMode.NoWrap => UILineBreakMode.Clip,
			LineBreakMode.WordWrap => UILineBreakMode.WordWrap,
			LineBreakMode.CharacterWrap => UILineBreakMode.CharacterWrap,
			LineBreakMode.HeadTruncation => UILineBreakMode.HeadTruncation,
			LineBreakMode.TailTruncation => UILineBreakMode.TailTruncation,
			LineBreakMode.MiddleTruncation => UILineBreakMode.MiddleTruncation,
			_ => throw new ArgumentOutOfRangeException(nameof(mode))
		};

		public static double GetCharacterSpacing(this NSAttributedString text)
		{
			if (text == null)
				return 0;

			var value = text.GetAttribute(UIStringAttributeKey.KerningAdjustment, 0, out var range);
			if (value == null)
				return 0;

			Assert.Equal(0, range.Location);
			Assert.Equal(text.Length, range.Length);

			var kerning = Assert.IsType<NSNumber>(value);

			return kerning.DoubleValue;
		}

		public static void AssertHasUnderline(this NSAttributedString attributedString)
		{
			var value = attributedString.GetAttribute(UIStringAttributeKey.UnderlineStyle, 0, out var range);

			if (value == null)
			{
				throw new XunitException("Label does not have the UnderlineStyle attribute");
			}
		}
	}
}
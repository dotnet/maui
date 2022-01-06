using System;
using CoreGraphics;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Controls.Platform.Compatibility
{
	public static class ColorExtensions
	{
		internal static readonly UIColor Black = UIColor.Black;
		internal static readonly UIColor SeventyPercentGrey = new UIColor(0.7f, 0.7f, 0.7f, 1);

		internal static UIColor LabelColor
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.LabelColor;

				return UIColor.Black;
			}
		}

		internal static UIColor PlaceholderColor
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.PlaceholderTextColor;

				return SeventyPercentGrey;
			}
		}

		internal static UIColor SecondaryLabelColor
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SecondaryLabelColor;

				return new Color(.32f, .4f, .57f).ToUIColor();
			}
		}

		internal static UIColor BackgroundColor
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SystemBackgroundColor;

				return UIColor.White;
			}
		}

		internal static UIColor SeparatorColor
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SeparatorColor;

				return UIColor.Gray;
			}
		}

		internal static UIColor OpaqueSeparatorColor
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.OpaqueSeparatorColor;

				return UIColor.Black;
			}
		}

		internal static UIColor GroupedBackground
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SystemGroupedBackgroundColor;

				return new UIColor(247f / 255f, 247f / 255f, 247f / 255f, 1);
			}
		}

		internal static UIColor AccentColor
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SystemBlueColor;

				return Color.FromRgba(50, 79, 133, 255).ToUIColor();
			}
		}

		internal static UIColor Red
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SystemRedColor;

				return UIColor.FromRGBA(255, 0, 0, 255);
			}
		}

		internal static UIColor Gray
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SystemGrayColor;

				return UIColor.Gray;
			}
		}

		internal static UIColor LightGray
		{
			get
			{
				if (NativeVersion.IsAtLeast(13))
					return UIColor.SystemGray2Color;
				return UIColor.LightGray;

			}
		}

		public static UIColor FromPatternImageFromBundle(string bgImage)
		{
			var image = UIImage.FromBundle(bgImage);
			if (image == null)
				return UIColor.White;

			return UIColor.FromPatternImage(image);
		}

		public static UIColor ToUIColor(this Color color)
		{
			return new UIColor(color.Red, color.Green, color.Blue, color.Alpha);
		}

		public static UIColor ToUIColor(this Color color, Color defaultColor)
			=> color?.ToUIColor() ?? defaultColor?.ToUIColor();

		public static UIColor ToUIColor(this Color color, UIColor defaultColor)
			=> color?.ToUIColor() ?? defaultColor;
	}

	public static class PointExtensions
	{
		public static Point ToPoint(this PointF point)
		{
			return new Point(point.X, point.Y);
		}

		public static PointF ToPointF(this Point point)
		{
			return new PointF(point.X, point.Y);
		}
	}

	public static class SizeExtensions
	{
		public static SizeF ToSizeF(this Size size)
		{
			return new SizeF((float)size.Width, (float)size.Height);
		}
	}

	public static class RectangleExtensions
	{
		public static Rectangle ToRectangle(this RectangleF rect)
		{
			return new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static RectangleF ToRectangleF(this Rectangle rect)
		{
			return new RectangleF((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);
		}
	}
}
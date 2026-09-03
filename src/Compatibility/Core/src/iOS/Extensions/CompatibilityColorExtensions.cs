using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Platform;
using ObjCRuntime;
using UIKit;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	// It's easier if this name is different than Maui.Platform.ColorExtensions
	public static class CompatibilityColorExtensions
	{
		public static UIColor FromPatternImageFromBundle(string bgImage)
		{
			var image = UIImage.FromBundle(bgImage);
			if (image == null)
				return UIColor.White;

			return UIColor.FromPatternImage(image);
		}

		[Obsolete("Use the Color.ToPlatform extension method instead")]
		public static UIColor ToUIColor(this Color color)
		{
			return color.ToPlatform();
		}

		[Obsolete("Use the Color.ToPlatform extension method instead")]
		public static UIColor ToUIColor(this Color color, Color defaultColor)
			=> color?.ToPlatform() ?? defaultColor?.ToUIColor();

		[Obsolete("Use the Color.ToPlatform extension method instead")]
		public static UIColor ToUIColor(this Color color, UIColor defaultColor)
			=> color?.ToPlatform() ?? defaultColor;
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
		public static Rect ToRectangle(this RectangleF rect)
		{
			return new Rect(rect.X, rect.Y, rect.Width, rect.Height);
		}

		public static RectangleF ToRectangleF(this Rect rect)
		{
			return new RectangleF((nfloat)rect.X, (nfloat)rect.Y, (nfloat)rect.Width, (nfloat)rect.Height);
		}
	}
}
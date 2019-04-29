using System;
using CoreGraphics;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
#if __MOBILE__
using UIKit;
namespace Xamarin.Forms.Platform.iOS
#else
using AppKit;
using UIColor = AppKit.NSColor;

namespace Xamarin.Forms.Platform.MacOS
#endif
{
	public static class ColorExtensions
	{
#if __MOBILE__
		internal static readonly UIColor Black = UIColor.Black;
		internal static readonly UIColor SeventyPercentGrey = new UIColor(0.7f, 0.7f, 0.7f, 1);
#else
		internal static readonly NSColor Black = NSColor.Black;
		internal static readonly NSColor SeventyPercentGrey = NSColor.FromRgba(0.7f, 0.7f, 0.7f, 1);
#endif

		public static CGColor ToCGColor(this Color color)
		{
#if __MOBILE__
			return color.ToUIColor().CGColor;
#else
            return color.ToNSColor().CGColor;
#endif
		}

		public static Color ToColor(this UIColor color)
		{
			nfloat red;
			nfloat green;
			nfloat blue;
			nfloat alpha;
#if __MOBILE__
			color.GetRGBA(out red, out green, out blue, out alpha);
#else
			color.GetRgba(out red, out green, out blue, out alpha);
#endif
			return new Color(red, green, blue, alpha);
		}

#if __MOBILE__
		public static UIColor ToUIColor(this Color color)
		{
			return new UIColor((float)color.R, (float)color.G, (float)color.B, (float)color.A);
		}

		public static UIColor ToUIColor(this Color color, Color defaultColor)
		{
			if (color.IsDefault)
				return defaultColor.ToUIColor();

			return color.ToUIColor();
		}

		public static UIColor ToUIColor(this Color color, UIColor defaultColor)
		{
			if (color.IsDefault)
				return defaultColor;

			return color.ToUIColor();
		}
#else
		public static NSColor ToNSColor(this Color color)
		{
			return NSColor.FromRgba((float)color.R, (float)color.G, (float)color.B, (float)color.A);
		}

		public static NSColor ToNSColor(this Color color, Color defaultColor)
		{
			if (color.IsDefault)
				return defaultColor.ToNSColor();

			return color.ToNSColor();
		}

		public static NSColor ToNSColor(this Color color, NSColor defaultColor)
		{
			if (color.IsDefault)
				return defaultColor;

			return color.ToNSColor();
		}
#endif
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
using System;
using CoreGraphics;
using PointF = CoreGraphics.CGPoint;
using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using Microsoft.Maui.Graphics;
#if __MOBILE__
using ObjCRuntime;
using UIKit;
namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
#else
using AppKit;
using UIColor = AppKit.NSColor;

namespace Microsoft.Maui.Controls.Compatibility.Platform.MacOS
#endif
{
	public static class ColorExtensions
	{
#if __MOBILE__
		internal static readonly UIColor Black = UIColor.Black;
		internal static readonly UIColor SeventyPercentGrey = new UIColor(0.7f, 0.7f, 0.7f, 1);

		internal static UIColor LabelColor
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.Label;

				return UIColor.Black;
			}
		}

		internal static UIColor PlaceholderColor
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.PlaceholderText;

				return SeventyPercentGrey;
			}
		}

		internal static UIColor SecondaryLabelColor
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.SecondaryLabel;

				return new Color(.32f, .4f, .57f).ToUIColor();
			}
		}

		internal static UIColor BackgroundColor
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.SystemBackground;

				return UIColor.White;
			}
		}

		internal static UIColor SeparatorColor
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.Separator;

				return UIColor.Gray;
			}
		}

		internal static UIColor OpaqueSeparatorColor
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.OpaqueSeparator;

				return UIColor.Black;
			}
		}

		internal static UIColor GroupedBackground
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.SystemGroupedBackground;

				return new UIColor(247f / 255f, 247f / 255f, 247f / 255f, 1);
			}
		}

		internal static UIColor AccentColor
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.SystemBlue;

				return Color.FromRgba(50, 79, 133, 255).ToUIColor();
			}
		}

		internal static UIColor Red
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.SystemRed;

				return UIColor.FromRGBA(255, 0, 0, 255);
			}
		}

		internal static UIColor Gray
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.SystemGray;

				return UIColor.Gray;
			}
		}

		internal static UIColor LightGray
		{
			get
			{
				if (Forms.IsiOS13OrNewer)
					return UIColor.SystemGray2;
				return UIColor.LightGray;

			}
		}

#else
		internal static readonly NSColor Black = NSColor.Black;
		internal static readonly NSColor SeventyPercentGrey = NSColor.FromRgba(0.7f, 0.7f, 0.7f, 1);
		internal static readonly NSColor AccentColor = Color.FromRgba(50, 79, 133, 255).ToNSColor();

		internal static NSColor LabelColor
		{
			get
			{
				if (Forms.IsMojaveOrNewer)
					return NSColor.LabelColor;

				return NSColor.Black.UsingColorSpace("NSCalibratedRGBColorSpace");
			}
		}

		internal static NSColor TextColor
		{
			get
			{
				if (Forms.IsMojaveOrNewer)
					return NSColor.Text;

				return NSColor.Black;
			}
		}

		internal static NSColor ControlBackgroundColor
		{
			get
			{
				if (Forms.IsMojaveOrNewer)
					return NSColor.ControlBackground;

				return NSColor.Clear;
			}
		}

		internal static NSColor WindowBackgroundColor
		{
			get
			{
				if (Forms.IsMojaveOrNewer)
					return NSColor.WindowBackground;

				return NSColor.White;
			}
		}

		internal static NSColor PlaceholderColor
		{
			get
			{
				if (Forms.IsMojaveOrNewer)
					return NSColor.PlaceholderTextColor;

				return SeventyPercentGrey;
			}
		}

		internal static NSColor SecondaryLabelColor
		{
			get
			{
				if (Forms.IsMojaveOrNewer)
					return NSColor.SecondaryLabelColor;

				return new Color(.32, .4, .57).ToNSColor();
			}
		}

		internal static NSColor GroupedBackground
		{
			get
			{
				if (Forms.IsMojaveOrNewer)
					return NSColor.SystemGrayColor;

				return Color.LightGray.ToNSColor();
			}
		}
#endif

#if __MOBILE__
		public static UIColor FromPatternImageFromBundle(string bgImage)
		{
			var image = UIImage.FromBundle(bgImage);
			if (image == null)
				return UIColor.White;

			return UIColor.FromPatternImage(image);
		}
#endif

#if __MACOS__
		public static Color ToColor(this UIColor color, NSColorSpace colorSpace)
		{
			var convertedColor = color.UsingColorSpace(colorSpace);

			return convertedColor.ToColor();
		}
#endif

#if __MOBILE__
		public static UIColor ToUIColor(this Color color)
		{
			return new UIColor(color.Red, color.Green, color.Blue, color.Alpha);
		}

		public static UIColor ToUIColor(this Color color, Color defaultColor)
			=> color?.ToUIColor() ?? defaultColor?.ToUIColor();

		public static UIColor ToUIColor(this Color color, UIColor defaultColor)
			=> color?.ToUIColor() ?? defaultColor;
#else
		public static NSColor ToNSColor(this Color color)
		{
			return NSColor.FromRgba((float)color.Red, (float)color.Green, (float)color.Blue, (float)color.Alpha);
		}

		public static NSColor ToNSColor(this Color color, Color defaultColor)
			=> color?.ToNSColor() ?? defaultColor?.ToNSColor();

		public static NSColor ToNSColor(this Color color, NSColor defaultColor)
			=> color?.ToNSColor() ?? defaultColor;
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
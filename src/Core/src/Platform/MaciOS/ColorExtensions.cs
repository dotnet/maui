using CoreGraphics;
using Microsoft.Maui;
using System;

#if __IOS__
using UIKit;
using UIColor = UIKit.UIColor;
#else
using AppKit;
using UIColor = AppKit.NSColor;
#endif

namespace Microsoft.Maui
{
	public static class ColorExtensions
	{
#if __IOS__
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

				return new Color(.32, .4, .57).ToNative();
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

				return Color.FromRgba(50, 79, 133, 255).ToNative();
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

#else
		internal static readonly UIColor Black = UIColor.Black;
		internal static readonly UIColor SeventyPercentGrey = UIColor.FromRgba(0.7f, 0.7f, 0.7f, 1);
		internal static readonly UIColor LabelColor = UIColor.Black.UsingColorSpace(NSColorSpace.DeviceRGB);
		internal static readonly UIColor AccentColor = Color.FromRgba(50, 79, 133, 255).ToNative();
#endif

		public static CGColor ToCGColor(this Color color)
		{
			return color.ToNative().CGColor;
		}

#if __IOS__
		public static UIColor FromPatternImageFromBundle(string bgImage)
		{
			var image = UIImage.FromBundle(bgImage);
			if (image == null)
				return UIColor.White;

			return UIColor.FromPatternImage(image);
		}
#endif

		public static Color ToColor(this UIColor color)
		{
			nfloat red;
			nfloat green;
			nfloat blue;
			nfloat alpha;

			if (color == null)
				return Color.Default;
#if __IOS__
			color.GetRGBA(out red, out green, out blue, out alpha);
#else
			if (color.Type == NSColorType.Catalog)
				color = UIColor.Black.UsingColorSpace(NSColorSpace.DeviceRGB);
			// TODO:
			//throw new InvalidOperationException("Cannot convert a NSColorType.Catalog color without specifying the color space, use the overload to specify an NSColorSpace");

			color.GetRgba(out red, out green, out blue, out alpha);
#endif
			return new Color(red, green, blue, alpha);
		}

#if __MACOS__
		public static Color ToColor(this UIColor color, NSColorSpace colorSpace)
		{
			var convertedColor = color.UsingColorSpace(colorSpace);

			return convertedColor.ToColor();
		}
#endif

#if __IOS__
		public static UIColor ToNative(this Color color)
		{
			return new UIColor((float)color.R, (float)color.G, (float)color.B, (float)color.A);
		}

		public static UIColor ToNative(this Color color, Color defaultColor)
		{
			if (color.IsDefault)
				return defaultColor.ToNative();

			return color.ToNative();
		}

		public static UIColor ToNative(this Color color, UIColor defaultColor)
		{
			if (color.IsDefault)
				return defaultColor;

			return color.ToNative();
		}
#else
		public static NSColor ToNative(this Color color)
		{
			return NSColor.FromRgba((float)color.R, (float)color.G, (float)color.B, (float)color.A);
		}

		public static NSColor ToNative(this Color color, Color defaultColor)
		{
			if (color.IsDefault)
				return defaultColor.ToNative();

			return color.ToNative();
		}

		public static NSColor ToNative(this Color color, NSColor defaultColor)
		{
			if (color.IsDefault)
				return defaultColor;

			return color.ToNative();
		}
#endif
	}
}
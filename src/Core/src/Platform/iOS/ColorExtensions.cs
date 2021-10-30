using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using UIKit;
using UIColor = UIKit.UIColor;

namespace Microsoft.Maui
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

				return new Color(.32f, .4f, .57f).ToNative();
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

		public static CGColor ToCGColor(this Color color)
		{
			return color.ToNative().CGColor;
		}

		public static UIColor FromPatternImageFromBundle(string bgImage)
		{
			var image = UIImage.FromBundle(bgImage);
			if (image == null)
				return UIColor.White;

			return UIColor.FromPatternImage(image);
		}

		public static Color? ToColor(this UIColor color)
		{
			if (color == null)
				return null;

			color.GetRGBA(out nfloat red, out nfloat green, out nfloat blue, out nfloat alpha);

			return new Color((float)red, (float)green, (float)blue, (float)alpha);
		}

		public static UIColor ToNative(this Color color)
		{
			return new UIColor(color.Red, color.Green, color.Blue, color.Alpha);
		}

		public static UIColor? ToNative(this Color? color, Color? defaultColor)
			=> color?.ToNative() ?? defaultColor?.ToNative();

		public static UIColor ToNative(this Color? color, UIColor defaultColor)
			=> color?.ToNative() ?? defaultColor;
	}
}
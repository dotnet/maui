using System;
using CoreGraphics;
using Microsoft.Maui.Graphics;
using ObjCRuntime;
using UIKit;
using UIColor = UIKit.UIColor;

namespace Microsoft.Maui.Platform
{
	public static class ColorExtensions
	{
		internal static readonly UIColor Black = UIColor.Black;
		internal static readonly UIColor SeventyPercentGrey = new UIColor(0.7f, 0.7f, 0.7f, 1);

		internal static UIColor LabelColor
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.Label;

				return UIColor.Black;
			}
		}

		internal static UIColor PlaceholderColor
		{
			get
			{

				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.PlaceholderText;

				return SeventyPercentGrey;
			}
		}

		internal static UIColor SecondaryLabelColor
		{
			get
			{

				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.SecondaryLabel;

				return new Color(.32f, .4f, .57f).ToPlatform();
			}
		}

		internal static UIColor BackgroundColor
		{
			get
			{

				if (OperatingSystem.IsIOSVersionAtLeast(13))
					return UIColor.SystemBackground;

				return UIColor.White;
			}
		}

		internal static UIColor SeparatorColor
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.Separator;

				return UIColor.Gray;
			}
		}

		internal static UIColor OpaqueSeparatorColor
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.OpaqueSeparator;

				return UIColor.Black;
			}
		}

		internal static UIColor GroupedBackground
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13))
					return UIColor.SystemGroupedBackground;

				return new UIColor(247f / 255f, 247f / 255f, 247f / 255f, 1);
			}
		}

		internal static UIColor AccentColor
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.SystemBlue;

				return Color.FromRgba(50, 79, 133, 255).ToPlatform();
			}
		}

		internal static UIColor Red
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.SystemRed;

				return UIColor.FromRGBA(255, 0, 0, 255);
			}
		}

		internal static UIColor Gray
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13))
					return UIColor.SystemGray;

				return UIColor.Gray;
			}
		}

		internal static UIColor LightGray
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13))
					return UIColor.SystemGray2;

				return UIColor.LightGray;
			}
		}

		public static CGColor ToCGColor(this Color color)
		{
			return color.ToPlatform().CGColor;
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

		public static UIColor ToPlatform(this Color color)
		{
			return new UIColor(color.Red, color.Green, color.Blue, color.Alpha);
		}

		public static UIColor? ToPlatform(this Color? color, Color? defaultColor)
			=> color?.ToPlatform() ?? defaultColor?.ToPlatform();

		public static UIColor ToPlatform(this Color? color, UIColor defaultColor)
			=> color?.ToPlatform() ?? defaultColor;

		internal static bool AreEqual(UIColor a, UIColor b)
		{
			a.GetRGBA(out nfloat aRed, out nfloat aGreen, out nfloat aBlue, out nfloat aAlpha);
			b.GetRGBA(out nfloat bRed, out nfloat bGreen, out nfloat bBlue, out nfloat bAlpha);

			var redMatches = aRed == bRed;
			var greenMatches = aGreen == bGreen;
			var blueMatches = aBlue == bBlue;
			var alphaMatches = aAlpha == bAlpha;

			return redMatches && greenMatches && blueMatches && alphaMatches;
		}
	}
}
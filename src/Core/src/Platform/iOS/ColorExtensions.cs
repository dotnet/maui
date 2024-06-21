using System;
using System.Runtime.Versioning;
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
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return Label;

				return UIColor.Black;
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		[SupportedOSPlatform("tvos13.0")]
		static UIColor Label => UIColor.Label;

		internal static UIColor PlaceholderColor
		{
			get
			{

				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return PlaceholderText;

				return SeventyPercentGrey;
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		[SupportedOSPlatform("tvos13.0")]
		static UIColor PlaceholderText => UIColor.Label;

		internal static UIColor SecondaryLabelColor
		{
			get
			{

				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return SecondaryLabel;

				return new Color(.32f, .4f, .57f).ToPlatform();
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		[SupportedOSPlatform("tvos13.0")]
		static UIColor SecondaryLabel => UIColor.Label;
		
		internal static UIColor BackgroundColor
		{
			get
			{

				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return SystemBackground;

				return UIColor.White;
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static UIColor SystemBackground => UIColor.SystemBackground;

		internal static UIColor SeparatorColor
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return SeparatorColorImpl;

				return UIColor.Gray;
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		[SupportedOSPlatform("tvos13.0")]
		static UIColor SeparatorColorImpl => UIColor.Separator;
		
		internal static UIColor OpaqueSeparatorColor
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsTvOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return OpaqueSeparator;

				return UIColor.Black;
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		[SupportedOSPlatform("tvos13.0")]
		static UIColor OpaqueSeparator => UIColor.Separator;

		internal static UIColor GroupedBackground
		{
			get
			{
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return SystemGroupedBackground;

				return new UIColor(247f / 255f, 247f / 255f, 247f / 255f, 1);
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static UIColor SystemGroupedBackground => UIColor.SystemGroupedBackground;

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
				if (OperatingSystem.IsIOSVersionAtLeast(13) || OperatingSystem.IsMacCatalystVersionAtLeast(13, 1))
					return SystemGray2;

				return UIColor.LightGray;
			}
		}

		[SupportedOSPlatform("ios13.0")]
		[SupportedOSPlatform("maccatalyst13.1")]
		static UIColor SystemGray2 => UIColor.SystemGray2;


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
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
			get => UIColor.Label;
		}

		internal static UIColor PlaceholderColor
		{
			get => UIColor.PlaceholderText;
		}

		internal static UIColor SecondaryLabelColor
		{
			get => UIColor.SecondaryLabel;
		}

		internal static UIColor BackgroundColor
		{
			get => UIColor.SystemBackground;
		}

		internal static UIColor SeparatorColor
		{
			get => UIColor.Separator;
		}

		internal static UIColor OpaqueSeparatorColor
		{
			get => UIColor.OpaqueSeparator;
		}

		internal static UIColor GroupedBackground
		{
			get => UIColor.SystemGroupedBackground;
		}

		internal static UIColor AccentColor
		{
			get => UIColor.SystemBlue;
		}

		internal static UIColor Red
		{
			get => UIColor.SystemRed;
		}

		internal static UIColor Gray
		{
			get => UIColor.SystemGray;
		}

		internal static UIColor LightGray
		{
			get => UIColor.SystemGray2;
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
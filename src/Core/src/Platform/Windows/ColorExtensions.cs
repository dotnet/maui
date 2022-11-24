#nullable enable
using System;
using Microsoft.UI;
using Windows.UI;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Platform
{
	public static class ColorExtensions
	{
		public static global::Windows.UI.Color GetContrastingColor(this global::Windows.UI.Color color)
		{
			var nThreshold = 105;
			int bgLuminance = Convert.ToInt32(color.R * 0.2 + color.G * 0.7 + color.B * 0.1);

			global::Windows.UI.Color contrastingColor = 255 - bgLuminance < nThreshold ? Colors.Black : Colors.White;
			return contrastingColor;
		}

		public static Graphics.Color ToColor(this global::Windows.UI.Color color)
		{
			return Graphics.Color.FromRgba(color.R, color.G, color.B, color.A);
		}

		public static Graphics.Color ToColor(this WSolidColorBrush solidColorBrush)
		{
			return solidColorBrush.Color.ToColor();
		}

		public static bool IsDefault(this Graphics.Color? color)
		{
			return color == null;
		}

		public static WBrush ToPlatform(this Graphics.Color color)
		{
			return new WSolidColorBrush(color.ToWindowsColor());
		}

		public static global::Windows.UI.Color ToWindowsColor(this Graphics.Color color)
		{
			return global::Windows.UI.Color.FromArgb((byte)(color.Alpha * 255), (byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));
		}
	}
}
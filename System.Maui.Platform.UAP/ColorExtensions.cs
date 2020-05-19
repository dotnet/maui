using System;
using global::Windows.UI;
using global::Windows.UI.Xaml.Media;

namespace System.Maui.Platform.UWP
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

		public static Color ToFormsColor(this global::Windows.UI.Color color)
		{
			return Color.FromRgba(color.R, color.G, color.B, color.A);
		}

		public static Color ToFormsColor(this SolidColorBrush solidColorBrush)
		{
			return solidColorBrush.Color.ToFormsColor();
		}

		public static Brush ToBrush(this Color color)
		{
			return new SolidColorBrush(color.ToWindowsColor());
		}

		public static global::Windows.UI.Color ToWindowsColor(this Color color)
		{
			return global::Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
		}
	}
}

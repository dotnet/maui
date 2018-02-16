using System;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class ColorExtensions
	{
		public static Windows.UI.Color GetContrastingColor(this Windows.UI.Color color)
		{
			var nThreshold = 105;
			int bgLuminance = Convert.ToInt32(color.R * 0.2 + color.G * 0.7 + color.B * 0.1);

			Windows.UI.Color contrastingColor = 255 - bgLuminance < nThreshold ? Colors.Black : Colors.White;
			return contrastingColor;
		}

		public static Color ToFormsColor(this Windows.UI.Color color)
		{
			return Color.FromRgba(color.R, color.G, color.B, color.A);
		}

		public static Color ToFormsColor(this SolidColorBrush solidColorBrush)
		{
			return solidColorBrush.Color.ToFormsColor();
		}
	}
}
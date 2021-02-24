using System;
using Windows.UI;
using Microsoft.UI;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class ColorExtensions
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

		public static Color ToFormsColor(this WSolidColorBrush solidColorBrush)
		{
			return solidColorBrush.Color.ToFormsColor();
		}

		public static WBrush ToBrush(this Color color)
		{
			return new WSolidColorBrush(color.ToWindowsColor());
		}

		public static Windows.UI.Color ToWindowsColor(this Color color)
		{
			return Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
		}
	}
}

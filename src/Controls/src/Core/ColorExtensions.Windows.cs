using System;
using Windows.UI;
using Microsoft.UI;
using Microsoft.Maui.Graphics;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Platform
{
	public static partial class ColorExtensions
	{
		public static Windows.UI.Color GetContrastingColor(this Windows.UI.Color color)
		{
			var nThreshold = 105;
			int bgLuminance = Convert.ToInt32(color.R * 0.2 + color.G * 0.7 + color.B * 0.1);

			Windows.UI.Color contrastingColor = 255 - bgLuminance < nThreshold ? UI.Colors.Black : UI.Colors.White;
			return contrastingColor;
		}

		public static WBrush ToBrush(this Graphics.Color color)
		{
			return new WSolidColorBrush(color.ToWindowsColor());
		}

		public static Windows.UI.Color ToWindowsColor(this Graphics.Color color)
		{
			return Windows.UI.Color.FromArgb((byte)(color.Alpha * 255), (byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));
		}
	}
}

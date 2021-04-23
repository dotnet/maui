using System;
using Windows.UI;
using Microsoft.UI;
using Microsoft.Maui.Graphics;
using WBrush = Microsoft.UI.Xaml.Media.Brush;
using WSolidColorBrush = Microsoft.UI.Xaml.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public static class ColorExtensions
	{
		public static Graphics.Color ToFormsColor(this Windows.UI.Color color)
		{
			return Graphics.Color.FromRgba(color.R, color.G, color.B, color.A);
		}

		public static Graphics.Color ToFormsColor(this WSolidColorBrush solidColorBrush)
		{
			return solidColorBrush.Color.ToFormsColor();
		}
	}
}

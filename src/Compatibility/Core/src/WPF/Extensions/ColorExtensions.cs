using WBrush = System.Windows.Media.Brush;
using WSolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
{
	public static class ColorExtensions
	{
		public static WBrush ToBrush(this Color color)
		{
			return new WSolidColorBrush(color.ToMediaColor());
		}

		public static System.Windows.Media.Color ToMediaColor(this Color color)
		{
			return System.Windows.Media.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
		}
	}
}
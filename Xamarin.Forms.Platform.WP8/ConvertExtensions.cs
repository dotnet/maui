using System.Windows.Media;

namespace Xamarin.Forms.Platform.WinPhone
{
	internal static class ConvertExtensions
	{
		public static Brush ToBrush(this Color color)
		{
			return new SolidColorBrush(color.ToMediaColor());
		}

		public static System.Windows.Media.Color ToMediaColor(this Color color)
		{
			return System.Windows.Media.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
		}
	}
}
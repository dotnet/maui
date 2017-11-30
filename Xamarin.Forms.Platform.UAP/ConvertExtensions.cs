using Windows.UI.Xaml.Media;

namespace Xamarin.Forms.Platform.UWP
{
	internal static class ConvertExtensions
	{
		public static Brush ToBrush(this Color color)
		{
			return new SolidColorBrush(color.ToWindowsColor());
		}

		public static Windows.UI.Color ToWindowsColor(this Color color)
		{
			return Windows.UI.Color.FromArgb((byte)(color.A * 255), (byte)(color.R * 255), (byte)(color.G * 255), (byte)(color.B * 255));
		}
	}
}
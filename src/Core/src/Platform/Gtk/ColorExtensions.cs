using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	public static class ColorExtensions
	{
		public static Gdk.RGBA ToNative(this Color color)
		{
			string hex = color.ToHex();
			Gdk.RGBA nativeColor = new Gdk.RGBA();
			nativeColor.Parse(hex);

			return nativeColor;
		}

		public static Color ToColor(this Gdk.Color color, float opacity = 255)
		{
			return new Color(color.Red, color.Green, color.Blue, opacity);
		}
	}
}
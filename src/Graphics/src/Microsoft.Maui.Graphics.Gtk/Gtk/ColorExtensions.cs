using Gdk;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public static class ColorExtensions {

		public static Gdk.RGBA ToGdkRgba(this Color color)
			=> color == default ? default : new RGBA {Red = color.Red, Green = color.Green, Blue = color.Blue, Alpha = color.Alpha};

		public static Gdk.Color ToGdkColor(this Gdk.RGBA color)
			=> new Gdk.Color((byte) (color.Red * 255), (byte) (color.Green * 255), (byte) (color.Blue * 255));

		public static Color ToColor(this Gdk.Color color, float opacity = 255)
			=> new Color(color.Red, color.Green, color.Blue, opacity);

		public static Color ToColor(this Gdk.RGBA color)
			=> new Color((byte) (color.Red * 255), (byte) (color.Green * 255), (byte) (color.Blue * 255), (byte) (color.Alpha * 255));

		public static Cairo.Color ToCairoColor(this Color color)
			=> color == default ? default : new Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha);

		public static Cairo.Color ToCairoColor(this Gdk.RGBA color)
			=> new Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha);

		public static Gdk.Color ToGdkColor(this Color color)
			=> color == default ? default : new Gdk.Color((byte) (color.Red * 255), (byte) (color.Green * 255), (byte) (color.Blue * 255));

		public static Color ToColor(this Gdk.Color color)
			=> new Color(color.Red / (float) ushort.MaxValue, color.Green / (float) ushort.MaxValue, color.Blue / (float) ushort.MaxValue);

	}

}

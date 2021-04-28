namespace Microsoft.Maui.Graphics.Native.Gtk
{

	public static class ColorExtensions
	{

		private static Gdk.RGBA ToNative_(this Color color)
		{
			var hex = color.ToHex();
			var nativeColor = new Gdk.RGBA();
			nativeColor.Parse(hex);

			return nativeColor;
		}

		public static Gdk.RGBA ToGdkRgba(this Color color)
			=> color == default ? default : new() { Red = color.Red, Green = color.Green, Blue = color.Blue, Alpha = color.Alpha };

		public static Gdk.Color ToGdkColor(this Gdk.RGBA color)
			=> new((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));

		public static Color ToColor(this Gdk.Color color, float opacity = 255)
			=> new(color.Red, color.Green, color.Blue, opacity);

		public static Color ToColor(this Gdk.RGBA color)
			=> new((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255), (byte)(color.Alpha * 255));

		public static Cairo.Color ToCairoColor(this Color color)
			=> color == default ? default : new(color.Red, color.Green, color.Blue, color.Alpha);

		public static Cairo.Color ToCairoColor(this Gdk.RGBA color)
			=> new(color.Red, color.Green, color.Blue, color.Alpha);

		public static Gdk.Color ToGdkColor(this Color color)
			=> color == default ? default : new((byte)(color.Red * 255), (byte)(color.Green * 255), (byte)(color.Blue * 255));

		public static Color ToColor(this Gdk.Color color)
			=> new((float)color.Red / (float)ushort.MaxValue, (float)color.Green / (float)ushort.MaxValue, (float)color.Blue / (float)ushort.MaxValue);

	}

}
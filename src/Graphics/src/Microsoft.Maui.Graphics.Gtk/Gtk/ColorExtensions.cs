using System;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public static class ColorExtensions {

		public static Gdk.RGBA ToGdkRgba(this Color color)
			=> color == default ? default : new Gdk.RGBA {Red = color.Red, Green = color.Green, Blue = color.Blue, Alpha = color.Alpha};

		public static Color ToColor(this Gdk.RGBA color)
			=> new Color((float) color.Red, (float) color.Green, (float) color.Blue, (float) color.Alpha);

		public static Cairo.Color ToCairoColor(this Color color)
			=> color == default ? default : new Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha);

		public static Cairo.Color ToCairoColor(this Gdk.RGBA color)
			=> new Cairo.Color(color.Red, color.Green, color.Blue, color.Alpha);

		public static Color ToColor(this Cairo.Color color)
			=> new Color((float) color.R, (float) color.G, (float) color.B, (float) color.A);

		// https://developer.gnome.org/gdk3/stable/gdk3-Colors.html#GdkColor
		// When working with cairo, it is often more convenient to use a GdkRGBA instead, and GdkColor has been deprecated in favor of GdkRGBA.
		[Obsolete("GdkColor has been deprecated in favor of GdkRGBA")]
		public static Gdk.Color ToGdkColor(this Color color)
			=> color == default ? default : new Gdk.Color((byte) (color.Red * 255), (byte) (color.Green * 255), (byte) (color.Blue * 255));

		[Obsolete("GdkColor has been deprecated in favor of GdkRGBA")]
		public static Gdk.Color ToGdkColor(this Gdk.RGBA color)
			=> new Gdk.Color((byte) (color.Red * 255), (byte) (color.Green * 255), (byte) (color.Blue * 255));

		[Obsolete("GdkColor has been deprecated in favor of GdkRGBA")]
		public static Color ToColor(this Gdk.Color color)
			=> new Color(color.Red / (float) ushort.MaxValue, color.Green / (float) ushort.MaxValue, color.Blue / (float) ushort.MaxValue);

	}

}

using System;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

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

	}

}

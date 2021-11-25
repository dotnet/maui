using System;

namespace Microsoft.Maui.Graphics.Platform.Gtk {

	public static class GraphicsExtensions {

		public static Rectangle ToRectangle(this Gdk.Rectangle it)
			=> new Rectangle(it.X, it.Y, it.Width, it.Height);

		public static RectangleF ToRectangleF(this Gdk.Rectangle it)
			=> new RectangleF(it.X, it.Y, it.Width, it.Height);

		public static Gdk.Rectangle ToNative(this Rectangle it)
			=> new Gdk.Rectangle((int) it.X, (int) it.Y, (int) it.Width, (int) it.Height);

		public static Gdk.Rectangle ToNative(this RectangleF it)
			=> new Gdk.Rectangle((int) it.X, (int) it.Y, (int) it.Width, (int) it.Height);

		public static Point ToPoint(this Gdk.Point it)
			=> new Point(it.X, it.Y);

		public static PointF ToPointF(this Gdk.Point it)
			=> new PointF(it.X, it.Y);

		public static PointF ToPointF(this Cairo.PointD it)
			=> new PointF((float) it.X, (float) it.Y);

		public static Gdk.Point ToNative(this Point it)
			=> new Gdk.Point((int) it.X, (int) it.Y);

		public static Gdk.Point ToNative(this PointF it)
			=> new Gdk.Point((int) it.X, (int) it.Y);

		public static Size ToSize(this Gdk.Size it)
			=> new Size(it.Width, it.Height);

		public static SizeF ToSizeF(this Gdk.Size it)
			=> new SizeF(it.Width, it.Height);

		public static Gdk.Size ToNative(this Size it)
			=> new Gdk.Size((int) it.Width, (int) it.Height);

		public static Gdk.Size ToNative(this SizeF it)
			=> new Gdk.Size((int) it.Width, (int) it.Height);

		public static double ScaledFromPango(this int it)
			=> Math.Ceiling(it / Pango.Scale.PangoScale);

		public static float ScaledFromPangoF(this int it)
			=> (float) Math.Ceiling(it / Pango.Scale.PangoScale);

		public static int ScaledToPango(this double it)
			=> (int) Math.Ceiling(it * Pango.Scale.PangoScale);

		public static int ScaledToPango(this float it)
			=> (int) Math.Ceiling(it * Pango.Scale.PangoScale);

		public static int ScaledToPango(this int it)
			=> (int) Math.Ceiling(it * Pango.Scale.PangoScale);

	}

}

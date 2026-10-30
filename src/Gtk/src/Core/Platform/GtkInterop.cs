using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Gtk.Platform
{
	/// <summary>
	/// Small conversions between MAUI and GTK/GSK/Graphene value types.
	/// </summary>
	public static class GtkInterop
	{
		/// <summary>Converts a MAUI <see cref="Color"/> to a <see cref="Gdk.RGBA"/>.</summary>
		public static Gdk.RGBA ToGdkRgba(this Color color) => new()
		{
			Red = color.Red,
			Green = color.Green,
			Blue = color.Blue,
			Alpha = color.Alpha,
		};

		/// <summary>Converts a <see cref="Gdk.RGBA"/> to a MAUI <see cref="Color"/>.</summary>
		public static Color ToMauiColor(this Gdk.RGBA rgba) => new(rgba.Red, rgba.Green, rgba.Blue, rgba.Alpha);

		/// <summary>Formats a colour as a CSS <c>rgba(...)</c> expression.</summary>
		public static string ToCss(this Color color) =>
			FormattableString.Invariant($"rgba({(int)Math.Round(color.Red * 255)},{(int)Math.Round(color.Green * 255)},{(int)Math.Round(color.Blue * 255)},{color.Alpha:0.###})");

		/// <summary>Creates a <see cref="Graphene.Point"/>.</summary>
		public static Graphene.Point ToGraphene(double x, double y)
		{
			var point = new Graphene.Point();
			point.Init((float)x, (float)y);
			return point;
		}

		/// <summary>Creates a <see cref="Graphene.Rect"/>.</summary>
		public static Graphene.Rect ToGrapheneRect(double x, double y, double width, double height)
		{
			var rect = Graphene.Rect.Alloc();
			rect.Init((float)x, (float)y, (float)width, (float)height);
			return rect;
		}

		/// <summary>Converts a MAUI constraint (infinity means unconstrained) to a GTK <c>for_size</c> (-1 means unconstrained).</summary>
		public static int ToForSize(double constraint) =>
			double.IsInfinity(constraint) || double.IsNaN(constraint) || constraint < 0
				? -1
				: (int)Math.Min(int.MaxValue / 2, Math.Ceiling(constraint));

		/// <summary>Rounds a MAUI length up to whole GTK pixels.</summary>
		public static int ToPixels(double value) =>
			double.IsNaN(value) || value <= 0 ? 0 : (int)Math.Min(int.MaxValue / 2, Math.Ceiling(value - 0.0001));
	}
}

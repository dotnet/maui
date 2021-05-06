using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Graphics.Native.Gtk {

	public static class FontExtensions {

		/// <summary>
		/// size in points
		/// <seealso cref="https://developer.gnome.org/pygtk/stable/class-pangofontdescription.html#method-pangofontdescription--set-size"/>
		/// the size of a font description is specified in pango units.
		/// There are <see cref="Pango.Scale.PangoScale"/> pango units in one device unit (the device unit is a point for font sizes).
		/// </summary>
		/// <param name="it"></param>
		/// <returns></returns>
		public static double GetSize(this Pango.FontDescription it)
			=> it.Size.ScaledFromPango();

		public static Pango.Style ToPangoStyle(this FontStyleType it) => it switch {
			FontStyleType.Oblique => Pango.Style.Oblique,
			FontStyleType.Italic => Pango.Style.Italic,
			_ => Pango.Style.Normal
		};

		public static FontStyleType ToFontStyleType(this Pango.Style it) => it switch {
			Pango.Style.Oblique => FontStyleType.Oblique,
			Pango.Style.Italic => FontStyleType.Italic,
			_ => FontStyleType.Normal
		};

		// enum Pango.Weight { Thin = 100, Ultralight = 200, Light = 300, Semilight = 350, Book = 380, Normal = 400, Medium = 500, Semibold = 600, Bold = 700, Ultrabold = 800, Heavy = 900, Ultraheavy = 1000,}

		public static Pango.Weight ToFontWeigth(int it) {
			static Pango.Weight Div(double v) => (Pango.Weight) ((int) v / 100 * 100);

			if (it < 100)
				return Pango.Weight.Thin;
			else if (it > 1000)
				return Pango.Weight.Ultraheavy;
			else if (it > 390 || it < 325)
				return Div(it);
			else if (it > 375)
				return Pango.Weight.Book;
			else if (it > 325)
				return Pango.Weight.Semilight;
			else
				return 0;

		}

		public static double ToFontWeigth(this Pango.Weight it)
			=> (int) it;

		public static Pango.FontDescription ToFontDescription(this IFontStyle it) =>
			new Pango.FontDescription {
				Style = it.StyleType.ToPangoStyle(),
				Weight = ToFontWeigth(it.Weight),
				Family = it.FontFamily?.Name,

			};

		public static IEnumerable<IFontStyle> GetAvailableFontStyles(this Pango.FontFamily _native) {
			return _native.Faces.Select(f => f.Describe().ToFontStyle());
		}

		public static IFontStyle ToFontStyle(this Pango.FontDescription it) =>
			new NativeFontStyle(it.Family, it.GetSize(), (int) it.Weight.ToFontWeigth(), it.Stretch, it.Style.ToFontStyleType());

		public static IFontFamily ToFontFamily(this Pango.FontFamily it) =>
			new NativeFontFamily(it.Name);

	};

}

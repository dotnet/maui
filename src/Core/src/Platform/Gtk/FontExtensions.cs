using System;
using System.Linq;
using Gtk;

namespace Microsoft.Maui
{

	public static class FontExtensions
	{

		public static Pango.FontDescription GetPangoFontDescription(this Widget it)
#pragma warning disable 612
			=> it.StyleContext.GetFont(it.StateFlags);
#pragma warning restore 612

		/// <summary>
		/// size in points
		/// <seealso cref="https://developer.gnome.org/pygtk/stable/class-pangofontdescription.html#method-pangofontdescription--set-size"/>
		/// the size of a font description is specified in pango units.
		/// There are <see cref="Pango.Scale.PangoScale"/> pango units in one device unit (the device unit is a point for font sizes).
		/// </summary>
		/// <param name="it"></param>
		/// <returns></returns>
		public static double GetSize(this Pango.FontDescription it)
			=> it.Size / Pango.Scale.PangoScale;

		public static Pango.FontFamily GetPangoFontFamily(this Widget it)
			=> it.FontMap.Families.First();

		// enum Pango.Style { Normal, Oblique,  Italic  }
		public static Pango.Style ToFontStyle(this FontSlant it) => it switch
		{
			FontSlant.Oblique => Pango.Style.Oblique, // ??
			FontSlant.Italic => Pango.Style.Italic,
			_ => Pango.Style.Normal
		};

		// enum Pango.Weight { Thin = 100, Ultralight = 200, Light = 300, Semilight = 350, Book = 380, Normal = 400, Medium = 500, Semibold = 600, Bold = 700, Ultrabold = 800, Heavy = 900, Ultraheavy = 1000,}
		public static Pango.Weight ToFontWeight(this FontWeight it) =>
			it switch
			{
				FontWeight.Bold => Pango.Weight.Bold,
				FontWeight.Regular => Pango.Weight.Normal,
				FontWeight.Thin => Pango.Weight.Thin,
				FontWeight.Ultralight => Pango.Weight.Ultralight,
				FontWeight.Light => Pango.Weight.Light,
				FontWeight.Medium => Pango.Weight.Medium,
				FontWeight.Semibold => Pango.Weight.Semibold,
				FontWeight.Heavy => Pango.Weight.Heavy,
				FontWeight.Black => Pango.Weight.Ultrabold,
				_ => Pango.Weight.Normal
			};

		// enum Pango.Stretch { UltraCondensed, ExtraCondensed, Condensed, SemiCondensed, Normal, SemiExpanded, Expanded, ExtraExpanded, UltraExpanded, }

		public static Pango.Stretch ToFontStretch(this Font it) => Pango.Stretch.Normal;

		public static Pango.FontDescription ToFontDescription(this Font it)
			=> new()
			{
				Family = it.FontFamily,
				Size = (int)(it.FontSize * Pango.Scale.PangoScale),
				Style = it.FontSlant.ToFontStyle(),
				Weight = it.Weight.ToFontWeight(),
				Stretch = it.ToFontStretch()
			};

		public static double GetFontHeigth(this Widget it, Pango.FontDescription? font = null)
		{
			font ??= it.GetPangoFontDescription();
			var metrics = it.PangoContext.GetMetrics(font, Pango.Language.Default);
			var pangoUnits = (metrics.Ascent + metrics.Descent) / Pango.Scale.PangoScale;

			var resolution = it.Screen.Resolution;
			var height = (pangoUnits * (resolution / 72.0));

			return height;
		}

	}

}
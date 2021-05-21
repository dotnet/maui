using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Maui.Graphics;
using Pango;

namespace Microsoft.Maui
{

	// see: https://developer.gnome.org/pygtk/stable/class-pangofontdescription.html
	/*
	  public enum Pango.Weight
	  {
	    Thin = 100, // 0x00000064
	    Ultralight = 200, // 0x000000C8
	    Light = 300, // 0x0000012C
	    Semilight = 350, // 0x0000015E
	    Book = 380, // 0x0000017C
	    Normal = 400, // 0x00000190
	    Medium = 500, // 0x000001F4
	    Semibold = 600, // 0x00000258
	    Bold = 700, // 0x000002BC
	    Ultrabold = 800, // 0x00000320
	    Heavy = 900, // 0x00000384
	    Ultraheavy = 1000, // 0x000003E8
	  }
	  
	  public enum Stretch
	  {
	    UltraCondensed,
	    ExtraCondensed,
	    Condensed,
	    SemiCondensed,
	    Normal,
	    SemiExpanded,
	    Expanded,
	    ExtraExpanded,
	    UltraExpanded,
	  }
  
  
    public enum Style { Normal, Oblique,  Italic  }
  
    public enum Variant
  {
    Normal,
    SmallCaps,
  }
  
    public enum Gravity
  {
    South,
    East,
    North,
    West,
    Auto,
  }
  
	 */

	public class FontManager : IFontManager
	{

		readonly IFontRegistrar _fontRegistrar;

		static Pango.Context? _systemContext;

		Pango.Context SystemContext => _systemContext ??= Gdk.PangoHelper.ContextGet();

		public FontManager(IFontRegistrar fontRegistrar)
		{
			_fontRegistrar = fontRegistrar;
		}

		FontDescription? _defaultFontFamily;

		public FontDescription DefaultFontFamily
		{
			get => _defaultFontFamily ??= GetFontFamily(default);
		}

		double? _defaultFontSize;

		public double DefaultFontSize => _defaultFontSize ??= DefaultFontFamily?.GetSize() ?? 0;

		public FontDescription GetFontFamily(Font font) => 
			font == default ? SystemContext.FontDescription : font.ToFontDescription();

		public double GetFontSize(Font font)
		{
			if (font.UseNamedSize)
				return GetFontSize(font.NamedSize);

			return font.FontSize;
		}

		public double GetFontSize(NamedSize namedSize)
		{
			// TODO: Hmm, maybe we need to revisit this, since we no longer support Windows Phone OR WinRT.
			// These are values pulled from the mapped sizes on Windows Phone, WinRT has no equivalent sizes, only intents.

			return namedSize switch
			{
				NamedSize.Default => DefaultFontSize,
				NamedSize.Micro => 15.667,
				NamedSize.Small => 18.667,
				NamedSize.Medium => 22.667,
				NamedSize.Large => 32,
				NamedSize.Body => 14,
				NamedSize.Caption => 12,
				NamedSize.Header => 46,
				NamedSize.Subtitle => 20,
				NamedSize.Title => 24,
				_ => throw new ArgumentOutOfRangeException(nameof(namedSize)),
			};
		}

		private IEnumerable<(Pango.FontFamily family, Pango.FontDescription description)> GetAvailableFamilyFaces(Pango.FontFamily family)
		{

			if (family != default)
			{
				foreach (var face in family.Faces)
					yield return (family, face.Describe());
			}

			yield break;
		}

		private FontDescription[] GetAvailableFontStyles()
		{
			var fontFamilies = SystemContext.FontMap?.Families.ToArray();

			var styles = new List<FontDescription>();

			if (fontFamilies != null)
			{
				styles.AddRange(fontFamilies.SelectMany(GetAvailableFamilyFaces).Select(font => font.description)
				   .OrderBy(d=>d.Family));
			}


			return styles.ToArray();
		}

	}

}
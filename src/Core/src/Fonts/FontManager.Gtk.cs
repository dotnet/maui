using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics;
using Pango;
using static Microsoft.Maui.GtkInterop.DllImportFontConfig;

namespace Microsoft.Maui
{

	// see: https://docs.gtk.org/Pango/struct.FontDescription.html
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

		public FontManager(IFontRegistrar fontRegistrar, ILogger<FontManager>? logger = null)
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
			return font.Size;
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
				   .OrderBy(d => d.Family));
			}

			return styles.ToArray();
		}
		
		internal static bool AddFontFile (string fontPath)
		{
			// Try to add font file to the current fontconfig configuration
			var result = FcConfigAppFontAddFile (System.IntPtr.Zero, fontPath);

			if (result)
			{
				_systemContext = null;
			}

			return result;
		}

	}

}
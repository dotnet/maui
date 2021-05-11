using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.Maui.Controls.Design
{
	public class NamedColorsDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public NamedColorsDesignTypeConverter()
		{ }

		protected override string[] KnownValues =>
			new[]
			{
				"AliceBlue",
				"AntiqueWhite",
				"Aqua",
				"Aquamarine",
				"Azure",
				"Beige",
				"Bisque",
				"Black",
				"BlanchedAlmond",
				"Blue",
				"BlueViolet",
				"Brown",
				"BurlyWood",
				"CadetBlue",
				"Chartreuse",
				"Chocolate",
				"Coral",
				"CornflowerBlue",
				"Cornsilk",
				"Crimson",
				"Cyan",
				"DarkBlue",
				"DarkCyan",
				"DarkGoldenrod",
				"DarkGray",
				"DarkGreen",
				"DarkGrey",
				"DarkKhaki",
				"DarkMagenta",
				"DarkOliveGreen",
				"DarkOrange",
				"DarkOrchid",
				"DarkRed",
				"DarkSalmon",
				"DarkSeaGreen",
				"DarkSlateBlue",
				"DarkSlateGray",
				"DarkSlateGrey",
				"DarkTurquoise",
				"DarkViolet",
				"DeepPink",
				"DeepSkyBlue",
				"DimGray",
				"DimGrey",
				"DodgerBlue",
				"Firebrick",
				"FloralWhite",
				"ForestGreen",
				"Fuchsia",
				"Gainsboro",
				"GhostWhite",
				"Gold",
				"Goldenrod",
				"Gray",
				"Green",
				"GreenYellow",
				"Grey",
				"Honeydew",
				"HotPink",
				"IndianRed",
				"Indigo",
				"Ivory",
				"Khaki",
				"Lavender",
				"LavenderBlush",
				"LawnGreen",
				"LemonChiffon",
				"LightBlue",
				"LightCoral",
				"LightCyan",
				"LightGoldenrodYellow",
				"LightGray",
				"LightGreen",
				"LightGrey",
				"LightPink",
				"LightSalmon",
				"LightSeaGreen",
				"LightSkyBlue",
				"LightSlateGray",
				"LightSlateGrey",
				"LightSteelBlue",
				"LightYellow",
				"Lime",
				"LimeGreen",
				"Linen",
				"Magenta",
				"Maroon",
				"MediumAquamarine",
				"MediumBlue",
				"MediumOrchid",
				"MediumPurple",
				"MediumSeaGreen",
				"MediumSlateBlue",
				"MediumSpringGreen",
				"MediumTurquoise",
				"MediumVioletRed",
				"MidnightBlue",
				"MintCream",
				"MistyRose",
				"Moccasin",
				"NavajoWhite",
				"Navy",
				"OldLace",
				"Olive",
				"OliveDrab",
				"Orange",
				"OrangeRed",
				"Orchid",
				"PaleGoldenrod",
				"PaleGreen",
				"PaleTurquoise",
				"PaleVioletRed",
				"PapayaWhip",
				"PeachPuff",
				"Peru",
				"Pink",
				"Plum",
				"PowderBlue",
				"Purple",
				"Red",
				"RosyBrown",
				"RoyalBlue",
				"SaddleBrown",
				"Salmon",
				"SandyBrown",
				"SeaGreen",
				"SeaShell",
				"Sienna",
				"Silver",
				"SkyBlue",
				"SlateBlue",
				"SlateGray",
				"SlateGrey",
				"Snow",
				"SpringGreen",
				"SteelBlue",
				"Tan",
				"Teal",
				"Thistle",
				"Tomato",
				"Transparent",
				"Turquoise",
				"Violet",
				"Wheat",
				"White",
				"WhiteSmoke",
				"Yellow",
				"YellowGreen"
			};

		// #000, #000fff, #000fff00 are all valid 
		const string RxColorHexPattern = @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}([0-9a-fA-F]{2})?)$";
		static readonly Lazy<Regex> RxColorHex = new(() => new Regex(RxColorHexPattern, RegexOptions.Compiled | RegexOptions.Singleline));

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			// Any named colors are ok
			if (KnownValues.Any(v => value?.ToString()?.Equals(v, StringComparison.Ordinal) ?? false))
				return true;

			// HTML Hex colors are also ok
			// Check if HTML Hex Color code, which is also valid
			return RxColorHex.Value.IsMatch(value?.ToString());
		}
	}
}

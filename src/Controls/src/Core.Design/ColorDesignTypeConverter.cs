using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Controls.Design
{
	/// <summary>
	/// Provides design-time type conversion for Color values.
	/// </summary>
	public class ColorDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ColorDesignTypeConverter"/> class.
		/// </summary>
		public ColorDesignTypeConverter()
		{ }

		private static readonly string[] knownValues =
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

		private static readonly HashSet<string> knowValuesSet;

		static ColorDesignTypeConverter()
		{
			knowValuesSet = new HashSet<string>(knownValues, StringComparer.OrdinalIgnoreCase);

			// Color.TryParse supports "default" (see ..\Graphics\Color.cs) as well as XAML parses used during build.
			knowValuesSet.Add("Default");
		}

		/// <inheritdoc/>
		protected override string[] KnownValues => knownValues;

		// #rgb, #rrggbb, #aarrggbb are all valid 
		const string RxColorHexPattern = @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}([0-9a-fA-F]{2})?)$";
		static readonly Lazy<Regex> RxColorHex = new(() => new Regex(RxColorHexPattern, RegexOptions.Compiled | RegexOptions.Singleline));

		// RGB		rgb(255,0,0), rgb(100%,0%,0%)					values in range 0-255 or 0%-100%
		// RGBA		rgba(255, 0, 0, 0.8), rgba(100%, 0%, 0%, 0.8)	opacity is 0.0-1.0
		// HSL		hsl(120, 100%, 50%)								h is 0-360, s and l are 0%-100%
		// HSLA		hsla(120, 100%, 50%, .8)						opacity is 0.0-1.0
		// HSV		hsv(120, 100%, 50%)								h is 0-360, s and v are 0%-100%
		// HSVA		hsva(120, 100%, 50%, .8)						opacity is 0.0-1.0
		const string RxFuncPattern = "^(?<func>rgba|argb|rgb|hsla|hsl|hsva|hsv)\\(((?<v>\\d%?),){2}((?<v>\\d%?)|(?<v>\\d%?),(?<v>\\d%?))\\);?$";
		static readonly Lazy<Regex> RxFuncExpr = new(() => new Regex(RxFuncPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline));

		/// <inheritdoc/>
		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			var str = value?.ToString();

			if (string.IsNullOrWhiteSpace(str))
				return false;

			// Any named colors are ok. Surrounding white spaces are ok.
			if (knowValuesSet.Contains(str.Trim()))
				return true;

			// Check for HEX Color string
			if (RxColorHex.Value.IsMatch(str))
				return true;

			var match = RxFuncExpr.Value.Match(str);

			var funcName = match?.Groups?["func"]?.Value;
			var funcValues = match?.Groups?["v"]?.Captures;

			if (!string.IsNullOrEmpty(funcName) && funcValues != null)
			{
				// ie: argb() needs 4 parameters:
				if (funcValues.Count == funcName.Length)
					return true;
			}

			return false;
		}
	}
}

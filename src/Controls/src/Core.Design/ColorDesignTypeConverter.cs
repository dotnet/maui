using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.RegularExpressions;

namespace Microsoft.Maui.Controls.Design
{
	public class ColorDesignTypeConverter : KnownValuesDesignTypeConverter
	{
		public ColorDesignTypeConverter()
		{ }

		private static readonly string[] knownValues =
			[
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
			];

		private static readonly HashSet<string> knowValuesSet;

		static ColorDesignTypeConverter()
		{
			knowValuesSet = new HashSet<string>(knownValues, StringComparer.OrdinalIgnoreCase);

			// Color.TryParse supports "default" (see ..\Graphics\Color.cs) as well as XAML parses used during build.
			knowValuesSet.Add("Default");
		}

		protected override string[] KnownValues => knownValues;

		static readonly Lazy<Regex> RxColorHex = new(() => RegexHelper.RxColorHex);
		static readonly Lazy<Regex> RxFuncExpr = new(() => RegexHelper.RxFunc);

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

	internal static partial class RegexHelper
	{
		static readonly ReadOnlySpan<char> pattern = @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}([0-9a-fA-F]{2})?)$";

#if NET7_0_OR_GREATER
		// #rgb, #rrggbb, #aarrggbb are all valid 
		[GeneratedRegex (pattern, RegexOptions.Singleline, matchTimeoutMilliseconds: 1000))]
		internal static partial Regex RxColorHex
		{
			get;
		}
#else
		internal static readonly Regex RxColorHex =
										new (
											pattern,
											RegexOptions.Compiled | RegexOptions.Singleline,		
											TimeSpan.FromMilliseconds(1000)							// against malicious input
											);
#endif
	}
}

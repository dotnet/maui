using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls.SourceGen.TypeConverters;

internal class ColorConverter : ISGTypeConverter
{
	private static readonly HashSet<string> KnownNamedColors = new(StringComparer.OrdinalIgnoreCase)
	{
		"AliceBlue", "AntiqueWhite", "Aqua", "Aquamarine", "Azure", "Beige", "Bisque", "Black",
		"BlanchedAlmond", "Blue", "BlueViolet", "Brown", "BurlyWood", "CadetBlue", "Chartreuse",
		"Chocolate", "Coral", "CornflowerBlue", "Cornsilk", "Crimson", "Cyan", "DarkBlue",
		"DarkCyan", "DarkGoldenrod", "DarkGray", "DarkGreen", "DarkGrey", "DarkKhaki",
		"DarkMagenta", "DarkOliveGreen", "DarkOrange", "DarkOrchid", "DarkRed", "DarkSalmon",
		"DarkSeaGreen", "DarkSlateBlue", "DarkSlateGray", "DarkSlateGrey", "DarkTurquoise",
		"DarkViolet", "DeepPink", "DeepSkyBlue", "DimGray", "DimGrey", "DodgerBlue", "Firebrick",
		"FloralWhite", "ForestGreen", "Fuchsia", "Gainsboro", "GhostWhite", "Gold", "Goldenrod",
		"Gray", "Green", "GreenYellow", "Grey", "Honeydew", "HotPink", "IndianRed", "Indigo",
		"Ivory", "Khaki", "Lavender", "LavenderBlush", "LawnGreen", "LemonChiffon", "LightBlue",
		"LightCoral", "LightCyan", "LightGoldenrodYellow", "LightGray", "LightGreen", "LightGrey",
		"LightPink", "LightSalmon", "LightSeaGreen", "LightSkyBlue", "LightSlateGray", "LightSlateGrey",
		"LightSteelBlue", "LightYellow", "Lime", "LimeGreen", "Linen", "Magenta", "Maroon",
		"MediumAquamarine", "MediumBlue", "MediumOrchid", "MediumPurple", "MediumSeaGreen",
		"MediumSlateBlue", "MediumSpringGreen", "MediumTurquoise", "MediumVioletRed", "MidnightBlue",
		"MintCream", "MistyRose", "Moccasin", "NavajoWhite", "Navy", "OldLace", "Olive", "OliveDrab",
		"Orange", "OrangeRed", "Orchid", "PaleGoldenrod", "PaleGreen", "PaleTurquoise", "PaleVioletRed",
		"PapayaWhip", "PeachPuff", "Peru", "Pink", "Plum", "PowderBlue", "Purple", "Red", "RosyBrown",
		"RoyalBlue", "SaddleBrown", "Salmon", "SandyBrown", "SeaGreen", "SeaShell", "Sienna", "Silver",
		"SkyBlue", "SlateBlue", "SlateGray", "SlateGrey", "Snow", "SpringGreen", "SteelBlue", "Tan",
		"Teal", "Thistle", "Tomato", "Transparent", "Turquoise", "Violet", "Wheat", "White",
		"WhiteSmoke", "Yellow", "YellowGreen"
	};

	// #rgb, #rrggbb, #aarrggbb are all valid 
	private const string RxColorHexPattern = @"^#([0-9a-fA-F]{3}|[0-9a-fA-F]{6}([0-9a-fA-F]{2})?)$";
	private static readonly Lazy<Regex> RxColorHex = new(() => new Regex(RxColorHexPattern, RegexOptions.Compiled | RegexOptions.Singleline));

	// RGB, RGBA, HSL, HSLA, HSV, HSVA function patterns
	private const string RxFuncPattern = "^(?<func>rgba|argb|rgb|hsla|hsl|hsva|hsv)\\(((?<v>\\d%?),){2}((?<v>\\d%?)|(?<v>\\d%?),(?<v>\\d%?))\\);?$";
	private static readonly Lazy<Regex> RxFuncExpr = new(() => new Regex(RxFuncPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.IgnorePatternWhitespace | RegexOptions.Singleline));

	public IEnumerable<string> SupportedTypes => new[] { "Color", "Microsoft.Maui.Graphics.Color" };

	public string Convert(string value, BaseNode node, ITypeSymbol toType, SourceGenContext context, LocalVariable? parentVar = null)
	{
		var xmlLineInfo = (IXmlLineInfo)node;
		if (!string.IsNullOrEmpty(value))
		{
			// Any named colors are ok. Surrounding white spaces are ok. Case insensitive.
			var actualColorName = KnownNamedColors.FirstOrDefault(c => string.Equals(c, value.Trim(), StringComparison.OrdinalIgnoreCase));
			if (actualColorName is not null)
			{
				var colorsType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Colors")!;
				return $"{colorsType.ToFQDisplayString()}.{actualColorName}";
			}

			// Check for HEX Color string
			if (RxColorHex.Value.IsMatch(value))
			{
				var colorType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!;
				return $"{colorType.ToFQDisplayString()}.FromArgb(\"{value}\")";
			}

			var match = RxFuncExpr.Value.Match(value);

			var funcName = match?.Groups?["func"]?.Value;
			var funcValues = match?.Groups?["v"]?.Captures;

			if (!string.IsNullOrEmpty(funcName) && funcValues is not null)
			{
				// ie: argb() needs 4 parameters:
				if (funcValues.Count == funcName?.Length)
				{
					var colorType = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!;
					return $"{colorType.ToFQDisplayString()}.Parse(\"{value}\")";
				}
			}

			// As a last resort, try Color.Parse() for any other valid color formats
			var colorType2 = context.Compilation.GetTypeByMetadataName("Microsoft.Maui.Graphics.Color")!;
			return $"{colorType2.ToFQDisplayString()}.Parse(\"{value}\")";
		}

		context.ReportConversionFailed(xmlLineInfo, value, toType, Descriptors.ConversionFailed);
		return "default";
	}
}
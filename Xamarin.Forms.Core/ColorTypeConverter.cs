using System;
using System.Globalization;
using System.Linq;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	[Xaml.ProvideCompiled("Xamarin.Forms.Core.XamlC.ColorTypeConverter")]
	[Xaml.TypeConversion(typeof(Color))]
	public class ColorTypeConverter : TypeConverter
	{
		// Supported inputs
		// HEX		#rgb, #argb, #rrggbb, #aarrggbb
		// RGB		rgb(255,0,0), rgb(100%,0%,0%)					values in range 0-255 or 0%-100%
		// RGBA		rgba(255, 0, 0, 0.8), rgba(100%, 0%, 0%, 0.8)	opacity is 0.0-1.0
		// HSL		hsl(120, 100%, 50%)								h is 0-360, s and l are 0%-100%
		// HSLA		hsla(120, 100%, 50%, .8)						opacity is 0.0-1.0
		// HSV		hsv(120, 100%, 50%)								h is 0-360, s and v are 0%-100%
		// HSVA		hsva(120, 100%, 50%, .8)						opacity is 0.0-1.0
		// Predefined color											case insensitive
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				value = value.Trim();
				if (value.StartsWith("#", StringComparison.Ordinal))
					return Color.FromHex(value);

				if (value.StartsWith("rgba", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var quad = value.Substring(op + 1, cp - op - 1).Split(',');
					if (quad.Length != 4)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var r = ParseColorValue(quad[0], 255, acceptPercent: true);
					var g = ParseColorValue(quad[1], 255, acceptPercent: true);
					var b = ParseColorValue(quad[2], 255, acceptPercent: true);
					var a = ParseOpacity(quad[3]);
					return new Color(r, g, b, a);
				}

				if (value.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var triplet = value.Substring(op + 1, cp - op - 1).Split(',');
					if (triplet.Length != 3)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var r = ParseColorValue(triplet[0], 255, acceptPercent: true);
					var g = ParseColorValue(triplet[1], 255, acceptPercent: true);
					var b = ParseColorValue(triplet[2], 255, acceptPercent: true);
					return new Color(r, g, b);
				}

				if (value.StartsWith("hsla", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var quad = value.Substring(op + 1, cp - op - 1).Split(',');
					if (quad.Length != 4)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var h = ParseColorValue(quad[0], 360, acceptPercent: false);
					var s = ParseColorValue(quad[1], 100, acceptPercent: true);
					var l = ParseColorValue(quad[2], 100, acceptPercent: true);
					var a = ParseOpacity(quad[3]);
					return Color.FromHsla(h, s, l, a);
				}

				if (value.StartsWith("hsl", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var triplet = value.Substring(op + 1, cp - op - 1).Split(',');
					if (triplet.Length != 3)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var h = ParseColorValue(triplet[0], 360, acceptPercent: false);
					var s = ParseColorValue(triplet[1], 100, acceptPercent: true);
					var l = ParseColorValue(triplet[2], 100, acceptPercent: true);
					return Color.FromHsla(h, s, l);
				}

				if (value.StartsWith("hsva", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var quad = value.Substring(op + 1, cp - op - 1).Split(',');
					if (quad.Length != 4)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var h = ParseColorValue(quad[0], 360, acceptPercent: false);
					var s = ParseColorValue(quad[1], 100, acceptPercent: true);
					var v = ParseColorValue(quad[2], 100, acceptPercent: true);
					var a = ParseOpacity(quad[3]);
					return Color.FromHsva(h, s, v, a);
				}

				if (value.StartsWith("hsv", StringComparison.OrdinalIgnoreCase))
				{
					var op = value.IndexOf('(');
					var cp = value.LastIndexOf(')');
					if (op < 0 || cp < 0 || cp < op)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var triplet = value.Substring(op + 1, cp - op - 1).Split(',');
					if (triplet.Length != 3)
						throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
					var h = ParseColorValue(triplet[0], 360, acceptPercent: false);
					var s = ParseColorValue(triplet[1], 100, acceptPercent: true);
					var v = ParseColorValue(triplet[2], 100, acceptPercent: true);
					return Color.FromHsv(h, s, v);
				}

				string[] parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts[0] == "Color"))
				{
					string color = parts[parts.Length - 1];
					switch (color.ToLowerInvariant())
					{
						case "default":
							return Color.Default;
						case "accent":
							return Color.Accent;
						case "aliceblue":
							return Color.AliceBlue;
						case "antiquewhite":
							return Color.AntiqueWhite;
						case "aqua":
							return Color.Aqua;
						case "aquamarine":
							return Color.Aquamarine;
						case "azure":
							return Color.Azure;
						case "beige":
							return Color.Beige;
						case "bisque":
							return Color.Bisque;
						case "black":
							return Color.Black;
						case "blanchedalmond":
							return Color.BlanchedAlmond;
						case "blue":
							return Color.Blue;
						case "blueViolet":
							return Color.BlueViolet;
						case "brown":
							return Color.Brown;
						case "burlywood":
							return Color.BurlyWood;
						case "cadetblue":
							return Color.CadetBlue;
						case "chartreuse":
							return Color.Chartreuse;
						case "chocolate":
							return Color.Chocolate;
						case "coral":
							return Color.Coral;
						case "cornflowerblue":
							return Color.CornflowerBlue;
						case "cornsilk":
							return Color.Cornsilk;
						case "crimson":
							return Color.Crimson;
						case "cyan":
							return Color.Cyan;
						case "darkblue":
							return Color.DarkBlue;
						case "darkcyan":
							return Color.DarkCyan;
						case "darkgoldenrod":
							return Color.DarkGoldenrod;
						case "darkgray":
							return Color.DarkGray;
						case "darkgreen":
							return Color.DarkGreen;
						case "darkkhaki":
							return Color.DarkKhaki;
						case "darkmagenta":
							return Color.DarkMagenta;
						case "darkolivegreen":
							return Color.DarkOliveGreen;
						case "darkorange":
							return Color.DarkOrange;
						case "darkorchid":
							return Color.DarkOrchid;
						case "darkred":
							return Color.DarkRed;
						case "darksalmon":
							return Color.DarkSalmon;
						case "darkseagreen":
							return Color.DarkSeaGreen;
						case "darkslateblue":
							return Color.DarkSlateBlue;
						case "darkslategray":
							return Color.DarkSlateGray;
						case "darkturquoise":
							return Color.DarkTurquoise;
						case "darkviolet":
							return Color.DarkViolet;
						case "deeppink":
							return Color.DeepPink;
						case "deepskyblue":
							return Color.DeepSkyBlue;
						case "dimgray":
							return Color.DimGray;
						case "dodgerblue":
							return Color.DodgerBlue;
						case "firebrick":
							return Color.Firebrick;
						case "floralwhite":
							return Color.FloralWhite;
						case "forestgreen":
							return Color.ForestGreen;
						case "fuchsia":
							return Color.Fuchsia;
						case "gainsboro":
							return Color.Gainsboro;
						case "ghostwhite":
							return Color.GhostWhite;
						case "gold":
							return Color.Gold;
						case "goldenrod":
							return Color.Goldenrod;
						case "gray":
							return Color.Gray;
						case "green":
							return Color.Green;
						case "greenyellow":
							return Color.GreenYellow;
						case "honeydew":
							return Color.Honeydew;
						case "hotpink":
							return Color.HotPink;
						case "indianred":
							return Color.IndianRed;
						case "indigo":
							return Color.Indigo;
						case "ivory":
							return Color.Ivory;
						case "khaki":
							return Color.Khaki;
						case "lavender":
							return Color.Lavender;
						case "lavenderblush":
							return Color.LavenderBlush;
						case "lawngreen":
							return Color.LawnGreen;
						case "lemonchiffon":
							return Color.LemonChiffon;
						case "lightblue":
							return Color.LightBlue;
						case "lightcoral":
							return Color.LightCoral;
						case "lightcyan":
							return Color.LightCyan;
						case "lightgoldenrodyellow":
							return Color.LightGoldenrodYellow;
						case "lightgrey":
						case "lightgray":
							return Color.LightGray;
						case "lightgreen":
							return Color.LightGreen;
						case "lightpink":
							return Color.LightPink;
						case "lightsalmon":
							return Color.LightSalmon;
						case "lightseagreen":
							return Color.LightSeaGreen;
						case "lightskyblue":
							return Color.LightSkyBlue;
						case "lightslategray":
							return Color.LightSlateGray;
						case "lightsteelblue":
							return Color.LightSteelBlue;
						case "lightyellow":
							return Color.LightYellow;
						case "lime":
							return Color.Lime;
						case "limegreen":
							return Color.LimeGreen;
						case "linen":
							return Color.Linen;
						case "magenta":
							return Color.Magenta;
						case "maroon":
							return Color.Maroon;
						case "mediumaquamarine":
							return Color.MediumAquamarine;
						case "mediumblue":
							return Color.MediumBlue;
						case "mediumorchid":
							return Color.MediumOrchid;
						case "mediumpurple":
							return Color.MediumPurple;
						case "mediumseagreen":
							return Color.MediumSeaGreen;
						case "mediumslateblue":
							return Color.MediumSlateBlue;
						case "mediumspringgreen":
							return Color.MediumSpringGreen;
						case "mediumturquoise":
							return Color.MediumTurquoise;
						case "mediumvioletred":
							return Color.MediumVioletRed;
						case "midnightblue":
							return Color.MidnightBlue;
						case "mintcream":
							return Color.MintCream;
						case "mistyrose":
							return Color.MistyRose;
						case "moccasin":
							return Color.Moccasin;
						case "navajowhite":
							return Color.NavajoWhite;
						case "navy":
							return Color.Navy;
						case "oldlace":
							return Color.OldLace;
						case "olive":
							return Color.Olive;
						case "olivedrab":
							return Color.OliveDrab;
						case "orange":
							return Color.Orange;
						case "orangered":
							return Color.OrangeRed;
						case "orchid":
							return Color.Orchid;
						case "palegoldenrod":
							return Color.PaleGoldenrod;
						case "palegreen":
							return Color.PaleGreen;
						case "paleturquoise":
							return Color.PaleTurquoise;
						case "palevioletred":
							return Color.PaleVioletRed;
						case "papayawhip":
							return Color.PapayaWhip;
						case "peachpuff":
							return Color.PeachPuff;
						case "peru":
							return Color.Peru;
						case "pink":
							return Color.Pink;
						case "plum":
							return Color.Plum;
						case "powderblue":
							return Color.PowderBlue;
						case "purple":
							return Color.Purple;
						case "red":
							return Color.Red;
						case "rosybrown":
							return Color.RosyBrown;
						case "royalblue":
							return Color.RoyalBlue;
						case "saddlebrown":
							return Color.SaddleBrown;
						case "salmon":
							return Color.Salmon;
						case "sandybrown":
							return Color.SandyBrown;
						case "seagreen":
							return Color.SeaGreen;
						case "seashell":
							return Color.SeaShell;
						case "sienna":
							return Color.Sienna;
						case "silver":
							return Color.Silver;
						case "skyblue":
							return Color.SkyBlue;
						case "slateblue":
							return Color.SlateBlue;
						case "slategray":
							return Color.SlateGray;
						case "snow":
							return Color.Snow;
						case "springgreen":
							return Color.SpringGreen;
						case "steelblue":
							return Color.SteelBlue;
						case "tan":
							return Color.Tan;
						case "teal":
							return Color.Teal;
						case "thistle":
							return Color.Thistle;
						case "tomato":
							return Color.Tomato;
						case "transparent":
							return Color.Transparent;
						case "turquoise":
							return Color.Turquoise;
						case "violet":
							return Color.Violet;
						case "wheat":
							return Color.Wheat;
						case "white":
							return Color.White;
						case "whitesmoke":
							return Color.WhiteSmoke;
						case "yellow":
							return Color.Yellow;
						case "yellowgreen":
							return Color.YellowGreen;
					}
					var field = typeof(Color).GetFields().FirstOrDefault(fi => fi.IsStatic && string.Equals(fi.Name, color, StringComparison.OrdinalIgnoreCase));
					if (field != null)
						return (Color)field.GetValue(null);
					var property = typeof(Color).GetProperties().FirstOrDefault(pi => string.Equals(pi.Name, color, StringComparison.OrdinalIgnoreCase) && pi.CanRead && pi.GetMethod.IsStatic);
					if (property != null)
						return (Color)property.GetValue(null, null);
				}

				var namedColor = Device.GetNamedColor(value);
				if (namedColor != default)
					return namedColor;
			}

			throw new InvalidOperationException($"Cannot convert \"{value}\" into {typeof(Color)}");
		}

		static double ParseColorValue(string elem, int maxValue, bool acceptPercent)
		{
			elem = elem.Trim();
			if (elem.EndsWith("%", StringComparison.Ordinal) && acceptPercent)
			{
				maxValue = 100;
				elem = elem.Substring(0, elem.Length - 1);
			}
			return double.Parse(elem, NumberStyles.Number, CultureInfo.InvariantCulture).Clamp(0, maxValue) / maxValue;
		}

		static double ParseOpacity(string elem)
		{
			return double.Parse(elem, NumberStyles.Number, CultureInfo.InvariantCulture).Clamp(0, 1);
		}
	}
}
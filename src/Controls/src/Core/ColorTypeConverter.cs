using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.ColorTypeConverter")]
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
					return Color.FromArgb(value);

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
					return new Color((float)r, (float)g, (float)b, (float)a);
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
					return new Color((float)r, (float)g, (float)b);
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
					return Color.FromHsva((float)h, (float)s, (float)v, (float)a);
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
					return Color.FromHsv((float)h, (float)s, (float)v);
				}

				string[] parts = value.Split('.');
				if (parts.Length == 1 || (parts.Length == 2 && parts[0] == "Color"))
				{
					string color = parts[parts.Length - 1];
					switch (color.ToLowerInvariant())
					{
						case "default":
							return null;
						//TODO: Remove this hack. Colors.Accent bad!
						//case "accent":
						//	return Colors.Accent;
						case "aliceblue":
							return Colors.AliceBlue;
						case "antiquewhite":
							return Colors.AntiqueWhite;
						case "aqua":
							return Colors.Aqua;
						case "aquamarine":
							return Colors.Aquamarine;
						case "azure":
							return Colors.Azure;
						case "beige":
							return Colors.Beige;
						case "bisque":
							return Colors.Bisque;
						case "black":
							return Colors.Black;
						case "blanchedalmond":
							return Colors.BlanchedAlmond;
						case "blue":
							return Colors.Blue;
						case "blueViolet":
							return Colors.BlueViolet;
						case "brown":
							return Colors.Brown;
						case "burlywood":
							return Colors.BurlyWood;
						case "cadetblue":
							return Colors.CadetBlue;
						case "chartreuse":
							return Colors.Chartreuse;
						case "chocolate":
							return Colors.Chocolate;
						case "coral":
							return Colors.Coral;
						case "cornflowerblue":
							return Colors.CornflowerBlue;
						case "cornsilk":
							return Colors.Cornsilk;
						case "crimson":
							return Colors.Crimson;
						case "cyan":
							return Colors.Cyan;
						case "darkblue":
							return Colors.DarkBlue;
						case "darkcyan":
							return Colors.DarkCyan;
						case "darkgoldenrod":
							return Colors.DarkGoldenrod;
						case "darkgray":
							return Colors.DarkGray;
						case "darkgreen":
							return Colors.DarkGreen;
						case "darkkhaki":
							return Colors.DarkKhaki;
						case "darkmagenta":
							return Colors.DarkMagenta;
						case "darkolivegreen":
							return Colors.DarkOliveGreen;
						case "darkorange":
							return Colors.DarkOrange;
						case "darkorchid":
							return Colors.DarkOrchid;
						case "darkred":
							return Colors.DarkRed;
						case "darksalmon":
							return Colors.DarkSalmon;
						case "darkseagreen":
							return Colors.DarkSeaGreen;
						case "darkslateblue":
							return Colors.DarkSlateBlue;
						case "darkslategray":
							return Colors.DarkSlateGray;
						case "darkturquoise":
							return Colors.DarkTurquoise;
						case "darkviolet":
							return Colors.DarkViolet;
						case "deeppink":
							return Colors.DeepPink;
						case "deepskyblue":
							return Colors.DeepSkyBlue;
						case "dimgray":
							return Colors.DimGray;
						case "dodgerblue":
							return Colors.DodgerBlue;
						case "firebrick":
							return Colors.Firebrick;
						case "floralwhite":
							return Colors.FloralWhite;
						case "forestgreen":
							return Colors.ForestGreen;
						case "fuchsia":
							return Colors.Fuchsia;
						case "gainsboro":
							return Colors.Gainsboro;
						case "ghostwhite":
							return Colors.GhostWhite;
						case "gold":
							return Colors.Gold;
						case "goldenrod":
							return Colors.Goldenrod;
						case "gray":
							return Colors.Gray;
						case "green":
							return Colors.Green;
						case "greenyellow":
							return Colors.GreenYellow;
						case "honeydew":
							return Colors.Honeydew;
						case "hotpink":
							return Colors.HotPink;
						case "indianred":
							return Colors.IndianRed;
						case "indigo":
							return Colors.Indigo;
						case "ivory":
							return Colors.Ivory;
						case "khaki":
							return Colors.Khaki;
						case "lavender":
							return Colors.Lavender;
						case "lavenderblush":
							return Colors.LavenderBlush;
						case "lawngreen":
							return Colors.LawnGreen;
						case "lemonchiffon":
							return Colors.LemonChiffon;
						case "lightblue":
							return Colors.LightBlue;
						case "lightcoral":
							return Colors.LightCoral;
						case "lightcyan":
							return Colors.LightCyan;
						case "lightgoldenrodyellow":
							return Colors.LightGoldenrodYellow;
						case "lightgrey":
						case "lightgray":
							return Colors.LightGray;
						case "lightgreen":
							return Colors.LightGreen;
						case "lightpink":
							return Colors.LightPink;
						case "lightsalmon":
							return Colors.LightSalmon;
						case "lightseagreen":
							return Colors.LightSeaGreen;
						case "lightskyblue":
							return Colors.LightSkyBlue;
						case "lightslategray":
							return Colors.LightSlateGray;
						case "lightsteelblue":
							return Colors.LightSteelBlue;
						case "lightyellow":
							return Colors.LightYellow;
						case "lime":
							return Colors.Lime;
						case "limegreen":
							return Colors.LimeGreen;
						case "linen":
							return Colors.Linen;
						case "magenta":
							return Colors.Magenta;
						case "maroon":
							return Colors.Maroon;
						case "mediumaquamarine":
							return Colors.MediumAquamarine;
						case "mediumblue":
							return Colors.MediumBlue;
						case "mediumorchid":
							return Colors.MediumOrchid;
						case "mediumpurple":
							return Colors.MediumPurple;
						case "mediumseagreen":
							return Colors.MediumSeaGreen;
						case "mediumslateblue":
							return Colors.MediumSlateBlue;
						case "mediumspringgreen":
							return Colors.MediumSpringGreen;
						case "mediumturquoise":
							return Colors.MediumTurquoise;
						case "mediumvioletred":
							return Colors.MediumVioletRed;
						case "midnightblue":
							return Colors.MidnightBlue;
						case "mintcream":
							return Colors.MintCream;
						case "mistyrose":
							return Colors.MistyRose;
						case "moccasin":
							return Colors.Moccasin;
						case "navajowhite":
							return Colors.NavajoWhite;
						case "navy":
							return Colors.Navy;
						case "oldlace":
							return Colors.OldLace;
						case "olive":
							return Colors.Olive;
						case "olivedrab":
							return Colors.OliveDrab;
						case "orange":
							return Colors.Orange;
						case "orangered":
							return Colors.OrangeRed;
						case "orchid":
							return Colors.Orchid;
						case "palegoldenrod":
							return Colors.PaleGoldenrod;
						case "palegreen":
							return Colors.PaleGreen;
						case "paleturquoise":
							return Colors.PaleTurquoise;
						case "palevioletred":
							return Colors.PaleVioletRed;
						case "papayawhip":
							return Colors.PapayaWhip;
						case "peachpuff":
							return Colors.PeachPuff;
						case "peru":
							return Colors.Peru;
						case "pink":
							return Colors.Pink;
						case "plum":
							return Colors.Plum;
						case "powderblue":
							return Colors.PowderBlue;
						case "purple":
							return Colors.Purple;
						case "red":
							return Colors.Red;
						case "rosybrown":
							return Colors.RosyBrown;
						case "royalblue":
							return Colors.RoyalBlue;
						case "saddlebrown":
							return Colors.SaddleBrown;
						case "salmon":
							return Colors.Salmon;
						case "sandybrown":
							return Colors.SandyBrown;
						case "seagreen":
							return Colors.SeaGreen;
						case "seashell":
							return Colors.SeaShell;
						case "sienna":
							return Colors.Sienna;
						case "silver":
							return Colors.Silver;
						case "skyblue":
							return Colors.SkyBlue;
						case "slateblue":
							return Colors.SlateBlue;
						case "slategray":
							return Colors.SlateGray;
						case "snow":
							return Colors.Snow;
						case "springgreen":
							return Colors.SpringGreen;
						case "steelblue":
							return Colors.SteelBlue;
						case "tan":
							return Colors.Tan;
						case "teal":
							return Colors.Teal;
						case "thistle":
							return Colors.Thistle;
						case "tomato":
							return Colors.Tomato;
						case "transparent":
							return Colors.Transparent;
						case "turquoise":
							return Colors.Turquoise;
						case "violet":
							return Colors.Violet;
						case "wheat":
							return Colors.Wheat;
						case "white":
							return Colors.White;
						case "whitesmoke":
							return Colors.WhiteSmoke;
						case "yellow":
							return Colors.Yellow;
						case "yellowgreen":
							return Colors.YellowGreen;
					}
					var field = typeof(Colors).GetFields().FirstOrDefault(fi => fi.IsStatic && string.Equals(fi.Name, color, StringComparison.OrdinalIgnoreCase));
					if (field != null)
						return (Color)field.GetValue(null);
					var property = typeof(Colors).GetProperties().FirstOrDefault(pi => string.Equals(pi.Name, color, StringComparison.OrdinalIgnoreCase) && pi.CanRead && pi.GetMethod.IsStatic);
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

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is Color color))
				throw new NotSupportedException();

			return color.ToHex();
		}
	}
}

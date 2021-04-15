using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	[TypeConverter(typeof(BrushTypeConverter))]
	public abstract partial class Brush : Element
	{
		public static Brush Default
		{
			get { return new SolidColorBrush(null); }
		}

		public abstract bool IsEmpty { get; }

		public static bool IsNullOrEmpty(Brush brush)
		{
			return brush == null || brush.IsEmpty;
		}

		public static readonly SolidColorBrush AliceBlue = new SolidColorBrush(Colors.AliceBlue);
		public static readonly SolidColorBrush AntiqueWhite = new SolidColorBrush(Colors.AntiqueWhite);
		public static readonly SolidColorBrush Aqua = new SolidColorBrush(Colors.Aqua);
		public static readonly SolidColorBrush Aquamarine = new SolidColorBrush(Colors.Aquamarine);
		public static readonly SolidColorBrush Azure = new SolidColorBrush(Colors.Azure);
		public static readonly SolidColorBrush Beige = new SolidColorBrush(Colors.Beige);
		public static readonly SolidColorBrush Bisque = new SolidColorBrush(Colors.Bisque);
		public static readonly SolidColorBrush Black = new SolidColorBrush(Colors.Black);
		public static readonly SolidColorBrush BlanchedAlmond = new SolidColorBrush(Colors.BlanchedAlmond);
		public static readonly SolidColorBrush Blue = new SolidColorBrush(Colors.Blue);
		public static readonly SolidColorBrush BlueViolet = new SolidColorBrush(Colors.BlueViolet);
		public static readonly SolidColorBrush Brown = new SolidColorBrush(Colors.Brown);
		public static readonly SolidColorBrush BurlyWood = new SolidColorBrush(Colors.BurlyWood);
		public static readonly SolidColorBrush CadetBlue = new SolidColorBrush(Colors.CadetBlue);
		public static readonly SolidColorBrush Chartreuse = new SolidColorBrush(Colors.Chartreuse);
		public static readonly SolidColorBrush Chocolate = new SolidColorBrush(Colors.Chocolate);
		public static readonly SolidColorBrush Coral = new SolidColorBrush(Colors.Coral);
		public static readonly SolidColorBrush CornflowerBlue = new SolidColorBrush(Colors.CornflowerBlue);
		public static readonly SolidColorBrush Cornsilk = new SolidColorBrush(Colors.Cornsilk);
		public static readonly SolidColorBrush Crimson = new SolidColorBrush(Colors.Crimson);
		public static readonly SolidColorBrush Cyan = new SolidColorBrush(Colors.Cyan);
		public static readonly SolidColorBrush DarkBlue = new SolidColorBrush(Colors.DarkBlue);
		public static readonly SolidColorBrush DarkCyan = new SolidColorBrush(Colors.DarkCyan);
		public static readonly SolidColorBrush DarkGoldenrod = new SolidColorBrush(Colors.DarkGoldenrod);
		public static readonly SolidColorBrush DarkGray = new SolidColorBrush(Colors.DarkGray);
		public static readonly SolidColorBrush DarkGreen = new SolidColorBrush(Colors.DarkGreen);
		public static readonly SolidColorBrush DarkKhaki = new SolidColorBrush(Colors.DarkKhaki);
		public static readonly SolidColorBrush DarkMagenta = new SolidColorBrush(Colors.DarkMagenta);
		public static readonly SolidColorBrush DarkOliveGreen = new SolidColorBrush(Colors.DarkOliveGreen);
		public static readonly SolidColorBrush DarkOrange = new SolidColorBrush(Colors.DarkOrange);
		public static readonly SolidColorBrush DarkOrchid = new SolidColorBrush(Colors.DarkOrchid);
		public static readonly SolidColorBrush DarkRed = new SolidColorBrush(Colors.DarkRed);
		public static readonly SolidColorBrush DarkSalmon = new SolidColorBrush(Colors.DarkSalmon);
		public static readonly SolidColorBrush DarkSeaGreen = new SolidColorBrush(Colors.DarkSeaGreen);
		public static readonly SolidColorBrush DarkSlateBlue = new SolidColorBrush(Colors.DarkSlateBlue);
		public static readonly SolidColorBrush DarkSlateGray = new SolidColorBrush(Colors.DarkSlateGray);
		public static readonly SolidColorBrush DarkTurquoise = new SolidColorBrush(Colors.DarkTurquoise);
		public static readonly SolidColorBrush DarkViolet = new SolidColorBrush(Colors.DarkViolet);
		public static readonly SolidColorBrush DeepPink = new SolidColorBrush(Colors.DeepPink);
		public static readonly SolidColorBrush DeepSkyBlue = new SolidColorBrush(Colors.DeepSkyBlue);
		public static readonly SolidColorBrush DimGray = new SolidColorBrush(Colors.DimGray);
		public static readonly SolidColorBrush DodgerBlue = new SolidColorBrush(Colors.DodgerBlue);
		public static readonly SolidColorBrush Firebrick = new SolidColorBrush(Colors.Firebrick);
		public static readonly SolidColorBrush FloralWhite = new SolidColorBrush(Colors.FloralWhite);
		public static readonly SolidColorBrush ForestGreen = new SolidColorBrush(Colors.ForestGreen);
		public static readonly SolidColorBrush Fuchsia = new SolidColorBrush(Colors.Fuchsia);
		public static readonly SolidColorBrush Gainsboro = new SolidColorBrush(Colors.Gainsboro);
		public static readonly SolidColorBrush GhostWhite = new SolidColorBrush(Colors.GhostWhite);
		public static readonly SolidColorBrush Gold = new SolidColorBrush(Colors.Gold);
		public static readonly SolidColorBrush Goldenrod = new SolidColorBrush(Colors.Goldenrod);
		public static readonly SolidColorBrush Gray = new SolidColorBrush(Colors.Gray);
		public static readonly SolidColorBrush Green = new SolidColorBrush(Colors.Green);
		public static readonly SolidColorBrush GreenYellow = new SolidColorBrush(Colors.GreenYellow);
		public static readonly SolidColorBrush Honeydew = new SolidColorBrush(Colors.Honeydew);
		public static readonly SolidColorBrush HotPink = new SolidColorBrush(Colors.HotPink);
		public static readonly SolidColorBrush IndianRed = new SolidColorBrush(Colors.IndianRed);
		public static readonly SolidColorBrush Indigo = new SolidColorBrush(Colors.Indigo);
		public static readonly SolidColorBrush Ivory = new SolidColorBrush(Colors.Ivory);
		public static readonly SolidColorBrush Khaki = new SolidColorBrush(Colors.Ivory);
		public static readonly SolidColorBrush Lavender = new SolidColorBrush(Colors.Lavender);
		public static readonly SolidColorBrush LavenderBlush = new SolidColorBrush(Colors.LavenderBlush);
		public static readonly SolidColorBrush LawnGreen = new SolidColorBrush(Colors.LawnGreen);
		public static readonly SolidColorBrush LemonChiffon = new SolidColorBrush(Colors.LemonChiffon);
		public static readonly SolidColorBrush LightBlue = new SolidColorBrush(Colors.LightBlue);
		public static readonly SolidColorBrush LightCoral = new SolidColorBrush(Colors.LightCoral);
		public static readonly SolidColorBrush LightCyan = new SolidColorBrush(Colors.LightCyan);
		public static readonly SolidColorBrush LightGoldenrodYellow = new SolidColorBrush(Colors.LightGoldenrodYellow);
		public static readonly SolidColorBrush LightGray = new SolidColorBrush(Colors.LightGray);
		public static readonly SolidColorBrush LightGreen = new SolidColorBrush(Colors.LightGreen);
		public static readonly SolidColorBrush LightPink = new SolidColorBrush(Colors.LightPink);
		public static readonly SolidColorBrush LightSalmon = new SolidColorBrush(Colors.LightSalmon);
		public static readonly SolidColorBrush LightSeaGreen = new SolidColorBrush(Colors.LightSeaGreen);
		public static readonly SolidColorBrush LightSkyBlue = new SolidColorBrush(Colors.LightSkyBlue);
		public static readonly SolidColorBrush LightSlateGray = new SolidColorBrush(Colors.LightSlateGray);
		public static readonly SolidColorBrush LightSteelBlue = new SolidColorBrush(Colors.LightSteelBlue);
		public static readonly SolidColorBrush LightYellow = new SolidColorBrush(Colors.LightYellow);
		public static readonly SolidColorBrush Lime = new SolidColorBrush(Colors.Lime);
		public static readonly SolidColorBrush LimeGreen = new SolidColorBrush(Colors.LimeGreen);
		public static readonly SolidColorBrush Linen = new SolidColorBrush(Colors.Linen);
		public static readonly SolidColorBrush Magenta = new SolidColorBrush(Colors.Magenta);
		public static readonly SolidColorBrush Maroon = new SolidColorBrush(Colors.Maroon);
		public static readonly SolidColorBrush MediumAquamarine = new SolidColorBrush(Colors.MediumAquamarine);
		public static readonly SolidColorBrush MediumBlue = new SolidColorBrush(Colors.MediumBlue);
		public static readonly SolidColorBrush MediumOrchid = new SolidColorBrush(Colors.MediumOrchid);
		public static readonly SolidColorBrush MediumPurple = new SolidColorBrush(Colors.MediumPurple);
		public static readonly SolidColorBrush MediumSeaGreen = new SolidColorBrush(Colors.MediumSeaGreen);
		public static readonly SolidColorBrush MediumSlateBlue = new SolidColorBrush(Colors.MediumSlateBlue);
		public static readonly SolidColorBrush MediumSpringGreen = new SolidColorBrush(Colors.MediumSpringGreen);
		public static readonly SolidColorBrush MediumTurquoise = new SolidColorBrush(Colors.MediumTurquoise);
		public static readonly SolidColorBrush MediumVioletRed = new SolidColorBrush(Colors.MediumVioletRed);
		public static readonly SolidColorBrush MidnightBlue = new SolidColorBrush(Colors.MidnightBlue);
		public static readonly SolidColorBrush MintCream = new SolidColorBrush(Colors.MintCream);
		public static readonly SolidColorBrush MistyRose = new SolidColorBrush(Colors.MistyRose);
		public static readonly SolidColorBrush Moccasin = new SolidColorBrush(Colors.Moccasin);
		public static readonly SolidColorBrush NavajoWhite = new SolidColorBrush(Colors.NavajoWhite);
		public static readonly SolidColorBrush Navy = new SolidColorBrush(Colors.Navy);
		public static readonly SolidColorBrush OldLace = new SolidColorBrush(Colors.DarkBlue);
		public static readonly SolidColorBrush Olive = new SolidColorBrush(Colors.Olive);
		public static readonly SolidColorBrush OliveDrab = new SolidColorBrush(Colors.OliveDrab);
		public static readonly SolidColorBrush Orange = new SolidColorBrush(Colors.Orange);
		public static readonly SolidColorBrush OrangeRed = new SolidColorBrush(Colors.OrangeRed);
		public static readonly SolidColorBrush Orchid = new SolidColorBrush(Colors.Orchid);
		public static readonly SolidColorBrush PaleGoldenrod = new SolidColorBrush(Colors.PaleGoldenrod);
		public static readonly SolidColorBrush PaleGreen = new SolidColorBrush(Colors.MistyRose);
		public static readonly SolidColorBrush PaleTurquoise = new SolidColorBrush(Colors.PaleTurquoise);
		public static readonly SolidColorBrush PaleVioletRed = new SolidColorBrush(Colors.PaleVioletRed);
		public static readonly SolidColorBrush PapayaWhip = new SolidColorBrush(Colors.PapayaWhip);
		public static readonly SolidColorBrush PeachPuff = new SolidColorBrush(Colors.PeachPuff);
		public static readonly SolidColorBrush Peru = new SolidColorBrush(Colors.Peru);
		public static readonly SolidColorBrush Pink = new SolidColorBrush(Colors.Pink);
		public static readonly SolidColorBrush Plum = new SolidColorBrush(Colors.Plum);
		public static readonly SolidColorBrush PowderBlue = new SolidColorBrush(Colors.PowderBlue);
		public static readonly SolidColorBrush Purple = new SolidColorBrush(Colors.Purple);
		public static readonly SolidColorBrush Red = new SolidColorBrush(Colors.Red);
		public static readonly SolidColorBrush RosyBrown = new SolidColorBrush(Colors.RosyBrown);
		public static readonly SolidColorBrush RoyalBlue = new SolidColorBrush(Colors.RoyalBlue);
		public static readonly SolidColorBrush SaddleBrown = new SolidColorBrush(Colors.SaddleBrown);
		public static readonly SolidColorBrush Salmon = new SolidColorBrush(Colors.Salmon);
		public static readonly SolidColorBrush SandyBrown = new SolidColorBrush(Colors.SandyBrown);
		public static readonly SolidColorBrush SeaGreen = new SolidColorBrush(Colors.SeaGreen);
		public static readonly SolidColorBrush SeaShell = new SolidColorBrush(Colors.SeaShell);
		public static readonly SolidColorBrush Sienna = new SolidColorBrush(Colors.Sienna);
		public static readonly SolidColorBrush Silver = new SolidColorBrush(Colors.Silver);
		public static readonly SolidColorBrush SkyBlue = new SolidColorBrush(Colors.SkyBlue);
		public static readonly SolidColorBrush SlateBlue = new SolidColorBrush(Colors.SlateBlue);
		public static readonly SolidColorBrush SlateGray = new SolidColorBrush(Colors.SlateGray);
		public static readonly SolidColorBrush Snow = new SolidColorBrush(Colors.Snow);
		public static readonly SolidColorBrush SpringGreen = new SolidColorBrush(Colors.SpringGreen);
		public static readonly SolidColorBrush SteelBlue = new SolidColorBrush(Colors.SteelBlue);
		public static readonly SolidColorBrush Tan = new SolidColorBrush(Colors.Tan);
		public static readonly SolidColorBrush Teal = new SolidColorBrush(Colors.Teal);
		public static readonly SolidColorBrush Thistle = new SolidColorBrush(Colors.Thistle);
		public static readonly SolidColorBrush Tomato = new SolidColorBrush(Colors.Tomato);
		public static readonly SolidColorBrush Transparent = new SolidColorBrush(Colors.Transparent);
		public static readonly SolidColorBrush Turquoise = new SolidColorBrush(Colors.Turquoise);
		public static readonly SolidColorBrush Violet = new SolidColorBrush(Colors.Violet);
		public static readonly SolidColorBrush Wheat = new SolidColorBrush(Colors.Wheat);
		public static readonly SolidColorBrush White = new SolidColorBrush(Colors.White);
		public static readonly SolidColorBrush WhiteSmoke = new SolidColorBrush(Colors.WhiteSmoke);
		public static readonly SolidColorBrush Yellow = new SolidColorBrush(Colors.Yellow);
		public static readonly SolidColorBrush YellowGreen = new SolidColorBrush(Colors.YellowGreen);
	}
}
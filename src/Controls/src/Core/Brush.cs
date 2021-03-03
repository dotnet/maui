namespace Microsoft.Maui.Controls
{
	[TypeConverter(typeof(BrushTypeConverter))]
	public abstract class Brush : Element
	{
		public static Brush Default
		{
			get { return new SolidColorBrush(Color.Default); }
		}

		public abstract bool IsEmpty { get; }

		public static bool IsNullOrEmpty(Brush brush)
		{
			return brush == null || brush.IsEmpty;
		}

		public static readonly SolidColorBrush AliceBlue = new SolidColorBrush(Color.AliceBlue);
		public static readonly SolidColorBrush AntiqueWhite = new SolidColorBrush(Color.AntiqueWhite);
		public static readonly SolidColorBrush Aqua = new SolidColorBrush(Color.Aqua);
		public static readonly SolidColorBrush Aquamarine = new SolidColorBrush(Color.Aquamarine);
		public static readonly SolidColorBrush Azure = new SolidColorBrush(Color.Azure);
		public static readonly SolidColorBrush Beige = new SolidColorBrush(Color.Beige);
		public static readonly SolidColorBrush Bisque = new SolidColorBrush(Color.Bisque);
		public static readonly SolidColorBrush Black = new SolidColorBrush(Color.Black);
		public static readonly SolidColorBrush BlanchedAlmond = new SolidColorBrush(Color.BlanchedAlmond);
		public static readonly SolidColorBrush Blue = new SolidColorBrush(Color.Blue);
		public static readonly SolidColorBrush BlueViolet = new SolidColorBrush(Color.BlueViolet);
		public static readonly SolidColorBrush Brown = new SolidColorBrush(Color.Brown);
		public static readonly SolidColorBrush BurlyWood = new SolidColorBrush(Color.BurlyWood);
		public static readonly SolidColorBrush CadetBlue = new SolidColorBrush(Color.CadetBlue);
		public static readonly SolidColorBrush Chartreuse = new SolidColorBrush(Color.Chartreuse);
		public static readonly SolidColorBrush Chocolate = new SolidColorBrush(Color.Chocolate);
		public static readonly SolidColorBrush Coral = new SolidColorBrush(Color.Coral);
		public static readonly SolidColorBrush CornflowerBlue = new SolidColorBrush(Color.CornflowerBlue);
		public static readonly SolidColorBrush Cornsilk = new SolidColorBrush(Color.Cornsilk);
		public static readonly SolidColorBrush Crimson = new SolidColorBrush(Color.Crimson);
		public static readonly SolidColorBrush Cyan = new SolidColorBrush(Color.Cyan);
		public static readonly SolidColorBrush DarkBlue = new SolidColorBrush(Color.DarkBlue);
		public static readonly SolidColorBrush DarkCyan = new SolidColorBrush(Color.DarkCyan);
		public static readonly SolidColorBrush DarkGoldenrod = new SolidColorBrush(Color.DarkGoldenrod);
		public static readonly SolidColorBrush DarkGray = new SolidColorBrush(Color.DarkGray);
		public static readonly SolidColorBrush DarkGreen = new SolidColorBrush(Color.DarkGreen);
		public static readonly SolidColorBrush DarkKhaki = new SolidColorBrush(Color.DarkKhaki);
		public static readonly SolidColorBrush DarkMagenta = new SolidColorBrush(Color.DarkMagenta);
		public static readonly SolidColorBrush DarkOliveGreen = new SolidColorBrush(Color.DarkOliveGreen);
		public static readonly SolidColorBrush DarkOrange = new SolidColorBrush(Color.DarkOrange);
		public static readonly SolidColorBrush DarkOrchid = new SolidColorBrush(Color.DarkOrchid);
		public static readonly SolidColorBrush DarkRed = new SolidColorBrush(Color.DarkRed);
		public static readonly SolidColorBrush DarkSalmon = new SolidColorBrush(Color.DarkSalmon);
		public static readonly SolidColorBrush DarkSeaGreen = new SolidColorBrush(Color.DarkSeaGreen);
		public static readonly SolidColorBrush DarkSlateBlue = new SolidColorBrush(Color.DarkSlateBlue);
		public static readonly SolidColorBrush DarkSlateGray = new SolidColorBrush(Color.DarkSlateGray);
		public static readonly SolidColorBrush DarkTurquoise = new SolidColorBrush(Color.DarkTurquoise);
		public static readonly SolidColorBrush DarkViolet = new SolidColorBrush(Color.DarkViolet);
		public static readonly SolidColorBrush DeepPink = new SolidColorBrush(Color.DeepPink);
		public static readonly SolidColorBrush DeepSkyBlue = new SolidColorBrush(Color.DeepSkyBlue);
		public static readonly SolidColorBrush DimGray = new SolidColorBrush(Color.DimGray);
		public static readonly SolidColorBrush DodgerBlue = new SolidColorBrush(Color.DodgerBlue);
		public static readonly SolidColorBrush Firebrick = new SolidColorBrush(Color.Firebrick);
		public static readonly SolidColorBrush FloralWhite = new SolidColorBrush(Color.FloralWhite);
		public static readonly SolidColorBrush ForestGreen = new SolidColorBrush(Color.ForestGreen);
		public static readonly SolidColorBrush Fuchsia = new SolidColorBrush(Color.Fuchsia);
		public static readonly SolidColorBrush Gainsboro = new SolidColorBrush(Color.Gainsboro);
		public static readonly SolidColorBrush GhostWhite = new SolidColorBrush(Color.GhostWhite);
		public static readonly SolidColorBrush Gold = new SolidColorBrush(Color.Gold);
		public static readonly SolidColorBrush Goldenrod = new SolidColorBrush(Color.Goldenrod);
		public static readonly SolidColorBrush Gray = new SolidColorBrush(Color.Gray);
		public static readonly SolidColorBrush Green = new SolidColorBrush(Color.Green);
		public static readonly SolidColorBrush GreenYellow = new SolidColorBrush(Color.GreenYellow);
		public static readonly SolidColorBrush Honeydew = new SolidColorBrush(Color.Honeydew);
		public static readonly SolidColorBrush HotPink = new SolidColorBrush(Color.HotPink);
		public static readonly SolidColorBrush IndianRed = new SolidColorBrush(Color.IndianRed);
		public static readonly SolidColorBrush Indigo = new SolidColorBrush(Color.Indigo);
		public static readonly SolidColorBrush Ivory = new SolidColorBrush(Color.Ivory);
		public static readonly SolidColorBrush Khaki = new SolidColorBrush(Color.Ivory);
		public static readonly SolidColorBrush Lavender = new SolidColorBrush(Color.Lavender);
		public static readonly SolidColorBrush LavenderBlush = new SolidColorBrush(Color.LavenderBlush);
		public static readonly SolidColorBrush LawnGreen = new SolidColorBrush(Color.LawnGreen);
		public static readonly SolidColorBrush LemonChiffon = new SolidColorBrush(Color.LemonChiffon);
		public static readonly SolidColorBrush LightBlue = new SolidColorBrush(Color.LightBlue);
		public static readonly SolidColorBrush LightCoral = new SolidColorBrush(Color.LightCoral);
		public static readonly SolidColorBrush LightCyan = new SolidColorBrush(Color.LightCyan);
		public static readonly SolidColorBrush LightGoldenrodYellow = new SolidColorBrush(Color.LightGoldenrodYellow);
		public static readonly SolidColorBrush LightGray = new SolidColorBrush(Color.LightGray);
		public static readonly SolidColorBrush LightGreen = new SolidColorBrush(Color.LightGreen);
		public static readonly SolidColorBrush LightPink = new SolidColorBrush(Color.LightPink);
		public static readonly SolidColorBrush LightSalmon = new SolidColorBrush(Color.LightSalmon);
		public static readonly SolidColorBrush LightSeaGreen = new SolidColorBrush(Color.LightSeaGreen);
		public static readonly SolidColorBrush LightSkyBlue = new SolidColorBrush(Color.LightSkyBlue);
		public static readonly SolidColorBrush LightSlateGray = new SolidColorBrush(Color.LightSlateGray);
		public static readonly SolidColorBrush LightSteelBlue = new SolidColorBrush(Color.LightSteelBlue);
		public static readonly SolidColorBrush LightYellow = new SolidColorBrush(Color.LightYellow);
		public static readonly SolidColorBrush Lime = new SolidColorBrush(Color.Lime);
		public static readonly SolidColorBrush LimeGreen = new SolidColorBrush(Color.LimeGreen);
		public static readonly SolidColorBrush Linen = new SolidColorBrush(Color.Linen);
		public static readonly SolidColorBrush Magenta = new SolidColorBrush(Color.Magenta);
		public static readonly SolidColorBrush Maroon = new SolidColorBrush(Color.Maroon);
		public static readonly SolidColorBrush MediumAquamarine = new SolidColorBrush(Color.MediumAquamarine);
		public static readonly SolidColorBrush MediumBlue = new SolidColorBrush(Color.MediumBlue);
		public static readonly SolidColorBrush MediumOrchid = new SolidColorBrush(Color.MediumOrchid);
		public static readonly SolidColorBrush MediumPurple = new SolidColorBrush(Color.MediumPurple);
		public static readonly SolidColorBrush MediumSeaGreen = new SolidColorBrush(Color.MediumSeaGreen);
		public static readonly SolidColorBrush MediumSlateBlue = new SolidColorBrush(Color.MediumSlateBlue);
		public static readonly SolidColorBrush MediumSpringGreen = new SolidColorBrush(Color.MediumSpringGreen);
		public static readonly SolidColorBrush MediumTurquoise = new SolidColorBrush(Color.MediumTurquoise);
		public static readonly SolidColorBrush MediumVioletRed = new SolidColorBrush(Color.MediumVioletRed);
		public static readonly SolidColorBrush MidnightBlue = new SolidColorBrush(Color.MidnightBlue);
		public static readonly SolidColorBrush MintCream = new SolidColorBrush(Color.MintCream);
		public static readonly SolidColorBrush MistyRose = new SolidColorBrush(Color.MistyRose);
		public static readonly SolidColorBrush Moccasin = new SolidColorBrush(Color.Moccasin);
		public static readonly SolidColorBrush NavajoWhite = new SolidColorBrush(Color.NavajoWhite);
		public static readonly SolidColorBrush Navy = new SolidColorBrush(Color.Navy);
		public static readonly SolidColorBrush OldLace = new SolidColorBrush(Color.DarkBlue);
		public static readonly SolidColorBrush Olive = new SolidColorBrush(Color.Olive);
		public static readonly SolidColorBrush OliveDrab = new SolidColorBrush(Color.OliveDrab);
		public static readonly SolidColorBrush Orange = new SolidColorBrush(Color.Orange);
		public static readonly SolidColorBrush OrangeRed = new SolidColorBrush(Color.OrangeRed);
		public static readonly SolidColorBrush Orchid = new SolidColorBrush(Color.Orchid);
		public static readonly SolidColorBrush PaleGoldenrod = new SolidColorBrush(Color.PaleGoldenrod);
		public static readonly SolidColorBrush PaleGreen = new SolidColorBrush(Color.MistyRose);
		public static readonly SolidColorBrush PaleTurquoise = new SolidColorBrush(Color.PaleTurquoise);
		public static readonly SolidColorBrush PaleVioletRed = new SolidColorBrush(Color.PaleVioletRed);
		public static readonly SolidColorBrush PapayaWhip = new SolidColorBrush(Color.PapayaWhip);
		public static readonly SolidColorBrush PeachPuff = new SolidColorBrush(Color.PeachPuff);
		public static readonly SolidColorBrush Peru = new SolidColorBrush(Color.Peru);
		public static readonly SolidColorBrush Pink = new SolidColorBrush(Color.Pink);
		public static readonly SolidColorBrush Plum = new SolidColorBrush(Color.Plum);
		public static readonly SolidColorBrush PowderBlue = new SolidColorBrush(Color.PowderBlue);
		public static readonly SolidColorBrush Purple = new SolidColorBrush(Color.Purple);
		public static readonly SolidColorBrush Red = new SolidColorBrush(Color.Red);
		public static readonly SolidColorBrush RosyBrown = new SolidColorBrush(Color.RosyBrown);
		public static readonly SolidColorBrush RoyalBlue = new SolidColorBrush(Color.RoyalBlue);
		public static readonly SolidColorBrush SaddleBrown = new SolidColorBrush(Color.SaddleBrown);
		public static readonly SolidColorBrush Salmon = new SolidColorBrush(Color.Salmon);
		public static readonly SolidColorBrush SandyBrown = new SolidColorBrush(Color.SandyBrown);
		public static readonly SolidColorBrush SeaGreen = new SolidColorBrush(Color.SeaGreen);
		public static readonly SolidColorBrush SeaShell = new SolidColorBrush(Color.SeaShell);
		public static readonly SolidColorBrush Sienna = new SolidColorBrush(Color.Sienna);
		public static readonly SolidColorBrush Silver = new SolidColorBrush(Color.Silver);
		public static readonly SolidColorBrush SkyBlue = new SolidColorBrush(Color.SkyBlue);
		public static readonly SolidColorBrush SlateBlue = new SolidColorBrush(Color.SlateBlue);
		public static readonly SolidColorBrush SlateGray = new SolidColorBrush(Color.SlateGray);
		public static readonly SolidColorBrush Snow = new SolidColorBrush(Color.Snow);
		public static readonly SolidColorBrush SpringGreen = new SolidColorBrush(Color.SpringGreen);
		public static readonly SolidColorBrush SteelBlue = new SolidColorBrush(Color.SteelBlue);
		public static readonly SolidColorBrush Tan = new SolidColorBrush(Color.Tan);
		public static readonly SolidColorBrush Teal = new SolidColorBrush(Color.Teal);
		public static readonly SolidColorBrush Thistle = new SolidColorBrush(Color.Thistle);
		public static readonly SolidColorBrush Tomato = new SolidColorBrush(Color.Tomato);
		public static readonly SolidColorBrush Transparent = new SolidColorBrush(Color.Transparent);
		public static readonly SolidColorBrush Turquoise = new SolidColorBrush(Color.Turquoise);
		public static readonly SolidColorBrush Violet = new SolidColorBrush(Color.Violet);
		public static readonly SolidColorBrush Wheat = new SolidColorBrush(Color.Wheat);
		public static readonly SolidColorBrush White = new SolidColorBrush(Color.White);
		public static readonly SolidColorBrush WhiteSmoke = new SolidColorBrush(Color.WhiteSmoke);
		public static readonly SolidColorBrush Yellow = new SolidColorBrush(Color.Yellow);
		public static readonly SolidColorBrush YellowGreen = new SolidColorBrush(Color.YellowGreen);
	}
}
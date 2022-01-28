using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="Type[@FullName='Microsoft.Maui.Controls.Brush']/Docs" />
	[System.ComponentModel.TypeConverter(typeof(BrushTypeConverter))]
	public abstract partial class Brush : Element
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Default']/Docs" />
		public static Brush Default
		{
			get { return new SolidColorBrush(null); }
		}

		public static implicit operator Brush(Color color) => new SolidColorBrush(color);

		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='IsEmpty']/Docs" />
		public abstract bool IsEmpty { get; }

		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='IsNullOrEmpty']/Docs" />
		public static bool IsNullOrEmpty(Brush brush)
		{
			return brush == null || brush.IsEmpty;
		}

		static ImmutableBrush aliceBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='AliceBlue']/Docs" />
		public static SolidColorBrush AliceBlue => aliceBlue ??= new(Colors.AliceBlue);
		static ImmutableBrush antiqueWhite;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='AntiqueWhite']/Docs" />
		public static SolidColorBrush AntiqueWhite => antiqueWhite ??= new(Colors.AntiqueWhite);
		static ImmutableBrush aqua;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Aqua']/Docs" />
		public static SolidColorBrush Aqua => aqua ??= new(Colors.Aqua);
		static ImmutableBrush aquamarine;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Aquamarine']/Docs" />
		public static SolidColorBrush Aquamarine => aquamarine ??= new(Colors.Aquamarine);
		static ImmutableBrush azure;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Azure']/Docs" />
		public static SolidColorBrush Azure => azure ??= new(Colors.Azure);
		static ImmutableBrush beige;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Beige']/Docs" />
		public static SolidColorBrush Beige => beige ??= new(Colors.Beige);
		static ImmutableBrush bisque;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Bisque']/Docs" />
		public static SolidColorBrush Bisque => bisque ??= new(Colors.Bisque);
		static ImmutableBrush black;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Black']/Docs" />
		public static SolidColorBrush Black => black ??= new(Colors.Black);
		static ImmutableBrush blanchedAlmond;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='BlanchedAlmond']/Docs" />
		public static SolidColorBrush BlanchedAlmond => blanchedAlmond ??= new(Colors.BlanchedAlmond);
		static ImmutableBrush blue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Blue']/Docs" />
		public static SolidColorBrush Blue => blue ??= new(Colors.Blue);
		static ImmutableBrush blueViolet;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='BlueViolet']/Docs" />
		public static SolidColorBrush BlueViolet => blueViolet ??= new(Colors.BlueViolet);
		static ImmutableBrush brown;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Brown']/Docs" />
		public static SolidColorBrush Brown => brown ??= new(Colors.Brown);
		static ImmutableBrush burlyWood;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='BurlyWood']/Docs" />
		public static SolidColorBrush BurlyWood => burlyWood ??= new(Colors.BurlyWood);
		static ImmutableBrush cadetBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='CadetBlue']/Docs" />
		public static SolidColorBrush CadetBlue => cadetBlue ??= new(Colors.CadetBlue);
		static ImmutableBrush chartreuse;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Chartreuse']/Docs" />
		public static SolidColorBrush Chartreuse => chartreuse ??= new(Colors.Chartreuse);
		static ImmutableBrush chocolate;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Chocolate']/Docs" />
		public static SolidColorBrush Chocolate => chocolate ??= new(Colors.Chocolate);
		static ImmutableBrush coral;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Coral']/Docs" />
		public static SolidColorBrush Coral => coral ??= new(Colors.Coral);
		static ImmutableBrush cornflowerBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='CornflowerBlue']/Docs" />
		public static SolidColorBrush CornflowerBlue => cornflowerBlue ??= new(Colors.CornflowerBlue);
		static ImmutableBrush cornsilk;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Cornsilk']/Docs" />
		public static SolidColorBrush Cornsilk => cornsilk ??= new(Colors.Cornsilk);
		static ImmutableBrush crimson;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Crimson']/Docs" />
		public static SolidColorBrush Crimson => crimson ??= new(Colors.Crimson);
		static ImmutableBrush cyan;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Cyan']/Docs" />
		public static SolidColorBrush Cyan => cyan ??= new(Colors.Cyan);
		static ImmutableBrush darkBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkBlue']/Docs" />
		public static SolidColorBrush DarkBlue => darkBlue ??= new(Colors.DarkBlue);
		static ImmutableBrush darkCyan;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkCyan']/Docs" />
		public static SolidColorBrush DarkCyan => darkCyan ??= new(Colors.DarkCyan);
		static ImmutableBrush darkGoldenrod;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkGoldenrod']/Docs" />
		public static SolidColorBrush DarkGoldenrod => darkGoldenrod ??= new(Colors.DarkGoldenrod);
		static ImmutableBrush darkGray;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkGray']/Docs" />
		public static SolidColorBrush DarkGray => darkGray ??= new(Colors.DarkGray);
		static ImmutableBrush darkGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkGreen']/Docs" />
		public static SolidColorBrush DarkGreen => darkGreen ??= new(Colors.DarkGreen);
		static ImmutableBrush darkKhaki;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkKhaki']/Docs" />
		public static SolidColorBrush DarkKhaki => darkKhaki ??= new(Colors.DarkKhaki);
		static ImmutableBrush darkMagenta;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkMagenta']/Docs" />
		public static SolidColorBrush DarkMagenta => darkMagenta ??= new(Colors.DarkMagenta);
		static ImmutableBrush darkOliveGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkOliveGreen']/Docs" />
		public static SolidColorBrush DarkOliveGreen => darkOliveGreen ??= new(Colors.DarkOliveGreen);
		static ImmutableBrush darkOrange;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkOrange']/Docs" />
		public static SolidColorBrush DarkOrange => darkOrange ??= new(Colors.DarkOrange);
		static ImmutableBrush darkOrchid;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkOrchid']/Docs" />
		public static SolidColorBrush DarkOrchid => darkOrchid ??= new(Colors.DarkOrchid);
		static ImmutableBrush darkRed;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkRed']/Docs" />
		public static SolidColorBrush DarkRed => darkRed ??= new(Colors.DarkRed);
		static ImmutableBrush darkSalmon;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkSalmon']/Docs" />
		public static SolidColorBrush DarkSalmon => darkSalmon ??= new(Colors.DarkSalmon);
		static ImmutableBrush darkSeaGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkSeaGreen']/Docs" />
		public static SolidColorBrush DarkSeaGreen => darkSeaGreen ??= new(Colors.DarkSeaGreen);
		static ImmutableBrush darkSlateBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkSlateBlue']/Docs" />
		public static SolidColorBrush DarkSlateBlue => darkSlateBlue ??= new(Colors.DarkSlateBlue);
		static ImmutableBrush darkSlateGray;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkSlateGray']/Docs" />
		public static SolidColorBrush DarkSlateGray => darkSlateGray ??= new(Colors.DarkSlateGray);
		static ImmutableBrush darkTurquoise;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkTurquoise']/Docs" />
		public static SolidColorBrush DarkTurquoise => darkTurquoise ??= new(Colors.DarkTurquoise);
		static ImmutableBrush darkViolet;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DarkViolet']/Docs" />
		public static SolidColorBrush DarkViolet => darkViolet ??= new(Colors.DarkViolet);
		static ImmutableBrush deepPink;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DeepPink']/Docs" />
		public static SolidColorBrush DeepPink => deepPink ??= new(Colors.DeepPink);
		static ImmutableBrush deepSkyBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DeepSkyBlue']/Docs" />
		public static SolidColorBrush DeepSkyBlue => deepSkyBlue ??= new(Colors.DeepSkyBlue);
		static ImmutableBrush dimGray;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DimGray']/Docs" />
		public static SolidColorBrush DimGray => dimGray ??= new(Colors.DimGray);
		static ImmutableBrush dodgerBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='DodgerBlue']/Docs" />
		public static SolidColorBrush DodgerBlue => dodgerBlue ??= new(Colors.DodgerBlue);
		static ImmutableBrush firebrick;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Firebrick']/Docs" />
		public static SolidColorBrush Firebrick => firebrick ??= new(Colors.Firebrick);
		static ImmutableBrush floralWhite;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='FloralWhite']/Docs" />
		public static SolidColorBrush FloralWhite => floralWhite ??= new(Colors.FloralWhite);
		static ImmutableBrush forestGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='ForestGreen']/Docs" />
		public static SolidColorBrush ForestGreen => forestGreen ??= new(Colors.ForestGreen);
		static ImmutableBrush fuschia;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Fuchsia']/Docs" />
		public static SolidColorBrush Fuchsia => fuschia ??= new(Colors.Fuchsia);
		static ImmutableBrush gainsboro;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Gainsboro']/Docs" />
		public static SolidColorBrush Gainsboro => gainsboro ??= new(Colors.Gainsboro);
		static ImmutableBrush ghostWhite;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='GhostWhite']/Docs" />
		public static SolidColorBrush GhostWhite => ghostWhite ??= new(Colors.GhostWhite);
		static ImmutableBrush gold;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Gold']/Docs" />
		public static SolidColorBrush Gold => gold ??= new(Colors.Gold);
		static ImmutableBrush goldenrod;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Goldenrod']/Docs" />
		public static SolidColorBrush Goldenrod => goldenrod ??= new(Colors.Goldenrod);
		static ImmutableBrush gray;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Gray']/Docs" />
		public static SolidColorBrush Gray => gray ??= new(Colors.Gray);
		static ImmutableBrush green;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Green']/Docs" />
		public static SolidColorBrush Green => green ??= new(Colors.Green);
		static ImmutableBrush greenYellow;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='GreenYellow']/Docs" />
		public static SolidColorBrush GreenYellow => greenYellow ??= new(Colors.GreenYellow);
		static ImmutableBrush honeydew;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Honeydew']/Docs" />
		public static SolidColorBrush Honeydew => honeydew ??= new(Colors.Honeydew);
		static ImmutableBrush hotPink;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='HotPink']/Docs" />
		public static SolidColorBrush HotPink => hotPink ??= new(Colors.HotPink);
		static ImmutableBrush indianRed;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='IndianRed']/Docs" />
		public static SolidColorBrush IndianRed => indianRed ??= new(Colors.IndianRed);
		static ImmutableBrush indigo;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Indigo']/Docs" />
		public static SolidColorBrush Indigo => indigo ??= new(Colors.Indigo);
		static ImmutableBrush ivory;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Ivory']/Docs" />
		public static SolidColorBrush Ivory => ivory ??= new(Colors.Ivory);
		static ImmutableBrush khaki;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Khaki']/Docs" />
		public static SolidColorBrush Khaki => khaki ??= new(Colors.Khaki);
		static ImmutableBrush lavender;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Lavender']/Docs" />
		public static SolidColorBrush Lavender => lavender ??= new(Colors.Lavender);
		static ImmutableBrush lavenderBlush;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LavenderBlush']/Docs" />
		public static SolidColorBrush LavenderBlush => lavenderBlush ??= new(Colors.LavenderBlush);
		static ImmutableBrush lawnGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LawnGreen']/Docs" />
		public static SolidColorBrush LawnGreen => lawnGreen ??= new(Colors.LawnGreen);
		static ImmutableBrush lemonChiffon;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LemonChiffon']/Docs" />
		public static SolidColorBrush LemonChiffon => lemonChiffon ??= new(Colors.LemonChiffon);
		static ImmutableBrush lightBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightBlue']/Docs" />
		public static SolidColorBrush LightBlue => lightBlue ??= new(Colors.LightBlue);
		static ImmutableBrush lightCoral;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightCoral']/Docs" />
		public static SolidColorBrush LightCoral => lightCoral ??= new(Colors.LightCoral);
		static ImmutableBrush lightCyan;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightCyan']/Docs" />
		public static SolidColorBrush LightCyan => lightCyan ??= new(Colors.LightCyan);
		static ImmutableBrush lightGoldenrodYellow;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightGoldenrodYellow']/Docs" />
		public static SolidColorBrush LightGoldenrodYellow => lightGoldenrodYellow ??= new(Colors.LightGoldenrodYellow);
		static ImmutableBrush lightGray;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightGray']/Docs" />
		public static SolidColorBrush LightGray => lightGray ??= new(Colors.LightGray);
		static ImmutableBrush lightGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightGreen']/Docs" />
		public static SolidColorBrush LightGreen => lightGreen ??= new(Colors.LightGreen);
		static ImmutableBrush lightPink;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightPink']/Docs" />
		public static SolidColorBrush LightPink => lightPink ??= new(Colors.LightPink);
		static ImmutableBrush lightSalmon;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightSalmon']/Docs" />
		public static SolidColorBrush LightSalmon => lightSalmon ??= new(Colors.LightSalmon);
		static ImmutableBrush lightSeaGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightSeaGreen']/Docs" />
		public static SolidColorBrush LightSeaGreen => lightSeaGreen ??= new(Colors.LightSeaGreen);
		static ImmutableBrush lightSkyBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightSkyBlue']/Docs" />
		public static SolidColorBrush LightSkyBlue => lightSkyBlue ??= new(Colors.LightSkyBlue);
		static ImmutableBrush lightSlateGray;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightSlateGray']/Docs" />
		public static SolidColorBrush LightSlateGray => lightSlateGray ??= new(Colors.LightSlateGray);
		static ImmutableBrush lightSteelBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightSteelBlue']/Docs" />
		public static SolidColorBrush LightSteelBlue => lightSteelBlue ??= new(Colors.LightSteelBlue);
		static ImmutableBrush lightYellow;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LightYellow']/Docs" />
		public static SolidColorBrush LightYellow => lightYellow ??= new(Colors.LightYellow);
		static ImmutableBrush lime;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Lime']/Docs" />
		public static SolidColorBrush Lime => lime ??= new(Colors.Lime);
		static ImmutableBrush limeGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='LimeGreen']/Docs" />
		public static SolidColorBrush LimeGreen => limeGreen ??= new(Colors.LimeGreen);
		static ImmutableBrush linen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Linen']/Docs" />
		public static SolidColorBrush Linen => linen ??= new(Colors.Linen);
		static ImmutableBrush magenta;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Magenta']/Docs" />
		public static SolidColorBrush Magenta => magenta ??= new(Colors.Magenta);
		static ImmutableBrush maroon;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Maroon']/Docs" />
		public static SolidColorBrush Maroon => maroon ??= new(Colors.Maroon);
		static ImmutableBrush mediumAquararine;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumAquamarine']/Docs" />
		public static SolidColorBrush MediumAquamarine => mediumAquararine ??= new(Colors.MediumAquamarine);
		static ImmutableBrush mediumBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumBlue']/Docs" />
		public static SolidColorBrush MediumBlue => mediumBlue ??= new(Colors.MediumBlue);
		static ImmutableBrush mediumOrchid;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumOrchid']/Docs" />
		public static SolidColorBrush MediumOrchid => mediumOrchid ??= new(Colors.MediumOrchid);
		static ImmutableBrush mediumPurple;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumPurple']/Docs" />
		public static SolidColorBrush MediumPurple => mediumPurple ??= new(Colors.MediumPurple);
		static ImmutableBrush mediumSeaGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumSeaGreen']/Docs" />
		public static SolidColorBrush MediumSeaGreen => mediumSeaGreen ??= new(Colors.MediumSeaGreen);
		static ImmutableBrush mediumSlateBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumSlateBlue']/Docs" />
		public static SolidColorBrush MediumSlateBlue => mediumSlateBlue ??= new(Colors.MediumSlateBlue);
		static ImmutableBrush mediumSpringGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumSpringGreen']/Docs" />
		public static SolidColorBrush MediumSpringGreen => mediumSpringGreen ??= new(Colors.MediumSpringGreen);
		static ImmutableBrush mediumTurquoise;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumTurquoise']/Docs" />
		public static SolidColorBrush MediumTurquoise => mediumTurquoise ??= new(Colors.MediumTurquoise);
		static ImmutableBrush mediumVioletRed;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MediumVioletRed']/Docs" />
		public static SolidColorBrush MediumVioletRed => mediumVioletRed ??= new(Colors.MediumVioletRed);
		static ImmutableBrush midnightBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MidnightBlue']/Docs" />
		public static SolidColorBrush MidnightBlue => midnightBlue ??= new(Colors.MidnightBlue);
		static ImmutableBrush mintCream;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MintCream']/Docs" />
		public static SolidColorBrush MintCream => mintCream ??= new(Colors.MintCream);
		static ImmutableBrush mistyRose;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='MistyRose']/Docs" />
		public static SolidColorBrush MistyRose => mistyRose ??= new(Colors.MistyRose);
		static ImmutableBrush moccasin;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Moccasin']/Docs" />
		public static SolidColorBrush Moccasin => moccasin ??= new(Colors.Moccasin);
		static ImmutableBrush navajoWhite;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='NavajoWhite']/Docs" />
		public static SolidColorBrush NavajoWhite => navajoWhite ??= new(Colors.NavajoWhite);
		static ImmutableBrush navy;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Navy']/Docs" />
		public static SolidColorBrush Navy => navy ??= new(Colors.Navy);
		static ImmutableBrush oldLace;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='OldLace']/Docs" />
		public static SolidColorBrush OldLace => oldLace ??= new(Colors.OldLace);
		static ImmutableBrush olive;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Olive']/Docs" />
		public static SolidColorBrush Olive => olive ??= new(Colors.Olive);
		static ImmutableBrush oliveDrab;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='OliveDrab']/Docs" />
		public static SolidColorBrush OliveDrab => oliveDrab ??= new(Colors.OliveDrab);
		static ImmutableBrush orange;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Orange']/Docs" />
		public static SolidColorBrush Orange => orange ??= new(Colors.Orange);
		static ImmutableBrush orangeRed;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='OrangeRed']/Docs" />
		public static SolidColorBrush OrangeRed => orangeRed ??= new(Colors.OrangeRed);
		static ImmutableBrush orchid;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Orchid']/Docs" />
		public static SolidColorBrush Orchid => orchid ??= new(Colors.Orchid);
		static ImmutableBrush paleGoldenrod;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='PaleGoldenrod']/Docs" />
		public static SolidColorBrush PaleGoldenrod => paleGoldenrod ??= new(Colors.PaleGoldenrod);
		static ImmutableBrush paleGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='PaleGreen']/Docs" />
		public static SolidColorBrush PaleGreen => paleGreen ??= new(Colors.PaleGreen);
		static ImmutableBrush paleTurquoise;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='PaleTurquoise']/Docs" />
		public static SolidColorBrush PaleTurquoise => paleTurquoise ??= new(Colors.PaleTurquoise);
		static ImmutableBrush paleVioletRed;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='PaleVioletRed']/Docs" />
		public static SolidColorBrush PaleVioletRed => paleVioletRed ??= new(Colors.PaleVioletRed);
		static ImmutableBrush papayaWhip;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='PapayaWhip']/Docs" />
		public static SolidColorBrush PapayaWhip => papayaWhip ??= new(Colors.PapayaWhip);
		static ImmutableBrush peachPuff;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='PeachPuff']/Docs" />
		public static SolidColorBrush PeachPuff => peachPuff ??= new(Colors.PeachPuff);
		static ImmutableBrush peru;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Peru']/Docs" />
		public static SolidColorBrush Peru => peru ??= new(Colors.Peru);
		static ImmutableBrush pink;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Pink']/Docs" />
		public static SolidColorBrush Pink => pink ??= new(Colors.Pink);
		static ImmutableBrush plum;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Plum']/Docs" />
		public static SolidColorBrush Plum => plum ??= new(Colors.Plum);
		static ImmutableBrush powderBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='PowderBlue']/Docs" />
		public static SolidColorBrush PowderBlue => powderBlue ??= new(Colors.PowderBlue);
		static ImmutableBrush purple;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Purple']/Docs" />
		public static SolidColorBrush Purple => purple ??= new(Colors.Purple);
		static ImmutableBrush red;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Red']/Docs" />
		public static SolidColorBrush Red => red ??= new(Colors.Red);
		static ImmutableBrush rosyBrown;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='RosyBrown']/Docs" />
		public static SolidColorBrush RosyBrown => rosyBrown ??= new(Colors.RosyBrown);
		static ImmutableBrush royalBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='RoyalBlue']/Docs" />
		public static SolidColorBrush RoyalBlue => royalBlue ??= new(Colors.RoyalBlue);
		static ImmutableBrush saddleBrown;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SaddleBrown']/Docs" />
		public static SolidColorBrush SaddleBrown => saddleBrown ??= new(Colors.SaddleBrown);
		static ImmutableBrush salmon;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Salmon']/Docs" />
		public static SolidColorBrush Salmon => salmon ??= new(Colors.Salmon);
		static ImmutableBrush sandyBrown;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SandyBrown']/Docs" />
		public static SolidColorBrush SandyBrown => sandyBrown ??= new(Colors.SandyBrown);
		static ImmutableBrush seaGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SeaGreen']/Docs" />
		public static SolidColorBrush SeaGreen => seaGreen ??= new(Colors.SeaGreen);
		static ImmutableBrush seaShell;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SeaShell']/Docs" />
		public static SolidColorBrush SeaShell => seaShell ??= new(Colors.SeaShell);
		static ImmutableBrush sienna;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Sienna']/Docs" />
		public static SolidColorBrush Sienna => sienna ??= new(Colors.Sienna);
		static ImmutableBrush silver;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Silver']/Docs" />
		public static SolidColorBrush Silver => silver ??= new(Colors.Silver);
		static ImmutableBrush skyBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SkyBlue']/Docs" />
		public static SolidColorBrush SkyBlue => skyBlue ??= new(Colors.SkyBlue);
		static ImmutableBrush slateBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SlateBlue']/Docs" />
		public static SolidColorBrush SlateBlue => slateBlue ??= new(Colors.SlateBlue);
		static ImmutableBrush slateGray;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SlateGray']/Docs" />
		public static SolidColorBrush SlateGray => slateGray ??= new(Colors.SlateGray);
		static ImmutableBrush snow;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Snow']/Docs" />
		public static SolidColorBrush Snow => snow ??= new(Colors.Snow);
		static ImmutableBrush springGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SpringGreen']/Docs" />
		public static SolidColorBrush SpringGreen => springGreen ??= new(Colors.SpringGreen);
		static ImmutableBrush steelBlue;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='SteelBlue']/Docs" />
		public static SolidColorBrush SteelBlue => steelBlue ??= new(Colors.SteelBlue);
		static ImmutableBrush tan;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Tan']/Docs" />
		public static SolidColorBrush Tan => tan ??= new(Colors.Tan);
		static ImmutableBrush teal;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Teal']/Docs" />
		public static SolidColorBrush Teal => teal ??= new(Colors.Teal);
		static ImmutableBrush thistle;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Thistle']/Docs" />
		public static SolidColorBrush Thistle => thistle ??= new(Colors.Thistle);
		static ImmutableBrush tomato;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Tomato']/Docs" />
		public static SolidColorBrush Tomato => tomato ??= new(Colors.Tomato);
		static ImmutableBrush transparent;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Transparent']/Docs" />
		public static SolidColorBrush Transparent => transparent ??= new(Colors.Transparent);
		static ImmutableBrush turquoise;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Turquoise']/Docs" />
		public static SolidColorBrush Turquoise => turquoise ??= new(Colors.Turquoise);
		static ImmutableBrush violet;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Violet']/Docs" />
		public static SolidColorBrush Violet => violet ??= new(Colors.Violet);
		static ImmutableBrush wheat;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Wheat']/Docs" />
		public static SolidColorBrush Wheat => wheat ??= new(Colors.Wheat);
		static ImmutableBrush white;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='White']/Docs" />
		public static SolidColorBrush White => white ??= new(Colors.White);
		static ImmutableBrush whiteSmoke;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='WhiteSmoke']/Docs" />
		public static SolidColorBrush WhiteSmoke => whiteSmoke ??= new(Colors.WhiteSmoke);
		static ImmutableBrush yellow;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='Yellow']/Docs" />
		public static SolidColorBrush Yellow => yellow ??= new(Colors.Yellow);
		static ImmutableBrush yellowGreen;
		/// <include file="../../docs/Microsoft.Maui.Controls/Brush.xml" path="//Member[@MemberName='YellowGreen']/Docs" />
		public static SolidColorBrush YellowGreen => yellowGreen ??= new(Colors.YellowGreen);
	}
}
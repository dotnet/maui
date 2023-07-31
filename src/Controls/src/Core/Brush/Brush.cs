#nullable disable
using Microsoft.Maui.Graphics;
using GraphicsGradientStop = Microsoft.Maui.Graphics.PaintGradientStop;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Defines the core behavior and built-in colors for painting an area.
	/// </summary>
	/// <remarks>Derived classes describe different ways of painting an area.</remarks>
	[System.ComponentModel.TypeConverter(typeof(BrushTypeConverter))]
	public abstract partial class Brush : Element
	{
		public static implicit operator Brush(Paint paint)
		{
			if (paint is SolidPaint solidPaint)
				return new SolidColorBrush { Color = solidPaint.Color };

			if (paint is GradientPaint gradientPaint)
			{
				var gradientStopCollection = gradientPaint.GradientStops;

				GradientStopCollection gradientStops = new GradientStopCollection();

				for (int i = 0; i < gradientStopCollection.Length; i++)
				{
					var gs = gradientStopCollection[i];
					gradientStops.Insert(i, new GradientStop(gs.Color, gs.Offset));
				}

				if (gradientPaint is LinearGradientPaint linearGradientPaint)
				{
					var startPoint = linearGradientPaint.StartPoint;
					var endPoint = linearGradientPaint.EndPoint;

					return new LinearGradientBrush { GradientStops = gradientStops, StartPoint = startPoint, EndPoint = endPoint };
				}

				if (gradientPaint is RadialGradientPaint radialGradientPaint)
				{
					var center = radialGradientPaint.Center;
					var radius = radialGradientPaint.Radius;

					return new RadialGradientBrush { GradientStops = gradientStops, Center = center, Radius = radius };
				}
			}

			if (paint is ImageSourcePaint imageSourcePaint && imageSourcePaint.ImageSource is ImageSource imageSource)
				return new ImageBrush { ImageSource = imageSource };

			return null;
		}

		public static implicit operator Paint(Brush brush)
		{
			if (brush is SolidColorBrush solidColorBrush)
				return new SolidPaint { Color = solidColorBrush.Color };

			if (brush is GradientBrush gradientBrush)
			{
				var gradientStopCollection = gradientBrush.GradientStops;

				GraphicsGradientStop[] gradientStops = new GraphicsGradientStop[gradientStopCollection.Count];

				for (int i = 0; i < gradientStopCollection.Count; i++)
				{
					var gs = gradientStopCollection[i];
					gradientStops[i] = new GraphicsGradientStop(gs.Offset, gs.Color);
				}

				if (gradientBrush is LinearGradientBrush linearGradientBrush)
				{
					var startPoint = linearGradientBrush.StartPoint;
					var endPoint = linearGradientBrush.EndPoint;

					return new LinearGradientPaint { GradientStops = gradientStops, StartPoint = startPoint, EndPoint = endPoint };
				}

				if (gradientBrush is RadialGradientBrush radialGradientBrush)
				{
					var center = radialGradientBrush.Center;
					var radius = radialGradientBrush.Radius;

					return new RadialGradientPaint { GradientStops = gradientStops, Center = center, Radius = radius };
				}
			}

			if (brush is ImageBrush imageBrush)
				return new ImageSourcePaint { ImageSource = imageBrush.ImageSource };

			return null;
		}

		static ImmutableBrush defaultBrush;

		/// <summary>
		/// Represents the default (empty) brush.
		/// </summary>
		public static Brush Default => defaultBrush ??= new(null);

		public static implicit operator Brush(Color color) => new SolidColorBrush(color);

		/// <summary>
		/// When overridden in a derived class, indicates whether the given brush represents the empty brush.
		/// </summary>
		public abstract bool IsEmpty { get; }

		/// <summary>
		/// Indicates whether the specified <see cref="Brush"/> is <see langword="null"/> or empty.
		/// </summary>
		/// <returns><see langword="true"/> if the brush is null or empty; <see langword="false"/> otherwise.</returns>
		public static bool IsNullOrEmpty(Brush brush)
		{
			return brush == null || brush.IsEmpty;
		}

		static ImmutableBrush aliceBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF0F8FF</c>.</summary>
		public static SolidColorBrush AliceBlue => aliceBlue ??= new(Colors.AliceBlue);
		static ImmutableBrush antiqueWhite;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFAEBD7</c>.</summary>
		public static SolidColorBrush AntiqueWhite => antiqueWhite ??= new(Colors.AntiqueWhite);
		static ImmutableBrush aqua;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00FFFF</c>.</summary>
		public static SolidColorBrush Aqua => aqua ??= new(Colors.Aqua);
		static ImmutableBrush aquamarine;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF7FFFD4</c>.</summary>
		public static SolidColorBrush Aquamarine => aquamarine ??= new(Colors.Aquamarine);
		static ImmutableBrush azure;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF0FFFF</c>.</summary>
		public static SolidColorBrush Azure => azure ??= new(Colors.Azure);
		static ImmutableBrush beige;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF5F5DC</c>.</summary>
		public static SolidColorBrush Beige => beige ??= new(Colors.Beige);
		static ImmutableBrush bisque;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFE4C4</c>.</summary>
		public static SolidColorBrush Bisque => bisque ??= new(Colors.Bisque);
		static ImmutableBrush black;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF000000</c>.</summary>
		public static SolidColorBrush Black => black ??= new(Colors.Black);
		static ImmutableBrush blanchedAlmond;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFEBCD</c>.</summary>
		public static SolidColorBrush BlanchedAlmond => blanchedAlmond ??= new(Colors.BlanchedAlmond);
		static ImmutableBrush blue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF0000FF</c>.</summary>
		public static SolidColorBrush Blue => blue ??= new(Colors.Blue);
		static ImmutableBrush blueViolet;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF8A2BE2</c>.</summary>
		public static SolidColorBrush BlueViolet => blueViolet ??= new(Colors.BlueViolet);
		static ImmutableBrush brown;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFA52A2A</c>.</summary>
		public static SolidColorBrush Brown => brown ??= new(Colors.Brown);
		static ImmutableBrush burlyWood;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFDEB887</c>.</summary>
		public static SolidColorBrush BurlyWood => burlyWood ??= new(Colors.BurlyWood);
		static ImmutableBrush cadetBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF5F9EA0</c>.</summary>
		public static SolidColorBrush CadetBlue => cadetBlue ??= new(Colors.CadetBlue);
		static ImmutableBrush chartreuse;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF7FFF00</c>.</summary>
		public static SolidColorBrush Chartreuse => chartreuse ??= new(Colors.Chartreuse);
		static ImmutableBrush chocolate;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFD2691E</c>.</summary>
		public static SolidColorBrush Chocolate => chocolate ??= new(Colors.Chocolate);
		static ImmutableBrush coral;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF7F50</c>.</summary>
		public static SolidColorBrush Coral => coral ??= new(Colors.Coral);
		static ImmutableBrush cornflowerBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF6495ED</c>.</summary>
		public static SolidColorBrush CornflowerBlue => cornflowerBlue ??= new(Colors.CornflowerBlue);
		static ImmutableBrush cornsilk;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFF8DC</c>.</summary>
		public static SolidColorBrush Cornsilk => cornsilk ??= new(Colors.Cornsilk);
		static ImmutableBrush crimson;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFDC143C</c>.</summary>
		public static SolidColorBrush Crimson => crimson ??= new(Colors.Crimson);
		static ImmutableBrush cyan;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00FFFF</c>.</summary>
		public static SolidColorBrush Cyan => cyan ??= new(Colors.Cyan);
		static ImmutableBrush darkBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00008B</c>.</summary>
		public static SolidColorBrush DarkBlue => darkBlue ??= new(Colors.DarkBlue);
		static ImmutableBrush darkCyan;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF008B8B</c>.</summary>
		public static SolidColorBrush DarkCyan => darkCyan ??= new(Colors.DarkCyan);
		static ImmutableBrush darkGoldenrod;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFB8860B</c>.</summary>
		public static SolidColorBrush DarkGoldenrod => darkGoldenrod ??= new(Colors.DarkGoldenrod);
		static ImmutableBrush darkGray;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFA9A9A9</c>.</summary>
		public static SolidColorBrush DarkGray => darkGray ??= new(Colors.DarkGray);
		static ImmutableBrush darkGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF006400</c>.</summary>
		public static SolidColorBrush DarkGreen => darkGreen ??= new(Colors.DarkGreen);
		static ImmutableBrush darkKhaki;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFBDB76B</c>.</summary>
		public static SolidColorBrush DarkKhaki => darkKhaki ??= new(Colors.DarkKhaki);
		static ImmutableBrush darkMagenta;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF8B008B</c>.</summary>
		public static SolidColorBrush DarkMagenta => darkMagenta ??= new(Colors.DarkMagenta);
		static ImmutableBrush darkOliveGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF556B2F</c>.</summary>
		public static SolidColorBrush DarkOliveGreen => darkOliveGreen ??= new(Colors.DarkOliveGreen);
		static ImmutableBrush darkOrange;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF8C00</c>.</summary>
		public static SolidColorBrush DarkOrange => darkOrange ??= new(Colors.DarkOrange);
		static ImmutableBrush darkOrchid;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF9932CC</c>.</summary>
		public static SolidColorBrush DarkOrchid => darkOrchid ??= new(Colors.DarkOrchid);
		static ImmutableBrush darkRed;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF8B0000</c>.</summary>
		public static SolidColorBrush DarkRed => darkRed ??= new(Colors.DarkRed);
		static ImmutableBrush darkSalmon;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFE9967A</c>.</summary>
		public static SolidColorBrush DarkSalmon => darkSalmon ??= new(Colors.DarkSalmon);
		static ImmutableBrush darkSeaGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF8FBC8F</c>.</summary>
		public static SolidColorBrush DarkSeaGreen => darkSeaGreen ??= new(Colors.DarkSeaGreen);
		static ImmutableBrush darkSlateBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF483D8B</c>.</summary>
		public static SolidColorBrush DarkSlateBlue => darkSlateBlue ??= new(Colors.DarkSlateBlue);
		static ImmutableBrush darkSlateGray;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF2F4F4F</c>.</summary>
		public static SolidColorBrush DarkSlateGray => darkSlateGray ??= new(Colors.DarkSlateGray);
		static ImmutableBrush darkTurquoise;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00CED1</c>.</summary>
		public static SolidColorBrush DarkTurquoise => darkTurquoise ??= new(Colors.DarkTurquoise);
		static ImmutableBrush darkViolet;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF9400D3</c>.</summary>
		public static SolidColorBrush DarkViolet => darkViolet ??= new(Colors.DarkViolet);
		static ImmutableBrush deepPink;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF1493</c>.</summary>
		public static SolidColorBrush DeepPink => deepPink ??= new(Colors.DeepPink);
		static ImmutableBrush deepSkyBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00BFFF</c>.</summary>
		public static SolidColorBrush DeepSkyBlue => deepSkyBlue ??= new(Colors.DeepSkyBlue);
		static ImmutableBrush dimGray;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF696969</c>.</summary>
		public static SolidColorBrush DimGray => dimGray ??= new(Colors.DimGray);
		static ImmutableBrush dodgerBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF1E90FF</c>.</summary>
		public static SolidColorBrush DodgerBlue => dodgerBlue ??= new(Colors.DodgerBlue);
		static ImmutableBrush firebrick;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFB22222</c>.</summary>
		public static SolidColorBrush Firebrick => firebrick ??= new(Colors.Firebrick);
		static ImmutableBrush floralWhite;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFFAF0</c>.</summary>
		public static SolidColorBrush FloralWhite => floralWhite ??= new(Colors.FloralWhite);
		static ImmutableBrush forestGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF228B22</c>.</summary>
		public static SolidColorBrush ForestGreen => forestGreen ??= new(Colors.ForestGreen);
		static ImmutableBrush fuschia;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF00FF</c>.</summary>
		public static SolidColorBrush Fuchsia => fuschia ??= new(Colors.Fuchsia);
		static ImmutableBrush gainsboro;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFDCDCDC</c>.</summary>
		public static SolidColorBrush Gainsboro => gainsboro ??= new(Colors.Gainsboro);
		static ImmutableBrush ghostWhite;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF8F8FF</c>.</summary>
		public static SolidColorBrush GhostWhite => ghostWhite ??= new(Colors.GhostWhite);
		static ImmutableBrush gold;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFD700</c>.</summary>
		public static SolidColorBrush Gold => gold ??= new(Colors.Gold);
		static ImmutableBrush goldenrod;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFDAA520</c>.</summary>
		public static SolidColorBrush Goldenrod => goldenrod ??= new(Colors.Goldenrod);
		static ImmutableBrush gray;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF808080</c>.</summary>
		public static SolidColorBrush Gray => gray ??= new(Colors.Gray);
		static ImmutableBrush green;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF008000</c>.</summary>
		public static SolidColorBrush Green => green ??= new(Colors.Green);
		static ImmutableBrush greenYellow;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFADFF2F</c>.</summary>
		public static SolidColorBrush GreenYellow => greenYellow ??= new(Colors.GreenYellow);
		static ImmutableBrush honeydew;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF0FFF0</c>.</summary>
		public static SolidColorBrush Honeydew => honeydew ??= new(Colors.Honeydew);
		static ImmutableBrush hotPink;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF69B4</c>.</summary>
		public static SolidColorBrush HotPink => hotPink ??= new(Colors.HotPink);
		static ImmutableBrush indianRed;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFCD5C5C</c>.</summary>
		public static SolidColorBrush IndianRed => indianRed ??= new(Colors.IndianRed);
		static ImmutableBrush indigo;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF4B0082</c>.</summary>
		public static SolidColorBrush Indigo => indigo ??= new(Colors.Indigo);
		static ImmutableBrush ivory;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFFFF0</c>.</summary>
		public static SolidColorBrush Ivory => ivory ??= new(Colors.Ivory);
		static ImmutableBrush khaki;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF0E68C</c>.</summary>
		public static SolidColorBrush Khaki => khaki ??= new(Colors.Khaki);
		static ImmutableBrush lavender;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFE6E6FA</c>.</summary>
		public static SolidColorBrush Lavender => lavender ??= new(Colors.Lavender);
		static ImmutableBrush lavenderBlush;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFF0F5</c>.</summary>
		public static SolidColorBrush LavenderBlush => lavenderBlush ??= new(Colors.LavenderBlush);
		static ImmutableBrush lawnGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF7CFC00</c>.</summary>
		public static SolidColorBrush LawnGreen => lawnGreen ??= new(Colors.LawnGreen);
		static ImmutableBrush lemonChiffon;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFFACD</c>.</summary>
		public static SolidColorBrush LemonChiffon => lemonChiffon ??= new(Colors.LemonChiffon);
		static ImmutableBrush lightBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFADD8E6</c>.</summary>
		public static SolidColorBrush LightBlue => lightBlue ??= new(Colors.LightBlue);
		static ImmutableBrush lightCoral;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF08080</c>.</summary>
		public static SolidColorBrush LightCoral => lightCoral ??= new(Colors.LightCoral);
		static ImmutableBrush lightCyan;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFE0FFFF</c>.</summary>
		public static SolidColorBrush LightCyan => lightCyan ??= new(Colors.LightCyan);
		static ImmutableBrush lightGoldenrodYellow;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFAFAD2</c>.</summary>
		public static SolidColorBrush LightGoldenrodYellow => lightGoldenrodYellow ??= new(Colors.LightGoldenrodYellow);
		static ImmutableBrush lightGray;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFD3D3D3</c>.</summary>
		public static SolidColorBrush LightGray => lightGray ??= new(Colors.LightGray);
		static ImmutableBrush lightGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF90EE90</c>.</summary>
		public static SolidColorBrush LightGreen => lightGreen ??= new(Colors.LightGreen);
		static ImmutableBrush lightPink;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFB6C1</c>.</summary>
		public static SolidColorBrush LightPink => lightPink ??= new(Colors.LightPink);
		static ImmutableBrush lightSalmon;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFA07A</c>.</summary>
		public static SolidColorBrush LightSalmon => lightSalmon ??= new(Colors.LightSalmon);
		static ImmutableBrush lightSeaGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF20B2AA</c>.</summary>
		public static SolidColorBrush LightSeaGreen => lightSeaGreen ??= new(Colors.LightSeaGreen);
		static ImmutableBrush lightSkyBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF87CEFA</c>.</summary>
		public static SolidColorBrush LightSkyBlue => lightSkyBlue ??= new(Colors.LightSkyBlue);
		static ImmutableBrush lightSlateGray;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF778899</c>.</summary>
		public static SolidColorBrush LightSlateGray => lightSlateGray ??= new(Colors.LightSlateGray);
		static ImmutableBrush lightSteelBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFB0C4DE</c>.</summary>
		public static SolidColorBrush LightSteelBlue => lightSteelBlue ??= new(Colors.LightSteelBlue);
		static ImmutableBrush lightYellow;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFFFE0</c>.</summary>
		public static SolidColorBrush LightYellow => lightYellow ??= new(Colors.LightYellow);
		static ImmutableBrush lime;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00FF00</c>.</summary>
		public static SolidColorBrush Lime => lime ??= new(Colors.Lime);
		static ImmutableBrush limeGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF32CD32</c>.</summary>
		public static SolidColorBrush LimeGreen => limeGreen ??= new(Colors.LimeGreen);
		static ImmutableBrush linen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFAF0E6</c>.</summary>
		public static SolidColorBrush Linen => linen ??= new(Colors.Linen);
		static ImmutableBrush magenta;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF00FF</c>.</summary>
		public static SolidColorBrush Magenta => magenta ??= new(Colors.Magenta);
		static ImmutableBrush maroon;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF800000</c>.</summary>
		public static SolidColorBrush Maroon => maroon ??= new(Colors.Maroon);
		static ImmutableBrush mediumAquararine;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF66CDAA</c>.</summary>
		public static SolidColorBrush MediumAquamarine => mediumAquararine ??= new(Colors.MediumAquamarine);
		static ImmutableBrush mediumBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF0000CD</c>.</summary>
		public static SolidColorBrush MediumBlue => mediumBlue ??= new(Colors.MediumBlue);
		static ImmutableBrush mediumOrchid;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFBA55D3</c>.</summary>
		public static SolidColorBrush MediumOrchid => mediumOrchid ??= new(Colors.MediumOrchid);
		static ImmutableBrush mediumPurple;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF9370D8</c>.</summary>
		public static SolidColorBrush MediumPurple => mediumPurple ??= new(Colors.MediumPurple);
		static ImmutableBrush mediumSeaGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF3CB371</c>.</summary>
		public static SolidColorBrush MediumSeaGreen => mediumSeaGreen ??= new(Colors.MediumSeaGreen);
		static ImmutableBrush mediumSlateBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF7B68EE</c>.</summary>
		public static SolidColorBrush MediumSlateBlue => mediumSlateBlue ??= new(Colors.MediumSlateBlue);
		static ImmutableBrush mediumSpringGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00FA9A</c>.</summary>
		public static SolidColorBrush MediumSpringGreen => mediumSpringGreen ??= new(Colors.MediumSpringGreen);
		static ImmutableBrush mediumTurquoise;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF48D1CC</c>.</summary>
		public static SolidColorBrush MediumTurquoise => mediumTurquoise ??= new(Colors.MediumTurquoise);
		static ImmutableBrush mediumVioletRed;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFC71585</c>.</summary>
		public static SolidColorBrush MediumVioletRed => mediumVioletRed ??= new(Colors.MediumVioletRed);
		static ImmutableBrush midnightBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF191970</c>.</summary>
		public static SolidColorBrush MidnightBlue => midnightBlue ??= new(Colors.MidnightBlue);
		static ImmutableBrush mintCream;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF5FFFA</c>.</summary>
		public static SolidColorBrush MintCream => mintCream ??= new(Colors.MintCream);
		static ImmutableBrush mistyRose;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFE4E1</c>.</summary>
		public static SolidColorBrush MistyRose => mistyRose ??= new(Colors.MistyRose);
		static ImmutableBrush moccasin;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFE4B5</c>.</summary>
		public static SolidColorBrush Moccasin => moccasin ??= new(Colors.Moccasin);
		static ImmutableBrush navajoWhite;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFDEAD</c>.</summary>
		public static SolidColorBrush NavajoWhite => navajoWhite ??= new(Colors.NavajoWhite);
		static ImmutableBrush navy;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF000080</c>.</summary>
		public static SolidColorBrush Navy => navy ??= new(Colors.Navy);
		static ImmutableBrush oldLace;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFDF5E6</c>.</summary>
		public static SolidColorBrush OldLace => oldLace ??= new(Colors.OldLace);
		static ImmutableBrush olive;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF808000</c>.</summary>
		public static SolidColorBrush Olive => olive ??= new(Colors.Olive);
		static ImmutableBrush oliveDrab;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF6B8E23</c>.</summary>
		public static SolidColorBrush OliveDrab => oliveDrab ??= new(Colors.OliveDrab);
		static ImmutableBrush orange;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFA500</c>.</summary>
		public static SolidColorBrush Orange => orange ??= new(Colors.Orange);
		static ImmutableBrush orangeRed;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF4500</c>.</summary>
		public static SolidColorBrush OrangeRed => orangeRed ??= new(Colors.OrangeRed);
		static ImmutableBrush orchid;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFDA70D6</c>.</summary>
		public static SolidColorBrush Orchid => orchid ??= new(Colors.Orchid);
		static ImmutableBrush paleGoldenrod;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFEEE8AA</c>.</summary>
		public static SolidColorBrush PaleGoldenrod => paleGoldenrod ??= new(Colors.PaleGoldenrod);
		static ImmutableBrush paleGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF98FB98</c>.</summary>
		public static SolidColorBrush PaleGreen => paleGreen ??= new(Colors.PaleGreen);
		static ImmutableBrush paleTurquoise;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFAFEEEE</c>.</summary>
		public static SolidColorBrush PaleTurquoise => paleTurquoise ??= new(Colors.PaleTurquoise);
		static ImmutableBrush paleVioletRed;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFD87093</c>.</summary>
		public static SolidColorBrush PaleVioletRed => paleVioletRed ??= new(Colors.PaleVioletRed);
		static ImmutableBrush papayaWhip;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFEFD5</c>.</summary>
		public static SolidColorBrush PapayaWhip => papayaWhip ??= new(Colors.PapayaWhip);
		static ImmutableBrush peachPuff;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFDAB9</c>.</summary>
		public static SolidColorBrush PeachPuff => peachPuff ??= new(Colors.PeachPuff);
		static ImmutableBrush peru;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFCD853F</c>.</summary>
		public static SolidColorBrush Peru => peru ??= new(Colors.Peru);
		static ImmutableBrush pink;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFC0CB</c>.</summary>
		public static SolidColorBrush Pink => pink ??= new(Colors.Pink);
		static ImmutableBrush plum;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFDDA0DD</c>.</summary>
		public static SolidColorBrush Plum => plum ??= new(Colors.Plum);
		static ImmutableBrush powderBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFB0E0E6</c>.</summary>
		public static SolidColorBrush PowderBlue => powderBlue ??= new(Colors.PowderBlue);
		static ImmutableBrush purple;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF800080</c>.</summary>
		public static SolidColorBrush Purple => purple ??= new(Colors.Purple);
		static ImmutableBrush red;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF0000</c>.</summary>
		public static SolidColorBrush Red => red ??= new(Colors.Red);
		static ImmutableBrush rosyBrown;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFBC8F8F</c>.</summary>
		public static SolidColorBrush RosyBrown => rosyBrown ??= new(Colors.RosyBrown);
		static ImmutableBrush royalBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF4169E1</c>.</summary>
		public static SolidColorBrush RoyalBlue => royalBlue ??= new(Colors.RoyalBlue);
		static ImmutableBrush saddleBrown;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF8B4513</c>.</summary>
		public static SolidColorBrush SaddleBrown => saddleBrown ??= new(Colors.SaddleBrown);
		static ImmutableBrush salmon;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFA8072</c>.</summary>
		public static SolidColorBrush Salmon => salmon ??= new(Colors.Salmon);
		static ImmutableBrush sandyBrown;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF4A460</c>.</summary>
		public static SolidColorBrush SandyBrown => sandyBrown ??= new(Colors.SandyBrown);
		static ImmutableBrush seaGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF2E8B57</c>.</summary>
		public static SolidColorBrush SeaGreen => seaGreen ??= new(Colors.SeaGreen);
		static ImmutableBrush seaShell;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFF5EE</c>.</summary>
		public static SolidColorBrush SeaShell => seaShell ??= new(Colors.SeaShell);
		static ImmutableBrush sienna;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFA0522D</c>.</summary>
		public static SolidColorBrush Sienna => sienna ??= new(Colors.Sienna);
		static ImmutableBrush silver;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFC0C0C0</c>.</summary>
		public static SolidColorBrush Silver => silver ??= new(Colors.Silver);
		static ImmutableBrush skyBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF87CEEB</c>.</summary>
		public static SolidColorBrush SkyBlue => skyBlue ??= new(Colors.SkyBlue);
		static ImmutableBrush slateBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF6A5ACD</c>.</summary>
		public static SolidColorBrush SlateBlue => slateBlue ??= new(Colors.SlateBlue);
		static ImmutableBrush slateGray;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF708090</c>.</summary>
		public static SolidColorBrush SlateGray => slateGray ??= new(Colors.SlateGray);
		static ImmutableBrush snow;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFFAFA</c>.</summary>
		public static SolidColorBrush Snow => snow ??= new(Colors.Snow);
		static ImmutableBrush springGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF00FF7F</c>.</summary>
		public static SolidColorBrush SpringGreen => springGreen ??= new(Colors.SpringGreen);
		static ImmutableBrush steelBlue;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF4682B4</c>.</summary>
		public static SolidColorBrush SteelBlue => steelBlue ??= new(Colors.SteelBlue);
		static ImmutableBrush tan;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFD2B48C</c>.</summary>
		public static SolidColorBrush Tan => tan ??= new(Colors.Tan);
		static ImmutableBrush teal;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF008080</c>.</summary>
		public static SolidColorBrush Teal => teal ??= new(Colors.Teal);
		static ImmutableBrush thistle;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFD8BFD8</c>.</summary>
		public static SolidColorBrush Thistle => thistle ??= new(Colors.Thistle);
		static ImmutableBrush tomato;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFF6347</c>.</summary>
		public static SolidColorBrush Tomato => tomato ??= new(Colors.Tomato);
		static ImmutableBrush transparent;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#00000000</c>.</summary>
		public static SolidColorBrush Transparent => transparent ??= new(Colors.Transparent);
		static ImmutableBrush turquoise;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF40E0D0</c>.</summary>
		public static SolidColorBrush Turquoise => turquoise ??= new(Colors.Turquoise);
		static ImmutableBrush violet;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFEE82EE</c>.</summary>
		public static SolidColorBrush Violet => violet ??= new(Colors.Violet);
		static ImmutableBrush wheat;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF5DEB3</c>.</summary>
		public static SolidColorBrush Wheat => wheat ??= new(Colors.Wheat);
		static ImmutableBrush white;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFFFFF</c>.</summary>
		public static SolidColorBrush White => white ??= new(Colors.White);
		static ImmutableBrush whiteSmoke;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFF5F5F5</c>.</summary>
		public static SolidColorBrush WhiteSmoke => whiteSmoke ??= new(Colors.WhiteSmoke);
		static ImmutableBrush yellow;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FFFFFF00</c>.</summary>
		public static SolidColorBrush Yellow => yellow ??= new(Colors.Yellow);
		static ImmutableBrush yellowGreen;
		/// <summary>Gets the system-defined color that has an ARGB value of <c>#FF9ACD32</c>.</summary>
		public static SolidColorBrush YellowGreen => yellowGreen ??= new(Colors.YellowGreen);
	}
}
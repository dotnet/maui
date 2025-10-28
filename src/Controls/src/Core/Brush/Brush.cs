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

					if (gs is not null)
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

		internal static bool HasTransparentBackground(Brush background)
		{
			if (IsNullOrEmpty(background))
			{
				return false;
			}

			if (background is SolidColorBrush solidColorBrush)
			{
				return solidColorBrush.Color?.Alpha < 1;
			}

			if (background is GradientBrush gradientBrush && gradientBrush.GradientStops is not null)
			{
				// Check if any gradient stop has transparency
				foreach (var stop in gradientBrush.GradientStops)
				{
					if (stop.Color?.Alpha < 1)
					{
						return true;
					}
				}
			}

			return false;
		}

		static ImmutableBrush aliceBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.AliceBlue"/>.</summary>
		public static SolidColorBrush AliceBlue => aliceBlue ??= new(Colors.AliceBlue);
		static ImmutableBrush antiqueWhite;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.AntiqueWhite"/>.</summary>
		public static SolidColorBrush AntiqueWhite => antiqueWhite ??= new(Colors.AntiqueWhite);
		static ImmutableBrush aqua;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Aqua"/>.</summary>
		public static SolidColorBrush Aqua => aqua ??= new(Colors.Aqua);
		static ImmutableBrush aquamarine;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Aquamarine"/>.</summary>
		public static SolidColorBrush Aquamarine => aquamarine ??= new(Colors.Aquamarine);
		static ImmutableBrush azure;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Azure"/>.</summary>
		public static SolidColorBrush Azure => azure ??= new(Colors.Azure);
		static ImmutableBrush beige;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Beige"/>.</summary>
		public static SolidColorBrush Beige => beige ??= new(Colors.Beige);
		static ImmutableBrush bisque;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Bisque"/>.</summary>
		public static SolidColorBrush Bisque => bisque ??= new(Colors.Bisque);
		static ImmutableBrush black;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Black"/>.</summary>
		public static SolidColorBrush Black => black ??= new(Colors.Black);
		static ImmutableBrush blanchedAlmond;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.BlanchedAlmond"/>.</summary>
		public static SolidColorBrush BlanchedAlmond => blanchedAlmond ??= new(Colors.BlanchedAlmond);
		static ImmutableBrush blue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Blue"/>.</summary>
		public static SolidColorBrush Blue => blue ??= new(Colors.Blue);
		static ImmutableBrush blueViolet;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.BlueViolet"/>.</summary>
		public static SolidColorBrush BlueViolet => blueViolet ??= new(Colors.BlueViolet);
		static ImmutableBrush brown;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Brown"/>.</summary>
		public static SolidColorBrush Brown => brown ??= new(Colors.Brown);
		static ImmutableBrush burlyWood;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.BurlyWood"/>.</summary>
		public static SolidColorBrush BurlyWood => burlyWood ??= new(Colors.BurlyWood);
		static ImmutableBrush cadetBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.CadetBlue"/>.</summary>
		public static SolidColorBrush CadetBlue => cadetBlue ??= new(Colors.CadetBlue);
		static ImmutableBrush chartreuse;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Chartreuse"/>.</summary>
		public static SolidColorBrush Chartreuse => chartreuse ??= new(Colors.Chartreuse);
		static ImmutableBrush chocolate;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Chocolate"/>.</summary>
		public static SolidColorBrush Chocolate => chocolate ??= new(Colors.Chocolate);
		static ImmutableBrush coral;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Coral"/>.</summary>
		public static SolidColorBrush Coral => coral ??= new(Colors.Coral);
		static ImmutableBrush cornflowerBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.CornflowerBlue"/>.</summary>
		public static SolidColorBrush CornflowerBlue => cornflowerBlue ??= new(Colors.CornflowerBlue);
		static ImmutableBrush cornsilk;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Cornsilk"/>.</summary>
		public static SolidColorBrush Cornsilk => cornsilk ??= new(Colors.Cornsilk);
		static ImmutableBrush crimson;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Crimson"/>.</summary>
		public static SolidColorBrush Crimson => crimson ??= new(Colors.Crimson);
		static ImmutableBrush cyan;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Cyan"/>.</summary>
		public static SolidColorBrush Cyan => cyan ??= new(Colors.Cyan);
		static ImmutableBrush darkBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkBlue"/>.</summary>
		public static SolidColorBrush DarkBlue => darkBlue ??= new(Colors.DarkBlue);
		static ImmutableBrush darkCyan;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkCyan"/>.</summary>
		public static SolidColorBrush DarkCyan => darkCyan ??= new(Colors.DarkCyan);
		static ImmutableBrush darkGoldenrod;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkGoldenrod"/>.</summary>
		public static SolidColorBrush DarkGoldenrod => darkGoldenrod ??= new(Colors.DarkGoldenrod);
		static ImmutableBrush darkGray;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkGray"/>.</summary>
		public static SolidColorBrush DarkGray => darkGray ??= new(Colors.DarkGray);
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkGrey"/>.</summary>
		public static SolidColorBrush DarkGrey => DarkGray;
		static ImmutableBrush darkGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkGreen"/>.</summary>
		public static SolidColorBrush DarkGreen => darkGreen ??= new(Colors.DarkGreen);
		static ImmutableBrush darkKhaki;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkKhaki"/>.</summary>
		public static SolidColorBrush DarkKhaki => darkKhaki ??= new(Colors.DarkKhaki);
		static ImmutableBrush darkMagenta;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkMagenta"/>.</summary>
		public static SolidColorBrush DarkMagenta => darkMagenta ??= new(Colors.DarkMagenta);
		static ImmutableBrush darkOliveGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkOliveGreen"/>.</summary>
		public static SolidColorBrush DarkOliveGreen => darkOliveGreen ??= new(Colors.DarkOliveGreen);
		static ImmutableBrush darkOrange;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkOrange"/>.</summary>
		public static SolidColorBrush DarkOrange => darkOrange ??= new(Colors.DarkOrange);
		static ImmutableBrush darkOrchid;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkOrchid"/>.</summary>
		public static SolidColorBrush DarkOrchid => darkOrchid ??= new(Colors.DarkOrchid);
		static ImmutableBrush darkRed;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkRed"/>.</summary>
		public static SolidColorBrush DarkRed => darkRed ??= new(Colors.DarkRed);
		static ImmutableBrush darkSalmon;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkSalmon"/>.</summary>
		public static SolidColorBrush DarkSalmon => darkSalmon ??= new(Colors.DarkSalmon);
		static ImmutableBrush darkSeaGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkSeaGreen"/>.</summary>
		public static SolidColorBrush DarkSeaGreen => darkSeaGreen ??= new(Colors.DarkSeaGreen);
		static ImmutableBrush darkSlateBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkSlateBlue"/>.</summary>
		public static SolidColorBrush DarkSlateBlue => darkSlateBlue ??= new(Colors.DarkSlateBlue);
		static ImmutableBrush darkSlateGray;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkSlateGray"/>.</summary>
		public static SolidColorBrush DarkSlateGray => darkSlateGray ??= new(Colors.DarkSlateGray);
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkSlateGrey"/>.</summary>
		public static SolidColorBrush DarkSlateGrey => DarkSlateGray;
		static ImmutableBrush darkTurquoise;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkTurquoise"/>.</summary>
		public static SolidColorBrush DarkTurquoise => darkTurquoise ??= new(Colors.DarkTurquoise);
		static ImmutableBrush darkViolet;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DarkViolet"/>.</summary>
		public static SolidColorBrush DarkViolet => darkViolet ??= new(Colors.DarkViolet);
		static ImmutableBrush deepPink;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DeepPink"/>.</summary>
		public static SolidColorBrush DeepPink => deepPink ??= new(Colors.DeepPink);
		static ImmutableBrush deepSkyBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DeepSkyBlue"/>.</summary>
		public static SolidColorBrush DeepSkyBlue => deepSkyBlue ??= new(Colors.DeepSkyBlue);
		static ImmutableBrush dimGray;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DimGray"/>.</summary>
		public static SolidColorBrush DimGray => dimGray ??= new(Colors.DimGray);
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DimGrey"/>.</summary>
		public static SolidColorBrush DimGrey => DimGray;
		static ImmutableBrush dodgerBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.DodgerBlue"/>.</summary>
		public static SolidColorBrush DodgerBlue => dodgerBlue ??= new(Colors.DodgerBlue);
		static ImmutableBrush firebrick;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Firebrick"/>.</summary>
		public static SolidColorBrush Firebrick => firebrick ??= new(Colors.Firebrick);
		static ImmutableBrush floralWhite;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.FloralWhite"/>.</summary>
		public static SolidColorBrush FloralWhite => floralWhite ??= new(Colors.FloralWhite);
		static ImmutableBrush forestGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.ForestGreen"/>.</summary>
		public static SolidColorBrush ForestGreen => forestGreen ??= new(Colors.ForestGreen);
		static ImmutableBrush fuchsia;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Fuchsia"/>.</summary>
		public static SolidColorBrush Fuchsia => fuchsia ??= new(Colors.Fuchsia);
		static ImmutableBrush gainsboro;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Gainsboro"/>.</summary>
		public static SolidColorBrush Gainsboro => gainsboro ??= new(Colors.Gainsboro);
		static ImmutableBrush ghostWhite;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.GhostWhite"/>.</summary>
		public static SolidColorBrush GhostWhite => ghostWhite ??= new(Colors.GhostWhite);
		static ImmutableBrush gold;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Gold"/>.</summary>
		public static SolidColorBrush Gold => gold ??= new(Colors.Gold);
		static ImmutableBrush goldenrod;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Goldenrod"/>.</summary>
		public static SolidColorBrush Goldenrod => goldenrod ??= new(Colors.Goldenrod);
		static ImmutableBrush gray;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Gray"/>.</summary>
		public static SolidColorBrush Gray => gray ??= new(Colors.Gray);
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Grey"/>.</summary>
		public static SolidColorBrush Grey => Gray;
		static ImmutableBrush green;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Green"/>.</summary>
		public static SolidColorBrush Green => green ??= new(Colors.Green);
		static ImmutableBrush greenYellow;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.GreenYellow"/>.</summary>
		public static SolidColorBrush GreenYellow => greenYellow ??= new(Colors.GreenYellow);
		static ImmutableBrush honeydew;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Honeydew"/>.</summary>
		public static SolidColorBrush Honeydew => honeydew ??= new(Colors.Honeydew);
		static ImmutableBrush hotPink;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.HotPink"/>.</summary>
		public static SolidColorBrush HotPink => hotPink ??= new(Colors.HotPink);
		static ImmutableBrush indianRed;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.IndianRed"/>.</summary>
		public static SolidColorBrush IndianRed => indianRed ??= new(Colors.IndianRed);
		static ImmutableBrush indigo;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Indigo"/>.</summary>
		public static SolidColorBrush Indigo => indigo ??= new(Colors.Indigo);
		static ImmutableBrush ivory;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Ivory"/>.</summary>
		public static SolidColorBrush Ivory => ivory ??= new(Colors.Ivory);
		static ImmutableBrush khaki;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Khaki"/>.</summary>
		public static SolidColorBrush Khaki => khaki ??= new(Colors.Khaki);
		static ImmutableBrush lavender;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Lavender"/>.</summary>
		public static SolidColorBrush Lavender => lavender ??= new(Colors.Lavender);
		static ImmutableBrush lavenderBlush;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LavenderBlush"/>.</summary>
		public static SolidColorBrush LavenderBlush => lavenderBlush ??= new(Colors.LavenderBlush);
		static ImmutableBrush lawnGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LawnGreen"/>.</summary>
		public static SolidColorBrush LawnGreen => lawnGreen ??= new(Colors.LawnGreen);
		static ImmutableBrush lemonChiffon;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LemonChiffon"/>.</summary>
		public static SolidColorBrush LemonChiffon => lemonChiffon ??= new(Colors.LemonChiffon);
		static ImmutableBrush lightBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightBlue"/>.</summary>
		public static SolidColorBrush LightBlue => lightBlue ??= new(Colors.LightBlue);
		static ImmutableBrush lightCoral;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightCoral"/>.</summary>
		public static SolidColorBrush LightCoral => lightCoral ??= new(Colors.LightCoral);
		static ImmutableBrush lightCyan;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightCyan"/>.</summary>
		public static SolidColorBrush LightCyan => lightCyan ??= new(Colors.LightCyan);
		static ImmutableBrush lightGoldenrodYellow;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightGoldenrodYellow"/>.</summary>
		public static SolidColorBrush LightGoldenrodYellow => lightGoldenrodYellow ??= new(Colors.LightGoldenrodYellow);
		static ImmutableBrush lightGray;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightGray"/>.</summary>
		public static SolidColorBrush LightGray => lightGray ??= new(Colors.LightGray);
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightGrey"/>.</summary>
		public static SolidColorBrush LightGrey => LightGray;
		static ImmutableBrush lightGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightGreen"/>.</summary>
		public static SolidColorBrush LightGreen => lightGreen ??= new(Colors.LightGreen);
		static ImmutableBrush lightPink;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightPink"/>.</summary>
		public static SolidColorBrush LightPink => lightPink ??= new(Colors.LightPink);
		static ImmutableBrush lightSalmon;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightSalmon"/>.</summary>
		public static SolidColorBrush LightSalmon => lightSalmon ??= new(Colors.LightSalmon);
		static ImmutableBrush lightSeaGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightSeaGreen"/>.</summary>
		public static SolidColorBrush LightSeaGreen => lightSeaGreen ??= new(Colors.LightSeaGreen);
		static ImmutableBrush lightSkyBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightSkyBlue"/>.</summary>
		public static SolidColorBrush LightSkyBlue => lightSkyBlue ??= new(Colors.LightSkyBlue);
		static ImmutableBrush lightSlateGray;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightSlateGray"/>.</summary>
		public static SolidColorBrush LightSlateGray => lightSlateGray ??= new(Colors.LightSlateGray);
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightSlateGrey"/>.</summary>
		public static SolidColorBrush LightSlateGrey => LightSlateGray;
		static ImmutableBrush lightSteelBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightSteelBlue"/>.</summary>
		public static SolidColorBrush LightSteelBlue => lightSteelBlue ??= new(Colors.LightSteelBlue);
		static ImmutableBrush lightYellow;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LightYellow"/>.</summary>
		public static SolidColorBrush LightYellow => lightYellow ??= new(Colors.LightYellow);
		static ImmutableBrush lime;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Lime"/>.</summary>
		public static SolidColorBrush Lime => lime ??= new(Colors.Lime);
		static ImmutableBrush limeGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.LimeGreen"/>.</summary>
		public static SolidColorBrush LimeGreen => limeGreen ??= new(Colors.LimeGreen);
		static ImmutableBrush linen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Linen"/>.</summary>
		public static SolidColorBrush Linen => linen ??= new(Colors.Linen);
		static ImmutableBrush magenta;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Magenta"/>.</summary>
		public static SolidColorBrush Magenta => magenta ??= new(Colors.Magenta);
		static ImmutableBrush maroon;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Maroon"/>.</summary>
		public static SolidColorBrush Maroon => maroon ??= new(Colors.Maroon);
		static ImmutableBrush mediumAquararine;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumAquamarine"/>.</summary>
		public static SolidColorBrush MediumAquamarine => mediumAquararine ??= new(Colors.MediumAquamarine);
		static ImmutableBrush mediumBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumBlue"/>.</summary>
		public static SolidColorBrush MediumBlue => mediumBlue ??= new(Colors.MediumBlue);
		static ImmutableBrush mediumOrchid;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumOrchid"/>.</summary>
		public static SolidColorBrush MediumOrchid => mediumOrchid ??= new(Colors.MediumOrchid);
		static ImmutableBrush mediumPurple;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumPurple"/>.</summary>
		public static SolidColorBrush MediumPurple => mediumPurple ??= new(Colors.MediumPurple);
		static ImmutableBrush mediumSeaGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumSeaGreen"/>.</summary>
		public static SolidColorBrush MediumSeaGreen => mediumSeaGreen ??= new(Colors.MediumSeaGreen);
		static ImmutableBrush mediumSlateBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumSlateBlue"/>.</summary>
		public static SolidColorBrush MediumSlateBlue => mediumSlateBlue ??= new(Colors.MediumSlateBlue);
		static ImmutableBrush mediumSpringGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumSpringGreen"/>.</summary>
		public static SolidColorBrush MediumSpringGreen => mediumSpringGreen ??= new(Colors.MediumSpringGreen);
		static ImmutableBrush mediumTurquoise;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumTurquoise"/>.</summary>
		public static SolidColorBrush MediumTurquoise => mediumTurquoise ??= new(Colors.MediumTurquoise);
		static ImmutableBrush mediumVioletRed;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MediumVioletRed"/>.</summary>
		public static SolidColorBrush MediumVioletRed => mediumVioletRed ??= new(Colors.MediumVioletRed);
		static ImmutableBrush midnightBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MidnightBlue"/>.</summary>
		public static SolidColorBrush MidnightBlue => midnightBlue ??= new(Colors.MidnightBlue);
		static ImmutableBrush mintCream;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MintCream"/>.</summary>
		public static SolidColorBrush MintCream => mintCream ??= new(Colors.MintCream);
		static ImmutableBrush mistyRose;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.MistyRose"/>.</summary>
		public static SolidColorBrush MistyRose => mistyRose ??= new(Colors.MistyRose);
		static ImmutableBrush moccasin;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Moccasin"/>.</summary>
		public static SolidColorBrush Moccasin => moccasin ??= new(Colors.Moccasin);
		static ImmutableBrush navajoWhite;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.NavajoWhite"/>.</summary>
		public static SolidColorBrush NavajoWhite => navajoWhite ??= new(Colors.NavajoWhite);
		static ImmutableBrush navy;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Navy"/>.</summary>
		public static SolidColorBrush Navy => navy ??= new(Colors.Navy);
		static ImmutableBrush oldLace;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.OldLace"/>.</summary>
		public static SolidColorBrush OldLace => oldLace ??= new(Colors.OldLace);
		static ImmutableBrush olive;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Olive"/>.</summary>
		public static SolidColorBrush Olive => olive ??= new(Colors.Olive);
		static ImmutableBrush oliveDrab;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.OliveDrab"/>.</summary>
		public static SolidColorBrush OliveDrab => oliveDrab ??= new(Colors.OliveDrab);
		static ImmutableBrush orange;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Orange"/>.</summary>
		public static SolidColorBrush Orange => orange ??= new(Colors.Orange);
		static ImmutableBrush orangeRed;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.OrangeRed"/>.</summary>
		public static SolidColorBrush OrangeRed => orangeRed ??= new(Colors.OrangeRed);
		static ImmutableBrush orchid;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Orchid"/>.</summary>
		public static SolidColorBrush Orchid => orchid ??= new(Colors.Orchid);
		static ImmutableBrush paleGoldenrod;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.PaleGoldenrod"/>.</summary>
		public static SolidColorBrush PaleGoldenrod => paleGoldenrod ??= new(Colors.PaleGoldenrod);
		static ImmutableBrush paleGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.PaleGreen"/>.</summary>
		public static SolidColorBrush PaleGreen => paleGreen ??= new(Colors.PaleGreen);
		static ImmutableBrush paleTurquoise;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.PaleTurquoise"/>.</summary>
		public static SolidColorBrush PaleTurquoise => paleTurquoise ??= new(Colors.PaleTurquoise);
		static ImmutableBrush paleVioletRed;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.PaleVioletRed"/>.</summary>
		public static SolidColorBrush PaleVioletRed => paleVioletRed ??= new(Colors.PaleVioletRed);
		static ImmutableBrush papayaWhip;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.PapayaWhip"/>.</summary>
		public static SolidColorBrush PapayaWhip => papayaWhip ??= new(Colors.PapayaWhip);
		static ImmutableBrush peachPuff;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.PeachPuff"/>.</summary>
		public static SolidColorBrush PeachPuff => peachPuff ??= new(Colors.PeachPuff);
		static ImmutableBrush peru;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Peru"/>.</summary>
		public static SolidColorBrush Peru => peru ??= new(Colors.Peru);
		static ImmutableBrush pink;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Pink"/>.</summary>
		public static SolidColorBrush Pink => pink ??= new(Colors.Pink);
		static ImmutableBrush plum;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Plum"/>.</summary>
		public static SolidColorBrush Plum => plum ??= new(Colors.Plum);
		static ImmutableBrush powderBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.PowderBlue"/>.</summary>
		public static SolidColorBrush PowderBlue => powderBlue ??= new(Colors.PowderBlue);
		static ImmutableBrush purple;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Purple"/>.</summary>
		public static SolidColorBrush Purple => purple ??= new(Colors.Purple);
		static ImmutableBrush red;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Red"/>.</summary>
		public static SolidColorBrush Red => red ??= new(Colors.Red);
		static ImmutableBrush rosyBrown;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.RosyBrown"/>.</summary>
		public static SolidColorBrush RosyBrown => rosyBrown ??= new(Colors.RosyBrown);
		static ImmutableBrush royalBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.RoyalBlue"/>.</summary>
		public static SolidColorBrush RoyalBlue => royalBlue ??= new(Colors.RoyalBlue);
		static ImmutableBrush saddleBrown;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SaddleBrown"/>.</summary>
		public static SolidColorBrush SaddleBrown => saddleBrown ??= new(Colors.SaddleBrown);
		static ImmutableBrush salmon;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Salmon"/>.</summary>
		public static SolidColorBrush Salmon => salmon ??= new(Colors.Salmon);
		static ImmutableBrush sandyBrown;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SandyBrown"/>.</summary>
		public static SolidColorBrush SandyBrown => sandyBrown ??= new(Colors.SandyBrown);
		static ImmutableBrush seaGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SeaGreen"/>.</summary>
		public static SolidColorBrush SeaGreen => seaGreen ??= new(Colors.SeaGreen);
		static ImmutableBrush seaShell;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SeaShell"/>.</summary>
		public static SolidColorBrush SeaShell => seaShell ??= new(Colors.SeaShell);
		static ImmutableBrush sienna;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Sienna"/>.</summary>
		public static SolidColorBrush Sienna => sienna ??= new(Colors.Sienna);
		static ImmutableBrush silver;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Silver"/>.</summary>
		public static SolidColorBrush Silver => silver ??= new(Colors.Silver);
		static ImmutableBrush skyBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SkyBlue"/>.</summary>
		public static SolidColorBrush SkyBlue => skyBlue ??= new(Colors.SkyBlue);
		static ImmutableBrush slateBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SlateBlue"/>.</summary>
		public static SolidColorBrush SlateBlue => slateBlue ??= new(Colors.SlateBlue);
		static ImmutableBrush slateGray;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SlateGray"/>.</summary>
		public static SolidColorBrush SlateGray => slateGray ??= new(Colors.SlateGray);
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SlateGrey"/>.</summary>
		public static SolidColorBrush SlateGrey => SlateGray;
		static ImmutableBrush snow;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Snow"/>.</summary>
		public static SolidColorBrush Snow => snow ??= new(Colors.Snow);
		static ImmutableBrush springGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SpringGreen"/>.</summary>
		public static SolidColorBrush SpringGreen => springGreen ??= new(Colors.SpringGreen);
		static ImmutableBrush steelBlue;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.SteelBlue"/>.</summary>
		public static SolidColorBrush SteelBlue => steelBlue ??= new(Colors.SteelBlue);
		static ImmutableBrush tan;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Tan"/>.</summary>
		public static SolidColorBrush Tan => tan ??= new(Colors.Tan);
		static ImmutableBrush teal;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Teal"/>.</summary>
		public static SolidColorBrush Teal => teal ??= new(Colors.Teal);
		static ImmutableBrush thistle;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Thistle"/>.</summary>
		public static SolidColorBrush Thistle => thistle ??= new(Colors.Thistle);
		static ImmutableBrush tomato;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Tomato"/>.</summary>
		public static SolidColorBrush Tomato => tomato ??= new(Colors.Tomato);
		static ImmutableBrush transparent;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Transparent"/>.</summary>
		public static SolidColorBrush Transparent => transparent ??= new(Colors.Transparent);
		static ImmutableBrush turquoise;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Turquoise"/>.</summary>
		public static SolidColorBrush Turquoise => turquoise ??= new(Colors.Turquoise);
		static ImmutableBrush violet;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Violet"/>.</summary>
		public static SolidColorBrush Violet => violet ??= new(Colors.Violet);
		static ImmutableBrush wheat;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Wheat"/>.</summary>
		public static SolidColorBrush Wheat => wheat ??= new(Colors.Wheat);
		static ImmutableBrush white;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.White"/>.</summary>
		public static SolidColorBrush White => white ??= new(Colors.White);
		static ImmutableBrush whiteSmoke;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.WhiteSmoke"/>.</summary>
		public static SolidColorBrush WhiteSmoke => whiteSmoke ??= new(Colors.WhiteSmoke);
		static ImmutableBrush yellow;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.Yellow"/>.</summary>
		public static SolidColorBrush Yellow => yellow ??= new(Colors.Yellow);
		static ImmutableBrush yellowGreen;
		/// <summary>Gets a <see cref="SolidColorBrush"/> of the system-defined color <see cref="Colors.YellowGreen"/>.</summary>
		public static SolidColorBrush YellowGreen => yellowGreen ??= new(Colors.YellowGreen);

	}
}

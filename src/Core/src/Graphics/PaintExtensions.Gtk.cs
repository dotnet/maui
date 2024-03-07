using System;
using System.Globalization;
using System.Linq;
using Microsoft.Maui.Graphics.Platform.Gtk;

namespace Microsoft.Maui.Graphics
{

	public static partial class PaintExtensions
	{

		/// <summary>
		/// supports <see cref="ImagePaint"/> and <see cref="PatternPaint"/>
		/// </summary>
		public static Gdk.Pixbuf? ToPixbuf(this Paint? paint, out bool owned)
		{
			owned = false;

			if (paint.IsNullOrEmpty())
				return null;

			switch (paint)
			{
				case ImagePaint { Image: PlatformImage image } imagePaint:
				{
					var pixbuf = image.NativeImage;

					return pixbuf;
				}

				case PatternPaint patternPaint:
				{

					var pixbuf = patternPaint.GetPatternBitmap(1);
					owned = true;

					// TODO: create a cairo.pattern & store it in pixbuf
					return pixbuf;

				}

			}

			return null;
		}

		/// <summary>
		/// supports <see cref="LinearGradientPaint"/> and <see cref="RadialGradientPaint"/>
		/// </summary>
		public static string? ToCss(this Paint? paint)
		{
			if (paint.IsNullOrEmpty())
				return null;

			string Stops(PaintGradientStop[] sorted)
			{
#if NET48
				var max = sorted[sorted.Length-1].Offset;
#else
				var max = sorted[^1].Offset;
#endif
				max = 100 / (max == 0 ? 1 : max);
				var stops = string.Join(",", sorted.Select(s => $"{s.Color.ToGdkRgba().ToString()} {(s.Offset * max).ToString(CultureInfo.InvariantCulture)}%"));

				return stops;
			}

			switch (paint)
			{
				case LinearGradientPaint lg:
				{
					var stops = Stops(lg.GetSortedStops());
					var css = $"linear-gradient( to right, {stops})";

					return css;
				}
				case RadialGradientPaint rg:
				{
					var stops = Stops(rg.GetSortedStops());
					var css = $"radial-gradient({stops})";

					return css;
				}

			}

			return null;

		}

	}

}
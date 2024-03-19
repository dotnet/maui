using System;

namespace Microsoft.Maui.Graphics.Platform.Gtk;

public partial class PlatformCanvas
{

	public void DrawFillPaint(Cairo.Context? context, Paint? paint, RectF rectangle)
	{
		if (paint == null || context == null)
			return;

		Color? fillColor = default;

		switch (paint)
		{

			case SolidPaint solidPaint:
			{
				fillColor = solidPaint.Color;

				break;
			}

			case LinearGradientPaint linearGradientPaint:
			{
				try
				{
					if (linearGradientPaint.GetCairoPattern(rectangle, DisplayScale) is { } pattern)
					{
						context.SetSource(pattern);
						pattern.Dispose();
					}
					else
					{
						fillColor = paint.BackgroundColor;
					}
				}
				catch (Exception exc)
				{
					System.Diagnostics.Debug.WriteLine(exc);
					fillColor = linearGradientPaint.BlendStartAndEndColors();
				}

				break;
			}

			case RadialGradientPaint radialGradientPaint:
			{

				try
				{
					if (radialGradientPaint.GetCairoPattern(rectangle, DisplayScale) is { } pattern)
					{
						context.SetSource(pattern);
						pattern.Dispose();
					}
					else
					{
						fillColor = paint.BackgroundColor;
					}
				}
				catch (Exception exc)
				{
					System.Diagnostics.Debug.WriteLine(exc);
					fillColor = radialGradientPaint.BlendStartAndEndColors();
				}

				break;
			}

			case PatternPaint patternPaint:
			{
				try
				{

#if UseSurfacePattern
						// would be nice to have: draw pattern without creating a pixpuf:

						using var paintSurface = CreateSurface(context, true);

						if (patternPaint.GetCairoPattern(Graphics, paintSurface, DisplayScale) is { } pattern) {
							pattern.Extend = Cairo.Extend.Repeat;
							context.SetSource(pattern);
							pattern.Dispose();


						} else {
							fillColor = paint.BackgroundColor;
						}
#else
					using var pixbuf = patternPaint.GetPatternBitmap(DisplayScale);

					if (pixbuf?.CreatePattern(DisplayScale) is { } pattern)
					{
						pattern.Extend = Cairo.Extend.Repeat;
						context.SetSource(pattern);
						pattern.Dispose();
					}

#endif

				}
				catch (Exception exc)
				{
					System.Diagnostics.Debug.WriteLine(exc);
					fillColor = paint.BackgroundColor;
				}

				break;
			}

			case ImagePaint { Image: PlatformImage image } imagePaint:
			{
				var pixbuf = image.NativeImage;

				if (pixbuf?.CreatePattern(DisplayScale) is { } pattern)
				{
					try
					{

						context.SetSource(pattern);
						pattern.Dispose();

					}
					catch (Exception exc)
					{
						System.Diagnostics.Debug.WriteLine(exc);
						fillColor = paint.BackgroundColor;
					}
				}
				else
				{
					fillColor = paint.BackgroundColor ?? Colors.White;
				}

				break;
			}

			case ImagePaint imagePaint:
				fillColor = paint.BackgroundColor ?? Colors.White;

				break;

			default:
				fillColor = paint.BackgroundColor ?? Colors.White;

				break;
		}

		if (fillColor is { })
		{
			FillColor = fillColor;
			context.SetSourceColor(fillColor.ToCairoColor());

		}
	}

}
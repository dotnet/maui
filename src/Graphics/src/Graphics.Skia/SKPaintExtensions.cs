using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public static class SKPaintExtensions
	{
		public static SKPaint CreateCopy(this SKPaint paint)
		{
			if (paint == null)
				return null;

			var copy = new SKPaint
			{
				BlendMode = paint.BlendMode,
				Color = paint.Color,
				ColorFilter = paint.ColorFilter,
				ImageFilter = paint.ImageFilter,
				IsAntialias = paint.IsAntialias,
				IsStroke = paint.IsStroke,
				MaskFilter = paint.MaskFilter,
				Shader = paint.Shader,
				StrokeCap = paint.StrokeCap,
				StrokeJoin = paint.StrokeJoin,
				StrokeMiter = paint.StrokeMiter,
				StrokeWidth = paint.StrokeWidth,
#pragma warning disable CS0618 // Type or member is obsolete
				TextAlign = paint.TextAlign,
				TextEncoding = paint.TextEncoding,
				TextScaleX = paint.TextScaleX,
				TextSize = paint.TextSize,
				TextSkewX = paint.TextSkewX,
				Typeface = paint.Typeface,
#pragma warning restore CS0618 // Type or member is obsolete
			};

			return copy;
		}
	}
}

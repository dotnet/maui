using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			var paint = new SKPaint
			{
				Typeface = font?.ToSKTypeface() ?? SKTypeface.Default,
				TextSize = fontSize
			};
			var width = paint.MeasureText(value);
			paint.Dispose();
			return new SizeF(width, fontSize);
		}

		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			var paint = new SKPaint
			{
				TextSize = fontSize,
				Typeface = font?.ToSKTypeface() ?? SKTypeface.Default
			};
			var width = paint.MeasureText(value);
			paint.Dispose();
			return new SizeF(width, fontSize);
		}
	}
}

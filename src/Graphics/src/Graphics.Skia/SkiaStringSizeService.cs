using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	public class SkiaStringSizeService : IStringSizeService
	{
		public SizeF GetStringSize(string value, IFont font, float fontSize)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			using var skiaFont = new SKFont
			{
				Typeface = font?.ToSKTypeface() ?? SKTypeface.Default,
				Size = fontSize
			};
			var width = skiaFont.MeasureText(value);
			return new SizeF(width, fontSize);
		}

		public SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
		{
			if (string.IsNullOrEmpty(value))
				return new SizeF();

			using var skiaFont = new SKFont
			{
				Size = fontSize,
				Typeface = font?.ToSKTypeface() ?? SKTypeface.Default
			};
			var width = skiaFont.MeasureText(value);
			return new SizeF(width, fontSize);
		}
	}
}

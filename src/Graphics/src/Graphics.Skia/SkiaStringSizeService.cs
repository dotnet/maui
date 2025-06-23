using SkiaSharp;

namespace Microsoft.Maui.Graphics.Skia
{
	/// <summary>
	/// Provides functionality for measuring string dimensions using SkiaSharp.
	/// </summary>
	public class SkiaStringSizeService : IStringSizeService
	{
		/// <summary>
		/// Gets the size of a string when rendered with the specified font and font size.
		/// </summary>
		/// <param name="value">The string to measure.</param>
		/// <param name="font">The font to use when measuring the string.</param>
		/// <param name="fontSize">The font size to use when measuring the string.</param>
		/// <returns>The size of the string in device-independent units.</returns>
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

		/// <summary>
		/// Gets the size of a string when rendered with the specified font, font size, and alignment.
		/// </summary>
		/// <param name="value">The string to measure.</param>
		/// <param name="font">The font to use when measuring the string.</param>
		/// <param name="fontSize">The font size to use when measuring the string.</param>
		/// <param name="horizontalAlignment">The horizontal alignment to use.</param>
		/// <param name="verticalAlignment">The vertical alignment to use.</param>
		/// <returns>The size of the string in device-independent units.</returns>
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

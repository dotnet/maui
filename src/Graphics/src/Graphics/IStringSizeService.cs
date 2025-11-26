namespace Microsoft.Maui.Graphics
{
	/// <summary>
	/// Defines a service for calculating the size of text strings with different fonts and alignments.
	/// </summary>
	public interface IStringSizeService
	{
		/// <summary>
		/// Gets the size of a string when rendered with the specified font and font size.
		/// </summary>
		/// <param name="value">The string to measure.</param>
		/// <param name="font">The font to use for measurement.</param>
		/// <param name="fontSize">The font size in points.</param>
		/// <returns>The size of the string in device-independent units.</returns>
		SizeF GetStringSize(string value, IFont font, float fontSize);

		/// <summary>
		/// Gets the size of a string when rendered with the specified font, font size, and alignments.
		/// </summary>
		/// <param name="value">The string to measure.</param>
		/// <param name="font">The font to use for measurement.</param>
		/// <param name="fontSize">The font size in points.</param>
		/// <param name="horizontalAlignment">The horizontal alignment of the text.</param>
		/// <param name="verticalAlignment">The vertical alignment of the text.</param>
		/// <returns>The size of the string in device-independent units.</returns>
		SizeF GetStringSize(string value, IFont font, float fontSize, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment);
	}
}

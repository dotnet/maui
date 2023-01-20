using Android.Graphics;
using Android.Util;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial interface IFontManager
	{
		/// <summary>
		/// Gets the default typeface for the operating system.
		/// </summary>
		Typeface DefaultTypeface { get; }

		/// <summary>
		/// Retrieves the platform equivalent <see cref="Typeface"/> for an abstract <see cref="Font"/> object.
		/// </summary>
		/// <param name="font">The abstract font representation to get the platform equivalent for.</param>
		/// <returns>The <see cref="Typeface"/> object representing the font as provided in <paramref name="font"/>.</returns>
		Typeface? GetTypeface(Font font);

		/// <summary>
		/// Gets the font size for the provided font.
		/// </summary>
		/// <param name="font">The font to get the size for.</param>
		/// <param name="defaultFontSize">Default font size when the provided font does not have a (valid) value.</param>
		/// <returns>
		/// If <see cref="Font.Size"/> is more than 0 and no equal to <see cref="double.NaN"/>, returns <see cref="Font.Size"/>.
		/// Else, if <paramref name="defaultFontSize"/> is more than 0, returns <paramref name="defaultFontSize"/>.
		/// Else, returns <see cref="DefaultFontSize"/>.</returns>
		/// <remarks>If <see cref="Font.AutoScalingEnabled"/> is <see langword="true"/> on <paramref name="font"/>,
		/// the returned <see cref="FontSize"/> is expressed in <see cref="ComplexUnitType.Sp"/>, otherwise <see cref="ComplexUnitType.Dip"/>.</remarks>
		FontSize GetFontSize(Font font, float defaultFontSize = 0);
	}
}
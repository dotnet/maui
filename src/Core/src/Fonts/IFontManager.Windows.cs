using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial interface IFontManager
	{
		/// <summary>
		/// Gets the default font family for the operating system.
		/// </summary>
		FontFamily DefaultFontFamily { get; }

		/// <summary>
		/// Retrieves the platform equivalent <see cref="FontFamily"/> for an abstract <see cref="Font"/> object.
		/// </summary>
		/// <param name="font">The abstract font representation to get the platform equivalent for.</param>
		/// <returns>The <see cref="FontFamily"/> object representing the font as provided in <paramref name="font"/>.</returns>
		FontFamily GetFontFamily(Font font);

		/// <summary>
		/// Gets the font size for the provided font.
		/// </summary>
		/// <param name="font">The font to get the size for.</param>
		/// <param name="defaultFontSize">Default font size when the provided font does not have a (valid) value.</param>
		/// <returns>
		/// If <see cref="Font.Size"/> is more than 0 and no equal to <see cref="double.NaN"/>, returns <see cref="Font.Size"/>.
		/// Else, if <paramref name="defaultFontSize"/> is more than 0, returns <paramref name="defaultFontSize"/>.
		/// Else, returns <see cref="DefaultFontSize"/>.</returns>
		double GetFontSize(Font font, double defaultFontSize = 0);
	}
}
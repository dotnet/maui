namespace Microsoft.Maui
{
	/// <inheritdoc/>
	public partial interface IFontManager
	{
		/// <summary>
		/// Retrieves the platform equivalent string with which a font can be applied for an abstract <see cref="Font"/> object.
		/// </summary>
		/// <param name="font">The abstract font representation to get the platform equivalent for.</param>
		/// <returns>A <see cref="string"/> containing styling information to apply the font.</returns>
		string GetFont(Font font);

		/// <summary>
		/// Retrieves the platform equivalent string with which a font can be applied for a specified font family.
		/// </summary>
		/// <param name="font">The font family to get the platform equivalent for.</param>
		/// <returns>A <see cref="string"/> containing styling information to apply the font.</returns>
		string GetFontFamily(string? font);
	}
}
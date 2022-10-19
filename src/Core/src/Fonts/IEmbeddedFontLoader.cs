#nullable enable
namespace Microsoft.Maui
{
	/// <summary>
	/// The <see cref="IEmbeddedFontLoader"/> is able to load a font from the embedded resources.
	/// </summary>
	public interface IEmbeddedFontLoader
	{
		/// <summary>
		/// Load the font from the embedded resources.
		/// </summary>
		/// <param name="font">And <see cref="EmbeddedFont"/> object with the information on the font to load.</param>
		/// <returns>The font name if the font was loaded correctly, otherwise <see langword="null"/>.</returns>
		// TODO: this should be async as it involves copying files
		string? LoadFont(EmbeddedFont font);
	}
}
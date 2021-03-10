namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize fonts.
	/// </summary>
	public interface IFont
	{
		/// <summary>
		/// Describe font styles (Bold, Italic, None).
		/// </summary>
		FontAttributes FontAttributes { get; }

		/// <summary>
		/// Gets the font family of the font.
		/// </summary>
		string FontFamily { get; }

		/// <summary>
		/// Gets the size of the font.
		/// </summary>
		double FontSize { get; }
	}
}
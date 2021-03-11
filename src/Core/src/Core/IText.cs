namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize Text.
	/// </summary>
	public interface IText
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		string Text { get; }

		/// <summary>
		/// Gets the text color.
		/// </summary>
		Color TextColor { get; }

		/// <summary>
		/// Gets the font family, style and size of the font.
		/// </summary>
		Font Font { get; }

		/// <summary>
		/// Gets the character spacing.
		/// </summary>
		double CharacterSpacing { get; }
	}
}
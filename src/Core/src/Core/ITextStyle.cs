using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize the appearance of text.
	/// </summary>
	public interface ITextStyle
	{
		/// <summary>
		/// Gets the text color.
		/// </summary>
		Color TextColor { get; }

		/// <summary>
		/// Gets the font family, style and size of the font.
		/// </summary>
		Font Font { get; }

		/// <summary>
		/// Gets the spacing between characters of the text.
		/// </summary>
		double CharacterSpacing { get; }
	}
}
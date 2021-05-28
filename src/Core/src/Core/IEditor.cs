using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View used to accept multi-line input.
	/// </summary>
	public interface IEditor : IView, ITextInput, ITextStyle
	{
		/// <summary>
		/// Gets or sets the placeholder text color.
		/// </summary>
		Color PlaceholderColor { get; set; }

		/// <summary>
		/// Occurs when the user finalizes the text in an editor with the return key.
		/// </summary>
		void Completed();
	}
}
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize Text.
	/// </summary>
	public interface IText : ITextStyle
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		string Text { get; }
	}
}
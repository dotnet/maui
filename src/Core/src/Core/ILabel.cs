namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText, ITextAlignment
	{
		/// <summary>
		/// Gets the space between the text of the Label and it's border.
		/// </summary>
		Thickness Padding { get; }

		/// <summary>
		/// Gets the text decoration applied to the Label.
		/// Underline and strikethrough text decorations can be applied.
		/// </summary>
		TextDecorations TextDecorations { get; }
	}
}
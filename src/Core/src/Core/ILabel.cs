namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText, IPadding
	{
		/// <summary>
		/// Gets the maximum number of lines allowed in the Label.
		/// </summary>
		int MaxLines { get; }

		/// <summary>
		/// Gets the text decoration applied to the Label.
		/// Underline and strikethrough text decorations can be applied.
		/// </summary>
		TextDecorations TextDecorations { get; }
	}
}

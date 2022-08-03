namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText, ITextAlignment, IPadding
	{
		/// <summary>
		/// Gets the text decoration applied to the Label.
		/// Underline and strike-through text decorations can be applied.
		/// </summary>
		TextDecorations TextDecorations { get; }

		/// <summary>
		/// Gets the line height applied to the Label.
		/// Underline and strike-through text decorations can be applied.
		/// </summary>
		double LineHeight { get; }
	}
}
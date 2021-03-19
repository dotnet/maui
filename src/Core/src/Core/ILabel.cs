namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText, ITextAlignment, IPadding
	{
		/// <summary>
		/// Gets the option for line breaking.
		/// </summary>
		LineBreakMode LineBreakMode { get; }

		/// <summary>
		/// Gets the maximum number of lines allowed in the Label.
		/// </summary>
		int MaxLines { get; }

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
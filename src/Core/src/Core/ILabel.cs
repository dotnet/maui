namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText
	{
		/// <summary>
		/// Gets the space between the text of the Label and it's border.
		/// </summary>
		TextDecorations TextDecorations { get; }
		Thickness Padding { get; }
	}
}
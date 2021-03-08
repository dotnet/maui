namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText, IFont, ITextAlignment
	{
		/// <summary>
		/// Gets the space between the text of the Label and it's border.
		/// </summary>
		Thickness Padding { get; }
	}
}
namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText
	{
		int MaxLines { get; }
		LineBreakMode LineBreakMode { get; }
		/// <summary>
		/// Gets the space between the text of the Label and it's border.
		/// </summary>
		Thickness Padding { get; }
	}
}
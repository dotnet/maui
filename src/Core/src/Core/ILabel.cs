namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that displays text.
	/// </summary>
	public interface ILabel : IView, IText, IFont
	{
		Thickness Padding { get; }
	}
}
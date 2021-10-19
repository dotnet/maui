namespace Microsoft.Maui
{
	/// <summary>
	/// Provides functionality to be able to customize Text.
	/// </summary>
	public interface IText : ITextStyle, IElement
	{
		/// <summary>
		/// Gets the text.
		/// </summary>
		string Text { get; }
	}
}
namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View which can take keyboard input.
	/// </summary>
	public interface ITextInput : IText
	{
		/// <summary>
		/// Gets the maximum allowed length of input.
		/// </summary>
		new string Text { get; set; }
	}
}
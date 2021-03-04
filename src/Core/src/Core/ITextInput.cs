namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View which can take keyboard input.
	/// </summary>
	public interface ITextInput : IText
	{
		/// <summary>
		/// Gets the Keyboard for the Input View. 
		/// </summary>

		/// <summary>
		/// Gets a value that controls whether spell checking is enabled.
		/// </summary>

		/// <summary>
		/// Gets the maximum allowed length of input.
		/// </summary>
		new string Text { get; set; }

		/// <summary>
		/// Gets the text of the placeholder.
		/// </summary>

		/// <summary>
		/// Gets the color of the placeholder text.
		/// </summary>

		/// <summary>
		/// Gets a value that indicates whether user should be prevented from modifying the text.
		/// </summary>
	}
}
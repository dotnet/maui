namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View which can take keyboard input.
	/// </summary>
	public interface ITextInput : IText
	{
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		new string Text { get; set; }

		/// <summary>
		/// Gets the placeholder or hint text.
		/// </summary>
		string Placeholder { get; }

		/// <summary>
		/// Gets a value indicating whether or not the view is read-only.
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		/// Gets the keyboard type for the given input control.
		/// </summary>
		Keyboard Keyboard{ get; }
	}
}
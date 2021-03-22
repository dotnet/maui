namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View which can take keyboard input.
	/// </summary>
	public interface ITextInput : IText, IPlaceholder
	{
		/// <summary>
		/// Gets or sets the text.
		/// </summary>
		new string Text { get; set; }

		/// <summary>
		/// Gets a value indicating whether or not the view is read-only.
		/// </summary>
		bool IsReadOnly { get; }

		/// <summary>
		/// Gets the maximum allowed length of input.
		/// </summary>
		int MaxLength { get; }
	}
}
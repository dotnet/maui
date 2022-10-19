namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a <see cref="IView"/> that is used for single-line text input.
	/// </summary>
	public interface IEntry : IView, ITextInput, ITextAlignment
	{
		/// <summary>
		/// Gets a value that indicates if the entry should visually obscure typed text.
		/// </summary>
		bool IsPassword { get; }

		/// <summary>
		/// Gets an enumeration value that controls the appearance of the return button.
		/// </summary>
		ReturnType ReturnType { get; }

		/// <summary>
		/// Gets an enumeration value that shows/hides clear button on the Entry.
		/// </summary>
		ClearButtonVisibility ClearButtonVisibility { get; }

		/// <summary>
		/// Occurs when the user finalizes the text in an entry with the return key.
		/// </summary>
		void Completed();
	}
}
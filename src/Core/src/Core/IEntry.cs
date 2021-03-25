namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that is used for single-line text input.
	/// </summary>
	public interface IEntry : IView, IText, ITextInput, ITextAlignment
	{
		/// <summary>
		/// Gets a value that indicates if the entry should visually obscure typed text.
		/// </summary>
		bool IsPassword { get; }

		/// <summary>
		/// Gets a value that controls whether text prediction and automatic text correction is on or off.
		/// </summary>
		bool IsTextPredictionEnabled { get; }

		/// <summary>
		/// Gets an enumeration value that controls the appearance of the return button.
		/// </summary>
		ReturnType ReturnType { get; }

		/// <summary>
		/// Gets an enumeration value that shows/hides clear button on the Entry.
		/// </summary>
		ClearButtonVisibility ClearButtonVisibility { get; }
	}
}
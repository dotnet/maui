namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View that is used for single-line text input.
	/// </summary>
	public interface IEntry : IView, IText, ITextInput
	{
		/// <summary>
		/// Gets a value that indicates if the entry should visually obscure typed text.
		/// </summary>
		bool IsPassword { get; }
		/// <summary>
		/// Gets a value that controls whether text prediction and automatic text correction is on or off.
		/// </summary>
		bool IsTextPredictionEnabled { get; }
	}
}
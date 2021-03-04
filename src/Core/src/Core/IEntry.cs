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
		
		bool IsTextPredictionEnabled { get; }
	}
}
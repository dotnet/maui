namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View used to accept multi-line input.
	/// </summary>
	public interface IEditor : IView, ITextInput
	{
		/// <summary>
		/// Gets or sets the placeholder text color.
		/// </summary>
		Color PlaceholderColor { get; set; }

		/// <summary>
		/// Gets a value that controls whether text prediction and automatic text correction is on or off.
		/// </summary>
		bool IsTextPredictionEnabled { get; }
	}
}
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
	}
}

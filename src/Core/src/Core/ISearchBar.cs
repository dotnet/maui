namespace Microsoft.Maui
{
	/// <summary>
	/// Represents a View used to initiating a search.
	/// </summary>
	public interface ISearchBar : IView, ITextInput, ITextAlignment
	{
		/// <summary>
		/// Notify when the user presses the Search button.
		/// </summary>
		void SearchButtonPressed();
	}
}
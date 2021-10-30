namespace Microsoft.Maui.Controls
{
	public partial class SearchBar : ISearchBar
	{
		Font ITextStyle.Font => (Font)GetValue(FontElement.FontProperty);

		bool ITextInput.IsTextPredictionEnabled => true;

		void ISearchBar.SearchButtonPressed()
		{
			(this as ISearchBarController).OnSearchButtonPressed();
		}
	}
}
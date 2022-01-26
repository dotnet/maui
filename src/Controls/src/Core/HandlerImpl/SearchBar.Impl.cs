namespace Microsoft.Maui.Controls
{
	/// <include file="../../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.SearchBar']/Docs" />
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
namespace Microsoft.Maui.Controls
{
	public partial class SearchBar : ISearchBar
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= Font.OfSize(FontFamily, FontSize).WithAttributes(FontAttributes);

		bool ITextInput.IsTextPredictionEnabled => true;

		void ISearchBar.SearchButtonPressed()
		{
			(this as ISearchBarController).OnSearchButtonPressed();
		}
	}
}
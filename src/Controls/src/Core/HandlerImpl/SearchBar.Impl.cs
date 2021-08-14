using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	public partial class SearchBar : ISearchBar, INotifyFontChanging
	{
		Font? _font;

		Font ITextStyle.Font => _font ??= this.ToFont();

		bool ITextInput.IsTextPredictionEnabled => true;

		void ISearchBar.SearchButtonPressed()
		{
			(this as ISearchBarController).OnSearchButtonPressed();
		}

		void INotifyFontChanging.FontChanging()
		{
			// Null out the Maui font value so it will be recreated next time it's accessed
			_font = null;
		}
	}
}
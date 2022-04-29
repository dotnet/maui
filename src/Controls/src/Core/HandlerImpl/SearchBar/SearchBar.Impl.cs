using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../../../docs/Microsoft.Maui.Controls/SearchBar.xml" path="Type[@FullName='Microsoft.Maui.Controls.SearchBar']/Docs" />
	public partial class SearchBar : ISearchBar
	{
		Font ITextStyle.Font => this.ToFont();

		Color ITextStyle.TextColor
		{
			get => TextColor ??
				DefaultStyles.GetColor(this, TextColorProperty)?.Value as Color;
		}

		Color IPlaceholder.PlaceholderColor
		{
			get => TextColor ??
				DefaultStyles.GetColor(this, PlaceholderColorProperty)?.Value as Color;
		}

		Color ISearchBar.CancelButtonColor
		{
			get => CancelButtonColor ??
				DefaultStyles.GetColor(this, CancelButtonColorProperty)?.Value as Color;
		}

		bool ITextInput.IsTextPredictionEnabled => true;

		void ISearchBar.SearchButtonPressed()
		{
			(this as ISearchBarController).OnSearchButtonPressed();
		}
	}
}
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateCharacterSpacing(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.CharacterSpacing = searchBar.CharacterSpacing.ToEm();
		}

		public static void UpdatePlaceholder(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.PlaceholderText = searchBar.Placeholder ?? string.Empty;
		}
  
		public static void UpdateText(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.Text = searchBar.Text;
		}

		public static void UpdateHorizontalTextAlignment(this AutoSuggestBox nativeControl, ISearchBar searchBar, MauiTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				return;

			queryTextBox.TextAlignment = searchBar.HorizontalTextAlignment.ToNative();
		}

		public static void UpdateMaxLength(this AutoSuggestBox nativeControl, ISearchBar searchBar, MauiTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				return;

			queryTextBox.MaxLength = searchBar.MaxLength;

			var currentControlText = nativeControl.Text;

			if (currentControlText.Length > searchBar.MaxLength)
				nativeControl.Text = currentControlText.Substring(0, searchBar.MaxLength);
		}
	}
}

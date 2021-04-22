using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateCharacterSpacing(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.CharacterSpacing = searchBar.CharacterSpacing.ToEm();
		}

		public static void UpdateText(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.Text = searchBar.Text;
		}
	}
}

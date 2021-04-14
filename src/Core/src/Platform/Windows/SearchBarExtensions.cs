using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdatePlaceholder(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.PlaceholderText = searchBar.Placeholder ?? string.Empty;
		}
	}
}
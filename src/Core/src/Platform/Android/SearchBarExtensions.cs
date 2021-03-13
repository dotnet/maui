using AndroidX.AppCompat.Widget;
using System.Linq;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateText(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.SetQuery(searchBar.Text, false);
		}

		public static void UpdatePlaceholder(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.QueryHint = searchBar.Placeholder;
		}

		public static void UpdateCharacterSpacing(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.UpdateCharacterSpacing(searchBar, null);
		}

		public static void UpdateCharacterSpacing(this SearchView searchView, ISearchBar searchBar, EditText? editText)
		{
			editText ??= searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText != null)
			{
				editText.LetterSpacing = searchBar.CharacterSpacing.ToEm();
			}
		}

		public static void UpdatePlaceholder(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.QueryHint = searchBar.Placeholder;
		}
	}
}

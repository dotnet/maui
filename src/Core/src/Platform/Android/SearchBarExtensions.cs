using System.Linq;
using Android.Widget;
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

		public static void UpdateHorizontalTextAlignment(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.UpdateHorizontalTextAlignment(searchBar, null);
		}

		public static void UpdateHorizontalTextAlignment(this SearchView searchView, ISearchBar searchBar, EditText? editText)
		{
			editText ??= searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText == null)
				return;

			bool hasRtlSupport = searchView.Context != null && searchView.Context.HasRtlSupport();
			editText.UpdateHorizontalAlignment(searchBar.HorizontalTextAlignment, hasRtlSupport);
		}
	}
}

using System.Linq;
using Android.Util;
using Android.Widget;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui
{
	public static class SearchViewExtensions
	{
		public static void UpdateText(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.SetQuery(searchBar.Text, false);
		}

		public static void UpdatePlaceholder(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.QueryHint = searchBar.Placeholder;
		}

		public static void UpdateFont(this SearchView searchView, ISearchBar searchBar, IFontManager fontManager)
		{
			searchView.UpdateFont(searchBar, fontManager, null);
		}

		public static void UpdateFont(this SearchView searchView, ISearchBar searchBar, IFontManager fontManager, EditText? editText)
		{
			editText ??= searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText == null)
				return;

			var font = searchBar.Font;

			var tf = fontManager.GetTypeface(font);
			editText.Typeface = tf;

			var sp = fontManager.GetScaledPixel(font);
			editText.SetTextSize(ComplexUnitType.Sp, sp);
		}
	}
}

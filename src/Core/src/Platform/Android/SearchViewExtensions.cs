using System.Collections.Generic;
using System.Linq;
using Android.Text;
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

		public static void UpdateFont(this SearchView searchView, ISearchBar searchBar, IFontManager fontManager, EditText? editText = null)
		{
			editText ??= searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			if (editText == null)
				return;

			editText.UpdateFont(searchBar, fontManager);
		}

		public static void UpdateMaxLength(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.UpdateMaxLength(searchBar, null);
		}

		public static void UpdateMaxLength(this SearchView searchView, ISearchBar searchBar, EditText? editText)
		{
			editText ??= searchView.GetChildrenOfType<EditText>().FirstOrDefault();

			var currentFilters = new List<IInputFilter>(editText?.GetFilters() ?? new IInputFilter[0]);

			for (var i = 0; i < currentFilters.Count; i++)
			{
				if (currentFilters[i] is InputFilterLengthFilter)
				{
					currentFilters.RemoveAt(i);
					break;
				}
			}

			currentFilters.Add(new InputFilterLengthFilter(searchBar.MaxLength));

			editText?.SetFilters(currentFilters.ToArray());

			var currentControlText = searchView.Query;

			if (currentControlText.Length > searchBar.MaxLength)
				searchView.SetQuery(currentControlText.Substring(0, searchBar.MaxLength), false);
    }
    
		public static void UpdateCancelButtonColor(this SearchView searchView, ISearchBar searchBar)
		{
			if (searchView.Resources == null)
				return;

			var searchCloseButtonIdentifier = Resource.Id.search_close_btn;
		
			if (searchCloseButtonIdentifier > 0)
			{
				var image = searchView.FindViewById<ImageView>(searchCloseButtonIdentifier);

				if (image != null && image.Drawable != null)
				{
					if (searchBar.CancelButtonColor != null)
						image.Drawable.SetColorFilter(searchBar.CancelButtonColor, FilterMode.SrcIn);
					else
						image.Drawable.ClearColorFilter();
				}
			}
		}
	}
}
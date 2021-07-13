using Android.Content.Res;
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

		public static void UpdatePlaceholderColor(this SearchView searchView, ISearchBar searchBar, ColorStateList? defaultPlaceholderColor, EditText? editText = null)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			var placeholderTextColor = searchBar.PlaceholderColor;

			if (placeholderTextColor == null)
			{
				editText.SetHintTextColor(defaultPlaceholderColor);
			}
			else
			{
				var androidColor = placeholderTextColor.ToNative();

				if (!editText.HintTextColors.IsOneColor(ColorExtensions.States, androidColor))
				{
					var acolor = androidColor.ToArgb();
					editText.SetHintTextColor(new ColorStateList(ColorExtensions.States, new[] { acolor, acolor }));
				}
			}
		}

		public static void UpdateFont(this SearchView searchView, ISearchBar searchBar, IFontManager fontManager, EditText? editText = null)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			editText.UpdateFont(searchBar, fontManager);
		}

		public static void UpdateMaxLength(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.UpdateMaxLength(searchBar.MaxLength, null);
		}

		public static void UpdateMaxLength(this SearchView searchView, ISearchBar searchBar, EditText? editText)
		{
			searchView.UpdateMaxLength(searchBar.MaxLength, editText);
		}

		public static void UpdateMaxLength(this SearchView searchView, int maxLength, EditText? editText)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();
			editText?.SetLengthFilter(maxLength);

			var query = searchView.Query;
			var trimmedQuery = query.TrimToMaxLength(maxLength);

			if (query != trimmedQuery)
			{
				searchView.SetQuery(trimmedQuery, false);
			}
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
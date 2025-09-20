using Android.Content;
using Android.Content.Res;
using Android.Text;
using Android.Views.InputMethods;
using Android.Widget;
using SearchView = AndroidX.AppCompat.Widget.SearchView;

namespace Microsoft.Maui.Platform
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
				if (PlatformInterop.CreateEditTextColorStateList(editText.HintTextColors, placeholderTextColor.ToPlatform()) is ColorStateList c)
				{
					editText.SetHintTextColor(c);
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

		public static void UpdateVerticalTextAlignment(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.UpdateVerticalTextAlignment(searchBar, null);
		}

		public static void UpdateVerticalTextAlignment(this SearchView searchView, ISearchBar searchBar, EditText? editText)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			editText.UpdateVerticalAlignment(searchBar.VerticalTextAlignment, TextAlignment.Center.ToVerticalGravityFlags());
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

		public static void UpdateIsReadOnly(this EditText editText, ISearchBar searchBar)
		{
			bool isReadOnly = !searchBar.IsReadOnly;

			editText.FocusableInTouchMode = isReadOnly;
			editText.Focusable = isReadOnly;
			editText.SetCursorVisible(isReadOnly);
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

		internal static void UpdateSearchIconColor(this SearchView searchView, ISearchBar searchBar)
		{
			if (searchView.Resources is null)
				return;

			var searchIconIdentifier = Resource.Id.search_mag_icon;

			if (searchIconIdentifier > 0)
			{
				var image = searchView.FindViewById<ImageView>(searchIconIdentifier);

				if (image?.Drawable is not null)
				{
					if (searchBar.SearchIconColor is not null)
						image.Drawable.SetColorFilter(searchBar.SearchIconColor, FilterMode.SrcIn);
					else
						image.Drawable.ClearColorFilter();
				}
			}
		}

		public static void UpdateIsTextPredictionEnabled(this SearchView searchView, ISearchBar searchBar, EditText? editText = null)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			if (searchBar.IsTextPredictionEnabled)
				editText.InputType |= InputTypes.TextFlagAutoCorrect;
			else
				editText.InputType &= ~InputTypes.TextFlagAutoCorrect;
		}

		public static void UpdateIsSpellCheckEnabled(this SearchView searchView, ISearchBar searchBar, EditText? editText = null)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			if (!searchBar.IsSpellCheckEnabled)
				editText.InputType |= InputTypes.TextFlagNoSuggestions;
			else
				editText.InputType &= ~InputTypes.TextFlagNoSuggestions;
		}

		public static void UpdateIsEnabled(this SearchView searchView, ISearchBar searchBar, EditText? editText = null)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			editText?.Enabled = searchBar.IsEnabled;
		}

		public static void UpdateKeyboard(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.SetInputType(searchBar);
		}

		public static void UpdateReturnType(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.SetInputType(searchBar);
			searchView.ImeOptions = (int)searchBar.ReturnType.ToPlatform();
		}

		internal static void SetInputType(this SearchView searchView, ISearchBar searchBar, EditText? editText = null)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			editText.SetInputType(searchBar);
		}
	}
}
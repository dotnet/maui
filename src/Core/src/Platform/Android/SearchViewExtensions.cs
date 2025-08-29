using System;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Util;
using Android.Widget;
using static Android.Content.Res.Resources;
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

			if (editText is null)
				return;

			if (searchBar?.PlaceholderColor is Graphics.Color placeholderTextColor)
			{
				if (PlatformInterop.CreateEditTextColorStateList(editText.HintTextColors, placeholderTextColor.ToPlatform()) is ColorStateList c)
				{
					editText.SetHintTextColor(c);
				}
			}
			else if (TryGetDefaultStateColor(searchView, Android.Resource.Attribute.TextColorHint, out var color))
			{
				editText.SetHintTextColor(color);

				var searchMagIconImage = searchView.FindViewById<ImageView>(Resource.Id.search_mag_icon);
				searchMagIconImage?.Drawable?.SetTint(color);
			}
		}

		internal static void UpdateTextColor(this SearchView searchView, ITextStyle entry)
		{
			if (TryGetDefaultStateColor(searchView, Android.Resource.Attribute.TextColorPrimary, out var color) &&
				searchView.GetFirstChildOfType<EditText>() is EditText editText)
			{
				if (entry.TextColor is null)
					editText.SetTextColor(color);

				var searchMagIconImage = searchView.FindViewById<ImageView>(Resource.Id.search_mag_icon);
				searchMagIconImage?.Drawable?.SetTint(color);
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

				if (image is not null && image.Drawable is Drawable drawable)
				{
					if (searchBar.CancelButtonColor is not null)
						drawable.SetColorFilter(searchBar.CancelButtonColor, FilterMode.SrcIn);
					else if (TryGetDefaultStateColor(searchView, Android.Resource.Attribute.TextColorPrimary, out var color))
						drawable.SetColorFilter(color, FilterMode.SrcIn);
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

			if (editText != null)
			{
				editText.Enabled = searchBar.IsEnabled;
			}
		}

		public static void UpdateKeyboard(this SearchView searchView, ISearchBar searchBar)
		{
			searchView.SetInputType(searchBar);
		}

		internal static void SetInputType(this SearchView searchView, ISearchBar searchBar, EditText? editText = null)
		{
			editText ??= searchView.GetFirstChildOfType<EditText>();

			if (editText == null)
				return;

			editText.SetInputType(searchBar);
		}

		static bool TryGetDefaultStateColor(SearchView searchView, int attribute, out Color color)
		{
			color = default;

			if (!OperatingSystem.IsAndroidVersionAtLeast(23))
				return false;

			if (searchView.Context?.Theme is not Theme theme)
				return false;

			int[] s_disabledState = [-Android.Resource.Attribute.StateEnabled];
			int[] s_enabledState = [Android.Resource.Attribute.StateEnabled];

			using var ta = theme.ObtainStyledAttributes([attribute]);
			var cs = ta.GetColorStateList(0);
			if (cs is null)
				return false;

			var state = searchView.Enabled ? s_enabledState : s_disabledState;
			color = new Color(cs.GetColorForState(state, Color.Black));
			return true;
		}
	}
}
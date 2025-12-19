using System;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Text;
using Android.Util;
using Android.Views.InputMethods;
using Android.Widget;
using static Android.Content.Res.Resources;
using AAttribute = Android.Resource.Attribute;
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
			else if (TryGetDefaultStateColor(searchView, AAttribute.TextColorHint, out var color))
			{
				editText.SetHintTextColor(color);

				var searchMagIconImage = searchView.FindViewById<ImageView>(Resource.Id.search_mag_icon);
				SafeSetTint(searchMagIconImage, color);
			}
		}

		internal static void UpdateTextColor(this SearchView searchView, ITextStyle entry)
		{
			if (TryGetDefaultStateColor(searchView, AAttribute.TextColorPrimary, out var color) &&
				searchView.GetFirstChildOfType<EditText>() is EditText editText)
			{
				if (entry.TextColor is null)
					editText.SetTextColor(color);

				var searchMagIconImage = searchView.FindViewById<ImageView>(Resource.Id.search_mag_icon);
				SafeSetTint(searchMagIconImage, color);
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
				if (searchBar.CancelButtonColor is not null)
					SafeSetTint(image, searchBar.CancelButtonColor.ToPlatform());
				else if (TryGetDefaultStateColor(searchView, AAttribute.TextColorPrimary, out var color))
					SafeSetTint(image, color);
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
						SafeSetTint(image, searchBar.SearchIconColor.ToPlatform());
					else
						SafeSetTint(image, Color.Transparent);
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

		static bool TryGetDefaultStateColor(SearchView searchView, int attribute, out Color color)
		{
			color = default;

			if (!OperatingSystem.IsAndroidVersionAtLeast(23))
				return false;

			if (searchView.Context?.Theme is not Theme theme)
				return false;

			int[] s_disabledState = [-AAttribute.StateEnabled];
			int[] s_enabledState = [AAttribute.StateEnabled];

			using var ta = theme.ObtainStyledAttributes([attribute]);
			var cs = ta.GetColorStateList(0);
			if (cs is null)
				return false;

			var state = searchView.Enabled ? s_enabledState : s_disabledState;
			color = new Color(cs.GetColorForState(state, Color.Black));
			return true;
		}

		/// <summary>
		/// Safely applies tint to an ImageView's drawable by mutating it first.
		/// This prevents crashes when the drawable is shared across multiple views.
		/// </summary>
		/// <remarks>
		/// Android shares Drawable resources for memory efficiency. Modifying a shared
		/// drawable without calling Mutate() first causes race conditions and crashes.
		/// See: https://developer.android.com/reference/android/graphics/drawable/Drawable#mutate()
		/// </remarks>
		internal static void SafeSetTint(ImageView? imageView, Color color)
		{
			if (imageView?.Drawable is not Drawable drawable)
				return;

			var safe = drawable.Mutate();
			safe.SetTint(color);
			imageView?.SetImageDrawable(safe);
		}
	}
}

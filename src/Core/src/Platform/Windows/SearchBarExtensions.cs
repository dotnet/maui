using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public static class SearchBarExtensions
	{
		public static void UpdateCharacterSpacing(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.CharacterSpacing = searchBar.CharacterSpacing.ToEm();
		}

		public static void UpdatePlaceholder(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.PlaceholderText = searchBar.Placeholder ?? string.Empty;
		}

		public static void UpdatePlaceholderColor(this AutoSuggestBox platformControl, ISearchBar searchBar, Brush? defaultPlaceholderColorBrush, Brush? defaultPlaceholderColorFocusBrush, MauiSearchTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				queryTextBox = platformControl.GetFirstDescendant<MauiSearchTextBox>();

			if (queryTextBox == null)
				return;

			Color placeholderColor = searchBar.PlaceholderColor;
			if (placeholderColor != null)
				queryTextBox.PlaceholderForegroundBrush = queryTextBox.PlaceholderForegroundFocusBrush = placeholderColor.ToPlatform();
		}

		public static void UpdateText(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.Text = searchBar.Text;
		}

		public static void UpdateTextColor(this AutoSuggestBox platformControl, ISearchBar searchBar, Brush? defaultTextColorBrush, Brush? defaultTextColorFocusBrush, MauiSearchTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				queryTextBox = platformControl.GetFirstDescendant<MauiSearchTextBox>();

			if (queryTextBox == null)
				return;

			Color textColor = searchBar.TextColor;
			if (textColor != null)
				queryTextBox.Foreground = queryTextBox.ForegroundFocusBrush = textColor.ToPlatform();
		}

		public static void UpdateFont(this AutoSuggestBox platformControl, ISearchBar searchBar, IFontManager fontManager) =>
			platformControl.UpdateFont(searchBar.Font, fontManager);

		public static void UpdateHorizontalTextAlignment(this AutoSuggestBox platformControl, ISearchBar searchBar, MauiSearchTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				queryTextBox = platformControl.GetFirstDescendant<MauiSearchTextBox>();

			if (queryTextBox == null)
				return;

			queryTextBox.TextAlignment = searchBar.HorizontalTextAlignment.ToPlatform();
		}

		public static void UpdateVerticalTextAlignment(this AutoSuggestBox platformControl, ISearchBar searchBar, MauiSearchTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				return;

			queryTextBox.VerticalAlignment = searchBar.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}

		public static void UpdateMaxLength(this AutoSuggestBox platformControl, ISearchBar searchBar, MauiSearchTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				queryTextBox = platformControl.GetFirstDescendant<MauiSearchTextBox>();

			if (queryTextBox == null)
				return;

			queryTextBox.MaxLength = searchBar.MaxLength;

			var currentControlText = platformControl.Text;

			if (currentControlText.Length > searchBar.MaxLength)
				platformControl.Text = currentControlText.Substring(0, searchBar.MaxLength);
		}
		
		public static void UpdateIsReadOnly(this AutoSuggestBox platformControl, ISearchBar searchBar, MauiSearchTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				queryTextBox = platformControl.GetFirstDescendant<MauiSearchTextBox>();

			if (queryTextBox == null)
				return;

			queryTextBox.IsReadOnly = searchBar.IsReadOnly;
		}

		public static void UpdateIsTextPredictionEnabled(this AutoSuggestBox platformControl, ISearchBar searchBar, MauiSearchTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				queryTextBox = platformControl.GetFirstDescendant<MauiSearchTextBox>();

			if (queryTextBox == null)
				return;

			queryTextBox.IsTextPredictionEnabled = searchBar.IsTextPredictionEnabled;
		}

		public static void UpdateCancelButtonColor(this AutoSuggestBox platformControl, ISearchBar searchBar, MauiCancelButton? cancelButton, Brush? defaultDeleteButtonBackgroundColorBrush, Brush? defaultDeleteButtonForegroundColorBrush)
		{
			if (cancelButton == null || !cancelButton.IsReady)
				return;

			Color cancelColor = searchBar.CancelButtonColor;
			if (cancelColor != null)
			{
				cancelButton.ForegroundBrush = cancelColor.ToPlatform();
				// Determine whether the background should be black or white (in order to make the foreground color visible) 
				var bcolor = cancelColor.ToWindowsColor().GetContrastingColor().ToColor();
				cancelButton.BackgroundBrush = bcolor.ToPlatform();
			}
		}
	}
}

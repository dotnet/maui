using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui
{
	public static class SearchBarExtensions
	{
		public static void UpdateCharacterSpacing(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.CharacterSpacing = searchBar.CharacterSpacing.ToEm();
		}

		public static void UpdatePlaceholder(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.PlaceholderText = searchBar.Placeholder ?? string.Empty;
		}
  
		public static void UpdateText(this AutoSuggestBox nativeControl, ISearchBar searchBar)
		{
			nativeControl.Text = searchBar.Text;
		}

		public static void UpdateHorizontalTextAlignment(this AutoSuggestBox nativeControl, ISearchBar searchBar, MauiTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				return;

			queryTextBox.TextAlignment = searchBar.HorizontalTextAlignment.ToNative();
		}

		public static void UpdateMaxLength(this AutoSuggestBox nativeControl, ISearchBar searchBar, MauiTextBox? queryTextBox)
		{
			if (queryTextBox == null)
				return;

			queryTextBox.MaxLength = searchBar.MaxLength;

			var currentControlText = nativeControl.Text;

			if (currentControlText.Length > searchBar.MaxLength)
				nativeControl.Text = currentControlText.Substring(0, searchBar.MaxLength);
		}

		public static void UpdateCancelButtonColor(this AutoSuggestBox nativeControl, ISearchBar searchBar, MauiCancelButton? cancelButton, Brush? defaultDeleteButtonBackgroundColorBrush, Brush? defaultDeleteButtonForegroundColorBrush)
		{
			if (cancelButton == null || !cancelButton.IsReady)
				return;

			Color cancelColor = searchBar.CancelButtonColor;

			BrushHelpers.UpdateColor(cancelColor, ref defaultDeleteButtonForegroundColorBrush,
				() => cancelButton.ForegroundBrush, brush => cancelButton.ForegroundBrush = brush);

			if (cancelColor == null)
			{
				BrushHelpers.UpdateColor(null, ref defaultDeleteButtonBackgroundColorBrush,
					() => cancelButton.BackgroundBrush, brush => cancelButton.BackgroundBrush = brush);
			}
			else
			{
				// Determine whether the background should be black or white (in order to make the foreground color visible) 
				var bcolor = cancelColor.ToWindowsColor().GetContrastingColor().ToColor();
				BrushHelpers.UpdateColor(bcolor, ref defaultDeleteButtonBackgroundColorBrush,
					() => cancelButton.BackgroundBrush, brush => cancelButton.BackgroundBrush = brush);
			}
		}
	}
}

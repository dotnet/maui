using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;

namespace Microsoft.Maui.Platform
{
	public static class SearchBarExtensions
	{
		private static readonly string[] _backgroundColorKeys =
		{
			"TextControlBackground",
			"TextControlBackgroundPointerOver",
			"TextControlBackgroundFocused",
			"TextControlBackgroundDisabled"
		};

		public static void UpdateBackground(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			UpdateColors(platformControl.Resources, _backgroundColorKeys, searchBar.Background?.ToPlatform());
		}

		public static void UpdateIsEnabled(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.IsEnabled = searchBar.IsEnabled;
		}

		public static void UpdateCharacterSpacing(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.CharacterSpacing = searchBar.CharacterSpacing.ToEm();
		}

		public static void UpdatePlaceholder(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.PlaceholderText = searchBar.Placeholder ?? string.Empty;
		}

		private static readonly string[] _placeholderForegroundColorKeys =
		{
			"TextControlPlaceholderForeground",
			"TextControlPlaceholderForegroundPointerOver",
			"TextControlPlaceholderForegroundFocused",
			"TextControlPlaceholderForegroundDisabled"
		};

		public static void UpdatePlaceholderColor(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			UpdateColors(platformControl.Resources, _placeholderForegroundColorKeys,
				searchBar.PlaceholderColor?.ToPlatform());
		}

		public static void UpdateText(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.Text = searchBar.Text;
		}

		private static readonly string[] _foregroundColorKeys =
		{
			"TextControlForeground",
			"TextControlForegroundPointerOver",
			"TextControlForegroundFocused",
			"TextControlForegroundDisabled"
		};

		public static void UpdateTextColor(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			var tintBrush = searchBar.TextColor?.ToPlatform();

			if (tintBrush == null)
			{
				platformControl.Resources.RemoveKeys(_foregroundColorKeys);
				platformControl.Foreground = null;
			}
			else
			{
				platformControl.Resources.SetValueForAllKey(_foregroundColorKeys, tintBrush);
				platformControl.Foreground = tintBrush;
			}

			platformControl.RefreshThemeResources();
		}

		private static void UpdateColors(ResourceDictionary resource, string[] keys, Brush? brush)
		{
			if (brush is null)
			{
				resource.RemoveKeys(keys);
			}
			else
			{
				resource.SetValueForAllKey(keys, brush);
			}
		}

		public static void UpdateFont(this AutoSuggestBox platformControl, ISearchBar searchBar, IFontManager fontManager) =>
			platformControl.UpdateFont(searchBar.Font, fontManager);

		public static void UpdateHorizontalTextAlignment(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.HorizontalContentAlignment = searchBar.HorizontalTextAlignment.ToPlatformHorizontalAlignment();
		}

		public static void UpdateVerticalTextAlignment(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			platformControl.VerticalContentAlignment = searchBar.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}

		public static void UpdateMaxLength(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			var maxLength = searchBar.MaxLength;

			if (maxLength == -1)
				maxLength = int.MaxValue;

			if (maxLength == 0)
				MauiAutoSuggestBox.SetIsReadOnly(platformControl, true);
			else
				MauiAutoSuggestBox.SetIsReadOnly(platformControl, searchBar.IsReadOnly);

			var currentControlText = platformControl.Text;

			if (currentControlText.Length > maxLength)
				platformControl.Text = currentControlText.Substring(0, maxLength);
		}

		public static void UpdateIsReadOnly(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			MauiAutoSuggestBox.SetIsReadOnly(platformControl, searchBar.IsReadOnly);
		}

		public static void UpdateIsTextPredictionEnabled(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			var textBox = platformControl.GetFirstDescendant<TextBox>();

			if (textBox is null)
				return;

			textBox.UpdateIsTextPredictionEnabled(searchBar);
		}

		public static void UpdateIsSpellCheckEnabled(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			var textBox = platformControl.GetFirstDescendant<TextBox>();

			if (textBox is null)
				return;

			textBox.UpdateIsSpellCheckEnabled(searchBar);
		}

		public static void UpdateKeyboard(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			var queryTextBox = platformControl.GetFirstDescendant<TextBox>();

			if (queryTextBox == null)
				return;

			queryTextBox.UpdateInputScope(searchBar);
		}

		private static readonly string[] CancelButtonColorKeys =
		{
			"TextControlButtonForeground",
			"TextControlButtonForegroundPointerOver",
			"TextControlButtonForegroundPressed",
		};

		internal static void UpdateCancelButtonColor(this AutoSuggestBox platformControl, ISearchBar searchBar)
		{
			var cancelButton = platformControl.GetDescendantByName<Button>("DeleteButton");

			if (cancelButton is null)
				return;

			cancelButton.UpdateTextColor(searchBar.CancelButtonColor, CancelButtonColorKeys);
		}
	}
}
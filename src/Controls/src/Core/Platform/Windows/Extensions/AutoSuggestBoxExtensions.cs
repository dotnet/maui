using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Controls.Platform
{
	internal static class AutoSuggestBoxExtensions
	{
		public static void UpdateText(this AutoSuggestBox platformControl, InputView inputView)
		{
			platformControl.Text = TextTransformUtilities.GetTransformedText(inputView.Text, inputView.TextTransform);
		}

		internal static void UpdateSearchHandlerText(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			platformControl.Text = TextTransformUtilites.GetTransformedText(searchHandler.Query, searchHandler.TextTransform);
		}

		static readonly string[] backgroundColorKeys =
		{
			"TextControlBackground",
			"TextControlBackgroundPointerOver",
			"TextControlBackgroundFocused",
			"TextControlBackgroundDisabled"
		};

		internal static void UpdateSearchHandlerBackground(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			UpdateColors(platformControl.Resources, backgroundColorKeys, searchHandler.BackgroundColor?.ToPlatform());
			platformControl.RefreshThemeResources();
		}

		internal static void UpdateSearchHandlerIsEnabled(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			platformControl.IsEnabled = searchHandler.IsSearchEnabled;
		}

		internal static void UpdateSearchHandlerCharacterSpacing(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			platformControl.CharacterSpacing = searchHandler.CharacterSpacing.ToEm();
		}

		internal static void UpdateSearchHandlerPlaceholder(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			platformControl.PlaceholderText = searchHandler.Placeholder ?? string.Empty;
		}

		static readonly string[] placeholderForegroundColorKeys =
		{
			"TextControlPlaceholderForeground",
			"TextControlPlaceholderForegroundPointerOver",
			"TextControlPlaceholderForegroundFocused",
			"TextControlPlaceholderForegroundDisabled"
		};

		internal static void UpdateSearchHandlerPlaceholderColor(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			UpdateColors(platformControl.Resources, placeholderForegroundColorKeys,
				searchHandler.PlaceholderColor?.ToPlatform());

			platformControl.RefreshThemeResources();
		}

		static readonly string[] foregroundColorKeys =
		{
			"TextControlForeground",
			"TextControlForegroundPointerOver",
			"TextControlForegroundFocused",
			"TextControlForegroundDisabled"
		};

		internal static void UpdateSearchHandlerTextColor(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			var tintBrush = searchHandler.TextColor?.ToPlatform();

			if (tintBrush == null)
			{
				platformControl.Resources.RemoveKeys(foregroundColorKeys);
				platformControl.Foreground = null;
			}
			else
			{
				platformControl.Resources.SetValueForAllKey(foregroundColorKeys, tintBrush);
				platformControl.Foreground = tintBrush;
			}

			platformControl.RefreshThemeResources();
		}

		static void UpdateColors(UI.Xaml.ResourceDictionary resource, string[] keys, UI.Xaml.Media.Brush? brush)
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
		internal static void UpdateSearchHandlerHorizontalTextAlignment(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			platformControl.HorizontalContentAlignment = searchHandler.HorizontalTextAlignment.ToPlatformHorizontalAlignment();
		}

		internal static void UpdateSearchHandlerVerticalTextAlignment(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			platformControl.VerticalContentAlignment = searchHandler.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}

		static readonly string[] cancelButtonColorKeys =
		{
			"TextControlButtonForeground",
			"TextControlButtonForegroundPointerOver",
			"TextControlButtonForegroundPressed",
		};

		internal static void UpdateSearchHandlerCancelButtonColor(this AutoSuggestBox platformControl, SearchHandler searchHandler)
		{
			var cancelButton = platformControl.GetDescendantByName<UI.Xaml.Controls.Button>("DeleteButton");

			if (cancelButton is null)
				return;

			cancelButton.UpdateTextColor(searchHandler.CancelButtonColor, cancelButtonColorKeys);
		}
	}
}

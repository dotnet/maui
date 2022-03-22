#nullable enable
using Microsoft.Maui.Graphics;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.Header = null;

			nativeComboBox.HeaderTemplate = string.IsNullOrEmpty(picker.Title) ? null : 
				(UI.Xaml.DataTemplate)UI.Xaml.Application.Current.Resources["ComboBoxHeader"];

			nativeComboBox.DataContext = picker;
		}

		public static void UpdateBackground(this MauiComboBox nativeComboBox, IPicker picker)
		{
			var platformBrush = picker.Background?.ToPlatform();

			if (platformBrush == null)
			{
				nativeComboBox.Resources.RemoveKeys(_backgroundColorResourceKeys);
			}
			else
			{
				nativeComboBox.Resources.SetValueForAllKey(_backgroundColorResourceKeys, platformBrush);
			}
		}

		static readonly string[] _backgroundColorResourceKeys =
		{
			"ComboBoxBackground",
			"ComboBoxBackgroundPointerOver",
			"ComboBoxBackgroundPressed",
			"ComboBoxBackgroundDisabled",
			"ComboBoxBackgroundUnfocused",
		};


		public static void UpdateTextColor(this MauiComboBox nativeComboBox, IPicker picker)
		{
			var platformBrush = picker.TextColor?.ToPlatform();
			if (platformBrush == null)
			{
				nativeComboBox.Resources.RemoveKeys(_textColorResourceKeys);
			}
			else
			{
				nativeComboBox.Resources.SetValueForAllKey(_textColorResourceKeys, platformBrush);
				nativeComboBox.Foreground = platformBrush;
			}

			//Color color = picker.TextColor;
			//if (color.IsDefault() && defaultForeground == null)
			//	return;

			//nativeComboBox.Foreground = color.IsDefault() ? (defaultForeground ?? color.ToPlatform()) : color.ToPlatform();
		}

		static readonly string[] _textColorResourceKeys =
		{
			"ComboBoxForeground",
			"ComboBoxForegroundDisabled",
			"ComboBoxForegroundFocused",
			"ComboBoxForegroundFocusedPressed",
		};

		public static void UpdateSelectedIndex(this MauiComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.SelectedIndex = picker.SelectedIndex;
		}

		public static void UpdateCharacterSpacing(this MauiComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.CharacterSpacing = picker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this MauiComboBox nativeComboBox, IPicker picker, IFontManager fontManager) =>
			nativeComboBox.UpdateFont(picker.Font, fontManager); 
		
		public static void UpdateHorizontalTextAlignment(this MauiComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.HorizontalContentAlignment = picker.HorizontalTextAlignment.ToPlatformHorizontalAlignment();
		}

		public static void UpdateVerticalTextAlignment(this MauiComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.VerticalContentAlignment = picker.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}
	}
}
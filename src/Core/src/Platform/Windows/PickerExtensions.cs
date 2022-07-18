#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this ComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.Header = null;

			nativeComboBox.HeaderTemplate = string.IsNullOrEmpty(picker.Title) ? null :
				(UI.Xaml.DataTemplate)UI.Xaml.Application.Current.Resources["ComboBoxHeader"];

			nativeComboBox.DataContext = picker;
		}

		public static void UpdateBackground(this ComboBox nativeComboBox, IPicker picker)
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


		public static void UpdateTextColor(this ComboBox nativeComboBox, IPicker picker)
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
		}

		static readonly string[] _textColorResourceKeys =
		{
			"ComboBoxForeground",
			"ComboBoxForegroundDisabled",
			"ComboBoxForegroundFocused",
			"ComboBoxForegroundFocusedPressed",
		};

		public static void UpdateSelectedIndex(this ComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.SelectedIndex = picker.SelectedIndex;
		}

		public static void UpdateCharacterSpacing(this ComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.CharacterSpacing = picker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this ComboBox nativeComboBox, IPicker picker, IFontManager fontManager) =>
			nativeComboBox.UpdateFont(picker.Font, fontManager);

		public static void UpdateHorizontalTextAlignment(this ComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.HorizontalContentAlignment = picker.HorizontalTextAlignment.ToPlatformHorizontalAlignment();
		}

		public static void UpdateVerticalTextAlignment(this ComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.VerticalContentAlignment = picker.VerticalTextAlignment.ToPlatformVerticalAlignment();
		}
	}
}
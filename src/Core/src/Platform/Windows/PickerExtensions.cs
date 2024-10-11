﻿#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this ComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.Header = string.IsNullOrEmpty(picker.Title) ? null : picker;

			nativeComboBox.HeaderTemplate = string.IsNullOrEmpty(picker.Title) ? null :
				(UI.Xaml.DataTemplate)UI.Xaml.Application.Current.Resources["ComboBoxHeader"];

		}

		public static void UpdateBackground(this ComboBox nativeComboBox, IPicker picker)
		{
			var platformBrush = picker.Background?.ToPlatform();

			if (platformBrush == null)
				nativeComboBox.Resources.RemoveKeys(BackgroundColorResourceKeys);
			else
				nativeComboBox.Resources.SetValueForAllKey(BackgroundColorResourceKeys, platformBrush);

			nativeComboBox.RefreshThemeResources();
		}

		static readonly string[] BackgroundColorResourceKeys =
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
				nativeComboBox.Resources.RemoveKeys(TextColorResourceKeys);
				nativeComboBox.ClearValue(ComboBox.ForegroundProperty);
			}
			else
			{
				nativeComboBox.Resources.SetValueForAllKey(TextColorResourceKeys, platformBrush);
				nativeComboBox.Foreground = platformBrush;
			}
		}

		// ResourceKeys controlling the foreground color of the ComboBox.
		// https://docs.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.combobox?view=windows-app-sdk-1.1
		static readonly string[] TextColorResourceKeys =
		{
			"ComboBoxForeground",
			"ComboBoxForegroundPointerOver",
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
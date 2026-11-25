#nullable enable
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this ComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.Header = string.IsNullOrEmpty(picker.Title) ? null : picker;

			if (string.IsNullOrEmpty(picker.Title))
			{
				nativeComboBox.HeaderTemplate = null;
				return;
			}

			var resources = UI.Xaml.Application.Current.Resources;
			var headerTemplate = (UI.Xaml.DataTemplate)resources["ComboBoxHeader"];
			if (picker.TitleColor.IsDefault() && IsDefaultHeaderTemplate(resources, headerTemplate))
				headerTemplate = (UI.Xaml.DataTemplate)resources["ComboBoxHeaderDefaultTitleColor"];

			nativeComboBox.HeaderTemplate = headerTemplate;
		}

		static bool IsDefaultHeaderTemplate(UI.Xaml.ResourceDictionary resources, UI.Xaml.DataTemplate headerTemplate)
		{
			foreach (var key in resources.Keys)
			{
				if (key is string resourceKey && resourceKey == "ComboBoxHeaderDefaultTitleColor")
					return ReferenceEquals(resources["ComboBoxHeader"], headerTemplate);
			}

			if (resources.MergedDictionaries is { } mergedDictionaries)
			{
				foreach (var dictionary in mergedDictionaries)
				{
					if (IsDefaultHeaderTemplate(dictionary, headerTemplate))
						return true;
				}
			}

			return false;
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

			nativeComboBox.RefreshThemeResources();
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
			var characterSpacing = picker.CharacterSpacing.ToEm();
			nativeComboBox.CharacterSpacing = characterSpacing;

			// Apply directly to the selected item's TextBlock so the closed picker reflects spacing.
			// If the control isn't loaded yet, defer until Loaded so the visual tree exists.
			if (nativeComboBox.IsLoaded)
			{
				ApplyCharacterSpacingToSelectedItem(nativeComboBox, characterSpacing);
			}
			else
			{
				nativeComboBox.OnLoaded(() =>
					ApplyCharacterSpacingToSelectedItem(nativeComboBox, nativeComboBox.CharacterSpacing));
			}
		}

		internal static void ApplyCharacterSpacingToSelectedItem(this ComboBox nativeComboBox, int characterSpacing)
		{
			var contentPresenter = nativeComboBox.GetDescendantByName<ContentPresenter>("ContentPresenter");
			var textBlock = contentPresenter?.GetFirstDescendant<TextBlock>();

			if (textBlock is not null)
			{
				textBlock.CharacterSpacing = characterSpacing;
			}
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

		internal static void UpdateIsOpen(this ComboBox nativeComboBox, IPicker picker)
		{
			if (!nativeComboBox.IsLoaded)
			{
				RoutedEventHandler? onLoaded = null;
				onLoaded = (s, e) =>
				{
					nativeComboBox.Loaded -= onLoaded;
					UpdateIsOpen(nativeComboBox, picker);
				};
				nativeComboBox.Loaded += onLoaded;
				return;
			}

			nativeComboBox.IsDropDownOpen = picker.IsOpen;
		}
	}
}
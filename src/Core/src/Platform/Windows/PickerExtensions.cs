#nullable enable
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

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
			var brush = picker.Background?.ToPlatform();

			if (platformBrush == null)
				nativeComboBox.Resources.RemoveKeys(BackgroundColorResourceKeys);
			else
			{
				nativeComboBox.Resources.SetValueForKey(ComboBoxBackground, brush);
				nativeComboBox.Resources.SetValueForKey(ComboBoxBackgroundUnfocused, brush);
				nativeComboBox.Resources.SetValueForAllKey(PointerBackgroundColorResourceKeys, brush.Darker());
				nativeComboBox.Resources.SetValueForKey(ComboBoxBackgroundDisabled, brush.Lighter());
			}

			nativeComboBox.RefreshThemeResources();
		}

		static readonly string ComboBoxBackground = "ComboBoxBackground";
		static readonly string ComboBoxBackgroundPointerOver = "ComboBoxBackgroundPointerOver";
		static readonly string ComboBoxBackgroundPressed = "ComboBoxBackgroundPressed";
		static readonly string ComboBoxBackgroundDisabled = "ComboBoxBackgroundDisabled";
		static readonly string ComboBoxBackgroundUnfocused = "ComboBoxBackgroundUnfocused";

		static readonly string[] BackgroundColorResourceKeys =
		{
			ComboBoxBackground,
			ComboBoxBackgroundPointerOver,
			ComboBoxBackgroundPressed,
			ComboBoxBackgroundDisabled,
			ComboBoxBackgroundUnfocused,
		}; 
		
		static readonly string[] PointerBackgroundColorResourceKeys =
		{
			ComboBoxBackgroundPointerOver,
			ComboBoxBackgroundPressed,
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
#nullable enable
using Microsoft.Maui.Graphics;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui
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
		public static void UpdateTextColor(this MauiComboBox nativeComboBox, IPicker picker)
		{
			nativeComboBox.UpdateTextColor(picker, null);
		}

		public static void UpdateTextColor(this MauiComboBox nativeComboBox, IPicker picker, WBrush? defaultForeground)
		{
			Color color = picker.TextColor;
			if (color.IsDefault() && defaultForeground == null)
				return;

			nativeComboBox.Foreground = color.IsDefault() ? (defaultForeground ?? color.ToNative()) : color.ToNative();
		}

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
			nativeComboBox.HorizontalContentAlignment = picker.HorizontalTextAlignment.ToNativeHorizontalAlignment();
		}
	}
}

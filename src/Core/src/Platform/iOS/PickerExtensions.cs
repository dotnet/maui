using System;
using Foundation;
using Microsoft.Maui.Handlers;
using UIKit;

namespace Microsoft.Maui
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker nativePicker, IPicker picker) =>
			nativePicker.UpdatePicker(picker);

		public static void UpdateSelectedIndex(this MauiPicker nativePicker, IPicker picker) =>
			nativePicker.SetSelectedIndex(picker, picker.SelectedIndex);

		internal static void UpdatePicker(this MauiPicker nativePicker, IPicker picker)
		{
			var selectedIndex = picker.SelectedIndex;
			var items = picker.Items;

			nativePicker.Text = selectedIndex == -1 || items == null || selectedIndex >= items.Count ? string.Empty : items[selectedIndex];

			var pickerView = nativePicker.UIPickerView;
			pickerView?.ReloadAllComponents();

			if (items == null || items.Count == 0)
				return;

			nativePicker.SetSelectedIndex(picker, selectedIndex);
			nativePicker.SetSelectedItem(picker);
		}

		public static void UpdateCharacterSpacing(this MauiPicker nativePicker, IPicker picker)
		{
			var textAttr = nativePicker.AttributedText?.WithCharacterSpacing(picker.CharacterSpacing);

			if (textAttr != null)
				nativePicker.AttributedText = textAttr;
		}

		internal static void SetSelectedIndex(this MauiPicker nativePicker, IPicker picker, int selectedIndex = 0)
		{
			picker.SelectedIndex = selectedIndex;

			var pickerView = nativePicker.UIPickerView;

			if (pickerView?.Model is PickerSource source)
			{
				source.SelectedIndex = selectedIndex;
				source.SelectedItem = selectedIndex >= 0 ? picker.Items[selectedIndex] : null;
			}

			pickerView?.Select(Math.Max(selectedIndex, 0), 0, true);
		}

		internal static void SetSelectedItem(this MauiPicker nativePicker, IPicker picker)
		{
			if (nativePicker == null)
				return;

			int index = picker.SelectedIndex;

			if (index == -1)
			{
				picker.SelectedItem = null;
				return;
			}

			if (picker.ItemsSource != null)
			{
				picker.SelectedItem = picker.ItemsSource[index];
				return;
			}

			picker.SelectedItem = picker.Items[index];
		}
	}
}
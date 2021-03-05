using System;
using Foundation;
using UIKit;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this NativePicker nativePicker, IPicker picker) =>
			nativePicker.UpdatePicker(picker);
				
		public static void UpdateSelectedIndex(this NativePicker nativePicker, IPicker picker) =>
			nativePicker.SetSelectedIndex(picker, picker.SelectedIndex);

		internal static void UpdatePicker(this NativePicker nativePicker, IPicker picker)
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

		internal static void SetSelectedIndex(this NativePicker nativePicker, IPicker picker, int selectedIndex = 0)
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

		internal static void SetSelectedItem(this NativePicker nativePicker, IPicker picker)
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

		internal static void UpdateAttributedPlaceholder(this NativePicker nativePicker, NSAttributedString nsAttributedString)
		{
			nativePicker.AttributedPlaceholder = nsAttributedString;
		}
	}
}
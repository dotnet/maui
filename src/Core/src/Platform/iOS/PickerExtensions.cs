#nullable enable
using System;
using Microsoft.Maui.Handlers;

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

			nativePicker.Text = selectedIndex == -1 ? "" : picker.GetItem(selectedIndex);

			var pickerView = nativePicker.UIPickerView;
			pickerView?.ReloadAllComponents();

			if (picker.GetCount() == 0)
				return;

			nativePicker.SetSelectedIndex(picker, selectedIndex);
		}

		internal static void SetSelectedIndex(this MauiPicker nativePicker, IPicker picker, int selectedIndex = 0)
		{
			picker.SelectedIndex = selectedIndex;

			var pickerView = nativePicker.UIPickerView;

			if (pickerView?.Model is PickerSource source)
			{
				source.SelectedIndex = selectedIndex;
			}

			pickerView?.Select(Math.Max(selectedIndex, 0), 0, true);
		}

	}
}
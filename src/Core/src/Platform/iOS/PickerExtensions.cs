#nullable enable
using System;
using Foundation;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker platformPicker, IPicker picker) =>
			platformPicker.UpdatePickerTitle(picker);

		public static void UpdateTitleColor(this MauiPicker platformPicker, IPicker picker) =>
 			platformPicker.UpdatePickerTitle(picker);

		public static void UpdateTextColor(this MauiPicker platformPicker, IPicker picker) =>
			platformPicker.TextColor = picker.TextColor?.ToPlatform();

		public static void UpdateSelectedIndex(this MauiPicker platformPicker, IPicker picker) =>
			platformPicker.UpdatePicker(picker, picker.SelectedIndex);

		internal static void UpdateIsOpen(this MauiPicker platformPicker, IPicker picker)
		{
			if (picker.IsOpen)
				platformPicker.BecomeFirstResponder();
			else
				platformPicker.ResignFirstResponder();
		}

		internal static void UpdateAttributedPlaceholder(this MauiPicker platformPicker, NSAttributedString nsAttributedString)
		{
			platformPicker.AttributedPlaceholder = nsAttributedString;
		}

		internal static void UpdatePickerTitle(this MauiPicker platformPicker, IPicker picker)
		{
			platformPicker.UpdateAttributedPlaceholder(new NSAttributedString(picker.Title ?? string.Empty, null, picker?.TitleColor?.ToPlatform()));
		}

		internal static void UpdatePicker(this MauiPicker platformPicker, IPicker picker, int? newSelectedIndex = null)
		{
			var selectedIndex = newSelectedIndex ?? picker.SelectedIndex;

			if (selectedIndex != -1)
			{
				platformPicker.Text = picker.GetItem(selectedIndex);
			}
			else
			{
				platformPicker.Text = null;
				platformPicker.UpdatePickerTitle(picker);
			}

			var pickerView = platformPicker.UIPickerView;
			pickerView?.ReloadAllComponents();

			if (picker.GetCount() == 0)
				return;

			picker.SelectedIndex = selectedIndex;

			if (pickerView?.Model is PickerSource source)
			{
				source.SelectedIndex = selectedIndex;
			}

			pickerView?.Select(Math.Max(selectedIndex, 0), 0, true);
		}
	}
}
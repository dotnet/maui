#nullable enable
using System;
using Foundation;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker platformPicker, IPicker picker) =>
			platformPicker.UpdatePicker(picker);

		public static void UpdateTitleColor(this MauiPicker platformPicker, IPicker picker) =>
 			platformPicker.SetTitleColor(picker);

		public static void UpdateTextColor(this MauiPicker platformPicker, IPicker picker) =>
			platformPicker.TextColor = picker.TextColor?.ToPlatform();

		public static void UpdateSelectedIndex(this MauiPicker platformPicker, IPicker picker) =>
			platformPicker.UpdatePicker(picker, picker.SelectedIndex);

		internal static void SetTitleColor(this MauiPicker platformPicker, IPicker picker)
		{
			var title = picker.Title;

			if (string.IsNullOrEmpty(title))
				return;

			var titleColor = picker.TitleColor;

			if (titleColor == null)
				return;

			platformPicker.UpdateAttributedPlaceholder(new NSAttributedString(title, null, titleColor.ToPlatform()));
		}

		internal static void UpdateAttributedPlaceholder(this MauiPicker platformPicker, NSAttributedString nsAttributedString)
		{
			platformPicker.AttributedPlaceholder = nsAttributedString;
		}

		internal static void UpdatePicker(this MauiPicker platformPicker, IPicker picker, int? newSelectedIndex = null)
		{
			var selectedIndex = newSelectedIndex ?? picker.SelectedIndex;

			// Revert to placeholder/title if nothing selected
			platformPicker.Text = selectedIndex == -1
				? (picker.Title ?? string.Empty)
				: picker.GetItem(selectedIndex);

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
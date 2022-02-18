using Android.Content.Res;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker platformPicker, IPicker picker) =>
			UpdatePicker(platformPicker, picker);

		public static void UpdateTitleColor(this MauiPicker platformPicker, IPicker picker, ColorStateList? defaultColor)
		{
			var titleColor = picker.TitleColor;

			if (titleColor == null)
			{
				platformPicker.SetHintTextColor(defaultColor);
			}
			else
			{
				var androidColor = titleColor.ToPlatform();
				if (!platformPicker.TextColors.IsOneColor(ColorStates.EditText, androidColor))
					platformPicker.SetHintTextColor(ColorStateListExtensions.CreateEditText(androidColor));
			}
		}

		public static void UpdateTextColor(this MauiPicker platformPicker, IPicker picker, ColorStateList? defaultColor)
		{
			var textColor = picker.TextColor;

			if (textColor == null)
			{
				platformPicker.SetTextColor(defaultColor);
			}
			else
			{
				var androidColor = textColor.ToPlatform();
				if (!platformPicker.TextColors.IsOneColor(ColorStates.EditText, androidColor))
					platformPicker.SetTextColor(ColorStateListExtensions.CreateEditText(androidColor));
			}
		}

		public static void UpdateSelectedIndex(this MauiPicker platformPicker, IPicker picker) =>
			UpdatePicker(platformPicker, picker);

		internal static void UpdatePicker(this MauiPicker platformPicker, IPicker picker)
		{
			platformPicker.Hint = picker.Title;

			if (picker.SelectedIndex == -1 || picker.SelectedIndex >= picker.GetCount())
				platformPicker.Text = null;
			else
				platformPicker.Text = picker.GetItem(picker.SelectedIndex);
		}
	}
}
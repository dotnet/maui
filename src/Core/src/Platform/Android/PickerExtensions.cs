using Android.App;
using Android.Content.Res;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker platformPicker, IPicker picker) =>
			UpdatePicker(platformPicker, picker);

		public static void UpdateTitleColor(this MauiPicker platformPicker, IPicker picker)
		{
			var titleColor = picker.TitleColor;

			if (titleColor != null)
			{
				if (PlatformInterop.CreateEditTextColorStateList(platformPicker.TextColors, titleColor.ToPlatform()) is ColorStateList c)
					platformPicker.SetHintTextColor(c);
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
				if (PlatformInterop.CreateEditTextColorStateList(platformPicker.TextColors, textColor.ToPlatform()) is ColorStateList c)
					platformPicker.SetTextColor(c);
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

		internal static void UpdateFlowDirection(this AndroidX.AppCompat.App.AlertDialog alertDialog, MauiPicker platformPicker)
		{
			var platformLayoutDirection = platformPicker.LayoutDirection;

			// Propagate the MauiPicker LayoutDirection to the AlertDialog
			var dv = alertDialog.Window?.DecorView;

			if (dv is not null)
				dv.LayoutDirection = platformLayoutDirection;

			var lv = alertDialog?.ListView;

			if (lv is not null)
			{
				lv.LayoutDirection = platformLayoutDirection;
				lv.TextDirection = platformLayoutDirection.ToTextDirection();
			}
		}
	}
}
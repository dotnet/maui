using Android.App;
using Android.Content.Res;
using AndroidX.AppCompat.Widget;
using AppCompatAlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Microsoft.Maui.Platform
{
	public static class PickerExtensions
	{
		public static void UpdateTitle(this MauiPicker platformPicker, IPicker picker) =>
			UpdatePicker(platformPicker, picker);

		public static void UpdateTitleColor(this MauiPicker platformPicker, IPicker picker)
		{
			platformPicker.UpdateTitleColorCore(picker);
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
			platformPicker.UpdatePickerCore(picker);
		}

		internal static void UpdateFlowDirection(this AppCompatAlertDialog alertDialog, MauiPicker platformPicker)
		{
			alertDialog.UpdateFlowDirectionCore(platformPicker);
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdateTitle(this MauiMaterialPicker platformPicker, IPicker picker)
		{
			UpdatePicker(platformPicker, picker);
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdateTitleColor(this MauiMaterialPicker platformPicker, IPicker picker)
		{
			platformPicker.UpdateTitleColorCore(picker);
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdateSelectedIndex(this MauiMaterialPicker platformPicker, IPicker picker)
		{
			UpdatePicker(platformPicker, picker);
		}

		internal static void UpdatePicker(this MauiMaterialPicker platformPicker, IPicker picker)
		{
			platformPicker.UpdatePickerCore(picker);
		}

		internal static void UpdateFlowDirection(this AppCompatAlertDialog alertDialog, MauiMaterialPicker platformPicker)
		{
			alertDialog.UpdateFlowDirectionCore(platformPicker);
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdateTitleColorCore(this AppCompatEditText platformPicker, IPicker picker)
		{
			var titleColor = picker.TitleColor;

			if (titleColor is not null)
			{
				if (PlatformInterop.CreateEditTextColorStateList(platformPicker.TextColors, titleColor.ToPlatform()) is ColorStateList c)
				{
					platformPicker.SetHintTextColor(c);
				}
				else if (picker.TextColor == picker.TitleColor)
				{
					platformPicker.SetHintTextColor(titleColor.ToPlatform());
				}
			}
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdatePickerCore(this AppCompatEditText platformPicker, IPicker picker)
		{
			platformPicker.Hint = picker.Title;

			if (picker.SelectedIndex == -1 || picker.SelectedIndex >= picker.GetCount())
			{
				platformPicker.Text = null;
			}
			else
			{
				platformPicker.Text = picker.GetItem(picker.SelectedIndex);
			}
		}

		// TODO: Material3 - make it public in .net 11
		internal static void UpdateFlowDirectionCore(this AppCompatAlertDialog alertDialog, AppCompatEditText platformPicker)
		{
			var platformLayoutDirection = platformPicker.LayoutDirection;

			// Propagate the LayoutDirection to the AlertDialog
			var dv = alertDialog.Window?.DecorView;

			dv?.LayoutDirection = platformLayoutDirection;

			var lv = alertDialog?.ListView;

			if (lv is not null)
			{
				lv.LayoutDirection = platformLayoutDirection;
				lv.TextDirection = platformLayoutDirection.ToTextDirection();
			}
		}
	}
}
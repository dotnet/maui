using ObjCRuntime;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public static class DatePickerExtensions
	{
		public static void UpdateFormat(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateDate(datePicker, null);
		}

		public static void UpdateFormat(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			platformDatePicker.UpdateDate(datePicker, picker);
		}

		public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateDate(datePicker, null);
		}

		public static void UpdateTextColor(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIColor? defaultTextColor)
		{
			var textColor = datePicker.TextColor;

			if (textColor == null)
				platformDatePicker.TextColor = defaultTextColor;
			else
				platformDatePicker.TextColor = textColor.ToPlatform();

			// HACK This forces the color to update; there's probably a more elegant way to make this happen
			platformDatePicker.UpdateDate(datePicker);
		}

		public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null && picker.Date.ToDateTime().Date != datePicker.Date.Date)
				picker.SetDate(datePicker.Date.ToNSDate(), false);

			platformDatePicker.Text = datePicker.Date.ToString(datePicker.Format);

			platformDatePicker.UpdateCharacterSpacing(datePicker);
		}

		public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null)
			{
				picker.MinimumDate = datePicker.MinimumDate.ToNSDate();
			}
		}

		public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateMaximumDate(datePicker, null);
		}

		public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null)
			{
				picker.MaximumDate = datePicker.MaximumDate.ToNSDate();
			}
		}
	}
}
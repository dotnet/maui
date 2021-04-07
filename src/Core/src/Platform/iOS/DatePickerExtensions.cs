using UIKit;

namespace Microsoft.Maui
{
	public static class DatePickerExtensions
	{
		public static void UpdateFormat(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.UpdateDate(datePicker, null);
		}

		public static void UpdateFormat(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			nativeDatePicker.UpdateDate(datePicker, picker);
		}

		public static void UpdateDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.UpdateDate(datePicker, null);
		}

		public static void UpdateDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null && picker.Date.ToDateTime().Date != datePicker.Date.Date)
				picker.SetDate(datePicker.Date.ToNSDate(), false);

			nativeDatePicker.Text = datePicker.Date.ToString(datePicker.Format);

			nativeDatePicker.UpdateCharacterSpacing(datePicker);
		}

		public static void UpdateMinimumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMinimumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null)
			{
				picker.MinimumDate = datePicker.MinimumDate.ToNSDate();
			}
		}

		public static void UpdateMaximumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.UpdateMaximumDate(datePicker, null);
		}

		public static void UpdateMaximumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null)
			{
				picker.MaximumDate = datePicker.MaximumDate.ToNSDate();
			}
		}
	}
}
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

		public static void UpdateTextColor(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, UIColor? defaultTextColor)
		{
			var textColor = datePicker.TextColor;

			if (textColor == null)
				nativeDatePicker.TextColor = defaultTextColor;
			else
				nativeDatePicker.TextColor = textColor.ToNative();

			// HACK This forces the color to update; there's probably a more elegant way to make this happen
			nativeDatePicker.UpdateDate(datePicker);
		}

		public static void UpdateDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			var datePickerDate = datePicker.Date;

			if (picker != null && picker.Date?.ToDateTime().Date != datePickerDate?.Date)
            {
				var nsDate = datePickerDate.ToNSDate();
				if (nsDate != null)
                {
					picker.SetDate(nsDate, false);
				}
			}
				
			nativeDatePicker.Text = datePickerDate?.ToString(datePicker.Format);

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
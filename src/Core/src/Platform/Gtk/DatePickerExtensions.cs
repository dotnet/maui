namespace Microsoft.Maui
{
	public static class DatePickerExtensions
	{
		public static void UpdateDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.Date = datePicker.Date;

		}
		
		public static void UpdateFormat(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.Format = datePicker.Format;

		}
	}
}
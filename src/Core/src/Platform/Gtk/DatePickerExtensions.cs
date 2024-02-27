using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public static class DatePickerExtensions
	{
		public static void UpdateDate(this MauiDatePicker platformView, IDatePicker datePicker)
		{
			platformView.Date = datePicker.Date;

		}
		
		public static void UpdateFormat(this MauiDatePicker platformView, IDatePicker datePicker)
		{
			platformView.Format = datePicker.Format;

		}
	}
}
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class DatePickerExtensions
	{
		public static void UpdateFormat(this Entry nativeDatePicker, IDatePicker datePicker)
		{
			UpdateDate(nativeDatePicker, datePicker);
		}

		public static void UpdateDate(this Entry nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.Text = datePicker.Date.ToString(datePicker.Format);
		}
	}
}
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Platform
{
	public static class DatePickerExtensions
	{
		public static void UpdateFormat(this Entry platformDatePicker, IDatePicker datePicker)
		{
			UpdateDate(platformDatePicker, datePicker);
		}

		public static void UpdateDate(this Entry platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.Text = datePicker.Date.ToString(datePicker.Format);
		}
	}
}
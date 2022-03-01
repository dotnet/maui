namespace Microsoft.Maui.Controls
{
	public partial class DatePicker
	{
		public static void MapText(DatePickerHandler handler, DatePicker datePicker)
		{
			Platform.MauiDatePickerExtensions.UpdateText(handler.PlatformView, datePicker, datePicker.TextTransform);
		}
	}
}
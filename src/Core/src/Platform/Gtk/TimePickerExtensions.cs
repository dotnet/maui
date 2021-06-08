namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateTime(this MauiTimePicker nativeTimePicker, ITimePicker timePicker)
		{
			nativeTimePicker.Time = timePicker.Time;

		}
		
		public static void UpdateFormat(this MauiTimePicker nativeTimePicker, ITimePicker timePicker)
		{
			nativeTimePicker.Format = timePicker.Format;

		}
	}
}

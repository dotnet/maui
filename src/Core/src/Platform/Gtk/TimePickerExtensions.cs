using Microsoft.Maui.Platform;

namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateTime(this MauiTimePicker platformView, ITimePicker timePicker)
		{
			platformView.Time = timePicker.Time;

		}
		
		public static void UpdateFormat(this MauiTimePicker platformView, ITimePicker timePicker)
		{
			platformView.Format = timePicker.Format;

		}
	}
}

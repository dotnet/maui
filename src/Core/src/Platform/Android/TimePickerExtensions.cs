using Android.Text.Format;

namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker view)
		{
			mauiTimePicker.SetTime(view);
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker view)
		{
			mauiTimePicker.SetTime(view);
		}

		internal static void SetTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			var time = timePicker.Time;
			var format = timePicker.Format;

			mauiTimePicker.Text = time.ToFormattedString(format);
		}

		internal static bool Is24HourView(this MauiTimePicker mauiTimePicker, ITimePicker? timePicker)
		{
			return timePicker != null && (DateFormat.Is24HourFormat(mauiTimePicker.Context) && timePicker.Format == "t" || timePicker.Format == "HH:mm");
		}
	}
}
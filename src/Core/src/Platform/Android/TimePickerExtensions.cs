using System;
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

		internal static bool Is24HourView(this MauiTimePicker mauiTimePicker, ITimePicker? timePicker)
		{
			return timePicker != null && (DateFormat.Is24HourFormat(mauiTimePicker.Context) && timePicker.Format == "t" || timePicker.Format == "HH:mm");
		}

		internal static void SetTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			var time = timePicker.Time;

			if (string.IsNullOrEmpty(timePicker.Format))
			{
				var timeFormat = "t";
				mauiTimePicker.Text = DateTime.Today.Add(time).ToString(timeFormat);
			}
			else
			{
				var timeFormat = timePicker.Format;
				mauiTimePicker.Text = DateTime.Today.Add(time).ToString(timeFormat);
			}
		}
	}
}
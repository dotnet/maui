using System;

namespace Microsoft.Maui
{
	public static class TimeExtensions
	{
		public static string ToFormattedString(this ITimePicker timePicker)
		{
			var time = timePicker.Time;
			var format = timePicker.Format;

			return time.ToFormattedString(format);
		}

		public static string ToFormattedString(this TimeSpan time, string format)
		{
			if (string.IsNullOrEmpty(format))
			{
				var timeFormat = "t";
				return DateTime.Today.Add(time).ToString(timeFormat);
			}
			else
			{
				return DateTime.Today.Add(time).ToString(format);
			}
		}
	}
}

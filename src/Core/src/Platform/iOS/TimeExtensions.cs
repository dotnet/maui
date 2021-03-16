using System;
using System.Globalization;

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
			var cultureInfo = Culture.CurrentCulture;

			return time.ToFormattedString(format, cultureInfo);
		}

		public static string ToFormattedString(this TimeSpan time, string format, CultureInfo cultureInfo)
		{
			if (string.IsNullOrEmpty(format))
			{
				string timeformat = cultureInfo.DateTimeFormat.ShortTimePattern;
				return DateTime.Today.Add(time).ToString(timeformat, cultureInfo);
			}
			else
			{
				return DateTime.Today.Add(time).ToString(format, cultureInfo);
			}
		}
	}
}
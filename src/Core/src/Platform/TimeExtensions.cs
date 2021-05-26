#nullable enable
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

		public static string ToFormattedString(this TimeSpan time, string format, CultureInfo? cultureInfo = null)
		{
			cultureInfo ??= CultureInfo.CurrentCulture;

			if (string.IsNullOrEmpty(format))
			{
				format = cultureInfo.DateTimeFormat.ShortTimePattern;
			}

			return DateTime.Today.Add(time).ToString(format, cultureInfo);
		}
	}
}

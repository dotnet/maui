using System;
using System.Globalization;
using System.Linq;
using Foundation;

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
			string iOSLocale = NSLocale.CurrentLocale.CountryCode;
			var cultureInfo = CultureInfo.GetCultures(CultureTypes.AllCultures)
				.Where(c => c.Name.EndsWith("-" + iOSLocale)).FirstOrDefault();

			if (cultureInfo == null)
				cultureInfo = CultureInfo.InvariantCulture;


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
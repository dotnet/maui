using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Microsoft.Maui
{
	public static partial class DateExtensions
	{
		public static string ToFormattedString(this IDatePicker datePicker)
		{
			var date = datePicker.Date;
			var format = datePicker.Format;

			return date.ToFormattedString(format);
		}

		public static string ToFormattedString(this DateTime dateTime, string format, CultureInfo? cultureInfo = null)
		{
			cultureInfo ??= CultureInfo.CurrentCulture;

			if (string.IsNullOrEmpty(format))
			{
				format = cultureInfo.DateTimeFormat.ShortTimePattern;
			}

			return dateTime.ToString(format);
		}
	}
}

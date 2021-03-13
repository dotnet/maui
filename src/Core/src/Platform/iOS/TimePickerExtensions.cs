using System;
using System.Globalization;
using System.Linq;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateFormat(this MauiTimePicker nativeView, ITimePicker view)
		{
			nativeView.UpdateTime(view, null);
		}

		public static void UpdateFormat(this MauiTimePicker nativeView, ITimePicker view, UIDatePicker? picker)
		{
			nativeView.UpdateTime(view, picker);
		}

		public static void UpdateTime(this MauiTimePicker nativeView, ITimePicker view)
		{
			nativeView.UpdateTime(view, null);
		}

		public static void UpdateTime(this MauiTimePicker nativeTimePicker, ITimePicker timePicker, UIDatePicker? picker)
		{
			if (picker != null)
				picker.Date = new DateTime(1, 1, 1).Add(timePicker.Time).ToNSDate();

			var cultureInfo = Culture.CurrentCulture;

			if (string.IsNullOrEmpty(timePicker.Format))
			{
				string timeformat = cultureInfo.DateTimeFormat.ShortTimePattern;
				NSLocale locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);

				if (picker != null)
					picker.Locale = locale;
			}

			var time = timePicker.Time;
			var format = timePicker.Format;

			nativeTimePicker.Text = time.ToFormattedString(format, cultureInfo);

			if (timePicker.Format?.Contains('H') == true)
			{
				var ci = new CultureInfo("de-DE");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

				if (picker != null)
					picker.Locale = locale;
			}
			else if (timePicker.Format?.Contains('h') == true)
			{
				var ci = new CultureInfo("en-US");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

				if (picker != null)
					picker.Locale = locale;
			}
		}
	}
}
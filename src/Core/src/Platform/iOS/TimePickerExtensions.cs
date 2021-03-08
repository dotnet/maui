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

			string iOSLocale = NSLocale.CurrentLocale.CountryCode;
			var cultureInfos = CultureInfo.GetCultures(CultureTypes.AllCultures)
				.Where(c => c.Name.EndsWith("-" + iOSLocale)).FirstOrDefault();

			if (cultureInfos == null)
				cultureInfos = CultureInfo.InvariantCulture;

			if (string.IsNullOrEmpty(timePicker.Format))
			{
				string timeformat = cultureInfos.DateTimeFormat.ShortTimePattern;
				NSLocale locale = new NSLocale(cultureInfos.TwoLetterISOLanguageName);
				nativeTimePicker.Text = DateTime.Today.Add(timePicker.Time).ToString(timeformat, cultureInfos);

				if (picker != null)
					picker.Locale = locale;
			}
			else
			{
				nativeTimePicker.Text = DateTime.Today.Add(timePicker.Time).ToString(timePicker.Format, cultureInfos);
			}

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
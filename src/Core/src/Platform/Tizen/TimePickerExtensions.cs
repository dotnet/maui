using System;
using System.Globalization;
using Tizen.UIExtensions.ElmSharp;

namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		static readonly string s_defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;
		static TimeSpan Time = DateTime.Now.TimeOfDay;

		public static void UpdateFormat(this Entry nativeTimePicker, ITimePicker timePicker)
		{
			UpdateTime(nativeTimePicker, timePicker);
		}

		public static void UpdateTime(this Entry nativeTimePicker, ITimePicker timePicker)
		{
			// Xamarin using DateTime formatting (https://developer.xamarin.com/api/property/Xamarin.Forms.TimePicker.Format/)
			nativeTimePicker.Text = new DateTime(Time.Ticks).ToString(timePicker.Format ?? s_defaultFormat);
		}

	}
}
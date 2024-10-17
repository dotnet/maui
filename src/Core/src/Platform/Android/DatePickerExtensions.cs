using System;
using Android.App;
using Android.Content.Res;

namespace Microsoft.Maui.Platform
{
	public static class DatePickerExtensions
	{
		public static void UpdateFormat(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.SetText(datePicker);
		}

		public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.SetText(datePicker);
		}

		public static void UpdateTextColor(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			var textColor = datePicker.TextColor;

			if (textColor != null)
			{
				if (PlatformInterop.CreateEditTextColorStateList(platformDatePicker.TextColors, textColor.ToPlatform()) is ColorStateList c)
					platformDatePicker.SetTextColor(c);
			}
		}

		public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, DatePickerDialog? datePickerDialog)
		{
			if (datePickerDialog != null)
			{
				datePickerDialog.DatePicker.MinDate = (long)datePicker.MinimumDate.ToUniversalTimeNative().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, DatePickerDialog? datePickerDialog)
		{
			if (datePickerDialog != null)
			{
				datePickerDialog.DatePicker.MaxDate = (long)datePicker.MaximumDate.ToUniversalTimeNative().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		internal static DateTime ToUniversalTimeNative(this DateTime date)
		{
			if (date.Kind == DateTimeKind.Utc)
			{
				return date;
			}
			var timeZone = Java.Util.TimeZone.Default;
			if (timeZone != null && date != DateTime.MaxValue && date != DateTime.MinValue)
			{
				return date.AddHours(-1 * (double)timeZone.RawOffset / 1000 / 60 / 60);
			}
			return date.ToUniversalTime();
		}

		internal static void SetText(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.Text = datePicker.Date.ToString(datePicker.Format);
		}
	}
}
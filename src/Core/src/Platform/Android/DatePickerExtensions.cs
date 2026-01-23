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

			if (textColor is not null)
			{
				if (PlatformInterop.CreateEditTextColorStateList(platformDatePicker.TextColors, textColor.ToPlatform()) is ColorStateList c)
				{
					platformDatePicker.SetTextColor(c);
				}
			}
		}

		public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, DatePickerDialog? datePickerDialog)
		{
			if (datePickerDialog is not null)
			{
				// Reset to 0 first to force Android to accept the new value.
				datePickerDialog.DatePicker.MinDate = 0;

				if (datePicker.MinimumDate is null)
				{
					datePickerDialog.DatePicker.MinDate = (long)DateTime.MinValue.ToUniversalTime()
						.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;

					return;
				}

				datePickerDialog.DatePicker.MinDate = (long)datePicker.MinimumDate.Value
					.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, DatePickerDialog? datePickerDialog)
		{
			if (datePickerDialog is not null)
			{
				// Reset to a large value first to force Android to accept the new value.
				datePickerDialog.DatePicker.MaxDate = long.MaxValue;

				if (datePicker.MaximumDate is null)
				{
					datePickerDialog.DatePicker.MaxDate = (long)DateTime.MaxValue.ToUniversalTime()
						.Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;

					return;
				}

				datePickerDialog.DatePicker.MaxDate = (long)datePicker.MaximumDate.Value
					.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		internal static void SetText(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.Text = datePicker.Date?.ToString(datePicker.Format) ?? string.Empty;
		}
	}
}
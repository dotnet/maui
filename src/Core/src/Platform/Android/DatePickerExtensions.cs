using System;
using Android.App;
using Android.Util;

namespace Microsoft.Maui
{
	public static class DatePickerExtensions
	{
		public static void UpdateFormat(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.SetText(datePicker);
		}

		public static void UpdateDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.SetText(datePicker);
		}

		public static void UpdateMinimumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMinimumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, DatePickerDialog? datePickerDialog)
		{
			if (datePickerDialog != null)
			{
				datePickerDialog.DatePicker.MinDate = (long)datePicker.MinimumDate.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		public static void UpdateMaximumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.UpdateMinimumDate(datePicker, null);
		}

		public static void UpdateMaximumDate(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, DatePickerDialog? datePickerDialog)
		{
			if (datePickerDialog != null)
			{
				datePickerDialog.DatePicker.MaxDate = (long)datePicker.MaximumDate.ToUniversalTime().Subtract(DateTime.MinValue.AddYears(1969)).TotalMilliseconds;
			}
		}

		public static void UpdateCharacterSpacing(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.LetterSpacing = datePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this MauiDatePicker nativeDatePicker, IDatePicker datePicker, IFontManager fontManager)
		{
			var font = datePicker.Font;

			var tf = fontManager.GetTypeface(font);
			nativeDatePicker.Typeface = tf;

			var sp = fontManager.GetScaledPixel(font);
			nativeDatePicker.SetTextSize(ComplexUnitType.Sp, sp);
		}

		internal static void SetText(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.Text = datePicker.Date.ToString(datePicker.Format);
		}
	}
}
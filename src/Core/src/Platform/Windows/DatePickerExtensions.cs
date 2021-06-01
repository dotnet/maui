using System;
using System.Linq;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class DatePickerExtensions
	{
		public static void UpdateDate(this DatePicker nativeDatePicker, IDatePicker datePicker)
		{
			var date = datePicker.Date;
			nativeDatePicker.UpdateDate(date);

			nativeDatePicker.UpdateDay(datePicker);
			nativeDatePicker.UpdateMonth(datePicker);
			nativeDatePicker.UpdateYear(datePicker);
		}

		public static void UpdateDate(this DatePicker nativeDatePicker, DateTime dateTime)
		{
			nativeDatePicker.Date = new DateTimeOffset(new DateTime(dateTime.Ticks, DateTimeKind.Unspecified));
		}
	
		public static void UpdateMinimumDate(this DatePicker nativeDatePicker, IDatePicker datePicker)
		{
			DateTime mindate = datePicker.MinimumDate;

			try
			{
				nativeDatePicker.MinYear = new DateTimeOffset(new DateTime(datePicker.MinimumDate.Ticks, DateTimeKind.Unspecified));
			}
			catch (ArgumentOutOfRangeException)
			{
				// This will be thrown when mindate equals DateTime.MinValue and the UTC offset is positive
				// because the resulting DateTimeOffset.UtcDateTime will be out of range. In that case let's
				// specify the Kind as UTC so there is no offset.
				mindate = DateTime.SpecifyKind(mindate, DateTimeKind.Utc);
				nativeDatePicker.MinYear = new DateTimeOffset(mindate);
			}
		}

		public static void UpdateMaximumDate(this DatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.MaxYear = new DateTimeOffset(new DateTime(datePicker.MaximumDate.Ticks, DateTimeKind.Unspecified));
		}

		public static void UpdateCharacterSpacing(this DatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.CharacterSpacing = datePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this DatePicker nativeDatePicker, IDatePicker datePicker, IFontManager fontManager) =>
			nativeDatePicker.UpdateFont(datePicker.Font, fontManager);

		internal static void UpdateDay(this DatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.DayVisible = true;

			if (string.IsNullOrWhiteSpace(datePicker.Format) || datePicker.Format.Equals("d"))
			{
				nativeDatePicker.DayFormat = "day";
			}
			else if (datePicker.Format.Equals("D"))
			{
				nativeDatePicker.DayFormat = "dayofweek.full";
			}
			else
			{
				var day = datePicker.Format.Count(x => x == 'd');
				if (day == 0)
					nativeDatePicker.DayVisible = false;
				else if (day == 3)
					nativeDatePicker.DayFormat = "day dayofweek.abbreviated";
				else if (day == 4)
					nativeDatePicker.DayFormat = "dayofweek.full";
				else
					nativeDatePicker.DayFormat = "day";
			}
		}

		internal static void UpdateMonth(this DatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.MonthVisible = true;

			if (string.IsNullOrWhiteSpace(datePicker.Format) || datePicker.Format.Equals("d"))
			{
				nativeDatePicker.MonthFormat = "month";
			}
			else if (datePicker.Format.Equals("D"))
			{
				nativeDatePicker.MonthFormat = "month.full";
			}
			else
			{
				var month = datePicker.Format.Count(x => x == 'M');
				if (month == 0)
					nativeDatePicker.MonthVisible = false;
				else if (month <= 2)
					nativeDatePicker.MonthFormat = "month.numeric";
				else if (month == 3)
					nativeDatePicker.MonthFormat = "month.abbreviated";
				else
					nativeDatePicker.MonthFormat = "month.full";
			}
		}

		internal static void UpdateYear(this DatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.YearVisible = true;

			if (string.IsNullOrWhiteSpace(datePicker.Format) || datePicker.Format.Equals("d"))
			{
				nativeDatePicker.YearFormat = "year";
			}
			else if (datePicker.Format.Equals("D"))
			{
				nativeDatePicker.YearFormat = "year.full";
			}
			else
			{
				var year = datePicker.Format.Count(x => x == 'y');
				if (year == 0)
					nativeDatePicker.YearVisible = false;
				else if (year <= 2)
					nativeDatePicker.YearFormat = "year.abbreviated";
				else
					nativeDatePicker.YearFormat = "year.full";
			}
		}
	}
}
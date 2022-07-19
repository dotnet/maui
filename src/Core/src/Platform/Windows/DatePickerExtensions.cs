using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class DatePickerExtensions
	{
		public static void UpdateDate(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			var date = datePicker.Date;
			platformDatePicker.UpdateDate(date);

			var format = datePicker.Format;
			var dateFormat = format.ToDateFormat();

			if (!string.IsNullOrEmpty(dateFormat))
				platformDatePicker.DateFormat = dateFormat;
				
			platformDatePicker.UpdateTextColor(datePicker);
		}

		public static void UpdateDate(this CalendarDatePicker platformDatePicker, DateTime dateTime)
		{
			platformDatePicker.Date = dateTime.ToDateTimeOffset();
		}

		public static void UpdateMinimumDate(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.MinDate = datePicker.MinimumDate.ToDateTimeOffset();
		}

		public static void UpdateMaximumDate(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.MaxDate = datePicker.MaximumDate.ToDateTimeOffset();
		}

		public static void UpdateCharacterSpacing(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.CharacterSpacing = datePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this CalendarDatePicker platformDatePicker, IDatePicker datePicker, IFontManager fontManager) =>
			platformDatePicker.UpdateFont(datePicker.Font, fontManager);

		public static void UpdateTextColor(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			Color textColor = datePicker.TextColor;

			WBrush? brush = textColor?.ToPlatform();

			if (brush is null)
			{
				platformDatePicker.Resources.RemoveKeys(TextColorResourceKeys);
				platformDatePicker.Foreground = null;
			}
			else
			{
				platformDatePicker.Resources.SetValueForAllKey(TextColorResourceKeys, brush);
				platformDatePicker.Foreground = brush;
			}

			platformDatePicker.RefreshThemeResources();
		}

		// ResourceKeys controlling the foreground color of the CalendarDatePicker.
		// https://docs.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.calendardatepicker?view=windows-app-sdk-1.1
		static readonly string[] TextColorResourceKeys =
		{
			"CalendarDatePickerTextForeground",
			"CalendarDatePickerTextForegroundDisabled",
			"CalendarDatePickerTextForegroundSelected"
		};
	}
}

using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

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
			platformDatePicker.Date = dateTime;
		}

		public static void UpdateMinimumDate(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.MinDate = datePicker.MinimumDate;
		}

		public static void UpdateMaximumDate(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.MaxDate = datePicker.MaximumDate;
		}

		public static void UpdateCharacterSpacing(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
		    var characterSpacing = datePicker.CharacterSpacing.ToEm();
			platformDatePicker.CharacterSpacing = characterSpacing;

			var dateTextBlock = platformDatePicker.GetDescendantByName<TextBlock>("DateText");
			if (dateTextBlock is not null)
			{
				dateTextBlock.CharacterSpacing = characterSpacing;
				dateTextBlock.RefreshThemeResources();
			}
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
				platformDatePicker.ClearValue(CalendarDatePicker.ForegroundProperty);
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
			"CalendarDatePickerTextForegroundPointerOver",
			"CalendarDatePickerTextForegroundPressed",
			"CalendarDatePickerTextForegroundDisabled",
			"CalendarDatePickerTextForegroundSelected",

			// below resource keys are used for the calendar icon
			"CalendarDatePickerCalendarGlyphForeground",
			"CalendarDatePickerCalendarGlyphForegroundPointerOver",
			"CalendarDatePickerCalendarGlyphForegroundPressed",
			"CalendarDatePickerCalendarGlyphForegroundDisabled",
		};

		// TODO NET8 add to public API
		internal static void UpdateBackground(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			var brush = datePicker?.Background?.ToPlatform();

			if (brush is null)
			{
				platformDatePicker.Resources.RemoveKeys(BackgroundColorResourceKeys);
				platformDatePicker.ClearValue(CalendarDatePicker.BackgroundProperty);
			}
			else
			{
				platformDatePicker.Resources.SetValueForAllKey(BackgroundColorResourceKeys, brush);
				platformDatePicker.Background = brush;
			}

			platformDatePicker.RefreshThemeResources();
		}

		static readonly string[] BackgroundColorResourceKeys =
		{
			"CalendarDatePickerBackground",
			"CalendarDatePickerBackgroundPointerOver",
			"CalendarDatePickerBackgroundPressed",
			"CalendarDatePickerBackgroundDisabled",
			"CalendarDatePickerBackgroundFocused",
		};
	}
}

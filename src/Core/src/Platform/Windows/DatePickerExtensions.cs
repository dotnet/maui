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
			if (datePicker.Date is null)
			{
				platformDatePicker.Date = null;
			}
			else
			{
				platformDatePicker.UpdateDate(datePicker.Date.Value);
			}

			var format = datePicker.Format;
			var dateFormat = format.ToDateFormat();

			if (!string.IsNullOrEmpty(dateFormat))
			{
				platformDatePicker.DateFormat = dateFormat;
			}

			platformDatePicker.UpdateTextColor(datePicker);
		}

		public static void UpdateDate(this CalendarDatePicker platformDatePicker, DateTime dateTime)
		{
			platformDatePicker.Date = dateTime;
		}

		public static void UpdateMinimumDate(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			if (datePicker?.MinimumDate is not null)
			{
				platformDatePicker.MinDate = datePicker.MinimumDate.Value;
			}
			else
			{
				// Matches WinUI default MinDate behavior by jumping 100 years back if MinDate is null.
				// Ref: https://github.com/microsoft/microsoft-ui-xaml/blob/2aa50f0dff795cbd948588ee0e62cac7da3a396f/src/dxaml/xcp/components/DependencyObject/DependencyProperty.cpp#L253
				platformDatePicker.MinDate = DateTime.Now.AddYears(-100);
			}
		}

		public static void UpdateMaximumDate(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.MaxDate = datePicker?.MaximumDate ?? DateTime.MaxValue;
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

		public static void UpdateBackground(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
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

		internal static void UpdateIsOpen(this CalendarDatePicker platformDatePicker, IDatePicker datePicker)
		{
			platformDatePicker.IsCalendarOpen = datePicker.IsOpen;
		}
	}
}

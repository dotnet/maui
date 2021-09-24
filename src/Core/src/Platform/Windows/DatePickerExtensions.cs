using System;
using System.Linq;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui
{
	public static class DatePickerExtensions
	{
		public static void UpdateDate(this CalendarDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			var date = datePicker.Date;
			nativeDatePicker.UpdateDate(date);
		}

		public static void UpdateDate(this CalendarDatePicker nativeDatePicker, DateTime dateTime)
		{
			nativeDatePicker.Date = dateTime;
		}
	
		public static void UpdateMinimumDate(this CalendarDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.MinDate = datePicker.MinimumDate;
		}

		public static void UpdateMaximumDate(this CalendarDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.MaxDate = datePicker.MaximumDate;
		}

		public static void UpdateCharacterSpacing(this CalendarDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			nativeDatePicker.CharacterSpacing = datePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this CalendarDatePicker nativeDatePicker, IDatePicker datePicker, IFontManager fontManager) =>
			nativeDatePicker.UpdateFont(datePicker.Font, fontManager);

		public static void UpdateTextColor(this CalendarDatePicker nativeDatePicker, IDatePicker datePicker, WBrush? defaultForeground)
		{
			Color textColor = datePicker.TextColor;
			nativeDatePicker.Foreground = textColor == null ? (defaultForeground ?? textColor?.ToNative()) : textColor.ToNative();
		}
	}
}
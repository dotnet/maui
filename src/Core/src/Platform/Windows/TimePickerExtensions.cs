using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.Platform
{
	public static class TimePickerExtensions
	{
		public static void UpdateTime(this TimePicker nativeTimePicker, ITimePicker timePicker)
		{
			nativeTimePicker.Time = timePicker.Time;

			if (timePicker.Format?.Contains('H', StringComparison.Ordinal) == true)
			{
				nativeTimePicker.ClockIdentifier = "24HourClock";
			}
			else
			{
				nativeTimePicker.ClockIdentifier = "12HourClock";
			}
		}

		public static void UpdateCharacterSpacing(this TimePicker platformTimePicker, ITimePicker timePicker)
		{
			platformTimePicker.CharacterSpacing = timePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this TimePicker platformTimePicker, ITimePicker timePicker, IFontManager fontManager) =>
			platformTimePicker.UpdateFont(timePicker.Font, fontManager);

		public static void UpdateTextColor(this TimePicker platformTimePicker, ITimePicker timePicker)
		{
			Color textColor = timePicker.TextColor;

			UI.Xaml.Media.Brush? brush = textColor?.ToPlatform();

			if (brush is null)
			{
				platformTimePicker.Resources.Remove("TimePickerButtonForeground");
				platformTimePicker.Resources.Remove("TimePickerButtonForegroundPointerOver");
				platformTimePicker.Resources.Remove("TimePickerButtonForegroundPressed");
				platformTimePicker.Resources.Remove("TimePickerButtonForegroundDisabled");

				platformTimePicker.ClearValue(TimePicker.ForegroundProperty);
			}
			else
			{
				platformTimePicker.Resources["TimePickerButtonForeground"] = brush;
				platformTimePicker.Resources["TimePickerButtonForegroundPointerOver"] = brush;
				platformTimePicker.Resources["TimePickerButtonForegroundPressed"] = brush;
				platformTimePicker.Resources["TimePickerButtonForegroundDisabled"] = brush;

				platformTimePicker.Foreground = brush;
			}
		}
	}
}
using System;
using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

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

		public static void UpdateCharacterSpacing(this TimePicker nativeTimePicker, ITimePicker timePicker)
		{
			nativeTimePicker.CharacterSpacing = timePicker.CharacterSpacing.ToEm();
		}

		public static void UpdateFont(this TimePicker nativeTimePicker, ITimePicker timePicker, IFontManager fontManager) =>
			nativeTimePicker.UpdateFont(timePicker.Font, fontManager);

		public static void UpdateTextColor(this TimePicker nativeTimePicker, ITimePicker timePicker)
		{
			Color textColor = timePicker.TextColor;
			if (textColor != null)
				nativeTimePicker.Foreground = textColor.ToPlatform();
		}
	}
}
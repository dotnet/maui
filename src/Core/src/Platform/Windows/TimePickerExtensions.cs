using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateTime(this TimePicker nativeTimePicker, ITimePicker timePicker)
		{
			nativeTimePicker.Time = timePicker.Time;

			if (timePicker.Format?.Contains('H') == true)
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
	}
}
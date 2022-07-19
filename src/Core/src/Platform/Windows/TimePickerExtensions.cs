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
			var brush = timePicker.TextColor?.ToPlatform();

			if (brush is null)
				platformTimePicker.Resources.RemoveKeys(TextColorResourceKeys);
			else
				platformTimePicker.Resources.SetValueForAllKey(TextColorResourceKeys, brush);

			platformTimePicker.RefreshThemeResources();
		}

		static readonly string[] TextColorResourceKeys =
		{
			"TimePickerButtonForeground",
			"TimePickerButtonForegroundDefault",
			"TimePickerButtonForegroundPointerOver",
			"TimePickerButtonForegroundPressed",
			"TimePickerButtonForegroundDisabled",
			"TimePickerButtonForegroundFocused",
		};
	}
}
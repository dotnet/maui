using Microsoft.Maui.Graphics;
using Microsoft.UI.Xaml.Controls;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

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
	
		public static void UpdateTextColor(this TimePicker nativeTimePicker, ITimePicker timePicker,WBrush? defaultForeground)
		{
			Color textColor = timePicker.TextColor;
			nativeTimePicker.Foreground = textColor == null ? (defaultForeground ?? textColor?.ToNative()) : textColor.ToNative();
		}
	}
}
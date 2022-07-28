using System;
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerHandlerTests
	{
		CalendarDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			datePickerHandler.PlatformView;

		DateTime GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var plaformDatePicker = GetNativeDatePicker(datePickerHandler);
			var date = plaformDatePicker.Date;

			if (date.HasValue)
				return date.Value.DateTime;

			return DateTime.MinValue;
		}

		Color GetNativeTextColor(DatePickerHandler datePickerHandler)
		{
			var foreground = GetNativeDatePicker(datePickerHandler).Foreground;

			if (foreground is UI.Xaml.Media.SolidColorBrush solidColorBrush)
				return solidColorBrush.Color.ToColor();

			return null;
		}
	}
}

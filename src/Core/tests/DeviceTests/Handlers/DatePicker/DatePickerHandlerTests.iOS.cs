using System;
using Microsoft.Maui.Handlers;

namespace Microsoft.Maui.DeviceTests
{
	public partial class DatePickerHandlerTests
	{
		MauiDatePicker GetNativeDatePicker(DatePickerHandler datePickerHandler) =>
			(MauiDatePicker)datePickerHandler.View;

		DateTime GetNativeDate(DatePickerHandler datePickerHandler)
		{
			var dateString = GetNativeDatePicker(datePickerHandler).Text;
			DateTime.TryParse(dateString, out DateTime result);

			return result;
		}
	}
}
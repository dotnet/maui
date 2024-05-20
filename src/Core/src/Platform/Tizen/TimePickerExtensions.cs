using System;
using System.Globalization;
using Tizen.UIExtensions.NUI;

namespace Microsoft.Maui.Platform
{
	public static class TimePickerExtensions
	{
		static readonly string s_defaultFormat = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

		public static void UpdateFormat(this Entry platformTimePicker, ITimePicker timePicker)
		{
			UpdateTime(platformTimePicker, timePicker);
		}

		public static void UpdateTime(this Entry platformTimePicker, ITimePicker timePicker)
		{
			// .NET MAUI using DateTime formatting (https://learn.microsoft.com//dotnet/api/microsoft.maui.controls.timepicker.format)
			platformTimePicker.Text = new DateTime(timePicker.Time.Ticks).ToString(timePicker.Format ?? s_defaultFormat);
		}

	}
}
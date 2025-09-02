using Android.Content.Res;

namespace Microsoft.Maui.Platform
{
	public static class TimePickerExtensions
	{
		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.SetTime(timePicker);
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.SetTime(timePicker);
		}

		internal static void SetTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			var time = timePicker.Time;
			var format = timePicker.Format;

			mauiTimePicker.Text = time.ToFormattedString(format);
		}

		internal static void UpdateTextColor(this MauiTimePicker platformTimePicker, ITimePicker timePicker)
		{
			var textColor = timePicker.TextColor;

			if (textColor != null)
			{
				if (PlatformInterop.CreateEditTextColorStateList(platformTimePicker.TextColors, textColor.ToPlatform()) is ColorStateList c)
					platformTimePicker.SetTextColor(c);
			}
		}
	}
}
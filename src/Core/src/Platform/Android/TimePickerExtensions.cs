using System;
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

		// TODO: Make it public in Net10
		internal static void UpdateTextColor(this MauiTimePicker platformTimePicker, ITimePicker timePicker)
		{
			var textColor = timePicker.TextColor;

			if (textColor is not null && PlatformInterop.CreateEditTextColorStateList(platformTimePicker.TextColors, textColor.ToPlatform()) is ColorStateList c)
			{
				platformTimePicker.SetTextColor(c);
			}
			else if (OperatingSystem.IsAndroidVersionAtLeast(23) && platformTimePicker.Context?.Theme is Resources.Theme theme)
			{
				// Restore to default (theme primary text color) instead of passing null
				using var ta = theme.ObtainStyledAttributes([Android.Resource.Attribute.TextColorPrimary]);
				if (ta.GetColorStateList(0) is ColorStateList cs)
				{
					platformTimePicker.SetTextColor(cs);
				}
			}
		}

		// Make it public in .NET 10.
		internal static void UpdateTextAlignment(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.TextAlignment = timePicker.FlowDirection == FlowDirection.RightToLeft
					? Android.Views.TextAlignment.TextEnd
					: Android.Views.TextAlignment.TextStart;
		}
	}
}
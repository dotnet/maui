using System;
using Android.Content.Res;
using AndroidX.AppCompat.Widget;

namespace Microsoft.Maui.Platform;

public static class TimePickerExtensions
{
	public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		=> SetTimeImpl(mauiTimePicker, timePicker);

	// TODO: Material3: Make it public in .NET 11
	internal static void UpdateFormat(this MauiMaterialTimePicker mauiTimePicker, ITimePicker timePicker)
		=> SetTimeImpl(mauiTimePicker, timePicker);

	public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		=> SetTimeImpl(mauiTimePicker, timePicker);

	// TODO: Material3: Make it public in .NET 11
	internal static void UpdateTime(this MauiMaterialTimePicker mauiTimePicker, ITimePicker timePicker)
		=> SetTimeImpl(mauiTimePicker, timePicker);

	internal static void SetTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		=> SetTimeImpl(mauiTimePicker, timePicker);

	internal static void SetTime(this MauiMaterialTimePicker mauiTimePicker, ITimePicker timePicker)
		=> SetTimeImpl(mauiTimePicker, timePicker);

	public static void UpdateTextColor(this MauiTimePicker platformTimePicker, ITimePicker timePicker)
		=> UpdateTextColorImpl(platformTimePicker, timePicker);

	// TODO: Material3: Make it public in .NET 11
	internal static void UpdateTextColor(this MauiMaterialTimePicker platformTimePicker, ITimePicker timePicker)
		=> UpdateTextColorImpl(platformTimePicker, timePicker);

	static void SetTimeImpl(AppCompatEditText editText, ITimePicker timePicker)
	{
		var time = timePicker.Time;
		var format = timePicker.Format;

		editText.Text = time?.ToFormattedString(format);
	}

	static void UpdateTextColorImpl(AppCompatEditText platformTimePicker, ITimePicker timePicker)
	{
		var textColor = timePicker.TextColor;

		if (textColor is not null && PlatformInterop.CreateEditTextColorStateList(platformTimePicker.TextColors, textColor.ToPlatform()) is ColorStateList c)
		{
			platformTimePicker.SetTextColor(c);
		}
		else if (OperatingSystem.IsAndroidVersionAtLeast(23) && platformTimePicker.Context?.Theme is Resources.Theme theme)
		{
			// Restore to default (theme primary text color) instead of passing null
			using var ta = theme.ObtainStyledAttributes([global::Android.Resource.Attribute.TextColorPrimary]);
			if (ta.GetColorStateList(0) is ColorStateList cs)
			{
				platformTimePicker.SetTextColor(cs);
			}
		}
	}
}
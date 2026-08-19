using System;
using System.Globalization;
using Foundation;
using Microsoft.Maui.Storage;
using UIKit;

namespace Microsoft.Maui.Platform;

public static class TimePickerExtensions
{
	public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
	{
		mauiTimePicker.UpdateTime(timePicker, null);
	}

	public static void UpdateFormat(this UIDatePicker picker, ITimePicker timePicker)
	{
		picker.UpdateTime(timePicker);
	}

	public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
	{
		mauiTimePicker.UpdateTime(timePicker, picker);
	}

	public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
	{
		mauiTimePicker.UpdateTime(timePicker, null);
	}

	public static void UpdateTime(this UIDatePicker picker, ITimePicker timePicker)
	{
		if (picker is not null)
		{
			picker.Date = new DateTime(1, 1, 1).Add(timePicker?.Time ?? TimeSpan.Zero).ToNSDate();
		}
	}

	public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
	{
		picker?.UpdateTime(timePicker);

		var cultureInfo = Culture.CurrentCulture;
		var time = timePicker.Time;
		var format = timePicker.Format;

		mauiTimePicker.Text = time?.ToFormattedString(format, cultureInfo);

		if (picker is not null)
		{
			if (string.IsNullOrEmpty(format))
			{
				picker.Locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);
			}
			else if (format.Contains('H', StringComparison.Ordinal))
			{
				var ci = new CultureInfo("de-DE");
				picker.Locale = new NSLocale(ci.TwoLetterISOLanguageName);
			}
			else if (format.Contains('h', StringComparison.Ordinal))
			{
				var ci = new CultureInfo("en-US");
				picker.Locale = new NSLocale(ci.TwoLetterISOLanguageName);
			}
		}

		mauiTimePicker.UpdateCharacterSpacing(timePicker);
	}

	public static void UpdateTextAlignment(this MauiTimePicker textField, ITimePicker timePicker)
	{
		UISemanticContentAttribute updateValue = textField.SemanticContentAttribute;

		textField.TextAlignment = (updateValue == UISemanticContentAttribute.ForceRightToLeft) ? UITextAlignment.Right : UITextAlignment.Left;
	}

	internal static void UpdateIsOpen(this UIDatePicker picker, ITimePicker timePicker)
	{
		if (timePicker.IsOpen)
			picker.BecomeFirstResponder();
		else
			picker.ResignFirstResponder();
	}

	internal static void UpdateIsOpen(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
	{
		if (timePicker.IsOpen)
			mauiTimePicker.BecomeFirstResponder();
		else
			mauiTimePicker.ResignFirstResponder();
	}
}
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

		var time = timePicker.Time;
		var format = timePicker.Format;

		mauiTimePicker.Text = time?.ToFormattedString(format);

		if (picker is not null)
		{
			var localeIdentifier = !string.IsNullOrEmpty(format) && format.Contains('H', StringComparison.Ordinal)
				? "de"
				: !string.IsNullOrEmpty(format) && format.Contains('h', StringComparison.Ordinal)
					? "en"
					: CultureInfo.CurrentCulture.Name;

			picker.Locale = new NSLocale(localeIdentifier);
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
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
			mauiTimePicker.UpdateTime(timePicker, null);
		}
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
		if (picker != null)
			picker.Date = new DateTime(1, 1, 1).Add(timePicker.Time).ToNSDate();
	}

	public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
	{
		picker?.UpdateTime(timePicker);

		var cultureInfo = Culture.CurrentCulture;

		if (string.IsNullOrEmpty(timePicker.Format))
		{
			NSLocale locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);

			if (picker != null)
				picker.Locale = locale;
		}

		var time = timePicker.Time;
		var format = timePicker.Format;

		// Determine if format contains AM/PM designator
		bool hasAmPmFormat = format != null && format.Contains('t', StringComparison.Ordinal);

		// Determine which culture to use for consistent formatting
		CultureInfo formattingCulture;
		if (format != null)
		{
			if (hasAmPmFormat || format.Contains('h', StringComparison.Ordinal))
			{
				// For 12-hour format or any format with AM/PM, use US locale
				formattingCulture = new CultureInfo("en-US");
			}

			var time = timePicker.Time;
			var format = timePicker.Format;
			
			// Determine which culture to use for consistent formatting
			CultureInfo formattingCulture;
			if (format != null)
			{
				if (format.Contains('t', StringComparison.Ordinal) || format.Contains('h', StringComparison.Ordinal))
				{
					// For 12-hour format or any format with AM/PM, use US locale
					formattingCulture = new CultureInfo("en-US");
				}
				else if (format.Contains('H', StringComparison.Ordinal))
				{
					// For 24-hour format without AM/PM, use German locale
					formattingCulture = new CultureInfo("de-DE");
				}
				else
				{
					formattingCulture = cultureInfo;
				}
			}
			else
			{
				formattingCulture = cultureInfo;
			}
		}
		else
		{
			formattingCulture = cultureInfo;
		}

		// Apply the same culture to both the text display and the picker
		mauiTimePicker.Text = time.ToFormattedString(format ?? string.Empty, formattingCulture);

		if (picker != null && format != null)
		{
			picker.Locale = new NSLocale(formattingCulture.TwoLetterISOLanguageName);
		}

		mauiTimePicker.UpdateCharacterSpacing(timePicker);
	}

	public static void UpdateTextAlignment(this MauiTimePicker textField, ITimePicker timePicker)
	{
		// TODO: Update TextAlignment based on the EffectiveFlowDirection property.
	}

	public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
	{
		picker?.UpdateTime(timePicker);

		var cultureInfo = Culture.CurrentCulture;

		if (string.IsNullOrEmpty(timePicker.Format))
		{
			NSLocale locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);

			if (picker is not null)
			{
				picker.Locale = locale;
			}
		}

		var time = timePicker.Time;
		var format = timePicker.Format;

		mauiTimePicker.Text = time?.ToFormattedString(format, cultureInfo);

		if (format is not null)
		{
			if (format.Contains('H', StringComparison.Ordinal))
			{
				var ci = new CultureInfo("de-DE");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

				if (picker is not null)
				{
					picker.Locale = locale;
				}
			}
			else if (format.Contains('h', StringComparison.Ordinal))
			{
				var ci = new CultureInfo("en-US");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

				if (picker is not null)
				{
					picker.Locale = locale;
				}
			}
		}

		mauiTimePicker.UpdateCharacterSpacing(timePicker);
	}

	public static void UpdateTextAlignment(this MauiTimePicker textField, ITimePicker timePicker)
	{
		// TODO: Update TextAlignment based on the EffectiveFlowDirection property.
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
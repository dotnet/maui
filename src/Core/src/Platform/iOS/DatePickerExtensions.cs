using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform;

public static class DatePickerExtensions
{
	public static void UpdateFormat(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
	{
		platformDatePicker.UpdateDate(datePicker, null);
	}

	public static void UpdateFormat(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
	{
		platformDatePicker.UpdateDate(datePicker, picker);
	}

	public static void UpdateFormat(this UIDatePicker picker, IDatePicker datePicker)
	{
		picker.UpdateDate(datePicker);
	}

	public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
	{
		platformDatePicker.UpdateDate(datePicker, null);
	}

	public static void UpdateTextColor(this MauiDatePicker platformDatePicker, IDatePicker datePicker) =>
		UpdateTextColor(platformDatePicker, datePicker, null);

	public static void UpdateTextColor(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIColor? defaultTextColor)
	{
		var textColor = datePicker.TextColor;

		if (textColor is null)
		{
			if (defaultTextColor is not null)
			{
				platformDatePicker.TextColor = defaultTextColor;
			}
		}
		else
		{
			platformDatePicker.TextColor = textColor.ToPlatform();
		}

		// HACK This forces the color to update; there's probably a more elegant way to make this happen
		platformDatePicker.UpdateDate(datePicker);
	}

	public static void UpdateDate(this UIDatePicker picker, IDatePicker datePicker)
	{
		if (picker is not null)
		{
			var targetDate = datePicker.Date ?? DateTime.Today;
			if (picker.Date.ToDateTime() != targetDate)
			{
				picker.SetDate(targetDate.ToNSDate(), false);
			}
		}
	}

	public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
	{
		if (picker is not null)
		{
			var targetDate = datePicker.Date ?? DateTime.Today;
			if (picker.Date.ToDateTime() != targetDate)
			{
				picker.SetDate(targetDate.ToNSDate(), false);
			}
		}

		string format = datePicker.Format ?? string.Empty;

		if (datePicker.Date is null)
		{
			platformDatePicker.Text = string.Empty;
		}
		else if (string.IsNullOrWhiteSpace(format) || format.Equals("d", StringComparison.OrdinalIgnoreCase))
		{
			NSDateFormatter dateFormatter = new NSDateFormatter
			{
				TimeZone = NSTimeZone.FromGMT(0)
			};

			// Use datePicker.Date (the source date) for formatting
			// This ensures consistent formatting whether picker is initialized or not
			var nsDate = datePicker.Date.Value.ToNSDate();

			if (format.Equals("D", StringComparison.Ordinal) == true)
			{
				dateFormatter.DateStyle = NSDateFormatterStyle.Full;
				var strDate = dateFormatter.StringFor(nsDate);
				platformDatePicker.Text = strDate;
			}
			else
			{
				dateFormatter.SetLocalizedDateFormatFromTemplate("yMd"); // Forces 4-digit year
				var strDate = dateFormatter.StringFor(nsDate);
				platformDatePicker.Text = strDate;
			}
		}
		else if (format.Contains('/', StringComparison.Ordinal))
		{
			platformDatePicker.Text = datePicker.Date?.ToString(format, CultureInfo.InvariantCulture) ?? string.Empty;
		}
		else
		{
			platformDatePicker.Text = datePicker.Date?.ToString(format) ?? string.Empty;
		}

		platformDatePicker.UpdateCharacterSpacing(datePicker);
	}

	public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
	{
		platformDatePicker.UpdateMinimumDate(datePicker, null);
	}

	public static void UpdateMinimumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
	{
		picker?.UpdateMinimumDate(datePicker);
	}

	public static void UpdateMinimumDate(this UIDatePicker platformDatePicker, IDatePicker datePicker)
	{
		if (platformDatePicker is not null)
		{
			platformDatePicker.MinimumDate = datePicker.MinimumDate?.ToNSDate();
		}
	}

	public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
	{
		platformDatePicker.UpdateMaximumDate(datePicker, null);
	}

	public static void UpdateMaximumDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
	{
		picker?.UpdateMaximumDate(datePicker);
	}

	public static void UpdateMaximumDate(this UIDatePicker platformDatePicker, IDatePicker datePicker)
	{
		if (platformDatePicker is not null)
		{
			platformDatePicker.MaximumDate = datePicker.MaximumDate?.ToNSDate();
		}
	}

	public static void UpdateTextAlignment(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
	{
		// TODO: Update TextAlignment based on the EffectiveFlowDirection property.
	}

	internal static void UpdateIsOpen(this MauiDatePicker platformDatePicker, IDatePicker datePicker)
	{
		if (datePicker.IsOpen)
			platformDatePicker.BecomeFirstResponder();
		else
			platformDatePicker.ResignFirstResponder();
	}
}
using System;
using System.Globalization;
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
				if (defaultTextColor != null)
				{
					platformDatePicker.TextColor = defaultTextColor;
				}
			}
			else
			{
				platformDatePicker.TextColor = textColor.ToPlatform();
			}

		}

		public static void UpdateDate(this UIDatePicker picker, IDatePicker datePicker)
		{
			if (picker != null)
			{
				// If date is equal to MinimumDate (could be default/null value), use Today's date for the picker
				var date = datePicker.Date == datePicker.MinimumDate ? DateTime.Today : datePicker.Date;
				if (picker.Date.ToDateTime().Date != date.Date)
					picker.SetDate(date.ToNSDate(), false);
			}
		}

		public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null)
			{
				// If date is equal to MinimumDate (could be default/null value), use Today's date for the picker
				var date = datePicker.Date == datePicker.MinimumDate ? DateTime.Today : datePicker.Date;
				if (picker.Date.ToDateTime().Date != date.Date)
					picker.SetDate(date.ToNSDate(), false);
			}

			string format = datePicker.Format ?? string.Empty;

			// Can't use VirtualView.Format because it won't display the correct format if the region and language are set differently
			if (picker != null && (string.IsNullOrWhiteSpace(format) || format.Equals("d", StringComparison.OrdinalIgnoreCase)))
			{
				NSDateFormatter dateFormatter = new NSDateFormatter
				{
					TimeZone = NSTimeZone.FromGMT(0)
				};

				if (format.Equals("D", StringComparison.Ordinal) == true)
				{
					dateFormatter.DateStyle = NSDateFormatterStyle.Long;
					var strDate = dateFormatter.StringFor(picker.Date);
					platformDatePicker.Text = strDate;
				}
				else
				{
					dateFormatter.DateStyle = NSDateFormatterStyle.Short;
					var strDate = dateFormatter.StringFor(picker.Date);
					platformDatePicker.Text = strDate;
				}
			}
			else if (format.Contains('/', StringComparison.Ordinal))
			{
				platformDatePicker.Text = datePicker.Date.ToString(format, CultureInfo.InvariantCulture);
			}
			else
			{
				platformDatePicker.Text = datePicker.Date.ToString(format);
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
			if (platformDatePicker != null)
			{
				platformDatePicker.MinimumDate = datePicker.MinimumDate.ToNSDate();
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
		if (picker is not null && picker.Date.ToDateTime() != datePicker.Date)
		{
			picker.SetDate(datePicker.Date?.ToNSDate() ?? NSDate.DistantPast, false);
		}
	}

	public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
	{
		if (picker is not null && picker.Date != NSDate.DistantPast && picker.Date.ToDateTime() != datePicker.Date)
		{
			picker.SetDate(datePicker.Date?.ToNSDate() ?? NSDate.DistantPast, false);
		}

		string format = datePicker.Format ?? string.Empty;

		// Can't use VirtualView.Format because it won't display the correct format if the region and language are set differently
		if (picker is not null && (string.IsNullOrWhiteSpace(format) || format.Equals("d", StringComparison.OrdinalIgnoreCase)))
		{
			NSDateFormatter dateFormatter = new NSDateFormatter
			{
				TimeZone = NSTimeZone.FromGMT(0)
			};

			if (format.Equals("D", StringComparison.Ordinal) == true)
			{
				dateFormatter.DateStyle = NSDateFormatterStyle.Long;
				var strDate = dateFormatter.StringFor(picker.Date);
				platformDatePicker.Text = strDate;
			}
			else
			{
				dateFormatter.DateStyle = NSDateFormatterStyle.Short;
				var strDate = dateFormatter.StringFor(picker.Date);
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
}
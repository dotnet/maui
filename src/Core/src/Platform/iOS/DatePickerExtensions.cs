using System;
using System.Globalization;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
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

			if (textColor == null)
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
			if (picker != null && picker.Date.ToDateTime().Date != datePicker.Date.Date)
				picker.SetDate(datePicker.Date.ToNSDate(), false);
		}

		public static void UpdateDate(this MauiDatePicker platformDatePicker, IDatePicker datePicker, UIDatePicker? picker)
		{
			if (picker != null && picker.Date.ToDateTime().Date != datePicker.Date.Date)
				picker.SetDate(datePicker.Date.ToNSDate(), false);

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
					dateFormatter.DateStyle = NSDateFormatterStyle.Full;
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
			if (platformDatePicker != null)
			{
				platformDatePicker.MaximumDate = datePicker.MaximumDate.ToNSDate();
			}
		}

		public static void UpdateTextAlignment(this MauiDatePicker nativeDatePicker, IDatePicker datePicker)
		{
			// TODO: Update TextAlignment based on the EffectiveFlowDirection property.
		}
	}
}
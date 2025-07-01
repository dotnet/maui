using System;
using System.Globalization;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
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
			if (picker != null)
				picker.Date = new DateTime(1, 1, 1).Add(timePicker.Time).ToNSDate();
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
		{
			picker?.UpdateTime(timePicker);

			var cultureInfo = CultureInfo.CurrentCulture;

			if (string.IsNullOrEmpty(timePicker.Format))
			{
				NSLocale locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);

				if (picker != null)
					picker.Locale = locale;
			}

			var time = timePicker.Time;
			var format = timePicker.Format;

			mauiTimePicker.Text = time.ToFormattedString(format, cultureInfo);

			if (format != null)
			{
				if (format.Contains('H', StringComparison.Ordinal))
				{
					var ci = new CultureInfo("de-DE");
					NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

					if (picker != null)
						picker.Locale = locale;
				}
				else if (format.Contains('h', StringComparison.Ordinal))
				{
					var ci = new CultureInfo("en-US");
					NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

					if (picker != null)
						picker.Locale = locale;
				}
			}

			mauiTimePicker.UpdateCharacterSpacing(timePicker);
		}

		public static void UpdateTextAlignment(this MauiTimePicker textField, ITimePicker timePicker)
		{
			UISemanticContentAttribute updateValue = textField.SemanticContentAttribute;

			textField.TextAlignment = (updateValue == UISemanticContentAttribute.ForceRightToLeft) ? UITextAlignment.Right : UITextAlignment.Left;
		}
	}
}
using System;
using System.Globalization;
using Foundation;
using UIKit;

namespace Microsoft.Maui
{
	public static class TimePickerExtensions
	{
		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.UpdateTime(timePicker, null);
		}

		public static void UpdateFormat(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
		{
			mauiTimePicker.UpdateTime(timePicker, picker);
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			mauiTimePicker.UpdateTime(timePicker, null);
		}

		public static void UpdateTime(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, UIDatePicker? picker)
		{
			if (picker != null)
				picker.Date = new DateTime(1, 1, 1).Add(timePicker.Time).ToNSDate();

			var cultureInfo = Culture.CurrentCulture;

			if (string.IsNullOrEmpty(timePicker.Format))
			{
				NSLocale locale = new NSLocale(cultureInfo.TwoLetterISOLanguageName);

				if (picker != null)
					picker.Locale = locale;
			}

			var time = timePicker.Time;
			var format = timePicker.Format;

			mauiTimePicker.Text = time.ToFormattedString(format, cultureInfo);

			if (timePicker.Format?.Contains('H') == true)
			{
				var ci = new CultureInfo("de-DE");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

				if (picker != null)
					picker.Locale = locale;
			}
			else if (timePicker.Format?.Contains('h') == true)
			{
				var ci = new CultureInfo("en-US");
				NSLocale locale = new NSLocale(ci.TwoLetterISOLanguageName);

				if (picker != null)
					picker.Locale = locale;
			}

			mauiTimePicker.UpdateCharacterSpacing(timePicker);
		}

		public static void UpdateCharacterSpacing(this MauiTimePicker mauiTimePicker, ITimePicker timePicker)
		{
			var textAttr = mauiTimePicker.AttributedText?.WithCharacterSpacing(timePicker.CharacterSpacing);

			if (textAttr != null)
				mauiTimePicker.AttributedText = textAttr;
		}

		public static void UpdateFont(this MauiTimePicker mauiTimePicker, ITimePicker timePicker, IFontManager fontManager)
		{
			var uiFont = fontManager.GetFont(timePicker.Font);
			mauiTimePicker.Font = uiFont;
		}
	}
}
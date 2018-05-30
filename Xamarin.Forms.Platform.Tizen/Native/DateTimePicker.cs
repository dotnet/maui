using System;
using ElmSharp;

namespace Xamarin.Forms.Platform.Tizen.Native
{
	public enum DateTimePickerMode
	{
		Date,
		Time
	}

	public class DateTimePicker : DateTimeSelector
	{
		const string TimeFormat = "%d/%b/%Y %I:%M %p";
		const string TimeLayoutStyle = "time_layout";

		const string DateFormat = "%d/%b/%Y";
		const string DateLayoutStyle = "date_layout";

		DateTimePickerMode _mode = DateTimePickerMode.Date;

		public DateTimePicker(EvasObject parent) : base(parent)
		{
			UpdateMode();
		}

		protected DateTimePicker() : base()
		{
		}

		public DateTimePickerMode Mode
		{
			get { return _mode; }
			set
			{
				if (_mode != value)
				{
					_mode = value;
					UpdateMode();
				}
			}
		}

		public TimeSpan Time
		{
			get
			{
				return DateTime.TimeOfDay;
			}
			set
			{
				DateTime -= DateTime.TimeOfDay;
				DateTime += value;
			}
		}

		protected virtual void UpdateMode()
		{
			if (Mode == DateTimePickerMode.Date)
			{
				Style = DateLayoutStyle;
				Format = DateFormat;
			}
			else
			{
				Style = TimeLayoutStyle;
				Format = TimeFormat;
			}
		}
	}
}

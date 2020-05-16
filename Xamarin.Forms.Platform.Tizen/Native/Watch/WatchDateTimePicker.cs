using System;
using ElmSharp;
using ElmSharp.Wearable;

namespace Xamarin.Forms.Platform.Tizen.Native.Watch
{
	public class WatchDateTimePicker : CircleDateTimeSelector, IRotaryInteraction
	{
		DateTimePickerMode _mode;

		public IRotaryActionWidget RotaryWidget => this;

		public WatchDateTimePicker(EvasObject parent, CircleSurface surface) : base(parent, surface)
		{
			UpdateMode();
		}

		public DateTimePickerMode Mode
		{
			get 
			{ 
				return _mode;
			}
			set
			{
				if (_mode != value)
				{
					_mode = value;
					UpdateMode();
				}
			}
		}

		protected virtual void UpdateMode()
		{
			if (_mode == DateTimePickerMode.Date)
			{
				Style = "datepicker/circle";
				Format = "%d/%b/%Y";
			}
			else
			{
				Style = "timepicker/circle";
				Format = "%d/%b/%Y %I:%M %p";
			}
		}
	}
}

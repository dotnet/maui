using System.Drawing;
using Foundation;
using UIKit;

namespace System.Maui.Platform
{
	public partial class TimePickerRenderer : AbstractViewRenderer<ITimePicker, PickerView>
	{
		UIColor _defaultTextColor;
		UIDatePicker _timePicker;

		protected override PickerView CreateView()
		{
			var pickerView = new PickerView();
			_defaultTextColor = pickerView.TextColor;

			_timePicker = new UIDatePicker
			{
				Mode = UIDatePickerMode.Time,
				TimeZone = new NSTimeZone("UTC"),
				Date = new DateTime(VirtualView.SelectedTime.Ticks).ToNSDate(),
				Locale = GetLocaleForClock()
			};

			var width = (float)UIScreen.MainScreen.Bounds.Width;
			var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);
			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
			{
				VirtualView.SelectedTime = _timePicker.Date.ToDateTime().TimeOfDay;
				pickerView.ResignFirstResponder();
			});

			toolbar.SetItems(new[] { spacer, doneButton }, false);

			_timePicker.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			toolbar.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			pickerView.SetInputView(_timePicker);
			pickerView.SetInputAccessoryView(toolbar);

			return pickerView;
		}

		protected override void DisposeView(PickerView nativeView)
		{
			_defaultTextColor = null;
			_timePicker = null;
			nativeView.SetInputView(null);
			nativeView.SetInputAccessoryView(null);
			base.DisposeView(nativeView);
		}

		private NSLocale GetLocaleForClock()
		{
			const string enUs = "en_US";
			const string enGb = "en_GB";

			var clock = VirtualView.ClockIdentifier;
			var currentLocale = NSLocale.CurrentLocale;

			if (currentLocale.Identifier == enUs && clock == ClockIdentifiers.TwelveHour)
			{
				return currentLocale;
			}

			if (currentLocale.Identifier != enUs && clock == ClockIdentifiers.TwentyFourHour)
			{
				return currentLocale;
			}

			if (clock == ClockIdentifiers.TwentyFourHour)
			{
				return NSLocale.FromLocaleIdentifier(enGb);
			}

			if (clock == ClockIdentifiers.TwelveHour)
			{
				return NSLocale.FromLocaleIdentifier(enUs);
			}

			return currentLocale;
		}
	}
}

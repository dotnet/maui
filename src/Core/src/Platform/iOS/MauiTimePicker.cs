using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform
{
	public class MauiTimePicker : NoCaretField
	{
		readonly Action _dateSelected;
		readonly UIDatePicker _picker;

		public MauiTimePicker(Action dateSelected)
		{
			BorderStyle = UITextBorderStyle.RoundedRect;

			_picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };
			_dateSelected = dateSelected;

			if (PlatformVersion.IsAtLeast(14))
			{
				_picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			InputView = _picker;

			InputAccessoryView = new MauiDoneAccessoryView(() =>
			{
				DateSelected?.Invoke(this, EventArgs.Empty);
				_dateSelected?.Invoke();
			});

			InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			InputAssistantItem.LeadingBarButtonGroups = null;
			InputAssistantItem.TrailingBarButtonGroups = null;

			AccessibilityTraits = UIAccessibilityTrait.Button;
		}

		public UIDatePicker Picker => _picker;

		public NSDate Date => Picker.Date;

		public event EventHandler? DateSelected;

		public void UpdateTime(TimeSpan time)
		{
			_picker.Date = new DateTime(1, 1, 1, time.Hours, time.Minutes, time.Seconds).ToNSDate();
		}
	}
}
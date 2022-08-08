using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform
{
	public class MauiTimePicker : NoCaretField
	{
		readonly UIDatePicker _picker;

#if !MACCATALYST
		readonly Action _dateSelected;
		public MauiTimePicker(Action dateSelected)
#else
		public MauiTimePicker()
#endif
		{
			BorderStyle = UITextBorderStyle.RoundedRect;

			_picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };

#if !MACCATALYST
			_dateSelected = dateSelected;
#endif

			if (OperatingSystem.IsIOSVersionAtLeast(14))
			{
				_picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			InputView = _picker;

#if !MACCATALYST
			InputAccessoryView = new MauiDoneAccessoryView(() =>
			{
				DateSelected?.Invoke(this, EventArgs.Empty);
				_dateSelected?.Invoke();
			});

			InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
#endif

			InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			InputAssistantItem.LeadingBarButtonGroups = null;
			InputAssistantItem.TrailingBarButtonGroups = null;

			AccessibilityTraits = UIAccessibilityTrait.Button;
		}

		public UIDatePicker Picker => _picker;

		public NSDate Date => Picker.Date;

#if !MACCATALYST
		public event EventHandler? DateSelected;
#endif
		public void UpdateTime(TimeSpan time)
		{
			_picker.Date = new DateTime(1, 1, 1, time.Hours, time.Minutes, time.Seconds).ToNSDate();
		}
	}
}
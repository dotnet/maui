using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui
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

			if (NativeVersion.IsAtLeast(14))
			{
				_picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			var width = UIScreen.MainScreen.Bounds.Width;
			var toolbar = new UIToolbar(new RectangleF(0, 0, width, 44)) { BarStyle = UIBarStyle.Default, Translucent = true };
			var spacer = new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace);

			var doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, (o, a) =>
			{
				_dateSelected?.Invoke();
			});

			toolbar.SetItems(new[] { spacer, doneButton }, false);

			InputView = _picker;
			InputAccessoryView = toolbar;

			InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			InputAssistantItem.LeadingBarButtonGroups = null;
			InputAssistantItem.TrailingBarButtonGroups = null;

			AccessibilityTraits = UIAccessibilityTrait.Button;
		}

		public NSDate Date => _picker.Date;
	}
}
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
		readonly WeakEventManager _weakEventManager = new WeakEventManager();
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

			var accessoryView = new MauiDoneAccessoryView();
			accessoryView.DoneClicked += (_, _) =>
			{
				_weakEventManager.HandleEvent(this, EventArgs.Empty, nameof(DateSelected));
				_dateSelected?.Invoke();
			};

			InputAccessoryView = accessoryView;

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
		public event EventHandler? DateSelected
		{
			add => _weakEventManager.AddEventHandler(value, nameof(DateSelected));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(DateSelected));
		}
#endif
		public void UpdateTime(TimeSpan time)
		{
			_picker.Date = new DateTime(1, 1, 1, time.Hours, time.Minutes, time.Seconds).ToNSDate();
		}
	}
}
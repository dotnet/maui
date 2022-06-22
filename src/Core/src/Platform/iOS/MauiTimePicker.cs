using System;
using Foundation;
using UIKit;
using RectangleF = CoreGraphics.CGRect;

namespace Microsoft.Maui.Platform
{
	public class MauiTimePicker : NoCaretField
	{
		readonly WeakEventManager _weakEventManager = new WeakEventManager();

#if !MACCATALYST
		readonly Action _dateSelected;
		public MauiTimePicker(Action dateSelected)
#else
		public MauiTimePicker()
#endif
		{
			BorderStyle = UITextBorderStyle.RoundedRect;

			var picker = new DatePickerInputView { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };
			picker.TimePicker = new WeakReference<MauiTimePicker>(this);

#if !MACCATALYST
			_dateSelected = dateSelected;
#endif

			if (OperatingSystem.IsIOSVersionAtLeast(14))
			{
				picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			InputView = picker;

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

			picker.ValueChanged += OnPickerValueChanged;
		}

		static void OnPickerValueChanged(object? sender, EventArgs e)
		{
			if (sender is DatePickerInputView datePicker &&
				datePicker.TimePicker?.TryGetTarget(out MauiTimePicker? mtp) == true)
			{
				mtp._weakEventManager.HandleEvent(sender, e, nameof(PickerValueChanged));
			}
		}

		public UIDatePicker Picker => (InputView as UIDatePicker)!;

		public NSDate Date => Picker.Date;

#if !MACCATALYST
		public event EventHandler? DateSelected
		{
			add => _weakEventManager.AddEventHandler(value, nameof(DateSelected));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(DateSelected));
		}
#endif

		internal event EventHandler? PickerValueChanged
		{
			add => _weakEventManager.AddEventHandler(value, nameof(PickerValueChanged));
			remove => _weakEventManager.RemoveEventHandler(value, nameof(PickerValueChanged));
		}

		public void UpdateTime(TimeSpan time)
		{
			Picker.Date = new DateTime(1, 1, 1, time.Hours, time.Minutes, time.Seconds).ToNSDate();
		}

		class DatePickerInputView : UIDatePicker
		{
			public WeakReference<MauiTimePicker>? TimePicker { get; set; }
		}
	}
}
using System;
using System.Diagnostics.CodeAnalysis;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiTimePicker : NoCaretField
	{
		[UnconditionalSuppressMessage("Memory", "MEM0002", Justification = "Proven safe in test: MemoryTests.HandlerDoesNotLeak")]
		readonly UIDatePicker _picker;

#if !MACCATALYST
		readonly MauiDoneAccessoryViewProxy _proxy;
		public MauiTimePicker(Action dateSelected)
#else
		public MauiTimePicker()
#endif
		{
			BorderStyle = UITextBorderStyle.RoundedRect;

			_picker = new UIDatePicker { Mode = UIDatePickerMode.Time, TimeZone = new NSTimeZone("UTC") };

#if !MACCATALYST
			_proxy = new(dateSelected);
#endif

			if (OperatingSystem.IsIOSVersionAtLeast(13, 4) || OperatingSystem.IsMacCatalyst())
			{
				_picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			InputView = _picker;

#if !MACCATALYST
			InputAccessoryView = new MauiDoneAccessoryView(_proxy.OnDateSelected);

			InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
#endif

			InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			InputAssistantItem.LeadingBarButtonGroups = null;
			InputAssistantItem.TrailingBarButtonGroups = null;

			AccessibilityTraits = UIAccessibilityTrait.Button;
		}

		public UIDatePicker Picker => _picker;

		public NSDate Date => Picker.Date;

		public void UpdateTime(TimeSpan? time)
		{
			_picker.Date = new DateTime(1, 1, 1, time?.Hours ?? 0,
				time?.Minutes ?? 0, time?.Seconds ?? 0).ToNSDate();
		}

#if !MACCATALYST
		[Obsolete("Use MauiTimePicker constructor instead.")]
		public event EventHandler? DateSelected
		{
			add { }
			remove { }
		}

		class MauiDoneAccessoryViewProxy
		{
			readonly Action _dateSelected;

			public MauiDoneAccessoryViewProxy(Action dateSelected) => _dateSelected = dateSelected;

			public void OnDateSelected() => _dateSelected();
		}
#endif
	}
}
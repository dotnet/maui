using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiDatePicker : NoCaretField
	{
#if !MACCATALYST
		public MauiDatePicker()
		{
			BorderStyle = UITextBorderStyle.RoundedRect;
			var picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

			if (OperatingSystem.IsIOSVersionAtLeast(13, 4))
			{
				picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			this.InputView = picker;
			var accessoryView = new MauiDoneAccessoryView();
			this.InputAccessoryView = accessoryView;

			accessoryView.SetDataContext(this);
			accessoryView.SetDoneClicked(OnDoneClicked);

			this.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			this.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			this.InputAssistantItem.LeadingBarButtonGroups = null;
			this.InputAssistantItem.TrailingBarButtonGroups = null;

			this.AccessibilityTraits = UIAccessibilityTrait.Button;

			this.EditingDidBegin += OnStarted;
			this.EditingDidEnd += OnEnded;
			picker.ValueChanged += OnValueChanged;
		}

		static void OnDoneClicked(object obj)
		{
			if (obj is MauiDatePicker mdp)
				mdp.MauiDatePickerDelegate?.DoneClicked();
		}

		void OnValueChanged(object? sender, EventArgs e) =>
			MauiDatePickerDelegate?.DatePickerValueChanged();

		void OnEnded(object? sender, EventArgs e) =>
			MauiDatePickerDelegate?.DatePickerEditingDidEnd();

		void OnStarted(object? sender, EventArgs e) =>
			MauiDatePickerDelegate?.DatePickerEditingDidBegin();

		internal MauiDatePickerDelegate? MauiDatePickerDelegate
		{
			get;
			set;
		}

		internal UIDatePicker? DatePickerDialog { get { return InputView as UIDatePicker; } }
#else
		public MauiDatePicker()
		{
			BorderStyle = UITextBorderStyle.RoundedRect;
		}
#endif
	}
}
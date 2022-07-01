using System;
using Foundation;
using UIKit;

namespace Microsoft.Maui.Platform
{
	public class MauiDatePicker : NoCaretField
	{
#if !MACCATALYST
		Action<object>? _editingDidBegin;
		Action<object>? _editingDidEnd;
		Action<object>? _valueChanged;
		WeakReference<object>? _data;

		public MauiDatePicker()
		{
			BorderStyle = UITextBorderStyle.RoundedRect;
			var picker = new UIDatePicker { Mode = UIDatePickerMode.Date, TimeZone = new NSTimeZone("UTC") };

			if (OperatingSystem.IsIOSVersionAtLeast(13, 4))
			{
				picker.PreferredDatePickerStyle = UIDatePickerStyle.Wheels;
			}

			this.InputView = picker;
			this.InputAccessoryView = new MauiDoneAccessoryView();

			this.InputView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;
			this.InputAccessoryView.AutoresizingMask = UIViewAutoresizing.FlexibleHeight;

			this.InputAssistantItem.LeadingBarButtonGroups = null;
			this.InputAssistantItem.TrailingBarButtonGroups = null;

			this.AccessibilityTraits = UIAccessibilityTrait.Button;

			picker.EditingDidBegin += OnStarted;
			picker.EditingDidEnd += OnEnded;
			picker.ValueChanged += OnValueChanged;
		}

		void OnValueChanged(object? sender, EventArgs e)
		{
			if (DataContext != null)
				_valueChanged?.Invoke(DataContext);
		}

		void OnEnded(object? sender, EventArgs e)
		{
			if (DataContext != null)
				_editingDidEnd?.Invoke(DataContext);
		}

		void OnStarted(object? sender, EventArgs e)
		{
			if (DataContext != null)
				_editingDidBegin?.Invoke(DataContext);
		}

		internal void SetPickerDialogActions(
			Action<object>? editingDidBegin,
			Action<object>? editingDidEnd,
			Action<object>? valueChanged)
		{
			_editingDidBegin = editingDidBegin;
			_editingDidEnd = editingDidEnd;
			_valueChanged = valueChanged;
		}

		object? DataContext
		{
			get
			{
				if (_data?.TryGetTarget(out object? target) == true)
					return target;

				return null;
			}
		}
		internal void SetDoneClicked(Action<object>? value, object? dataContext)
		{
			if (this.InputAccessoryView is MauiDoneAccessoryView mda)
			{
				mda?.SetDoneClicked(value);
				mda?.SetDataContext(dataContext);
			}
		}

		internal void SetDataContext(object? dataContext)
		{
			_data = null;
			if (dataContext == null)
				return;

			_data = new WeakReference<object>(dataContext);
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